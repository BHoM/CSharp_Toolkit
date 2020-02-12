using BH.Engine.Reflection;
using BH.oM.Base;
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

        public static ExpressionSyntax IExpression(this INode node, List<ExpressionSyntax> inputs)
        {
            return Expression(node as dynamic, inputs);
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this MethodNode node, List<ExpressionSyntax> inputs)
        {
            string methodName = node.Method.MethodName();

            switch (methodName)
            {
                case "BH.UI.Components.DeleteCaller.Delete":
                case "BH.UI.Components.ExecuteCaller.Execute":
                case "BH.UI.Components.MoveCaller.Move":
                case "BH.UI.Components.PullCaller.Pull":
                case "BH.UI.Components.PushCaller.Push":
                    int lastNonDefault = node.Inputs.FindLastIndex(node.Inputs.Count - 2, x => x.SourceId != Guid.Empty);
                    inputs = inputs.Take(lastNonDefault + 1).ToList();
                    return InstanceMethodExpression(node, inputs);
                default:
                    return StaticMethodExpression(node, inputs);
            }
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this ConstructorNode node, List<ExpressionSyntax> inputs)
        {
            List<ArgumentSyntax> arguments = inputs.Select(x => SyntaxFactory.Argument(x)).ToList();
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

            TypeSyntax objectType = SyntaxFactory.ParseTypeName(node.Constructor.MethodName());
            if (node.Constructor.IsGenericMethod)
            {
                List<TypeSyntax> genericTypes = node.Constructor.GetGenericArguments().Select(x => SyntaxFactory.ParseTypeName(x.FullName)).ToList();
                objectType = SyntaxFactory.GenericName(SyntaxFactory.Identifier(node.Constructor.MethodName()), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(genericTypes)));
            }

            return SyntaxFactory.ObjectCreationExpression(objectType, argumentList, null);
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this InitialiserNode node, List<ExpressionSyntax> inputs)
        {
            var arguments = node.Inputs.Take(inputs.Count)
                .Zip(inputs, (a, b) => new Tuple<ReceiverParam, ExpressionSyntax>(a, b))
                .Where(x => x.Item1.SourceId != Guid.Empty)
                .ToList();

            List<ExpressionSyntax> assignments = arguments.Select(x => SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(x.Item1.Name),
                x.Item2
            ) as ExpressionSyntax).ToList();

            TypeSyntax objectType = SyntaxFactory.ParseTypeName(node.ObjectType.MethodName());
            InitializerExpressionSyntax initialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(assignments));
            return SyntaxFactory.ObjectCreationExpression(objectType, null, initialiser);
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this SetPropertyNode node, List<ExpressionSyntax> inputs)
        {
            if (inputs.Count < 3)
                return null;

            MemberAccessExpressionSyntax memberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                inputs[0],
                SyntaxFactory.IdentifierName(inputs[1].ToFullString().Replace("\"", ""))
            );

            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, memberAccess, inputs[2]);
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this CustomObjectNode node, List<ExpressionSyntax> inputs)
        {
            var arguments = node.Inputs.Take(inputs.Count)
                .Zip(inputs, (a, b) => new Tuple<ReceiverParam, ExpressionSyntax>(a, b))
                .Where(x => x.Item1.SourceId != Guid.Empty)
                .ToList();

            List<ExpressionSyntax> dicAssignments = arguments.Select(x => SyntaxFactory.InitializerExpression(
                SyntaxKind.ComplexElementInitializerExpression,
                SyntaxFactory.SeparatedList(new List<ExpressionSyntax> {
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(x.Item1.Name)),
                    x.Item2
                })
            ).WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.Tab) as ExpressionSyntax).ToList();

            TypeSyntax dicType = SyntaxFactory.ParseTypeName(typeof(Dictionary<string, object>).ToText(true));
            InitializerExpressionSyntax dicInitialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(dicAssignments));
            ObjectCreationExpressionSyntax dicCreator = SyntaxFactory.ObjectCreationExpression(dicType, null, dicInitialiser);

            List<ExpressionSyntax> assignments = new List<ExpressionSyntax>
            {
                 SyntaxFactory.AssignmentExpression(
                     SyntaxKind.SimpleAssignmentExpression,
                     SyntaxFactory.IdentifierName("CustomData"),
                     dicCreator
                 )
            };

            TypeSyntax objectType = SyntaxFactory.ParseTypeName(typeof(CustomObject).FullName);
            InitializerExpressionSyntax initialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(assignments));
            return SyntaxFactory.ObjectCreationExpression(objectType, null, initialiser);
        }

        /***************************************************/

        public static ExpressionSyntax Expression(this LibraryNode node, List<ExpressionSyntax> inputs)
        {
            string dataName = "";
            if (node.Outputs.Count > 0)
                dataName = node.Outputs.First().Name;

            ExpressionSyntax sourceExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(node.SourceFile));
            ExpressionSyntax dataExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(dataName));

            List<ArgumentSyntax> arguments = new ExpressionSyntax[] { sourceExpression, dataExpression }.Select(x => SyntaxFactory.Argument(x)).ToList();
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

            return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("BH.Engine.Library.Query.Match"), argumentList);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        public static ExpressionSyntax Expression(this INode node, List<ExpressionSyntax> inputs)
        {
            return null;
        }

        /***************************************************/

        public static ExpressionSyntax StaticMethodExpression(this MethodNode node, List<ExpressionSyntax> inputs)
        {
            List<ArgumentSyntax> arguments = inputs.Select(x => SyntaxFactory.Argument(x)).ToList();
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

            return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(node.Method.MethodName()), argumentList);
        }

        /***************************************************/

        public static ExpressionSyntax InstanceMethodExpression(this MethodNode node, List<ExpressionSyntax> inputs)
        {
            List<ArgumentSyntax> arguments = inputs.Skip(1).Select(x => SyntaxFactory.Argument(x)).ToList();
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

            MemberAccessExpressionSyntax memberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                inputs[0],
                SyntaxFactory.IdentifierName(node.Method.Name)
            );
            return SyntaxFactory.InvocationExpression(memberAccess, argumentList);
        }

        /***************************************************/
    }
}
