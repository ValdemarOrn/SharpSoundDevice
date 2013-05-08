using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// Indicates the direction of the port.
	/// </summary>
	public enum PortDirection
	{
		/// <summary>
		/// Designates the Port as an output port
		/// </summary>
		Output = 0,

		/// <summary>
		/// Designates the Port as an input port
		/// </summary>
		Input = 1
	}

	/// <summary>
	/// Indicates the type of device.
	/// 
	/// Note that DeviceType is really only needed to conform with current standards, unlike VST, 
	///	SharpSoundDevice allows for a device to do any or all of these things simultaneously.
	/// </summary>
	public enum DeviceType
	{
		/// <summary>
		/// A device, e.g. a synthesizer, that generates sound without any input signal 
		///	connected to it. (corresponds to VST function call "isSynth(true)" )
		/// </summary>
		Generator = 1,

		/// <summary>
		/// An effects device that manipulates incoming audio, processes it and streams it
		///	back out.
		/// </summary>
		Effect = 2,

		/// <summary>
		/// A Midi effect that only listens to Midi input and generates Midi output.
		///	Unsupported by VST bridge!
		/// </summary>
		Midi = 3
	}

	/// <summary>
	/// The type of event that is being signaled.
	/// </summary>
	public enum EventType
	{
		/// <summary>
		/// A parameter has changed. Can be sent from Host to device to alert the device, 
		///	but can also be sent from device to host when a parameter is edited via the device GUI.
		/// </summary>
		Parameter = 1,

		/// <summary>
		/// A midi event. Can be sent from host to device (e.g. Note/CC input) or from device to
		///	host if the device is a midi plugin.
		/// </summary>
		Midi = 2,

		/// <summary>
		/// Send if the currentprogram has changed. Can be sent from host to device to 
		///	signal the device to change presets, or from  device to host when the program is changed 
		///	via the device GUI.
		/// </summary>
		ProgramChange = 3,

		/// <summary>
		/// Alert the host that the plugin is requesting the GUI window to be resized.
		///	Needed because of compatibility with VST.
		/// </summary>
		WindowSize = 4
	}
}
