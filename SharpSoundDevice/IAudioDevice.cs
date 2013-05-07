using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// The public interface that is implemented by an audio device.
	/// A host or plugin bridge will use this interface to communicate with the plugin.
	/// 
	/// IMPORTANT: All classes implementing this interface MUST implement a no-argument constructor!
	/// </summary>
	public interface IAudioDevice
	{
		/// <summary>
		/// Called when a new device instance is created.
		/// </summary>
		void InitializeDevice();

		/// <summary>
		/// Called when the device is being disposed of (removed from the host).
		/// </summary>
		void DisposeDevice();

		/// <summary>
		/// Called before processing starts.
		/// </summary>
		void Start();

		/// <summary>
		/// Called after processing stops.
		/// </summary>
		void Stop();

		/// <summary>
		/// Struct containing information about the current device. See DeviceInfo.
		/// </summary>
		DeviceInfo DeviceInfo { get; }

		/// <summary>
		/// An array of parameters implemented by the device.
		/// </summary>
		Parameter[] ParameterInfo { get; }

		/// <summary>
		/// An array of ports (audio inputs and outputs) implemented by the device.
		/// </summary>
		Port[] PortInfo { get; }

		/// <summary>
		/// Gives the index of the currently selected program.
		/// </summary>
		int CurrentProgram { get; }

		/// <summary>
		/// Method called by the host to send events to the device.
		/// </summary>
		/// <param name="ev">The event to pass into the device</param>
		void SendEvent(Event ev);

		/// <summary>
		/// The main processing loop
		/// </summary>
		/// <param name="input">
		/// input signal. 
		/// Array of arrays, size double[numberOfInputs][bufferSize]
		/// </param>
		/// <param name="output">
		/// Output signal. 
		/// Array of arrays, size double[numberOfOutputs][bufferSize]
		/// </param>
		/// <param name="bufferSize">the number of samples in the buffers</param>
		void ProcessSample(double[][] input, double[][] output, uint bufferSize);

		/// <summary>
		/// Called by the host when the device editor should be displayed.
		/// </summary>
		/// <param name="parentWindow">
		/// IntPtr indicating the parent window to dock the editor. Used because of VST 
		/// compatibility.
		/// 
		/// You can use Interop.DockWpfWindow() to dock a WPF window inside the parent window
		/// or Interop.DockWinFormsPanel() to dock a WinForms panel inside the parent window.
		/// </param>
		void OpenEditor(IntPtr parentWindow);

		/// <summary>
		/// Called by the host to close the device editor.
		/// </summary>
		void CloseEditor();

		/// <summary>
		/// Called by the host to set program or bank data
		/// </summary>
		/// <param name="program">The program to load</param>
		/// <param name="index">The index indicating where the program should be loaded</param>
		void SetProgramData(Program program, int index);

		/// <summary>
		/// Called by the host to get program data from the device.
		/// </summary>
		/// <param name="index">the index of the program to return</param>
		/// <returns></returns>
		Program GetProgramData(int index);

		/// <summary>
		/// Called by the host when the host environment has changed (sample rate, buffer size, etc..)
		/// When called, the device should query the host for all properties it needs, via HostInfo
		/// </summary>
		void HostChanged();

		/// <summary>
		/// An interface to allow the device to query the host for information. The host sets this
		/// as a reference to itself.
		/// </summary>
		HostInfo HostInfo { get; set; }
	}
}
