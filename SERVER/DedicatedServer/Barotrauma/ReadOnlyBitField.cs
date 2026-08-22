using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Lidgren.Network;

namespace Barotrauma
{
	// Token: 0x02000268 RID: 616
	internal sealed class ReadOnlyBitField
	{
		// Token: 0x06002C45 RID: 11333 RVA: 0x00124FE0 File Offset: 0x001231E0
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

		// Token: 0x06002C46 RID: 11334 RVA: 0x00125038 File Offset: 0x00123238
		public bool ReadBoolean()
		{
			int arrayIndex = (int)MathF.Floor((float)this.index / 7f);
			int bitIndex = this.index % 7;
			this.index++;
			return ReadOnlyBitField.IsBitSet(this.buffer[arrayIndex], bitIndex);
		}

		// Token: 0x06002C47 RID: 11335 RVA: 0x00125084 File Offset: 0x00123284
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

		// Token: 0x06002C48 RID: 11336 RVA: 0x001250C4 File Offset: 0x001232C4
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

		// Token: 0x06002C49 RID: 11337 RVA: 0x0012510A File Offset: 0x0012330A
		private static bool IsBitSet(byte b, int bitIndex)
		{
			return ((int)b & 1 << bitIndex) != 0;
		}

		// Token: 0x040015D6 RID: 5590
		private const int AmountOfBoolsInByte = 7;

		// Token: 0x040015D7 RID: 5591
		private readonly ImmutableArray<byte> buffer;

		// Token: 0x040015D8 RID: 5592
		private int index;
	}
}
