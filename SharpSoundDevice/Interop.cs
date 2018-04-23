using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpSoundDevice
{
	/// <summary>
	/// Class containing function used by the VST plugin bridge to create new device
	/// instances and track running devices.
	/// 
	/// Not to be used by plugin devices!
	/// </summary>
	public class Interop
	{
		private static object LockObject = new object();
		private static int CurrentID = 1;
		private static List<double[][]> AudioBuffers = new List<double[][]> { null };
		private static List<IAudioDevice> Devices = new List<IAudioDevice> { null };

		/// <summary>
		/// Load the plugin assembly and instantiates a new instance of the plugin class
		/// </summary>
		/// <param name="dllFilename"></param>
		/// <param name="assemblyFilename"></param>
		/// <returns></returns>
		public static int CreateDevice(string dllFilename, string assemblyFilename)
		{
			assemblyFilename = assemblyFilename.Trim();

			if (assemblyFilename.StartsWith("::PLACEHOLDER"))
			{
				Logging.Log("VST Bridge dll has not been patched, it only contains placeholder assembly name. You must set the correct assembly name");
				return -1;
			}

			lock (LockObject)
			{
				int id = CurrentID;
				CurrentID++;

				Logging.Log("SharpSoundDevice Version: " + Assembly.GetExecutingAssembly().FullName);
				Logging.Log("Starting new plugin instance, ID: " + id);

				var assemblyName = assemblyFilename;
				Logging.Log("Assembly name: " + assemblyName);
				var bridgeDllDir = Path.GetDirectoryName(dllFilename);
				Logging.Log("Bridge Dll Dir: " + bridgeDllDir);
				var instance = PluginLoader.Create(assemblyName, bridgeDllDir);

				if (instance == null)
					return -1;

				Devices.Add(instance); // extend list by one
				AudioBuffers.Add(null);
				instance.DeviceId = GetID(instance);
				Logging.Log("Object successfully loaded, DeviceId: " + GetID(instance));
				return id;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public static int GetID(IAudioDevice device)
		{
			if (device == null)
				return -1;

			return Devices.IndexOf(device);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IAudioDevice GetDevice(int id)
		{
			if (id > 0 && id < Devices.Count)
				return Devices[id];
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool DeleteDevice(int id)
		{
			var dev = GetDevice(id);
			if (dev == null)
				return true;

			Devices[id] = null;
			return true;
		}

		/// <summary>
		/// removes any reserved buffers for the specified device. Used at plugin unload time
		/// </summary>
		/// <param name="deviceId"></param>
		public static void UnloadAudioBuffers(int deviceId)
		{
			AudioBuffers[deviceId] = null;
		}

		public static double[][] GetAudioBuffers(int deviceId, int channelCount, int bufferSize)
		{
			var existingBuffer = AudioBuffers[deviceId];
			if (existingBuffer == null || existingBuffer.Length != channelCount || existingBuffer[0].Length != bufferSize)
			{
				var newBuf = new double[channelCount][];
				for (int i = 0; i < channelCount; i++)
				{
					newBuf[i] = new double[bufferSize];
				}

				AudioBuffers[deviceId] = newBuf;
				existingBuffer = newBuf;
			}

			return existingBuffer;
		}
	}
}
