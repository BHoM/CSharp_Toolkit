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
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Variable Variable(DataParam output, Dictionary<string, int> nameCounts)
        {
            // Create the output variables
            string outputName = VariableName(output.Name, nameCounts);
            if (outputName.Length > 0)
            {
                return new Variable
                {
                    Name = outputName,
                    Expression = SyntaxFactory.IdentifierName(outputName),
                    SourceId = output.BHoM_Guid,
                    Type = output.DataType
                };
            }
            else if (output.Data != null)
            {
                return new Variable
                {
                    Expression = SyntaxFactory.IdentifierName(output.Data.ToString()),
                    SourceId = output.BHoM_Guid,
                    Type = output.DataType
                };
            }
            else
                return null;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static string VariableName(string name, Dictionary<string, int> nameCounts)
        {
            name = name.Length == 0 ? "" : name.Substring(0, 1).ToLower() + name.Substring(1);

            if (nameCounts.ContainsKey(name))
            {
                nameCounts[name] += 1;
                name += "_" + nameCounts[name];
            }
            else
                nameCounts[name] = 1;

            return name;
        }

        /***************************************************/
    }
}
