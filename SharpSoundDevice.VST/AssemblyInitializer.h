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
			static Boolean Loaded;
		public:
			static void AddAssemblyResolver();
			static System::Reflection::Assembly^ loadAsm(Object^ sender, ResolveEventArgs^ args);
			static System::Collections::Generic::List<double>^ times = gcnew System::Collections::Generic::List<double>();
		};
	}
}

#endif
