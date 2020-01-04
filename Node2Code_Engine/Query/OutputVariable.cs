using BH.Engine.Reflection;
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

        public static List<Variable> IOutputVariables(this INode node, Dictionary<Guid, Variable> variables)
        {
            return OutputVariables(node as dynamic, variables);
        }

        /***************************************************/

        public static List<Variable> OutputVariables(this SetPropertyNode node, Dictionary<Guid, Variable> variables)
        {
            if (node.Inputs.Count == 0)
                return new List<Variable>();

            Guid id = node.Inputs.First().SourceId;
            if (variables.ContainsKey(id))
            {
                variables[node.Outputs.First().BHoM_Guid] = variables[id];
                return new List<Variable> { variables[id] };
            }
            else
                return new List<Variable>();
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        public static List<Variable> OutputVariables(this INode node, Dictionary<Guid, Variable> variables)
        {
            return node.Outputs.Where(x => x.TargetIds.Count > 0 && variables.ContainsKey(x.BHoM_Guid))
                .Select(x => variables[node.Outputs.First().BHoM_Guid]).ToList();
        }

        /***************************************************/
    }
}
