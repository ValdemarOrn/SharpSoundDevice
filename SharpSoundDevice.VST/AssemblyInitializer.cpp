#include "AssemblyInitializer.h"

#using <mscorlib.dll>
using namespace System;
using namespace System::IO;
using namespace System::Reflection;

namespace SharpSoundDevice
{
	namespace VST
	{
		void AssemblyLoader::AddAssemblyResolver(System::String^ pluginAssemblyName)
		{
			try
			{
				// store assemblyname and this dll's directory
				String^ bridgeDllDir = Path::GetDirectoryName(Reflection::Assembly::GetExecutingAssembly()->Location);
				if (!pluginPaths->ContainsKey(pluginAssemblyName))
					pluginPaths->Add(pluginAssemblyName, bridgeDllDir);

				if (!Loaded)
				{
					AppDomain^ domain = AppDomain::CurrentDomain;
					domain->AssemblyResolve += gcnew ResolveEventHandler(LoadAsm);
				}
			}
			catch (System::Exception^ ex)
			{
				System::Console::WriteLine(ex->Message);
			}
		}
		
		// This is used to look for a missing assembly. If the assembly is not located in the same directory as
		// the host process (very unlikely as most peopel have a separate VST plugin directory) we instruct 
		// the .NET runtime to look in the same directory as this dll
		System::Reflection::Assembly^ AssemblyLoader::LoadAsm(Object^ sender, ResolveEventArgs^ args)
		{
			try
			{
				String^ missingAssemblyFilename = (gcnew Reflection::AssemblyName(args->Name))->Name + ".dll";

				for each(System::Collections::Generic::KeyValuePair<System::String^, System::String^> kvp in pluginPaths)
				{
					String^ pluginAssemblyName = kvp.Key;
					String^ bridgeDllDir = kvp.Value;
					String^ pluginDir = pluginAssemblyName->EndsWith(".dll") ? pluginAssemblyName->Substring(0, pluginAssemblyName->Length - 4) : pluginAssemblyName;

					String^ path = nullptr;
					String^ path1 = Path::Combine(bridgeDllDir, pluginDir, missingAssemblyFilename);
					String^ path2 = Path::Combine(bridgeDllDir, missingAssemblyFilename);

					if (File::Exists(path1))
						path = path1;
					else if (File::Exists(path2))
						path = path2;
					else
						continue;

					Assembly^ assembly = Assembly::LoadFrom(path);
					return assembly;
				}
			}
			catch (System::Exception^ ex)
			{
				System::Console::WriteLine(ex->Message);
				System::Console::WriteLine(ex->StackTrace);
				if (ex->InnerException != nullptr)
				{
					System::Console::WriteLine(ex->InnerException->Message);
					System::Console::WriteLine(ex->InnerException->StackTrace);
				}
			}

			return nullptr;
		}
	}
}