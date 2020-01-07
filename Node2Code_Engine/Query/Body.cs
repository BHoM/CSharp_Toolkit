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
            List<INode> nodes = Compute.ApplyGroups(content.InternalNodes, content.NodeGroups);

            // Order the nodes 
            nodes = Compute.NodeSequence(nodes);

            // Get the variables
            Dictionary<Guid, Variable> variables = Compute.Variables(nodes, content.Inputs);

            // Create the statements
            List<StatementInfo> statements = nodes.Where(x => !x.IsInline)
                .SelectMany(node => IStatements(node, variables))
                .ToList();

            // Add return statement
            if (content.Outputs.Count > 0)
                statements.Add(Create.ReturnStatement(variables[content.Outputs.First().SourceId]));

            return statements.Select(x => x.Statement).ToList();
        }

        /***************************************************/
    }
}
