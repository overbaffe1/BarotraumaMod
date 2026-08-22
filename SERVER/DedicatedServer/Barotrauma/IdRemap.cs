using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002BE RID: 702
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class IdRemap
	{
		// Token: 0x06002FB9 RID: 12217 RVA: 0x00149ED4 File Offset: 0x001480D4
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

		// Token: 0x06002FBA RID: 12218 RVA: 0x00149F90 File Offset: 0x00148190
		public void AssignMaxId(out ushort result)
		{
			this.maxId++;
			result = (ushort)this.maxId;
		}

		// Token: 0x06002FBB RID: 12219 RVA: 0x00149FAC File Offset: 0x001481AC
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

		// Token: 0x06002FBC RID: 12220 RVA: 0x0014A06F File Offset: 0x0014826F
		public ushort GetOffsetId(XElement element)
		{
			return this.GetOffsetId(element.GetAttributeInt("ID", 0));
		}

		// Token: 0x06002FBD RID: 12221 RVA: 0x0014A084 File Offset: 0x00148284
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

		// Token: 0x06002FBE RID: 12222 RVA: 0x0014A11C File Offset: 0x0014831C
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

		// Token: 0x06002FC0 RID: 12224 RVA: 0x0014A19C File Offset: 0x0014839C
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

		// Token: 0x06002FC1 RID: 12225 RVA: 0x0014A218 File Offset: 0x00148418
		[NullableContext(0)]
		[CompilerGenerated]
		internal static int <GetOffsetId>g__rangeSize|8_0(in Range<int> r)
		{
			Range<int> range = r;
			int end = range.End;
			range = r;
			return end - range.Start + 1;
		}

		// Token: 0x040017F7 RID: 6135
		public static readonly IdRemap DiscardId = new IdRemap(null, -1);

		// Token: 0x040017F8 RID: 6136
		private int maxId;

		// Token: 0x040017F9 RID: 6137
		[Nullable(new byte[]
		{
			2,
			0
		})]
		private readonly List<Range<int>> srcRanges;

		// Token: 0x040017FA RID: 6138
		private readonly int destOffset;
	}
}
