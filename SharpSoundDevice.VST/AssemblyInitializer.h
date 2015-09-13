#ifndef ASSEMBLY_INITIALIZER
#define ASSEMBLY_INITIALIZER

#using <mscorlib.dll>
using namespace System;

namespace SharpSoundDevice
{
	namespace VST
	{
		// ---------------------- Assembly Initializer ------------------------------------

		public ref class AssemblyLoader
		{
		private:
			static bool Loaded;
			static System::Collections::Generic::Dictionary<System::String^, System::String^>^ pluginPaths = gcnew System::Collections::Generic::Dictionary<System::String^, System::String^>();

			static System::Reflection::Assembly^ LoadAsm(Object^ sender, ResolveEventArgs^ args);
		public:
			static void AddAssemblyResolver(System::String^ assemblyName);
		};
	}
}

#endif
