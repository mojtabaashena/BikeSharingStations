using System;

namespace GAF.Operators
{
	/// <summary>
	/// Event arguments for the Crossover events.
	/// </summary>
    /// 
   	public class CrossoverEventArgs : EventArgs
	{
		private readonly CrossoverData _crossoverResult;
		private readonly Chromosome _parent1;
		private readonly Chromosome _parent2;
		private readonly Chromosome _child1;
		private readonly Chromosome _child2;


        /// <summary>
        /// Initializes a new instance of the <see cref="GAF.Operators.CrossoverEventArgs"/> class.
        /// </summary>
        /// <param name="crossoverResult">Crossover result.</param>
        /// <param name="parent1">Parent1.</param>
        /// <param name="parent2">Parent2.</param>
        /// <param name="child1">Child1.</param>
        /// <param name="child2">Child2.</param>
		public CrossoverEventArgs (CrossoverData crossoverResult, Chromosome parent1, Chromosome parent2, Chromosome child1, Chromosome child2 )
		{
			_crossoverResult = crossoverResult;
			_parent1 = parent1;
			_parent2 = parent2;
			_child1 = child1;
			_child2 = child2;
		}

		/// <summary>
		/// Returns the crossover result.
		/// </summary>
		public CrossoverData CrossoverData {
			get { return _crossoverResult; }
		}
		/// <summary>
		/// Gets parent 1.
		/// </summary>
		/// <value>The first parent.</value>
		public Chromosome Parent1 {
			get { return _parent1; }
		}
		/// <summary>
		/// Gets parent 2.
		/// </summary>
		/// <value>The second parent.</value>
		public Chromosome Parent2 {
			get { return _parent2; }
		}
		/// <summary>
		/// Gets child 1.
		/// </summary>
		/// <value>The first child.</value>
		public Chromosome Child1 {
			get { return _child1; }
		}
		/// <summary>
		/// Gets child 2.
		/// </summary>
		/// <value>The second child.</value>
		public Chromosome Child2 {
			get { return _child2; }
		}

	}
}

