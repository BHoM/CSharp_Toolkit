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
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string ToCSharpText(this ClusterContent content)
        {
            if (content == null)
                return "";

            return content.ToCSharpSyntaxNode().NormalizeWhitespace().ToFullString();
        }


        /***************************************************/
    }
}
