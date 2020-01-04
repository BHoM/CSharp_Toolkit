using BH.Engine.Node2Code.Objects;
using BH.oM.Base;
using BH.oM.Node2Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Node2Code
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<StatementSyntax> Body(this ClusterContent content)
        {
            // Apply the groups
            Tuple<List<INode>, List<DataParam>> items = Compute.ApplyGroups(content.InternalNodes, content.InternalParams, content.NodeGroups);
            List<INode> nodes = items.Item1;
            List<DataParam> internalParams = items.Item2;

            // Order the nodes 
            nodes = Compute.NodeSequence(nodes);

            // Get the variables
            Dictionary<Guid, Variable> variables = Compute.Variables(nodes, content.Inputs, internalParams);

            // Create the statements
            List<StatementInfo> statements = nodes.Where(x => !x.IsInline)
                .SelectMany(node => IStatements(node, variables))
                .ToList();

            // Add return statement
            if (content.Outputs.Count > 0)
                statements.Add(Create.ReturnStatement(variables[content.Outputs.First().SourceId]));

            return statements.Select(x => x.Statement).ToList();
        }

        /*public static List<StatementSyntax> Body(this ClusterContent content)
        {
            // Apply the groups
            content.InternalNodes = Compute.ApplyGroups(content.InternalNodes, content.InternalParams, content.NodeGroups);

            // Create the initial body state
            BodyState bodyState = InitialiseState(content);

            // Generate the statements 
            List<StatementInfo> statements = new List<StatementInfo>();
            while (bodyState.BoundaryReceivers.Count > 0)
            {
                List<NodeState> boundaryNodes = bodyState.NodeStates.Values.Where(x => !x.Processed && x.InputStates.All(i => i.Reached)).ToList();
                if (boundaryNodes.Count == 0)
                    break;

                foreach (NodeState nodeState in boundaryNodes)
                    statements.AddRange(ProcessNode(nodeState, bodyState).Where(x => x != null));    
            }

            // Post-process the statements
            statements = statements.Order(content.NodeGroups);
            statements = statements.AddComments(content.NodeGroups);

            // Add return statement
            statements.Add(Create.ReturnStatement(bodyState.Variables[bodyState.BoundaryReceivers.First().Receiver.SourceId]));

            return statements.Select(x => x.Statement).ToList();
        }*/


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        /*private static Dictionary<Guid, Variable> Variables(List<DataParam> inputs, List<DataParam> internalParams)
        {
            // Process the inputs
            Dictionary<Guid, Variable> variables = inputs.ToDictionary(
                x => x.BHoM_Guid,
                x => new Variable
                {
                    Name = x.Name,
                    Expression = SyntaxFactory.IdentifierName(x.Name),
                    Type = x.DataType,
                    SourceId = x.BHoM_Guid
                }
            );

            foreach (DataParam data in internalParams)
            {
                ExpressionSyntax expression = null;
                if (data.Data is Enum)
                    expression = SyntaxFactory.IdentifierName(data.DataType.FullName + "." + data.Data.ToString()); // will need to figure out this one
                else if (data.Data is string)
                    expression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(data.Data.ToString()));
                else
                    expression = SyntaxFactory.IdentifierName(data.Data.ToString());

                if (expression != null)
                {
                    variables[data.BHoM_Guid] = new Variable
                    {
                        Expression = expression,
                        Type = data.DataType,
                        SourceId = data.BHoM_Guid
                    };
                }
            }

            return variables;
        }*/

        /***************************************************/

        /*private static BodyState InitialiseState(ClusterContent content)
        {
            // Collect all emiters
            Dictionary<Guid, DataParam> emiters = content.Inputs
                .Concat(content.InternalParams)
                .Concat(content.InternalNodes.SelectMany(x => x.Outputs))
                .ToDictionary(x => x.BHoM_Guid, x => x);

            // Create the internal receivers state
            Dictionary<Guid, ReceiverState> internalReceiversState = content.InternalNodes
                .SelectMany(x => x.Inputs).Concat(content.Outputs)
                .Where(x => x.BHoM_Guid != Guid.Empty)
                .ToDictionary(x => x.BHoM_Guid, x => new ReceiverState { Receiver = x, Reached = x.SourceId == Guid.Empty, DepthDifference = DepthDifference(x, emiters) });

            // Create the internal nodes state
            Dictionary<Guid, NodeState> internalNodesState = content.InternalNodes.ToDictionary(
                x => x.BHoM_Guid,
                x => new NodeState
                {
                    Node = x,
                    InputStates = x.Inputs.Where(i => internalReceiversState.ContainsKey(i.BHoM_Guid)).Select(i => internalReceiversState[i.BHoM_Guid]).ToList(),
                    Processed = false
                }
            );

            //Create the variable
            Dictionary<Guid, Variable> variables = content.Inputs.ToDictionary(
                x => x.BHoM_Guid, 
                x => new Variable {
                    Name = x.Name,
                    Expression = SyntaxFactory.IdentifierName(x.Name),
                    Type = x.DataType,
                    SourceId = x.BHoM_Guid
                }
            );
            foreach (DataParam data in content.InternalParams)
            {
                ExpressionSyntax expression = null;
                if (data.Data is Enum)
                    expression = SyntaxFactory.IdentifierName(data.DataType.FullName + "." + data.Data.ToString()); // will need to figure out this one
                else if (data.Data is string)
                    expression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(data.Data.ToString()));
                else if (data.Data is LibraryData)
                    internalNodesState[data.BHoM_Guid] = LibraryNodeState(data, variables);
                else
                    expression = SyntaxFactory.IdentifierName(data.Data.ToString());

                if (expression != null)
                {
                    variables[data.BHoM_Guid] = new Variable
                    {
                        Expression = expression,
                        Type = data.DataType,
                        SourceId = data.BHoM_Guid
                    };
                }
            }

            // Get the boudary receivers
            List<ReceiverState> boundaryReceivers = NextReceivers(content.Inputs.Concat(content.InternalParams).ToList(), internalReceiversState);
            boundaryReceivers.ForEach(x => x.Reached = true);

            // Return the intial state
            return new BodyState
            {
                NodeStates = internalNodesState,
                ReceiverStates = internalReceiversState,
                Variables = variables,
                BoundaryReceivers = boundaryReceivers
            };
        }*/

        /***************************************************/

        /*private static List<StatementInfo> ProcessNode(NodeState nodeState, BodyState bodyState)
        {
            INode node = nodeState.Node;
            List<StatementInfo> statements = new List<StatementInfo>();

            if (node.IsInline)
            {
                Dictionary<Guid, Variable> expressions = IInlineExpression(node, bodyState.Variables);
                foreach (KeyValuePair<Guid, Variable> kvp in expressions)
                    bodyState.Variables[kvp.Key] = kvp.Value;
            }
            else
            {
                statements = node.Statements(bodyState);
            }
            
            // Mark node as processed
            nodeState.Processed = true;

            // Update the boundary receivers
            node.Inputs.Where(x => bodyState.ReceiverStates.ContainsKey(x.BHoM_Guid))
                .Select(x => bodyState.ReceiverStates[x.BHoM_Guid]).ToList().ForEach(x => {
                    bodyState.BoundaryReceivers.Remove(x);
                });
            List<ReceiverState> nextReceivers = NextReceivers(node.Outputs, bodyState.ReceiverStates);
            nextReceivers.ForEach(x => x.Reached = true);
            bodyState.BoundaryReceivers.AddRange(nextReceivers);
            bodyState.BoundaryReceivers = bodyState.BoundaryReceivers.Distinct().ToList();

            // Return statement
            return statements;
        }*/

        /***************************************************/

        /*private static List<ReceiverState> NextReceivers(List<DataParam> emiters, Dictionary<Guid, ReceiverState> internalReceiversState)
        {
            return emiters.SelectMany(x => x.TargetIds).Distinct()
                .Where(x => internalReceiversState.ContainsKey(x))
                .Select(x => internalReceiversState[x])
                .ToList();
        }*/

        /***************************************************/

        private static NodeState LibraryNodeState(DataParam data, Dictionary<Guid, Variable> variables)
        {
            LibraryData libraryData = data.Data as LibraryData;
            Guid fileGuid = Guid.NewGuid();
            Guid objectNameGuid = Guid.NewGuid();
            variables[fileGuid] = new Variable {
                Expression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(libraryData.SourceFile)),
                Type = typeof(string),
                SourceId = fileGuid
            };
            variables[objectNameGuid] = new Variable
            {
                Expression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(libraryData.Data.Name)),
                Type = typeof(string),
                SourceId = objectNameGuid
            };

            MethodInfo method = typeof(BH.Engine.Library.Query).GetMethod("Match", new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) });
            return new NodeState
            {
                Node = new MethodNode
                {
                    BHoM_Guid = data.BHoM_Guid,
                    Outputs = new List<DataParam> { data },
                    Inputs = new List<Guid> { fileGuid, objectNameGuid }.Select(x => new ReceiverParam { SourceId = x }).ToList(),
                    Method = method
                },
                Processed = false
            };
        }

        /***************************************************/
    }
}
