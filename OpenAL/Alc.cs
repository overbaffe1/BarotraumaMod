using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenAL
{
	// Token: 0x02000013 RID: 19
	public class Alc
	{
		// Token: 0x060000AD RID: 173
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcSetErrorReasonCallback")]
		private static extern void SetErrorReasonCallback(IntPtr callback);

		// Token: 0x060000AE RID: 174 RVA: 0x00006158 File Offset: 0x00004358
		public static void SetErrorReasonCallback(Alc.ErrorReasonCallback callback)
		{
			Alc.CurrentErrorReasonCallback = delegate(IntPtr cstr)
			{
				int strLen = 0;
				while (Marshal.ReadByte(cstr, strLen) != 0)
				{
					strLen++;
				}
				byte[] bytes = new byte[strLen];
				Marshal.Copy(cstr, bytes, 0, strLen);
				string csStr = Encoding.UTF8.GetString(bytes);
				Alc.ErrorReasonCallback callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(csStr);
			};
			Alc.CurrentErrorReasonCallbackPtr = Marshal.GetFunctionPointerForDelegate<Alc.ErrorReasonCallbackInternal>(Alc.CurrentErrorReasonCallback);
			Alc.SetErrorReasonCallback(Alc.CurrentErrorReasonCallbackPtr);
		}

		// Token: 0x060000AF RID: 175
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCreateContext")]
		private static extern IntPtr _CreateContext(IntPtr device, IntPtr attrlist);

		// Token: 0x060000B0 RID: 176 RVA: 0x0000619C File Offset: 0x0000439C
		public static IntPtr CreateContext(IntPtr device, int[] attrList)
		{
			GCHandle handle = GCHandle.Alloc(attrList, GCHandleType.Pinned);
			IntPtr retVal = Alc._CreateContext(device, handle.AddrOfPinnedObject());
			handle.Free();
			return retVal;
		}

		// Token: 0x060000B1 RID: 177
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcMakeContextCurrent")]
		public static extern bool MakeContextCurrent(IntPtr context);

		// Token: 0x060000B2 RID: 178
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcProcessContext")]
		public static extern void ProcessContext(IntPtr context);

		// Token: 0x060000B3 RID: 179
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcSuspendContext")]
		public static extern void SuspendContext(IntPtr context);

		// Token: 0x060000B4 RID: 180
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcDestroyContext")]
		public static extern void DestroyContext(IntPtr context);

		// Token: 0x060000B5 RID: 181
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetCurrentContext")]
		public static extern IntPtr GetCurrentContext();

		// Token: 0x060000B6 RID: 182
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetContextsDevice")]
		public static extern IntPtr GetContextsDevice(IntPtr context);

		// Token: 0x060000B7 RID: 183
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcOpenDevice")]
		private static extern IntPtr OpenDevice(IntPtr deviceName);

		// Token: 0x060000B8 RID: 184 RVA: 0x000061C8 File Offset: 0x000043C8
		public static IntPtr OpenDevice(string deviceName)
		{
			if (deviceName == null)
			{
				return Alc.OpenDevice(IntPtr.Zero);
			}
			byte[] devicenameBytes = Encoding.UTF8.GetBytes(deviceName + "\0");
			GCHandle devicenameHandle = GCHandle.Alloc(devicenameBytes, GCHandleType.Pinned);
			IntPtr retVal = Alc.OpenDevice(devicenameHandle.AddrOfPinnedObject());
			devicenameHandle.Free();
			return retVal;
		}

		// Token: 0x060000B9 RID: 185
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCloseDevice")]
		public static extern bool CloseDevice(IntPtr device);

		// Token: 0x060000BA RID: 186
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetError")]
		public static extern int GetError(IntPtr device);

		// Token: 0x060000BB RID: 187 RVA: 0x00006218 File Offset: 0x00004418
		public static string GetErrorString(int errorCode)
		{
			if (errorCode == 0)
			{
				return "No error";
			}
			switch (errorCode)
			{
			case 40961:
				return "Invalid device";
			case 40962:
				return "Invalid context";
			case 40963:
				return "Invalid enum";
			case 40964:
				return "Invalid value";
			case 40965:
				return "Out of memory";
			default:
				return "Unknown error";
			}
		}

		// Token: 0x060000BC RID: 188
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcIsExtensionPresent")]
		public static extern bool IsExtensionPresent(IntPtr device, string extname);

		// Token: 0x060000BD RID: 189
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetProcAddress")]
		public static extern IntPtr GetProcAddress(IntPtr device, string funcname);

		// Token: 0x060000BE RID: 190
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetEnumValue")]
		public static extern int GetEnumValue(IntPtr device, string enumname);

		// Token: 0x060000BF RID: 191
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetString")]
		private static extern IntPtr _GetString(IntPtr device, int param);

		// Token: 0x060000C0 RID: 192 RVA: 0x00006274 File Offset: 0x00004474
		public static string GetString(IntPtr device, int param)
		{
			IntPtr strPtr = Alc._GetString(device, param);
			int strLen = 0;
			while (Marshal.ReadByte(strPtr, strLen) != 0)
			{
				strLen++;
			}
			byte[] bytes = new byte[strLen];
			Marshal.Copy(strPtr, bytes, 0, strLen);
			return Encoding.UTF8.GetString(bytes);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000062B8 File Offset: 0x000044B8
		public static IReadOnlyList<string> GetStringList(IntPtr device, int param)
		{
			List<string> retVal = new List<string>();
			IntPtr strPtr = Alc._GetString(device, param);
			if (strPtr == IntPtr.Zero)
			{
				return retVal;
			}
			int strStart = 0;
			int strEnd = 0;
			byte currChar = Marshal.ReadByte(strPtr, strEnd);
			if (currChar == 0)
			{
				return retVal;
			}
			for (;;)
			{
				strEnd++;
				byte prevChar = currChar;
				currChar = Marshal.ReadByte(strPtr, strEnd);
				if (currChar == 0)
				{
					if (prevChar == 0)
					{
						break;
					}
					byte[] bytes = new byte[strEnd - strStart];
					Marshal.Copy(strPtr + (IntPtr)strStart, bytes, 0, strEnd - strStart);
					retVal.Add(Encoding.UTF8.GetString(bytes));
					strStart = strEnd + 1;
				}
			}
			return retVal;
		}

		// Token: 0x060000C2 RID: 194
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcGetIntegerv")]
		public static extern void GetIntegerv(IntPtr device, int param, int size, IntPtr data);

		// Token: 0x060000C3 RID: 195 RVA: 0x00006344 File Offset: 0x00004544
		public static void GetInteger(IntPtr device, int param, out int data)
		{
			data = 0;
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			Alc.GetIntegerv(device, param, 1, handle.AddrOfPinnedObject());
			data = Marshal.ReadInt32(handle.AddrOfPinnedObject());
			handle.Free();
		}

		// Token: 0x060000C4 RID: 196
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCaptureOpenDevice")]
		private static extern IntPtr CaptureOpenDevice(IntPtr devicename, uint frequency, int format, int buffersize);

		// Token: 0x060000C5 RID: 197 RVA: 0x00006388 File Offset: 0x00004588
		public static IntPtr CaptureOpenDevice(string devicename, uint frequency, int format, int buffersize)
		{
			byte[] devicenameBytes = Encoding.UTF8.GetBytes(devicename + "\0");
			GCHandle devicenameHandle = GCHandle.Alloc(devicenameBytes, GCHandleType.Pinned);
			IntPtr retVal = Alc.CaptureOpenDevice(devicenameHandle.AddrOfPinnedObject(), frequency, format, buffersize);
			devicenameHandle.Free();
			return retVal;
		}

		// Token: 0x060000C6 RID: 198
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCaptureCloseDevice")]
		public static extern bool CaptureCloseDevice(IntPtr device);

		// Token: 0x060000C7 RID: 199
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCaptureStart")]
		public static extern void CaptureStart(IntPtr device);

		// Token: 0x060000C8 RID: 200
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCaptureStop")]
		public static extern void CaptureStop(IntPtr device);

		// Token: 0x060000C9 RID: 201
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alcCaptureSamples")]
		public static extern void CaptureSamples(IntPtr device, IntPtr buffer, int samples);

		// Token: 0x040000A3 RID: 163
		public const string OpenAlDll = "soft_oal_x64.dll";

		// Token: 0x040000A4 RID: 164
		private static Alc.ErrorReasonCallbackInternal CurrentErrorReasonCallback;

		// Token: 0x040000A5 RID: 165
		private static IntPtr CurrentErrorReasonCallbackPtr;

		// Token: 0x040000A6 RID: 166
		public const int False = 0;

		// Token: 0x040000A7 RID: 167
		public const int True = 1;

		// Token: 0x040000A8 RID: 168
		public const int Frequency = 4103;

		// Token: 0x040000A9 RID: 169
		public const int Refresh = 4104;

		// Token: 0x040000AA RID: 170
		public const int Sync = 4105;

		// Token: 0x040000AB RID: 171
		public const int MonoSources = 4112;

		// Token: 0x040000AC RID: 172
		public const int StereoSources = 4113;

		// Token: 0x040000AD RID: 173
		public const int NoError = 0;

		// Token: 0x040000AE RID: 174
		public const int InvalidDevice = 40961;

		// Token: 0x040000AF RID: 175
		public const int InvalidContext = 40962;

		// Token: 0x040000B0 RID: 176
		public const int InvalidEnum = 40963;

		// Token: 0x040000B1 RID: 177
		public const int InvalidValue = 40964;

		// Token: 0x040000B2 RID: 178
		public const int OutOfMemory = 40965;

		// Token: 0x040000B3 RID: 179
		public const int DefaultDeviceSpecifier = 4100;

		// Token: 0x040000B4 RID: 180
		public const int DeviceSpecifier = 4101;

		// Token: 0x040000B5 RID: 181
		public const int Extensions = 4102;

		// Token: 0x040000B6 RID: 182
		public const int MajorVersion = 4096;

		// Token: 0x040000B7 RID: 183
		public const int MinorVersion = 4097;

		// Token: 0x040000B8 RID: 184
		public const int AttributesSize = 4098;

		// Token: 0x040000B9 RID: 185
		public const int AllAttributes = 4099;

		// Token: 0x040000BA RID: 186
		public const int DefaultAllDevicesSpecifier = 4114;

		// Token: 0x040000BB RID: 187
		public const int AllDevicesSpecifier = 4115;

		// Token: 0x040000BC RID: 188
		public const int CaptureDeviceSpecifier = 784;

		// Token: 0x040000BD RID: 189
		public const int CaptureDefaultDeviceSpecifier = 785;

		// Token: 0x040000BE RID: 190
		public const int EnumCaptureSamples = 786;

		// Token: 0x040000BF RID: 191
		public const int EnumConnected = 787;

		// Token: 0x040000C0 RID: 192
		public const int OutputDevicesSpecifier = 4115;

		// Token: 0x02000635 RID: 1589
		// (Invoke) Token: 0x0600650B RID: 25867
		public delegate void ErrorReasonCallback(string str);

		// Token: 0x02000636 RID: 1590
		// (Invoke) Token: 0x0600650F RID: 25871
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ErrorReasonCallbackInternal(IntPtr cstr);
	}
}
