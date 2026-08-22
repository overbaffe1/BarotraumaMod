using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A5 RID: 1189
	internal sealed class WriteOnlyMessage : IWriteMessage
	{
		// Token: 0x17001421 RID: 5153
		// (get) Token: 0x06004ECE RID: 20174 RVA: 0x002AD98C File Offset: 0x002ABB8C
		// (set) Token: 0x06004ECF RID: 20175 RVA: 0x002AD994 File Offset: 0x002ABB94
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

		// Token: 0x17001422 RID: 5154
		// (get) Token: 0x06004ED0 RID: 20176 RVA: 0x002AD99D File Offset: 0x002ABB9D
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x17001423 RID: 5155
		// (get) Token: 0x06004ED1 RID: 20177 RVA: 0x002AD9A7 File Offset: 0x002ABBA7
		public byte[] Buffer
		{
			get
			{
				return this.buf;
			}
		}

		// Token: 0x17001424 RID: 5156
		// (get) Token: 0x06004ED2 RID: 20178 RVA: 0x002AD9AF File Offset: 0x002ABBAF
		// (set) Token: 0x06004ED3 RID: 20179 RVA: 0x002AD9D9 File Offset: 0x002ABBD9
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

		// Token: 0x17001425 RID: 5157
		// (get) Token: 0x06004ED4 RID: 20180 RVA: 0x002ADA15 File Offset: 0x002ABC15
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x06004ED5 RID: 20181 RVA: 0x002ADA21 File Offset: 0x002ABC21
		public void WriteBoolean(bool val)
		{
			MsgWriter.WriteBoolean(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004ED6 RID: 20182 RVA: 0x002ADA3B File Offset: 0x002ABC3B
		public void WritePadBits()
		{
			MsgWriter.WritePadBits(ref this.buf, ref this.seekPos, ref this.lengthBits);
		}

		// Token: 0x06004ED7 RID: 20183 RVA: 0x002ADA54 File Offset: 0x002ABC54
		public void WriteByte(byte val)
		{
			MsgWriter.WriteByte(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004ED8 RID: 20184 RVA: 0x002ADA6E File Offset: 0x002ABC6E
		public void WriteUInt16(ushort val)
		{
			MsgWriter.WriteUInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004ED9 RID: 20185 RVA: 0x002ADA88 File Offset: 0x002ABC88
		public void WriteInt16(short val)
		{
			MsgWriter.WriteInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDA RID: 20186 RVA: 0x002ADAA2 File Offset: 0x002ABCA2
		public void WriteUInt32(uint val)
		{
			MsgWriter.WriteUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDB RID: 20187 RVA: 0x002ADABC File Offset: 0x002ABCBC
		public void WriteInt32(int val)
		{
			MsgWriter.WriteInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDC RID: 20188 RVA: 0x002ADAD6 File Offset: 0x002ABCD6
		public void WriteUInt64(ulong val)
		{
			MsgWriter.WriteUInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDD RID: 20189 RVA: 0x002ADAF0 File Offset: 0x002ABCF0
		public void WriteInt64(long val)
		{
			MsgWriter.WriteInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDE RID: 20190 RVA: 0x002ADB0A File Offset: 0x002ABD0A
		public void WriteSingle(float val)
		{
			MsgWriter.WriteSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EDF RID: 20191 RVA: 0x002ADB24 File Offset: 0x002ABD24
		public void WriteDouble(double val)
		{
			MsgWriter.WriteDouble(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EE0 RID: 20192 RVA: 0x002ADB3E File Offset: 0x002ABD3E
		public void WriteColorR8G8B8(Color val)
		{
			MsgWriter.WriteColorR8G8B8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EE1 RID: 20193 RVA: 0x002ADB58 File Offset: 0x002ABD58
		public void WriteColorR8G8B8A8(Color val)
		{
			MsgWriter.WriteColorR8G8B8A8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EE2 RID: 20194 RVA: 0x002ADB72 File Offset: 0x002ABD72
		public void WriteVariableUInt32(uint val)
		{
			MsgWriter.WriteVariableUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EE3 RID: 20195 RVA: 0x002ADB8C File Offset: 0x002ABD8C
		public void WriteString(string val)
		{
			MsgWriter.WriteString(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004EE4 RID: 20196 RVA: 0x002ADBA6 File Offset: 0x002ABDA6
		public void WriteIdentifier(Identifier val)
		{
			this.WriteString(val.Value);
		}

		// Token: 0x06004EE5 RID: 20197 RVA: 0x002ADBB5 File Offset: 0x002ABDB5
		public void WriteRangedInteger(int val, int min, int max)
		{
			MsgWriter.WriteRangedInteger(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max);
		}

		// Token: 0x06004EE6 RID: 20198 RVA: 0x002ADBD1 File Offset: 0x002ABDD1
		public void WriteRangedSingle(float val, float min, float max, int bitCount)
		{
			MsgWriter.WriteRangedSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max, bitCount);
		}

		// Token: 0x06004EE7 RID: 20199 RVA: 0x002ADBEF File Offset: 0x002ABDEF
		public void WriteBytes(byte[] val, int startPos, int length)
		{
			MsgWriter.WriteBytes(ref this.buf, ref this.seekPos, ref this.lengthBits, val, startPos, length);
		}

		// Token: 0x06004EE8 RID: 20200 RVA: 0x002ADC0C File Offset: 0x002ABE0C
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

		// Token: 0x040029C0 RID: 10688
		private byte[] buf = new byte[256];

		// Token: 0x040029C1 RID: 10689
		private int seekPos;

		// Token: 0x040029C2 RID: 10690
		private int lengthBits;
	}
}
