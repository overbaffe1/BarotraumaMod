using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000299 RID: 665
	[NullableContext(1)]
	[Nullable(0)]
	public class ConcatLString : LocalizedString
	{
		// Token: 0x06002E7F RID: 11903 RVA: 0x0013800F File Offset: 0x0013620F
		public ConcatLString(LocalizedString l, LocalizedString r)
		{
			this.left = l;
			this.right = r;
		}

		// Token: 0x17000D97 RID: 3479
		// (get) Token: 0x06002E80 RID: 11904 RVA: 0x00138025 File Offset: 0x00136225
		public override bool Loaded
		{
			get
			{
				return this.left.Loaded || this.right.Loaded;
			}
		}

		// Token: 0x06002E81 RID: 11905 RVA: 0x00138041 File Offset: 0x00136241
		public override void RetrieveValue()
		{
			this.cachedValue = (this.left.Value ?? string.Empty) + (this.right.Value ?? string.Empty);
			base.UpdateLanguage();
		}

		// Token: 0x0400175B RID: 5979
		private readonly LocalizedString left;

		// Token: 0x0400175C RID: 5980
		private readonly LocalizedString right;
	}
}
