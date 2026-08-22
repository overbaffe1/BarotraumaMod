using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A8 RID: 936
	internal sealed class WriteOnlyMessage : IWriteMessage
	{
		// Token: 0x17000F26 RID: 3878
		// (get) Token: 0x060036F9 RID: 14073 RVA: 0x00174E50 File Offset: 0x00173050
		// (set) Token: 0x060036FA RID: 14074 RVA: 0x00174E58 File Offset: 0x00173058
		public int BitPosition
		{
			get
			{
				return this.seekPos;
			}
			set
			{
				this.seekPos = value;
			}
		}

		// Token: 0x17000F27 RID: 3879
		// (get) Token: 0x060036FB RID: 14075 RVA: 0x00174E61 File Offset: 0x00173061
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x17000F28 RID: 3880
		// (get) Token: 0x060036FC RID: 14076 RVA: 0x00174E6B File Offset: 0x0017306B
		public byte[] Buffer
		{
			get
			{
				return this.buf;
			}
		}

		// Token: 0x17000F29 RID: 3881
		// (get) Token: 0x060036FD RID: 14077 RVA: 0x00174E73 File Offset: 0x00173073
		// (set) Token: 0x060036FE RID: 14078 RVA: 0x00174E9D File Offset: 0x0017309D
		public int LengthBits
		{
			get
			{
				this.lengthBits = ((this.seekPos > this.lengthBits) ? this.seekPos : this.lengthBits);
				return this.lengthBits;
			}
			set
			{
				this.lengthBits = value;
				this.seekPos = ((this.seekPos > this.lengthBits) ? this.lengthBits : this.seekPos);
				MsgWriter.EnsureBufferSize(ref this.buf, this.lengthBits);
			}
		}

		// Token: 0x17000F2A RID: 3882
		// (get) Token: 0x060036FF RID: 14079 RVA: 0x00174ED9 File Offset: 0x001730D9
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x06003700 RID: 14080 RVA: 0x00174EE5 File Offset: 0x001730E5
		public void WriteBoolean(bool val)
		{
			MsgWriter.WriteBoolean(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003701 RID: 14081 RVA: 0x00174EFF File Offset: 0x001730FF
		public void WritePadBits()
		{
			MsgWriter.WritePadBits(ref this.buf, ref this.seekPos, ref this.lengthBits);
		}

		// Token: 0x06003702 RID: 14082 RVA: 0x00174F18 File Offset: 0x00173118
		public void WriteByte(byte val)
		{
			MsgWriter.WriteByte(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003703 RID: 14083 RVA: 0x00174F32 File Offset: 0x00173132
		public void WriteUInt16(ushort val)
		{
			MsgWriter.WriteUInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003704 RID: 14084 RVA: 0x00174F4C File Offset: 0x0017314C
		public void WriteInt16(short val)
		{
			MsgWriter.WriteInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003705 RID: 14085 RVA: 0x00174F66 File Offset: 0x00173166
		public void WriteUInt32(uint val)
		{
			MsgWriter.WriteUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003706 RID: 14086 RVA: 0x00174F80 File Offset: 0x00173180
		public void WriteInt32(int val)
		{
			MsgWriter.WriteInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003707 RID: 14087 RVA: 0x00174F9A File Offset: 0x0017319A
		public void WriteUInt64(ulong val)
		{
			MsgWriter.WriteUInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003708 RID: 14088 RVA: 0x00174FB4 File Offset: 0x001731B4
		public void WriteInt64(long val)
		{
			MsgWriter.WriteInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003709 RID: 14089 RVA: 0x00174FCE File Offset: 0x001731CE
		public void WriteSingle(float val)
		{
			MsgWriter.WriteSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370A RID: 14090 RVA: 0x00174FE8 File Offset: 0x001731E8
		public void WriteDouble(double val)
		{
			MsgWriter.WriteDouble(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370B RID: 14091 RVA: 0x00175002 File Offset: 0x00173202
		public void WriteColorR8G8B8(Color val)
		{
			MsgWriter.WriteColorR8G8B8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370C RID: 14092 RVA: 0x0017501C File Offset: 0x0017321C
		public void WriteColorR8G8B8A8(Color val)
		{
			MsgWriter.WriteColorR8G8B8A8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370D RID: 14093 RVA: 0x00175036 File Offset: 0x00173236
		public void WriteVariableUInt32(uint val)
		{
			MsgWriter.WriteVariableUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370E RID: 14094 RVA: 0x00175050 File Offset: 0x00173250
		public void WriteString(string val)
		{
			MsgWriter.WriteString(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600370F RID: 14095 RVA: 0x0017506A File Offset: 0x0017326A
		public void WriteIdentifier(Identifier val)
		{
			this.WriteString(val.Value);
		}

		// Token: 0x06003710 RID: 14096 RVA: 0x00175079 File Offset: 0x00173279
		public void WriteRangedInteger(int val, int min, int max)
		{
			MsgWriter.WriteRangedInteger(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max);
		}

		// Token: 0x06003711 RID: 14097 RVA: 0x00175095 File Offset: 0x00173295
		public void WriteRangedSingle(float val, float min, float max, int bitCount)
		{
			MsgWriter.WriteRangedSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max, bitCount);
		}

		// Token: 0x06003712 RID: 14098 RVA: 0x001750B3 File Offset: 0x001732B3
		public void WriteBytes(byte[] val, int startPos, int length)
		{
			MsgWriter.WriteBytes(ref this.buf, ref this.seekPos, ref this.lengthBits, val, startPos, length);
		}

		// Token: 0x06003713 RID: 14099 RVA: 0x001750D0 File Offset: 0x001732D0
		public byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int length)
		{
			byte[] outBuf;
			if (this.LengthBytes <= 1000 || !compressPastThreshold)
			{
				isCompressed = false;
				outBuf = new byte[this.LengthBytes];
				Array.Copy(this.buf, outBuf, this.LengthBytes);
				length = this.LengthBytes;
			}
			else
			{
				using (MemoryStream output = new MemoryStream())
				{
					using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Fastest))
					{
						dstream.Write(this.buf, 0, this.LengthBytes);
					}
					byte[] compressedBuf = output.ToArray();
					if (compressedBuf.Length >= this.LengthBytes)
					{
						isCompressed = false;
						outBuf = new byte[this.LengthBytes];
						Array.Copy(this.buf, outBuf, this.LengthBytes);
						length = this.LengthBytes;
					}
					else
					{
						isCompressed = true;
						outBuf = compressedBuf;
						length = outBuf.Length;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Compressed message: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.LengthBytes);
						defaultInterpolatedStringHandler.AppendLiteral(" to ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(length);
						DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			return outBuf;
		}

		// Token: 0x04001BC9 RID: 7113
		private byte[] buf = new byte[256];

		// Token: 0x04001BCA RID: 7114
		private int seekPos;

		// Token: 0x04001BCB RID: 7115
		private int lengthBits;
	}
}
