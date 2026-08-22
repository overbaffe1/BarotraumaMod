using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001A5 RID: 421
	internal class WreckAIConfig : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17000C69 RID: 3177
		// (get) Token: 0x0600300C RID: 12300 RVA: 0x001F93B0 File Offset: 0x001F75B0
		public string Name
		{
			get
			{
				return "Wreck AI Config";
			}
		}

		// Token: 0x17000C6A RID: 3178
		// (get) Token: 0x0600300D RID: 12301 RVA: 0x001F93B7 File Offset: 0x001F75B7
		// (set) Token: 0x0600300E RID: 12302 RVA: 0x001F93BF File Offset: 0x001F75BF
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000C6B RID: 3179
		// (get) Token: 0x0600300F RID: 12303 RVA: 0x001F93C8 File Offset: 0x001F75C8
		public Identifier Entity
		{
			get
			{
				return this.Identifier;
			}
		}

		// Token: 0x17000C6C RID: 3180
		// (get) Token: 0x06003010 RID: 12304 RVA: 0x001F93D0 File Offset: 0x001F75D0
		// (set) Token: 0x06003011 RID: 12305 RVA: 0x001F93D8 File Offset: 0x001F75D8
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier DefensiveAgent { get; private set; }

		// Token: 0x17000C6D RID: 3181
		// (get) Token: 0x06003012 RID: 12306 RVA: 0x001F93E1 File Offset: 0x001F75E1
		// (set) Token: 0x06003013 RID: 12307 RVA: 0x001F93E9 File Offset: 0x001F75E9
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string OffensiveAgent { get; private set; }

		// Token: 0x17000C6E RID: 3182
		// (get) Token: 0x06003014 RID: 12308 RVA: 0x001F93F2 File Offset: 0x001F75F2
		// (set) Token: 0x06003015 RID: 12309 RVA: 0x001F93FA File Offset: 0x001F75FA
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Brain { get; private set; }

		// Token: 0x17000C6F RID: 3183
		// (get) Token: 0x06003016 RID: 12310 RVA: 0x001F9403 File Offset: 0x001F7603
		// (set) Token: 0x06003017 RID: 12311 RVA: 0x001F940B File Offset: 0x001F760B
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Spawner { get; private set; }

		// Token: 0x17000C70 RID: 3184
		// (get) Token: 0x06003018 RID: 12312 RVA: 0x001F9414 File Offset: 0x001F7614
		// (set) Token: 0x06003019 RID: 12313 RVA: 0x001F941C File Offset: 0x001F761C
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomBackground { get; private set; }

		// Token: 0x17000C71 RID: 3185
		// (get) Token: 0x0600301A RID: 12314 RVA: 0x001F9425 File Offset: 0x001F7625
		// (set) Token: 0x0600301B RID: 12315 RVA: 0x001F942D File Offset: 0x001F762D
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomVerticalWall { get; private set; }

		// Token: 0x17000C72 RID: 3186
		// (get) Token: 0x0600301C RID: 12316 RVA: 0x001F9436 File Offset: 0x001F7636
		// (set) Token: 0x0600301D RID: 12317 RVA: 0x001F943E File Offset: 0x001F763E
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomHorizontalWall { get; private set; }

		// Token: 0x17000C73 RID: 3187
		// (get) Token: 0x0600301E RID: 12318 RVA: 0x001F9447 File Offset: 0x001F7647
		// (set) Token: 0x0600301F RID: 12319 RVA: 0x001F944F File Offset: 0x001F764F
		[Serialize(60f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelay { get; private set; }

		// Token: 0x17000C74 RID: 3188
		// (get) Token: 0x06003020 RID: 12320 RVA: 0x001F9458 File Offset: 0x001F7658
		// (set) Token: 0x06003021 RID: 12321 RVA: 0x001F9460 File Offset: 0x001F7660
		[Serialize(0.5f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelayRandomFactor { get; private set; }

		// Token: 0x17000C75 RID: 3189
		// (get) Token: 0x06003022 RID: 12322 RVA: 0x001F9469 File Offset: 0x001F7669
		// (set) Token: 0x06003023 RID: 12323 RVA: 0x001F9471 File Offset: 0x001F7671
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelayDifficultyMultiplier { get; private set; }

		// Token: 0x17000C76 RID: 3190
		// (get) Token: 0x06003024 RID: 12324 RVA: 0x001F947A File Offset: 0x001F767A
		// (set) Token: 0x06003025 RID: 12325 RVA: 0x001F9482 File Offset: 0x001F7682
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnCountDifficultyMultiplier { get; private set; }

		// Token: 0x17000C77 RID: 3191
		// (get) Token: 0x06003026 RID: 12326 RVA: 0x001F948B File Offset: 0x001F768B
		// (set) Token: 0x06003027 RID: 12327 RVA: 0x001F9493 File Offset: 0x001F7693
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsPerBrainRoom { get; private set; }

		// Token: 0x17000C78 RID: 3192
		// (get) Token: 0x06003028 RID: 12328 RVA: 0x001F949C File Offset: 0x001F769C
		// (set) Token: 0x06003029 RID: 12329 RVA: 0x001F94A4 File Offset: 0x001F76A4
		[Serialize(3, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsPerRoom { get; private set; }

		// Token: 0x17000C79 RID: 3193
		// (get) Token: 0x0600302A RID: 12330 RVA: 0x001F94AD File Offset: 0x001F76AD
		// (set) Token: 0x0600302B RID: 12331 RVA: 0x001F94B5 File Offset: 0x001F76B5
		[Serialize(2, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsOutside { get; private set; }

		// Token: 0x17000C7A RID: 3194
		// (get) Token: 0x0600302C RID: 12332 RVA: 0x001F94BE File Offset: 0x001F76BE
		// (set) Token: 0x0600302D RID: 12333 RVA: 0x001F94C6 File Offset: 0x001F76C6
		[Serialize(5, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsOutside { get; private set; }

		// Token: 0x17000C7B RID: 3195
		// (get) Token: 0x0600302E RID: 12334 RVA: 0x001F94CF File Offset: 0x001F76CF
		// (set) Token: 0x0600302F RID: 12335 RVA: 0x001F94D7 File Offset: 0x001F76D7
		[Serialize(3, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsInside { get; private set; }

		// Token: 0x17000C7C RID: 3196
		// (get) Token: 0x06003030 RID: 12336 RVA: 0x001F94E0 File Offset: 0x001F76E0
		// (set) Token: 0x06003031 RID: 12337 RVA: 0x001F94E8 File Offset: 0x001F76E8
		[Serialize(10, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsInside { get; private set; }

		// Token: 0x17000C7D RID: 3197
		// (get) Token: 0x06003032 RID: 12338 RVA: 0x001F94F1 File Offset: 0x001F76F1
		// (set) Token: 0x06003033 RID: 12339 RVA: 0x001F94F9 File Offset: 0x001F76F9
		[Serialize(15, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentCount { get; private set; }

		// Token: 0x17000C7E RID: 3198
		// (get) Token: 0x06003034 RID: 12340 RVA: 0x001F9502 File Offset: 0x001F7702
		// (set) Token: 0x06003035 RID: 12341 RVA: 0x001F950A File Offset: 0x001F770A
		[Serialize(100f, IsPropertySaveable.No, "", "", false)]
		public float MinWaterLevel { get; private set; }

		// Token: 0x17000C7F RID: 3199
		// (get) Token: 0x06003036 RID: 12342 RVA: 0x001F9513 File Offset: 0x001F7713
		// (set) Token: 0x06003037 RID: 12343 RVA: 0x001F951B File Offset: 0x001F771B
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool KillAgentsWhenEntityDies { get; private set; }

		// Token: 0x17000C80 RID: 3200
		// (get) Token: 0x06003038 RID: 12344 RVA: 0x001F9524 File Offset: 0x001F7724
		// (set) Token: 0x06003039 RID: 12345 RVA: 0x001F952C File Offset: 0x001F772C
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float DeadEntityColorMultiplier { get; private set; }

		// Token: 0x17000C81 RID: 3201
		// (get) Token: 0x0600303A RID: 12346 RVA: 0x001F9535 File Offset: 0x001F7735
		// (set) Token: 0x0600303B RID: 12347 RVA: 0x001F953D File Offset: 0x001F773D
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float DeadEntityColorFadeOutTime { get; private set; }

		// Token: 0x0600303C RID: 12348 RVA: 0x001F9546 File Offset: 0x001F7746
		public static WreckAIConfig GetRandom()
		{
			return (from p in WreckAIConfig.Prefabs
			orderby p.UintIdentifier
			select p).GetRandom(Rand.RandSync.ServerAndClient);
		}

		// Token: 0x0600303D RID: 12349 RVA: 0x001F9577 File Offset: 0x001F7777
		protected override Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("Entity", base.DetermineIdentifier(element));
		}

		// Token: 0x0600303E RID: 12350 RVA: 0x001F958B File Offset: 0x001F778B
		public WreckAIConfig(ContentXElement element, WreckAIConfigFile file) : base(file, element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ForbiddenAmmunition = element.GetAttributeIdentifierArray("ForbiddenAmmunition", Array.Empty<Identifier>(), true);
		}

		// Token: 0x0600303F RID: 12351 RVA: 0x001F95C3 File Offset: 0x001F77C3
		public override void Dispose()
		{
		}

		// Token: 0x04001906 RID: 6406
		public static readonly PrefabCollection<WreckAIConfig> Prefabs = new PrefabCollection<WreckAIConfig>();

		// Token: 0x0400191E RID: 6430
		public readonly Identifier[] ForbiddenAmmunition;
	}
}
