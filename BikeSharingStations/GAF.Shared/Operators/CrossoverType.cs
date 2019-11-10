/*
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
    /// Enum that defines the type of Crossover to be implemented.
    /// </summary>
    public enum CrossoverType
    {
        /// <summary>
        /// A single random point is selected for each parent chromosome and one part is swapped between them.
        /// </summary>
        SinglePoint = 1,
        /// <summary>
        /// Two points are selected to determine a centre section in each parent, this is swapped between them.
        /// </summary>
        DoublePoint = 2,
        /// <summary>
        /// A single parent is used to create a child however the order of a second parent determines how the 
        /// chromosome is arranged. This method only works with cromosomes that have a unique set of genes (by Value).
        /// For this to be useable with custom object based genes, the Equals method should be overriden in the gene definition to return a value. 
        /// </summary>
        DoublePointOrdered = 3
    }
}
