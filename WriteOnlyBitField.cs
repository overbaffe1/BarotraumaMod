using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Lidgren.Network;

namespace Barotrauma
{
	// Token: 0x0200033A RID: 826
	internal sealed class WriteOnlyBitField : IDisposable
	{
		// Token: 0x0600415D RID: 16733 RVA: 0x0024513C File Offset: 0x0024333C
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

		// Token: 0x0600415E RID: 16734 RVA: 0x002451BC File Offset: 0x002433BC
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

		// Token: 0x0600415F RID: 16735 RVA: 0x002451FC File Offset: 0x002433FC
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

		// Token: 0x06004160 RID: 16736 RVA: 0x00245250 File Offset: 0x00243450
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

		// Token: 0x06004161 RID: 16737 RVA: 0x002452F0 File Offset: 0x002434F0
		public void Dispose()
		{
			this.disposed = true;
		}

		// Token: 0x06004162 RID: 16738 RVA: 0x002452F9 File Offset: 0x002434F9
		private void ThrowIfDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("WriteOnlyBitField");
			}
		}

		// Token: 0x04002218 RID: 8728
		private const int AmountOfBoolsInByte = 7;

		// Token: 0x04002219 RID: 8729
		[Nullable(1)]
		private readonly List<byte> Buffer = new List<byte>();

		// Token: 0x0400221A RID: 8730
		private int index;

		// Token: 0x0400221B RID: 8731
		private bool disposed;
	}
}
