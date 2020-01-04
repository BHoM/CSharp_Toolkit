using BH.oM.Base;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Node2Code
{
    public class Variable : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Type Type { get; set; }

        public ExpressionSyntax Expression { get; set; }

        public Guid SourceId { get; set; } = Guid.Empty;

        /***************************************************/
    }
}
