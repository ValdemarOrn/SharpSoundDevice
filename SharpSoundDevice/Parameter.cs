using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSoundDevice
{
	/// <summary>
	/// A class containing parameter info.
	/// </summary>
	public class Parameter
	{
		/// <summary>
		/// The name of the parameter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The index of the parameter
		/// </summary>
		public uint Index { get; set; }

		/// <summary>
		/// Indicates resolution / number of steps the parameter has.
		/// Set to zero for infinite resolution.
		/// </summary>
		public uint Steps { get; set; }

		/// <summary>
		/// The current value of the parameter.
		/// Must be between 0.0...1.0 (inclusive)
		/// </summary>
		public double Value { get; set; }

		/// <summary>
		/// The formatted display value of the parameter
		/// </summary>
		public string Display { get; set; }
	}
}
