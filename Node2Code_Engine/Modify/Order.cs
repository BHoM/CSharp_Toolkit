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

        public static List<StatementInfo> Order(this List<StatementInfo> statements, List<NodeGroup> groups)
        {
            foreach (NodeGroup group in groups)
            {
                Guid groupId = group.BHoM_Guid;
                int startIndex = statements.FindLastIndex(x => x.GroupId == groupId);
                while (startIndex > 0)
                {
                    int nextIndex = statements.FindLastIndex(startIndex - 1, x => x.GroupId == groupId);
                    if (nextIndex >= 0 && nextIndex < startIndex - 1)
                    {
                        List<Guid> outsideVariables = statements.GetRange(nextIndex + 1, startIndex - nextIndex - 1).SelectMany(x => x.InputVariables).ToList();
                        if (!outsideVariables.Contains(statements[nextIndex].OutputVariable))
                        {
                            StatementInfo statement = statements[nextIndex];
                            statements.Remove(statement);
                            statements.Insert(startIndex - 1, statement);

                            startIndex -= 1;
                        }
                        else
                            break;
                    }
                    else if (nextIndex == startIndex - 1)
                    {
                        startIndex -= 1;
                    }
                    else
                        break;
                }
            }

            return statements;
        }

        /***************************************************/
    }
}
