using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// Indicates the direction of the port
	/// </summary>
	public enum PortDirection
	{
		Output = 0,
		Input = 1
	}

	/// <summary>
	/// Indicates the type of device.
	/// 
	/// Generator = A device, e.g. a synthesizer, that generates sound without any input signal 
	///		connected to it.
	/// Effect = an effects device that manipulates incoming audio, processes it and streams it
	///		back out.
	/// Midi = A Midi effect that only listens to Midi input and generates Midi output.
	/// 
	/// Note that DeviceType is really only needed to conform with current standards, like VST, 
	///		as SharpSoundDevice allows for a device to do any or all of these things 
	///		simultaneously.
	/// </summary>
	public enum DeviceType
	{
		Generator = 1,
		Effect = 2,
		Midi = 3
	}

	/// <summary>
	/// The type of event that is being signaled.
	/// 
	/// Parameter = A parameter has changed. Can be sent from Host to device to alert the device, 
	///		but can also be sent from device to host when a parameter is edited via the device GUI.
	///	Midi = A midi event. Can be sent from host to device (e.g. Note/CC input) or from device to
	///		host if the device is a midi plugin.
	///	ProgramChange = The program has changed. Can be sent from host to device to signal the
	///		device to change presets, or from  device to host when the program is changed via the 
	///		device GUI.
	///	WindowSize = Alert the host that the plugin is requesting the GUI window to be resized.
	///		Needed because of compatibility with VST.
	/// </summary>
	public enum EventType
	{
		Parameter = 1,
		Midi = 2,
		ProgramChange = 3,
		WindowSize = 4
	}
}
