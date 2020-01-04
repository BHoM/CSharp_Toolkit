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
    public class ReceiverState
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public ReceiverParam Receiver { get; set; }

        public int DepthDifference { get; set; } = 0;

        public bool Reached { get; set; } = false;

        /***************************************************/
    }
}
