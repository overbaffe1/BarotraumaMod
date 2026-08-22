using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000389 RID: 905
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class IdRemap
	{
		// Token: 0x06004437 RID: 17463 RVA: 0x00263978 File Offset: 0x00261B78
		[NullableContext(2)]
		public IdRemap(XElement parentElement, int offset)
		{
			this.destOffset = offset;
			if (parentElement != null && parentElement.HasElements)
			{
				this.srcRanges = new List<Range<int>>();
				foreach (XElement subElement in parentElement.Elements())
				{
					int id = subElement.GetAttributeInt("ID", -1);
					if (id > 0)
					{
						this.InsertId(id);
					}
				}
				this.maxId = (int)this.GetOffsetId(this.srcRanges.Any<Range<int>>() ? this.srcRanges.Last<Range<int>>().End : offset);
				return;
			}
			this.maxId = offset;
		}

		// Token: 0x06004438 RID: 17464 RVA: 0x00263A34 File Offset: 0x00261C34
		public void AssignMaxId(out ushort result)
		{
			this.maxId++;
			result = (ushort)this.maxId;
		}

		// Token: 0x06004439 RID: 17465 RVA: 0x00263A50 File Offset: 0x00261C50
		private void InsertId(int id)
		{
			if (this.srcRanges == null)
			{
				throw new NullReferenceException("Called InsertId when srcRanges is null");
			}
			int insertIndex = this.srcRanges.Count;
			for (int i = 0; i < this.srcRanges.Count; i++)
			{
				if (this.srcRanges[i].Contains(id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Duplicate ID: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(id);
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				if (this.srcRanges[i].Start > id)
				{
					insertIndex = i;
					break;
				}
			}
			this.srcRanges.Insert(insertIndex, new Range<int>(id, id));
			this.<InsertId>g__tryMergeRangeWithNext|6_0(insertIndex);
			this.<InsertId>g__tryMergeRangeWithNext|6_0(insertIndex - 1);
		}

		// Token: 0x0600443A RID: 17466 RVA: 0x00263B13 File Offset: 0x00261D13
		public ushort GetOffsetId(XElement element)
		{
			return this.GetOffsetId(element.GetAttributeInt("ID", 0));
		}

		// Token: 0x0600443B RID: 17467 RVA: 0x00263B28 File Offset: 0x00261D28
		public ushort GetOffsetId(int id)
		{
			if (id <= 0)
			{
				return 0;
			}
			if (this.destOffset < 0)
			{
				return 0;
			}
			if (this.srcRanges == null)
			{
				return (ushort)(id + this.destOffset);
			}
			int currOffset = this.destOffset;
			for (int i = 0; i < this.srcRanges.Count; i++)
			{
				Range<int> range = this.srcRanges[i];
				if (range.Contains(id))
				{
					int num = id;
					range = this.srcRanges[i];
					return (ushort)(num - range.Start + currOffset);
				}
				int num2 = currOffset;
				range = this.srcRanges[i];
				currOffset = num2 + IdRemap.<GetOffsetId>g__rangeSize|8_0(range);
			}
			return 0;
		}

		// Token: 0x0600443C RID: 17468 RVA: 0x00263BC0 File Offset: 0x00261DC0
		public static ushort DetermineNewOffset()
		{
			int largestEntityId = 0;
			foreach (Entity e in Entity.GetEntities())
			{
				if (e.ID <= 65532 && !(e is Submarine))
				{
					largestEntityId = Math.Max(largestEntityId, (int)e.ID);
				}
			}
			return (ushort)(largestEntityId + 1);
		}

		// Token: 0x0600443E RID: 17470 RVA: 0x00263C40 File Offset: 0x00261E40
		[CompilerGenerated]
		private void <InsertId>g__tryMergeRangeWithNext|6_0(int indexA)
		{
			int indexB = indexA + 1;
			if (indexA < 0 || indexB >= this.srcRanges.Count)
			{
				return;
			}
			Range<int> rangeA = this.srcRanges[indexA];
			Range<int> rangeB = this.srcRanges[indexB];
			if (rangeA.End + 1 >= rangeB.Start)
			{
				this.srcRanges[indexA] = new Range<int>(rangeA.Start, rangeB.End);
				this.srcRanges.RemoveAt(indexB);
			}
		}

		// Token: 0x0600443F RID: 17471 RVA: 0x00263CBC File Offset: 0x00261EBC
		[NullableContext(0)]
		[CompilerGenerated]
		internal static int <GetOffsetId>g__rangeSize|8_0(in Range<int> r)
		{
			Range<int> range = r;
			int end = range.End;
			range = r;
			return end - range.Start + 1;
		}

		// Token: 0x040023C9 RID: 9161
		public static readonly IdRemap DiscardId = new IdRemap(null, -1);

		// Token: 0x040023CA RID: 9162
		private int maxId;

		// Token: 0x040023CB RID: 9163
		[Nullable(new byte[]
		{
			2,
			0
		})]
		private readonly List<Range<int>> srcRanges;

		// Token: 0x040023CC RID: 9164
		private readonly int destOffset;
	}
}
