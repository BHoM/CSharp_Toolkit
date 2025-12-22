/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Engine.CSharp
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Add the minimum amount of code to the input code so it can be compiled in isolation.")]
        [Input("code", "Original code to be modified so it can be compiled is isolation.")]
        [Input("method", "Method that was used to generate the input code.")]
        [Output("code", "Stand alone code that should content all the necessary declarations to compile.")]
        public static string MakeStandAlone(string code, MethodInfo method)
        {
            List<MethodBase> usedMethods = method.UsedMethods(true);
            List<MethodBase> sameType = usedMethods.Where(x => x.DeclaringType == method.DeclaringType).ToList();
            if (sameType.Count == 0)
                return code;

            // Change the class name to avoid obfuscation
            string declaringTypeName = method.DeclaringType.Name;
            code = code.Replace($"class {declaringTypeName}", $"class _{declaringTypeName}");

            // Add declaring type in front of method calls from the same type
            foreach (MethodBase m in sameType.Where(x => x.IsPublic))
                code = Regex.Replace(code, "([^\\.])"+ m.Name,  "$1" + declaringTypeName + "." + m.Name);

            // Add the private methods
            List<string> extraCode = new List<string>();
            List<string> extraUsings = new List<string>();
            List<MethodBase> assemblyMethods = usedMethods.Where(x => x.IsAssembly).ToList();
            List<MethodInfo> privateMethods = CollectPrivateMethods(sameType.Where(x => x.IsPrivate).Concat(assemblyMethods)).OfType<MethodInfo>().ToList();
            
            foreach (MethodInfo m in privateMethods)
            {
                List<string> extraLines = m.ToCSharpText().Split(new char[] { '\n' }).ToList();
                int index = extraLines.FindIndex(x => !x.StartsWith("using"));

                extraUsings.AddRange(extraLines.Take(index));
                extraCode.AddRange(new List<string> { "", "/***************************************************/" });
                extraCode.AddRange(extraLines.Skip(index));
            }
                
            // Complete the code if any private method needs to be added
            if (extraCode.Count > 0)
            {
                List<string> lines = code.Split(new char[] { '\n' }).ToList();

                // Fix the usings if any was added from private methods
                int usingCut = lines.FindIndex(x => !x.StartsWith("using"));
                List<string> usings = lines.Take(usingCut).Concat(extraUsings).Distinct().ToList();
                lines = lines.Skip(usingCut).ToList();

                int lastIndex = lines.LastIndexOf("\t}");
                if (lastIndex > 0)
                {
                    code = usings
                        .Concat(lines.Take(lastIndex))
                        .Concat(extraCode.Select(x => "\t\t" + x))
                        .Concat(lines.Skip(lastIndex))
                        .Aggregate((a, b) => a + "\n" + b);
                }
                else
                    code = usings.Concat(lines).Concat(extraCode).Aggregate((a, b) => a + "\n" + b);
            }

            return code;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<MethodBase> CollectPrivateMethods(IEnumerable<MethodBase> methods, List<MethodBase> collected = null)
        {
            if (collected == null)
                collected = new List<MethodBase>();
            collected.AddRange(methods.Where(x => !collected.Contains(x) && !x.IsAssembly));

            foreach (MethodBase method in methods)
            {
                List<MethodBase> privateMethods = method.UsedMethods(true).Where(x => x.IsPrivate && !collected.Contains(x)).ToList();
                collected.AddRange(privateMethods);
                CollectPrivateMethods(privateMethods, collected);
            }

            return collected;
        }
        

        /***************************************************/
    }
}






