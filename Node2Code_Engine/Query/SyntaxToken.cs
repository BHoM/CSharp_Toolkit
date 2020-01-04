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

namespace BH.Engine.Node2Code
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static SyntaxToken SyntaxToken(this Variable variable)
        {
            if (variable == null || variable.Expression == null)
                return new SyntaxToken();
            else
                return variable.Expression.GetFirstToken();
        }

        /***************************************************/
    }
}
