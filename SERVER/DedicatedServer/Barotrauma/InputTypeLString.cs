using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029C RID: 668
	[NullableContext(1)]
	[Nullable(0)]
	public class InputTypeLString : LocalizedString
	{
		// Token: 0x06002E8C RID: 11916 RVA: 0x001382BC File Offset: 0x001364BC
		public InputTypeLString(LocalizedString nStr, bool useColorHighlight = false)
		{
			this.nestedStr = nStr;
			this.useColorHighlight = useColorHighlight;
		}

		// Token: 0x06002E8D RID: 11917 RVA: 0x001382D2 File Offset: 0x001364D2
		protected override bool MustRetrieveValue()
		{
			return base.MustRetrieveValue();
		}

		// Token: 0x17000D9B RID: 3483
		// (get) Token: 0x06002E8E RID: 11918 RVA: 0x001382DA File Offset: 0x001364DA
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002E8F RID: 11919 RVA: 0x001382E7 File Offset: 0x001364E7
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value;
			base.UpdateLanguage();
		}

		// Token: 0x04001763 RID: 5987
		private readonly LocalizedString nestedStr;

		// Token: 0x04001764 RID: 5988
		private bool useColorHighlight;
	}
}
