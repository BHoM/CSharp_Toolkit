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

        public static TypeSyntax ReturnType(this ClusterContent content)
        {
            string returnTypeString = "void";
            if (content.Outputs.Count > 0)
                returnTypeString = content.Outputs.First().DataType.FullName;

            return SyntaxFactory.ParseTypeName(returnTypeString);
        }

        /***************************************************/

        public static TypeSyntax ReturnType(this INode node, int depth = 0)
        {
            string returnTypeString = "void";
            if (node.Outputs.Count > 0)
                returnTypeString = node.Outputs.First().DataType.ToText(true);

            for (int i = 0; i < depth; i++)
                returnTypeString = "List<" + returnTypeString + ">";

            return SyntaxFactory.ParseTypeName(returnTypeString);
        }

        /***************************************************/
    }
}
