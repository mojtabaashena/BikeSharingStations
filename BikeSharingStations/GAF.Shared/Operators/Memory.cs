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
	/// This class, relies on the new population being fully populated and is designed to be the last operator in the chain.
	/// </summary>
	public class Memory : OperatorBase, IGeneticOperator
	{
		private Population _memory;
		private int _evaluations;
		private int _memorySize;
		private int _generationalUpdatePeriod;
		private int _currentGeneration = 0;
		private object _synclock = new object ();

		/// <summary>
		/// This memory operator is designed to store recently identified solutions.
		/// The operator stores the fittest solution determined and stores this after
		/// a period of generations determined by the generationalUpdatePeriod parameter.
		/// Once the memory is full, older solutions are overwritten.
		/// </summary>
		/// <param name="memorySize"></param>
		/// <param name="generationalUpdatePeriod"></param>
		public Memory (int memorySize, int generationalUpdatePeriod)
		{
			if (memorySize < 0)
				throw new ArgumentOutOfRangeException ("nameof (memorySize)");

			Enabled = false;
			_memorySize = memorySize;
			_generationalUpdatePeriod = generationalUpdatePeriod;

			Enabled = true;

		}

		/// <summary>
		/// Gets or sets the size of the memory.
		/// If this property is changed when the operator is enabled and the GA is running
		/// will cause the memory to clear before being re-created at its new size.
		/// </summary>
		/// <value>The size of the memory.</value>
		public int MemorySize {
			get {
				lock (_synclock) {
					return _memorySize;
				}
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("nameof (value)");

				lock (_synclock) {
					_memorySize = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the generational update period.
		/// </summary>
		/// <value>The generational update period.</value>
		public int GenerationalUpdatePeriod {
			get {
				lock (_synclock) {
					return _generationalUpdatePeriod;
				}
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("nameof (value)");

				lock (_synclock) {
					_generationalUpdatePeriod = value;
				}
			}
		}

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly.
		/// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnessFunctionDelegate"></param>
		public override void Invoke (Population currentPopulation, ref Population newPopulation,
						   FitnessFunction fitnessFunctionDelegate)
		{
			if (currentPopulation.Solutions == null || currentPopulation.Solutions.Count == 0) {
				throw new ArgumentException ("There are no Solutions in the current Population.");
			}

			if (newPopulation == null) {
				newPopulation = currentPopulation.CreateEmptyCopy ();
			}

			CurrentPopulation = currentPopulation;
			NewPopulation = newPopulation;
			FitnessFunction = fitnessFunctionDelegate;

			if (!Enabled)
				return;

			// Copy everything accross including elites.
			newPopulation.Solutions.Clear ();
			newPopulation.Solutions.AddRange (currentPopulation.Solutions);

			if (_memory == null) {

				/*
				 * Create the memory if one doesn't exist or it is of a different size
				 * ensure that if the newPopulation has parallel evaluations set, the
				 * memory does also.
				 * 
				 * This operator is designed for use in noisy landscapes so set the memory
				 * to ReEvaluateAll as cached fitness values are of no use in these
				 * environments
				 */

				   _memory = new Population (_memorySize, 0, true, false) {
					   EvaluateInParallel = newPopulation.EvaluateInParallel
				   };

			} else {
				// trim size if necessary
				int memSize = _memory.PopulationSize;
				if (memSize > _memorySize) {
					_memory.Solutions.RemoveRange (_memorySize, memSize - _memorySize);
				}
			}

			_currentGeneration++;

			//Adding these every n generations, simply add the best to our memoryy
			if (_currentGeneration % _generationalUpdatePeriod == 0) {
				this.AddToMemory (newPopulation.GetTop (1) [0]);
			}

			if (_memory.Solutions.Count > 0) {

				/* 
				 * If what we have in memory is better than the best in the current population, 
				 * copy this to the new population to the new population. However, we need to 
				 * re-evaluate the memory first as this operator is designed for noisy landscapes,
				 * therefore any previous fitness is invalid.
				*/

				_evaluations =_memory.Evaluate (FitnessFunction); ;

				//compare best memory with population and replace worst in population if appropriate
				var memorySolution = _memory.GetTop (1) [0];
				var bestInPopulation = newPopulation.GetTop (1) [0];

				if (memorySolution.Fitness > bestInPopulation.Fitness) {
					//replace the worst with the best from memory
					newPopulation.DeleteLast ();
					newPopulation.Solutions.Add (memorySolution);
				}
			}

		}
		/// <summary>
		/// Returns the number of evaluations performed by this operator.
		/// </summary>
		/// <returns></returns>
		public override int GetOperatorInvokedEvaluations ()
		{
			return _evaluations;
		}

		private void AddToMemory (Chromosome solution)
		{
			//simply add the best to our memory
			_memory.Solutions.Add (solution);

			if (_memory.Solutions.Count > _memorySize) {
				//remove the oldest
				_memory.Solutions.RemoveAt (0);
			}
		}
	}
}
