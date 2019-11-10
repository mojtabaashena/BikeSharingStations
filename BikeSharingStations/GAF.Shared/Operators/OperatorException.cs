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
using System.Runtime.Serialization;
using System.Text;

namespace GAF.Operators
{
    /*
     * 1. Throw exception with out message
     * throw new CustomException() 
     * 
     * 2. Throw exception with simple message
     * throw new CustomException(message)
     * 
     * 3. Throw exception with message format and parameters
     * throw new CustomException("Exception with parameter value '{0}'", param) 
     * 
     * 4. Throw exception with simple message and inner exception
     * throw new CustomException(message, innerException) 
     * 
     * 5. Throw exception with message format and inner exception. Note that, the variable length params are always floating.
     * throw new CustomException("Exception with parameter value '{0}'", innerException, param)

     */
    /// <summary>
    /// Custom exception used to indicate an exception with an operator. See the inner exception and message for full exception details.
    /// </summary>
//    [Serializable]
    public class OperatorException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public OperatorException()
            : base()
        {
        }

        /// <summary>
        /// Constructor accepting a message.
        /// </summary>
        /// <param name="message"></param>
        public OperatorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor accepting a formatted message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public OperatorException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        /// <summary>
        /// Constructor accepting a message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public OperatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor accepting a formatted message and inner exception.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="innerException"></param>
        /// <param name="args"></param>
        public OperatorException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }


    }
}
