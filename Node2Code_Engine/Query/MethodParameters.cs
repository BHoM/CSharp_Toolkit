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

        public static List<ParameterSyntax> MethodParameters(this ClusterContent content)
        {
            return content.Inputs.Select(input =>
            {
                return SyntaxFactory.Parameter
                (
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(),
                    SyntaxFactory.ParseTypeName(input.DataType.FullName),
                    SyntaxFactory.Identifier(input.Name),
                    null
                );
            }).ToList();
        }

        /***************************************************/

        public static List<ParameterSyntax> MethodParameters(this MethodNode node)
        {
            return node.Inputs.Select(input =>
            {
                return SyntaxFactory.Parameter
                (
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(),
                    SyntaxFactory.ParseTypeName(input.DataType.FullName),
                    SyntaxFactory.Identifier(input.Name),
                    null
                );
            }).ToList();
        }

        /***************************************************/

        public static List<ParameterSyntax> MethodParameters(this ConstructorNode node)
        {
            return node.Inputs.Select(input =>
            {
                return SyntaxFactory.Parameter
                (
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(),
                    SyntaxFactory.ParseTypeName(input.DataType.FullName),
                    SyntaxFactory.Identifier(input.Name),
                    null
                );
            }).ToList();
        }

        /***************************************************/
    }
}
