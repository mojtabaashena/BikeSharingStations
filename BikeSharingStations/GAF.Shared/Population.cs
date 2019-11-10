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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAF.Extensions;
using GAF.Threading;
using System.Threading;
using System.Diagnostics;

namespace GAF
{
	/// <summary>
	/// This class represents a population of solutions.
	/// </summary>
	public class Population
	{
		private List<Chromosome> _chromosomes = new List<Chromosome> ();
		private bool _reEvaluateAll;
		private bool _evaluateInParallel;
		private bool _useLinearlyNormalisedFitness;
		private ParentSelectionMethod _parentSelectionMethod;
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Delegate definition for the EvaluationBeginHandler event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void EvaluationBeginHandler (object sender, EvaluationEventArgs e);

		/// <summary>
		/// Event definition for the EvaluationBeginHandler event handler.
		/// </summary>
		public event EvaluationBeginHandler OnEvaluationBegin;

		/// <summary>
		/// Event definition for the LoggingEventHandler event handler.
		/// </summary>
		public event LoggingEventHandler OnLogging;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Population"/> class.
		/// Uses Linear Normalised fitness and does not re-evaluate those 
		/// Chromosomes that have already been evaluated in previous generations.
		/// </summary>
		public Population ()
			: this (0, 0, false, true, ParentSelectionMethod.FitnessProportionateSelection)
		{
		}

		/// <summary>
		/// Initializes a new empty instance of the <see cref="GAF.Population"/> class.
		/// </summary>
		/// <param name="reEvaluateAll">If set to <c>true</c> re evaluate all.</param>
		/// <param name="useLinearlyNormalisedFitness">If set to <c>true</c> use linearly normalised fitness.</param>
		public Population (bool reEvaluateAll, bool useLinearlyNormalisedFitness)
			: this (0, 0, reEvaluateAll, useLinearlyNormalisedFitness, ParentSelectionMethod.FitnessProportionateSelection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Population"/> class.
		/// Uses Linear Normalised fitness and does not re-evaluate those 
		/// Chromosomes that have already been evaluated in previous generations.
		/// </summary>
		/// <param name="populationSize"></param>
		/// <param name="chromosomeLength"></param>
		public Population (int populationSize, int chromosomeLength)
			: this (populationSize, chromosomeLength, false, true, ParentSelectionMethod.FitnessProportionateSelection)
		{
		}

		/// <summary>
		/// Constructor for the Population object. 
		/// Uses Linear Normalised fitness.
		/// </summary>
		/// <param name="populationSize"></param>
		/// <param name="chromosomeLength"></param>
		/// <param name="reEvaluateAll"></param>
		public Population (int populationSize, int chromosomeLength, bool reEvaluateAll)
			: this (
				populationSize, chromosomeLength, reEvaluateAll, true,
				ParentSelectionMethod.FitnessProportionateSelection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Population"/> class.
		/// </summary>
		/// <param name="populationSize"></param>
		/// <param name="chromosomeLength"></param>
		/// <param name="reEvaluateAll"></param>
		/// <param name="useLinearlyNormalisedFitness"></param>
		public Population (int populationSize, int chromosomeLength, bool reEvaluateAll,
		                   bool useLinearlyNormalisedFitness)
			: this (populationSize, chromosomeLength, reEvaluateAll,
			        useLinearlyNormalisedFitness, ParentSelectionMethod.FitnessProportionateSelection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Population"/> class.
		/// </summary>
		/// <param name="populationSize">Population size.</param>
		/// <param name="chromosomeLength">Chromosome length.</param>
		/// <param name="reEvaluateAll">If set to <c>true</c> re evaluate all.</param>
		/// <param name="useLinearlyNormalisedFitness">If set to <c>true</c> use linearly normalised fitness.</param>
		/// <param name="selectionMethod">Selection method.</param>
		public Population (int populationSize, int chromosomeLength, bool reEvaluateAll,
		                   bool useLinearlyNormalisedFitness, ParentSelectionMethod selectionMethod)
			: this (populationSize, chromosomeLength, reEvaluateAll,
			        useLinearlyNormalisedFitness, selectionMethod, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Population"/> class.
		/// </summary>
		/// <param name="populationSize"></param>
		/// <param name="chromosomeLength"></param>
		/// <param name="reEvaluateAll"></param>
		/// <param name="useLinearlyNormalisedFitness"></param>
		/// <param name="selectionMethod"></param>
		/// <param name = "evaluateInParallel"></param>
		public Population (int populationSize, int chromosomeLength, bool reEvaluateAll,
		                   bool useLinearlyNormalisedFitness, ParentSelectionMethod selectionMethod, bool evaluateInParallel)
		{
			if (populationSize % 2 != 0) {
				throw new ArgumentException ("Population size must be an even number.");
			}

			//set the default values
			this.InitialisationType = InitialisationType.Random;
			_reEvaluateAll = reEvaluateAll;
			_useLinearlyNormalisedFitness = useLinearlyNormalisedFitness;
			_parentSelectionMethod = selectionMethod;
			_evaluateInParallel = evaluateInParallel;

			this.Initialise (populationSize, chromosomeLength);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns the length of the chromosomes used in the population.
		/// </summary>
		public int ChromosomeLength {
			get {

				if (!IsEmptyList<Chromosome> (Solutions) &&
				    !IsEmptyList<Gene> (Solutions [0].Genes)) {
					return Solutions [0].Genes.Count;
				}

				return  0;
			}
		}

		/// <summary>
		/// Sets/gets the current list of solutions as Chromosomes.
		/// </summary>
		public List<Chromosome> Solutions {
			get {
				return _chromosomes;
			}
			internal set {
				_chromosomes = value;
			}
		}

		/// <summary>
		/// Sets/Gets the method used to initialise the list of solutions. 
		/// The default setting is Random.
		/// </summary>
		internal InitialisationType InitialisationType { get; set; }

		/// <summary>
		/// Gets the population size.
		/// </summary>
		public int PopulationSize {
			get { return this.Solutions.Count; }
		}

		/// <summary>
		/// Returns the total fitness of the population.
		/// </summary>
		public double TotalFitness {

			get { return Solutions.Sum (c => c.Fitness); }
		}

		/// <summary>
		/// Returns the total linearly normalised fitness of the population.
		/// </summary>
		public double TotalFitnessNormalised {
			get { return Solutions.Sum (c => c.FitnessNormalised); }
		}

		/// <summary>
		/// Returns the maximum fitness value for the popluation. Returns -1 if there are no chromosomes in the population.
		/// </summary>
		/// <returns></returns>
		public double MaximumFitness {
			get {
				if (Solutions.Count > 0) {
					return Solutions.Max (c => c.Fitness);
				} else {
					return -1;
				}
			}
		}

		/// <summary>
		/// Returns the minimum fitness value for the popluation.
		/// </summary>
		/// <returns></returns>
		public double MinimumFitness {
			get { return Solutions.Min (c => c.Fitness); }
		}

		/// <summary>
		/// Returns the average (mean) fitness value for the popluation.
		/// </summary>
		public double AverageFitness {
			get { return Solutions.Average (c => c.Fitness); }
		}

		/// <summary>
		/// Returns whether the evaluation will re-evaluate all chromosomes. 
		/// If set to false only chromosomes without a fitness value will be evaluated.
		/// </summary>
		public bool ReEvaluateAll {
			get {
				lock (_syncLock) {
					return _reEvaluateAll;
				}
			}
			set { 
				lock (_syncLock) {
					_reEvaluateAll = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="GAF.Population"/> evaluates
		/// the solutions in parallel using the parallel library IF the parallel library is supported.
		/// NOTE: The PCL Version of the library does not support parallel evaluations.
		/// </summary>
		/// <value><c>true</c> if evaluate in parallel; otherwise, <c>false</c>.</value>
		public bool EvaluateInParallel {
			get {
				lock (_syncLock) {
					return _evaluateInParallel;
				}
			}
			set { 
				lock (_syncLock) {
					#if PCL //parallel not supported in PCL profile
					value = false;
					#endif
					_evaluateInParallel = value;
				}
			}
		}

		/// <summary>
		/// Returns boolean to indicate whether linear normalisasion is being used.
		/// </summary>
		public bool LinearlyNormalised {
			get {
				lock (_syncLock) {
					return _useLinearlyNormalisedFitness;
				}
			}
			set { 
				lock (_syncLock) {
					_useLinearlyNormalisedFitness = value;
				}
			}
		}

		/// <summary>
		/// Returns the Parent Selection method.
		/// </summary>
		public ParentSelectionMethod ParentSelectionMethod {

			get {
				lock (_syncLock) {
					return _parentSelectionMethod;
				}
			}
			set { 
				lock (_syncLock) {
					_parentSelectionMethod = value;
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This function evaluates all solutions in the population (see note below). The Fitness property
		/// of each solution should be updated by the fitness function. The fitness function should return a higher 
		/// value for those solutions that are deemed fitter.
		/// Note: If the GAF has been configured to not re-evaluate Elites (Constructor). Only those elites
		/// with no previous evaluation will be passed to the fitness function. 
		/// </summary>
		/// <param name="fitnessFunctionDelegate"></param>
		/// <returns></returns>
		public int Evaluate (FitnessFunction fitnessFunctionDelegate)
		{
			bool cancelled = false;
			int evaluations = 0;

			//get the list of soultions to be evaluated
			var solutionsToEvaluate = Solutions.Where (solution => ReEvaluateAll ||
			                          (!ReEvaluateAll && solution.Fitness <= 0)).ToList ();

			if (OnEvaluationBegin != null) {
				var evalArgs = new EvaluationEventArgs (solutionsToEvaluate, fitnessFunctionDelegate);
				this.OnEvaluationBegin (this, evalArgs);

				evaluations += evalArgs.Evaluations;
				cancelled = evalArgs.Cancel;
			}



			if (!cancelled) {
				
				//Parallel tasking is faster in almost all cases, however, it relies on the evaluation function
				//being able to handle multiple threads. Adding synchronisation code to the evaluation 
				//function may make this pointless and could even be slower due to the Parallel overhead.

				//Running the TSP using the GAF on a Raspberry Pi 3 (Arch Linux/Mono 4.22) showed that the 
				//.Net40 version using all four cores took 1m 10s, whilst using a single core took 1m 21s.

				//PCL does not support Parallel Task Library
#if PCL
				foreach (var solution in solutionsToEvaluate) {
					solution.Evaluate (fitnessFunctionDelegate);
					evaluations++;
				}
#else
				if (EvaluateInParallel) {

					// First type parameter is the type of the source elements
					// Second type parameter is the type of the thread-local variable (partition subtotal)
					Parallel.ForEach<Chromosome,int> (solutionsToEvaluate,
						() => 0, // method to initialize the local variable
						(solution, loop, subtotal) => {					// method invoked by the loop on each iteration
							solution.Evaluate (fitnessFunctionDelegate);
							subtotal++;
							return subtotal; // value to be passed to next iteration
						},
					// Method to be executed when each partition has completed.
					// finalResult is the final value of subtotal for a particular partition.
						(finalResult) => Interlocked.Add (ref evaluations, finalResult)
					);			


				} else {
			
					foreach (var solution in solutionsToEvaluate) {
						solution.Evaluate (fitnessFunctionDelegate);
						evaluations++;
					}
				}

#endif
			
			}
			//if any have been evaluated, update the linear normalisation
			if (evaluations > 0 && this.LinearlyNormalised) {
				ApplyLinearNormalisation ();
			}
			return evaluations;
		}


		/// <summary>
		/// Deletes the solution with the worst fitness value.
		/// </summary>
		public void DeleteLast ()
		{
			Solutions.Sort ();
			Solutions.RemoveAt (Solutions.Count - 1);
		}

		/// <summary>
		/// Returns true if the solution exists. Only valid when using binary chromosomes.
		/// </summary>
		/// <param name="chromosome"></param>
		/// <returns></returns>
		public bool SolutionExists (Chromosome chromosome)
		{
			//ToString() is overridden to ensure real numbers are considered
			if (this.Solutions.Exists (c => c.ToString () == chromosome.ToString ())) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Returns true if the chromosome ID (GUID) matches that passed in. 
		/// The comparison takes no account of the chromosome value.
		/// </summary>
		/// <param name="chromosome"></param>
		/// <returns></returns>
		public bool ChromosomeExists (Chromosome chromosome)
		{
			if (this.Solutions.Exists (c => c.Id.ToString () == chromosome.Id.ToString ())) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Returns details of population.
		/// </summary>
		/// <returns></returns>
		public new string ToString ()
		{
			return ToCsv ();
		}

		/// <summary>
		/// Returns the fitness of each solution in the population as a comma delimited string.
		/// </summary>/// <returns>The csv.</returns>
		public string ToCsv ()
		{
			var report = new StringBuilder ();
			report.Append ("Solution, Fitness \n");

			var index = 0;
			foreach (var solution in this.Solutions) {
				report.Append (index.ToString (CultureInfo.InvariantCulture));
				report.Append (",");
				report.Append (solution.Fitness.ToString (CultureInfo.InvariantCulture));
				report.Append ("\n");

				index++;
			}

			return report.ToString ();
		}

		/// <summary>
		/// Returns the fitness of each solution in the population as an array.
		/// </summary>/// <returns>The array.</returns>
		public double[] ToArray ()
		{
			var length = Solutions.Count;
			var result = new double[length];

			for (var index = 0; index < length; index++) {
				if (LinearlyNormalised) {
					result [index] = Solutions [index].FitnessNormalised;
				} else {
					result [index] = Solutions [index].Fitness;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the fitness of each solution in the population as a generic list.
		/// </summary>/// <returns>The list.</returns>
		public List<double> ToList ()
		{
			var length = Solutions.Count;
			var result = new List<double> (length);

			foreach (var solution in Solutions) {
				if (LinearlyNormalised) {
					result.Add (solution.FitnessNormalised);
				} else {
					result.Add (solution.Fitness);
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the top n percent of the population based on highest
		/// fitness value. This method returns a deep copy of the Solutions 
		/// and always returns an even number of solutions.
		/// </summary>
		/// <param name="percent"></param>
		/// <returns></returns>
		public List<Chromosome> GetTopPercent (int percent)
		{
			var chromosomeCount = (int)System.Math.Round ((Solutions.Count / 100.0) * percent);
			Solutions.Sort ();

			return GetTop (chromosomeCount);
		}

		/// <summary>
		/// Gets the top percent from a unique (based on Fitness) set of Solutions.
		/// </summary>
		/// <returns>The top unique percent.</returns>
		/// <param name="percent">Percent.</param>
		public List<Chromosome> GetTopUniquePercent (int percent)
		{
			var chromosomeCount = (int)System.Math.Round ((Solutions.Count / 100.0) * percent);
			Solutions.Sort ();

			return GetUniqueTop (chromosomeCount);
		}

		/// <summary>
		/// Returns true if the population is valid.
		/// </summary>
		/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
		public bool IsValid ()
		{
			bool result = true;

			foreach (Chromosome chromosome in Solutions) {

				var q1 = from b in this.Solutions
				         where b.Id.ToString () == chromosome.Id.ToString ()
				         select b;

				if (q1.Count () - 1 > 0) {
					result = false;
				}

			}

			return result;
		}

		/// <summary>
		/// Gets the number of duplicate solutions based on binary string representation.
		/// </summary>
		/// <returns>The duplicate count.</returns>
		public int GetDuplicateCount ()
		{
			//get the number of uniques and take that from the population size
			//to get the number of duplicates
			var uniqueSolutions = this.Solutions.Distinct (new ChromosomeComparer ()).ToList ();
			var count = this.PopulationSize - uniqueSolutions.Count ();

			return count;
		}

		/// <summary>
		/// Gets the number of duplicate solutions based on binary string representation.
		/// </summary>
		/// <returns>The duplicate count.</returns>
		[Obsolete("The method is obsolete, use GetDuplicateCount() instead.", false)]
		public int GetDuplicates ()
		{
			return GetDuplicateCount();
		}

		/// <summary>
		/// Returns the non-elites.        
		/// </summary>
		/// <returns>The non elites.</returns>
		public List<Chromosome> GetNonElites ()
		{
			return Solutions.Where (c => !c.IsElite).ToList ();
		}

		/// <summary>
		/// Returns the elites.
		/// </summary>
		/// <returns>The elites.</returns>
		public List<Chromosome> GetElites ()
		{
			return Solutions.Where (c => c.IsElite).ToList ();
		}

		/// <summary>
		/// Returns the top count of the population based on highest
		/// fitness value.        
		/// </summary>
		/// <param name="count"></param>
		/// <returns>The top.</returns>
		public List<Chromosome> GetTop (int count)
		{
			if (Solutions.Count < count)
				throw new PopulationException (
					"The population is smaller than the number of chromosomes requested.");

			var top = new Chromosome[count];
			Solutions.Sort ();
			//Solutions.Reverse ();

			Solutions.CopyTo (0, top, 0, count);
			return top.ToList ();
		}

		/// <summary>
		/// Returns a unique list representing the top unique count of 
		/// the population based on highest
		/// fitness value.        
		/// </summary>
		/// <param name="count"></param>
		/// <returns>The top.</returns>
		public List<Chromosome> GetUniqueTop (int count)
		{
			if (Solutions.Count < count)
				throw new PopulationException (
					"The unique population is smaller than the number of chromosomes requested.");

			var top = new Chromosome [count];
			var uniqueSolutions = this.Solutions.Distinct (new ChromosomeComparer ()).ToList();
			Solutions.Sort ();

			uniqueSolutions.CopyTo (0, top, 0, count);
			return top.ToList ();
		}

		/// <summary>
		/// Returns the top count of the population based on lowest
		/// fitness value.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public List<Chromosome> GetBottom (int count)
		{
			Solutions.Sort ();
			Solutions.Reverse ();

			var bottom = new Chromosome[count];
			Solutions.CopyTo (0, bottom, 0, count);

			return bottom.ToList ();
		}

		/// <summary>
		/// Method to select parents from the population based on the Parent Selection Method setting.
		/// </summary>
		/// <returns></returns>
		public List<Chromosome> SelectParents ()
		{
			//apply a Fisher-Yates shuffle
			Solutions.ShuffleFast (RandomProvider.GetThreadRandom ());

			switch (ParentSelectionMethod) {
			case ParentSelectionMethod.FitnessProportionateSelection:
				{
					return GetFpsSelection ();
				}
			case ParentSelectionMethod.StochasticUniversalSampling:
				{
					return GetSusSelection ();
				}
			case ParentSelectionMethod.TournamentSelection:
				{
					return GetTournamentSelection ();
				}
			default:
				{
					return GetFpsSelection ();
				}
			}
		}

		/// <summary>
		/// Returns the empty copy of the population object based on the current instance.
		/// </summary>
		/// <returns>The empty copy.</returns>
		public Population CreateEmptyCopy ()
		{
			
			var newPopulation = new Population (0,
				                    0,
				                    this.ReEvaluateAll,
				                    this.LinearlyNormalised,
				                    this.ParentSelectionMethod,
				                    this.EvaluateInParallel);

			//copy any event subscribers over 
			newPopulation.OnEvaluationBegin = this.OnEvaluationBegin;
			newPopulation.OnLogging = this.OnLogging;

			return newPopulation;
		}

		#endregion

		#region Private Methods

		internal List<Chromosome> GetTournamentSelection ()
		{
			const int parentCount = 2;
			const int maxIterations = 16;

			var parents = new List<Chromosome> ();
			var tour = new List<Chromosome> ();

			Chromosome selected;

			var populationSize = PopulationSize;

			for (var iteration = 0; iteration < parentCount; iteration++) {

				do {

					//determine the size of the tour
					var tourSize = RandomProvider.GetThreadRandom ().Next (populationSize);

					//get the tour
					tour = GetSusSelection (tourSize + 1);

					//select the best of the tour
					tour.Sort ();
					selected = tour.First ();

					iteration++;

					if (iteration < maxIterations)
					{
						if (!parents.Exists (c => selected.Id == c.Id)) {
							parents.Add (selected);
						}
					}
					else
					{
						//can't get unique so just add any way
						//TODO: Make this more adaptive. As the GA converges it becomes harder to ensure unique parents.
						parents.Add (selected);

						if(this.OnLogging != null)
						{
							this .OnLogging(this,new LoggingEventArgs(true, "Tournament Selection has failed to find two parents after {0} iterations. This could be due to a small or converged population.", maxIterations));
						}
					}


				} while(parents.Count < parentCount);
			}
			return parents;
		}

		private List<Chromosome> GetSusSelection ()
		{
			const int parentCount = 2;
			return GetSusSelection (parentCount);
		}

		internal List<Chromosome> GetSusSelection (int numberToSelect)
		{

			//Stochastic universal sampling

			//Baker,J., E. (1987). Reducing Bias and Inefficiency in the Selection Algorithm 
			//Proceedings of the Second International Conference on Genetic Algorithms and their Application 
			//Pages 14-21.

			//PSEUDOCODE

			// fitness proportionate selection (Roulette)
			//    SUS(Population, N)
			//    F := total fitness of population
			//    N := number of offspring to keep
			//    P := distance between the pointers (F/N)
			//    Start := random number between 0 and P
			//    Pointers := [Start + i*P | i in [0..N-1]]
			//    return RouletteWheelSelection(Population,Pointers)

			//    RouletteWheelSelection(Population, Points)
			//    Keep = []
			//    i := 0
			//    for P in Points
			//        while fitness of Population[i] < P
			//            i++
			//        add Population[i] to Keep
			//    return Keep

			var totalFitness = LinearlyNormalised
                ? this.TotalFitnessNormalised
                : this.TotalFitness;

			//get the distance between pointers
			var pointDistance = totalFitness / numberToSelect;
			var startingPoint = RandomProvider.GetThreadRandom ().NextDouble () * pointDistance;

			var pointers = new List<double> ();

			//create a list of pointers representing the number of parents to select (i.e. 2)
			for (var i = 0; i < numberToSelect; i++) {
				pointers.Add (startingPoint + (i * pointDistance));
			}

			var parents = new List<Chromosome> ();

			//now the roulette part
			var index = 0;
			var fitness = 0.0;
			foreach (var point in pointers) {

				//select the individuals
				bool selected = false;
				while (!selected) {
					//handle Nornalised fitness
					fitness = fitness + (LinearlyNormalised
                        ? Solutions [index].FitnessNormalised
                        : Solutions [index].Fitness);
					if (fitness < point) {
						index++;
					} else {
						selected = true;
					}
				}

				parents.Add (Solutions [index]);

			}

			return parents;

		}

		/// <summary>
		/// Returns a Chromosome based on the (Roulette) selection method.
		/// </summary>
		/// <returns></returns>
		internal List<Chromosome> GetFpsSelection ()
		{

			const int parentCount = 2;
			var result = new List<Chromosome> ();

			Chromosome parent = null;

			// Roulette Wheel
			//(Davis, 1992, P.14-15

			// sum the fitness of all the population (TotalFitnes)
			// generate n (random between 0 and total fitness 
			// return the first population member whose fitness 
			// added to the fitness of the precedding population 
			// members is greater or equal to n.

			var totalFitness = LinearlyNormalised
                ? this.TotalFitnessNormalised
                : this.TotalFitness;


			for (var iteration = 0; iteration < parentCount; iteration++) {

				var rndNum = RandomProvider.GetThreadRandom ().NextDouble () * totalFitness;
				var runningTotal = 0.0;

				foreach (var chromosome in Solutions) {
					//handle Nornalised fitness
					runningTotal = runningTotal + (LinearlyNormalised
                        ? chromosome.FitnessNormalised
                        : chromosome.Fitness);

					if (runningTotal >= rndNum) {
						parent = chromosome;
						break;
					}
				}

				result.Add (parent);

			}

			return result;
		}


		private void Initialise (int populationSize, int chromosomeLength)
		{
			if (populationSize <= 0 || chromosomeLength == 0)
				return;


			bool complete = false;
			//var failures = 0;

			while (!complete) {
				//var chr = new Chromosome(_chromosomeLength);

				var solution = new Chromosome (chromosomeLength);

				switch (InitialisationType) {
				case InitialisationType.Random:
					{
						this.Solutions.Add (solution);
						break;
					}
				case InitialisationType.RandomUnique:
					{
						if (!SolutionExists (solution)) {
							this.Solutions.Add (solution);
						} else {
							throw new PopulationException (
								"It has not been possible to create a unique random Chromosome. This may be because the Chromosome length is two short or the Population is two large.");
						}

						break;
					}
				default:
					{
						this.Solutions.Add (solution);
						break;
					}
				}

				if (this.Solutions.Count == populationSize) {
					complete = true;
				}
			}

		}

		/// <summary>
		/// Applies Linear Normalisation to the fitness values of the population.
		/// </summary>
		private void ApplyLinearNormalisation ()
		{
			//at this point we can apply some linear normalisation
			//put them in order
			Solutions.Sort ();

			//set the current value to the max value
			var maxValue = Solutions.Count;
			double currentValue = maxValue;
			foreach (var chromosome in Solutions) {
				chromosome.FitnessNormalised = currentValue;
				currentValue--;
			}
		}

		private bool IsEmptyList<T> (List<T> list)
		{
			if (list == null) {
				return true; // or throw an exception
			} else {
				return !list.Any ();			
			}
		}

		#endregion


	}
		
}
