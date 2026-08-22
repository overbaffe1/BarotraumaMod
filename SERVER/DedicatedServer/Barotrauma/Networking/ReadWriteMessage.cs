using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003AA RID: 938
	internal sealed class ReadWriteMessage : IWriteMessage, IReadMessage
	{
		// Token: 0x06003732 RID: 14130 RVA: 0x0017565A File Offset: 0x0017385A
		public ReadWriteMessage()
		{
			this.buf = new byte[256];
			this.seekPos = 0;
			this.lengthBits = 0;
		}

		// Token: 0x06003733 RID: 14131 RVA: 0x00175680 File Offset: 0x00173880
		public ReadWriteMessage(byte[] b, int bitPos, int lBits, bool copyBuf)
		{
			this.buf = (copyBuf ? ((byte[])b.Clone()) : b);
			this.seekPos = bitPos;
			this.lengthBits = lBits;
		}

		// Token: 0x17000F31 RID: 3889
		// (get) Token: 0x06003734 RID: 14132 RVA: 0x001756AE File Offset: 0x001738AE
		// (set) Token: 0x06003735 RID: 14133 RVA: 0x001756B6 File Offset: 0x001738B6
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

		// Token: 0x17000F32 RID: 3890
		// (get) Token: 0x06003736 RID: 14134 RVA: 0x001756BF File Offset: 0x001738BF
		public int BytePosition
		{
			get
			{
				return this.seekPos / 8;
			}
		}

		// Token: 0x17000F33 RID: 3891
		// (get) Token: 0x06003737 RID: 14135 RVA: 0x001756C9 File Offset: 0x001738C9
		public byte[] Buffer
		{
			get
			{
				return this.buf;
			}
		}

		// Token: 0x17000F34 RID: 3892
		// (get) Token: 0x06003738 RID: 14136 RVA: 0x001756D1 File Offset: 0x001738D1
		// (set) Token: 0x06003739 RID: 14137 RVA: 0x001756FB File Offset: 0x001738FB
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

		// Token: 0x17000F35 RID: 3893
		// (get) Token: 0x0600373A RID: 14138 RVA: 0x00175726 File Offset: 0x00173926
		public int LengthBytes
		{
			get
			{
				return (this.LengthBits + 7) / 8;
			}
		}

		// Token: 0x17000F36 RID: 3894
		// (get) Token: 0x0600373B RID: 14139 RVA: 0x00175732 File Offset: 0x00173932
		public NetworkConnection Sender
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600373C RID: 14140 RVA: 0x00175735 File Offset: 0x00173935
		public void WriteBoolean(bool val)
		{
			MsgWriter.WriteBoolean(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600373D RID: 14141 RVA: 0x0017574F File Offset: 0x0017394F
		public void WritePadBits()
		{
			MsgWriter.WritePadBits(ref this.buf, ref this.seekPos, ref this.lengthBits);
		}

		// Token: 0x0600373E RID: 14142 RVA: 0x00175768 File Offset: 0x00173968
		public void WriteByte(byte val)
		{
			MsgWriter.WriteByte(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600373F RID: 14143 RVA: 0x00175782 File Offset: 0x00173982
		public void WriteUInt16(ushort val)
		{
			MsgWriter.WriteUInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003740 RID: 14144 RVA: 0x0017579C File Offset: 0x0017399C
		public void WriteInt16(short val)
		{
			MsgWriter.WriteInt16(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003741 RID: 14145 RVA: 0x001757B6 File Offset: 0x001739B6
		public void WriteUInt32(uint val)
		{
			MsgWriter.WriteUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003742 RID: 14146 RVA: 0x001757D0 File Offset: 0x001739D0
		public void WriteInt32(int val)
		{
			MsgWriter.WriteInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003743 RID: 14147 RVA: 0x001757EA File Offset: 0x001739EA
		public void WriteUInt64(ulong val)
		{
			MsgWriter.WriteUInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003744 RID: 14148 RVA: 0x00175804 File Offset: 0x00173A04
		public void WriteInt64(long val)
		{
			MsgWriter.WriteInt64(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003745 RID: 14149 RVA: 0x0017581E File Offset: 0x00173A1E
		public void WriteSingle(float val)
		{
			MsgWriter.WriteSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003746 RID: 14150 RVA: 0x00175838 File Offset: 0x00173A38
		public void WriteDouble(double val)
		{
			MsgWriter.WriteDouble(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003747 RID: 14151 RVA: 0x00175852 File Offset: 0x00173A52
		public void WriteColorR8G8B8(Color val)
		{
			MsgWriter.WriteColorR8G8B8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003748 RID: 14152 RVA: 0x0017586C File Offset: 0x00173A6C
		public void WriteColorR8G8B8A8(Color val)
		{
			MsgWriter.WriteColorR8G8B8A8(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x06003749 RID: 14153 RVA: 0x00175886 File Offset: 0x00173A86
		public void WriteVariableUInt32(uint val)
		{
			MsgWriter.WriteVariableUInt32(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600374A RID: 14154 RVA: 0x001758A0 File Offset: 0x00173AA0
		public void WriteString(string val)
		{
			MsgWriter.WriteString(ref this.buf, ref this.seekPos, ref this.lengthBits, val);
		}

		// Token: 0x0600374B RID: 14155 RVA: 0x001758BA File Offset: 0x00173ABA
		public void WriteIdentifier(Identifier val)
		{
			this.WriteString(val.Value);
		}

		// Token: 0x0600374C RID: 14156 RVA: 0x001758C9 File Offset: 0x00173AC9
		public void WriteRangedInteger(int val, int min, int max)
		{
			MsgWriter.WriteRangedInteger(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max);
		}

		// Token: 0x0600374D RID: 14157 RVA: 0x001758E5 File Offset: 0x00173AE5
		public void WriteRangedSingle(float val, float min, float max, int bitCount)
		{
			MsgWriter.WriteRangedSingle(ref this.buf, ref this.seekPos, ref this.lengthBits, val, min, max, bitCount);
		}

		// Token: 0x0600374E RID: 14158 RVA: 0x00175903 File Offset: 0x00173B03
		public void WriteBytes(byte[] val, int startPos, int length)
		{
			MsgWriter.WriteBytes(ref this.buf, ref this.seekPos, ref this.lengthBits, val, startPos, length);
		}

		// Token: 0x0600374F RID: 14159 RVA: 0x0017591F File Offset: 0x00173B1F
		public bool ReadBoolean()
		{
			return MsgReader.ReadBoolean(this.buf, ref this.seekPos);
		}

		// Token: 0x06003750 RID: 14160 RVA: 0x00175932 File Offset: 0x00173B32
		public void ReadPadBits()
		{
			MsgReader.ReadPadBits(ref this.seekPos);
		}

		// Token: 0x06003751 RID: 14161 RVA: 0x0017593F File Offset: 0x00173B3F
		public byte ReadByte()
		{
			return MsgReader.ReadByte(this.buf, ref this.seekPos);
		}

		// Token: 0x06003752 RID: 14162 RVA: 0x00175952 File Offset: 0x00173B52
		public byte PeekByte()
		{
			return MsgReader.PeekByte(this.buf, ref this.seekPos);
		}

		// Token: 0x06003753 RID: 14163 RVA: 0x00175965 File Offset: 0x00173B65
		public ushort ReadUInt16()
		{
			return MsgReader.ReadUInt16(this.buf, ref this.seekPos);
		}

		// Token: 0x06003754 RID: 14164 RVA: 0x00175978 File Offset: 0x00173B78
		public short ReadInt16()
		{
			return MsgReader.ReadInt16(this.buf, ref this.seekPos);
		}

		// Token: 0x06003755 RID: 14165 RVA: 0x0017598B File Offset: 0x00173B8B
		public uint ReadUInt32()
		{
			return MsgReader.ReadUInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x06003756 RID: 14166 RVA: 0x0017599E File Offset: 0x00173B9E
		public int ReadInt32()
		{
			return MsgReader.ReadInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x06003757 RID: 14167 RVA: 0x001759B1 File Offset: 0x00173BB1
		public ulong ReadUInt64()
		{
			return MsgReader.ReadUInt64(this.buf, ref this.seekPos);
		}

		// Token: 0x06003758 RID: 14168 RVA: 0x001759C4 File Offset: 0x00173BC4
		public long ReadInt64()
		{
			return MsgReader.ReadInt64(this.buf, ref this.seekPos);
		}

		// Token: 0x06003759 RID: 14169 RVA: 0x001759D7 File Offset: 0x00173BD7
		public float ReadSingle()
		{
			return MsgReader.ReadSingle(this.buf, ref this.seekPos);
		}

		// Token: 0x0600375A RID: 14170 RVA: 0x001759EA File Offset: 0x00173BEA
		public double ReadDouble()
		{
			return MsgReader.ReadDouble(this.buf, ref this.seekPos);
		}

		// Token: 0x0600375B RID: 14171 RVA: 0x001759FD File Offset: 0x00173BFD
		public uint ReadVariableUInt32()
		{
			return MsgReader.ReadVariableUInt32(this.buf, ref this.seekPos);
		}

		// Token: 0x0600375C RID: 14172 RVA: 0x00175A10 File Offset: 0x00173C10
		public string ReadString()
		{
			return MsgReader.ReadString(this.buf, ref this.seekPos);
		}

		// Token: 0x0600375D RID: 14173 RVA: 0x00175A23 File Offset: 0x00173C23
		public Identifier ReadIdentifier()
		{
			return this.ReadString().ToIdentifier();
		}

		// Token: 0x0600375E RID: 14174 RVA: 0x00175A30 File Offset: 0x00173C30
		public Color ReadColorR8G8B8()
		{
			return MsgReader.ReadColorR8G8B8(this.buf, ref this.seekPos);
		}

		// Token: 0x0600375F RID: 14175 RVA: 0x00175A43 File Offset: 0x00173C43
		public Color ReadColorR8G8B8A8()
		{
			return MsgReader.ReadColorR8G8B8A8(this.buf, ref this.seekPos);
		}

		// Token: 0x06003760 RID: 14176 RVA: 0x00175A56 File Offset: 0x00173C56
		public int ReadRangedInteger(int min, int max)
		{
			return MsgReader.ReadRangedInteger(this.buf, ref this.seekPos, min, max);
		}

		// Token: 0x06003761 RID: 14177 RVA: 0x00175A6B File Offset: 0x00173C6B
		public float ReadRangedSingle(float min, float max, int bitCount)
		{
			return MsgReader.ReadRangedSingle(this.buf, ref this.seekPos, min, max, bitCount);
		}

		// Token: 0x06003762 RID: 14178 RVA: 0x00175A81 File Offset: 0x00173C81
		public byte[] ReadBytes(int numberOfBytes)
		{
			return MsgReader.ReadBytes(this.buf, ref this.seekPos, numberOfBytes);
		}

		// Token: 0x06003763 RID: 14179 RVA: 0x00175A95 File Offset: 0x00173C95
		public byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int outLength)
		{
			throw new InvalidOperationException("ReadWriteMessages are not to be sent");
		}

		// Token: 0x04001BD0 RID: 7120
		private byte[] buf;

		// Token: 0x04001BD1 RID: 7121
		private int seekPos;

		// Token: 0x04001BD2 RID: 7122
		private int lengthBits;
	}
}
