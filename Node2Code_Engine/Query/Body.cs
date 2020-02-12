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

using BH.Engine.Node2Code.Objects;
using BH.oM.Base;
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
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<StatementSyntax> Body(this ClusterContent content)
        {
            // Apply the groups
            List<INode> nodes = Compute.ApplyGroups(content.InternalNodes, content.NodeGroups);

            // Order the nodes 
            nodes = Compute.NodeSequence(nodes);

            // Get the variables
            Dictionary<Guid, Variable> variables = Compute.Variables(nodes, content.Inputs);

            // Create the statements
            List<StatementInfo> statements = nodes.Where(x => !x.IsInline)
                .SelectMany(node => IStatements(node, variables))
                .ToList();

            // Add return statement
            if (content.Outputs.Count > 0)
                statements.Add(Create.ReturnStatement(variables[content.Outputs.First().SourceId]));

            return statements.Select(x => x.Statement).ToList();
        }

        /***************************************************/
    }
}
