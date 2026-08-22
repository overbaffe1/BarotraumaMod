using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200036C RID: 876
	public class RawLString : LocalizedString
	{
		// Token: 0x06004352 RID: 17234 RVA: 0x00252AF6 File Offset: 0x00250CF6
		[NullableContext(1)]
		public RawLString(string value)
		{
			this.cachedValue = value;
		}

		// Token: 0x06004353 RID: 17235 RVA: 0x00252B05 File Offset: 0x00250D05
		protected override bool MustRetrieveValue()
		{
			return false;
		}

		// Token: 0x170011A4 RID: 4516
		// (get) Token: 0x06004354 RID: 17236 RVA: 0x00252B08 File Offset: 0x00250D08
		public override bool Loaded
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004355 RID: 17237 RVA: 0x00252B0B File Offset: 0x00250D0B
		public override void RetrieveValue()
		{
		}
	}
}
