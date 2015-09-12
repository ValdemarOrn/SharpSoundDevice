using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// Interface implemented by a plugin bridge or host to facilitate device-to-host communication.
	/// </summary>
	public interface IHostInfo
	{
		/// <summary>
		/// Method used to send events from device to host.
		/// See Event struct for more info.
		/// </summary>
		/// <param name="pluginSenderId">The id of the device that is sending the event.</param>
		/// <param name="ev">Event sent from device to host.</param>
		void SendEvent(int pluginSenderId, Event ev);

		/// <summary>
		/// Returns the current BPM (beats per minute) of the host.
		/// </summary>
		double BPM { get; }

		/// <summary>
		/// Returns the number of samples from start position.
		/// </summary>
		double SamplePosition { get; }

		/// <summary>
		/// Returns the current sample rate of the host.
		/// </summary>
		double SampleRate { get; }

		/// <summary>
		/// Returns the current sample buffer size of the host / audio driver.
		/// </summary>
		int BlockSize { get; }

		/// <summary>
		/// Returns the current time signature numerator of the host.
		/// </summary>
		int TimeSignatureNum { get; }

		/// <summary>
		/// Returns the current time signature denominator of the host.
		/// </summary>
		int TimeSignatureDen { get; }

		/// <summary>
		/// Returns the host author/vendor/manufacturer (e.g. Steinberg, Ableton...).
		/// </summary>
		string HostVendor { get; }

		/// <summary>
		/// Returns the host name (e.g. Cubase, Live...).
		/// </summary>
		string HostName { get; }

		/// <summary>
		/// Returns the host version.
		/// </summary>
		uint HostVersion { get; }
	}
}
