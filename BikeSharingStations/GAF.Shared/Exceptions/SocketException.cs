using System;

namespace GAF.Exceptions
{
	/// <summary>
	/// Custom exception used to indicate an exception with a socket. See the inner exception and message for full exception details.
	/// </summary>
	public class SocketException : Exception
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public SocketException()
			: base()
		{
		}

		/// <summary>
		/// Constructor accepting a message.
		/// </summary>
		/// <param name="message"></param>
		public SocketException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Constructor accepting a formatted message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public SocketException(string format, params object[] args)
			: base(string.Format(format, args))
		{
		}

		/// <summary>
		/// Constructor accepting a message and inner exception.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public SocketException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Constructor accepting a formatted message and inner exception.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="innerException"></param>
		/// <param name="args"></param>
		public SocketException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException)
		{
		}

	}
}

