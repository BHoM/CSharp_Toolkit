using BH.oM.Node2Code;
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
