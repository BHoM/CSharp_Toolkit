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

using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
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

        [Description("Convert a BHoM Cluster content into C# code")]
        [Input("content", "Cluster content to convert")]
        [Output("Corresponding C# code")]
        public static string ToCSharpText(this ClusterContent content)
        {
            if (content == null)
                return "";

            CSharpSyntaxNode cSharpNode = content.ToCSharpSyntaxNode();

            if (cSharpNode == null)
            {
                BH.Engine.Base.Compute.RecordError("failed to convert the cluster content into a CSharp syntax node.");
                return "";
            }   
            else
                return cSharpNode.NormalizeWhitespace().ToFullString();
        }

        /***************************************************/

        [Description("Convert a method into C# code")]
        [Input("method", "method to convert")]
        [Input("wrapWithType", "The returned code will include the declaring type and its namespace.")]
        [Input("standAlone", "Add the minimum amount of code to the declaring type definition so it can be compiled in isolation.")]
        [Output("Corresponding C# code")]
        public static string ToCSharpText(this MethodInfo method, bool wrapWithType = false, bool standAlone = false)
        {
            Assembly asm = method.DeclaringType.Assembly;
            List<ParameterInfo> methodParams = method.GetParameters().ToList();

            var decompiler = new CSharpDecompiler(asm.Location, new DecompilerSettings());
            var name = new FullTypeName(method.DeclaringType.FullName);
            ITypeDefinition typeInfo = decompiler.TypeSystem.MainModule.Compilation.FindType(name).GetDefinition();
            List<IMethod> matches = typeInfo.Methods.Where(x => x.Name == method.Name && x.Parameters.Count == methodParams.Count).ToList();

            IMethod match = null;
            if (matches.Count == 1)
                match = matches.First();
            else if (matches.Count > 1)
                match = matches.FirstOrDefault(x => IsMatching(methodParams, x.Parameters));

            if (match == null)
                return "Failed to find the corresponding method";
            else
            {
                string code = decompiler.DecompileAsString(match.MetadataToken);
                if (wrapWithType)
                    code = WrapWithType(code, method);
                if (standAlone)
                    code = Compute.MakeStandAlone(code, method);
                return code;
            }
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool IsMatching(List<ParameterInfo> methodParams, IReadOnlyList<IParameter> reflectionParams)
        {
            if (methodParams.Count != reflectionParams.Count)
                return false;

            bool ok = true;
            for (int i = 0; i < methodParams.Count; i++)
            {
                string fullName = methodParams[i].ParameterType.FullName;
                if (fullName != null && !fullName.Contains(','))
                    ok &= fullName == reflectionParams[i].Type.FullName;
                else
                    ok &= methodParams[i].Name == reflectionParams[i].Name;
            }

            return ok;
        }

        /***************************************************/

        private static string WrapWithType(string code, MethodInfo method)
        {
            List<string> lines = code.Split(new char[] { '\n' }).ToList();
            int nbUsing = lines.FindLastIndex(x => x.StartsWith("using")) + 1;

            List<string> usings = lines.Take(nbUsing).ToList();
            List<string> body = lines.Skip(nbUsing).ToList();

            for (int i = 0; i < body.Count; i++)
                body[i] = "\t\t" + body[i];

            List<string> newLines = new List<string>
            {
                "",
                $"namespace {method.DeclaringType.Namespace}",
                "{",
                $"\tpublic static partial class {method.DeclaringType.Name}",
                "\t{"
            };

            List<string> lastLines = new List<string>
            {
                "\t}",
                "}"
            };

            List<string> fullCode = usings.Concat(newLines).Concat(body).Concat(lastLines).ToList();
            return fullCode.Aggregate((a, b) => a + "\n" + b);
        }

        /***************************************************/
    }
}




