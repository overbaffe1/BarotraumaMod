using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A7 RID: 1191
	internal sealed class ReadWriteMessage : IWriteMessage, IReadMessage
	{
		// Token: 0x06004F07 RID: 20231 RVA: 0x002AE196 File Offset: 0x002AC396
		public ReadWriteMessage()
		{
			this.buf = new byte[256];
			this.seekPos = 0;
			this.lengthBits = 0;
		}

		// Token: 0x06004F08 RID: 20232 RVA: 0x002AE1BC File Offset: 0x002AC3BC
		public ReadWriteMessage(byte[] b, int bitPos, int lBits, bool copyBuf)
		{
			this.buf = (copyBuf ? ((byte[])b.Clone()) : b);
			this.seekPos = bitPos;
			this.lengthBits = lBits;
		}

		// Token: 0x1700142C RID: 5164
		// (get) Token: 0x06004F09 RID: 20233 RVA: 0x002AE1EA File Offset: 0x002AC3EA
		// (set) Token: 0x06004F0A RID: 20234 RVA: 0x002AE1F2 File Offset: 0x002AC3F2
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

		// Token: 0x1700142D RID: 5165
		// (get) Token: 0x06004F0B RID: 20235 RVA: 0x002AE1FB File Offset: 0x002AC3FB
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x1700142E RID: 5166
		// (get) Token: 0x06004F0C RID: 20236 RVA: 0x002AE205 File Offset: 0x002AC405
		public byte[] Buffer
		{
			get
			{
				return this.buf;
			}
		}

		// Token: 0x1700142F RID: 5167
		// (get) Token: 0x06004F0D RID: 20237 RVA: 0x002AE20D File Offset: 0x002AC40D
		// (set) Token: 0x06004F0E RID: 20238 RVA: 0x002AE237 File Offset: 0x002AC437
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

		// Token: 0x17001430 RID: 5168
		// (get) Token: 0x06004F0F RID: 20239 RVA: 0x002AE262 File Offset: 0x002AC462
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x17001431 RID: 5169
		// (get) Token: 0x06004F10 RID: 20240 RVA: 0x002AE26E File Offset: 0x002AC46E
		public NetworkConnection Sender
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06004F11 RID: 20241 RVA: 0x002AE271 File Offset: 0x002AC471
		public void WriteBoolean(bool val)
		{
			MsgWriter.WriteBoolean(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F12 RID: 20242 RVA: 0x002AE28B File Offset: 0x002AC48B
		public void WritePadBits()
		{
			MsgWriter.WritePadBits(ref this.buf, ref this.seekPos, ref this.lengthBits);
		}

		// Token: 0x06004F13 RID: 20243 RVA: 0x002AE2A4 File Offset: 0x002AC4A4
		public void WriteByte(byte val)
		{
			MsgWriter.WriteByte(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F14 RID: 20244 RVA: 0x002AE2BE File Offset: 0x002AC4BE
		public void WriteUInt16(ushort val)
		{
			MsgWriter.WriteUInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F15 RID: 20245 RVA: 0x002AE2D8 File Offset: 0x002AC4D8
		public void WriteInt16(short val)
		{
			MsgWriter.WriteInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F16 RID: 20246 RVA: 0x002AE2F2 File Offset: 0x002AC4F2
		public void WriteUInt32(uint val)
		{
			MsgWriter.WriteUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F17 RID: 20247 RVA: 0x002AE30C File Offset: 0x002AC50C
		public void WriteInt32(int val)
		{
			MsgWriter.WriteInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F18 RID: 20248 RVA: 0x002AE326 File Offset: 0x002AC526
		public void WriteUInt64(ulong val)
		{
			MsgWriter.WriteUInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F19 RID: 20249 RVA: 0x002AE340 File Offset: 0x002AC540
		public void WriteInt64(long val)
		{
			MsgWriter.WriteInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1A RID: 20250 RVA: 0x002AE35A File Offset: 0x002AC55A
		public void WriteSingle(float val)
		{
			MsgWriter.WriteSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1B RID: 20251 RVA: 0x002AE374 File Offset: 0x002AC574
		public void WriteDouble(double val)
		{
			MsgWriter.WriteDouble(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1C RID: 20252 RVA: 0x002AE38E File Offset: 0x002AC58E
		public void WriteColorR8G8B8(Color val)
		{
			MsgWriter.WriteColorR8G8B8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1D RID: 20253 RVA: 0x002AE3A8 File Offset: 0x002AC5A8
		public void WriteColorR8G8B8A8(Color val)
		{
			MsgWriter.WriteColorR8G8B8A8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1E RID: 20254 RVA: 0x002AE3C2 File Offset: 0x002AC5C2
		public void WriteVariableUInt32(uint val)
		{
			MsgWriter.WriteVariableUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F1F RID: 20255 RVA: 0x002AE3DC File Offset: 0x002AC5DC
		public void WriteString(string val)
		{
			MsgWriter.WriteString(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06004F20 RID: 20256 RVA: 0x002AE3F6 File Offset: 0x002AC5F6
		public void WriteIdentifier(Identifier val)
		{
			this.WriteString(val.Value);
		}

		// Token: 0x06004F21 RID: 20257 RVA: 0x002AE405 File Offset: 0x002AC605
		public void WriteRangedInteger(int val, int min, int max)
		{
			MsgWriter.WriteRangedInteger(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max);
		}

		// Token: 0x06004F22 RID: 20258 RVA: 0x002AE421 File Offset: 0x002AC621
		public void WriteRangedSingle(float val, float min, float max, int bitCount)
		{
			MsgWriter.WriteRangedSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max, bitCount);
		}

		// Token: 0x06004F23 RID: 20259 RVA: 0x002AE43F File Offset: 0x002AC63F
		public void WriteBytes(byte[] val, int startPos, int length)
		{
			MsgWriter.WriteBytes(ref this.buf, ref this.seekPos, ref this.lengthBits, val, startPos, length);
		}

		// Token: 0x06004F24 RID: 20260 RVA: 0x002AE45B File Offset: 0x002AC65B
		public bool ReadBoolean()
		{
			return MsgReader.ReadBoolean(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F25 RID: 20261 RVA: 0x002AE46E File Offset: 0x002AC66E
		public void ReadPadBits()
		{
			MsgReader.ReadPadBits(ref this.seekPos);
		}

		// Token: 0x06004F26 RID: 20262 RVA: 0x002AE47B File Offset: 0x002AC67B
		public byte ReadByte()
		{
			return MsgReader.ReadByte(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F27 RID: 20263 RVA: 0x002AE48E File Offset: 0x002AC68E
		public byte PeekByte()
		{
			return MsgReader.PeekByte(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F28 RID: 20264 RVA: 0x002AE4A1 File Offset: 0x002AC6A1
		public ushort ReadUInt16()
		{
			return MsgReader.ReadUInt16(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F29 RID: 20265 RVA: 0x002AE4B4 File Offset: 0x002AC6B4
		public short ReadInt16()
		{
			return MsgReader.ReadInt16(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2A RID: 20266 RVA: 0x002AE4C7 File Offset: 0x002AC6C7
		public uint ReadUInt32()
		{
			return MsgReader.ReadUInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2B RID: 20267 RVA: 0x002AE4DA File Offset: 0x002AC6DA
		public int ReadInt32()
		{
			return MsgReader.ReadInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2C RID: 20268 RVA: 0x002AE4ED File Offset: 0x002AC6ED
		public ulong ReadUInt64()
		{
			return MsgReader.ReadUInt64(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2D RID: 20269 RVA: 0x002AE500 File Offset: 0x002AC700
		public long ReadInt64()
		{
			return MsgReader.ReadInt64(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2E RID: 20270 RVA: 0x002AE513 File Offset: 0x002AC713
		public float ReadSingle()
		{
			return MsgReader.ReadSingle(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F2F RID: 20271 RVA: 0x002AE526 File Offset: 0x002AC726
		public double ReadDouble()
		{
			return MsgReader.ReadDouble(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F30 RID: 20272 RVA: 0x002AE539 File Offset: 0x002AC739
		public uint ReadVariableUInt32()
		{
			return MsgReader.ReadVariableUInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F31 RID: 20273 RVA: 0x002AE54C File Offset: 0x002AC74C
		public string ReadString()
		{
			return MsgReader.ReadString(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F32 RID: 20274 RVA: 0x002AE55F File Offset: 0x002AC75F
		public Identifier ReadIdentifier()
		{
			return this.ReadString().ToIdentifier();
		}

		// Token: 0x06004F33 RID: 20275 RVA: 0x002AE56C File Offset: 0x002AC76C
		public Color ReadColorR8G8B8()
		{
			return MsgReader.ReadColorR8G8B8(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F34 RID: 20276 RVA: 0x002AE57F File Offset: 0x002AC77F
		public Color ReadColorR8G8B8A8()
		{
			return MsgReader.ReadColorR8G8B8A8(this.buf, ref this.seekPos);
		}

		// Token: 0x06004F35 RID: 20277 RVA: 0x002AE592 File Offset: 0x002AC792
		public int ReadRangedInteger(int min, int max)
		{
			return MsgReader.ReadRangedInteger(this.buf, ref this.seekPos, min, max);
		}

		// Token: 0x06004F36 RID: 20278 RVA: 0x002AE5A7 File Offset: 0x002AC7A7
		public float ReadRangedSingle(float min, float max, int bitCount)
		{
			return MsgReader.ReadRangedSingle(this.buf, ref this.seekPos, min, max, bitCount);
		}

		// Token: 0x06004F37 RID: 20279 RVA: 0x002AE5BD File Offset: 0x002AC7BD
		public byte[] ReadBytes(int numberOfBytes)
		{
			return MsgReader.ReadBytes(this.buf, ref this.seekPos, numberOfBytes);
		}

		// Token: 0x06004F38 RID: 20280 RVA: 0x002AE5D1 File Offset: 0x002AC7D1
		public byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int outLength)
		{
			throw new InvalidOperationException("ReadWriteMessages are not to be sent");
		}

		// Token: 0x040029C7 RID: 10695
		private byte[] buf;

		// Token: 0x040029C8 RID: 10696
		private int seekPos;

		// Token: 0x040029C9 RID: 10697
		private int lengthBits;
	}
}
