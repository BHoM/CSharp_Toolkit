/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Reflection.Attributes;
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

        [Description("Replace groups of nodes into block nodes")]
        [Input("nodes", "Flat list of nodes that need to be grouped")]
        [Input("groups", "Defines how the nodes should be grouped")]
        [Output("New list where the grouped nodes are now contained in block nodes")]
        public static List<INode> ApplyGroups(this List<INode> nodes, List<NodeGroup> groups)
        {
            if (nodes == null || groups == null)
                return new List<INode>();

            Dictionary<Guid, INode> nodeDictionary = nodes.Where(x => x != null).ToDictionary(x => x.BHoM_Guid, x => x);

            List<INode> result = new List<INode>();
            List<Guid> coveredChildren = new List<Guid>();

            foreach (NodeGroup group in groups.Where(x => x != null))
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

            return Engine.Programming.Create.BlockNode(content, group.Description);
        }

        /***************************************************/
    }
}


