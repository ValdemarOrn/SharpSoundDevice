using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	[Serializable]
	public struct DeviceInfo
	{
		/// <summary>
		/// A unique ID for the device. The format is unspecified and device makers are encouraged
		/// to pick a good, unique ID, preferably one that contains the name of the device and the
		/// name of the author/manufacturer.
		/// </summary>
		public string DeviceID { get; set; }

		/// <summary>
		/// The Virtual Studio Technology (VST) ID. See Steinberg documentation for more details.
		/// You can use DeviceUtilities.GenerateIntegerId() to create a pseudo-random ID for your 
		/// device.
		/// </summary>
		public int VstId { get; set; }

		/// <summary>
		/// The full name for the plugin.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The name of the device developer.
		/// </summary>
		public string Developer { get; set; }

		/// <summary>
		/// A version number. Optional
		/// </summary>
		public uint Version { get; set; }

		/// <summary>
		/// Indicates the type of device. See DeviceType enum.
		/// </summary>
		public DeviceType Type { get; set; }

		/// <summary>
		/// Indicates how many programs are in each program bank.
		/// </summary>
		public uint ProgramCount { get; set; }

		/// <summary>
		/// Indicates is the device has a custom editor (GUI).
		/// </summary>
		public bool HasEditor { get; set; }

		/// <summary>
		/// Indicates the width of the editor, in pixels.
		/// Needed for compatibility with VST standard, where the host is responsible for setting
		/// the correct size of the editor window
		/// </summary>
		public int EditorWidth { get; set; }

		/// <summary>
		/// Indicates the height of the editor, in pixels.
		/// Needed for compatibility with VST standard, where the host is responsible for setting
		/// the correct size of the editor window
		/// </summary>
		public int EditorHeight { get; set; }
	}
}
