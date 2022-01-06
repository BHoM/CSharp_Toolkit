/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Reflection.Attributes;
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
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert a BHoM Cluster content into a Microsoft CSharp syntax node")]
        [Input("content", "Cluster content to convert")]
        [Output("Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode corresponding to the input BHoM content")]
        public static CSharpSyntaxNode ToCSharpSyntaxNode(this ClusterContent content)
        {
            MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(content.ReturnType(), content.Name)
                .AddModifiers(new SyntaxToken[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword) })
                .AddParameterListParameters(content.MethodParameters().ToArray())
                .AddBodyStatements(content.Body().ToArray());

            return methodDeclaration;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<ParameterSyntax> MethodParameters(this ClusterContent content)
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
    }
}


