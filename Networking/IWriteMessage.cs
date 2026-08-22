using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A0 RID: 1184
	public interface IWriteMessage
	{
		// Token: 0x06004E8C RID: 20108
		void WriteBoolean(bool val);

		// Token: 0x06004E8D RID: 20109
		void WritePadBits();

		// Token: 0x06004E8E RID: 20110
		void WriteByte(byte val);

		// Token: 0x06004E8F RID: 20111
		void WriteInt16(short val);

		// Token: 0x06004E90 RID: 20112
		void WriteUInt16(ushort val);

		// Token: 0x06004E91 RID: 20113
		void WriteInt32(int val);

		// Token: 0x06004E92 RID: 20114
		void WriteUInt32(uint val);

		// Token: 0x06004E93 RID: 20115
		void WriteInt64(long val);

		// Token: 0x06004E94 RID: 20116
		void WriteUInt64(ulong val);

		// Token: 0x06004E95 RID: 20117
		void WriteSingle(float val);

		// Token: 0x06004E96 RID: 20118
		void WriteDouble(double val);

		// Token: 0x06004E97 RID: 20119
		void WriteColorR8G8B8(Color val);

		// Token: 0x06004E98 RID: 20120
		void WriteColorR8G8B8A8(Color val);

		// Token: 0x06004E99 RID: 20121
		void WriteVariableUInt32(uint val);

		// Token: 0x06004E9A RID: 20122
		void WriteString(string val);

		// Token: 0x06004E9B RID: 20123
		void WriteIdentifier(Identifier val);

		// Token: 0x06004E9C RID: 20124
		void WriteRangedInteger(int val, int min, int max);

		// Token: 0x06004E9D RID: 20125
		void WriteRangedSingle(float val, float min, float max, int bitCount);

		// Token: 0x06004E9E RID: 20126
		void WriteBytes(byte[] val, int startIndex, int length);

		// Token: 0x06004E9F RID: 20127
		byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int outLength);

		// Token: 0x1700141C RID: 5148
		// (get) Token: 0x06004EA0 RID: 20128
		// (set) Token: 0x06004EA1 RID: 20129
		int BitPosition { get; set; }

		// Token: 0x1700141D RID: 5149
		// (get) Token: 0x06004EA2 RID: 20130
		int BytePosition { get; }

		// Token: 0x1700141E RID: 5150
		// (get) Token: 0x06004EA3 RID: 20131
		byte[] Buffer { get; }

		// Token: 0x1700141F RID: 5151
		// (get) Token: 0x06004EA4 RID: 20132
		// (set) Token: 0x06004EA5 RID: 20133
		int LengthBits { get; set; }

		// Token: 0x17001420 RID: 5152
		// (get) Token: 0x06004EA6 RID: 20134
		int LengthBytes { get; }
	}
}
