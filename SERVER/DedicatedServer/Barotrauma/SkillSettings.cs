using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020000F4 RID: 244
	internal class SkillSettings : Prefab, ISerializableEntity
	{
		// Token: 0x170007A2 RID: 1954
		// (get) Token: 0x06001984 RID: 6532 RVA: 0x000C6FFB File Offset: 0x000C51FB
		public static SkillSettings Current
		{
			get
			{
				return SkillSettings.Prefabs.ActivePrefab;
			}
		}

		// Token: 0x170007A3 RID: 1955
		// (get) Token: 0x06001985 RID: 6533 RVA: 0x000C7007 File Offset: 0x000C5207
		// (set) Token: 0x06001986 RID: 6534 RVA: 0x000C700F File Offset: 0x000C520F
		[Serialize(4f, IsPropertySaveable.Yes, "", "", false)]
		public float SingleRoundSkillGainMultiplier { get; set; }

		// Token: 0x170007A4 RID: 1956
		// (get) Token: 0x06001987 RID: 6535 RVA: 0x000C7018 File Offset: 0x000C5218
		// (set) Token: 0x06001988 RID: 6536 RVA: 0x000C7027 File Offset: 0x000C5227
		[Serialize(5f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerRepair
		{
			get
			{
				return this.skillIncreasePerRepair * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerRepair = value;
			}
		}

		// Token: 0x170007A5 RID: 1957
		// (get) Token: 0x06001989 RID: 6537 RVA: 0x000C7030 File Offset: 0x000C5230
		// (set) Token: 0x0600198A RID: 6538 RVA: 0x000C703F File Offset: 0x000C523F
		[Serialize(3f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerSabotage
		{
			get
			{
				return this.skillIncreasePerSabotage * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerSabotage = value;
			}
		}

		// Token: 0x170007A6 RID: 1958
		// (get) Token: 0x0600198B RID: 6539 RVA: 0x000C7048 File Offset: 0x000C5248
		// (set) Token: 0x0600198C RID: 6540 RVA: 0x000C7057 File Offset: 0x000C5257
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerCprRevive
		{
			get
			{
				return this.skillIncreasePerCprRevive * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerCprRevive = value;
			}
		}

		// Token: 0x170007A7 RID: 1959
		// (get) Token: 0x0600198D RID: 6541 RVA: 0x000C7060 File Offset: 0x000C5260
		// (set) Token: 0x0600198E RID: 6542 RVA: 0x000C706F File Offset: 0x000C526F
		[Serialize(0.0025f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerRepairedStructureDamage
		{
			get
			{
				return this.skillIncreasePerRepairedStructureDamage * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerRepairedStructureDamage = value;
			}
		}

		// Token: 0x170007A8 RID: 1960
		// (get) Token: 0x0600198F RID: 6543 RVA: 0x000C7078 File Offset: 0x000C5278
		// (set) Token: 0x06001990 RID: 6544 RVA: 0x000C7087 File Offset: 0x000C5287
		[Serialize(0.005f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerSecondWhenSteering
		{
			get
			{
				return this.skillIncreasePerSecondWhenSteering * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerSecondWhenSteering = value;
			}
		}

		// Token: 0x170007A9 RID: 1961
		// (get) Token: 0x06001991 RID: 6545 RVA: 0x000C7090 File Offset: 0x000C5290
		// (set) Token: 0x06001992 RID: 6546 RVA: 0x000C709F File Offset: 0x000C529F
		[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerFabricatorRequiredSkill
		{
			get
			{
				return this.skillIncreasePerFabricatorRequiredSkill * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerFabricatorRequiredSkill = value;
			}
		}

		// Token: 0x170007AA RID: 1962
		// (get) Token: 0x06001993 RID: 6547 RVA: 0x000C70A8 File Offset: 0x000C52A8
		// (set) Token: 0x06001994 RID: 6548 RVA: 0x000C70B7 File Offset: 0x000C52B7
		[Serialize(0.01f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerHostileDamage
		{
			get
			{
				return this.skillIncreasePerHostileDamage * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerHostileDamage = value;
			}
		}

		// Token: 0x170007AB RID: 1963
		// (get) Token: 0x06001995 RID: 6549 RVA: 0x000C70C0 File Offset: 0x000C52C0
		// (set) Token: 0x06001996 RID: 6550 RVA: 0x000C70CF File Offset: 0x000C52CF
		[Serialize(0.001f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerSecondWhenOperatingTurret
		{
			get
			{
				return this.skillIncreasePerSecondWhenOperatingTurret * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerSecondWhenOperatingTurret = value;
			}
		}

		// Token: 0x170007AC RID: 1964
		// (get) Token: 0x06001997 RID: 6551 RVA: 0x000C70D8 File Offset: 0x000C52D8
		// (set) Token: 0x06001998 RID: 6552 RVA: 0x000C70E7 File Offset: 0x000C52E7
		[Serialize(0.001f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreasePerFriendlyHealed
		{
			get
			{
				return this.skillIncreasePerFriendlyHealed * this.GetCurrentSkillGainMultiplier();
			}
			set
			{
				this.skillIncreasePerFriendlyHealed = value;
			}
		}

		// Token: 0x170007AD RID: 1965
		// (get) Token: 0x06001999 RID: 6553 RVA: 0x000C70F0 File Offset: 0x000C52F0
		// (set) Token: 0x0600199A RID: 6554 RVA: 0x000C70F8 File Offset: 0x000C52F8
		[Serialize(1.1f, IsPropertySaveable.Yes, "", "", false)]
		public float AssistantSkillIncreaseMultiplier { get; set; }

		// Token: 0x170007AE RID: 1966
		// (get) Token: 0x0600199B RID: 6555 RVA: 0x000C7101 File Offset: 0x000C5301
		// (set) Token: 0x0600199C RID: 6556 RVA: 0x000C7109 File Offset: 0x000C5309
		[Serialize(200f, IsPropertySaveable.Yes, "The \"absolute\" maximum skill level with talents that increase the default maximum.", "", false)]
		public float MaximumSkillWithTalents { get; set; }

		// Token: 0x170007AF RID: 1967
		// (get) Token: 0x0600199D RID: 6557 RVA: 0x000C7112 File Offset: 0x000C5312
		// (set) Token: 0x0600199E RID: 6558 RVA: 0x000C711A File Offset: 0x000C531A
		[Serialize(1.5f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreaseExponent { get; set; }

		// Token: 0x0600199F RID: 6559 RVA: 0x000C7123 File Offset: 0x000C5323
		public SkillSettings(XElement element, SkillSettingsFile file) : base(file, "SkillSettings".ToIdentifier())
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x170007B0 RID: 1968
		// (get) Token: 0x060019A0 RID: 6560 RVA: 0x000C7143 File Offset: 0x000C5343
		public string Name
		{
			get
			{
				return "SkillSettings";
			}
		}

		// Token: 0x170007B1 RID: 1969
		// (get) Token: 0x060019A1 RID: 6561 RVA: 0x000C714A File Offset: 0x000C534A
		// (set) Token: 0x060019A2 RID: 6562 RVA: 0x000C7152 File Offset: 0x000C5352
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x060019A3 RID: 6563 RVA: 0x000C715B File Offset: 0x000C535B
		private float GetCurrentSkillGainMultiplier()
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode)
			{
				return 1f;
			}
			return this.SingleRoundSkillGainMultiplier;
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x000C7181 File Offset: 0x000C5381
		public override void Dispose()
		{
		}

		// Token: 0x04000C31 RID: 3121
		public static readonly PrefabSelector<SkillSettings> Prefabs = new PrefabSelector<SkillSettings>();

		// Token: 0x04000C33 RID: 3123
		private float skillIncreasePerRepair;

		// Token: 0x04000C34 RID: 3124
		private float skillIncreasePerSabotage;

		// Token: 0x04000C35 RID: 3125
		private float skillIncreasePerCprRevive;

		// Token: 0x04000C36 RID: 3126
		private float skillIncreasePerRepairedStructureDamage;

		// Token: 0x04000C37 RID: 3127
		private float skillIncreasePerSecondWhenSteering;

		// Token: 0x04000C38 RID: 3128
		private float skillIncreasePerFabricatorRequiredSkill;

		// Token: 0x04000C39 RID: 3129
		private float skillIncreasePerHostileDamage;

		// Token: 0x04000C3A RID: 3130
		private float skillIncreasePerSecondWhenOperatingTurret;

		// Token: 0x04000C3B RID: 3131
		private float skillIncreasePerFriendlyHealed;
	}
}
