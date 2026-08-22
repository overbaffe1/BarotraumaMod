using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A7 RID: 679
	[NullableContext(1)]
	[Nullable(0)]
	public class UpperLString : LocalizedString
	{
		// Token: 0x06002EE0 RID: 12000 RVA: 0x0013914A File Offset: 0x0013734A
		public UpperLString(LocalizedString nestedStr)
		{
			this.nestedStr = nestedStr;
		}

		// Token: 0x17000DAB RID: 3499
		// (get) Token: 0x06002EE1 RID: 12001 RVA: 0x00139159 File Offset: 0x00137359
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002EE2 RID: 12002 RVA: 0x00139166 File Offset: 0x00137366
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value.ToUpperInvariant();
			base.UpdateLanguage();
		}

		// Token: 0x06002EE3 RID: 12003 RVA: 0x00139184 File Offset: 0x00137384
		public override LocalizedString ToUpper()
		{
			return this;
		}

		// Token: 0x04001784 RID: 6020
		private readonly LocalizedString nestedStr;
	}
}
