#include "AssemblyInitializer.h"

#using <mscorlib.dll>
using namespace System;

namespace SharpSoundDevice
{
	namespace VST
	{
		// ---------------------- Assembly Initializer ------------------------------------

		void AssemblyLoader::AddAssemblyResolver()
		{
			if (!Loaded)
			{
				AppDomain^ domain = AppDomain::CurrentDomain;
				domain->AssemblyResolve += gcnew ResolveEventHandler(loadAsm);
				Loaded = true;
			}
		}

		// This is used to look for a missing assembly. If the assembly is not located in the same directory as
		// the host process (very unlikely as most peopel have a separate VST plugin directory) we instruct 
		// the .NET runtime to look in the same directory as this dll
		Reflection::Assembly^ AssemblyLoader::loadAsm(Object^ sender, ResolveEventArgs^ args)
		{
			String^ folderPath = System::IO::Path::GetDirectoryName(Reflection::Assembly::GetExecutingAssembly()->Location);
			String^ assemblyPath = System::IO::Path::Combine(folderPath, (gcnew Reflection::AssemblyName(args->Name))->Name + ".dll");
			if (System::IO::File::Exists(assemblyPath) == false)
				return nullptr;

			Reflection::Assembly^ assembly = Reflection::Assembly::LoadFrom(assemblyPath);
			return assembly;
		}
	}
}