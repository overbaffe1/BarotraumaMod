using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016F RID: 367
	internal class DecalPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000867 RID: 2151
		// (get) Token: 0x06001D83 RID: 7555 RVA: 0x000D2232 File Offset: 0x000D0432
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x000D2240 File Offset: 0x000D0440
		public override void Dispose()
		{
			foreach (Sprite spr in this.Sprites)
			{
				spr.Remove();
			}
			this.Sprites.Clear();
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x000D22A0 File Offset: 0x000D04A0
		public DecalPrefab(ContentXElement element, DecalsFile file) : base(file, new Identifier(element.Name.LocalName))
		{
			this.Sprites = new List<Sprite>();
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("sprite", StringComparison.OrdinalIgnoreCase))
				{
					this.Sprites.Add(new Sprite(subElement, "", "", false, 1f));
				}
			}
			string key = "color";
			Color white = Color.White;
			this.Color = element.GetAttributeColor(key, white);
			this.LifeTime = element.GetAttributeFloat("lifetime", 10f);
			this.FadeOutTime = Math.Min(this.LifeTime, element.GetAttributeFloat("fadeouttime", 1f));
			this.FadeInTime = Math.Min(this.LifeTime - this.FadeOutTime, element.GetAttributeFloat("fadeintime", 0f));
		}

		// Token: 0x04000D55 RID: 3413
		public readonly List<Sprite> Sprites;

		// Token: 0x04000D56 RID: 3414
		public readonly Color Color;

		// Token: 0x04000D57 RID: 3415
		public readonly float LifeTime;

		// Token: 0x04000D58 RID: 3416
		public readonly float FadeOutTime;

		// Token: 0x04000D59 RID: 3417
		public readonly float FadeInTime;
	}
}
