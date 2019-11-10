using System;

namespace GAF
{
	/// <summary>
	/// This interface is provided to support external programs such as the GAF.Lab GUI application.
	/// </summary>
	[Obsolete ("Interface is deprecated, please consider using IFitness instead.")]
	public interface IConsumerFunctions
	{

		/// <summary>
		/// This method should implement the GA's evaluation function.
		/// </summary>
		/// <returns>The fitness.</returns>
		/// <param name="chromosome">Chromosome.</param>
		double EvaluateFitness(Chromosome chromosome);

		/// <summary>
		/// This method should implement the GA's terminate function.
		/// </summary>/// <returns><c>true</c>, if algorithm was terminated, <c>false</c> otherwise.</returns>
		/// <param name="population">Population.</param>
		/// <param name="currentGeneration">Current generation.</param>
		/// <param name="currentEvaluation">Current evaluation.</param>
		bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation);


	}
}

