using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// Interface implemented by a plugin bridge or host to facilitate device-to-host communication.
	/// </summary>
	public interface HostInfo
	{
		/// <summary>
		/// Method used to send events from device to host.
		/// </summary>
		/// <param name="sender">The device that is sending the event.</param>
		/// <param name="ev">Event sent from device to host.</param>
		void SendEvent(IAudioDevice sender, Event ev);

		/// <summary>
		/// Getter to query the host for current BPM (beats per minute)
		/// </summary>
		double BPM { get; }

		/// <summary>
		/// Getter to query the host for number of samples from start position.
		/// </summary>
		double SamplePosition { get; }

		/// <summary>
		/// Getter to query the host for the current sample rate
		/// </summary>
		double SampleRate { get; }

		/// <summary>
		/// Getter to query the host for the current sample buffer size.
		/// </summary>
		int BlockSize { get; }

		/// <summary>
		/// Getter to query the host for the current time signature numerator
		/// </summary>
		int TimeSignatureNum { get; }

		/// <summary>
		/// Getter to query the host for the current time signature denominator
		/// </summary>
		int TimeSignatureDen { get; }

		/// <summary>
		/// Getter to query the host for the host author/vendor/manufacturer
		/// </summary>
		string HostVendor { get; }

		/// <summary>
		/// Getter to query the host for the host name (e.g. Cubase, Live...)
		/// </summary>
		string HostName { get; }

		/// <summary>
		/// Getter to query the host for the host version.
		/// </summary>
		uint HostVersion { get; }
	}
}
