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
using System.Linq;

namespace GAF.Operators
{
	/// <summary>
	/// This operator will replace the weakest solutions in the new population 
	/// with the selected amount (by percentatge) of randomly generated solutions 
	/// (Chromosomes) from the current population. Any chromosome marked as Elite
	/// will not be replaced. Therefore, 50% of a population of 100 that has 10
	/// 'Elites' will replace 45 solutions.
	/// </summary>
	public class RandomReplace : OperatorBase, IGeneticOperator
    {
        private readonly object _syncLock = new object();
        private bool _allowDuplicatesS;
        private int _evaluations;
		private int _percentageToReplace;

        /// <summary>
        /// Replaces the whole population with randomly generated solutions.
        /// </summary>
        internal RandomReplace()
        {
        }

		/// <summary>
		/// Replaces the whole population with randomly generated solutions.
		/// </summary>/// <param name="allowDuplicates">If set to <c>true</c> allow duplicates.</param>
        public RandomReplace(bool allowDuplicates)
            :this(100, allowDuplicates)
        {
        }

        /// <summary>
        /// Replaces the specified number of the weakest individuals, with randomly generated ones.
        /// </summary>
        /// <param name="percentageToReplace">Set the number to replace.</param>
        public RandomReplace(int percentageToReplace)
            :this(percentageToReplace, false)
        {
        }

        /// <summary>
        /// Replaces the specified number of the weakest individuals, with randomly generated ones.
        /// </summary>
        /// <param name="percentageToReplace">Set the number to replace.</param>
        /// <param name="allowDuplicates"></param>
        public RandomReplace(int percentageToReplace, bool allowDuplicates)
        {
            _percentageToReplace = percentageToReplace;
            _allowDuplicatesS = allowDuplicates;

        }

        /// <summary>
        /// This is the method that invokes the operator. This should not normally be called explicitly.
        /// </summary>
        /// <param name="currentPopulation"></param>
        /// <param name="newPopulation"></param>
        /// <param name="fitnessFunctionDelegate"></param>
        public override void Invoke(Population currentPopulation, ref Population newPopulation, FitnessFunction fitnessFunctionDelegate)
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

            if (currentPopulation.Solutions[0].Genes.Any(g => g.GeneType != GeneType.Binary))
            {
                throw new Exception("Only Genes with a GeneType of Binary can be handled by the RandomReplace operator.");
            }

            Replace(currentPopulation, ref newPopulation, this.Percentage, this.AllowDuplicates, fitnessFunctionDelegate);

         }

        /// <summary>
        /// Helper Method marked as Internal for Unit Testing purposes.
        /// </summary>
        /// <param name="currentPopulation"></param>
        /// <param name="newPopulation"></param>
        /// <param name="percentage"></param>
        /// <param name="allowDuplicates"></param>
        /// <param name="fitnessFunctionDelegate"></param>
        internal void Replace(Population currentPopulation, ref Population newPopulation, int percentage, bool allowDuplicates, FitnessFunction fitnessFunctionDelegate)
        {
			//copy everything accross in order of fitness with Elites at the top
			//NOTE: at this point, the Elites may not be the fittest, depending upon
			//effects of the previous operator(s)
			newPopulation.Solutions.AddRange(currentPopulation.Solutions);
			newPopulation.Solutions.Sort ((x, y) => {
				int result = y.IsElite.CompareTo (x.IsElite);
				return result != 0 ? result : y.Fitness.CompareTo (x.Fitness);
			});

            //find the number of non elites
			var nonElites = newPopulation.Solutions.Count(s => !s.IsElite);

            //determine how many we are replacing based on the percentage
			var numberToReplace = (int)System.Math.Round((nonElites / 100.0) * percentage);

            if (numberToReplace > nonElites)
            {
                numberToReplace = nonElites;
            }

            if (numberToReplace > 0)
            {
                //we are adding random imigrants to the new population
                if (newPopulation == null || newPopulation.PopulationSize < numberToReplace)
                {
                    throw new ArgumentException(
                        "The 'newPopulation' does not contain enough solutions for the current operation.");
                }

				//reduce the population as required
				var populationCount = newPopulation.Solutions.Count();
				newPopulation.Solutions.RemoveRange(populationCount - numberToReplace,numberToReplace);

				var chromosomeLength = currentPopulation.ChromosomeLength;

                //var immigrants = new List<Chromosome>();
                for (var index = 0; index < numberToReplace; index++)
                {
					if (!allowDuplicates) {

						Chromosome uniqueChromosome;

						//if the new population is empty
						uniqueChromosome = CreateUniqueChromosome (chromosomeLength, newPopulation);
						uniqueChromosome.Evaluate (fitnessFunctionDelegate);
						AddImigrant (newPopulation, uniqueChromosome, fitnessFunctionDelegate);

					}
                    else
                    {
						AddImigrant (newPopulation, new Chromosome(chromosomeLength), fitnessFunctionDelegate);
                    }
                }
            }
        }

		private void AddImigrant(Population population, Chromosome imigrant, FitnessFunction fitnessFunctionDelegate)
		{
			//need to add these to the solution, sort and then remove the weakest
			if (imigrant != null && population != null) {
				
				imigrant.Evaluate (fitnessFunctionDelegate);
				_evaluations++;

				//TODO: Consider that Random may not want to remove weakest as we are trying to increase diversity.

				//add the imigrant this extends the population
				population.Solutions.Add (imigrant);

			}
		}

		/// <summary>
		/// Creates a random unique Binary Gene. The RandomReplacement operator can only be used with Binary Genes. 
		/// </summary>
		/// <returns>The unique chromosome.</returns>
		/// <param name="chromosomeLength">Chromosome length.</param>
		/// <param name="population">Population.</param>
		internal Chromosome CreateUniqueChromosome(int chromosomeLength, Population population)
        {

			Chromosome rndChromosome = null;

			const int maxAttempts = 100; //give up after 100 attempts
			var success = false;

			if (chromosomeLength == 0)
				throw new ChromosomeNotUniqueException ("The chromosome length is set to zero. Zero length chromosomes cannot be unique.");

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                rndChromosome = new Chromosome(chromosomeLength);

				if (!population.SolutionExists(rndChromosome))
                {
					success = true;
                    break;
                }

            }
				
			if(!success)
			{
				throw new ChromosomeNotUniqueException ("It has not been possible to create a unique random Chromosome. This may be because the Chromosome length is two short or the Population is two large.");
			}

            return rndChromosome;

        }

        /// <summary>
        /// Returns the number of evaluations performed by this operator.
        /// </summary>
        /// <returns></returns>
        public override int GetOperatorInvokedEvaluations()
        {
            return _evaluations;
        }

        /// <summary>
        /// Sets/Gets the Percentage number to be replaced. The setting and getting of this property is thread safe.
        /// </summary>
        public int Percentage
        {
            get
            {
                //not really needed as 32bit int updates are atomic on 32bit systems 
                lock (_syncLock)
                {
                    return _percentageToReplace;
                }
            }

            set
            {
                lock (_syncLock)
                {
                    _percentageToReplace = value;
                }
            }
        }
			
        /// <summary>
        /// Sets/Gets whether duplicates are allowed in the population. 
        /// The setting and getting of this property is thread safe.
        /// </summary>
        public bool AllowDuplicates
        {
            get
            {
                lock (_syncLock)
                {
                    return _allowDuplicatesS;
                }
            }
            set
            {
                lock (_syncLock)
                {
                    _allowDuplicatesS = value;
                }
            }
        }
    }
}
