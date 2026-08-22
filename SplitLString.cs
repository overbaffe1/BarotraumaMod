using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000370 RID: 880
	[NullableContext(1)]
	[Nullable(0)]
	public class SplitLString : LocalizedString
	{
		// Token: 0x06004369 RID: 17257 RVA: 0x0025325D File Offset: 0x0025145D
		public SplitLString(LStringSplitter splitter, int index)
		{
			this.splitter = splitter;
			this.index = index;
		}

		// Token: 0x170011A9 RID: 4521
		// (get) Token: 0x0600436A RID: 17258 RVA: 0x00253273 File Offset: 0x00251473
		public override bool Loaded
		{
			get
			{
				return this.loaded && this.splitter.Loaded;
			}
		}

		// Token: 0x0600436B RID: 17259 RVA: 0x0025328A File Offset: 0x0025148A
		public override void RetrieveValue()
		{
			this.loaded = true;
			this.cachedValue = this.splitter.GetValue(this.index);
			base.UpdateLanguage();
		}

		// Token: 0x0400235B RID: 9051
		private bool loaded;

		// Token: 0x0400235C RID: 9052
		private readonly LStringSplitter splitter;

		// Token: 0x0400235D RID: 9053
		private readonly int index;
	}
}
