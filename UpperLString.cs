using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000373 RID: 883
	[NullableContext(1)]
	[Nullable(0)]
	public class UpperLString : LocalizedString
	{
		// Token: 0x06004375 RID: 17269 RVA: 0x0025358A File Offset: 0x0025178A
		public UpperLString(LocalizedString nestedStr)
		{
			this.nestedStr = nestedStr;
		}

		// Token: 0x170011AD RID: 4525
		// (get) Token: 0x06004376 RID: 17270 RVA: 0x00253599 File Offset: 0x00251799
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06004377 RID: 17271 RVA: 0x002535A6 File Offset: 0x002517A6
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value.ToUpperInvariant();
			base.UpdateLanguage();
		}

		// Token: 0x06004378 RID: 17272 RVA: 0x002535C4 File Offset: 0x002517C4
		public override LocalizedString ToUpper()
		{
			return this;
		}

		// Token: 0x04002364 RID: 9060
		private readonly LocalizedString nestedStr;
	}
}
