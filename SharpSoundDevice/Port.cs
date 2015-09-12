using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A class containing port information.
	/// </summary>
	[Serializable]
	public struct Port
	{
		/// <summary>
		/// The name of the port.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The direction of the port (input/output).
		/// </summary>
		public PortDirection Direction { get; set; }

		/// <summary>
		/// The number of channels the port has (1 = mono port, 2 = stereo port, etc...)
		/// Can range from 1 to 9.
		/// </summary>
		public uint NumberOfChannels { get; set; }
	}
}
