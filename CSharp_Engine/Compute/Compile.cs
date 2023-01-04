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
using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.CSharp
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compiles C# code into a method")]
        [Input("code", "C# code to be compiled")]
        [Input("refFiles", "List of dll files necessary for that code to compile")]
        [Output("method", "Resulting method")]
        public static MethodInfo CompileMethod(string code, List<string> refFiles)
        {
            CSharpCodeProvider cSharp = new CSharpCodeProvider();
            var compileParams = new CompilerParameters(refFiles.ToArray());
            compileParams.GenerateInMemory = true;
            compileParams.GenerateExecutable = false;

            CompilerResults compilerResult = cSharp.CompileAssemblyFromSource(compileParams, code);
            if (compilerResult.Errors.Count > 0)
            {
                string message = "Failed to compile code. Errors:" + compilerResult.Errors.OfType<CompilerError>().Select(x => "\n" + x.ToString()).Aggregate((a,b) => a + b);
                BH.Engine.Base.Compute.RecordError(message);
                return null;
            }
            else
            {
                Assembly asm = compilerResult.CompiledAssembly;
                Type asmType = asm.GetTypes().First();
                return asmType.GetMethods().First();
            }
        }
        
        /***************************************************/
    }
}


