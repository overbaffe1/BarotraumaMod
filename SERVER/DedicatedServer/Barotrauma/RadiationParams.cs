using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200024B RID: 587
	internal class RadiationParams : ISerializableEntity
	{
		// Token: 0x17000C55 RID: 3157
		// (get) Token: 0x060029DA RID: 10714 RVA: 0x0011306F File Offset: 0x0011126F
		public string Name
		{
			get
			{
				return "RadiationParams";
			}
		}

		// Token: 0x17000C56 RID: 3158
		// (get) Token: 0x060029DB RID: 10715 RVA: 0x00113076 File Offset: 0x00111276
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x17000C57 RID: 3159
		// (get) Token: 0x060029DC RID: 10716 RVA: 0x0011307E File Offset: 0x0011127E
		// (set) Token: 0x060029DD RID: 10717 RVA: 0x00113086 File Offset: 0x00111286
		[Serialize(-100f, IsPropertySaveable.No, "How much Jovian radiation the world starts with.", "", false)]
		public float StartingRadiation { get; set; }

		// Token: 0x17000C58 RID: 3160
		// (get) Token: 0x060029DE RID: 10718 RVA: 0x0011308F File Offset: 0x0011128F
		// (set) Token: 0x060029DF RID: 10719 RVA: 0x00113097 File Offset: 0x00111297
		[Serialize(100f, IsPropertySaveable.No, "How much Jovian radiation is added on each step.", "", false)]
		public float RadiationStep { get; set; }

		// Token: 0x17000C59 RID: 3161
		// (get) Token: 0x060029E0 RID: 10720 RVA: 0x001130A0 File Offset: 0x001112A0
		// (set) Token: 0x060029E1 RID: 10721 RVA: 0x001130A8 File Offset: 0x001112A8
		[Serialize(250f, IsPropertySaveable.No, "The interval at which Jovian radiation's effect multiplies in intensity, measured in map pixels. For example if this is 200, then 400 map pixels into the radiation its effect will be doubled.", "", false)]
		public float RadiationEffectMultipliedPerPixelDistance { get; set; }

		// Token: 0x17000C5A RID: 3162
		// (get) Token: 0x060029E2 RID: 10722 RVA: 0x001130B1 File Offset: 0x001112B1
		// (set) Token: 0x060029E3 RID: 10723 RVA: 0x001130B9 File Offset: 0x001112B9
		[Serialize(10, IsPropertySaveable.No, "How many turns in Jovian radiation does it take for an outpost to be removed from the map.", "", false)]
		public int CriticalRadiationThreshold { get; set; }

		// Token: 0x17000C5B RID: 3163
		// (get) Token: 0x060029E4 RID: 10724 RVA: 0x001130C2 File Offset: 0x001112C2
		// (set) Token: 0x060029E5 RID: 10725 RVA: 0x001130CA File Offset: 0x001112CA
		[Serialize(3, IsPropertySaveable.No, "Minimum amount of outposts in the level that cannot be removed due to Jovian radiation.", "", false)]
		public int MinimumOutpostAmount { get; set; }

		// Token: 0x17000C5C RID: 3164
		// (get) Token: 0x060029E6 RID: 10726 RVA: 0x001130D3 File Offset: 0x001112D3
		// (set) Token: 0x060029E7 RID: 10727 RVA: 0x001130DB File Offset: 0x001112DB
		[Serialize(10f, IsPropertySaveable.No, "How long it takes to apply more of the Jovian radiation's effect while in the radiated zone.", "", false)]
		public float RadiationDamageDelay { get; set; }

		// Token: 0x17000C5D RID: 3165
		// (get) Token: 0x060029E8 RID: 10728 RVA: 0x001130E4 File Offset: 0x001112E4
		// (set) Token: 0x060029E9 RID: 10729 RVA: 0x001130EC File Offset: 0x001112EC
		[Serialize(1f, IsPropertySaveable.No, "How much is the Jovian radiation affliction increased by while in a radiated zone.", "", false)]
		public float RadiationDamageAmount { get; set; }

		// Token: 0x17000C5E RID: 3166
		// (get) Token: 0x060029EA RID: 10730 RVA: 0x001130F5 File Offset: 0x001112F5
		// (set) Token: 0x060029EB RID: 10731 RVA: 0x001130FD File Offset: 0x001112FD
		[Serialize(-1f, IsPropertySaveable.No, "Maximum amount of Jovian radiation.", "", false)]
		public float MaxRadiation { get; set; }

		// Token: 0x17000C5F RID: 3167
		// (get) Token: 0x060029EC RID: 10732 RVA: 0x00113106 File Offset: 0x00111306
		// (set) Token: 0x060029ED RID: 10733 RVA: 0x0011310E File Offset: 0x0011130E
		[Serialize(3f, IsPropertySaveable.No, "How fast the Jovian radiation increase animation goes in the map view.", "", false)]
		public float AnimationSpeed { get; set; }

		// Token: 0x17000C60 RID: 3168
		// (get) Token: 0x060029EE RID: 10734 RVA: 0x00113117 File Offset: 0x00111317
		// (set) Token: 0x060029EF RID: 10735 RVA: 0x0011311F File Offset: 0x0011131F
		[Serialize("139,0,0,85", IsPropertySaveable.No, "The color of the radiated area in the map view.", "", false)]
		public Color RadiationAreaColor { get; set; }

		// Token: 0x17000C61 RID: 3169
		// (get) Token: 0x060029F0 RID: 10736 RVA: 0x00113128 File Offset: 0x00111328
		// (set) Token: 0x060029F1 RID: 10737 RVA: 0x00113130 File Offset: 0x00111330
		[Serialize("255,0,0,255", IsPropertySaveable.No, "The tint of the Jovian radiation border sprites in the map view.", "", false)]
		public Color RadiationBorderTint { get; set; }

		// Token: 0x17000C62 RID: 3170
		// (get) Token: 0x060029F2 RID: 10738 RVA: 0x00113139 File Offset: 0x00111339
		// (set) Token: 0x060029F3 RID: 10739 RVA: 0x00113141 File Offset: 0x00111341
		[Serialize(16.66f, IsPropertySaveable.No, "Speed of the border spritesheet animation in the map view.", "", false)]
		public float BorderAnimationSpeed { get; set; }

		// Token: 0x060029F4 RID: 10740 RVA: 0x0011314A File Offset: 0x0011134A
		public RadiationParams(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}
	}
}
