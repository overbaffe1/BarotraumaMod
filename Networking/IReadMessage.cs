using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200049F RID: 1183
	public interface IReadMessage
	{
		// Token: 0x06004E70 RID: 20080
		bool ReadBoolean();

		// Token: 0x06004E71 RID: 20081
		void ReadPadBits();

		// Token: 0x06004E72 RID: 20082
		byte ReadByte();

		// Token: 0x06004E73 RID: 20083
		byte PeekByte();

		// Token: 0x06004E74 RID: 20084
		ushort ReadUInt16();

		// Token: 0x06004E75 RID: 20085
		short ReadInt16();

		// Token: 0x06004E76 RID: 20086
		uint ReadUInt32();

		// Token: 0x06004E77 RID: 20087
		int ReadInt32();

		// Token: 0x06004E78 RID: 20088
		ulong ReadUInt64();

		// Token: 0x06004E79 RID: 20089
		long ReadInt64();

		// Token: 0x06004E7A RID: 20090
		float ReadSingle();

		// Token: 0x06004E7B RID: 20091
		double ReadDouble();

		// Token: 0x06004E7C RID: 20092
		uint ReadVariableUInt32();

		// Token: 0x06004E7D RID: 20093
		string ReadString();

		// Token: 0x06004E7E RID: 20094
		Identifier ReadIdentifier();

		// Token: 0x06004E7F RID: 20095
		Color ReadColorR8G8B8();

		// Token: 0x06004E80 RID: 20096
		Color ReadColorR8G8B8A8();

		// Token: 0x06004E81 RID: 20097
		int ReadRangedInteger(int min, int max);

		// Token: 0x06004E82 RID: 20098
		float ReadRangedSingle(float min, float max, int bitCount);

		// Token: 0x06004E83 RID: 20099
		byte[] ReadBytes(int numberOfBytes);

		// Token: 0x17001416 RID: 5142
		// (get) Token: 0x06004E84 RID: 20100
		// (set) Token: 0x06004E85 RID: 20101
		int BitPosition { get; set; }

		// Token: 0x17001417 RID: 5143
		// (get) Token: 0x06004E86 RID: 20102
		int BytePosition { get; }

		// Token: 0x17001418 RID: 5144
		// (get) Token: 0x06004E87 RID: 20103
		byte[] Buffer { get; }

		// Token: 0x17001419 RID: 5145
		// (get) Token: 0x06004E88 RID: 20104
		// (set) Token: 0x06004E89 RID: 20105
		int LengthBits { get; set; }

		// Token: 0x1700141A RID: 5146
		// (get) Token: 0x06004E8A RID: 20106
		int LengthBytes { get; }

		// Token: 0x1700141B RID: 5147
		// (get) Token: 0x06004E8B RID: 20107
		NetworkConnection Sender { get; }
	}
}
