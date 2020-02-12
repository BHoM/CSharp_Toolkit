using BH.Engine.Reflection;
using BH.oM.Node2Code;
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

namespace BH.Engine.Node2Code
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static int DepthDifference(this ReceiverParam receiver, Dictionary<Guid, DataParam> emiters)
        {
            if (emiters.ContainsKey(receiver.SourceId))
            {
                return DepthDifference(receiver.DataType, emiters[receiver.SourceId].DataType);
            }
            else
                return 0;
        }

        /***************************************************/

        public static int DepthDifference(this ReceiverParam receiver, Dictionary<Guid, Variable> variables)
        {
            if (variables.ContainsKey(receiver.SourceId))
            {
                return DepthDifference(receiver.DataType, variables[receiver.SourceId].Type);
            }
            else
                return 0;
        }

        /***************************************************/

        public static int DepthDifference(this Type t1, Type t2)
        {
            return t1.UnderlyingType().Depth - t2.UnderlyingType().Depth;
        }

        /***************************************************/
    }
}
