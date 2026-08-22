using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003D1 RID: 977
	[CompilerFeatureRequired("RefStructs")]
	internal readonly ref struct SegmentTableReader<T> where T : struct
	{
		// Token: 0x06003829 RID: 14377 RVA: 0x00177B40 File Offset: 0x00175D40
		[NullableContext(1)]
		private SegmentTableReader(IReadMessage message, [Nullable(new byte[]
		{
			1,
			0,
			0
		})] List<Segment<T>> segments, int pointerLocation, int exitLocation)
		{
			this.message = message;
			this.segments = segments;
			this.PointerLocation = pointerLocation;
			this.exitLocation = exitLocation;
		}

		// Token: 0x17000F65 RID: 3941
		// (get) Token: 0x0600382A RID: 14378 RVA: 0x00177B5F File Offset: 0x00175D5F
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		public IReadOnlyList<Segment<T>> Segments
		{
			[return: Nullable(new byte[]
			{
				1,
				0,
				0
			})]
			get
			{
				return this.segments;
			}
		}

		// Token: 0x0600382B RID: 14379 RVA: 0x00177B68 File Offset: 0x00175D68
		[NullableContext(1)]
		public static void Read(IReadMessage msg, [Nullable(new byte[]
		{
			1,
			0
		})] SegmentTableReader<T>.SegmentDataReader segmentDataReader, [Nullable(new byte[]
		{
			2,
			0
		})] SegmentTableReader<T>.ExceptionHandler exceptionHandler = null)
		{
			int pointerLocation = msg.BitPosition;
			int tablePointer = msg.ReadInt32();
			int tableLocation = pointerLocation + tablePointer;
			int returnPosition = msg.BitPosition;
			List<Segment<T>> segments = new List<Segment<T>>();
			msg.BitPosition = tableLocation;
			int numSegments = (int)msg.ReadUInt16();
			for (int i = 0; i < numSegments; i++)
			{
				segments.Add(INetSerializableStruct.Read<Segment<T>>(msg));
			}
			int exitLocation = msg.BitPosition;
			msg.BitPosition = returnPosition;
			using (SegmentTableReader<T> segmentTable = new SegmentTableReader<T>(msg, segments, pointerLocation, exitLocation))
			{
				for (int j = 0; j < segmentTable.Segments.Count; j++)
				{
					Segment<T> segment = segmentTable.Segments[j];
					msg.BitPosition = segmentTable.PointerLocation + segment.Pointer;
					try
					{
						if (segmentDataReader(segment.Identifier, new SegmentTableReader<T>.SegmentReadMsg(msg, segments, j, segmentTable.PointerLocation + segment.Pointer, ((j < segmentTable.Segments.Count - 1) ? segments[j + 1].Pointer : tablePointer) - segment.Pointer)) == SegmentTableReader<T>.BreakSegmentReading.Yes)
						{
							break;
						}
					}
					catch (Exception e)
					{
						Segment<T>[] prevSegments = segments.Take(j).ToArray<Segment<T>>();
						if (exceptionHandler == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Exception thrown while reading segment ");
							defaultInterpolatedStringHandler.AppendFormatted<T>(segment.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" at position ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(segment.Pointer);
							defaultInterpolatedStringHandler.AppendLiteral(".");
							throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear() + (prevSegments.Any<Segment<T>>() ? (" Previous segments: " + string.Join<Segment<T>>(", ", prevSegments) + ".") : ""), e);
						}
						exceptionHandler(segment, prevSegments, e);
					}
				}
			}
		}

		// Token: 0x0600382C RID: 14380 RVA: 0x00177D78 File Offset: 0x00175F78
		public void Dispose()
		{
			this.message.BitPosition = this.exitLocation;
		}

		// Token: 0x04001C34 RID: 7220
		[Nullable(1)]
		private readonly IReadMessage message;

		// Token: 0x04001C35 RID: 7221
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		private readonly List<Segment<T>> segments;

		// Token: 0x04001C36 RID: 7222
		private readonly int exitLocation;

		// Token: 0x04001C37 RID: 7223
		public readonly int PointerLocation;

		// Token: 0x02000C52 RID: 3154
		[NullableContext(1)]
		[Nullable(0)]
		private class SegmentReadMsg : IReadMessage
		{
			// Token: 0x060063C0 RID: 25536 RVA: 0x002131BC File Offset: 0x002113BC
			public SegmentReadMsg(IReadMessage underlyingMsg, [Nullable(new byte[]
			{
				1,
				0,
				0
			})] IReadOnlyList<Segment<T>> segments, int segmentIndex, int offset, int lengthBits)
			{
				this.underlyingMsg = underlyingMsg;
				this.segments = segments;
				this.segmentIndex = segmentIndex;
				this.offset = offset;
				this.lengthBits = lengthBits;
				if (offset + lengthBits >= underlyingMsg.LengthBits)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Segment table is corrupt, segment length is invalid: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(offset);
					defaultInterpolatedStringHandler.AppendLiteral(" + ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(lengthBits);
					defaultInterpolatedStringHandler.AppendLiteral(" >= ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(underlyingMsg.LengthBits);
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}

			// Token: 0x060063C1 RID: 25537 RVA: 0x0021325B File Offset: 0x0021145B
			private void Check()
			{
				if (this.BitPosition > this.lengthBits)
				{
					throw new Exception("Tried to read too much data from segment.");
				}
			}

			// Token: 0x060063C2 RID: 25538 RVA: 0x00213276 File Offset: 0x00211476
			private TRead Check<[Nullable(2)] TRead>(TRead v)
			{
				this.Check();
				return v;
			}

			// Token: 0x060063C3 RID: 25539 RVA: 0x0021327F File Offset: 0x0021147F
			public bool ReadBoolean()
			{
				return this.Check<bool>(this.underlyingMsg.ReadBoolean());
			}

			// Token: 0x060063C4 RID: 25540 RVA: 0x00213292 File Offset: 0x00211492
			public void ReadPadBits()
			{
				this.Check();
				this.underlyingMsg.ReadPadBits();
			}

			// Token: 0x060063C5 RID: 25541 RVA: 0x002132A5 File Offset: 0x002114A5
			public byte ReadByte()
			{
				return this.Check<byte>(this.underlyingMsg.ReadByte());
			}

			// Token: 0x060063C6 RID: 25542 RVA: 0x002132B8 File Offset: 0x002114B8
			public byte PeekByte()
			{
				return this.Check<byte>(this.underlyingMsg.PeekByte());
			}

			// Token: 0x060063C7 RID: 25543 RVA: 0x002132CB File Offset: 0x002114CB
			public ushort ReadUInt16()
			{
				return this.Check<ushort>(this.underlyingMsg.ReadUInt16());
			}

			// Token: 0x060063C8 RID: 25544 RVA: 0x002132DE File Offset: 0x002114DE
			public short ReadInt16()
			{
				return this.Check<short>(this.underlyingMsg.ReadInt16());
			}

			// Token: 0x060063C9 RID: 25545 RVA: 0x002132F1 File Offset: 0x002114F1
			public uint ReadUInt32()
			{
				return this.Check<uint>(this.underlyingMsg.ReadUInt32());
			}

			// Token: 0x060063CA RID: 25546 RVA: 0x00213304 File Offset: 0x00211504
			public int ReadInt32()
			{
				return this.Check<int>(this.underlyingMsg.ReadInt32());
			}

			// Token: 0x060063CB RID: 25547 RVA: 0x00213317 File Offset: 0x00211517
			public ulong ReadUInt64()
			{
				return this.Check<ulong>(this.underlyingMsg.ReadUInt64());
			}

			// Token: 0x060063CC RID: 25548 RVA: 0x0021332A File Offset: 0x0021152A
			public long ReadInt64()
			{
				return this.Check<long>(this.underlyingMsg.ReadInt64());
			}

			// Token: 0x060063CD RID: 25549 RVA: 0x0021333D File Offset: 0x0021153D
			public float ReadSingle()
			{
				return this.Check<float>(this.underlyingMsg.ReadSingle());
			}

			// Token: 0x060063CE RID: 25550 RVA: 0x00213350 File Offset: 0x00211550
			public double ReadDouble()
			{
				return this.Check<double>(this.underlyingMsg.ReadDouble());
			}

			// Token: 0x060063CF RID: 25551 RVA: 0x00213363 File Offset: 0x00211563
			public uint ReadVariableUInt32()
			{
				return this.Check<uint>(this.underlyingMsg.ReadVariableUInt32());
			}

			// Token: 0x060063D0 RID: 25552 RVA: 0x00213376 File Offset: 0x00211576
			public string ReadString()
			{
				return this.Check<string>(this.underlyingMsg.ReadString());
			}

			// Token: 0x060063D1 RID: 25553 RVA: 0x00213389 File Offset: 0x00211589
			public Identifier ReadIdentifier()
			{
				return this.Check<Identifier>(this.underlyingMsg.ReadIdentifier());
			}

			// Token: 0x060063D2 RID: 25554 RVA: 0x0021339C File Offset: 0x0021159C
			public Color ReadColorR8G8B8()
			{
				return this.Check<Color>(this.underlyingMsg.ReadColorR8G8B8());
			}

			// Token: 0x060063D3 RID: 25555 RVA: 0x002133AF File Offset: 0x002115AF
			public Color ReadColorR8G8B8A8()
			{
				return this.Check<Color>(this.underlyingMsg.ReadColorR8G8B8A8());
			}

			// Token: 0x060063D4 RID: 25556 RVA: 0x002133C2 File Offset: 0x002115C2
			public int ReadRangedInteger(int min, int max)
			{
				return this.Check<int>(this.underlyingMsg.ReadRangedInteger(min, max));
			}

			// Token: 0x060063D5 RID: 25557 RVA: 0x002133D7 File Offset: 0x002115D7
			public float ReadRangedSingle(float min, float max, int bitCount)
			{
				return this.Check<float>(this.underlyingMsg.ReadRangedSingle(min, max, bitCount));
			}

			// Token: 0x060063D6 RID: 25558 RVA: 0x002133ED File Offset: 0x002115ED
			public byte[] ReadBytes(int numberOfBytes)
			{
				return this.Check<byte[]>(this.underlyingMsg.ReadBytes(numberOfBytes));
			}

			// Token: 0x17001628 RID: 5672
			// (get) Token: 0x060063D7 RID: 25559 RVA: 0x00213401 File Offset: 0x00211601
			// (set) Token: 0x060063D8 RID: 25560 RVA: 0x00213418 File Offset: 0x00211618
			public int BitPosition
			{
				get
				{
					return this.underlyingMsg.BitPosition - this.offset;
				}
				set
				{
					this.Check<int>(this.underlyingMsg.BitPosition = value + this.offset);
				}
			}

			// Token: 0x17001629 RID: 5673
			// (get) Token: 0x060063D9 RID: 25561 RVA: 0x00213442 File Offset: 0x00211642
			public int BytePosition
			{
				get
				{
					return this.BitPosition / 8;
				}
			}

			// Token: 0x1700162A RID: 5674
			// (get) Token: 0x060063DA RID: 25562 RVA: 0x0021344C File Offset: 0x0021164C
			public byte[] Buffer
			{
				get
				{
					return this.underlyingMsg.Buffer;
				}
			}

			// Token: 0x1700162B RID: 5675
			// (get) Token: 0x060063DB RID: 25563 RVA: 0x00213459 File Offset: 0x00211659
			// (set) Token: 0x060063DC RID: 25564 RVA: 0x00213461 File Offset: 0x00211661
			public int LengthBits
			{
				get
				{
					return this.lengthBits;
				}
				set
				{
					throw new InvalidOperationException("Cannot resize SegmentReadMsg");
				}
			}

			// Token: 0x1700162C RID: 5676
			// (get) Token: 0x060063DD RID: 25565 RVA: 0x0021346D File Offset: 0x0021166D
			public int LengthBytes
			{
				get
				{
					return this.lengthBits / 8;
				}
			}

			// Token: 0x1700162D RID: 5677
			// (get) Token: 0x060063DE RID: 25566 RVA: 0x00213477 File Offset: 0x00211677
			public NetworkConnection Sender
			{
				get
				{
					return this.underlyingMsg.Sender;
				}
			}

			// Token: 0x04003C1C RID: 15388
			private readonly IReadMessage underlyingMsg;

			// Token: 0x04003C1D RID: 15389
			[Nullable(new byte[]
			{
				1,
				0,
				0
			})]
			private readonly IReadOnlyList<Segment<T>> segments;

			// Token: 0x04003C1E RID: 15390
			private readonly int segmentIndex;

			// Token: 0x04003C1F RID: 15391
			private readonly int offset;

			// Token: 0x04003C20 RID: 15392
			private readonly int lengthBits;
		}

		// Token: 0x02000C53 RID: 3155
		public enum BreakSegmentReading
		{
			// Token: 0x04003C22 RID: 15394
			No,
			// Token: 0x04003C23 RID: 15395
			Yes
		}

		// Token: 0x02000C54 RID: 3156
		// (Invoke) Token: 0x060063E0 RID: 25568
		public delegate SegmentTableReader<T>.BreakSegmentReading SegmentDataReader(T segmentHeader, [Nullable(1)] IReadMessage incMsg);

		// Token: 0x02000C55 RID: 3157
		// (Invoke) Token: 0x060063E4 RID: 25572
		public delegate void ExceptionHandler(Segment<T> segmentWithError, [Nullable(new byte[]
		{
			1,
			0,
			0
		})] Segment<T>[] previousSegments, [Nullable(1)] Exception exceptionThrown);
	}
}
