﻿/*
	Genetic Algorithm Framework for .Net
	Copyright (C) 2016  John Newcombe

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

		You should have received a copy of the GNU Lesser General Public License
		along with this program.  If not, see <http://www.gnu.org/licenses/>.

	http://johnnewcombe.net
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAF.Operators
{
    /// <summary>
    /// Enum representing the method to be used for the Copy operator.
    /// </summary>
    public enum CopyMethod
    {
        /// <summary>
        /// A random selection of solutions are copied.
        /// </summary>
        Random,
        /// <summary>
        /// The fittest solutions are copied.
        /// </summary>
        Fittest
    }
}
