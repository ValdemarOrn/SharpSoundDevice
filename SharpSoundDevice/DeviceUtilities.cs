using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharpSoundDevice
{
	public class DeviceUtilities
	{
		/// <summary>
		/// Generates a 4-byte integer by hashing an input string and taking modulus.
		/// Useful to create VST Id that is unique to your plugin
		/// </summary>
		/// <param name="seedString">input string, e.g. plugin and developer name</param>
		/// <returns></returns>
		public static int GenerateIntegerId(string seedString)
		{
			var bytes = Encoding.UTF8.GetBytes(seedString);
			SHA256Managed hashstring = new SHA256Managed();
			byte[] hash = hashstring.ComputeHash(bytes);
			var bignum = BitConverter.ToUInt64(hash, 4);
			return (Int32)(bignum % Int32.MaxValue);
		}

		public static byte[] SerializeParameters(Parameter[] parameters)
		{
			parameters = parameters.OrderBy(x => x.Index).ToArray();

			string output = "";
			foreach (var p in parameters)
				output += p.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", ";

			return Encoding.UTF8.GetBytes(output);
		}

		public static void DeserializeParameters(Parameter[] parameters, byte[] data)
		{
			parameters = parameters.OrderBy(x => x.Index).ToArray();

			string values = Encoding.UTF8.GetString(data);
			var items = values.Split(',');
			var filtered = items.Where(x => x != null && x.Trim() != "").ToList();
			var select = filtered.Select(x => Convert.ToDouble(x.Trim(), System.Globalization.CultureInfo.InvariantCulture)).ToList();

			if (select.Count != parameters.Length)
				throw new Exception("Illegal program data. Number of parameters does not match");

			for (int i = 0; i < parameters.Length; i++)
				parameters[i].Value = select[i];
		}
	}
}
