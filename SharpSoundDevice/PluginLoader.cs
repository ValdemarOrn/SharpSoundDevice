using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharpSoundDevice
{
	public class PluginLoader
	{
		public static IAudioDevice Create(string assemblyFilename, string bridgeDllDir)
		{
			var pluginAssemblyPath = GetAssemblyPath(assemblyFilename, bridgeDllDir);
			if (pluginAssemblyPath == null)
			{
				Logging.Log(string.Format("Unable to find assembly file. AssemblyFilename: {0}, BridgeDllDir: {1}", assemblyFilename, bridgeDllDir));
				return null;
			}

			var asm = LoadPluginAssembly(pluginAssemblyPath);
			//RegisterAssemblyResolver(Path.GetDirectoryName(pluginAssemblyPath)); // Already taken care of in C++/CLI code
			
			var instance = CreatePluginInstance(asm);
			return instance;
        }

		/*private static void RegisterAssemblyResolver(string pluginDir)
		{
			// Attempts to first resolve the dependency in the same directory as the main plugin dll
			// Will then try looking in the parent directory

			AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
			{
				var requestedAssemblyName = e.Name;
				var parentDir = Path.GetDirectoryName(pluginDir);

				string requestAssemblyPath = null;
				var path1 = Path.Combine(pluginDir, requestedAssemblyName + ".dll");
				var path2 = Path.Combine(parentDir, requestedAssemblyName + ".dll");
				if (File.Exists(path1))
					requestAssemblyPath = path1;
				else if (File.Exists(path2))
					requestAssemblyPath = path2;

				if (requestAssemblyPath == null)
					return null;

				var requestedAssembly = Assembly.LoadFrom(requestAssemblyPath);
				return requestedAssembly;
			};
        }*/

		private static string GetAssemblyPath(string assemblyFilename, string bridgeDllDir)
		{
			Logging.Log(string.Format("Locating Assembly: {0} BridgeDllDir: {1}", assemblyFilename, bridgeDllDir));
			string requestAssemblyPath = null;
			var intermediaryDir = assemblyFilename.EndsWith(".dll") ? assemblyFilename.Substring(0, assemblyFilename.Length - 4) : assemblyFilename;
			var filename = assemblyFilename.EndsWith(".dll") ? assemblyFilename : assemblyFilename + ".dll";

			// Look in:
			// bridgeDllDir/assemblyFilename/assemblyFilename.dll
			// bridgeDllDir/assemblyFilename.dll
			var path1 = Path.Combine(bridgeDllDir, intermediaryDir, filename);
			var path2 = Path.Combine(bridgeDllDir, filename);
			
			if (File.Exists(path1))
				requestAssemblyPath = path1;
			else if (File.Exists(path2))
				requestAssemblyPath = path2;

			return requestAssemblyPath;
		}

		/// <summary>
		/// Tracks down and loads the actual SharpSoundDevice plugin dll.
		/// </summary>
		/// <param name="pluginAssemblyPath"></param>
		private static Assembly LoadPluginAssembly(string pluginAssemblyPath)
		{
			try
			{
				Logging.Log(string.Format("Attempting load load Assembly {0} into AppDomain {1}", pluginAssemblyPath, AppDomain.CurrentDomain.FriendlyName));
				var asm = Assembly.LoadFile(pluginAssemblyPath);
				Logging.Log("Loaded assembly " + asm.FullName);
				return asm;
			}
			catch (Exception ex)
			{
				Logging.Log("Unexpected error while trying to load assembly " + pluginAssemblyPath + ":\n" + ex.GetTrace());
				return null;
			}
        }

		/// <summary>
		/// Creates an instance of the first IAudioDevice class found in the assembly
		/// </summary>
		/// <param name="asm"></param>
		private static IAudioDevice CreatePluginInstance(Assembly asm)
		{
			Logging.Log("Searching for classes in assembly " + asm.FullName);
			var exported = asm.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IAudioDevice))).ToList();
			Logging.Log("Number of classes: " + exported.Count);

			if (exported.Count == 0)
			{
				Logging.Log("No class implementing IAudioDevice was found in the specified assembly");
				return null;
			}

			var type = exported.First();
			Logging.Log("Found at least one plugin class, calling parameterless constructor for " + type.FullName);
			var obj = type.GetConstructor(new Type[0]).Invoke(null);
			if (obj == null)
			{
				Logging.Log("Constructor call failed");
				return null;
			}

			Logging.Log("Attempting to cast new object to IAudioDevice");
			IAudioDevice instance;

			try
			{
				instance = (IAudioDevice)obj;
			}
			catch (Exception e)
			{
				Logging.Log("Cast failed: " + e.Message);
				return null;
			}

			Logging.Log("Successfully created new plugin instance");
			return instance;
		}
	}
}
