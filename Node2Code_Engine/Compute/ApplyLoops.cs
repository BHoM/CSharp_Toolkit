/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.Engine.Node2Code.Objects;
using BH.oM.Programming;
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

        public static List<INode> ApplyLoops(this List<INode> nodes, Dictionary<Guid, Variable> variables)
        {
            Dictionary<Guid, INode> nodeDic = nodes.ToDictionary(x => x.BHoM_Guid, x => x);

            List<INode> result = new List<INode>();

            foreach (INode node in nodes)
            {
                List<int> depths = node.Inputs.Where(x => x.SourceId != Guid.Empty).Select(x => x.DepthDifference(variables)).ToList();
                if (depths.Any(x => x < 0))
                {
                    foreach(DataParam output in node.Outputs)
                    {
                        output.DataType = MakeList(output.DataType);
                        variables[output.BHoM_Guid].Type = output.DataType;
                    }
                    result.Add(new LoopNode(new List<INode> { node }));
                }  
                else
                    result.Add(node);
            }

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<INode> CollectLoopNodes(INode source, Dictionary<Guid, INode> nodes, Dictionary<Guid, ReceiverParam> receivers)
        {
            List<INode> result = new List<INode>();

            Dictionary<Guid, DataParam> emiters = source.Outputs.ToDictionary(x => x.BHoM_Guid, x => x);
            var receiversByParents = source.Outputs.SelectMany(x => x.TargetIds).Where(x => receivers.ContainsKey(x)).Select(x => receivers[x]).GroupBy(x => x.ParentId);
            foreach (var group in receiversByParents)
            {
                if (nodes.ContainsKey(group.Key) && group.All(x => Query.DepthDifference(x, emiters) == 0))
                {
                    result.Add(nodes[group.Key]);
                    nodes.Remove(group.Key);
                    List<INode> next = CollectLoopNodes(nodes[group.Key], nodes, receivers);
                }   
            }

            return result;
        }

        /***************************************************/

        private static Type MakeList(Type type)
        {
            return typeof(List<>).MakeGenericType(new Type[] { type });
        }

        /***************************************************/
    }
}
