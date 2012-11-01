using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpSoundDevice.Devices
{
	class Childs
	{
		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

		static EnumWindowProc childProc;
		/// <summary>
		/// Returns a list of child windows
		/// </summary>
		/// <param name="parent">Parent of the windows to return</param>
		/// <returns>List of child windows</returns>
		public static List<IntPtr> GetChildWindows(IntPtr parent)
		{
			List<IntPtr> result = new List<IntPtr>();
			GCHandle listHandle = GCHandle.Alloc(result);
			try
			{
				childProc = new EnumWindowProc(EnumWindow);
				EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
			}
			finally
			{
				if (listHandle.IsAllocated)
					listHandle.Free();
			}
			return result;
		}

		/// <summary>
		/// Callback method to be used when enumerating windows.
		/// </summary>
		/// <param name="handle">Handle of the next window</param>
		/// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
		/// <returns>True to continue the enumeration, false to bail</returns>
		public static bool EnumWindow(IntPtr handle, IntPtr pointer)
		{
			GCHandle gch = GCHandle.FromIntPtr(pointer);
			List<IntPtr> list = gch.Target as List<IntPtr>;
			if (list == null)
			{
				throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
			}
			list.Add(handle);
			//  You can modify this to check to see if you want to cancel the operation, then return a null here
			return true;
		}

		/// <summary>
		/// Delegate for the EnumChildWindows method
		/// </summary>
		/// <param name="hWnd">Window handle</param>
		/// <param name="parameter">Caller-defined variable; we use it for a pointer to our list</param>
		/// <returns>True to continue enumerating, false to bail.</returns>
		public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
	}
}
