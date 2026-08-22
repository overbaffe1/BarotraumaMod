using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200036F RID: 879
	[NullableContext(1)]
	[Nullable(0)]
	public class LStringSplitter
	{
		// Token: 0x170011A7 RID: 4519
		// (get) Token: 0x06004363 RID: 17251 RVA: 0x00253168 File Offset: 0x00251368
		public IReadOnlyList<LocalizedString> Substrings
		{
			get
			{
				return this.substrings;
			}
		}

		// Token: 0x170011A8 RID: 4520
		// (get) Token: 0x06004364 RID: 17252 RVA: 0x00253170 File Offset: 0x00251370
		public bool Loaded
		{
			get
			{
				return this.originalString.Loaded;
			}
		}

		// Token: 0x06004365 RID: 17253 RVA: 0x0025317D File Offset: 0x0025137D
		public LStringSplitter(LocalizedString input, params char[] separators)
		{
			this.originalString = input;
			this.substrings = new LStringSplitter.SubstringList(this);
			this.substrValues = Array.Empty<string>();
			this.separators = separators;
			this.cachedOriginal = "";
		}

		// Token: 0x06004366 RID: 17254 RVA: 0x002531B8 File Offset: 0x002513B8
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

		// Token: 0x06004367 RID: 17255 RVA: 0x00253244 File Offset: 0x00251444
		public string GetValue(int index)
		{
			this.UpdateSubstrings();
			return this.substrValues[index];
		}

		// Token: 0x04002356 RID: 9046
		private readonly LStringSplitter.SubstringList substrings;

		// Token: 0x04002357 RID: 9047
		private readonly char[] separators;

		// Token: 0x04002358 RID: 9048
		private readonly LocalizedString originalString;

		// Token: 0x04002359 RID: 9049
		private string[] substrValues;

		// Token: 0x0400235A RID: 9050
		private string cachedOriginal;

		// Token: 0x02001089 RID: 4233
		[Nullable(0)]
		private class SubstringList : IReadOnlyList<LocalizedString>, IEnumerable<LocalizedString>, IEnumerable, IReadOnlyCollection<LocalizedString>
		{
			// Token: 0x06008CFA RID: 36090 RVA: 0x003B0C07 File Offset: 0x003AEE07
			public SubstringList(LStringSplitter splitter)
			{
				this.splitter = splitter;
			}

			// Token: 0x17001C73 RID: 7283
			// (get) Token: 0x06008CFB RID: 36091 RVA: 0x003B0C21 File Offset: 0x003AEE21
			public List<LocalizedString> UnderlyingList
			{
				get
				{
					this.splitter.UpdateSubstrings();
					return this.underlyingList;
				}
			}

			// Token: 0x06008CFC RID: 36092 RVA: 0x003B0C34 File Offset: 0x003AEE34
			public IEnumerator<LocalizedString> GetEnumerator()
			{
				return this.UnderlyingList.GetEnumerator();
			}

			// Token: 0x06008CFD RID: 36093 RVA: 0x003B0C46 File Offset: 0x003AEE46
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x17001C74 RID: 7284
			// (get) Token: 0x06008CFE RID: 36094 RVA: 0x003B0C4E File Offset: 0x003AEE4E
			public int Count
			{
				get
				{
					return this.UnderlyingList.Count;
				}
			}

			// Token: 0x17001C75 RID: 7285
			public LocalizedString this[int index]
			{
				get
				{
					return this.UnderlyingList[index];
				}
			}

			// Token: 0x040058FD RID: 22781
			private LStringSplitter splitter;

			// Token: 0x040058FE RID: 22782
			private readonly List<LocalizedString> underlyingList = new List<LocalizedString>();
		}
	}
}
