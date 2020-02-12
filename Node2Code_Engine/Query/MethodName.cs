using BH.Engine.Reflection;
using BH.oM.Node2Code;
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

        public static string MethodName(this MethodInfo method)
        {
            return method.DeclaringType.FullName + "." + method.Name;
        }

        /***************************************************/

        public static string MethodName(this ConstructorInfo constructor)
        {
            return constructor.DeclaringType.FullName;
        }

        /***************************************************/

        public static string MethodName(this Type type)
        {
            return type.FullName;
        }

        /***************************************************/
    }
}
