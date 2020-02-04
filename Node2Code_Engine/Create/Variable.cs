using BH.oM.Node2Code;
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
