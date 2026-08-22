using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000364 RID: 868
	[NullableContext(1)]
	[Nullable(0)]
	public class CapitalizeLString : LocalizedString
	{
		// Token: 0x06004311 RID: 17169 RVA: 0x002522A2 File Offset: 0x002504A2
		public CapitalizeLString(LocalizedString nStr)
		{
			this.nestedStr = nStr;
		}

		// Token: 0x17001198 RID: 4504
		// (get) Token: 0x06004312 RID: 17170 RVA: 0x002522B1 File Offset: 0x002504B1
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06004313 RID: 17171 RVA: 0x002522C0 File Offset: 0x002504C0
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

		// Token: 0x0400233A RID: 9018
		private readonly LocalizedString nestedStr;
	}
}
