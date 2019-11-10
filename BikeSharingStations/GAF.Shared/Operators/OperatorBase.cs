using System;
namespace GAF.Operators
{

	/// <summary>
	/// Operator base class.
	/// </summary>
	public abstract class OperatorBase : IGeneticOperator
	{
		/// <summary>
		/// Called by derived classes when initializing.
		/// </summary>
		public  OperatorBase () 
		{
			Enabled = true;
			RequiresEvaluatedPopulation = true;
		}

		/// <summary>
		/// Enabled property. Diabling this operator will cause the population to 'pass through' unaltered.
		/// </summary>
		public bool Enabled { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:GAF.Operators.OperatorBase"/> requires evaluated population.
		/// </summary>
		/// <value><c>true</c> if requires evaluated population; otherwise, <c>false</c>.</value>
		public bool RequiresEvaluatedPopulation { get; set; }

		/// <summary>
		/// Gets or sets the new population.
		/// </summary>
		/// <value>The new population.</value>
		protected Population NewPopulation { set; get; }

		/// <summary>
		/// Gets or sets the current population.
		/// </summary>
		/// <value>The current population.</value>
		protected Population CurrentPopulation { set; get; }

		/// <summary>
		/// Gets or sets the fitness function.
		/// </summary>
		/// <value>The fitness function.</value>
		protected FitnessFunction FitnessFunction { set; get; }

		/// <summary>
		/// Event definition for the LoggingEventHandler event handler.
		/// </summary>
		public event LoggingEventHandler OnLogging;

		/// <summary>
		/// Returns the number of evaluations performed by this operator.
		/// Default behaviour returns zero, override in derived class where appropriate.
		/// </summary>
		/// <returns></returns>
		public virtual int GetOperatorInvokedEvaluations ()
		{
			return 0;
		}

		/// This is the method that invokes the operator. This should not be called explicitly as
		/// it will be called by the framework as required.
		/// This method is virtual and allows the derived class to override and extend 
		/// the functionality of the method. 
		/// <param name="currentPopulation">Current population.</param>
		/// <param name="newPopulation">New population.</param>
		/// <param name="fitnessFunctionDelegate">Fitness function delegate.</param>
		public virtual void Invoke (Population currentPopulation, ref Population newPopulation, FitnessFunction fitnessFunctionDelegate)
		{
			throw new NotImplementedException ("This method is virtual and should be implemented in a derived class.");
		}
	}
}
