using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000097 RID: 151
	[NullableContext(1)]
	[Nullable(0)]
	public class GUISpriteSheetPrefab : GUIPrefab
	{
		// Token: 0x06001406 RID: 5126 RVA: 0x000BEF77 File Offset: 0x000BD177
		public GUISpriteSheetPrefab(ContentXElement element, UIStyleFile file) : base(element, file)
		{
			this.SpriteSheet = new SpriteSheet(element, "", "");
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x000BEF97 File Offset: 0x000BD197
		public override void Dispose()
		{
			this.SpriteSheet.Remove();
		}

		// Token: 0x040009E6 RID: 2534
		public readonly SpriteSheet SpriteSheet;
	}
}
