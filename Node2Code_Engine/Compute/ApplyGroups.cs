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

        public static Tuple<List<INode>, List<DataParam>> ApplyGroups(this List<INode> nodes, List<DataParam> parameters, List<NodeGroup> groups)
        {
            Dictionary<Guid, INode> nodeDictionary = nodes.ToDictionary(x => x.BHoM_Guid, x => x);
            Dictionary<Guid, DataParam> paramDictionary = parameters.ToDictionary(x => x.BHoM_Guid, x => x);

            List<INode> result = new List<INode>();
            List<Guid> coveredChildren = new List<Guid>();

            foreach (NodeGroup group in groups)
            {
                BlockNode block = ApplyGroup(nodeDictionary, paramDictionary, group);
                result.Add(block);
                coveredChildren.AddRange(group.Children());
            }

            IEnumerable<Guid> remainingNodes = nodeDictionary.Keys.Except(coveredChildren);
            result.AddRange(remainingNodes.Select(x => nodeDictionary[x]));

            IEnumerable<Guid> remainingParams = paramDictionary.Keys.Except(coveredChildren);

            return new Tuple<List<INode>, List<DataParam>>(result, remainingParams.Select(x => paramDictionary[x]).ToList());
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static BlockNode ApplyGroup(Dictionary<Guid, INode> nodes, Dictionary<Guid, DataParam> parameters, NodeGroup group)
        {
            List<INode> content = group.NodeIds.Where(x => nodes.ContainsKey(x)).Select(x => nodes[x]).ToList();
            content = content.Concat(group.InternalGroups.Select(x => ApplyGroup(nodes, parameters, x))).ToList();
            content = NodeSequence(content);

            List<DataParam> internalParams = group.NodeIds.Where(x => parameters.ContainsKey(x)).Select(x => parameters[x]).ToList();
            return new BlockNode(content, internalParams, group.Description);
        }

        /***************************************************/
    }
}
