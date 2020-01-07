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

        public static Dictionary<Guid, Variable> Variables(this List<INode> nodes, List<DataParam> blockInputs, Dictionary<string, int> nameCounts = null, Dictionary<Guid, Variable> variables = null)
        {
            // Intialise the name counter
            if (nameCounts == null)
                nameCounts = new Dictionary<string, int>();

            // Process the inputs
            Dictionary<Guid, Variable> inputVariables = InputVariables(blockInputs, nameCounts);

            // Process the nodes
            Dictionary<Guid, Variable> nodeVariables = NodeVariables(nodes, nameCounts, variables);

            // Return all
            return inputVariables.Concat(nodeVariables).ToDictionary(x => x.Key, x => x.Value);
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
                else
                {
                    CollectVariables(node as dynamic, nameCounts, variables);
                }
            }

            return variables;
        }

        /***************************************************/

        private static void CollectVariables(INode node, Dictionary<string, int> nameCounts, Dictionary<Guid, Variable> variables)
        {
            foreach (DataParam output in node.Outputs)
                variables[output.BHoM_Guid] = Create.Variable(output, nameCounts);
        }

        /***************************************************/

        private static void CollectVariables(BlockNode node, Dictionary<string, int> nameCounts, Dictionary<Guid, Variable> variables)
        {
            foreach (KeyValuePair<Guid, Variable> kvp in Variables(node.InternalNodes, new List<DataParam>(), nameCounts, variables))
                variables[kvp.Key] = kvp.Value;
        }

        /***************************************************/
    }
}
