using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A class containing port info.
	/// </summary>
	public struct Port
	{
		/// <summary>
		/// The name of the port.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The direction of the port (input/output). See PortDirection.
		/// </summary>
		public PortDirection Direction { get; set; }

		/// <summary>
		/// The number of channels the port has (1 = mono port, 2 = stereo port, etc...)
		/// </summary>
		public uint NumberOfChannels { get; set; }
	}
}
