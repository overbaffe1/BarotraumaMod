using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A3 RID: 675
	[NullableContext(1)]
	[Nullable(0)]
	public class LStringSplitter
	{
		// Token: 0x17000DA5 RID: 3493
		// (get) Token: 0x06002ECE RID: 11982 RVA: 0x00138D28 File Offset: 0x00136F28
		public IReadOnlyList<LocalizedString> Substrings
		{
			get
			{
				return this.substrings;
			}
		}

		// Token: 0x17000DA6 RID: 3494
		// (get) Token: 0x06002ECF RID: 11983 RVA: 0x00138D30 File Offset: 0x00136F30
		public bool Loaded
		{
			get
			{
				return this.originalString.Loaded;
			}
		}

		// Token: 0x06002ED0 RID: 11984 RVA: 0x00138D3D File Offset: 0x00136F3D
		public LStringSplitter(LocalizedString input, params char[] separators)
		{
			this.originalString = input;
			this.substrings = new LStringSplitter.SubstringList(this);
			this.substrValues = Array.Empty<string>();
			this.separators = separators;
			this.cachedOriginal = "";
		}

		// Token: 0x06002ED1 RID: 11985 RVA: 0x00138D78 File Offset: 0x00136F78
		private void UpdateSubstrings()
		{
			if (this.originalString.Value != this.cachedOriginal)
			{
				this.cachedOriginal = this.originalString.Value;
				this.substrValues = this.cachedOriginal.Split(this.separators);
				this.substrings.UnderlyingList.Clear();
				this.substrings.UnderlyingList.AddRange(from i in Enumerable.Range(0, this.substrValues.Length)
				select new SplitLString(this, i));
			}
		}

		// Token: 0x06002ED2 RID: 11986 RVA: 0x00138E04 File Offset: 0x00137004
		public string GetValue(int index)
		{
			this.UpdateSubstrings();
			return this.substrValues[index];
		}

		// Token: 0x04001776 RID: 6006
		private readonly LStringSplitter.SubstringList substrings;

		// Token: 0x04001777 RID: 6007
		private readonly char[] separators;

		// Token: 0x04001778 RID: 6008
		private readonly LocalizedString originalString;

		// Token: 0x04001779 RID: 6009
		private string[] substrValues;

		// Token: 0x0400177A RID: 6010
		private string cachedOriginal;

		// Token: 0x02000B2D RID: 2861
		[Nullable(0)]
		private class SubstringList : IReadOnlyList<LocalizedString>, IEnumerable<LocalizedString>, IEnumerable, IReadOnlyCollection<LocalizedString>
		{
			// Token: 0x06005FF2 RID: 24562 RVA: 0x00208E37 File Offset: 0x00207037
			public SubstringList(LStringSplitter splitter)
			{
				this.splitter = splitter;
			}

			// Token: 0x170015D9 RID: 5593
			// (get) Token: 0x06005FF3 RID: 24563 RVA: 0x00208E51 File Offset: 0x00207051
			public List<LocalizedString> UnderlyingList
			{
				get
				{
					this.splitter.UpdateSubstrings();
					return this.underlyingList;
				}
			}

			// Token: 0x06005FF4 RID: 24564 RVA: 0x00208E64 File Offset: 0x00207064
			public IEnumerator<LocalizedString> GetEnumerator()
			{
				return this.UnderlyingList.GetEnumerator();
			}

			// Token: 0x06005FF5 RID: 24565 RVA: 0x00208E76 File Offset: 0x00207076
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x170015DA RID: 5594
			// (get) Token: 0x06005FF6 RID: 24566 RVA: 0x00208E7E File Offset: 0x0020707E
			public int Count
			{
				get
				{
					return this.UnderlyingList.Count;
				}
			}

			// Token: 0x170015DB RID: 5595
			public LocalizedString this[int index]
			{
				get
				{
					return this.UnderlyingList[index];
				}
			}

			// Token: 0x040038C9 RID: 14537
			private LStringSplitter splitter;

			// Token: 0x040038CA RID: 14538
			private readonly List<LocalizedString> underlyingList = new List<LocalizedString>();
		}
	}
}
