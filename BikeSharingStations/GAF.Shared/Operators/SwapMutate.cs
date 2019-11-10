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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAF.Threading;

namespace GAF.Operators
{
	/// <summary>
	/// Th Swap Mutate operator, when enabled, traverses each gene in the population and, 
	/// based on the probability swaps one gene in the chromosome with another. 
	/// The aim of this operator is to provide mutation without changing any gene values.
	/// </summary>
	public class SwapMutate : MutateBase, IGeneticOperator
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mutationProbability"></param>
		public SwapMutate (double mutationProbability) : base(mutationProbability)
		{
			RequiresEvaluatedPopulation = false;
		}

		/// <summary>
		/// This method is virtual and allows the consumer to override and extend 
		/// the functionality of the operator to be extended within a derived class.
		/// </summary>
		/// <param name="child"></param>
		protected override void Mutate (Chromosome child)
		{
			//check probability by generating a random number between zero and one and if 
			//this number is less than or equal to the given mutation probability 
			//e.g. 0.001 then the bit value is changed.
			var rd = RandomProvider.GetThreadRandom ().NextDouble ();
			if (rd <= MutationProbability) {

				var points = GetSwapPoints (child);
				Mutate (child, points [0], points [1]);
			}
		}

		/// <summary>
		/// Exposed for Unit Testing.
		/// </summary>
		/// <param name="chromosome">Genes.</param>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		internal void Mutate (Chromosome chromosome, int first, int second)
		{
			//we ignore the base implementation as we are not implementing gene by gene mutation
			var temp = chromosome.Genes [first];
			chromosome.Genes [first] = chromosome.Genes [second];
			chromosome.Genes [second] = temp;

			//we are changing a chromosome so any existing fitness is now inappropriate
			//this has to be done in this class as we are not using the base implementation
			chromosome.Fitness = 0;
			chromosome.FitnessNormalised = 0;

		}

		/// <summary>
		/// Mutates the gene.
		/// </summary>
		/// <param name="gene">Gene.</param>
		protected override void MutateGene (Gene gene)
		{
			throw new NotImplementedException ("This operator does not mutate individual genes.");
		}

		/// <summary>
		/// Gets the swap points.
		/// </summary>
		/// <returns>The swap points.</returns>
		/// <param name="chromosome">Gene.</param>
		internal List<int> GetSwapPoints (Chromosome chromosome)
		{
			var result = new List<int> ();

			var first = RandomProvider.GetThreadRandom ().Next (chromosome.Genes.Count);

			var second = 0;
			while (first == second || second == 0) {
				second = RandomProvider.GetThreadRandom ().Next (chromosome.Genes.Count);
			}

			result.Add (first);
			result.Add (second);
			return result;

		}

	}
}
