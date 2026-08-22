using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000263 RID: 611
	internal class DecalPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000EBB RID: 3771
		// (get) Token: 0x0600384F RID: 14415 RVA: 0x0021786A File Offset: 0x00215A6A
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x06003850 RID: 14416 RVA: 0x00217878 File Offset: 0x00215A78
		public override void Dispose()
		{
			foreach (Sprite spr in this.Sprites)
			{
				spr.Remove();
			}
			this.Sprites.Clear();
		}

		// Token: 0x06003851 RID: 14417 RVA: 0x002178D8 File Offset: 0x00215AD8
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

		// Token: 0x04001C4C RID: 7244
		public readonly List<Sprite> Sprites;

		// Token: 0x04001C4D RID: 7245
		public readonly Color Color;

		// Token: 0x04001C4E RID: 7246
		public readonly float LifeTime;

		// Token: 0x04001C4F RID: 7247
		public readonly float FadeOutTime;

		// Token: 0x04001C50 RID: 7248
		public readonly float FadeInTime;
	}
}
