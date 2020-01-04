using BH.Engine.Node2Code.Objects;
using BH.Engine.Reflection;
using BH.oM.Base;
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

        public static List<StatementInfo> IStatements(this INode node, Dictionary<Guid, Variable> variables, int depth = 0)
        {
            if (node.IsInline)
                return new List<StatementInfo>();
            else if (node is BlockNode)
                return Statements(node as BlockNode, variables, depth);
            else
                return Statements(node, variables);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        public static List<StatementInfo> Statements(this BlockNode node, Dictionary<Guid, Variable> variables, int depth = 0)
        {
            string commentLead = new string(Enumerable.Repeat('-', 4 * depth).ToArray());

            List<StatementInfo> statements = node.InternalNodes.SelectMany(x => x.IStatements(variables, depth + 1)).Where(x => x != null).ToList();

            if (statements.Count > 0)
            {
                StatementSyntax statement = statements[0].Statement;
                if (statement.HasLeadingTrivia)
                {
                    SyntaxTriviaList trivia = statement.GetLeadingTrivia();
                    trivia = trivia.InsertRange(0, new List<SyntaxTrivia> {
                        SyntaxFactory.Comment(" "),
                        SyntaxFactory.Comment("//" + commentLead + " " + node.Description)
                    });
                    statements[0].Statement = statement.WithLeadingTrivia(trivia);
                }
                else
                {
                    statements[0].Statement = statements[0].Statement.WithLeadingTrivia(
                        SyntaxFactory.Comment(" "),
                        SyntaxFactory.Comment("//" + commentLead + " " + node.Description)
                    );
                }
            }

            return statements;
        }

        /***************************************************/

        public static List<StatementInfo> Statements(this INode node, Dictionary<Guid, Variable> variables)
        {
            List<Variable> outputVariables = node.IOutputVariables(variables);
            List<ExpressionSyntax> arguments = Arguments(node, variables, out List<ReceiverParam> listInputs);
            ExpressionSyntax expression = Expression(node as dynamic, arguments);

            expression = HandleTypeDifferences(node, expression, variables, listInputs, out List<StatementSyntax> extraStatements);

            if (expression == null)
                return new List<StatementInfo>();
            else if (node.IsDeclaration && outputVariables.Count > 0)
            {
                TypeSyntax returnType = node.ReturnType(listInputs.Count > 0 ? 1 : 0);
                List<StatementSyntax> statements = new List<StatementSyntax> { Declaration(expression, outputVariables, returnType) };
                return statements.Concat(extraStatements).Select(x => new StatementInfo { Statement = x }).ToList();
            }
            else
            {
                return new List<StatementInfo> {
                    new StatementInfo {
                        Statement = SyntaxFactory.ExpressionStatement(expression)
                    }
                };
            }
        }

        /***************************************************/

        private static List<ExpressionSyntax> Arguments(INode node, Dictionary<Guid, Variable> variables, out List<ReceiverParam> listInputs)
        {
            // Get the arguments
            int lastNonDefault = node.Inputs.FindLastIndex(x => x.SourceId != Guid.Empty);
            List<ReceiverParam> inputs = node.Inputs.Take(lastNonDefault + 1).ToList();
            List<ExpressionSyntax> arguments = inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

            listInputs = inputs.Where(x => x.DepthDifference(variables) == -1).ToList();
            if (listInputs.Count == 1)
                arguments[inputs.IndexOf(listInputs.First())] = SyntaxFactory.IdentifierName("_x");
            else if (listInputs.Count > 1)
            {
                foreach (ReceiverParam receiver in listInputs)
                {
                    arguments[inputs.IndexOf(receiver)] = SyntaxFactory.ElementAccessExpression(
                        arguments[inputs.IndexOf(receiver)],
                        SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax>
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("_i")) 
                        }))
                    );
                }
                    
            }

            return arguments;
        }

        /***************************************************/

        private static ExpressionSyntax HandleTypeDifferences(INode node, ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs, out List<StatementSyntax> extraStatements)
        {
            extraStatements = new List<StatementSyntax>();

            if (listInputs.Count > 0)
            {
                if (listInputs.Count == 1)
                {
                    if (node.IsDeclaration)
                        expression = SelectLoop(expression, variables, listInputs);
                    else
                        expression = ForEachLoop(expression, variables, listInputs);
                }
                else
                {
                    expression = ForLoop(node, expression, variables, listInputs, out StatementSyntax loopStatement);
                    extraStatements.Add(loopStatement);
                }
                    

                if (node.Outputs.Count > 0 && !(node is SetPropertyNode)) // need a better way to do this
                    PromoteToList(variables, node.Outputs.First().BHoM_Guid);
            }

            return expression;
        }

        /***************************************************/

        private static ExpressionSyntax SelectLoop(ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs)
        {
            ReceiverParam input = listInputs.First();

            var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_x")), expression);

            var selectAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Query.ArgumentValue(input, variables), SyntaxFactory.IdentifierName("Select"));
            var selectInvoc = SyntaxFactory.InvocationExpression(selectAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));

            var toListAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, selectInvoc, SyntaxFactory.IdentifierName("ToList"));
            return SyntaxFactory.InvocationExpression(toListAccess, SyntaxFactory.ArgumentList());
        }

        /***************************************************/

        private static ExpressionSyntax ForEachLoop(ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs)
        {
            ReceiverParam input = listInputs.First();

            var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_x")), expression);

            var foreachAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Query.ArgumentValue(input, variables), SyntaxFactory.IdentifierName("ForEach"));
            return SyntaxFactory.InvocationExpression(foreachAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));
        }

        /***************************************************/

        private static ExpressionSyntax ForLoop(INode node, ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs, out StatementSyntax loopStatement)
        {
            Type outputType = variables[node.Outputs.First().BHoM_Guid].Type;

            var addAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, 
                SyntaxFactory.IdentifierName(node.Outputs.First().Name), 
                SyntaxFactory.IdentifierName("Add")
            );
            var body = SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                addAccess, 
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(expression) })))
            );

            var zeroExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword) ), 
                SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(new List<VariableDeclaratorSyntax>
                {
                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_i"), null, SyntaxFactory.EqualsValueClause(zeroExpression))
                })
            );
            SeparatedSyntaxList<ExpressionSyntax> initialisers = SyntaxFactory.SeparatedList(new List<ExpressionSyntax>());

            ExpressionSyntax maxLimit = MinCountExpression(listInputs.Select(x => ArgumentValue(x, variables)).ToList());
            ExpressionSyntax condition = SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, SyntaxFactory.IdentifierName("_i"), maxLimit);

            SeparatedSyntaxList<ExpressionSyntax> incrementors = SyntaxFactory.SeparatedList(new List<ExpressionSyntax>
            {
                SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName("_i"))
            });

            loopStatement = SyntaxFactory.ForStatement(declaration, initialisers, condition, incrementors, body);


            return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(outputType.MethodName()), null, null);
        }

        /***************************************************/

        private static ExpressionSyntax MinCountExpression(List<ExpressionSyntax> lists)
        {
            List<ArgumentSyntax> arguments = lists.Select(list =>
            {
                var countAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, list, SyntaxFactory.IdentifierName("Count"));
                var countInvoc = SyntaxFactory.InvocationExpression(countAccess, SyntaxFactory.ArgumentList());
                return SyntaxFactory.Argument(countInvoc);
            }).ToList();

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Math"),
                    SyntaxFactory.IdentifierName("Min")
                ),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments))
            );
        }

        /***************************************************/

        private static LocalDeclarationStatementSyntax Declaration(ExpressionSyntax expression, List<Variable> outputVariables, TypeSyntax outputType)
        {
            EqualsValueClauseSyntax intialiser = SyntaxFactory.EqualsValueClause(expression);
            VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariables.First().SyntaxToken(), null, intialiser);

            VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(outputType, SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
            return SyntaxFactory.LocalDeclarationStatement(declaration);
        }

        /***************************************************/

        private static void PromoteToList(Dictionary<Guid, Variable> variables, Guid id)
        {
            if (variables.ContainsKey(id))
                variables[id].Type = typeof(List<>).MakeGenericType(new Type[] { variables[id].Type });
        }

        /***************************************************/
    }
}
