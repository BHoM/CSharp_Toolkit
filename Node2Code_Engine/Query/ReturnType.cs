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

        public static TypeSyntax ReturnType(this ClusterContent content)
        {
            string returnTypeString = "void";
            if (content.Outputs.Count > 0)
                returnTypeString = content.Outputs.First().DataType.FullName;

            return SyntaxFactory.ParseTypeName(returnTypeString);
        }

        /***************************************************/

        public static TypeSyntax ReturnType(this INode node, int depth = 0)
        {
            string returnTypeString = "void";
            if (node.Outputs.Count > 0)
                returnTypeString = node.Outputs.First().DataType.ToText(true);

            for (int i = 0; i < depth; i++)
                returnTypeString = "List<" + returnTypeString + ">";

            return SyntaxFactory.ParseTypeName(returnTypeString);
        }

        /***************************************************/
    }
}
