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

using BH.Engine.Reflection;
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
