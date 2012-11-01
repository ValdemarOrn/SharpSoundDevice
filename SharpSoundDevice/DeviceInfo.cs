using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	[Serializable]
	public struct DeviceInfo
	{
		public string DeviceID { get; set; }
		public int VstId { get; set; }
		public string Name { get; set; }
		public string Developer { get; set; }
		public uint Version { get; set; }
		public DeviceType Type { get; set; }
		public uint ProgramCount { get; set; }
		public bool HasEditor { get; set; }
		public int EditorWidth { get; set; }
		public int EditorHeight { get; set; }
	}
}
