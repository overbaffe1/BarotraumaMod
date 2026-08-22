using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200014B RID: 331
	[NullableContext(1)]
	[Nullable(0)]
	public class LimitLString : LocalizedString
	{
		// Token: 0x060029E9 RID: 10729 RVA: 0x001D0741 File Offset: 0x001CE941
		public LimitLString(LocalizedString text, GUIFont font, int maxWidth)
		{
			this.nestedStr = text;
			this.font = font;
			this.maxWidth = maxWidth;
		}

		// Token: 0x17000AA7 RID: 2727
		// (get) Token: 0x060029EA RID: 10730 RVA: 0x001D075E File Offset: 0x001CE95E
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x060029EB RID: 10731 RVA: 0x001D076C File Offset: 0x001CE96C
		protected override bool MustRetrieveValue()
		{
			if (!base.MustRetrieveValue() && this.cachedFont == this.font.Value)
			{
				ScalableFont scalableFont = this.cachedFont;
				uint? num = (scalableFont != null) ? new uint?(scalableFont.Size) : null;
				uint size = this.font.Size;
				return !(num.GetValueOrDefault() == size & num != null);
			}
			return true;
		}

		// Token: 0x060029EC RID: 10732 RVA: 0x001D07D8 File Offset: 0x001CE9D8
		public override void RetrieveValue()
		{
			this.cachedValue = ((this.font.Value != null) ? ToolBox.LimitString(this.nestedStr.Value, this.font.Value, this.maxWidth) : this.nestedStr.Value);
			this.cachedFont = this.font.Value;
			base.UpdateLanguage();
		}

		// Token: 0x040015E3 RID: 5603
		private readonly LocalizedString nestedStr;

		// Token: 0x040015E4 RID: 5604
		private readonly GUIFont font;

		// Token: 0x040015E5 RID: 5605
		private readonly int maxWidth;

		// Token: 0x040015E6 RID: 5606
		[Nullable(2)]
		private ScalableFont cachedFont;
	}
}
