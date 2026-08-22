using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A9 RID: 681
	[NullableContext(1)]
	[Nullable(0)]
	internal class StripRichTagsLString : LocalizedString
	{
		// Token: 0x06002F03 RID: 12035 RVA: 0x00139480 File Offset: 0x00137680
		public StripRichTagsLString(RichString richStr)
		{
			this.RichStr = richStr;
		}

		// Token: 0x17000DB1 RID: 3505
		// (get) Token: 0x06002F04 RID: 12036 RVA: 0x0013948F File Offset: 0x0013768F
		public override bool Loaded
		{
			get
			{
				return this.RichStr.NestedStr.Loaded;
			}
		}

		// Token: 0x06002F05 RID: 12037 RVA: 0x001394A1 File Offset: 0x001376A1
		public override void RetrieveValue()
		{
			this.cachedValue = this.RichStr.SanitizedValue;
		}

		// Token: 0x0400178F RID: 6031
		public readonly RichString RichStr;
	}
}
