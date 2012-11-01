using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	public struct Program
	{
		public string Name;
		public byte[] Data;
	}

	public class ProgramData
	{
		static string Header = "VSTAudioDevice Version 1.0";
		static string ProgType = "Prog";
		static string BankType = "Bank";

		/*
		 * Serialized Format:
		 * 
		 * Header
		 * Type
		 * Program Name
		 * Base64 Encoded Data
		 * ....
		 * Program Name
		 * Base64 Encoded Data
		 * ....
		 * 
		*/

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

		public static byte[] SerializeBank(IAudioDevice device)
		{
			var programs = new Program[device.DeviceInfo.ProgramCount];

			for (int i = 0; i < programs.Length; i++)
				programs[i] = device.GetProgramData(i);

			return SerializeBank(programs);
		}

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

		/*static byte[] Combine(params byte[][] args)
		{
			long index = 0;
			byte[] output = new byte[args.Sum(x => x.Length)];

			for(int i=0; i<args.Length; i++)
			{
				Array.Copy(args[i], 0, output, index, args[i].Length);
				index += args[i].Length;
			}

			return output;
		}*/
	}
}
