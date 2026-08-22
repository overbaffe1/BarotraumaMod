using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000298 RID: 664
	[NullableContext(1)]
	[Nullable(0)]
	public class CapitalizeLString : LocalizedString
	{
		// Token: 0x06002E7C RID: 11900 RVA: 0x00137F92 File Offset: 0x00136192
		public CapitalizeLString(LocalizedString nStr)
		{
			this.nestedStr = nStr;
		}

		// Token: 0x17000D96 RID: 3478
		// (get) Token: 0x06002E7D RID: 11901 RVA: 0x00137FA1 File Offset: 0x001361A1
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002E7E RID: 11902 RVA: 0x00137FB0 File Offset: 0x001361B0
		public override void RetrieveValue()
		{
			string str = this.nestedStr.Value;
			if (!string.IsNullOrEmpty(str))
			{
				char c = char.ToUpper(str[0]);
				this.cachedValue = new ReadOnlySpan<char>(ref c) + str.Substring(1);
			}
			else
			{
				this.cachedValue = "";
			}
			base.UpdateLanguage();
		}

		// Token: 0x0400175A RID: 5978
		private readonly LocalizedString nestedStr;
	}
}
