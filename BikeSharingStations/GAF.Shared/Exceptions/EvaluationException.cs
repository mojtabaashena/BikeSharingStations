using System;
using System.Collections.Generic;
using System.Text;

namespace GAF.Exceptions
{

    /// <summary>
    /// Chromosome exception.
    /// </summary>
    public class EvaluationException : Exception
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Exceptions.EvaluationException"/> class.
		/// </summary>
        public EvaluationException()
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Exceptions.EvaluationException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
        public EvaluationException(string message) : base(message)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Exceptions.EvaluationException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="inner">Inner.</param>
        public EvaluationException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
