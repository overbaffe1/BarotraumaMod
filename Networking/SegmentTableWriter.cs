using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004CB RID: 1227
	[CompilerFeatureRequired("RefStructs")]
	internal readonly ref struct SegmentTableWriter<T> where T : struct
	{
		// Token: 0x06004FF2 RID: 20466 RVA: 0x002B00AF File Offset: 0x002AE2AF
		[NullableContext(1)]
		private SegmentTableWriter(IWriteMessage message, int pointerLocation)
		{
			this.message = message;
			this.PointerLocation = pointerLocation;
			this.segments = new List<Segment<T>>();
		}

		// Token: 0x06004FF3 RID: 20467 RVA: 0x002B00CC File Offset: 0x002AE2CC
		public static SegmentTableWriter<T> StartWriting([Nullable(1)] IWriteMessage msg)
		{
			SegmentTableWriter<T> retVal = new SegmentTableWriter<T>(msg, msg.BitPosition);
			msg.WriteInt32(0);
			return retVal;
		}

		// Token: 0x06004FF4 RID: 20468 RVA: 0x002B00EF File Offset: 0x002AE2EF
		private void ThrowOnInvalidState()
		{
			if (this.segments.Count >= 65535)
			{
				throw new InvalidOperationException("Too many segments in SegmentTable<" + typeof(T).Name + ">");
			}
		}

		// Token: 0x06004FF5 RID: 20469 RVA: 0x002B0127 File Offset: 0x002AE327
		public void StartNewSegment(T value)
		{
			this.ThrowOnInvalidState();
			this.segments.Add(new Segment<T>(value, this.message.BitPosition - this.PointerLocation));
		}

		// Token: 0x06004FF6 RID: 20470 RVA: 0x002B0154 File Offset: 0x002AE354
		public void Dispose()
		{
			this.ThrowOnInvalidState();
			int tablePosition = this.message.BitPosition;
			this.message.BitPosition = this.PointerLocation;
			this.message.WriteInt32(tablePosition - this.PointerLocation);
			this.message.BitPosition = tablePosition;
			this.message.WriteUInt16((ushort)this.segments.Count);
			foreach (Segment<T> segment in this.segments)
			{
				this.message.WriteNetSerializableStruct(segment);
			}
		}

		// Token: 0x04002A1C RID: 10780
		[Nullable(1)]
		private readonly IWriteMessage message;

		// Token: 0x04002A1D RID: 10781
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		private readonly List<Segment<T>> segments;

		// Token: 0x04002A1E RID: 10782
		public readonly int PointerLocation;
	}
}
