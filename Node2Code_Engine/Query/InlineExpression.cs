/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.Engine.Node2Code.Objects;
using BH.Engine.Reflection;
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

        public static Dictionary<Guid, Variable> IInlineExpression(this INode node, Dictionary<Guid, Variable> variables)
        {
            return InlineExpression(node as dynamic, variables);
        }

        /***************************************************/

        public static Dictionary<Guid, Variable> InlineExpression(this TypeNode node, Dictionary<Guid, Variable> variables)
        {
            Guid id = node.Outputs.First().BHoM_Guid;

            Variable variable = new Variable
            {
                Expression = SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(node.Type.FullName)),
                SourceId = id,
                Type = node.Outputs.First().DataType
            };

            return new Dictionary<Guid, Variable> {
                { id, variable }
            };
        }

        /***************************************************/

        public static Dictionary<Guid, Variable> InlineExpression(this ParamNode node, Dictionary<Guid, Variable> variables)
        {
            Dictionary<Guid, Variable> newVariables = new Dictionary<Guid, Variable>();

            foreach (DataParam data in node.Outputs)
            {
                ExpressionSyntax expression = null;
                if (data.Data is Enum)
                    expression = SyntaxFactory.IdentifierName(data.DataType.FullName + "." + data.Data.ToString()); // will need to figure out this one
                else if (data.Data is string)
                    expression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(data.Data.ToString()));
                else
                    expression = SyntaxFactory.IdentifierName(data.Data.ToString());

                if (expression != null)
                {
                    newVariables[data.BHoM_Guid] = new Variable
                    {
                        Expression = expression,
                        Type = data.DataType,
                        SourceId = data.BHoM_Guid
                    };
                }
            }

            return newVariables;
        }

        /***************************************************/

        public static Dictionary<Guid, Variable> InlineExpression(this GetPropertyNode node, Dictionary<Guid, Variable> variables)
        {
            List<ExpressionSyntax> arguments = node.Inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

            Guid id = node.Outputs.First().BHoM_Guid;

            Variable variable = new Variable
            {
                Expression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    arguments[0],
                    SyntaxFactory.IdentifierName(arguments[1].ToFullString().Replace("\"", ""))
                ),
                SourceId = id,
                Type = node.Outputs.First().DataType
            };

            return new Dictionary<Guid, Variable> {
                { id, variable }
            };
        }

        /***************************************************/

        public static Dictionary<Guid, Variable> InlineExpression(this ExplodeNode node, Dictionary<Guid, Variable> variables)
        {
            List<ExpressionSyntax> arguments = node.Inputs.Select(x => Query.ArgumentValue(x, variables)).ToList();

            IEnumerable<DataParam> validOutputs = node.Outputs.Where(x => x.TargetIds.Count > 0);
            if (validOutputs.Count() == 0)
                return new Dictionary<Guid, Variable>();
            else
                return validOutputs.ToDictionary(
                    output =>   output.BHoM_Guid,
                    output => new Variable
                    {
                        Expression = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            arguments[0],
                            SyntaxFactory.IdentifierName(output.Name)
                        ) as ExpressionSyntax,
                        SourceId = output.BHoM_Guid,
                        Type = output.DataType
                    }
                );
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        public static Dictionary<Guid, Variable> InlineExpression(this INode node, Dictionary<Guid, Variable> variables)
        {
            return new Dictionary<Guid, Variable>();
        }

        /***************************************************/
    }
}
