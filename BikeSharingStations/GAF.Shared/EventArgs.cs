using System;
using System.Collections.Generic;

namespace GAF
{

	/// <summary>
	/// Event arguments used within the main GA exeption events.
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{
		private readonly string _message;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name="message"></param>
		public ExceptionEventArgs (string method, string message)
		{
			_message = message;
		}

		/// <summary>
		/// Returns the list of Exception messages.
		/// </summary>
		public string Message {
			get { return _message; }
		}
	}

	/// <summary>
	/// Event arguments used within the logging events.
	/// </summary>
	public class LoggingEventArgs : EventArgs
	{
		private readonly string _message;
		private readonly bool _isWarning;

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.LoggingEventArgs"/> class.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public LoggingEventArgs (string format, params object[] args) : this (false, format, args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.LoggingEventArgs"/> class.
		/// </summary>
		/// <param name = "isWarning"></param>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		public LoggingEventArgs (bool isWarning, string format, params object[] args)
		{
			var msg = string.Format (format, args);
			_message = msg;
			_isWarning = isWarning;
		}

		/// <summary>
		/// Returns the list of Exception messages.
		/// </summary>
		public string Message {
			get { return _message; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:GAF.LoggingEventArgs"/> is a warning.
		/// </summary>
		/// <value><c>true</c> if is warning; otherwise, <c>false</c>.</value>
		public bool IsWarning {
			get { return _isWarning; }
		}
	}

	/// <summary>
	/// Event arguments used within the main GA events.
	/// </summary>
	public class GaEventArgs : EventArgs
	{
		/// <summary>
		/// The generation.
		/// </summary>
		protected readonly int _generation;

		/// <summary>
		/// The population.
		/// </summary>
		protected readonly Population _population;

		/// <summary>
		/// The evaluations.
		/// </summary>
		protected readonly long _evaluations;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="population"></param>
		/// <param name="generation"></param>
		/// <param name="evaluations"></param>
		public GaEventArgs (Population population, int generation, long evaluations)
		{
			_generation = generation;
			_population = population;
			_evaluations = evaluations;
		}

		/// <summary>
		/// Returns the population.
		/// </summary>
		public Population Population {
			get { return _population; }
		}

		/// <summary>
		/// Returns the number of the current generation.
		/// </summary>
		public int Generation {
			get { return _generation; }
		}

		/// <summary>
		/// Returns the number of the evaluations undertaken so far.
		/// </summary>
		public long Evaluations {
			get { return _evaluations; }
		}
	}

	/// <summary>
	/// GA operator event arguments.
	/// </summary>
	public class GaOperatorEventArgs : GaEventArgs
	{
		IGeneticOperator _geneticOperator;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GAF.GaOperatorEventArgs"/> class.
		/// </summary>
		/// <param name="geneticOperator">Genetic operator.</param>
		/// <param name="population">Population.</param>
		/// <param name="generation">Generation.</param>
		/// <param name="evaluations">Evaluations.</param>
		public GaOperatorEventArgs (IGeneticOperator geneticOperator, Population population, int generation, long evaluations)
			:base(population,generation,evaluations)
		{
			_geneticOperator = geneticOperator;
		}

		/// <summary>
		/// Gets the genetic operator.
		/// </summary>
		/// <value>The genetic operator.</value>
		public IGeneticOperator GeneticOperator {
			get { return _geneticOperator; }
		}
	}

	/// <summary>
	/// Evaluation event arguments.
	/// </summary>
	public class EvaluationEventArgs : EventArgs
	{
		private readonly List<Chromosome> _solutionsToEvaluate;
		private readonly FitnessFunction _fitnessFunction;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name = "solutionsToEvaluate"></param>
		/// <param name = "fitnessFunctionDelegate"></param>
		public EvaluationEventArgs (List<Chromosome> solutionsToEvaluate, FitnessFunction fitnessFunctionDelegate)
		{
			_solutionsToEvaluate = solutionsToEvaluate;
			_fitnessFunction = fitnessFunctionDelegate;
			Evaluations = 0;
		}

		/// <summary>
		/// Returns the population.
		/// </summary>
		public List<Chromosome> SolutionsToEvaluate {
			get { return _solutionsToEvaluate; }
		}

		/// <summary>
		/// Gets the fitness function.
		/// </summary>
		/// <value>The fitness function.</value>
		public FitnessFunction FitnessFunctionDelegate {
			get { return _fitnessFunction; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to cancel further evaluations. This property
		/// can be used by consumers of the event to stop further evaluations.
		/// </summary>
		/// <value><c>true</c> to cancel; otherwise, <c>false</c>.</value>
		public bool Cancel { set; get; }

		/// <summary>
		/// Gets or sets the evaluation count. This property should be used by consumers of the event
		/// to update the evaluation count if appropriate.
		/// count
		/// </summary>
		/// <value>The numbe of evaluations undertaken eithin the event.</value>
		public int Evaluations { set; get; }

	}

}

