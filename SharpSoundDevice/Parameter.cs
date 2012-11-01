using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public struct Parameter
	{
		public string Name { get; set; }
		public uint Index { get; set; }

		//public double Min { get; set; }
		//public double Max { get; set; }
		public uint Steps { get; set; }

		public double Value { get; set; }
		public string Display { get; set; }
	}
}
