using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029F RID: 671
	[NullableContext(1)]
	[Nullable(0)]
	public class LowerLString : LocalizedString
	{
		// Token: 0x06002EBA RID: 11962 RVA: 0x0013867C File Offset: 0x0013687C
		public LowerLString(LocalizedString nestedStr)
		{
			this.nestedStr = nestedStr;
		}

		// Token: 0x17000DA1 RID: 3489
		// (get) Token: 0x06002EBB RID: 11963 RVA: 0x0013868B File Offset: 0x0013688B
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002EBC RID: 11964 RVA: 0x00138698 File Offset: 0x00136898
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value.ToLowerInvariant();
			base.UpdateLanguage();
		}

		// Token: 0x0400176B RID: 5995
		private readonly LocalizedString nestedStr;
	}
}
