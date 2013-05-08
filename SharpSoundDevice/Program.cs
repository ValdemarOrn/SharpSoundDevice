using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A struct containing program data.
	/// </summary>
	public struct Program
	{
		/// <summary>
		/// The name of the program
		/// </summary>
		public string Name;

		/// <summary>
		/// Program data. Format is specified by the device.
		/// </summary>
		public byte[] Data;
	}

	/// <summary>
	/// A class used by the host / bridge plugin to serialize and deserialize programs and banks in a common format.
	/// </summary>
	public class ProgramData
	{
		static string Header = "SharpSoundDevice Program Format, version 1000";
		static string ProgType = "Prog";
		static string BankType = "Bank";

		/*
		 * Serialized Format:
		 * (newline character separates each line of data)
		 * 
		 * Header
		 * Type
		 * Program Name
		 * Base64 Encoded Pogram 1
		 * Base64 Encoded Pogram 2
		 * Base64 Encoded Pogram 3
		 * ....
		 * Program Name
		 * Base64 Encoded Pogram 1
		 * Base64 Encoded Pogram 2
		 * Base64 Encoded Pogram 3
		 * ....
		 * 
		*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="program"></param>
		/// <returns></returns>
		public static byte[] SerializeSingleProgram(Program program)
		{
			string output = "";
			output += Header + "\n";
			output += ProgType + "\n";

			output += ((program.Name != null) ? program.Name : "") + "\n";
			output += ((program.Data != null) ? Convert.ToBase64String(program.Data) : "") + "\n";

			var bytes = Encoding.UTF8.GetBytes(output);
			return bytes;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="programs"></param>
		/// <returns></returns>
		public static byte[] SerializeBank(Program[] programs)
		{
			string output = "";
			output += Header + "\n";
			output += BankType + "\n";

			foreach (var program in programs)
			{
				output += ((program.Name != null) ? program.Name : "") + "\n";
				output += ((program.Data != null) ? Convert.ToBase64String(program.Data) : "") + "\n";
			}

			var bytes = Encoding.UTF8.GetBytes(output);
			return bytes;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public static byte[] SerializeBank(IAudioDevice device)
		{
			var programs = new Program[device.DeviceInfo.ProgramCount];

			for (int i = 0; i < programs.Length; i++)
				programs[i] = device.GetProgramData(i);

			return SerializeBank(programs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Program DeserializeSingleProgram(byte[] data)
		{
			string input = Encoding.UTF8.GetString(data);

			var lines = input.Split('\n');
			if (lines[0] != Header || lines[1] != ProgType)
				return new Program();

			string name = lines[2];
			byte[] d = Convert.FromBase64String(lines[3]);

			var output = new Program();
			output.Name = name;
			output.Data = d;

			return output;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Program[] DeserializeBank(byte[] data)
		{
			var output = new List<Program>();
			string input = Encoding.UTF8.GetString(data);

			var lines = input.Split('\n');
			if (lines[0] != Header || lines[1] != BankType)
				return new Program[0];

			for (int i = 2; i < lines.Length - 1; i += 2)
			{
				string name = lines[i];
				byte[] d = Convert.FromBase64String(lines[i + 1]);

				var prog = new Program();
				prog.Name = name;
				prog.Data = d;
				output.Add(prog);
			}

			return output.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="device"></param>
		public static void DeserializeBank(byte[] data, IAudioDevice device)
		{
			var programs = DeserializeBank(data);
			var max = device.DeviceInfo.ProgramCount;

			for(int i=0; i<programs.Length; i++)
			{
				if (i >= max) // safety, don't overfill the device
					break;

				device.SetProgramData(programs[i], i);
			}
		}
	}
}
