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
using System.Net;
using System.IO;

#if !PCL

using System;

namespace GAF.Net
{
	/// <summary>
	/// Evaluation server.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class EvaluationServer
	{
		private readonly FitnessFunction _fitnessFunction;

		/// <summary>
		/// Delegate definition for the InitialEvaluationComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void EvaluationCompleteHandler (object sender, RemoteEvaluationEventArgs e);

		/// <summary>
		/// Event definition for the InitialEvaluationComplete event handler.
		/// </summary>
		public event EvaluationCompleteHandler OnEvaluationComplete;

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.EvaluationServer"/> class.
		/// </summary>
		/// <param name="fitnessFunctionDelegate">Fitness function delegate.</param>
		public EvaluationServer (FitnessFunction fitnessFunctionDelegate)
		{
			_fitnessFunction = fitnessFunctionDelegate;
		}

		/// <summary>
		/// Start the server listening on the specified ipAddress and port.
		/// </summary>
		/// <param name="ipAddress">Ip address.</param>
		/// <param name="port">Port.</param>
		public void Start (IPAddress ipAddress, int port)
		{

			SocketListener.OnPacketReceived += listener_OnPacketReceived;
			SocketListener.StartListening (ipAddress, port);
		}

		private void listener_OnPacketReceived (object sender, PacketEventArgs e)
		{
			if (e.Packet.Header.DataLength > 0) {

				switch ((PacketId)e.Packet.Header.PacketId) {

				case PacketId.Data: {
						var chromosome = BinarySerializer.DeSerialize<Chromosome> (e.Packet.Data);
						e.Result = chromosome.Evaluate (_fitnessFunction);
						if (OnEvaluationComplete != null) {

							var eventArgs = new RemoteEvaluationEventArgs (chromosome);
							this.OnEvaluationComplete (this, eventArgs);
						}
						break;
					}
				case PacketId.Init: {
						File.WriteAllBytes ("GAF.ConsumerFunctions.Dynamic.dll", e.Packet.Data);

						break;
					}
				}
			}
		}
	}
}

#endif