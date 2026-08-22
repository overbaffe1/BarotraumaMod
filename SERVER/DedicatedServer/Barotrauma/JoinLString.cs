using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029D RID: 669
	[NullableContext(1)]
	[Nullable(0)]
	public class JoinLString : LocalizedString
	{
		// Token: 0x06002E90 RID: 11920 RVA: 0x00138300 File Offset: 0x00136500
		public JoinLString(string separator, IEnumerable<LocalizedString> subStrs)
		{
			this.separator = separator;
			this.subStrs = subStrs;
		}

		// Token: 0x17000D9C RID: 3484
		// (get) Token: 0x06002E91 RID: 11921 RVA: 0x00138316 File Offset: 0x00136516
		public override bool Loaded
		{
			get
			{
				return this.subStrs.All((LocalizedString s) => s.Loaded);
			}
		}

		// Token: 0x06002E92 RID: 11922 RVA: 0x00138342 File Offset: 0x00136542
		public override void RetrieveValue()
		{
			this.cachedValue = string.Join<LocalizedString>(this.separator, this.subStrs);
			base.UpdateLanguage();
		}

		// Token: 0x04001765 RID: 5989
		private readonly IEnumerable<LocalizedString> subStrs;

		// Token: 0x04001766 RID: 5990
		private readonly string separator;
	}
}
