using System;
using System.Runtime.InteropServices;

namespace OpenAL
{
	// Token: 0x02000012 RID: 18
	public class Al
	{
		// Token: 0x06000066 RID: 102
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alEnable")]
		public static extern void Enable(int capability);

		// Token: 0x06000067 RID: 103
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDisable")]
		public static extern void Disable(int capability);

		// Token: 0x06000068 RID: 104
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alIsEnabled")]
		public static extern bool IsEnabled(int capability);

		// Token: 0x06000069 RID: 105
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetString")]
		private static extern IntPtr _GetString(int param);

		// Token: 0x0600006A RID: 106 RVA: 0x00005E34 File Offset: 0x00004034
		public static string GetString(int param)
		{
			return Marshal.PtrToStringAnsi(Al._GetString(param));
		}

		// Token: 0x0600006B RID: 107
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetBooleanv")]
		public static extern void GetBooleanv(int param, out bool data);

		// Token: 0x0600006C RID: 108
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetIntegerv")]
		public static extern void GetIntegerv(int param, out int data);

		// Token: 0x0600006D RID: 109
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetFloatv")]
		public static extern void GetFloatv(int param, out float data);

		// Token: 0x0600006E RID: 110
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetDoublev")]
		public static extern void GetDoublev(int param, out double data);

		// Token: 0x0600006F RID: 111
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetBoolean")]
		public static extern bool GetBoolean(int param);

		// Token: 0x06000070 RID: 112
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetInteger")]
		public static extern int GetInteger(int param);

		// Token: 0x06000071 RID: 113
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetFloat")]
		public static extern float GetFloat(int param);

		// Token: 0x06000072 RID: 114
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetDouble")]
		public static extern double GetDouble(int param);

		// Token: 0x06000073 RID: 115
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetError")]
		public static extern int GetError();

		// Token: 0x06000074 RID: 116 RVA: 0x00005E44 File Offset: 0x00004044
		public static string GetErrorString(int error)
		{
			if (error == 0)
			{
				return "No error";
			}
			switch (error)
			{
			case 40961:
				return "Invalid name";
			case 40962:
				return "Invalid enum";
			case 40963:
				return "Invalid value";
			case 40964:
				return "Invalid operation";
			case 40965:
				return "Out of memory";
			default:
				return "Unknown error";
			}
		}

		// Token: 0x06000075 RID: 117
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alIsExtensionPresent")]
		public static extern bool IsExtensionPresent(string extname);

		// Token: 0x06000076 RID: 118
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetProcAddress")]
		public static extern IntPtr GetProcAddress(string fname);

		// Token: 0x06000077 RID: 119
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetEnumValue")]
		public static extern int GetEnumValue(string ename);

		// Token: 0x06000078 RID: 120
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alListenerf")]
		public static extern void Listenerf(int param, float value);

		// Token: 0x06000079 RID: 121
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alListener3f")]
		public static extern void Listener3f(int param, float value1, float value2, float value3);

		// Token: 0x0600007A RID: 122
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alListenerfv")]
		public static extern void Listenerfv(int param, float[] values);

		// Token: 0x0600007B RID: 123
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetListenerf")]
		public static extern void GetListenerf(int param, out float value);

		// Token: 0x0600007C RID: 124
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetListener3f")]
		public static extern void GetListener3f(int param, out float value1, out float value2, out float value3);

		// Token: 0x0600007D RID: 125
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetListenerfv")]
		private static extern void _GetListenerfv(int param, IntPtr values);

		// Token: 0x0600007E RID: 126 RVA: 0x00005EA0 File Offset: 0x000040A0
		public static void GetListenerfv(int param, out float[] values)
		{
			int len;
			if (param <= 4102)
			{
				if (param == 4100 || param == 4102)
				{
					len = 3;
					goto IL_3A;
				}
			}
			else
			{
				if (param == 4106)
				{
					len = 1;
					goto IL_3A;
				}
				if (param == 4111)
				{
					len = 6;
					goto IL_3A;
				}
			}
			len = 0;
			IL_3A:
			values = new float[len];
			GCHandle arrayHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Al._GetListenerfv(param, arrayHandle.AddrOfPinnedObject());
			arrayHandle.Free();
		}

		// Token: 0x0600007F RID: 127
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGenSources")]
		private static extern void _GenSources(int n, IntPtr sources);

		// Token: 0x06000080 RID: 128 RVA: 0x00005F0C File Offset: 0x0000410C
		public static void GenSources(int n, out uint[] sources)
		{
			sources = new uint[n];
			GCHandle arrayHandle = GCHandle.Alloc(sources, GCHandleType.Pinned);
			Al._GenSources(n, arrayHandle.AddrOfPinnedObject());
			arrayHandle.Free();
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00005F40 File Offset: 0x00004140
		public static void GenSource(out uint source)
		{
			uint[] sources;
			Al.GenSources(1, out sources);
			source = sources[0];
		}

		// Token: 0x06000082 RID: 130
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDeleteSources")]
		public static extern void DeleteSources(int n, uint[] sources);

		// Token: 0x06000083 RID: 131 RVA: 0x00005F5A File Offset: 0x0000415A
		public static void DeleteSource(uint source)
		{
			Al.DeleteSources(1, new uint[]
			{
				source
			});
		}

		// Token: 0x06000084 RID: 132
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alIsSource")]
		public static extern bool IsSource(uint sid);

		// Token: 0x06000085 RID: 133
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcef")]
		public static extern void Sourcef(uint sid, int param, float value);

		// Token: 0x06000086 RID: 134
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSource3f")]
		public static extern void Source3f(uint sid, int param, float value1, float value2, float value3);

		// Token: 0x06000087 RID: 135
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcefv")]
		public static extern void Sourcefv(uint sid, int param, float[] values);

		// Token: 0x06000088 RID: 136
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcei")]
		public static extern void Sourcei(uint sid, int param, int value);

		// Token: 0x06000089 RID: 137
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSource3i")]
		public static extern void Source3i(uint sid, int param, int value1, int value2, int value3);

		// Token: 0x0600008A RID: 138
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceiv")]
		public static extern void Sourceiv(uint sid, int param, int[] values);

		// Token: 0x0600008B RID: 139
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSourcef")]
		public static extern void GetSourcef(uint sid, int param, out float value);

		// Token: 0x0600008C RID: 140
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSource3f")]
		public static extern void GetSource3f(uint sid, int param, out float value1, out float value2, out float value3);

		// Token: 0x0600008D RID: 141
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSourcefv")]
		private static extern void _GetSourcefv(uint sid, int param, IntPtr values);

		// Token: 0x0600008E RID: 142 RVA: 0x00005F6C File Offset: 0x0000416C
		public static void GetSourcefv(uint sid, int param, out float[] values)
		{
			int len;
			switch (param)
			{
			case 4097:
			case 4098:
			case 4099:
			case 4106:
			case 4109:
			case 4110:
				break;
			case 4100:
			case 4101:
			case 4102:
				len = 3;
				goto IL_58;
			case 4103:
			case 4104:
			case 4105:
			case 4107:
			case 4108:
				goto IL_56;
			default:
				if (param - 4128 > 6)
				{
					goto IL_56;
				}
				break;
			}
			len = 1;
			goto IL_58;
			IL_56:
			len = 0;
			IL_58:
			values = new float[len];
			GCHandle arrayHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Al._GetSourcefv(sid, param, arrayHandle.AddrOfPinnedObject());
			arrayHandle.Free();
		}

		// Token: 0x0600008F RID: 143
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSourcei")]
		public static extern void GetSourcei(uint sid, int param, out int value);

		// Token: 0x06000090 RID: 144
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSource3i")]
		public static extern void GetSource3i(uint sid, int param, out int value1, out int value2, out int value3);

		// Token: 0x06000091 RID: 145
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetSourceiv")]
		private static extern void _GetSourceiv(uint sid, int param, IntPtr values);

		// Token: 0x06000092 RID: 146 RVA: 0x00005FF8 File Offset: 0x000041F8
		public static void GetSourceiv(uint sid, int param, out int[] values)
		{
			int len;
			if (param <= 4112)
			{
				if (param != 514)
				{
					switch (param)
					{
					case 4097:
					case 4098:
					case 4103:
					case 4105:
						break;
					case 4099:
					case 4100:
					case 4102:
					case 4104:
						goto IL_70;
					case 4101:
						len = 3;
						goto IL_72;
					default:
						if (param != 4112)
						{
							goto IL_70;
						}
						break;
					}
				}
			}
			else if (param - 4117 > 1 && param - 4128 > 1 && param - 4131 > 4)
			{
				goto IL_70;
			}
			len = 1;
			goto IL_72;
			IL_70:
			len = 0;
			IL_72:
			values = new int[len];
			GCHandle arrayHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Al._GetSourceiv(sid, param, arrayHandle.AddrOfPinnedObject());
			arrayHandle.Free();
		}

		// Token: 0x06000093 RID: 147
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcePlayv")]
		public static extern void SourcePlayv(int ns, uint[] sids);

		// Token: 0x06000094 RID: 148
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceStopv")]
		public static extern void SourceStopv(int ns, uint[] sids);

		// Token: 0x06000095 RID: 149
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceRewindv")]
		public static extern void SourceRewindv(int ns, uint[] sids);

		// Token: 0x06000096 RID: 150
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcePausev")]
		public static extern void SourcePausev(int ns, uint[] sids);

		// Token: 0x06000097 RID: 151
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcePlay")]
		public static extern void SourcePlay(uint sid);

		// Token: 0x06000098 RID: 152
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceStop")]
		public static extern void SourceStop(uint sid);

		// Token: 0x06000099 RID: 153
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceRewind")]
		public static extern void SourceRewind(uint sid);

		// Token: 0x0600009A RID: 154
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourcePause")]
		public static extern void SourcePause(uint sid);

		// Token: 0x0600009B RID: 155
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceQueueBuffers")]
		public static extern void SourceQueueBuffers(uint sid, int numEntries, uint[] bids);

		// Token: 0x0600009C RID: 156 RVA: 0x000060A0 File Offset: 0x000042A0
		public static void SourceQueueBuffer(uint sid, uint bid)
		{
			Al.SourceQueueBuffers(sid, 1, new uint[]
			{
				bid
			});
		}

		// Token: 0x0600009D RID: 157
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSourceUnqueueBuffers")]
		public static extern void SourceUnqueueBuffers(uint sid, int numEntries, uint[] bids);

		// Token: 0x0600009E RID: 158
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGenBuffers")]
		private static extern void _GenBuffers(int n, IntPtr buffers);

		// Token: 0x0600009F RID: 159 RVA: 0x000060C0 File Offset: 0x000042C0
		public static void GenBuffers(int n, out uint[] buffers)
		{
			buffers = new uint[n];
			GCHandle arrayHandle = GCHandle.Alloc(buffers, GCHandleType.Pinned);
			Al._GenBuffers(n, arrayHandle.AddrOfPinnedObject());
			arrayHandle.Free();
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x000060F4 File Offset: 0x000042F4
		public static void GenBuffer(out uint buffer)
		{
			uint[] buffers;
			Al.GenBuffers(1, out buffers);
			buffer = buffers[0];
		}

		// Token: 0x060000A1 RID: 161
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDeleteBuffers")]
		public static extern void DeleteBuffers(int n, uint[] buffers);

		// Token: 0x060000A2 RID: 162 RVA: 0x0000610E File Offset: 0x0000430E
		public static void DeleteBuffer(uint buffer)
		{
			Al.DeleteBuffers(1, new uint[]
			{
				buffer
			});
		}

		// Token: 0x060000A3 RID: 163
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alIsBuffer")]
		public static extern bool IsBuffer(uint bid);

		// Token: 0x060000A4 RID: 164
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alBufferData")]
		public static extern void BufferData(uint bid, int format, IntPtr data, int size, int freq);

		// Token: 0x060000A5 RID: 165 RVA: 0x00006120 File Offset: 0x00004320
		public static void BufferData<T>(uint bid, int format, T[] data, int len, int freq)
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			Al.BufferData(bid, format, handle.AddrOfPinnedObject(), len, freq);
			handle.Free();
		}

		// Token: 0x060000A6 RID: 166
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alBufferi")]
		public static extern void Bufferi(uint bid, int param, int value);

		// Token: 0x060000A7 RID: 167
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alGetBufferi")]
		public static extern void GetBufferi(uint bid, int param, out int value);

		// Token: 0x060000A8 RID: 168
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDopplerFactor")]
		public static extern void DopplerFactor(float value);

		// Token: 0x060000A9 RID: 169
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDopplerVelocity")]
		public static extern void DopplerVelocity(float value);

		// Token: 0x060000AA RID: 170
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alSpeedOfSound")]
		public static extern void SpeedOfSound(float value);

		// Token: 0x060000AB RID: 171
		[DllImport("soft_oal_x64.dll", CallingConvention = 2, EntryPoint = "alDistanceModel")]
		public static extern void DistanceModel(int distanceModel);

		// Token: 0x04000061 RID: 97
		public const string OpenAlDll = "soft_oal_x64.dll";

		// Token: 0x04000062 RID: 98
		public const int None = 0;

		// Token: 0x04000063 RID: 99
		public const int False = 0;

		// Token: 0x04000064 RID: 100
		public const int True = 1;

		// Token: 0x04000065 RID: 101
		public const int SourceRelative = 514;

		// Token: 0x04000066 RID: 102
		public const int ConeInnerAngle = 4097;

		// Token: 0x04000067 RID: 103
		public const int ConeOuterAngle = 4098;

		// Token: 0x04000068 RID: 104
		public const int Pitch = 4099;

		// Token: 0x04000069 RID: 105
		public const int Position = 4100;

		// Token: 0x0400006A RID: 106
		public const int Direction = 4101;

		// Token: 0x0400006B RID: 107
		public const int Velocity = 4102;

		// Token: 0x0400006C RID: 108
		public const int Looping = 4103;

		// Token: 0x0400006D RID: 109
		public const int Buffer = 4105;

		// Token: 0x0400006E RID: 110
		public const int Gain = 4106;

		// Token: 0x0400006F RID: 111
		public const int MinGain = 4109;

		// Token: 0x04000070 RID: 112
		public const int MaxGain = 4110;

		// Token: 0x04000071 RID: 113
		public const int Orientation = 4111;

		// Token: 0x04000072 RID: 114
		public const int SourceState = 4112;

		// Token: 0x04000073 RID: 115
		public const int Initial = 4113;

		// Token: 0x04000074 RID: 116
		public const int Playing = 4114;

		// Token: 0x04000075 RID: 117
		public const int Paused = 4115;

		// Token: 0x04000076 RID: 118
		public const int Stopped = 4116;

		// Token: 0x04000077 RID: 119
		public const int BuffersQueued = 4117;

		// Token: 0x04000078 RID: 120
		public const int BuffersProcessed = 4118;

		// Token: 0x04000079 RID: 121
		public const int SecOffset = 4132;

		// Token: 0x0400007A RID: 122
		public const int SampleOffset = 4133;

		// Token: 0x0400007B RID: 123
		public const int ByteOffset = 4134;

		// Token: 0x0400007C RID: 124
		public const int SourceType = 4135;

		// Token: 0x0400007D RID: 125
		public const int Static = 4136;

		// Token: 0x0400007E RID: 126
		public const int Streaming = 4137;

		// Token: 0x0400007F RID: 127
		public const int Undetermined = 4144;

		// Token: 0x04000080 RID: 128
		public const int FormatMono8 = 4352;

		// Token: 0x04000081 RID: 129
		public const int FormatMono16 = 4353;

		// Token: 0x04000082 RID: 130
		public const int FormatStereo8 = 4354;

		// Token: 0x04000083 RID: 131
		public const int FormatStereo16 = 4355;

		// Token: 0x04000084 RID: 132
		public const int ReferenceDistance = 4128;

		// Token: 0x04000085 RID: 133
		public const int RolloffFactor = 4129;

		// Token: 0x04000086 RID: 134
		public const int ConeOuterGain = 4130;

		// Token: 0x04000087 RID: 135
		public const int MaxDistance = 4131;

		// Token: 0x04000088 RID: 136
		public const int Frequency = 8193;

		// Token: 0x04000089 RID: 137
		public const int Bits = 8194;

		// Token: 0x0400008A RID: 138
		public const int Channels = 8195;

		// Token: 0x0400008B RID: 139
		public const int Size = 8196;

		// Token: 0x0400008C RID: 140
		public const int Unused = 8208;

		// Token: 0x0400008D RID: 141
		public const int Pending = 8209;

		// Token: 0x0400008E RID: 142
		public const int Processed = 8210;

		// Token: 0x0400008F RID: 143
		public const int NoError = 0;

		// Token: 0x04000090 RID: 144
		public const int InvalidName = 40961;

		// Token: 0x04000091 RID: 145
		public const int InvalidEnum = 40962;

		// Token: 0x04000092 RID: 146
		public const int InvalidValue = 40963;

		// Token: 0x04000093 RID: 147
		public const int InvalidOperation = 40964;

		// Token: 0x04000094 RID: 148
		public const int OutOfMemory = 40965;

		// Token: 0x04000095 RID: 149
		public const int Vendor = 45057;

		// Token: 0x04000096 RID: 150
		public const int Version = 45058;

		// Token: 0x04000097 RID: 151
		public const int Renderer = 45059;

		// Token: 0x04000098 RID: 152
		public const int Extensions = 45060;

		// Token: 0x04000099 RID: 153
		public const int EnumDopplerFactor = 49152;

		// Token: 0x0400009A RID: 154
		public const int EnumDopplerVelocity = 49153;

		// Token: 0x0400009B RID: 155
		public const int EnumSpeedOfSound = 49155;

		// Token: 0x0400009C RID: 156
		public const int EnumDistanceModel = 53248;

		// Token: 0x0400009D RID: 157
		public const int InverseDistance = 53249;

		// Token: 0x0400009E RID: 158
		public const int InverseDistanceClamped = 53250;

		// Token: 0x0400009F RID: 159
		public const int LinearDistance = 53251;

		// Token: 0x040000A0 RID: 160
		public const int LinearDistanceClamped = 53252;

		// Token: 0x040000A1 RID: 161
		public const int ExponentDistance = 53253;

		// Token: 0x040000A2 RID: 162
		public const int ExponentDistanceClamped = 53254;
	}
}
