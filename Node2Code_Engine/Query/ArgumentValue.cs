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

        public static ExpressionSyntax ArgumentValue(this ReceiverParam receiver, Dictionary<Guid, Variable> variables)
        {
            if (receiver.SourceId == Guid.Empty)
            {
                if (receiver.DefaultValue == null)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                else if (receiver.DefaultValue is string)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(receiver.DefaultValue.ToString()));
                else
                    return SyntaxFactory.IdentifierName(receiver.DefaultValue.ToString());
            }
            else if (variables.ContainsKey(receiver.SourceId))
                return variables[receiver.SourceId].Expression;
            else
                return SyntaxFactory.IdentifierName(receiver.Name);
        }

        /***************************************************/
    }
}
