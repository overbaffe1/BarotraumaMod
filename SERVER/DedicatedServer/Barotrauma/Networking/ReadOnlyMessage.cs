using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A9 RID: 937
	internal sealed class ReadOnlyMessage : IReadMessage
	{
		// Token: 0x17000F2B RID: 3883
		// (get) Token: 0x06003715 RID: 14101 RVA: 0x00175214 File Offset: 0x00173414
		// (set) Token: 0x06003716 RID: 14102 RVA: 0x0017521C File Offset: 0x0017341C
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

		// Token: 0x17000F2C RID: 3884
		// (get) Token: 0x06003717 RID: 14103 RVA: 0x00175225 File Offset: 0x00173425
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x17000F2D RID: 3885
		// (get) Token: 0x06003718 RID: 14104 RVA: 0x0017522F File Offset: 0x0017342F
		public byte[] Buffer { get; }

		// Token: 0x17000F2E RID: 3886
		// (get) Token: 0x06003719 RID: 14105 RVA: 0x00175237 File Offset: 0x00173437
		// (set) Token: 0x0600371A RID: 14106 RVA: 0x00175261 File Offset: 0x00173461
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

		// Token: 0x17000F2F RID: 3887
		// (get) Token: 0x0600371B RID: 14107 RVA: 0x0017528C File Offset: 0x0017348C
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x17000F30 RID: 3888
		// (get) Token: 0x0600371C RID: 14108 RVA: 0x00175298 File Offset: 0x00173498
		public NetworkConnection Sender { get; }

		// Token: 0x0600371D RID: 14109 RVA: 0x001752A0 File Offset: 0x001734A0
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

		// Token: 0x0600371E RID: 14110 RVA: 0x001754E4 File Offset: 0x001736E4
		public bool ReadBoolean()
		{
			return MsgReader.ReadBoolean(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600371F RID: 14111 RVA: 0x001754F7 File Offset: 0x001736F7
		public void ReadPadBits()
		{
			MsgReader.ReadPadBits(ref this.seekPos);
		}

		// Token: 0x06003720 RID: 14112 RVA: 0x00175504 File Offset: 0x00173704
		public byte ReadByte()
		{
			return MsgReader.ReadByte(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003721 RID: 14113 RVA: 0x00175517 File Offset: 0x00173717
		public byte PeekByte()
		{
			return MsgReader.PeekByte(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003722 RID: 14114 RVA: 0x0017552A File Offset: 0x0017372A
		public ushort ReadUInt16()
		{
			return MsgReader.ReadUInt16(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003723 RID: 14115 RVA: 0x0017553D File Offset: 0x0017373D
		public short ReadInt16()
		{
			return MsgReader.ReadInt16(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003724 RID: 14116 RVA: 0x00175550 File Offset: 0x00173750
		public uint ReadUInt32()
		{
			return MsgReader.ReadUInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003725 RID: 14117 RVA: 0x00175563 File Offset: 0x00173763
		public int ReadInt32()
		{
			return MsgReader.ReadInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003726 RID: 14118 RVA: 0x00175576 File Offset: 0x00173776
		public ulong ReadUInt64()
		{
			return MsgReader.ReadUInt64(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003727 RID: 14119 RVA: 0x00175589 File Offset: 0x00173789
		public long ReadInt64()
		{
			return MsgReader.ReadInt64(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003728 RID: 14120 RVA: 0x0017559C File Offset: 0x0017379C
		public float ReadSingle()
		{
			return MsgReader.ReadSingle(this.Buffer, ref this.seekPos);
		}

		// Token: 0x06003729 RID: 14121 RVA: 0x001755AF File Offset: 0x001737AF
		public double ReadDouble()
		{
			return MsgReader.ReadDouble(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600372A RID: 14122 RVA: 0x001755C2 File Offset: 0x001737C2
		public uint ReadVariableUInt32()
		{
			return MsgReader.ReadVariableUInt32(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600372B RID: 14123 RVA: 0x001755D5 File Offset: 0x001737D5
		public string ReadString()
		{
			return MsgReader.ReadString(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600372C RID: 14124 RVA: 0x001755E8 File Offset: 0x001737E8
		public Identifier ReadIdentifier()
		{
			return this.ReadString().ToIdentifier();
		}

		// Token: 0x0600372D RID: 14125 RVA: 0x001755F5 File Offset: 0x001737F5
		public Color ReadColorR8G8B8()
		{
			return MsgReader.ReadColorR8G8B8(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600372E RID: 14126 RVA: 0x00175608 File Offset: 0x00173808
		public Color ReadColorR8G8B8A8()
		{
			return MsgReader.ReadColorR8G8B8A8(this.Buffer, ref this.seekPos);
		}

		// Token: 0x0600372F RID: 14127 RVA: 0x0017561B File Offset: 0x0017381B
		public int ReadRangedInteger(int min, int max)
		{
			return MsgReader.ReadRangedInteger(this.Buffer, ref this.seekPos, min, max);
		}

		// Token: 0x06003730 RID: 14128 RVA: 0x00175630 File Offset: 0x00173830
		public float ReadRangedSingle(float min, float max, int bitCount)
		{
			return MsgReader.ReadRangedSingle(this.Buffer, ref this.seekPos, min, max, bitCount);
		}

		// Token: 0x06003731 RID: 14129 RVA: 0x00175646 File Offset: 0x00173846
		public byte[] ReadBytes(int numberOfBytes)
		{
			return MsgReader.ReadBytes(this.Buffer, ref this.seekPos, numberOfBytes);
		}

		// Token: 0x04001BCC RID: 7116
		private int seekPos;

		// Token: 0x04001BCD RID: 7117
		private int lengthBits;
	}
}
