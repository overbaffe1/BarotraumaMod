using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200014C RID: 332
	[NullableContext(1)]
	[Nullable(0)]
	public class WrappedLString : LocalizedString
	{
		// Token: 0x060029ED RID: 10733 RVA: 0x001D083D File Offset: 0x001CEA3D
		public WrappedLString(LocalizedString text, float lineLength, GUIFont font, float textScale = 1f)
		{
			this.nestedStr = text;
			this.lineLength = lineLength;
			this.font = font;
			this.textScale = textScale;
		}

		// Token: 0x17000AA8 RID: 2728
		// (get) Token: 0x060029EE RID: 10734 RVA: 0x001D0862 File Offset: 0x001CEA62
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x060029EF RID: 10735 RVA: 0x001D0870 File Offset: 0x001CEA70
		public override void RetrieveValue()
		{
			ScalableFont scalableFont = this.font.GetFontForStr(this.nestedStr.Value);
			this.cachedValue = ((scalableFont != null) ? ToolBox.WrapText(this.nestedStr.Value, this.lineLength, scalableFont, this.textScale) : this.nestedStr.Value);
			base.UpdateLanguage();
		}

		// Token: 0x040015E7 RID: 5607
		private readonly LocalizedString nestedStr;

		// Token: 0x040015E8 RID: 5608
		private readonly float lineLength;

		// Token: 0x040015E9 RID: 5609
		private readonly GUIFont font;

		// Token: 0x040015EA RID: 5610
		private readonly float textScale;
	}
}
