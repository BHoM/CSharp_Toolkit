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

        //public static StatementInfo IStatement(this INode node, Dictionary<Guid, Variable> variables)
        //{
        //    List<Variable> outputVariables = node.IOutputVariables(variables);
        //    List<ExpressionSyntax> arguments = Arguments(node, variables, out List<ReceiverParam> listInputs);
        //    ExpressionSyntax expression = Expression(node as dynamic, arguments);

        //    if (listInputs.Count > 0)
        //    {
        //        expression = Loop(expression, variables, listInputs);
        //        if (node.Outputs.Count > 0)
        //            PromoteToList(variables, node.Outputs.First().BHoM_Guid);
        //    }
                
        //    if (expression != null && node.IsDeclaration && outputVariables.Count > 0)
        //    {
        //        TypeSyntax returnType = node.ReturnType(listInputs.Count > 0 ? 1 : 0);
        //        return new StatementInfo
        //        {
        //            Statement = Declaration(expression, outputVariables, returnType)
        //        };
        //    }
        //    else
        //    {
        //        return new StatementInfo
        //        {
        //            Statement = SyntaxFactory.ExpressionStatement(expression)
        //        };
        //    }
        //}

        /***************************************************/
        /**** Private Methods                           ****/
        ///***************************************************/

        //private static List<ExpressionSyntax> Arguments(INode node, Dictionary<Guid, Variable> variables, out List<ReceiverParam> listInputs)
        //{
        //    // Get the arguments
        //    int lastNonDefault = node.Inputs.FindLastIndex(x => x.SourceId != Guid.Empty);
        //    List<ReceiverParam> inputs = node.Inputs.Take(lastNonDefault + 1).ToList();
        //    List<ExpressionSyntax> arguments = inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

        //    listInputs = inputs.Where(x => x.DepthDifference(variables) == -1).ToList();
        //    if (listInputs.Count == 1)
        //        arguments[inputs.IndexOf(listInputs.First())] = SyntaxFactory.IdentifierName("_x");

        //    return arguments;
        //}

        ///***************************************************/

        //private static ExpressionSyntax Loop(ExpressionSyntax expression, Dictionary<Guid, Variable> variables, List<ReceiverParam> listInputs)
        //{
        //    ReceiverParam input = listInputs.First();

        //    var selectAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Query.ArgumentValue(input, variables), SyntaxFactory.IdentifierName("Select"));
        //    var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_x")), expression);
        //    var selectInvoc = SyntaxFactory.InvocationExpression(selectAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));
        //    return selectInvoc;
        //}

        ///***************************************************/

        //private static LocalDeclarationStatementSyntax Declaration(ExpressionSyntax expression, List<Variable> outputVariables, TypeSyntax outputType)
        //{
        //    EqualsValueClauseSyntax intialiser = SyntaxFactory.EqualsValueClause(expression);
        //    VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariables.First().SyntaxToken(), null, intialiser);

        //    VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(outputType, SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //    return SyntaxFactory.LocalDeclarationStatement(declaration);
        //}

        ///***************************************************/

        //private static void PromoteToList(Dictionary<Guid, Variable> variables, Guid id)
        //{
        //    if (variables.ContainsKey(id))
        //        variables[id].Type = typeof(List<>).MakeGenericType(new Type[] { variables[id].Type });
        //}

        ///***************************************************/

        //public static StatementInfo Statement(this MethodNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    StatementSyntax statement = null;
        //    int lastNonDefault = node.Inputs.FindLastIndex(x => x.SourceId != Guid.Empty);
        //    List<ReceiverParam> inputs = node.Inputs.Take(lastNonDefault + 1).ToList();
        //    List<ExpressionSyntax> arguments = inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

        //    string methodName = node.Method.MethodName();

        //    switch (methodName)
        //    {
        //        case "BH.UI.Components.DeleteCaller.Delete":
        //        case "BH.UI.Components.ExecuteCaller.Execute":
        //        case "BH.UI.Components.MoveCaller.Move":
        //        case "BH.UI.Components.PullCaller.Pull":
        //        case "BH.UI.Components.PushCaller.Push":
        //            lastNonDefault = node.Inputs.FindLastIndex(node.Inputs.Count - 2, x => x.SourceId != Guid.Empty);
        //            arguments = arguments.Take(lastNonDefault + 1).ToList();
        //            statement = InstanceMethodStatement(node, arguments, outputVariable);
        //            break;
        //        default:
        //            statement = StaticMethodStatement(node, variables, outputVariable);
        //            break;
        //    }

        //    return new StatementInfo
        //    {
        //        Statement = statement,
        //        InputVariables = inputs.Select(x => x.BHoM_Guid).ToList(),
        //        OutputVariable = outputVariable.SourceId
        //    };
        //}

        ///***************************************************/

        //public static StatementInfo Statement(this ConstructorNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    int lastNonDefault = node.Inputs.FindLastIndex(x => x.SourceId != Guid.Empty);
        //    List<ReceiverParam> inputs = node.Inputs.Take(lastNonDefault + 1).ToList();
        //    List<ArgumentSyntax> arguments = inputs.Select(x => SyntaxFactory.Argument(Query.ArgumentValue(x, variables))).ToList();
        //    ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

        //    TypeSyntax objectType = SyntaxFactory.ParseTypeName(node.Constructor.MethodName());
        //    if (node.Constructor.IsGenericMethod)
        //    {
        //        List<TypeSyntax> genericTypes = node.Constructor.GetGenericArguments().Select(x => SyntaxFactory.ParseTypeName(x.FullName)).ToList();
        //        objectType = SyntaxFactory.GenericName(SyntaxFactory.Identifier(node.Constructor.MethodName()), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(genericTypes)));
        //    }

        //    ObjectCreationExpressionSyntax creator = SyntaxFactory.ObjectCreationExpression(objectType, argumentList, null);
        //    EqualsValueClauseSyntax intialiser = SyntaxFactory.EqualsValueClause(creator);
        //    VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariable.SyntaxToken(), null, intialiser);

        //    VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(node.ReturnType(), SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //    LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration);

        //    return new StatementInfo
        //    {
        //        Statement = localDeclaration,
        //        InputVariables = inputs.Select(x => x.BHoM_Guid).ToList(),
        //        OutputVariable = outputVariable.SourceId
        //    };
        //}

        ///***************************************************/

        //public static StatementInfo Statement(this InitialiserNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    List<ReceiverParam> inputs = node.Inputs.Where(x => x.SourceId != Guid.Empty).ToList();
        //    List<Tuple<string, ExpressionSyntax>> arguments = inputs
        //        .Select(x => new Tuple<string, ExpressionSyntax>(x.Name, Query.ArgumentValue(x, variables)))
        //        .ToList();

        //    List<ExpressionSyntax> assignments = arguments.Select(x => SyntaxFactory.AssignmentExpression(
        //        SyntaxKind.SimpleAssignmentExpression,
        //        SyntaxFactory.IdentifierName(x.Item1),
        //        x.Item2
        //    ) as ExpressionSyntax).ToList();

        //    TypeSyntax objectType = SyntaxFactory.ParseTypeName(node.ObjectType.MethodName());
        //    InitializerExpressionSyntax initialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(assignments));
        //    ObjectCreationExpressionSyntax creator = SyntaxFactory.ObjectCreationExpression(objectType, null, initialiser);

        //    EqualsValueClauseSyntax equal = SyntaxFactory.EqualsValueClause(creator);
        //    VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariable.SyntaxToken(), null, equal);

        //    VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(node.ReturnType(), SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //    LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration);

        //    return new StatementInfo
        //    {
        //        Statement = localDeclaration,
        //        InputVariables = inputs.Select(x => x.BHoM_Guid).ToList(),
        //        OutputVariable = outputVariable.SourceId
        //    };
        //}

        ///***************************************************/

        //public static StatementInfo Statement(this SetPropertyNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    if (node.Inputs.Count < 3)
        //        return null;

        //    List<ExpressionSyntax> inputs = node.Inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

        //    MemberAccessExpressionSyntax memberAccess = SyntaxFactory.MemberAccessExpression(
        //        SyntaxKind.SimpleMemberAccessExpression,
        //        inputs[0],
        //        SyntaxFactory.IdentifierName(inputs[1].ToFullString().Replace("\"", ""))
        //    );

        //    AssignmentExpressionSyntax assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, memberAccess, inputs[2]);
        //    ExpressionStatementSyntax statement = SyntaxFactory.ExpressionStatement(assignment);

        //    return new StatementInfo
        //    {
        //        Statement = statement,
        //        InputVariables = node.Inputs.Select(x => x.BHoM_Guid).ToList(),
        //        OutputVariable = outputVariable.SourceId
        //    };

        //}

        ///***************************************************/

        //public static StatementInfo Statement(this CustomObjectNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    List<ReceiverParam> inputs = node.Inputs.Where(x => x.SourceId != Guid.Empty).ToList();
        //    List<Tuple<string, ExpressionSyntax>> arguments = inputs
        //        .Select(x => new Tuple<string, ExpressionSyntax>(x.Name, Query.ArgumentValue(x, variables)))
        //        .ToList();

        //    List<ExpressionSyntax> dicAssignments = arguments.Select(x => SyntaxFactory.InitializerExpression(
        //        SyntaxKind.ComplexElementInitializerExpression,
        //        SyntaxFactory.SeparatedList(new List<ExpressionSyntax> {
        //            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(x.Item1)),
        //            x.Item2
        //        })
        //    ).WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.Tab) as ExpressionSyntax).ToList();

        //    TypeSyntax dicType = SyntaxFactory.ParseTypeName(typeof(Dictionary<string, object>).ToText(true));
        //    InitializerExpressionSyntax dicInitialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(dicAssignments));
        //    ObjectCreationExpressionSyntax dicCreator = SyntaxFactory.ObjectCreationExpression(dicType, null, dicInitialiser);

        //    List<ExpressionSyntax> assignments = new List<ExpressionSyntax>
        //    {
        //         SyntaxFactory.AssignmentExpression(
        //             SyntaxKind.SimpleAssignmentExpression,
        //             SyntaxFactory.IdentifierName("CustomData"),
        //             dicCreator
        //         )
        //    };

        //    TypeSyntax objectType = SyntaxFactory.ParseTypeName(typeof(CustomObject).FullName);
        //    InitializerExpressionSyntax initialiser = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(assignments));
        //    ObjectCreationExpressionSyntax creator = SyntaxFactory.ObjectCreationExpression(objectType, null, initialiser);

        //    EqualsValueClauseSyntax equal = SyntaxFactory.EqualsValueClause(creator);
        //    VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariable.SyntaxToken(), null, equal);

        //    VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(node.ReturnType(), SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //    LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration);

        //    return new StatementInfo
        //    {
        //        Statement = localDeclaration,
        //        InputVariables = inputs.Select(x => x.BHoM_Guid).ToList(),
        //        OutputVariable = outputVariable.SourceId
        //    };
        //}


        ///***************************************************/
        ///**** Private Methods                           ****/
        ///***************************************************/

        //public static StatementInfo Statement(this INode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    return null;
        //}

        ///***************************************************/

        //public static StatementSyntax StaticMethodStatement(this MethodNode node, Dictionary<Guid, Variable> variables, Variable outputVariable)
        //{
        //    int lastNonDefault = node.Inputs.FindLastIndex(x => x.SourceId != Guid.Empty);
        //    List<ReceiverParam> inputs = node.Inputs.Take(lastNonDefault + 1).ToList();

        //    List<ReceiverParam> listInputs = inputs.Where(x => x.DepthDifference(variables) == -1).ToList();
        //    bool needLoop = listInputs.Count == 1;

        //    List<ArgumentSyntax> arguments = inputs.Select(x => SyntaxFactory.Argument(Query.ArgumentValue(x, variables))).ToList();
        //    if (needLoop)
        //        arguments[inputs.IndexOf(listInputs.First())] = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("x"));
        //    ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

        //    InvocationExpressionSyntax invocation = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(node.Method.MethodName()), argumentList);
        //    int returnTypeDepth = 0;

        //    if (needLoop)
        //    {
        //        ReceiverParam input = listInputs.First();

        //        var selectAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Query.ArgumentValue(input, variables), SyntaxFactory.IdentifierName("Select"));
        //        var lambda = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("x")), invocation);
        //        var selectInvoc = SyntaxFactory.InvocationExpression(selectAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new List<ArgumentSyntax> { SyntaxFactory.Argument(lambda) })));
        //        invocation = selectInvoc;

        //        returnTypeDepth = 1;
        //    }

        //    if (node.Outputs.Where(x => x.TargetIds.Count > 0).Count() == 0)
        //    {
        //        return SyntaxFactory.ExpressionStatement(invocation);
        //    }
        //    else
        //    {
        //        EqualsValueClauseSyntax intialiser = SyntaxFactory.EqualsValueClause(invocation);
        //        VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariable.SyntaxToken(), null, intialiser);

        //        VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(node.ReturnType(returnTypeDepth), SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //        LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration);

        //        return localDeclaration;
        //    }

        //}

        ///***************************************************/

        //public static StatementSyntax InstanceMethodStatement(this MethodNode node, List<ExpressionSyntax> inputs, Variable outputVariable)
        //{
        //    List<ArgumentSyntax> arguments = inputs.Skip(1).Select(x => SyntaxFactory.Argument(x)).ToList();
        //    ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));

        //    MemberAccessExpressionSyntax memberAccess = SyntaxFactory.MemberAccessExpression(
        //        SyntaxKind.SimpleMemberAccessExpression,
        //        inputs[0],
        //        SyntaxFactory.IdentifierName(node.Method.Name)
        //    );
        //    InvocationExpressionSyntax invocation = SyntaxFactory.InvocationExpression(memberAccess, argumentList);


        //    if (node.Outputs.Where(x => x.TargetIds.Count > 0).Count() == 0)
        //    {
        //        return SyntaxFactory.ExpressionStatement(invocation);
        //    }
        //    else
        //    {
        //        EqualsValueClauseSyntax intialiser = SyntaxFactory.EqualsValueClause(invocation);
        //        VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(outputVariable.SyntaxToken(), null, intialiser);

        //        VariableDeclarationSyntax declaration = SyntaxFactory.VariableDeclaration(node.ReturnType(), SyntaxFactory.SeparatedList(new List<VariableDeclaratorSyntax> { declarator }));
        //        LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration);

        //        return localDeclaration;
        //    }
        //}

        ///***************************************************/
    }
}
