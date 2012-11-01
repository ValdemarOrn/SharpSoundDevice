using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public struct Port
	{
		public string Name { get; set; }
		public PortDirection Direction { get; set; }
		public uint NumberOfChannels { get; set; }
	}
}
