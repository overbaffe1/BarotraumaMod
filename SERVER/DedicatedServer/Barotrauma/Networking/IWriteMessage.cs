using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A3 RID: 931
	public interface IWriteMessage
	{
		// Token: 0x060036B7 RID: 14007
		void WriteBoolean(bool val);

		// Token: 0x060036B8 RID: 14008
		void WritePadBits();

		// Token: 0x060036B9 RID: 14009
		void WriteByte(byte val);

		// Token: 0x060036BA RID: 14010
		void WriteInt16(short val);

		// Token: 0x060036BB RID: 14011
		void WriteUInt16(ushort val);

		// Token: 0x060036BC RID: 14012
		void WriteInt32(int val);

		// Token: 0x060036BD RID: 14013
		void WriteUInt32(uint val);

		// Token: 0x060036BE RID: 14014
		void WriteInt64(long val);

		// Token: 0x060036BF RID: 14015
		void WriteUInt64(ulong val);

		// Token: 0x060036C0 RID: 14016
		void WriteSingle(float val);

		// Token: 0x060036C1 RID: 14017
		void WriteDouble(double val);

		// Token: 0x060036C2 RID: 14018
		void WriteColorR8G8B8(Color val);

		// Token: 0x060036C3 RID: 14019
		void WriteColorR8G8B8A8(Color val);

		// Token: 0x060036C4 RID: 14020
		void WriteVariableUInt32(uint val);

		// Token: 0x060036C5 RID: 14021
		void WriteString(string val);

		// Token: 0x060036C6 RID: 14022
		void WriteIdentifier(Identifier val);

		// Token: 0x060036C7 RID: 14023
		void WriteRangedInteger(int val, int min, int max);

		// Token: 0x060036C8 RID: 14024
		void WriteRangedSingle(float val, float min, float max, int bitCount);

		// Token: 0x060036C9 RID: 14025
		void WriteBytes(byte[] val, int startIndex, int length);

		// Token: 0x060036CA RID: 14026
		byte[] PrepareForSending(bool compressPastThreshold, out bool isCompressed, out int outLength);

		// Token: 0x17000F21 RID: 3873
		// (get) Token: 0x060036CB RID: 14027
		// (set) Token: 0x060036CC RID: 14028
		int BitPosition { get; set; }

		// Token: 0x17000F22 RID: 3874
		// (get) Token: 0x060036CD RID: 14029
		int BytePosition { get; }

		// Token: 0x17000F23 RID: 3875
		// (get) Token: 0x060036CE RID: 14030
		byte[] Buffer { get; }

		// Token: 0x17000F24 RID: 3876
		// (get) Token: 0x060036CF RID: 14031
		// (set) Token: 0x060036D0 RID: 14032
		int LengthBits { get; set; }

		// Token: 0x17000F25 RID: 3877
		// (get) Token: 0x060036D1 RID: 14033
		int LengthBytes { get; }
	}
}
