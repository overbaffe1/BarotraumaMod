using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000375 RID: 885
	[NullableContext(1)]
	[Nullable(0)]
	internal class StripRichTagsLString : LocalizedString
	{
		// Token: 0x0600439B RID: 17307 RVA: 0x002539C4 File Offset: 0x00251BC4
		public StripRichTagsLString(RichString richStr)
		{
			this.RichStr = richStr;
		}

		// Token: 0x170011B4 RID: 4532
		// (get) Token: 0x0600439C RID: 17308 RVA: 0x002539D3 File Offset: 0x00251BD3
		public override bool Loaded
		{
			get
			{
				return this.RichStr.NestedStr.Loaded;
			}
		}

		// Token: 0x0600439D RID: 17309 RVA: 0x002539E5 File Offset: 0x00251BE5
		public override void RetrieveValue()
		{
			this.cachedValue = this.RichStr.SanitizedValue;
		}

		// Token: 0x04002372 RID: 9074
		public readonly RichString RichStr;
	}
}
