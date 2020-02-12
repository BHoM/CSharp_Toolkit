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

        public static List<INode> NodeSequence(this List<INode> nodes)
        {
            // Create the receivers states & and node states
            Dictionary<Guid, ReceiverState> receiverStates = ReceiverStates(nodes);
            Dictionary<Guid, NodeState> nodeStates = NodeStates(nodes, receiverStates);

            // Collect the nodes in order
            List<INode> sequence = new List<INode>();
            List<NodeState> boundary = nodeStates.Values.Where(x => IsReady(x)).ToList();
            while (boundary.Count > 0)
            {
                // Get a node from the boundary
                NodeState nodeState = boundary.First();
                nodeState.Processed = true;
                boundary.RemoveAt(0);
                sequence.Add(nodeState.Node);

                // Get nodes that are now ready
                List<ReceiverState> nextReceivers = NextReceivers(nodeState.Node.Outputs, receiverStates);
                nextReceivers.ForEach(x => x.Reached = true);
                List<NodeState> nextNodes = nextReceivers.Select(x => x.Receiver.ParentId).Distinct()
                    .Where(x => nodeStates.ContainsKey(x)).Select(x => nodeStates[x])
                    .Where(x => IsReady(x))
                    .ToList();
                boundary = nextNodes.Concat(boundary).ToList();
            }

            return sequence;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Dictionary<Guid, ReceiverState> ReceiverStates(List<INode> nodes)
        {
            List<Guid> emiters = nodes.SelectMany(n => n.Outputs.Where(x => x.TargetIds.Count > 0)).Select(x => x.BHoM_Guid).ToList();

            return nodes
                .SelectMany(x => x.Inputs)
                .Where(x => x.BHoM_Guid != Guid.Empty && emiters.Contains(x.SourceId))
                .ToDictionary(x => x.BHoM_Guid, x => new ReceiverState { Receiver = x, Reached = x.SourceId == Guid.Empty });
        }

        /***************************************************/

        private static Dictionary<Guid, NodeState> NodeStates(List<INode> nodes, Dictionary<Guid, ReceiverState> receiverStates)
        {
            return nodes.ToDictionary(
                x => x.BHoM_Guid,
                x => new NodeState
                {
                    Node = x,
                    InputStates = x.Inputs.Where(i => receiverStates.ContainsKey(i.BHoM_Guid)).Select(i => receiverStates[i.BHoM_Guid]).ToList(),
                    Processed = false
                }
            );
        }

        /***************************************************/

        private static bool IsReady(NodeState nodeState)
        {
            return !nodeState.Processed && nodeState.InputStates.All(x => x.Reached);
        }

        /***************************************************/

        private static List<ReceiverState> NextReceivers(List<DataParam> emiters, Dictionary<Guid, ReceiverState> receiversState)
        {
            return emiters.SelectMany(x => x.TargetIds).Distinct()
                .Where(x => receiversState.ContainsKey(x))
                .Select(x => receiversState[x])
                .ToList();
        }

        /***************************************************/
    }
}
