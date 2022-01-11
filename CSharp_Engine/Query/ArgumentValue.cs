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

        [Description("Get the C# expression corresponding to a receiver param given a list of available variables")]
        [Input("receiver", "Input of a node we need the C# expression for")]
        [Input("variables", "Variables available in the context of the receiver param")]
        [Output("Microsoft.CodeAnalysis.CSharp.ExpressionSyntax corresponding to the receiver parameter")]
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


