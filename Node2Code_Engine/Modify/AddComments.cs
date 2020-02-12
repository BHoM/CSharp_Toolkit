using BH.Engine.Reflection;
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
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<StatementInfo> AddComments(this List<StatementInfo> statements, List<NodeGroup> groups)
        {
            foreach (NodeGroup group in groups)
            {
                Guid groupId = group.BHoM_Guid;
                int startIndex = statements.FindIndex(x => x.GroupId == groupId);
                if (startIndex >= 0)
                {
                    StatementSyntax statement = statements[startIndex].Statement;
                    statements[startIndex].Statement = statement.WithLeadingTrivia(
                        SyntaxFactory.Comment(startIndex == 0 ? "" : " "),
                        SyntaxFactory.Comment("// " + group.Description)
                    );
                }  
            }

            return statements;
        }

        /***************************************************/
    }
}
