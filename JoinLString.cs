using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000369 RID: 873
	[NullableContext(1)]
	[Nullable(0)]
	public class JoinLString : LocalizedString
	{
		// Token: 0x06004325 RID: 17189 RVA: 0x00252740 File Offset: 0x00250940
		public JoinLString(string separator, IEnumerable<LocalizedString> subStrs)
		{
			this.separator = separator;
			this.subStrs = subStrs;
		}

		// Token: 0x1700119E RID: 4510
		// (get) Token: 0x06004326 RID: 17190 RVA: 0x00252756 File Offset: 0x00250956
		public override bool Loaded
		{
			get
			{
				return this.subStrs.All((LocalizedString s) => s.Loaded);
			}
		}

		// Token: 0x06004327 RID: 17191 RVA: 0x00252782 File Offset: 0x00250982
		public override void RetrieveValue()
		{
			this.cachedValue = string.Join<LocalizedString>(this.separator, this.subStrs);
			base.UpdateLanguage();
		}

		// Token: 0x04002345 RID: 9029
		private readonly IEnumerable<LocalizedString> subStrs;

		// Token: 0x04002346 RID: 9030
		private readonly string separator;
	}
}
