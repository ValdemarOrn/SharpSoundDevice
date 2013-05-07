using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BridgeGenerator
{
	public static class Extensions
	{
		/// <summary>
		/// Returns the index where the first sub-sequence in the array. Returns -1 if not found
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static int SequenceIndex<T>(this T[] data, T[] sequence, int start = 0)
		{
			for (int i = start; i < data.Length; i++)
			{
				int k = 0;
				while (k < sequence.Length)
				{
					if (data[i].Equals(sequence[k]))
					{
						i++;
						k++;
					}
					else
						break;
				}

				if (k >= sequence.Length)
					return i - k;
			}

			return -1;
		}
	}
}
