/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.Engine.Reflection;
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

        [Description("Get the C# inline expressions corresponding to a node outputs given a list of available variables.")]
        [Input("node", "Node to get the expression from.")]
        [Input("variables", "List of variables available in the context of the node.")]
        [Output("Microsoft.CodeAnalysis.CSharp.ExpressionSyntax corresponding to the node")]
        public static Dictionary<Guid, Variable> IOutputVariables(this INode node, Dictionary<Guid, Variable> variables)
        {
            return OutputVariables(node as dynamic, variables);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Dictionary<Guid, Variable> OutputVariables(this TypeNode node, Dictionary<Guid, Variable> variables)
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

        private static Dictionary<Guid, Variable> OutputVariables(this ParamNode node, Dictionary<Guid, Variable> variables)
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

        private static Dictionary<Guid, Variable> OutputVariables(this GetPropertyNode node, Dictionary<Guid, Variable> variables)
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

        private static Dictionary<Guid, Variable> OutputVariables(this SetPropertyNode node, Dictionary<Guid, Variable> variables)
        {
            if (node.Inputs.Count == 0)
                return new Dictionary<Guid, Variable>();

            Guid id = node.Inputs.First().SourceId;
            if (variables.ContainsKey(id))
                return new Dictionary<Guid, Variable> { { node.Outputs.First().BHoM_Guid, variables[id] } };
            else
                return new Dictionary<Guid, Variable>();
        }

        /***************************************************/

        private static Dictionary<Guid, Variable> OutputVariables(this ExplodeNode node, Dictionary<Guid, Variable> variables)
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

        private static Dictionary<Guid, Variable> OutputVariables(this INode node, Dictionary<Guid, Variable> variables)
        {
            return node.Outputs.Where(x => x.TargetIds.Count > 0 && variables.ContainsKey(x.BHoM_Guid))
                .ToDictionary(x => x.BHoM_Guid, x => variables[node.Outputs.First().BHoM_Guid]);
        }

        /***************************************************/
    }
}





