using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A2 RID: 930
	public interface IReadMessage
	{
		// Token: 0x0600369B RID: 13979
		bool ReadBoolean();

		// Token: 0x0600369C RID: 13980
		void ReadPadBits();

		// Token: 0x0600369D RID: 13981
		byte ReadByte();

		// Token: 0x0600369E RID: 13982
		byte PeekByte();

		// Token: 0x0600369F RID: 13983
		ushort ReadUInt16();

		// Token: 0x060036A0 RID: 13984
		short ReadInt16();

		// Token: 0x060036A1 RID: 13985
		uint ReadUInt32();

		// Token: 0x060036A2 RID: 13986
		int ReadInt32();

		// Token: 0x060036A3 RID: 13987
		ulong ReadUInt64();

		// Token: 0x060036A4 RID: 13988
		long ReadInt64();

		// Token: 0x060036A5 RID: 13989
		float ReadSingle();

		// Token: 0x060036A6 RID: 13990
		double ReadDouble();

		// Token: 0x060036A7 RID: 13991
		uint ReadVariableUInt32();

		// Token: 0x060036A8 RID: 13992
		string ReadString();

		// Token: 0x060036A9 RID: 13993
		Identifier ReadIdentifier();

		// Token: 0x060036AA RID: 13994
		Color ReadColorR8G8B8();

		// Token: 0x060036AB RID: 13995
		Color ReadColorR8G8B8A8();

		// Token: 0x060036AC RID: 13996
		int ReadRangedInteger(int min, int max);

		// Token: 0x060036AD RID: 13997
		float ReadRangedSingle(float min, float max, int bitCount);

		// Token: 0x060036AE RID: 13998
		byte[] ReadBytes(int numberOfBytes);

		// Token: 0x17000F1B RID: 3867
		// (get) Token: 0x060036AF RID: 13999
		// (set) Token: 0x060036B0 RID: 14000
		int BitPosition { get; set; }

		// Token: 0x17000F1C RID: 3868
		// (get) Token: 0x060036B1 RID: 14001
		int BytePosition { get; }

		// Token: 0x17000F1D RID: 3869
		// (get) Token: 0x060036B2 RID: 14002
		byte[] Buffer { get; }

		// Token: 0x17000F1E RID: 3870
		// (get) Token: 0x060036B3 RID: 14003
		// (set) Token: 0x060036B4 RID: 14004
		int LengthBits { get; set; }

		// Token: 0x17000F1F RID: 3871
		// (get) Token: 0x060036B5 RID: 14005
		int LengthBytes { get; }

		// Token: 0x17000F20 RID: 3872
		// (get) Token: 0x060036B6 RID: 14006
		NetworkConnection Sender { get; }
	}
}
