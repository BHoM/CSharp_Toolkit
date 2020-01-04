using BH.Engine.Node2Code.Objects;
using BH.oM.Node2Code;
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
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<Guid, Variable> Variables(this List<INode> nodes, List<DataParam> blockInputs, List<DataParam> internalParams, Dictionary<string, int> nameCounts = null, Dictionary<Guid, Variable> variables = null)
        {
            // Intialise the name counter
            if (nameCounts == null)
                nameCounts = new Dictionary<string, int>();

            // Process the inputs
            Dictionary<Guid, Variable> inputVariables = InputVariables(blockInputs, nameCounts);

            // Process the internal parameters
            Dictionary<Guid, Variable> paramVariables = ParamVariables(internalParams, nameCounts);

            // Process the nodes
            Dictionary<Guid, Variable> nodeVariables = NodeVariables(nodes, nameCounts, variables);

            // Return all
            return inputVariables.Concat(paramVariables).Concat(nodeVariables).ToDictionary(x => x.Key, x => x.Value);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Dictionary<Guid, Variable> InputVariables(List<DataParam> inputs, Dictionary<string, int> nameCounts)
        {
            return inputs.ToDictionary(
                x => x.BHoM_Guid,
                x => Create.Variable(x, nameCounts)
            ); 
        }

        /***************************************************/

        private static Dictionary<Guid, Variable> ParamVariables(List<DataParam> internalParams, Dictionary<string, int> nameCounts)
        {
            Dictionary<Guid, Variable> variables = new Dictionary<Guid, Variable>();

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
        }

        /***************************************************/

        private static Dictionary<Guid, Variable> NodeVariables(List<INode> nodes, Dictionary<string, int> nameCounts, Dictionary<Guid, Variable> variables = null)
        {
            if (variables == null)
                variables = new Dictionary<Guid, Variable>();

            foreach(INode node in nodes)
            {
                if (node.IsInline)
                {
                    Dictionary<Guid, Variable> expressions = Query.IInlineExpression(node, variables);
                    foreach (KeyValuePair<Guid, Variable> kvp in expressions)
                        variables[kvp.Key] = kvp.Value;
                }
                else if (node is BlockNode)
                {
                    BlockNode block = node as BlockNode;
                    foreach(KeyValuePair<Guid, Variable> kvp in Variables(block.InternalNodes, new List<DataParam>(), block.InternalParams, nameCounts, variables))
                        variables[kvp.Key] = kvp.Value;
                }
                else
                {
                    foreach (DataParam output in node.Outputs)
                        variables[output.BHoM_Guid] = Create.Variable(output, nameCounts);
                }
            }

            return variables;
        }

        /***************************************************/
    }
}
