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
    public class StatementInfo : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public StatementSyntax Statement { get; set; } = null;

        public List<Guid> InputVariables { get; set; } = new List<Guid>();

        public Guid OutputVariable { get; set; } = Guid.Empty;

        public Guid GroupId { get; set; } = Guid.Empty;

        /***************************************************/
    }
}
