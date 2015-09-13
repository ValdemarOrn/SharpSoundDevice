using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharpSoundDevice
{
	/*
	/// This is a plugin Loader that loads each plugin into a separate appDomain. Unfortunately, the overhead of passing between domains is too big, performance goes to shit.
	public class PluginMarshall : MarshalByRefObject, IAudioDevice
	{
		#region SetupRegion

		private static Dictionary<string, AppDomain> appDomains = new Dictionary<string, AppDomain>();

		public static PluginMarshall Create(string assemblyFilename, string bridgeDllDir)
		{
			AppDomain appDomain;
			var pluginAssemblyPath = GetAssemblyPath(assemblyFilename, bridgeDllDir);
			if (pluginAssemblyPath == null)
			{
				Logging.Log(string.Format("Unable to find assembly file. AssemblyFilename: {0}, BridgeDllDir: {1}", assemblyFilename, bridgeDllDir));
				return null;
			}
			
			if (appDomains.ContainsKey(assemblyFilename))
			{
				appDomain = appDomains[assemblyFilename];
			}
			else
			{
				appDomain = CreateAppDomain(assemblyFilename, Path.GetDirectoryName(pluginAssemblyPath));
				appDomains[assemblyFilename] = appDomain;
			}

			var marshall = (PluginMarshall)appDomain.CreateInstanceAndUnwrap(typeof(PluginMarshall).Assembly.FullName, typeof(PluginMarshall).FullName);
			var asm = marshall.LoadPluginAssembly(pluginAssemblyPath);
			marshall.CreatePluginInstance(asm);
			return marshall.HasInstance ? marshall : null;
        }

		private static string GetAssemblyPath(string assemblyFilename, string bridgeDllDir)
		{
			Logging.Log(string.Format("Locating Assembly: {0} BridgeDllDir: {1}", assemblyFilename, bridgeDllDir));
			string requestAssemblyPath = null;
			var intermediaryDir = assemblyFilename.EndsWith(".dll") ? assemblyFilename.Substring(0, assemblyFilename.Length - 4) : assemblyFilename;
			var filename = assemblyFilename.EndsWith(".dll") ? assemblyFilename : assemblyFilename + ".dll";

			// Look in:
			// bridgeDllDir/assemblyFilename.dll
			// bridgeDllDir/assemblyFilename/assemblyFilename.dll
			var path1 = Path.Combine(bridgeDllDir, filename);
			var path2 = Path.Combine(bridgeDllDir, intermediaryDir, filename);

			if (File.Exists(path1))
				requestAssemblyPath = path1;
			else if (File.Exists(path2))
				requestAssemblyPath = path2;

			return requestAssemblyPath;
		}

		private static AppDomain CreateAppDomain(string assemblyFilename, string appDomainBaseDir)
		{
			var info = new AppDomainSetup { ApplicationBase = appDomainBaseDir };
			var appDomain = AppDomain.CreateDomain("PluginDomain " + assemblyFilename, null, info);
			Logging.Log("Created AppDomain: " + appDomain.FriendlyName);
			Logging.Log("AppDomain BaseDir:" + appDomainBaseDir);
			return appDomain;
		}

		
		private class DomainResolver : MarshalByRefObject
		{
			private readonly string assemblyFilename;
			private readonly string bridgeDllDir;

			public DomainResolver(string assemblyFilename, string bridgeDllDir)
			{
				this.assemblyFilename = assemblyFilename;
				this.bridgeDllDir = bridgeDllDir;
			}
			
			public Assembly Resolve(object s, ResolveEventArgs e)
			{
				var folderPath = bridgeDllDir;
				var requestedAssemblyName = e.Name;

				string requestAssemblyPath = null;
				var path1 = Path.Combine(folderPath, requestedAssemblyName + ".dll");
				var path2 = Path.Combine(folderPath, assemblyFilename, requestedAssemblyName + ".dll");
				if (File.Exists(path1))
					requestAssemblyPath = path1;
				else if (File.Exists(path2))
					requestAssemblyPath = path2;

				if (requestAssemblyPath == null)
					return null;

				var requestedAssembly = Assembly.LoadFrom(requestAssemblyPath);
				return requestedAssembly;
			}
		}
		

		/// <summary>
		/// Works within an isolated AppDomain.
		/// 
		/// Tracks down and loads the actual SharpSoundDevice plugin dll into the appdomain.
		/// Sets up AssemblyResolver to resolve plugin's dependencies, without affecting other plugins
		/// </summary>
		/// <param name="pluginAssemblyPath"></param>
		private Assembly LoadPluginAssembly(string pluginAssemblyPath)
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
		/// Creates an instance of the first IAudioDevice class found in the assembly, sets it to the internal property of the marshaller
		/// </summary>
		/// <param name="asm"></param>
		private void CreatePluginInstance(Assembly asm)
		{
			Logging.Log("Searching for classes in assembly " + asm.FullName);
			var exported = asm.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IAudioDevice))).ToList();
			Logging.Log("Number of classes: " + exported.Count);

			if (exported.Count == 0)
			{
				Logging.Log("No class implementing IAudioDevice was found in the specified assembly");
				return;
			}

			var type = exported.First();
			Logging.Log("Found at least one plugin class, calling parameterless constructor for " + type.FullName);
			var obj = type.GetConstructor(new Type[0]).Invoke(null);
			if (obj == null)
			{
				Logging.Log("Constructor call failed");
				return;
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
				return;
			}

			Logging.Log("Successfully created new plugin instance");
			this.Instance = instance;
			this.HasInstance = true;
		}

		#endregion

		private IAudioDevice Instance { get; set; }
		public bool HasInstance { get; private set; }

		public int CurrentProgram
		{
			get
			{
				return Instance.CurrentProgram;
			}
		}

		public int DeviceId
		{
			get { return Instance.DeviceId; }
			set { Instance.DeviceId = value; }
		}

		public DeviceInfo DeviceInfo
		{
			get
			{
				return Instance.DeviceInfo;
            }
		}

		public IHostInfo HostInfo
		{
			get
			{
				return Instance.HostInfo;
            }

			set
			{
				Instance.HostInfo = value;
            }
		}

		public Parameter[] ParameterInfo
		{
			get
			{
				return Instance.ParameterInfo;
            }
		}

		public Port[] PortInfo
		{
			get
			{
				return Instance.PortInfo;
            }
		}

		public void CloseEditor()
		{
			Instance.CloseEditor();
        }

		public void DisposeDevice()
		{
			Instance.DisposeDevice();
        }

		public Program GetProgramData(int index)
		{
			return Instance.GetProgramData(index);
        }

		public void HostChanged()
		{
			Instance.HostChanged();
        }

		public void InitializeDevice()
		{
			Instance.InitializeDevice();
        }

		public void OpenEditor(IntPtr parentWindow)
		{
			Instance.OpenEditor(parentWindow);
        }

		private static List<double> times = new List<double>();

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			Instance.ProcessSample(input, output, bufferSize);
        }

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			Instance.ProcessSample(input, output, inChannelCount, outChannelCount, bufferSize);
        }

		public bool SendEvent(Event ev)
		{
			return Instance.SendEvent(ev);
        }

		public void SetProgramData(Program program, int index)
		{
			Instance.SetProgramData(program, index);
        }

		public void Start()
		{
			Instance.Start();
        }

		public void Stop()
		{
			Instance.Stop();
        }
	}
	*/
}
