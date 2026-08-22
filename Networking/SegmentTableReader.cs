using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004CC RID: 1228
	[CompilerFeatureRequired("RefStructs")]
	internal readonly ref struct SegmentTableReader<T> where T : struct
	{
		// Token: 0x06004FF7 RID: 20471 RVA: 0x002B0208 File Offset: 0x002AE408
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

		// Token: 0x1700145F RID: 5215
		// (get) Token: 0x06004FF8 RID: 20472 RVA: 0x002B0227 File Offset: 0x002AE427
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

		// Token: 0x06004FF9 RID: 20473 RVA: 0x002B0230 File Offset: 0x002AE430
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

		// Token: 0x06004FFA RID: 20474 RVA: 0x002B0440 File Offset: 0x002AE640
		public void Dispose()
		{
			this.message.BitPosition = this.exitLocation;
		}

		// Token: 0x04002A1F RID: 10783
		[Nullable(1)]
		private readonly IReadMessage message;

		// Token: 0x04002A20 RID: 10784
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		private readonly List<Segment<T>> segments;

		// Token: 0x04002A21 RID: 10785
		private readonly int exitLocation;

		// Token: 0x04002A22 RID: 10786
		public readonly int PointerLocation;

		// Token: 0x0200124F RID: 4687
		[NullableContext(1)]
		[Nullable(0)]
		private class SegmentReadMsg : IReadMessage
		{
			// Token: 0x060093D6 RID: 37846 RVA: 0x003CE018 File Offset: 0x003CC218
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

			// Token: 0x060093D7 RID: 37847 RVA: 0x003CE0B7 File Offset: 0x003CC2B7
			private void Check()
			{
				if (this.BitPosition > this.lengthBits)
				{
					throw new Exception("Tried to read too much data from segment.");
				}
			}

			// Token: 0x060093D8 RID: 37848 RVA: 0x003CE0D2 File Offset: 0x003CC2D2
			private TRead Check<[Nullable(2)] TRead>(TRead v)
			{
				this.Check();
				return v;
			}

			// Token: 0x060093D9 RID: 37849 RVA: 0x003CE0DB File Offset: 0x003CC2DB
			public bool ReadBoolean()
			{
				return this.Check<bool>(this.underlyingMsg.ReadBoolean());
			}

			// Token: 0x060093DA RID: 37850 RVA: 0x003CE0EE File Offset: 0x003CC2EE
			public void ReadPadBits()
			{
				this.Check();
				this.underlyingMsg.ReadPadBits();
			}

			// Token: 0x060093DB RID: 37851 RVA: 0x003CE101 File Offset: 0x003CC301
			public byte ReadByte()
			{
				return this.Check<byte>(this.underlyingMsg.ReadByte());
			}

			// Token: 0x060093DC RID: 37852 RVA: 0x003CE114 File Offset: 0x003CC314
			public byte PeekByte()
			{
				return this.Check<byte>(this.underlyingMsg.PeekByte());
			}

			// Token: 0x060093DD RID: 37853 RVA: 0x003CE127 File Offset: 0x003CC327
			public ushort ReadUInt16()
			{
				return this.Check<ushort>(this.underlyingMsg.ReadUInt16());
			}

			// Token: 0x060093DE RID: 37854 RVA: 0x003CE13A File Offset: 0x003CC33A
			public short ReadInt16()
			{
				return this.Check<short>(this.underlyingMsg.ReadInt16());
			}

			// Token: 0x060093DF RID: 37855 RVA: 0x003CE14D File Offset: 0x003CC34D
			public uint ReadUInt32()
			{
				return this.Check<uint>(this.underlyingMsg.ReadUInt32());
			}

			// Token: 0x060093E0 RID: 37856 RVA: 0x003CE160 File Offset: 0x003CC360
			public int ReadInt32()
			{
				return this.Check<int>(this.underlyingMsg.ReadInt32());
			}

			// Token: 0x060093E1 RID: 37857 RVA: 0x003CE173 File Offset: 0x003CC373
			public ulong ReadUInt64()
			{
				return this.Check<ulong>(this.underlyingMsg.ReadUInt64());
			}

			// Token: 0x060093E2 RID: 37858 RVA: 0x003CE186 File Offset: 0x003CC386
			public long ReadInt64()
			{
				return this.Check<long>(this.underlyingMsg.ReadInt64());
			}

			// Token: 0x060093E3 RID: 37859 RVA: 0x003CE199 File Offset: 0x003CC399
			public float ReadSingle()
			{
				return this.Check<float>(this.underlyingMsg.ReadSingle());
			}

			// Token: 0x060093E4 RID: 37860 RVA: 0x003CE1AC File Offset: 0x003CC3AC
			public double ReadDouble()
			{
				return this.Check<double>(this.underlyingMsg.ReadDouble());
			}

			// Token: 0x060093E5 RID: 37861 RVA: 0x003CE1BF File Offset: 0x003CC3BF
			public uint ReadVariableUInt32()
			{
				return this.Check<uint>(this.underlyingMsg.ReadVariableUInt32());
			}

			// Token: 0x060093E6 RID: 37862 RVA: 0x003CE1D2 File Offset: 0x003CC3D2
			public string ReadString()
			{
				return this.Check<string>(this.underlyingMsg.ReadString());
			}

			// Token: 0x060093E7 RID: 37863 RVA: 0x003CE1E5 File Offset: 0x003CC3E5
			public Identifier ReadIdentifier()
			{
				return this.Check<Identifier>(this.underlyingMsg.ReadIdentifier());
			}

			// Token: 0x060093E8 RID: 37864 RVA: 0x003CE1F8 File Offset: 0x003CC3F8
			public Color ReadColorR8G8B8()
			{
				return this.Check<Color>(this.underlyingMsg.ReadColorR8G8B8());
			}

			// Token: 0x060093E9 RID: 37865 RVA: 0x003CE20B File Offset: 0x003CC40B
			public Color ReadColorR8G8B8A8()
			{
				return this.Check<Color>(this.underlyingMsg.ReadColorR8G8B8A8());
			}

			// Token: 0x060093EA RID: 37866 RVA: 0x003CE21E File Offset: 0x003CC41E
			public int ReadRangedInteger(int min, int max)
			{
				return this.Check<int>(this.underlyingMsg.ReadRangedInteger(min, max));
			}

			// Token: 0x060093EB RID: 37867 RVA: 0x003CE233 File Offset: 0x003CC433
			public float ReadRangedSingle(float min, float max, int bitCount)
			{
				return this.Check<float>(this.underlyingMsg.ReadRangedSingle(min, max, bitCount));
			}

			// Token: 0x060093EC RID: 37868 RVA: 0x003CE249 File Offset: 0x003CC449
			public byte[] ReadBytes(int numberOfBytes)
			{
				return this.Check<byte[]>(this.underlyingMsg.ReadBytes(numberOfBytes));
			}

			// Token: 0x17001CE7 RID: 7399
			// (get) Token: 0x060093ED RID: 37869 RVA: 0x003CE25D File Offset: 0x003CC45D
			// (set) Token: 0x060093EE RID: 37870 RVA: 0x003CE274 File Offset: 0x003CC474
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

			// Token: 0x17001CE8 RID: 7400
			// (get) Token: 0x060093EF RID: 37871 RVA: 0x003CE29E File Offset: 0x003CC49E
			public int BytePosition
			{
				get
				{
					return this.BitPosition / 8;
				}
			}

			// Token: 0x17001CE9 RID: 7401
			// (get) Token: 0x060093F0 RID: 37872 RVA: 0x003CE2A8 File Offset: 0x003CC4A8
			public byte[] Buffer
			{
				get
				{
					return this.underlyingMsg.Buffer;
				}
			}

			// Token: 0x17001CEA RID: 7402
			// (get) Token: 0x060093F1 RID: 37873 RVA: 0x003CE2B5 File Offset: 0x003CC4B5
			// (set) Token: 0x060093F2 RID: 37874 RVA: 0x003CE2BD File Offset: 0x003CC4BD
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

			// Token: 0x17001CEB RID: 7403
			// (get) Token: 0x060093F3 RID: 37875 RVA: 0x003CE2C9 File Offset: 0x003CC4C9
			public int LengthBytes
			{
				get
				{
					return this.lengthBits / 8;
				}
			}

			// Token: 0x17001CEC RID: 7404
			// (get) Token: 0x060093F4 RID: 37876 RVA: 0x003CE2D3 File Offset: 0x003CC4D3
			public NetworkConnection Sender
			{
				get
				{
					return this.underlyingMsg.Sender;
				}
			}

			// Token: 0x04005ED3 RID: 24275
			private readonly IReadMessage underlyingMsg;

			// Token: 0x04005ED4 RID: 24276
			[Nullable(new byte[]
			{
				1,
				0,
				0
			})]
			private readonly IReadOnlyList<Segment<T>> segments;

			// Token: 0x04005ED5 RID: 24277
			private readonly int segmentIndex;

			// Token: 0x04005ED6 RID: 24278
			private readonly int offset;

			// Token: 0x04005ED7 RID: 24279
			private readonly int lengthBits;
		}

		// Token: 0x02001250 RID: 4688
		public enum BreakSegmentReading
		{
			// Token: 0x04005ED9 RID: 24281
			No,
			// Token: 0x04005EDA RID: 24282
			Yes
		}

		// Token: 0x02001251 RID: 4689
		// (Invoke) Token: 0x060093F6 RID: 37878
		public delegate SegmentTableReader<T>.BreakSegmentReading SegmentDataReader(T segmentHeader, [Nullable(1)] IReadMessage incMsg);

		// Token: 0x02001252 RID: 4690
		// (Invoke) Token: 0x060093FA RID: 37882
		public delegate void ExceptionHandler(Segment<T> segmentWithError, [Nullable(new byte[]
		{
			1,
			0,
			0
		})] Segment<T>[] previousSegments, [Nullable(1)] Exception exceptionThrown);
	}
}
