using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A4 RID: 676
	[NullableContext(1)]
	[Nullable(0)]
	public class SplitLString : LocalizedString
	{
		// Token: 0x06002ED4 RID: 11988 RVA: 0x00138E1D File Offset: 0x0013701D
		public SplitLString(LStringSplitter splitter, int index)
		{
			this.splitter = splitter;
			this.index = index;
		}

		// Token: 0x17000DA7 RID: 3495
		// (get) Token: 0x06002ED5 RID: 11989 RVA: 0x00138E33 File Offset: 0x00137033
		public override bool Loaded
		{
			get
			{
				return this.loaded && this.splitter.Loaded;
			}
		}

		// Token: 0x06002ED6 RID: 11990 RVA: 0x00138E4A File Offset: 0x0013704A
		public override void RetrieveValue()
		{
			this.loaded = true;
			this.cachedValue = this.splitter.GetValue(this.index);
			base.UpdateLanguage();
		}

		// Token: 0x0400177B RID: 6011
		private bool loaded;

		// Token: 0x0400177C RID: 6012
		private readonly LStringSplitter splitter;

		// Token: 0x0400177D RID: 6013
		private readonly int index;
	}
}
