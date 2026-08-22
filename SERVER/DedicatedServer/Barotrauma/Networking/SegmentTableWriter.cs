using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003D0 RID: 976
	[CompilerFeatureRequired("RefStructs")]
	internal readonly ref struct SegmentTableWriter<T> where T : struct
	{
		// Token: 0x06003824 RID: 14372 RVA: 0x001779E7 File Offset: 0x00175BE7
		[NullableContext(1)]
		private SegmentTableWriter(IWriteMessage message, int pointerLocation)
		{
			this.message = message;
			this.PointerLocation = pointerLocation;
			this.segments = new List<Segment<T>>();
		}

		// Token: 0x06003825 RID: 14373 RVA: 0x00177A04 File Offset: 0x00175C04
		public static SegmentTableWriter<T> StartWriting([Nullable(1)] IWriteMessage msg)
		{
			SegmentTableWriter<T> retVal = new SegmentTableWriter<T>(msg, msg.BitPosition);
			msg.WriteInt32(0);
			return retVal;
		}

		// Token: 0x06003826 RID: 14374 RVA: 0x00177A27 File Offset: 0x00175C27
		private void ThrowOnInvalidState()
		{
			if (this.segments.Count >= 65535)
			{
				throw new InvalidOperationException("Too many segments in SegmentTable<" + typeof(T).Name + ">");
			}
		}

		// Token: 0x06003827 RID: 14375 RVA: 0x00177A5F File Offset: 0x00175C5F
		public void StartNewSegment(T value)
		{
			this.ThrowOnInvalidState();
			this.segments.Add(new Segment<T>(value, this.message.BitPosition - this.PointerLocation));
		}

		// Token: 0x06003828 RID: 14376 RVA: 0x00177A8C File Offset: 0x00175C8C
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

		// Token: 0x04001C31 RID: 7217
		[Nullable(1)]
		private readonly IWriteMessage message;

		// Token: 0x04001C32 RID: 7218
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		private readonly List<Segment<T>> segments;

		// Token: 0x04001C33 RID: 7219
		public readonly int PointerLocation;
	}
}
