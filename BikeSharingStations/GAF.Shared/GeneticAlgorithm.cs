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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GAF
{

	/// <summary>
	/// Delegate definition for the Terminate function.
	/// </summary>
	/// <param name="population"></param>
	/// <param name="currentGeneration"></param>
	/// <param name="currentEvaluation"></param>
	/// <returns></returns>
	public delegate bool TerminateFunction (Population population, int currentGeneration, long currentEvaluation);

	/// <summary>
	/// Main Generic Algorithm controller class.
	/// </summary>
	public class GeneticAlgorithm
	{
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Delegate definition for the InitialEvaluationComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void InitialEvaluationCompleteHandler (object sender, GaEventArgs e);

		/// <summary>
		/// Event definition for the InitialEvaluationComplete event handler.
		/// </summary>
		public event InitialEvaluationCompleteHandler OnInitialEvaluationComplete;

		/// <summary>
		/// Delegate definition for the GenerationComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void GenerationCompleteHandler (object sender, GaEventArgs e);

		/// <summary>
		/// Event definition for the GenerationComplete event handler.
		/// </summary>
		public event GenerationCompleteHandler OnGenerationComplete;

		/// <summary>
		/// Delegate definition for the OnOperatorComplete event handler.
		/// </summary>
		public delegate void OperatorCompleteHandler (object sender, GaOperatorEventArgs e);

		/// <summary>
		/// Event definition for the OnOperatorComplete event handler.
		/// </summary>
		public event OperatorCompleteHandler OnOperatorComplete;

		/// <summary>
		/// Delegate definition for the RunException event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void RunExceptionHandler (object sender, ExceptionEventArgs e);

		/// <summary>
		/// Event definition for the RunException event handler.
		/// </summary>
		public event RunExceptionHandler OnRunException;

		/// <summary>
		/// Delegate definition for the RunComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void RunCompleteHandler (object sender, GaEventArgs e);

		/// <summary>
		/// Event definition for the RunComplete event handler.
		/// </summary>
		public event RunCompleteHandler OnRunComplete;

		/// <summary>
		/// Event definition for the LoggingEventHandler event handler.
		/// </summary>
		public event LoggingEventHandler OnLogging;

		private CancellationTokenSource _tokenSource = new CancellationTokenSource ();
		private Task _task;
		private Population _population;
		private int _currentGeneration;

		private long _evals;
		private long _evalsRemoved;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.GeneticAlgorithm"/> class.
		/// </summary>
		public GeneticAlgorithm ()
		{
			this.Operators = new List<IGeneticOperator> ();

		}

		/// <summary>
		/// Constuctor, requires a configured Population object.
		/// </summary>/// <param name="population">Population.</param>
		/// <param name="fitnessFunctionDelegate">Fitness function delegate.</param>
		public GeneticAlgorithm (Population population, FitnessFunction fitnessFunctionDelegate)
		{
			_population = population;

			FitnessFunction = fitnessFunctionDelegate;

			this.Operators = new List<IGeneticOperator> ();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Main method for executing the GA. The GA runs until the number of evaluations 
		/// have reached the value specified in the maxEvaluations parameter.
		/// This method runs syncronously.
		/// </summary>
		/// <param name="maxEvaluations">Max evaluations.</param>
		public void Run (long maxEvaluations)
		{
			MainTask (maxEvaluations, null, CancellationToken.None);
		}

		/// <summary>
		/// Main method for executing the GA. The GA runs until the
		/// Terminate function returns true. 
		/// This method runs syncronously.
		/// </summary>
		/// <param name="terminateFunction">Terminate function.</param>
		public void Run (TerminateFunction terminateFunction)
		{
			MainTask (long.MaxValue, terminateFunction, CancellationToken.None);
		}

		/// <summary>
		/// Set to true to force the GA to stop running. The GA will halt in a clean fashion.
		/// This method is typically used to control the RunAsync methods.
		/// </summary>
		public void Halt ()
		{
			_tokenSource.Cancel ();
		}

		/// <summary>
		/// Resume this GA from a paused state.
		/// </summary>
		public void Resume ()
		{
			if (IsRunning && IsPaused) {
				((ManualResetEvent)_tokenSource.Token.WaitHandle).Set ();
				IsPaused = false;
			}
		}

		/// <summary>
		/// Pauses the GA if it is running.
		/// </summary>
		public void Pause ()
		{
			if (IsRunning && !IsPaused) {
				((ManualResetEvent)_tokenSource.Token.WaitHandle).Reset ();
				IsPaused = true;
			}
		}

		/// <summary>
		/// Runs the algorithn for the specified number of generations.
		/// </summary>
		/// <param name="maxEvaluations"></param>
		public void RunAsync (int maxEvaluations)
		{
			RunAsync (maxEvaluations, null);
		}



		/// <summary>
		/// Runs the algorithm the specified number of times. Each run executes until the specified delegate function returns true.
		/// </summary>
		/// <param name="terminateFunction"></param>
		public void RunAsync (TerminateFunction terminateFunction)
		{
			RunAsync (long.MaxValue, terminateFunction);
		}

		private void RunAsync (long maxEvaluations, TerminateFunction terminateFunction)
		{
			var token = _tokenSource.Token;
			((ManualResetEvent)token.WaitHandle).Set ();

			_task = Task.Factory.StartNew (() => MainTask (maxEvaluations, terminateFunction, token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			_task.ContinueWith (t => {
				/* error handling */
				var exception = t.Exception;
				IsRunning = false;

				if (this.OnRunException != null && t.Exception != null) {
					var message = new StringBuilder ();
					foreach (var ex in t.Exception.InnerExceptions) {
						message.Append (ex.Message);
						message.Append ("\r\n");
					}

					var eventArgs = new ExceptionEventArgs ("RunAsync", message.ToString ());
					this.OnRunException (this, eventArgs);
				}

			}, TaskContinuationOptions.OnlyOnFaulted);

		}

		/// <summary>
		/// Main run routine of genetic algorithm.
		/// </summary>
		private void MainTask (long maxEvaluations, TerminateFunction terminateFunction, CancellationToken token)
		{
			try {

				IsRunning = true;

				TerminateFunction = terminateFunction;
				//validate the population
				if (this.Population == null || this.Population.Solutions.Count == 0) {
					throw new NullReferenceException (
						"Either the Population is null, or there are no solutions within the population.");
				}

				//perform the initial evaluation
				Evaluations += _population.Evaluate (FitnessFunction);

				//raise the Initial Evaluation Complete event
				if (this.OnInitialEvaluationComplete != null) {
					var eventArgs = new GaEventArgs (_population, 0, Evaluations);
					this.OnInitialEvaluationComplete (this, eventArgs);
				}

				//main run loop for GA
				for (int generation = 0; Evaluations < maxEvaluations; generation++) {

					//Note: Selection handled by the operator(s)
					_currentGeneration = generation;

					var newPopulation = RunGeneration (_currentGeneration, _population, FitnessFunction);

					_population.Solutions.Clear ();
					_population.Solutions.AddRange (newPopulation.Solutions);

					//raise the Generation Complete event
					if (this.OnGenerationComplete != null) {
						var eventArgs = new GaEventArgs (_population, generation + 1, Evaluations);
						this.OnGenerationComplete (this, eventArgs);
					}

					if (TerminateFunction != null) {
						if (TerminateFunction.Invoke (_population, generation + 1, Evaluations)) {
							break;
						}
					}

					//if running synchronously, this never gets set
					if (token.CanBeCanceled) {
						//running asynchronously so check for a 'Pause' by monitoring the wait handle.
						token.WaitHandle.WaitOne ();
					}

					if (token.IsCancellationRequested) {
						break;
					}

				}

				IsRunning = false;
				IsPaused = false;

				//raise the Run Complete event
				if (this.OnRunComplete != null) {
					var eventArgs = new GaEventArgs (_population, _currentGeneration + 1, Evaluations);
					this.OnRunComplete (this, eventArgs);
				}
			} catch (Exception ex) {

				// re-throw for now
				throw ex;
			}
		}

		internal Population RunGeneration (int currentGeneration, Population currentPopulation, FitnessFunction fitnessFunctionDelegate)
		{

			//create new empty populations for processing 
			var tempPopulation = currentPopulation.CreateEmptyCopy ();
			var processedPopulation = tempPopulation.CreateEmptyCopy ();

			tempPopulation.Solutions.AddRange (currentPopulation.Solutions);

			//clear the current population, this keeps it simple as the solutions could
			//easily get corrupted by genetic operators as from v 2.04 on, the genes are no longer cloned
			currentPopulation.Solutions.Clear ();

			//invoke the operators
			var enabledOperators = this.Operators.Where ((o) => o.Enabled).ToList ();
			var operatorCount = enabledOperators.Count ();

			for (int op = 0; op < operatorCount; op++) {

				var enabled = true;

				var genOp = enabledOperators [op] as IGeneticOperator;
				if (genOp != null) {
					enabled = genOp.Enabled;
				} else {
					throw new NullReferenceException ("A genetic operator was null.");
				}

				//note that each internal operator will adhere to the enabled flag itself, 
				//however the check is made here as this cannot be guaranteed with third party
				//operators.
				if (enabled) {

					genOp.Invoke (tempPopulation, ref processedPopulation, fitnessFunctionDelegate);

					Evaluations += genOp.GetOperatorInvokedEvaluations ();

					tempPopulation.Solutions.Clear ();
					tempPopulation.Solutions.AddRange (processedPopulation.Solutions);
					processedPopulation.Solutions.Clear ();

					// if the next operator in the collection does not require an evaluated population
					// then no need to evaluate
					if (op + 1 < operatorCount) {

						// we are not the last operator, there is another operator to follow
						// so check to see if an evaluation is required
						if (enabledOperators [op + 1].RequiresEvaluatedPopulation) {
							Evaluations += tempPopulation.Evaluate (fitnessFunctionDelegate);
						} else {
							EvaluationsRemoved++;
						}
					} else {
						Evaluations += tempPopulation.Evaluate (fitnessFunctionDelegate);
					}

					//TODO: Add appropriate Logging
					//if (this.OnLogging != null) {
					//	var eventArgs = new LoggingEventArgs ("Generation: {0}, Evaluations: {1}", currentGeneration + 1, Evaluations);
					//	this.OnLogging (this, eventArgs);
					//}

					//raise the Generation Complete event
					if (this.OnOperatorComplete != null) {
						var eventArgs = new GaOperatorEventArgs (genOp, tempPopulation, currentGeneration + 1, Evaluations);
						this.OnOperatorComplete (this, eventArgs);
					}
				}

			}

			return tempPopulation;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Sets/Gets a list of the Operators that are applied.
		/// </summary>
		public List<IGeneticOperator> Operators { set; get; }

		/// <summary>
		/// Thread safe private property
		/// </summary>
		/// <value>The evaluations.</value>
		private long Evaluations {
			set {
				lock (_syncLock) {
					_evals = value;
				}
			}
			get {
				lock (_syncLock) {
					return _evals;
				}
			}
		}

		private long EvaluationsRemoved {
			set {
				lock (_syncLock) {
					_evalsRemoved = value;
				}
			}
			get {
				lock (_syncLock) {
					return _evalsRemoved;
				}
			}
		}
		/// <summary>
		/// Returns the current population.
		/// </summary>
		public Population Population {
			get { return _population; }
		}

		/// <summary>
		/// Gets the running state of the GA.
		/// </summary>
		public bool IsRunning { private set; get; }

		/// <summary>
		/// Gets the paused state of the GA.
		/// </summary>
		public bool IsPaused { private set; get; }

		/// <summary>
		/// Gets the fitness function.
		/// </summary>
		/// <value>The fitness function.</value>
		public FitnessFunction FitnessFunction { set; get; }

		/// <summary>
		/// Gets or sets the terminate function.
		/// </summary>
		/// <value>The terminate function.</value>
		public TerminateFunction TerminateFunction { set; get; }

		#endregion

		#region Private Methods

		#endregion

	}

}
