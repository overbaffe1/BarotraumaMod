using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Lidgren.Network;

namespace Barotrauma
{
	// Token: 0x02000267 RID: 615
	internal sealed class WriteOnlyBitField : IDisposable
	{
		// Token: 0x06002C3E RID: 11326 RVA: 0x00124DF8 File Offset: 0x00122FF8
		public void WriteBoolean(bool b)
		{
			this.ThrowIfDisposed();
			int arrayIndex = (int)Math.Floor((double)((float)this.index / 7f));
			if (arrayIndex >= this.Buffer.Count)
			{
				this.Buffer.Add(0);
			}
			int bitIndex = this.index % 7;
			List<byte> buffer = this.Buffer;
			int num = arrayIndex;
			buffer[num] |= (byte)(b ? (1 << bitIndex) : 0);
			this.index++;
		}

		// Token: 0x06002C3F RID: 11327 RVA: 0x00124E78 File Offset: 0x00123078
		public void WriteInteger(int value, int min, int max)
		{
			this.ThrowIfDisposed();
			uint range = (uint)(max - min);
			int numberOfBits = NetUtility.BitsToHoldUInt(range);
			uint writeValue = (uint)(value - min);
			for (int i = 0; i < numberOfBits; i++)
			{
				this.WriteBoolean((writeValue & 1U << i) > 0U);
			}
		}

		// Token: 0x06002C40 RID: 11328 RVA: 0x00124EB8 File Offset: 0x001230B8
		public void WriteFloat(float value, float min, float max, int numberOfBits)
		{
			this.ThrowIfDisposed();
			float range = max - min;
			float unit = (value - min) / range;
			uint maxVal = (1U << numberOfBits) - 1U;
			uint writeValue = (uint)(maxVal * unit);
			for (int i = 0; i < numberOfBits; i++)
			{
				this.WriteBoolean((writeValue & 1U << i) > 0U);
			}
		}

		// Token: 0x06002C41 RID: 11329 RVA: 0x00124F0C File Offset: 0x0012310C
		[NullableContext(1)]
		public void WriteToMessage(IWriteMessage msg)
		{
			this.ThrowIfDisposed();
			if (this.Buffer.Count == 0)
			{
				this.Buffer.Add(0);
			}
			List<byte> buffer = this.Buffer;
			int num = buffer.Count - 1;
			buffer[num] |= 128;
			foreach (byte b in this.Buffer)
			{
				msg.WriteByte(b);
			}
			this.Dispose();
		}

		// Token: 0x06002C42 RID: 11330 RVA: 0x00124FAC File Offset: 0x001231AC
		public void Dispose()
		{
			this.disposed = true;
		}

		// Token: 0x06002C43 RID: 11331 RVA: 0x00124FB5 File Offset: 0x001231B5
		private void ThrowIfDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("WriteOnlyBitField");
			}
		}

		// Token: 0x040015D2 RID: 5586
		private const int AmountOfBoolsInByte = 7;

		// Token: 0x040015D3 RID: 5587
		[Nullable(1)]
		private readonly List<byte> Buffer = new List<byte>();

		// Token: 0x040015D4 RID: 5588
		private int index;

		// Token: 0x040015D5 RID: 5589
		private bool disposed;
	}
}
