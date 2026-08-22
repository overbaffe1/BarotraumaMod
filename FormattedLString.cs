using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000367 RID: 871
	[NullableContext(1)]
	[Nullable(0)]
	public class FormattedLString : LocalizedString
	{
		// Token: 0x0600431E RID: 17182 RVA: 0x002524F1 File Offset: 0x002506F1
		public FormattedLString(LocalizedString str, params LocalizedString[] subStrs)
		{
			this.str = str;
			this.subStrs = subStrs.ToImmutableArray<LocalizedString>();
		}

		// Token: 0x1700119C RID: 4508
		// (get) Token: 0x0600431F RID: 17183 RVA: 0x00252511 File Offset: 0x00250711
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

		// Token: 0x06004320 RID: 17184 RVA: 0x0025254C File Offset: 0x0025074C
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

		// Token: 0x04002341 RID: 9025
		private readonly LocalizedString str;

		// Token: 0x04002342 RID: 9026
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<LocalizedString> subStrs;
	}
}
