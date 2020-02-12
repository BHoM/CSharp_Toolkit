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

using BH.oM.Node2Code;
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

        public static List<INode> ApplyGroups(this List<INode> nodes, List<NodeGroup> groups)
        {
            Dictionary<Guid, INode> nodeDictionary = nodes.ToDictionary(x => x.BHoM_Guid, x => x);

            List<INode> result = new List<INode>();
            List<Guid> coveredChildren = new List<Guid>();

            foreach (NodeGroup group in groups)
            {
                BlockNode block = ApplyGroup(nodeDictionary, group);
                result.Add(block);
                coveredChildren.AddRange(group.Children());
            }

            IEnumerable<Guid> remainingNodes = nodeDictionary.Keys.Except(coveredChildren);
            result.AddRange(remainingNodes.Select(x => nodeDictionary[x]));

            return result;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static BlockNode ApplyGroup(Dictionary<Guid, INode> nodes, NodeGroup group)
        {
            List<INode> content = group.NodeIds.Where(x => nodes.ContainsKey(x)).Select(x => nodes[x]).ToList();
            content = content.Concat(group.InternalGroups.Select(x => ApplyGroup(nodes, x))).ToList();
            content = NodeSequence(content);

            return new BlockNode(content, group.Description);
        }

        /***************************************************/
    }
}
