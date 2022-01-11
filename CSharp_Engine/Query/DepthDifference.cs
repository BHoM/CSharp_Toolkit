/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.CSharp;
using BH.oM.Programming;
using BH.oM.Base.Attributes;
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
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Compares the depth of the data type accepted by a receiver against the data type of its source." +
            "\nThe depth represents the number of list levels are wrapped around the underlying type." +
            "\nE.g. List<T> havve a depth of 1, List<List<T>> havve a depth of 2,...")]
        [Input("receiver", "The receiver to get the depth difference for compared to its source")]
        [Input("emiters", "The receiver only stores the id of its source so this is the list of available sources")]
        [Output("receiver.Depth - receiver.Source.Depth")]
        public static int DepthDifference(this ReceiverParam receiver, Dictionary<Guid, DataParam> emitters)
        {
            if (emitters.ContainsKey(receiver.SourceId))
            {
                return DepthDifference(receiver.DataType, emitters[receiver.SourceId].DataType);
            }
            else
                return 0;
        }

        /***************************************************/

        [Description("Compares the depth of the data type accepted by a receiver against the data type of its source." +
        "\nThe depth represents the number of list levels are wrapped around the underlying type." +
        "\nE.g. List<T> havve a depth of 1, List<List<T>> havve a depth of 2,...")]
        [Input("receiver", "The receiver to get the depth difference for compared to its source")]
        [Input("variables", "The receiver only stores the id of its source so this is the list of available sources")]
        [Output("receiver.Depth - receiver.Source.Depth")]
        public static int DepthDifference(this ReceiverParam receiver, Dictionary<Guid, Variable> variables)
        {
            if (variables.ContainsKey(receiver.SourceId))
            {
                return DepthDifference(receiver.DataType, variables[receiver.SourceId].Type);
            }
            else
                return 0;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static int DepthDifference(this Type t1, Type t2)
        {
            return t1.UnderlyingType().Depth - t2.UnderlyingType().Depth;
        }

        /***************************************************/
    }
}


