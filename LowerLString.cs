using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200036B RID: 875
	[NullableContext(1)]
	[Nullable(0)]
	public class LowerLString : LocalizedString
	{
		// Token: 0x0600434F RID: 17231 RVA: 0x00252ABC File Offset: 0x00250CBC
		public LowerLString(LocalizedString nestedStr)
		{
			this.nestedStr = nestedStr;
		}

		// Token: 0x170011A3 RID: 4515
		// (get) Token: 0x06004350 RID: 17232 RVA: 0x00252ACB File Offset: 0x00250CCB
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06004351 RID: 17233 RVA: 0x00252AD8 File Offset: 0x00250CD8
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value.ToLowerInvariant();
			base.UpdateLanguage();
		}

		// Token: 0x0400234B RID: 9035
		private readonly LocalizedString nestedStr;
	}
}
