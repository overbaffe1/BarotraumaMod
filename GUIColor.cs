using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000094 RID: 148
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	public class GUIColor : GUISelector<GUIColorPrefab>
	{
		// Token: 0x060013FB RID: 5115 RVA: 0x000BEE50 File Offset: 0x000BD050
		public GUIColor(string identifier, Color fallbackColor) : base(identifier)
		{
			this.fallbackColor = fallbackColor;
		}

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x060013FC RID: 5116 RVA: 0x000BEE60 File Offset: 0x000BD060
		public Color Value
		{
			get
			{
				PrefabSelector<GUIColorPrefab> prefabs = this.Prefabs;
				Color? color;
				if (prefabs == null)
				{
					color = null;
				}
				else
				{
					GUIColorPrefab activePrefab = prefabs.ActivePrefab;
					color = ((activePrefab != null) ? new Color?(activePrefab.Color) : null);
				}
				Color? color2 = color;
				if (color2 == null)
				{
					return this.fallbackColor;
				}
				return color2.GetValueOrDefault();
			}
		}

		// Token: 0x060013FD RID: 5117 RVA: 0x000BEEB8 File Offset: 0x000BD0B8
		public static implicit operator Color(GUIColor reference)
		{
			return reference.Value;
		}

		// Token: 0x060013FE RID: 5118 RVA: 0x000BEEC0 File Offset: 0x000BD0C0
		public static Color operator *(GUIColor value, float scale)
		{
			return value.Value * scale;
		}

		// Token: 0x040009E4 RID: 2532
		private readonly Color fallbackColor;
	}
}
