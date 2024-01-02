/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
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

        [Description("Get the C# type syntax corresponding to the first output of a cluster content")]
        [Input("content", "Cluster content to get the type syntax from")]
        [Output("Microsoft.CodeAnalysis.CSharp.TypeSyntax corresponding to the return type of the content first output")]
        public static TypeSyntax ReturnType(this ClusterContent content)
        {
            string returnTypeString = "void";
            if (content.Outputs.Count > 0)
                returnTypeString = content.Outputs.First().DataType.FullName;

            return SyntaxFactory.ParseTypeName(returnTypeString);
        }

        /***************************************************/

        [Description("Get the C# type syntax corresponding to the first output of a node")]
        [Input("node", "node to get the type syntax from")]
        [Input("depth", "number of list levels the return type needs to be wrapped into")]
        [Output("Microsoft.CodeAnalysis.CSharp.TypeSyntax corresponding to the return type of the node first output")]
        public static TypeSyntax IReturnType(this INode node, int depth = 0)
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




