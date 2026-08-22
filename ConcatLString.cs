using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000365 RID: 869
	[NullableContext(1)]
	[Nullable(0)]
	public class ConcatLString : LocalizedString
	{
		// Token: 0x06004314 RID: 17172 RVA: 0x0025231F File Offset: 0x0025051F
		public ConcatLString(LocalizedString l, LocalizedString r)
		{
			this.left = l;
			this.right = r;
		}

		// Token: 0x17001199 RID: 4505
		// (get) Token: 0x06004315 RID: 17173 RVA: 0x00252335 File Offset: 0x00250535
		public override bool Loaded
		{
			get
			{
				return this.left.Loaded || this.right.Loaded;
			}
		}

		// Token: 0x06004316 RID: 17174 RVA: 0x00252351 File Offset: 0x00250551
		public override void RetrieveValue()
		{
			this.cachedValue = (this.left.Value ?? string.Empty) + (this.right.Value ?? string.Empty);
			base.UpdateLanguage();
		}

		// Token: 0x0400233B RID: 9019
		private readonly LocalizedString left;

		// Token: 0x0400233C RID: 9020
		private readonly LocalizedString right;
	}
}
