/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.Engine.CSharp.Objects;
using BH.Engine.Reflection;
using BH.oM.Base;
using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.CSharp
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Get the list of C# statements corresponding to a node given a list of available variables")]
        [Input("node", "Node to get the statements from")]
        [Input("variables", "List of variables available in the context of the node")]
        [Input("depth", "Optional input defining how many groups are wrapping this node")]
        [Output("Microsoft.CodeAnalysis.CSharp.StatementSyntax corresponding to the node")]
        public static List<StatementSyntax> IStatements(this INode node, Dictionary<Guid, Variable> variables, int depth = 0)
        {
            if (node.IsInline)
            {
                List<ExpressionSyntax> arguments = Arguments(node, variables, out List<ReceiverParam> listInputs);
                if (listInputs.Count > 0 && !(node is SetPropertyNode)) // need a better way to do this
                    node.Outputs.ForEach(output => PromoteToList(variables, output.BHoM_Guid));
                return new List<StatementSyntax>();
            }
            else if (node is BlockNode)
                return Statements(node as BlockNode, variables, depth);
            else
                return Statements(node, variables);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<StatementSyntax> Statements(this BlockNode node, Dictionary<Guid, Variable> variables, int depth = 0)
        {
            string commentLead = new string(Enumerable.Repeat('-', 4 * depth).ToArray());

            List<StatementSyntax> statements = node.InternalNodes.SelectMany(x => x.IStatements(variables, depth + 1)).Where(x => x != null).ToList();

            if (statements.Count > 0)
            {
                StatementSyntax statement = statements[0];
                if (statement.HasLeadingTrivia)
                {
                    SyntaxTriviaList trivia = statement.GetLeadingTrivia();
                    trivia = trivia.InsertRange(0, new List<SyntaxTrivia> {
                        SyntaxFactory.Comment(" "),
                        SyntaxFactory.Comment("//" + commentLead + " " + node.Description)
                    });
                    statements[0] = statement.WithLeadingTrivia(trivia);
                }
                else
                {
                    statements[0] = statements[0].WithLeadingTrivia(
                        SyntaxFactory.Comment(" "),
                        SyntaxFactory.Comment("//" + commentLead + " " + node.Description)
                    );
                }
            }

            return statements;
        }

        /***************************************************/

        private static List<StatementSyntax> Statements(this INode node, Dictionary<Guid, Variable> variables)
        {
            Dictionary<Guid, Variable> outputVariables = node.IOutputVariables(variables);
            foreach (KeyValuePair<Guid, Variable> kvp in outputVariables)
                variables[kvp.Key] = kvp.Value;

            List<ExpressionSyntax> arguments = Arguments(node, variables, out List<ReceiverParam> listInputs);
            ExpressionSyntax expression = IExpression(node, arguments);

            expression = HandleTypeDifferences(node, expression, variables, listInputs, out List<StatementSyntax> extraStatements);

            if (expression == null)
                return new List<StatementSyntax>();
            else if (node.IsDeclaration && outputVariables.Count > 0)
            {
                TypeSyntax returnType = node.IReturnType(listInputs.Count > 0 ? 1 : 0);
                List<StatementSyntax> statements = new List<StatementSyntax> { Declaration(expression, outputVariables.Values.ToList(), returnType) };
                return statements.Concat(extraStatements).ToList();
            }
            else
            {
                return new List<StatementSyntax> {
                    SyntaxFactory.ExpressionStatement(expression)
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

            // This method is becoming a bit of a mess and needs refactoring

            if (listInputs.Count == 1)
            {
                MemberAccessExpressionSyntax memberAccess = arguments[inputs.IndexOf(listInputs.First())] as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                    arguments[inputs.IndexOf(listInputs.First())] = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("_x"),
                        memberAccess.Name
                    );
                else
                    arguments[inputs.IndexOf(listInputs.First())] = SyntaxFactory.IdentifierName("_x");
            }
            else if (listInputs.Count > 1)
            {
                Dictionary<int, Tuple<ExpressionSyntax, SimpleNameSyntax>> listArguments = listInputs.ToDictionary(
                    x => inputs.IndexOf(x),
                    x =>
                    {
                        MemberAccessExpressionSyntax memberAccess = arguments[inputs.IndexOf(x)] as MemberAccessExpressionSyntax;
                        if (memberAccess != null)
                            return new Tuple<ExpressionSyntax, SimpleNameSyntax>(memberAccess.Expression, memberAccess.Name);
                        else
                            return new Tuple<ExpressionSyntax, SimpleNameSyntax>(arguments[inputs.IndexOf(x)], null);
                    }
                );
                bool switchToSelect = (listArguments.Values.Select(x => x.Item1.ToFullString()).Distinct().Count() == 1);

                foreach (var kvp in listArguments)
                {
                    if (switchToSelect)
                        arguments[kvp.Key] = SyntaxFactory.IdentifierName("_x");
                    else
                        arguments[kvp.Key] = SyntaxFactory.ElementAccessExpression(
                            kvp.Value.Item1,
                            SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax>
                            {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("_i"))
                            }))
                        );

                    if (kvp.Value.Item2 != null)
                    {
                        arguments[kvp.Key] = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            arguments[kvp.Key],
                            kvp.Value.Item2
                        );
                    }
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
                int count = listInputs.Count;
                List<ExpressionSyntax> arguments = listInputs.Select(x => ArgumentValue(x, variables)).ToList();
                List<MemberAccessExpressionSyntax> memberAccess = arguments.OfType<MemberAccessExpressionSyntax>().ToList();
                if (memberAccess.Count > 0)
                    count -= memberAccess.Count - memberAccess.Select(x => x.Expression.ToFullString()).Distinct().Count();

                if (count == 1)
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
                    
                if (!(node is SetPropertyNode)) // need a better way to do this
                {
                    foreach (DataParam output in node.Outputs)
                        PromoteToList(variables, output.BHoM_Guid);
                }
                    
            }

            return expression;
        }

        /***************************************************/

        private static ExpressionSyntax SelectLoop(ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs)
        {
            ReceiverParam input = listInputs.First();

            var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_x")), expression);

            var selectedTarget = Query.ArgumentValue(input, variables);
            if (selectedTarget is MemberAccessExpressionSyntax)
                selectedTarget = ((MemberAccessExpressionSyntax)selectedTarget).Expression;

            var selectAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, selectedTarget, SyntaxFactory.IdentifierName("Select"));
            var selectInvoc = SyntaxFactory.InvocationExpression(selectAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));

            var toListAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, selectInvoc, SyntaxFactory.IdentifierName("ToList"));
            return SyntaxFactory.InvocationExpression(toListAccess, SyntaxFactory.ArgumentList());
        }

        /***************************************************/

        private static ExpressionSyntax ForEachLoop(ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs)
        {
            ReceiverParam input = listInputs.First();

            var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_x")), expression);

            var loopTarget = Query.ArgumentValue(input, variables);
            if (loopTarget is MemberAccessExpressionSyntax)
                loopTarget = ((MemberAccessExpressionSyntax)loopTarget).Expression;

            var foreachAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, loopTarget, SyntaxFactory.IdentifierName("ForEach"));
            return SyntaxFactory.InvocationExpression(foreachAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));
        }

        /***************************************************/

        private static ExpressionSyntax ForLoop(INode node, ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs, out StatementSyntax loopStatement)
        {
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

            ExpressionSyntax minLimit = MinCountExpression(listInputs.Select(x => ArgumentValue(x, variables)).ToList());
            ExpressionSyntax condition = SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, SyntaxFactory.IdentifierName("_i"), minLimit);

            SeparatedSyntaxList<ExpressionSyntax> incrementors = SyntaxFactory.SeparatedList(new List<ExpressionSyntax>
            {
                SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName("_i"))
            });

            loopStatement = SyntaxFactory.ForStatement(declaration, initialisers, condition, incrementors, body);


            return SyntaxFactory.InvocationExpression(SyntaxFactory.ObjectCreationExpression(node.IReturnType(1), null, null));
        }

        /***************************************************/

        private static ExpressionSyntax MinCountExpression(List<ExpressionSyntax> lists)
        {
            lists = lists.Select(x => x is MemberAccessExpressionSyntax ? ((MemberAccessExpressionSyntax)x).Expression : x)
                .GroupBy(x => x.ToFullString())
                .Select(x => x.First())
                .ToList();

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



