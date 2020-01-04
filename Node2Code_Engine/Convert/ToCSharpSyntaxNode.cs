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
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static CSharpSyntaxNode ToCSharpSyntaxNode(this ClusterContent content)
        {
            MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(content.ReturnType(), content.Name)
                .AddModifiers(new SyntaxToken[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword) })
                .AddParameterListParameters(content.MethodParameters().ToArray())
                .AddBodyStatements(content.Body().ToArray());

            return methodDeclaration;
        }

        /***************************************************/
    }
}
