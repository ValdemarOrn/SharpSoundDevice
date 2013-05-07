using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BridgeGenerator
{
	/// <summary>
	/// This is a very simple program that creates a VST-to-C# bridge.
	/// It uses the SharpSoundDevice.VST dll as a template and replaces the placeholder
	/// string with the name of the .NET assembly to load.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			string TemplateName = "SharpSoundDevice.VST.dll";
			var location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			TemplateName = Path.Combine(location, TemplateName);


			if(args.Length == 0)
			{
				Console.WriteLine("Specify the name of the .NET assembly");
				Console.WriteLine("Example:\nBridgeGenerator.exe MyAssembly.dll");
				Console.WriteLine("Example:\nBridgeGenerator.exe MyAssembly.dll MyAssembly.VST.dll");
				return;
			}

			var dllName = Path.GetFileName(args[0]);

			if(dllName.Length > 200)
			{
				Console.WriteLine("Dll name must be less than 200 characters long");
				return;
			}

			if (!File.Exists(TemplateName))
			{
				Console.WriteLine("Error! " + TemplateName + " is missing.");
				return;
			}

			var template = File.ReadAllBytes(TemplateName);
			var placeholder = Encoding.ASCII.GetBytes("::PLACEHOLDER::");

			var idx = template.SequenceIndex(placeholder);

			if(idx == -1)
			{
				Console.WriteLine("An error occured. Template.VST.dll is not a valid SharpSoundDevice DLL!");
				return;
			}

			if (dllName.Length < placeholder.Length)
				dllName += new String(' ', placeholder.Length - dllName.Length);

			var dllNameBytes = Encoding.ASCII.GetBytes(dllName);
			Array.Copy(dllNameBytes, 0, template, idx, dllNameBytes.Length);
			
			string outputFile = "Bridge." + dllName;
			if (args.Length >= 2)
				outputFile = args[1];

			if (File.Exists(outputFile))
			{
				Console.WriteLine("There already exists a file named " + outputFile);
				Console.WriteLine("Rename or remove the file before trying again");
				return;
			}

			File.WriteAllBytes(outputFile, template);
			Console.WriteLine(outputFile + " file generated");
		}
	}

}
