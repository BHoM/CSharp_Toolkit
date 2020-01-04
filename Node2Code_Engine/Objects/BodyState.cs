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

namespace BH.Engine.Node2Code.Objects
{
    public class BodyState
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Dictionary<Guid, NodeState> NodeStates { get; set; } = new Dictionary<Guid, NodeState>();

        public Dictionary<Guid, ReceiverState> ReceiverStates { get; set; } = new Dictionary<Guid, ReceiverState>();

        public Dictionary<string, int> NameCounts { get; set; } = new Dictionary<string, int>();

        public Dictionary<Guid, Variable> Variables { get; set; } = new Dictionary<Guid, Variable>();

        public List<ReceiverState> BoundaryReceivers { get; set; } = new List<ReceiverState>();

        /***************************************************/
    }
}
