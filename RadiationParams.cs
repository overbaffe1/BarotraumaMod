using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000324 RID: 804
	internal class RadiationParams : ISerializableEntity
	{
		// Token: 0x1700110F RID: 4367
		// (get) Token: 0x06004063 RID: 16483 RVA: 0x0023D98E File Offset: 0x0023BB8E
		public string Name
		{
			get
			{
				return "RadiationParams";
			}
		}

		// Token: 0x17001110 RID: 4368
		// (get) Token: 0x06004064 RID: 16484 RVA: 0x0023D995 File Offset: 0x0023BB95
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x17001111 RID: 4369
		// (get) Token: 0x06004065 RID: 16485 RVA: 0x0023D99D File Offset: 0x0023BB9D
		// (set) Token: 0x06004066 RID: 16486 RVA: 0x0023D9A5 File Offset: 0x0023BBA5
		[Serialize(-100f, IsPropertySaveable.No, "How much Jovian radiation the world starts with.", "", false)]
		public float StartingRadiation { get; set; }

		// Token: 0x17001112 RID: 4370
		// (get) Token: 0x06004067 RID: 16487 RVA: 0x0023D9AE File Offset: 0x0023BBAE
		// (set) Token: 0x06004068 RID: 16488 RVA: 0x0023D9B6 File Offset: 0x0023BBB6
		[Serialize(100f, IsPropertySaveable.No, "How much Jovian radiation is added on each step.", "", false)]
		public float RadiationStep { get; set; }

		// Token: 0x17001113 RID: 4371
		// (get) Token: 0x06004069 RID: 16489 RVA: 0x0023D9BF File Offset: 0x0023BBBF
		// (set) Token: 0x0600406A RID: 16490 RVA: 0x0023D9C7 File Offset: 0x0023BBC7
		[Serialize(250f, IsPropertySaveable.No, "The interval at which Jovian radiation's effect multiplies in intensity, measured in map pixels. For example if this is 200, then 400 map pixels into the radiation its effect will be doubled.", "", false)]
		public float RadiationEffectMultipliedPerPixelDistance { get; set; }

		// Token: 0x17001114 RID: 4372
		// (get) Token: 0x0600406B RID: 16491 RVA: 0x0023D9D0 File Offset: 0x0023BBD0
		// (set) Token: 0x0600406C RID: 16492 RVA: 0x0023D9D8 File Offset: 0x0023BBD8
		[Serialize(10, IsPropertySaveable.No, "How many turns in Jovian radiation does it take for an outpost to be removed from the map.", "", false)]
		public int CriticalRadiationThreshold { get; set; }

		// Token: 0x17001115 RID: 4373
		// (get) Token: 0x0600406D RID: 16493 RVA: 0x0023D9E1 File Offset: 0x0023BBE1
		// (set) Token: 0x0600406E RID: 16494 RVA: 0x0023D9E9 File Offset: 0x0023BBE9
		[Serialize(3, IsPropertySaveable.No, "Minimum amount of outposts in the level that cannot be removed due to Jovian radiation.", "", false)]
		public int MinimumOutpostAmount { get; set; }

		// Token: 0x17001116 RID: 4374
		// (get) Token: 0x0600406F RID: 16495 RVA: 0x0023D9F2 File Offset: 0x0023BBF2
		// (set) Token: 0x06004070 RID: 16496 RVA: 0x0023D9FA File Offset: 0x0023BBFA
		[Serialize(10f, IsPropertySaveable.No, "How long it takes to apply more of the Jovian radiation's effect while in the radiated zone.", "", false)]
		public float RadiationDamageDelay { get; set; }

		// Token: 0x17001117 RID: 4375
		// (get) Token: 0x06004071 RID: 16497 RVA: 0x0023DA03 File Offset: 0x0023BC03
		// (set) Token: 0x06004072 RID: 16498 RVA: 0x0023DA0B File Offset: 0x0023BC0B
		[Serialize(1f, IsPropertySaveable.No, "How much is the Jovian radiation affliction increased by while in a radiated zone.", "", false)]
		public float RadiationDamageAmount { get; set; }

		// Token: 0x17001118 RID: 4376
		// (get) Token: 0x06004073 RID: 16499 RVA: 0x0023DA14 File Offset: 0x0023BC14
		// (set) Token: 0x06004074 RID: 16500 RVA: 0x0023DA1C File Offset: 0x0023BC1C
		[Serialize(-1f, IsPropertySaveable.No, "Maximum amount of Jovian radiation.", "", false)]
		public float MaxRadiation { get; set; }

		// Token: 0x17001119 RID: 4377
		// (get) Token: 0x06004075 RID: 16501 RVA: 0x0023DA25 File Offset: 0x0023BC25
		// (set) Token: 0x06004076 RID: 16502 RVA: 0x0023DA2D File Offset: 0x0023BC2D
		[Serialize(3f, IsPropertySaveable.No, "How fast the Jovian radiation increase animation goes in the map view.", "", false)]
		public float AnimationSpeed { get; set; }

		// Token: 0x1700111A RID: 4378
		// (get) Token: 0x06004077 RID: 16503 RVA: 0x0023DA36 File Offset: 0x0023BC36
		// (set) Token: 0x06004078 RID: 16504 RVA: 0x0023DA3E File Offset: 0x0023BC3E
		[Serialize("139,0,0,85", IsPropertySaveable.No, "The color of the radiated area in the map view.", "", false)]
		public Color RadiationAreaColor { get; set; }

		// Token: 0x1700111B RID: 4379
		// (get) Token: 0x06004079 RID: 16505 RVA: 0x0023DA47 File Offset: 0x0023BC47
		// (set) Token: 0x0600407A RID: 16506 RVA: 0x0023DA4F File Offset: 0x0023BC4F
		[Serialize("255,0,0,255", IsPropertySaveable.No, "The tint of the Jovian radiation border sprites in the map view.", "", false)]
		public Color RadiationBorderTint { get; set; }

		// Token: 0x1700111C RID: 4380
		// (get) Token: 0x0600407B RID: 16507 RVA: 0x0023DA58 File Offset: 0x0023BC58
		// (set) Token: 0x0600407C RID: 16508 RVA: 0x0023DA60 File Offset: 0x0023BC60
		[Serialize(16.66f, IsPropertySaveable.No, "Speed of the border spritesheet animation in the map view.", "", false)]
		public float BorderAnimationSpeed { get; set; }

		// Token: 0x0600407D RID: 16509 RVA: 0x0023DA69 File Offset: 0x0023BC69
		public RadiationParams(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}
	}
}
