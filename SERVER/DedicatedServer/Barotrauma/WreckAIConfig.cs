using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020000A0 RID: 160
	internal class WreckAIConfig : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x0600132D RID: 4909 RVA: 0x000A788D File Offset: 0x000A5A8D
		public string Name
		{
			get
			{
				return "Wreck AI Config";
			}
		}

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x0600132E RID: 4910 RVA: 0x000A7894 File Offset: 0x000A5A94
		// (set) Token: 0x0600132F RID: 4911 RVA: 0x000A789C File Offset: 0x000A5A9C
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x06001330 RID: 4912 RVA: 0x000A78A5 File Offset: 0x000A5AA5
		public Identifier Entity
		{
			get
			{
				return this.Identifier;
			}
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x06001331 RID: 4913 RVA: 0x000A78AD File Offset: 0x000A5AAD
		// (set) Token: 0x06001332 RID: 4914 RVA: 0x000A78B5 File Offset: 0x000A5AB5
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier DefensiveAgent { get; private set; }

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x06001333 RID: 4915 RVA: 0x000A78BE File Offset: 0x000A5ABE
		// (set) Token: 0x06001334 RID: 4916 RVA: 0x000A78C6 File Offset: 0x000A5AC6
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string OffensiveAgent { get; private set; }

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x06001335 RID: 4917 RVA: 0x000A78CF File Offset: 0x000A5ACF
		// (set) Token: 0x06001336 RID: 4918 RVA: 0x000A78D7 File Offset: 0x000A5AD7
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Brain { get; private set; }

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x06001337 RID: 4919 RVA: 0x000A78E0 File Offset: 0x000A5AE0
		// (set) Token: 0x06001338 RID: 4920 RVA: 0x000A78E8 File Offset: 0x000A5AE8
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Spawner { get; private set; }

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x06001339 RID: 4921 RVA: 0x000A78F1 File Offset: 0x000A5AF1
		// (set) Token: 0x0600133A RID: 4922 RVA: 0x000A78F9 File Offset: 0x000A5AF9
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomBackground { get; private set; }

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x0600133B RID: 4923 RVA: 0x000A7902 File Offset: 0x000A5B02
		// (set) Token: 0x0600133C RID: 4924 RVA: 0x000A790A File Offset: 0x000A5B0A
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomVerticalWall { get; private set; }

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x0600133D RID: 4925 RVA: 0x000A7913 File Offset: 0x000A5B13
		// (set) Token: 0x0600133E RID: 4926 RVA: 0x000A791B File Offset: 0x000A5B1B
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string BrainRoomHorizontalWall { get; private set; }

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x0600133F RID: 4927 RVA: 0x000A7924 File Offset: 0x000A5B24
		// (set) Token: 0x06001340 RID: 4928 RVA: 0x000A792C File Offset: 0x000A5B2C
		[Serialize(60f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelay { get; private set; }

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x06001341 RID: 4929 RVA: 0x000A7935 File Offset: 0x000A5B35
		// (set) Token: 0x06001342 RID: 4930 RVA: 0x000A793D File Offset: 0x000A5B3D
		[Serialize(0.5f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelayRandomFactor { get; private set; }

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x06001343 RID: 4931 RVA: 0x000A7946 File Offset: 0x000A5B46
		// (set) Token: 0x06001344 RID: 4932 RVA: 0x000A794E File Offset: 0x000A5B4E
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnDelayDifficultyMultiplier { get; private set; }

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x06001345 RID: 4933 RVA: 0x000A7957 File Offset: 0x000A5B57
		// (set) Token: 0x06001346 RID: 4934 RVA: 0x000A795F File Offset: 0x000A5B5F
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AgentSpawnCountDifficultyMultiplier { get; private set; }

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x06001347 RID: 4935 RVA: 0x000A7968 File Offset: 0x000A5B68
		// (set) Token: 0x06001348 RID: 4936 RVA: 0x000A7970 File Offset: 0x000A5B70
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsPerBrainRoom { get; private set; }

		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06001349 RID: 4937 RVA: 0x000A7979 File Offset: 0x000A5B79
		// (set) Token: 0x0600134A RID: 4938 RVA: 0x000A7981 File Offset: 0x000A5B81
		[Serialize(3, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsPerRoom { get; private set; }

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x0600134B RID: 4939 RVA: 0x000A798A File Offset: 0x000A5B8A
		// (set) Token: 0x0600134C RID: 4940 RVA: 0x000A7992 File Offset: 0x000A5B92
		[Serialize(2, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsOutside { get; private set; }

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x0600134D RID: 4941 RVA: 0x000A799B File Offset: 0x000A5B9B
		// (set) Token: 0x0600134E RID: 4942 RVA: 0x000A79A3 File Offset: 0x000A5BA3
		[Serialize(5, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsOutside { get; private set; }

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x0600134F RID: 4943 RVA: 0x000A79AC File Offset: 0x000A5BAC
		// (set) Token: 0x06001350 RID: 4944 RVA: 0x000A79B4 File Offset: 0x000A5BB4
		[Serialize(3, IsPropertySaveable.No, "", "", false)]
		public int MinAgentsInside { get; private set; }

		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x06001351 RID: 4945 RVA: 0x000A79BD File Offset: 0x000A5BBD
		// (set) Token: 0x06001352 RID: 4946 RVA: 0x000A79C5 File Offset: 0x000A5BC5
		[Serialize(10, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentsInside { get; private set; }

		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x06001353 RID: 4947 RVA: 0x000A79CE File Offset: 0x000A5BCE
		// (set) Token: 0x06001354 RID: 4948 RVA: 0x000A79D6 File Offset: 0x000A5BD6
		[Serialize(15, IsPropertySaveable.No, "", "", false)]
		public int MaxAgentCount { get; private set; }

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06001355 RID: 4949 RVA: 0x000A79DF File Offset: 0x000A5BDF
		// (set) Token: 0x06001356 RID: 4950 RVA: 0x000A79E7 File Offset: 0x000A5BE7
		[Serialize(100f, IsPropertySaveable.No, "", "", false)]
		public float MinWaterLevel { get; private set; }

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06001357 RID: 4951 RVA: 0x000A79F0 File Offset: 0x000A5BF0
		// (set) Token: 0x06001358 RID: 4952 RVA: 0x000A79F8 File Offset: 0x000A5BF8
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool KillAgentsWhenEntityDies { get; private set; }

		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001359 RID: 4953 RVA: 0x000A7A01 File Offset: 0x000A5C01
		// (set) Token: 0x0600135A RID: 4954 RVA: 0x000A7A09 File Offset: 0x000A5C09
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float DeadEntityColorMultiplier { get; private set; }

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x0600135B RID: 4955 RVA: 0x000A7A12 File Offset: 0x000A5C12
		// (set) Token: 0x0600135C RID: 4956 RVA: 0x000A7A1A File Offset: 0x000A5C1A
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float DeadEntityColorFadeOutTime { get; private set; }

		// Token: 0x0600135D RID: 4957 RVA: 0x000A7A23 File Offset: 0x000A5C23
		public static WreckAIConfig GetRandom()
		{
			return (from p in WreckAIConfig.Prefabs
			orderby p.UintIdentifier
			select p).GetRandom(Rand.RandSync.ServerAndClient);
		}

		// Token: 0x0600135E RID: 4958 RVA: 0x000A7A54 File Offset: 0x000A5C54
		protected override Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("Entity", base.DetermineIdentifier(element));
		}

		// Token: 0x0600135F RID: 4959 RVA: 0x000A7A68 File Offset: 0x000A5C68
		public WreckAIConfig(ContentXElement element, WreckAIConfigFile file) : base(file, element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.ForbiddenAmmunition = element.GetAttributeIdentifierArray("ForbiddenAmmunition", Array.Empty<Identifier>(), true);
		}

		// Token: 0x06001360 RID: 4960 RVA: 0x000A7AA0 File Offset: 0x000A5CA0
		public override void Dispose()
		{
		}

		// Token: 0x0400091A RID: 2330
		public static readonly PrefabCollection<WreckAIConfig> Prefabs = new PrefabCollection<WreckAIConfig>();

		// Token: 0x04000932 RID: 2354
		public readonly Identifier[] ForbiddenAmmunition;
	}
}
