using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A6 RID: 1190
	internal sealed class ReadOnlyMessage : IReadMessage
	{
		// Token: 0x17001426 RID: 5158
		// (get) Token: 0x06004EEA RID: 20202 RVA: 0x002ADD50 File Offset: 0x002ABF50
		// (set) Token: 0x06004EEB RID: 20203 RVA: 0x002ADD58 File Offset: 0x002ABF58
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

		// Token: 0x17001427 RID: 5159
		// (get) Token: 0x06004EEC RID: 20204 RVA: 0x002ADD61 File Offset: 0x002ABF61
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x17001428 RID: 5160
		// (get) Token: 0x06004EED RID: 20205 RVA: 0x002ADD6B File Offset: 0x002ABF6B
		public byte[] Buffer { get; }

		// Token: 0x17001429 RID: 5161
		// (get) Token: 0x06004EEE RID: 20206 RVA: 0x002ADD73 File Offset: 0x002ABF73
		// (set) Token: 0x06004EEF RID: 20207 RVA: 0x002ADD9D File Offset: 0x002ABF9D
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
			}
		}

		// Token: 0x1700142A RID: 5162
		// (get) Token: 0x06004EF0 RID: 20208 RVA: 0x002ADDC8 File Offset: 0x002ABFC8
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x1700142B RID: 5163
		// (get) Token: 0x06004EF1 RID: 20209 RVA: 0x002ADDD4 File Offset: 0x002ABFD4
		public NetworkConnection Sender { get; }

		// Token: 0x06004EF2 RID: 20210 RVA: 0x002ADDDC File Offset: 0x002ABFDC
		public ReadOnlyMessage(byte[] inBuf, bool isCompressed, int startPos, int byteLength, NetworkConnection sender)
		{
			this.Sender = sender;
			if (isCompressed)
			{
				byte[] decompressedData;
				using (MemoryStream input = new MemoryStream(inBuf, startPos, byteLength))
				{
					using (MemoryStream output = new MemoryStream())
					{
						using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
						{
							dstream.CopyTo(output);
						}
						decompressedData = output.ToArray();
					}
				}
				this.Buffer = new byte[decompressedData.Length];
				try
				{
					Array.Copy(decompressedData, 0, this.Buffer, 0, decompressedData.Length);
				}
				catch (ArgumentException e)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(127, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to copy the incoming compressed buffer. Source buffer length: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(decompressedData.Length);
					defaultInterpolatedStringHandler.AppendLiteral(", start position: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(0);
					defaultInterpolatedStringHandler.AppendLiteral(", length: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(decompressedData.Length);
					defaultInterpolatedStringHandler.AppendLiteral(", destination buffer length: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.Buffer.Length);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear(), e);
				}
				this.lengthBits = decompressedData.Length * 8;
				DebugConsole.Log("Decompressing message: " + byteLength.ToString() + " to " + this.LengthBytes.ToString());
			}
			else
			{
				this.Buffer = new byte[inBuf.Length];
				try
				{
					Array.Copy(inBuf, startPos, this.Buffer, 0, byteLength);
				}
				catch (ArgumentException e2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(129, 4);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to copy the incoming uncompressed buffer. Source buffer length: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(inBuf.Length);
					defaultInterpolatedStringHandler2.AppendLiteral(", start position: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(startPos);
					defaultInterpolatedStringHandler2.AppendLiteral(", length: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(byteLength);
					defaultInterpolatedStringHandler2.AppendLiteral(", destination buffer length: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Buffer.Length);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					throw new ArgumentException(defaultInterpolatedStringHandler2.ToStringAndClear(), e2);
				}
				this.lengthBits = byteLength * 8;
			}
			this.seekPos = 0;
		}

		// Token: 0x06004EF3 RID: 20211 RVA: 0x002AE020 File Offset: 0x002AC220
		public bool ReadBoolean()
		{
			return MsgReader.ReadBoolean(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EF4 RID: 20212 RVA: 0x002AE033 File Offset: 0x002AC233
		public void ReadPadBits()
		{
			MsgReader.ReadPadBits(ref this.seekPos);
		}

		// Token: 0x06004EF5 RID: 20213 RVA: 0x002AE040 File Offset: 0x002AC240
		public byte ReadByte()
		{
			return MsgReader.ReadByte(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EF6 RID: 20214 RVA: 0x002AE053 File Offset: 0x002AC253
		public byte PeekByte()
		{
			return MsgReader.PeekByte(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EF7 RID: 20215 RVA: 0x002AE066 File Offset: 0x002AC266
		public ushort ReadUInt16()
		{
			return MsgReader.ReadUInt16(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EF8 RID: 20216 RVA: 0x002AE079 File Offset: 0x002AC279
		public short ReadInt16()
		{
			return MsgReader.ReadInt16(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EF9 RID: 20217 RVA: 0x002AE08C File Offset: 0x002AC28C
		public uint ReadUInt32()
		{
			return MsgReader.ReadUInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFA RID: 20218 RVA: 0x002AE09F File Offset: 0x002AC29F
		public int ReadInt32()
		{
			return MsgReader.ReadInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFB RID: 20219 RVA: 0x002AE0B2 File Offset: 0x002AC2B2
		public ulong ReadUInt64()
		{
			return MsgReader.ReadUInt64(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFC RID: 20220 RVA: 0x002AE0C5 File Offset: 0x002AC2C5
		public long ReadInt64()
		{
			return MsgReader.ReadInt64(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFD RID: 20221 RVA: 0x002AE0D8 File Offset: 0x002AC2D8
		public float ReadSingle()
		{
			return MsgReader.ReadSingle(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFE RID: 20222 RVA: 0x002AE0EB File Offset: 0x002AC2EB
		public double ReadDouble()
		{
			return MsgReader.ReadDouble(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004EFF RID: 20223 RVA: 0x002AE0FE File Offset: 0x002AC2FE
		public uint ReadVariableUInt32()
		{
			return MsgReader.ReadVariableUInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004F00 RID: 20224 RVA: 0x002AE111 File Offset: 0x002AC311
		public string ReadString()
		{
			return MsgReader.ReadString(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004F01 RID: 20225 RVA: 0x002AE124 File Offset: 0x002AC324
		public Identifier ReadIdentifier()
		{
			return this.ReadString().ToIdentifier();
		}

		// Token: 0x06004F02 RID: 20226 RVA: 0x002AE131 File Offset: 0x002AC331
		public Color ReadColorR8G8B8()
		{
			return MsgReader.ReadColorR8G8B8(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004F03 RID: 20227 RVA: 0x002AE144 File Offset: 0x002AC344
		public Color ReadColorR8G8B8A8()
		{
			return MsgReader.ReadColorR8G8B8A8(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06004F04 RID: 20228 RVA: 0x002AE157 File Offset: 0x002AC357
		public int ReadRangedInteger(int min, int max)
		{
			return MsgReader.ReadRangedInteger(this.Buffer, ref this.seekPos, min, max);
		}

		// Token: 0x06004F05 RID: 20229 RVA: 0x002AE16C File Offset: 0x002AC36C
		public float ReadRangedSingle(float min, float max, int bitCount)
		{
			return MsgReader.ReadRangedSingle(this.Buffer, ref this.seekPos, min, max, bitCount);
		}

		// Token: 0x06004F06 RID: 20230 RVA: 0x002AE182 File Offset: 0x002AC382
		public byte[] ReadBytes(int numberOfBytes)
		{
			return MsgReader.ReadBytes(this.Buffer, ref this.seekPos, numberOfBytes);
		}

		// Token: 0x040029C3 RID: 10691
		private int seekPos;

		// Token: 0x040029C4 RID: 10692
		private int lengthBits;
	}
}
