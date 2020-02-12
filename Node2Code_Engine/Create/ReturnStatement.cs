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
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static StatementInfo ReturnStatement(Variable variable)
        {
            return new StatementInfo
            {
                Statement = SyntaxFactory.ReturnStatement(variable.Expression).WithLeadingTrivia(SyntaxFactory.Comment(" ")),
                InputVariables = new List<Guid> { variable.SourceId }
            };
        }


        /***************************************************/
    }
}
