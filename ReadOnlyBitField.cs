using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Lidgren.Network;

namespace Barotrauma
{
	// Token: 0x0200033B RID: 827
	internal sealed class ReadOnlyBitField
	{
		// Token: 0x06004164 RID: 16740 RVA: 0x00245324 File Offset: 0x00243524
		[NullableContext(1)]
		public ReadOnlyBitField(IReadMessage inc)
		{
			List<byte> bytes = new List<byte>();
			while (inc.BitPosition < inc.LengthBits)
			{
				byte currentByte = inc.ReadByte();
				bytes.Add(currentByte);
				if (ReadOnlyBitField.IsBitSet(currentByte, 7))
				{
					this.buffer = bytes.ToImmutableArray<byte>();
					return;
				}
			}
			throw new NetStructReadException("Failed to find the end of the bit field: end of the message reached.", null);
		}

		// Token: 0x06004165 RID: 16741 RVA: 0x0024537C File Offset: 0x0024357C
		public bool ReadBoolean()
		{
			int arrayIndex = (int)MathF.Floor((float)this.index / 7f);
			int bitIndex = this.index % 7;
			this.index++;
			return ReadOnlyBitField.IsBitSet(this.buffer[arrayIndex], bitIndex);
		}

		// Token: 0x06004166 RID: 16742 RVA: 0x002453C8 File Offset: 0x002435C8
		public int ReadInteger(int min, int max)
		{
			uint range = (uint)(max - min);
			int numberOfBits = NetUtility.BitsToHoldUInt(range);
			uint value = 0U;
			for (int i = 0; i < numberOfBits; i++)
			{
				value |= (this.ReadBoolean() ? (1U << i) : 0U);
			}
			return (int)((long)min + (long)((ulong)value));
		}

		// Token: 0x06004167 RID: 16743 RVA: 0x00245408 File Offset: 0x00243608
		public float ReadFloat(float min, float max, int numberOfBits)
		{
			int maxInt = (1 << numberOfBits) - 1;
			uint value = 0U;
			for (int i = 0; i < numberOfBits; i++)
			{
				value |= (this.ReadBoolean() ? (1U << i) : 0U);
			}
			float range = max - min;
			return min + range * value / (float)maxInt;
		}

		// Token: 0x06004168 RID: 16744 RVA: 0x0024544E File Offset: 0x0024364E
		private static bool IsBitSet(byte b, int bitIndex)
		{
			return ((int)b & 1 << bitIndex) != 0;
		}

		// Token: 0x0400221C RID: 8732
		private const int AmountOfBoolsInByte = 7;

		// Token: 0x0400221D RID: 8733
		private readonly ImmutableArray<byte> buffer;

		// Token: 0x0400221E RID: 8734
		private int index;
	}
}
