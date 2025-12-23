/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.Engine.CSharp.Objects;
using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.CSharp
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compute all variables existing within a block of nodes (i.e the nodes and the inputs for that block).")]
        [Input("nodes", "List of nodes inside that block to extract variables from.")]
        [Input("blockInputs", "List of input parameters for that block.")]
        [Output("Variables organised by the id of the parameter generating them")]
        public static Dictionary<Guid, Variable> Variables(this List<INode> nodes, List<DataParam> blockInputs)
        {
            return CollectVariables(nodes, blockInputs);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Dictionary<Guid, Variable> CollectVariables(this List<INode> nodes, List<DataParam> blockInputs, Dictionary<string, int> nameCounts = null, Dictionary<Guid, Variable> variables = null)
        {
            // Intialise the name counter
            if (nameCounts == null)
                nameCounts = new Dictionary<string, int>();

            // Process the inputs
            Dictionary<Guid, Variable> inputVariables = CollectVariables(blockInputs, nameCounts);

            // Process the nodes
            Dictionary<Guid, Variable> nodeVariables = CollectVariables(nodes, nameCounts, variables);

            // Return all
            return inputVariables.Concat(nodeVariables).ToDictionary(x => x.Key, x => x.Value);
        }

        /***************************************************/

        private static Dictionary<Guid, Variable> CollectVariables(List<DataParam> inputs, Dictionary<string, int> nameCounts)
        {
            return inputs.ToDictionary(
                x => x.BHoM_Guid,
                x => CreateVariable(x, nameCounts)
            ); 
        }

        /***************************************************/

        private static Dictionary<Guid, Variable> CollectVariables(List<INode> nodes, Dictionary<string, int> nameCounts, Dictionary<Guid, Variable> variables = null)
        {
            if (variables == null)
                variables = new Dictionary<Guid, Variable>();

            foreach(INode node in nodes)
            {
                if (node.IsInline)
                {
                    Dictionary<Guid, Variable> outputVariables = Query.IOutputVariables(node, variables);
                    foreach (KeyValuePair<Guid, Variable> kvp in outputVariables)
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
                variables[output.BHoM_Guid] = CreateVariable(output, nameCounts);
        }

        /***************************************************/

        private static void CollectVariables(BlockNode node, Dictionary<string, int> nameCounts, Dictionary<Guid, Variable> variables)
        {
            foreach (KeyValuePair<Guid, Variable> kvp in CollectVariables(node.InternalNodes, new List<DataParam>(), nameCounts, variables))
                variables[kvp.Key] = kvp.Value;
        }

        /***************************************************/

        private static Variable CreateVariable(DataParam output, Dictionary<string, int> nameCounts)
        {
            if (output == null)
                return null;

            // Create the output variables
            string outputName = VariableName(output.Name, nameCounts);
            if (outputName.Length > 0)
            {
                return new Variable
                {
                    Name = outputName,
                    Expression = SyntaxFactory.IdentifierName(outputName),
                    SourceId = output.BHoM_Guid,
                    Type = output.DataType
                };
            }
            else if (output.Data != null)
            {
                return new Variable
                {
                    Expression = SyntaxFactory.IdentifierName(output.Data.ToString()),
                    SourceId = output.BHoM_Guid,
                    Type = output.DataType
                };
            }
            else
                return null;
        }

        /***************************************************/

        private static string VariableName(string name, Dictionary<string, int> nameCounts)
        {
            name = name.Length == 0 ? "" : name.Substring(0, 1).ToLower() + name.Substring(1);

            if (nameCounts.ContainsKey(name))
            {
                nameCounts[name] += 1;
                name += "_" + nameCounts[name];
            }
            else
                nameCounts[name] = 1;

            return name;
        }

        /***************************************************/
    }
}






