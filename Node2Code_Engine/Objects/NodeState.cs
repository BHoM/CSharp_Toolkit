using BH.Engine.Reflection;
using BH.oM.Programming;
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
    public class NodeState
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public INode Node { get; set; }

        public List<ReceiverState> InputStates { get; set; } = new List<ReceiverState>();

        public bool Processed { get; set; } = false;

        /***************************************************/
    }
}
