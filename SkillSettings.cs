using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001F0 RID: 496
	internal class SkillSettings : Prefab, ISerializableEntity
	{
		// Token: 0x17000E0A RID: 3594
		// (get) Token: 0x06003485 RID: 13445 RVA: 0x0020D9FF File Offset: 0x0020BBFF
		public static SkillSettings Current
		{
			get
			{
				return SkillSettings.Prefabs.ActivePrefab;
			}
		}

		// Token: 0x17000E0B RID: 3595
		// (get) Token: 0x06003486 RID: 13446 RVA: 0x0020DA0B File Offset: 0x0020BC0B
		// (set) Token: 0x06003487 RID: 13447 RVA: 0x0020DA13 File Offset: 0x0020BC13
		[Serialize(4f, IsPropertySaveable.Yes, "", "", false)]
		public float SingleRoundSkillGainMultiplier { get; set; }

		// Token: 0x17000E0C RID: 3596
		// (get) Token: 0x06003488 RID: 13448 RVA: 0x0020DA1C File Offset: 0x0020BC1C
		// (set) Token: 0x06003489 RID: 13449 RVA: 0x0020DA2B File Offset: 0x0020BC2B
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

		// Token: 0x17000E0D RID: 3597
		// (get) Token: 0x0600348A RID: 13450 RVA: 0x0020DA34 File Offset: 0x0020BC34
		// (set) Token: 0x0600348B RID: 13451 RVA: 0x0020DA43 File Offset: 0x0020BC43
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

		// Token: 0x17000E0E RID: 3598
		// (get) Token: 0x0600348C RID: 13452 RVA: 0x0020DA4C File Offset: 0x0020BC4C
		// (set) Token: 0x0600348D RID: 13453 RVA: 0x0020DA5B File Offset: 0x0020BC5B
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

		// Token: 0x17000E0F RID: 3599
		// (get) Token: 0x0600348E RID: 13454 RVA: 0x0020DA64 File Offset: 0x0020BC64
		// (set) Token: 0x0600348F RID: 13455 RVA: 0x0020DA73 File Offset: 0x0020BC73
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

		// Token: 0x17000E10 RID: 3600
		// (get) Token: 0x06003490 RID: 13456 RVA: 0x0020DA7C File Offset: 0x0020BC7C
		// (set) Token: 0x06003491 RID: 13457 RVA: 0x0020DA8B File Offset: 0x0020BC8B
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

		// Token: 0x17000E11 RID: 3601
		// (get) Token: 0x06003492 RID: 13458 RVA: 0x0020DA94 File Offset: 0x0020BC94
		// (set) Token: 0x06003493 RID: 13459 RVA: 0x0020DAA3 File Offset: 0x0020BCA3
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

		// Token: 0x17000E12 RID: 3602
		// (get) Token: 0x06003494 RID: 13460 RVA: 0x0020DAAC File Offset: 0x0020BCAC
		// (set) Token: 0x06003495 RID: 13461 RVA: 0x0020DABB File Offset: 0x0020BCBB
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

		// Token: 0x17000E13 RID: 3603
		// (get) Token: 0x06003496 RID: 13462 RVA: 0x0020DAC4 File Offset: 0x0020BCC4
		// (set) Token: 0x06003497 RID: 13463 RVA: 0x0020DAD3 File Offset: 0x0020BCD3
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

		// Token: 0x17000E14 RID: 3604
		// (get) Token: 0x06003498 RID: 13464 RVA: 0x0020DADC File Offset: 0x0020BCDC
		// (set) Token: 0x06003499 RID: 13465 RVA: 0x0020DAEB File Offset: 0x0020BCEB
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

		// Token: 0x17000E15 RID: 3605
		// (get) Token: 0x0600349A RID: 13466 RVA: 0x0020DAF4 File Offset: 0x0020BCF4
		// (set) Token: 0x0600349B RID: 13467 RVA: 0x0020DAFC File Offset: 0x0020BCFC
		[Serialize(1.1f, IsPropertySaveable.Yes, "", "", false)]
		public float AssistantSkillIncreaseMultiplier { get; set; }

		// Token: 0x17000E16 RID: 3606
		// (get) Token: 0x0600349C RID: 13468 RVA: 0x0020DB05 File Offset: 0x0020BD05
		// (set) Token: 0x0600349D RID: 13469 RVA: 0x0020DB0D File Offset: 0x0020BD0D
		[Serialize(200f, IsPropertySaveable.Yes, "The \"absolute\" maximum skill level with talents that increase the default maximum.", "", false)]
		public float MaximumSkillWithTalents { get; set; }

		// Token: 0x17000E17 RID: 3607
		// (get) Token: 0x0600349E RID: 13470 RVA: 0x0020DB16 File Offset: 0x0020BD16
		// (set) Token: 0x0600349F RID: 13471 RVA: 0x0020DB1E File Offset: 0x0020BD1E
		[Serialize(1.5f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillIncreaseExponent { get; set; }

		// Token: 0x060034A0 RID: 13472 RVA: 0x0020DB27 File Offset: 0x0020BD27
		public SkillSettings(XElement element, SkillSettingsFile file) : base(file, "SkillSettings".ToIdentifier())
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x17000E18 RID: 3608
		// (get) Token: 0x060034A1 RID: 13473 RVA: 0x0020DB47 File Offset: 0x0020BD47
		public string Name
		{
			get
			{
				return "SkillSettings";
			}
		}

		// Token: 0x17000E19 RID: 3609
		// (get) Token: 0x060034A2 RID: 13474 RVA: 0x0020DB4E File Offset: 0x0020BD4E
		// (set) Token: 0x060034A3 RID: 13475 RVA: 0x0020DB56 File Offset: 0x0020BD56
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x060034A4 RID: 13476 RVA: 0x0020DB5F File Offset: 0x0020BD5F
		private float GetCurrentSkillGainMultiplier()
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode)
			{
				return 1f;
			}
			return this.SingleRoundSkillGainMultiplier;
		}

		// Token: 0x060034A5 RID: 13477 RVA: 0x0020DB85 File Offset: 0x0020BD85
		public override void Dispose()
		{
		}

		// Token: 0x04001B57 RID: 6999
		public static readonly PrefabSelector<SkillSettings> Prefabs = new PrefabSelector<SkillSettings>();

		// Token: 0x04001B59 RID: 7001
		private float skillIncreasePerRepair;

		// Token: 0x04001B5A RID: 7002
		private float skillIncreasePerSabotage;

		// Token: 0x04001B5B RID: 7003
		private float skillIncreasePerCprRevive;

		// Token: 0x04001B5C RID: 7004
		private float skillIncreasePerRepairedStructureDamage;

		// Token: 0x04001B5D RID: 7005
		private float skillIncreasePerSecondWhenSteering;

		// Token: 0x04001B5E RID: 7006
		private float skillIncreasePerFabricatorRequiredSkill;

		// Token: 0x04001B5F RID: 7007
		private float skillIncreasePerHostileDamage;

		// Token: 0x04001B60 RID: 7008
		private float skillIncreasePerSecondWhenOperatingTurret;

		// Token: 0x04001B61 RID: 7009
		private float skillIncreasePerFriendlyHealed;
	}
}
