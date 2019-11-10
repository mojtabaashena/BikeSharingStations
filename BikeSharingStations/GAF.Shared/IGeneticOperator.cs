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

namespace GAF
{
    /// <summary>
    /// This is the interface that should be implemented when creating custom genetic operators.
    /// </summary>
	/// <remarks>An operator shoult only NOT change
	/// the currentPopulation. It IS acceptable for an operator to return a new
	/// population with a different number of chromosomes than the current population.
	/// </remarks>
    public interface IGeneticOperator// : IOperator
    {
		/// <summary>
		/// This method should be used to perform the operation. The the 'currentPopulation' variable will be in an 
		/// unknown state following the call.
		/// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnesFunctionDelegate"></param>
		/// <returns>Population</returns>
		void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnesFunctionDelegate);

		/// <summary>
		/// This method should return the number of evaluations that were carried out, 
		/// i.e. the number of times the fitness function was called during the Invoke method.
		/// </summary>
		/// <returns></returns>
		int GetOperatorInvokedEvaluations();

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="GAF.IGeneticOperator"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:GAF.IGeneticOperator"/> reqires evaluated population.
		/// </summary>
		/// <value><c>true</c> if reqires evaluated population; otherwise, <c>false</c>.</value>
		bool RequiresEvaluatedPopulation { set; get; }
    }
}
