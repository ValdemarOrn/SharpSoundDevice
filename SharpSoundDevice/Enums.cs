using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/*public enum DataType
	{
		None = 1,
		Integer = 2,
		Double = 3,
		String = 4,
		Bytes = 5
	}*/

	public enum PortDirection
	{
		Output = 0,
		Input = 1
	}

	public enum DeviceType
	{
		Generator = 1,
		Effect = 2,
		Midi = 3
	}

	public enum EventType
	{
		Parameter = 1,
		Midi = 2,
		ProgramChange = 3,
		WindowSize = 4
	}
}
