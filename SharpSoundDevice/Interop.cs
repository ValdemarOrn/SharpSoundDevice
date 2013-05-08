using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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
		static object LockObject = new object();

		static int CurrentID = 1000;

		/// <summary>
		/// 
		/// </summary>
		public static Dictionary<int, IAudioDevice> Devices = new Dictionary<int, IAudioDevice>();
		
		/// <summary>
		/// 
		/// </summary>
		public static Dictionary<int, string> LogFiles = new Dictionary<int, string>();

		private static string LogFile;

		private static void Log(string message, string filename = null)
		{
#if LOG
			if (filename == null)
				filename = LogFile;

			var ts = "<" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff ") + "> ";
			try
			{
				for(int i=0; i<10; i++)
				{
					try
					{
						System.IO.File.AppendAllText(filename, ts + message + "\n");
						break;
					}
					catch (Exception) { }
					System.Threading.Thread.Sleep(50);
				}
			}
			catch(Exception)
			{
				
			}
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dllFilename"></param>
		/// <param name="assemblyFilename"></param>
		/// <returns></returns>
		public static int CreateDevice(string dllFilename, string assemblyFilename)
		{
			assemblyFilename = assemblyFilename.Trim();

			if (assemblyFilename.StartsWith("::PLACEHOLDER"))
			{
				Log("VST Bridge dll has not been patched, it only contains placeholder assembly name. You must set the correct assembly name");
				return -1;
			}

			
			lock(LockObject)
			{
				int id = 0;
				id = CurrentID;
				CurrentID++;

				LogFile = dllFilename + ".log";
				Log("");
				Log("SharpSoundDevice Version: " + Assembly.GetExecutingAssembly().ImageRuntimeVersion);
#if DEBUG
				Log("SharpSoundDevice Configuration: Debug");
#else
				Log("SharpSoundDevice Configuration: Release");
#endif
				Log("Starting new plugin instance, ID: " + id);

				var assemblyName = assemblyFilename;
				Log("Assembly name: " + assemblyName);
				var searchPath = System.IO.Path.GetDirectoryName(dllFilename);
				var asm = LoadAssembly(assemblyName, searchPath);

				Log("Searching for classes in assembly");
				var exported = asm.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IAudioDevice))).ToList();
				Log("Number of classes: " + exported.Count);

				if (exported.Count == 0)
				{
					Log("No class implementing IAudioDevice was found in the specified assembly");
					return -1;
				}

				string className = exported[0].FullName;
				var type = exported.FirstOrDefault(x => x.FullName == className);

				if(type == null)
				{
					Log("Class not found");
					return -1;
				}

				Log("Calling parameterless constructor for class");
				var obj = type.GetConstructor(new Type[0]).Invoke(null);
				if (obj == null)
				{
					Log("Constructor call failed");
					return -1;
				}

				Log("Attempting to cast new object to IAudioDevice");
				IAudioDevice instance;

				try
				{
					instance = (IAudioDevice)obj;
				}
				catch(Exception e)
				{
					Log("Cast failed: " + e.Message);
					return -1;
				}

				Devices[id] = instance;
				LogFiles[id] = LogFile;

				Log("Object successfully loaded");
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
			if (Devices.ContainsValue(device))
				return Devices.First(x => x.Value == device).Key;
			else
				return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IAudioDevice GetDevice(int id)
		{
			if (Devices.ContainsKey(id))
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

			Devices.Remove(id);
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="message"></param>
		public static void LogDeviceMessage(IAudioDevice device, string message)
		{
			LogDeviceMessage(GetID(device), message);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="e"></param>
		public static void LogDeviceException(IAudioDevice device, Exception e)
		{
			LogDeviceException(GetID(device), e);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="message"></param>
		public static void LogDeviceMessage(int id, string message)
		{
			string filename = LogFiles.ContainsKey(id) ? LogFiles[id] : LogFile;
			Log(message, filename);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="e"></param>
		public static void LogDeviceException(int id, Exception e)
		{
			string filename = LogFiles.ContainsKey(id) ? LogFiles[id] : LogFile;
			var msg = e.Message + "\n\n" + e.StackTrace;
			if(e.InnerException != null)
			{
				msg += "\n\n" + e.InnerException.Message + "\n\n" + e.InnerException.StackTrace;
			}

			Log(msg, filename);
		}

		// --------------------------------------------------------------------

		/// <summary>
		/// Attempts to load a dll assembly with the specified name. Returns true if successful
		/// </summary>
		/// <param name="filename">the name of the dll file</param>
		/// <param name="searchPath">alternative path where the file might be located (other than current working directory)</param>
		/// <returns></returns>
		public static Assembly LoadAssembly(string filename, string searchPath = null)
		{
			Log("Attempting to load assembly " + filename);
			Log("Alternative search path is " + searchPath);

			if (!System.IO.File.Exists(filename))
				filename = System.IO.Path.Combine(searchPath, filename);

			Log("Searching for " + filename);

			if (!System.IO.File.Exists(filename))
			{
				Log("File not found: " + filename);
				return null;
			}

			// check if assembly is already loaded
			try
			{
				Log("Checking if assembly is already loaded");
				var loaded = AppDomain.CurrentDomain.GetAssemblies();
				var refOnly = Assembly.ReflectionOnlyLoadFrom(filename);

				if (loaded.Any(x => x.FullName == refOnly.FullName))
				{
					Log("Assembly is already loaded, returning a reference");
					return loaded.First(x => x.FullName == refOnly.FullName);
				}
			}
			catch (Exception e)
			{
				Log("Unexpected error: " + e.Message);
				return null;
			}

			// If not already loaded, then we load it

			try
			{
				Log("Loading assembly");
				var asm = Assembly.LoadFrom(filename);
				Log("Assembly loaded");
				return asm;
			}
			catch (Exception e)
			{
				Log("Unexpected error: " + e.Message);
				return null;
			}
		}

		/// <summary>
		/// Copies data from double** into managed 2d array
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="InputPortCount"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static double[][] GetManagedSamples(IntPtr ptr, int InputPortCount, uint bufferSize)
		{
			unsafe
			{
				double** input = (double**)ptr;

				var output = new double[InputPortCount][];
				for(int i=0; i < InputPortCount; i++)
				{
					double[] ch = new double[bufferSize];
					output[i] = ch;

					Marshal.Copy((IntPtr)input[i], ch, 0, (int)bufferSize);
				}

				return output;
			}
		}

		/// <summary>
		/// Creates an empty 2d managed array
		/// </summary>
		/// <param name="InputPortCount"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static double[][] GetEmptyArrays(int InputPortCount, uint bufferSize)
		{
			var output = new double[InputPortCount][];

			for (int i = 0; i < InputPortCount; i++)
				output[i] = new double[bufferSize];
			
			return output;
		}

		/// <summary>
		/// Copies data from a managed 2d array into an unmanaged double**
		/// </summary>
		/// <param name="outp"></param>
		/// <param name="ptr"></param>
		/// <param name="OutputPortCount"></param>
		/// <param name="bufferSize"></param>
		public static void CopyToUnmanaged(double[][] outp, IntPtr ptr, int OutputPortCount, uint bufferSize)
		{
			unsafe
			{
				double** output = (double**)ptr;

				for (int i = 0; i < OutputPortCount; i++)
				{
					void* channel = output[i];
					Marshal.Copy(outp[i], 0, (IntPtr)channel, (int)bufferSize);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="buffer"></param>
		/// <param name="maxLen"></param>
		public static void CopyStringToBuffer(string input, IntPtr buffer, int maxLen)
		{
			var bytes = Encoding.UTF8.GetBytes(input + '\0');
			int count = (maxLen < bytes.Length) ? maxLen : bytes.Length;
			Marshal.Copy(bytes, 0, buffer, count);

			// add null terminator at end of buffer
			unsafe
			{
				byte* ptr = (byte*)buffer;
				ptr[maxLen - 1] = 0;
			}
		}

	}
}
