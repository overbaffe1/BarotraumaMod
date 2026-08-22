using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029B RID: 667
	[NullableContext(1)]
	[Nullable(0)]
	public class FormattedLString : LocalizedString
	{
		// Token: 0x06002E89 RID: 11913 RVA: 0x001381E1 File Offset: 0x001363E1
		public FormattedLString(LocalizedString str, params LocalizedString[] subStrs)
		{
			this.str = str;
			this.subStrs = subStrs.ToImmutableArray<LocalizedString>();
		}

		// Token: 0x17000D9A RID: 3482
		// (get) Token: 0x06002E8A RID: 11914 RVA: 0x00138201 File Offset: 0x00136401
		public override bool Loaded
		{
			get
			{
				if (this.str.Loaded)
				{
					return this.subStrs.All((LocalizedString s) => s.Loaded);
				}
				return false;
			}
		}

		// Token: 0x06002E8B RID: 11915 RVA: 0x0013823C File Offset: 0x0013643C
		public override void RetrieveValue()
		{
			try
			{
				this.cachedValue = string.Format(this.str.Value, (from s in this.subStrs
				select s.Value).ToArray<object>());
			}
			catch (FormatException)
			{
				this.cachedValue = this.str.Value;
			}
			base.UpdateLanguage();
		}

		// Token: 0x04001761 RID: 5985
		private readonly LocalizedString str;

		// Token: 0x04001762 RID: 5986
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<LocalizedString> subStrs;
	}
}
