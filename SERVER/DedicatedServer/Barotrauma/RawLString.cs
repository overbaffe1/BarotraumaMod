using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A0 RID: 672
	public class RawLString : LocalizedString
	{
		// Token: 0x06002EBD RID: 11965 RVA: 0x001386B6 File Offset: 0x001368B6
		[NullableContext(1)]
		public RawLString(string value)
		{
			this.cachedValue = value;
		}

		// Token: 0x06002EBE RID: 11966 RVA: 0x001386C5 File Offset: 0x001368C5
		protected override bool MustRetrieveValue()
		{
			return false;
		}

		// Token: 0x17000DA2 RID: 3490
		// (get) Token: 0x06002EBF RID: 11967 RVA: 0x001386C8 File Offset: 0x001368C8
		public override bool Loaded
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002EC0 RID: 11968 RVA: 0x001386CB File Offset: 0x001368CB
		public override void RetrieveValue()
		{
		}
	}
}
