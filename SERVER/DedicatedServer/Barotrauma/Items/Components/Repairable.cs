using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A4 RID: 1188
	internal class Repairable : ItemComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004238 RID: 16952 RVA: 0x001A944C File Offset: 0x001A764C
		public override void OnMapLoaded()
		{
			this.item.CreateServerEvent<Repairable>(this);
		}

		// Token: 0x06004239 RID: 16953 RVA: 0x001A945C File Offset: 0x001A765C
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			if (c.Character == null)
			{
				return;
			}
			Repairable.FixActions requestedFixAction = (Repairable.FixActions)msg.ReadRangedInteger(0, 2);
			bool QTESuccess = msg.ReadBoolean();
			if (!this.item.CanClientAccess(c) || !this.HasRequiredItems(c.Character, false, null))
			{
				return;
			}
			if (requestedFixAction != Repairable.FixActions.None)
			{
				if (!c.Character.IsTraitor && requestedFixAction == Repairable.FixActions.Sabotage)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.Log("Non traitor \"" + c.Character.Name + "\" attempted to sabotage item.");
					}
					requestedFixAction = Repairable.FixActions.Repair;
				}
				if (this.CurrentFixer == null || (this.CurrentFixer == c.Character && requestedFixAction != this.currentFixerAction))
				{
					this.StartRepairing(c.Character, requestedFixAction);
					this.item.CreateServerEvent<Repairable>(this);
					return;
				}
			}
			else
			{
				this.RepairBoost(QTESuccess);
				this.item.CreateServerEvent<Repairable>(this);
			}
		}

		// Token: 0x0600423A RID: 16954 RVA: 0x001A9530 File Offset: 0x001A7730
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteSingle(this.deteriorationTimer);
			msg.WriteSingle(this.ForceDeteriorationTimer);
			msg.WriteSingle(this.tinkeringDuration);
			msg.WriteSingle(this.tinkeringStrength);
			msg.WriteBoolean(this.tinkeringPowersDevices);
			msg.WriteUInt16((this.CurrentFixer == null) ? 0 : this.CurrentFixer.ID);
			msg.WriteRangedInteger((int)this.currentFixerAction, 0, 2);
		}

		// Token: 0x170011A8 RID: 4520
		// (get) Token: 0x0600423B RID: 16955 RVA: 0x001A95A3 File Offset: 0x001A77A3
		// (set) Token: 0x0600423C RID: 16956 RVA: 0x001A95AB File Offset: 0x001A77AB
		public float ForceDeteriorationTimer { get; private set; }

		// Token: 0x170011A9 RID: 4521
		// (get) Token: 0x0600423D RID: 16957 RVA: 0x001A95B4 File Offset: 0x001A77B4
		// (set) Token: 0x0600423E RID: 16958 RVA: 0x001A95BC File Offset: 0x001A77BC
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the condition of the item deteriorates per second.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
		public float DeteriorationSpeed { get; set; }

		// Token: 0x170011AA RID: 4522
		// (get) Token: 0x0600423F RID: 16959 RVA: 0x001A95C5 File Offset: 0x001A77C5
		// (set) Token: 0x06004240 RID: 16960 RVA: 0x001A95CD File Offset: 0x001A77CD
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum initial delay before the item starts to deteriorate.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MinDeteriorationDelay { get; set; }

		// Token: 0x170011AB RID: 4523
		// (get) Token: 0x06004241 RID: 16961 RVA: 0x001A95D6 File Offset: 0x001A77D6
		// (set) Token: 0x06004242 RID: 16962 RVA: 0x001A95DE File Offset: 0x001A77DE
		[Serialize(0f, IsPropertySaveable.Yes, "Maximum initial delay before the item starts to deteriorate.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MaxDeteriorationDelay { get; set; }

		// Token: 0x170011AC RID: 4524
		// (get) Token: 0x06004243 RID: 16963 RVA: 0x001A95E7 File Offset: 0x001A77E7
		// (set) Token: 0x06004244 RID: 16964 RVA: 0x001A95EF File Offset: 0x001A77EF
		[Serialize(50f, IsPropertySaveable.Yes, "The item won't deteriorate spontaneously if the condition is below this value. For example, if set to 10, the condition will spontaneously drop to 10 and then stop dropping (unless the item is damaged further by external factors). Percentages of max condition.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinDeteriorationCondition { get; set; }

		// Token: 0x170011AD RID: 4525
		// (get) Token: 0x06004245 RID: 16965 RVA: 0x001A95F8 File Offset: 0x001A77F8
		// (set) Token: 0x06004246 RID: 16966 RVA: 0x001A9600 File Offset: 0x001A7800
		[Serialize(0f, IsPropertySaveable.Yes, "How low a traitor must get the item's condition for it to start breaking down.", "", false)]
		public float MinSabotageCondition { get; set; }

		// Token: 0x170011AE RID: 4526
		// (get) Token: 0x06004247 RID: 16967 RVA: 0x001A9609 File Offset: 0x001A7809
		// (set) Token: 0x06004248 RID: 16968 RVA: 0x001A9611 File Offset: 0x001A7811
		[Serialize(60f, IsPropertySaveable.Yes, "How long will the item spontaneously deteriorate after being sabotaged.", "", false)]
		public float SabotageDeteriorationDuration { get; set; }

		// Token: 0x170011AF RID: 4527
		// (get) Token: 0x06004249 RID: 16969 RVA: 0x001A961A File Offset: 0x001A781A
		// (set) Token: 0x0600424A RID: 16970 RVA: 0x001A9622 File Offset: 0x001A7822
		[Serialize(80f, IsPropertySaveable.Yes, "The condition of the item has to be below this for it to become repairable. Percentages of max condition.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float RepairThreshold { get; set; }

		// Token: 0x170011B0 RID: 4528
		// (get) Token: 0x0600424B RID: 16971 RVA: 0x001A962B File Offset: 0x001A782B
		// (set) Token: 0x0600424C RID: 16972 RVA: 0x001A9633 File Offset: 0x001A7833
		[Serialize(1f, IsPropertySaveable.Yes, "How much faster the device can deteriorate when under stress (e.g. when operating at full speed/power).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float MaxStressDeteriorationMultiplier { get; set; }

		// Token: 0x170011B1 RID: 4529
		// (get) Token: 0x0600424D RID: 16973 RVA: 0x001A963C File Offset: 0x001A783C
		// (set) Token: 0x0600424E RID: 16974 RVA: 0x001A9644 File Offset: 0x001A7844
		[Serialize(0.5f, IsPropertySaveable.Yes, "At what speed/power must the device be operating at to be considered \"under stress\".", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationThreshold { get; set; }

		// Token: 0x170011B2 RID: 4530
		// (get) Token: 0x0600424F RID: 16975 RVA: 0x001A964D File Offset: 0x001A784D
		// (set) Token: 0x06004250 RID: 16976 RVA: 0x001A9655 File Offset: 0x001A7855
		[Serialize(0.1f, IsPropertySaveable.Yes, "How fast the deterioration speed increases when under stress.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationIncreaseSpeed { get; set; }

		// Token: 0x170011B3 RID: 4531
		// (get) Token: 0x06004251 RID: 16977 RVA: 0x001A965E File Offset: 0x001A785E
		// (set) Token: 0x06004252 RID: 16978 RVA: 0x001A9666 File Offset: 0x001A7866
		[Serialize(0.1f, IsPropertySaveable.Yes, "How fast the deterioration speed decreases when not under stress.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 2)]
		public float StressDeteriorationDecreaseSpeed { get; set; }

		// Token: 0x170011B4 RID: 4532
		// (get) Token: 0x06004253 RID: 16979 RVA: 0x001A966F File Offset: 0x001A786F
		// (set) Token: 0x06004254 RID: 16980 RVA: 0x001A9677 File Offset: 0x001A7877
		[Serialize(100f, IsPropertySaveable.Yes, "The amount of time it takes to fix the item with insufficient skill levels.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FixDurationLowSkill { get; set; }

		// Token: 0x170011B5 RID: 4533
		// (get) Token: 0x06004255 RID: 16981 RVA: 0x001A9680 File Offset: 0x001A7880
		// (set) Token: 0x06004256 RID: 16982 RVA: 0x001A9688 File Offset: 0x001A7888
		[Serialize(10f, IsPropertySaveable.Yes, "The amount of time it takes to fix the item with sufficient skill levels.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FixDurationHighSkill { get; set; }

		// Token: 0x170011B6 RID: 4534
		// (get) Token: 0x06004257 RID: 16983 RVA: 0x001A9691 File Offset: 0x001A7891
		// (set) Token: 0x06004258 RID: 16984 RVA: 0x001A969C File Offset: 0x001A789C
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillRequirementMultiplier
		{
			get
			{
				return this.skillRequirementMultiplier;
			}
			set
			{
				float oldValue = this.skillRequirementMultiplier;
				this.skillRequirementMultiplier = value;
			}
		}

		// Token: 0x170011B7 RID: 4535
		// (get) Token: 0x06004259 RID: 16985 RVA: 0x001A96B7 File Offset: 0x001A78B7
		// (set) Token: 0x0600425A RID: 16986 RVA: 0x001A96C0 File Offset: 0x001A78C0
		public bool IsTinkering
		{
			get
			{
				return this.isTinkering;
			}
			private set
			{
				if (this.isTinkering == value)
				{
					return;
				}
				this.isTinkering = value;
				if (this.tinkeringPowersDevices)
				{
					foreach (Powered powered in this.item.GetComponents<Powered>())
					{
						if (!(powered is PowerContainer))
						{
							powered.PoweredByTinkering = this.isTinkering;
						}
					}
				}
			}
		}

		// Token: 0x170011B8 RID: 4536
		// (get) Token: 0x0600425B RID: 16987 RVA: 0x001A9738 File Offset: 0x001A7938
		// (set) Token: 0x0600425C RID: 16988 RVA: 0x001A9740 File Offset: 0x001A7940
		public Character CurrentFixer { get; private set; }

		// Token: 0x170011B9 RID: 4537
		// (get) Token: 0x0600425D RID: 16989 RVA: 0x001A9749 File Offset: 0x001A7949
		// (set) Token: 0x0600425E RID: 16990 RVA: 0x001A9751 File Offset: 0x001A7951
		public float StressDeteriorationMultiplier { get; private set; } = 1f;

		// Token: 0x170011BA RID: 4538
		// (get) Token: 0x0600425F RID: 16991 RVA: 0x001A975A File Offset: 0x001A795A
		public float TinkeringStrength
		{
			get
			{
				return this.tinkeringStrength;
			}
		}

		// Token: 0x170011BB RID: 4539
		// (get) Token: 0x06004260 RID: 16992 RVA: 0x001A9762 File Offset: 0x001A7962
		public bool TinkeringPowersDevices
		{
			get
			{
				return this.tinkeringPowersDevices;
			}
		}

		// Token: 0x170011BC RID: 4540
		// (get) Token: 0x06004261 RID: 16993 RVA: 0x001A976A File Offset: 0x001A796A
		public bool IsBelowRepairThreshold
		{
			get
			{
				return this.item.ConditionPercentageRelativeToDefaultMaxCondition < this.RepairThreshold;
			}
		}

		// Token: 0x170011BD RID: 4541
		// (get) Token: 0x06004262 RID: 16994 RVA: 0x001A977F File Offset: 0x001A797F
		public bool IsBelowRepairIconThreshold
		{
			get
			{
				return this.item.ConditionPercentageRelativeToDefaultMaxCondition < this.RepairThreshold / 2f;
			}
		}

		// Token: 0x170011BE RID: 4542
		// (get) Token: 0x06004263 RID: 16995 RVA: 0x001A979A File Offset: 0x001A799A
		// (set) Token: 0x06004264 RID: 16996 RVA: 0x001A97A2 File Offset: 0x001A79A2
		public Repairable.FixActions CurrentFixerAction
		{
			get
			{
				return this.currentFixerAction;
			}
			private set
			{
				this.currentFixerAction = value;
			}
		}

		// Token: 0x06004265 RID: 16997 RVA: 0x001A97AC File Offset: 0x001A79AC
		public Repairable(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.canBeSelected = true;
			this.item = item;
			this.header = TextManager.Get(element.GetAttributeString("header", "")).Fallback(TextManager.Get(item.Prefab.ConfigElement.GetAttributeString("header", "")), true).Fallback(element.GetAttributeString("name", ""), true);
			XAttribute xattribute;
			if ((xattribute = element.Attributes().FirstOrDefault((XAttribute a) => a.Name.ToString().Equals("showrepairuithreshold", StringComparison.OrdinalIgnoreCase))) == null)
			{
				xattribute = element.Attributes().FirstOrDefault((XAttribute a) => a.Name.ToString().Equals("airepairthreshold", StringComparison.OrdinalIgnoreCase));
			}
			XAttribute repairThresholdAttribute = xattribute;
			float repairThreshold;
			if (repairThresholdAttribute != null && float.TryParse(repairThresholdAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out repairThreshold))
			{
				this.RepairThreshold = repairThreshold;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			List<StatusEffect> onFailureEffects;
			if (campaign != null && this.statusEffectLists != null && this.statusEffectLists.TryGetValue(ActionType.OnFailure, out onFailureEffects))
			{
				foreach (StatusEffect effect in onFailureEffects)
				{
					foreach (Affliction affliction in effect.Afflictions)
					{
						if (!(affliction.Prefab.AfflictionType == Tags.Stun))
						{
							affliction.Strength *= campaign.Settings.RepairFailMultiplier;
						}
					}
				}
			}
		}

		// Token: 0x06004266 RID: 16998 RVA: 0x001A999C File Offset: 0x001A7B9C
		public override void OnItemLoaded()
		{
			this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
		}

		// Token: 0x06004267 RID: 16999 RVA: 0x001A99B8 File Offset: 0x001A7BB8
		public bool CheckCharacterSuccess(Character character, Item bestRepairItem)
		{
			if (character == null)
			{
				return false;
			}
			if (this.statusEffectLists == null)
			{
				return true;
			}
			if (bestRepairItem != null && bestRepairItem.Prefab.CannotRepairFail)
			{
				return true;
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "electrical"))
			{
				Reactor reactor = this.item.GetComponent<Reactor>();
				if (reactor != null)
				{
					if (MathUtils.NearlyEqual(reactor.CurrPowerConsumption, 0f, 0.1f))
					{
						return true;
					}
				}
				else
				{
					Powered powered = this.item.GetComponent<Powered>();
					if (powered != null && powered.Voltage < 0.1f)
					{
						return true;
					}
				}
			}
			bool success = Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) < this.RepairDegreeOfSuccess(character, this.RequiredSkills);
			ActionType actionType = success ? ActionType.OnSuccess : ActionType.OnFailure;
			Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|106_1(this, actionType, character);
			Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|106_1(this, ActionType.OnUse, character);
			if (bestRepairItem != null)
			{
				Holdable holdable = bestRepairItem.GetComponent<Holdable>();
				if (holdable != null)
				{
					Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|106_1(holdable, actionType, character);
					Repairable.<CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|106_1(holdable, ActionType.OnUse, character);
				}
			}
			return success;
		}

		// Token: 0x06004268 RID: 17000 RVA: 0x001A9AB3 File Offset: 0x001A7CB3
		public override float GetSkillMultiplier()
		{
			return this.SkillRequirementMultiplier;
		}

		// Token: 0x06004269 RID: 17001 RVA: 0x001A9ABC File Offset: 0x001A7CBC
		public float RepairDegreeOfSuccess(Character character, List<Skill> skills)
		{
			if (skills.Count == 0)
			{
				return 1f;
			}
			if (character == null)
			{
				return 0f;
			}
			float skillSum = (from t in skills
			let characterLevel = character.GetSkillLevel(t.Identifier)
			select characterLevel - t.Level * this.SkillRequirementMultiplier).Sum();
			float average = skillSum / (float)skills.Count;
			return (average + 100f) / 2f / 100f;
		}

		// Token: 0x0600426A RID: 17002 RVA: 0x001A9B40 File Offset: 0x001A7D40
		public void RepairBoost(bool qteSuccess)
		{
			if (this.CurrentFixer == null)
			{
				return;
			}
			if (qteSuccess)
			{
				this.item.Condition += this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills) * 3f * ((this.currentFixerAction == Repairable.FixActions.Repair) ? 1f : -1f);
				return;
			}
			if (Rand.Range(0f, 2f, Rand.RandSync.Unsynced) > this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills))
			{
				base.ApplyStatusEffects(ActionType.OnFailure, 1f, this.CurrentFixer, null, null, null, null, 1f);
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnFailure, this, this.CurrentFixer, null, null, null));
			}
		}

		// Token: 0x0600426B RID: 17003 RVA: 0x001A9C18 File Offset: 0x001A7E18
		public bool StartRepairing(Character character, Repairable.FixActions action)
		{
			if (character == null || character.IsDead || action == Repairable.FixActions.None)
			{
				DebugConsole.ThrowError("Invalid repair command!", null, null, false, false);
				return false;
			}
			if (this.CurrentFixerAction == Repairable.FixActions.Tinker && action != Repairable.FixActions.Tinker)
			{
				Character currentFixer = this.CurrentFixer;
				if (currentFixer != null)
				{
					currentFixer.CheckTalents(AbilityEffectType.OnStopTinkering);
				}
			}
			Item bestRepairItem = Repairable.<StartRepairing>g__GetBestRepairItem|110_0(character);
			if (this.CurrentFixer != character || this.currentFixerAction != action)
			{
				if (!this.CheckCharacterSuccess(character, bestRepairItem))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 3);
					defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(character));
					defaultInterpolatedStringHandler.AppendLiteral(" failed to ");
					defaultInterpolatedStringHandler.AppendFormatted((action == Repairable.FixActions.Sabotage) ? "sabotage" : "repair");
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
					return false;
				}
				if ((character != this.prevLoggedFixer || action != this.prevLoggedFixAction) && (character.TeamID == CharacterTeamType.Team1 || character.TeamID == CharacterTeamType.Team2))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 3);
					defaultInterpolatedStringHandler2.AppendFormatted(GameServer.CharacterLogName(character));
					defaultInterpolatedStringHandler2.AppendLiteral(" started ");
					defaultInterpolatedStringHandler2.AppendFormatted((action == Repairable.FixActions.Sabotage) ? "sabotaging" : "repairing");
					defaultInterpolatedStringHandler2.AppendLiteral(" ");
					defaultInterpolatedStringHandler2.AppendFormatted(this.item.Name);
					GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
					this.item.CreateServerEvent<Repairable>(this);
					this.prevLoggedFixer = character;
					this.prevLoggedFixAction = action;
				}
			}
			this.CurrentFixer = character;
			this.currentRepairItem = bestRepairItem;
			this.CurrentFixerAction = action;
			if (action == Repairable.FixActions.Tinker)
			{
				this.tinkeringStrength = 1f + this.CurrentFixer.GetStatValue(StatTypes.TinkeringStrength, true);
				this.tinkeringPowersDevices = this.CurrentFixer.HasAbilityFlag(AbilityFlags.TinkeringPowersDevices);
				if ((character.HasAbilityFlag(AbilityFlags.CanTinkerFabricatorsAndDeconstructors) && this.item.GetComponent<Deconstructor>() != null) || this.item.GetComponent<Fabricator>() != null)
				{
					this.tinkeringDuration = float.MaxValue;
				}
				else
				{
					this.tinkeringDuration = this.CurrentFixer.GetStatValue(StatTypes.TinkeringDuration, true);
				}
			}
			return true;
		}

		// Token: 0x0600426C RID: 17004 RVA: 0x001A9E24 File Offset: 0x001A8024
		public bool StopRepairing(Character character)
		{
			if (this.CurrentFixer == character)
			{
				if (this.CurrentFixer != character || this.currentFixerAction != Repairable.FixActions.None)
				{
					this.item.CreateServerEvent<Repairable>(this);
				}
				if (this.currentRepairItem != null)
				{
					foreach (ItemComponent ic in this.currentRepairItem.GetComponents<ItemComponent>())
					{
						ic.ApplyStatusEffects(ActionType.OnSuccess, 1f, character, null, null, null, null, 1f);
					}
				}
				if (this.CurrentFixerAction == Repairable.FixActions.Tinker)
				{
					this.CurrentFixer.CheckTalents(AbilityEffectType.OnStopTinkering);
				}
				this.CurrentFixer.AnimController.StopUsingItem();
				this.CurrentFixer = null;
				this.currentRepairItem = null;
				this.currentFixerAction = Repairable.FixActions.None;
				return true;
			}
			return false;
		}

		// Token: 0x0600426D RID: 17005 RVA: 0x001A9F00 File Offset: 0x001A8100
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x0600426E RID: 17006 RVA: 0x001A9F0A File Offset: 0x001A810A
		public void ResetDeterioration()
		{
			this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
			this.item.Condition = this.item.MaxCondition;
			this.item.CreateServerEvent<Repairable>(this);
		}

		// Token: 0x0600426F RID: 17007 RVA: 0x001A9F48 File Offset: 0x001A8148
		public override void Update(float deltaTime, Camera cam)
		{
			this.IsTinkering = false;
			int condition = (int)(this.item.Condition / (this.item.MaxCondition / this.item.MaxRepairConditionMultiplier) * 100f);
			if (this.prevSentConditionValue != condition || this.conditionSignal == null)
			{
				this.prevSentConditionValue = condition;
				this.conditionSignal = this.prevSentConditionValue.ToString();
			}
			this.item.SendSignal(this.conditionSignal, "condition_out");
			foreach (ItemComponent component in this.item.Components)
			{
				IDeteriorateUnderStress deteriorateUnderStress = component as IDeteriorateUnderStress;
				if (deteriorateUnderStress != null)
				{
					if (deteriorateUnderStress.CurrentStress >= this.StressDeteriorationThreshold)
					{
						this.StressDeteriorationMultiplier = Math.Min(this.StressDeteriorationMultiplier + deltaTime * this.StressDeteriorationIncreaseSpeed, this.MaxStressDeteriorationMultiplier);
					}
					else
					{
						this.StressDeteriorationMultiplier = Math.Max(this.StressDeteriorationMultiplier - deltaTime * this.StressDeteriorationDecreaseSpeed, 1f);
					}
				}
			}
			if (this.ForceDeteriorationTimer > 0f)
			{
				this.ForceDeteriorationTimer -= deltaTime;
				if (this.ForceDeteriorationTimer <= 0f)
				{
					this.item.CreateServerEvent<Repairable>(this);
				}
			}
			if (this.CurrentFixer == null)
			{
				this.updateDeteriorationCounter++;
				if (this.updateDeteriorationCounter >= 10)
				{
					this.UpdateDeterioration(deltaTime * 10f);
					this.updateDeteriorationCounter = 0;
				}
				return;
			}
			this.UpdateFixAnimation(this.CurrentFixer);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.CurrentFixer != null && (this.CurrentFixer.SelectedItem != this.item || !this.CurrentFixer.CanInteractWith(this.item, true) || this.CurrentFixer.IsDead))
			{
				this.StopRepairing(this.CurrentFixer);
				return;
			}
			if (this.currentFixerAction != Repairable.FixActions.Tinker)
			{
				float successFactor = (this.RequiredSkills.Count == 0) ? 1f : this.RepairDegreeOfSuccess(this.CurrentFixer, this.RequiredSkills);
				if (this.IsBelowRepairThreshold)
				{
					this.wasBroken = true;
				}
				if (this.item.ConditionPercentage > this.MinSabotageCondition)
				{
					this.wasGoodCondition = true;
				}
				float talentMultiplier = this.CurrentFixer.GetStatValue(StatTypes.RepairSpeed, true);
				foreach (Skill skill in this.RequiredSkills)
				{
					if (skill.Identifier == "mechanical")
					{
						talentMultiplier += this.CurrentFixer.GetStatValue(StatTypes.MechanicalRepairSpeed, true);
					}
					else if (skill.Identifier == "electrical")
					{
						talentMultiplier += this.CurrentFixer.GetStatValue(StatTypes.ElectricalRepairSpeed, true);
					}
				}
				float fixDuration = MathHelper.Lerp(this.FixDurationLowSkill, this.FixDurationHighSkill, successFactor);
				float num = fixDuration;
				float num2 = 1f + talentMultiplier;
				Item item = this.currentRepairItem;
				fixDuration = num / (num2 + ((item != null) ? new float?(item.Prefab.AddedRepairSpeedMultiplier) : null)).GetValueOrDefault();
				fixDuration /= 1f + this.item.GetQualityModifier(Quality.StatType.RepairSpeed);
				this.item.MaxRepairConditionMultiplier = this.GetMaxRepairConditionMultiplier(this.CurrentFixer);
				if (this.currentFixerAction == Repairable.FixActions.Repair)
				{
					if (fixDuration <= 0f)
					{
						this.item.Condition = this.item.MaxCondition;
					}
					else
					{
						float conditionIncrease = deltaTime / (fixDuration / this.item.Prefab.Health);
						this.item.Condition += conditionIncrease;
						GameMain.Server.KarmaManager.OnItemRepaired(this.CurrentFixer, this, conditionIncrease);
					}
					if (this.item.IsFullCondition)
					{
						if (this.wasBroken)
						{
							foreach (Skill skill2 in this.RequiredSkills)
							{
								CharacterInfo info = this.CurrentFixer.Info;
								if (info != null)
								{
									info.ApplySkillGain(skill2.Identifier, SkillSettings.Current.SkillIncreasePerRepair, false, 2f, false);
								}
							}
							AchievementManager.OnItemRepaired(this.item, this.CurrentFixer);
							this.CurrentFixer.CheckTalents(AbilityEffectType.OnRepairComplete, new AbilityRepairable(this.item));
						}
						Character currentFixer = this.CurrentFixer;
						if (((currentFixer != null) ? currentFixer.SelectedItem : null) == this.item)
						{
							this.CurrentFixer.SelectedItem = null;
						}
						this.deteriorationTimer = Rand.Range(this.MinDeteriorationDelay, this.MaxDeteriorationDelay, Rand.RandSync.Unsynced);
						this.wasBroken = false;
						this.StopRepairing(this.CurrentFixer);
						this.prevLoggedFixer = null;
						this.prevLoggedFixAction = Repairable.FixActions.None;
						return;
					}
				}
				else
				{
					if (this.currentFixerAction != Repairable.FixActions.Sabotage)
					{
						throw new NotImplementedException(this.currentFixerAction.ToString());
					}
					if (fixDuration <= 0f)
					{
						this.item.Condition = this.item.MaxCondition * (this.MinSabotageCondition / 100f);
					}
					else
					{
						float conditionDecrease = deltaTime / (fixDuration / this.item.Prefab.Health);
						this.item.Condition -= conditionDecrease;
					}
					if (this.item.ConditionPercentage <= this.MinSabotageCondition)
					{
						if (this.wasGoodCondition)
						{
							foreach (Skill skill3 in this.RequiredSkills)
							{
								float characterSkillLevel = this.CurrentFixer.GetSkillLevel(skill3.Identifier);
								CharacterInfo info2 = this.CurrentFixer.Info;
								if (info2 != null)
								{
									info2.IncreaseSkillLevel(skill3.Identifier, SkillSettings.Current.SkillIncreasePerSabotage / Math.Max(characterSkillLevel, 1f), false, false);
								}
							}
							this.deteriorationTimer = 0f;
							this.ForceDeteriorationTimer = this.SabotageDeteriorationDuration;
							this.item.Condition = this.item.MaxCondition * (this.MinSabotageCondition / 100f);
							this.wasGoodCondition = false;
						}
						this.StopRepairing(this.CurrentFixer);
						return;
					}
				}
				return;
			}
			this.tinkeringDuration -= deltaTime;
			float conditionDecrease2 = deltaTime * (this.CurrentFixer.GetStatValue(StatTypes.TinkeringDamage, true) / this.item.Prefab.Health) * 100f;
			this.item.Condition -= conditionDecrease2;
			if (!this.CanTinker(this.CurrentFixer) || this.tinkeringDuration <= 0f)
			{
				this.StopRepairing(this.CurrentFixer);
				return;
			}
			this.IsTinkering = true;
		}

		// Token: 0x06004270 RID: 17008 RVA: 0x001AA634 File Offset: 0x001A8834
		private void UpdateDeterioration(float deltaTime)
		{
			if (this.item.Condition <= 0f)
			{
				return;
			}
			if (!this.ShouldDeteriorate())
			{
				return;
			}
			if (this.deteriorationTimer > 0f && this.ForceDeteriorationTimer <= 0f)
			{
				if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
				{
					this.deteriorationTimer -= deltaTime * this.GetDeteriorationDelayMultiplier();
					if (this.deteriorationTimer <= 0f)
					{
						this.item.CreateServerEvent<Repairable>(this);
					}
				}
				return;
			}
			if (this.item.ConditionPercentage > this.MinDeteriorationCondition)
			{
				float deteriorationSpeed = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.DetoriationSpeed, this.DeteriorationSpeed);
				if (this.ForceDeteriorationTimer > 0f)
				{
					deteriorationSpeed = Math.Max(deteriorationSpeed, 1f);
				}
				this.item.Condition -= deteriorationSpeed * this.StressDeteriorationMultiplier * deltaTime;
			}
		}

		// Token: 0x06004271 RID: 17009 RVA: 0x001AA71C File Offset: 0x001A891C
		private float GetMaxRepairConditionMultiplier(Character character)
		{
			if (character == null)
			{
				return 1f;
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "mechanical"))
			{
				return 1f + character.GetStatValue(StatTypes.MaxRepairConditionMultiplierMechanical, true);
			}
			if (this.RequiredSkills.Any((Skill s) => s != null && s.Identifier == "electrical"))
			{
				return 1f + character.GetStatValue(StatTypes.MaxRepairConditionMultiplierElectrical, true);
			}
			return 1f;
		}

		// Token: 0x06004272 RID: 17010 RVA: 0x001AA7B0 File Offset: 0x001A89B0
		private bool IsTinkerable(Character character)
		{
			return character.HasAbilityFlag(AbilityFlags.CanTinker) && (this.item.GetComponent<Engine>() != null || this.item.GetComponent<Pump>() != null || this.item.HasTag(Tags.TurretAmmoSource) || (character.HasAbilityFlag(AbilityFlags.CanTinkerFabricatorsAndDeconstructors) && (this.item.GetComponent<Fabricator>() != null || this.item.GetComponent<Deconstructor>() != null)));
		}

		// Token: 0x06004273 RID: 17011 RVA: 0x001AA826 File Offset: 0x001A8A26
		private Affliction GetTinkerExhaustion(Character character)
		{
			return character.CharacterHealth.GetAffliction("tinkerexhaustion", true);
		}

		// Token: 0x06004274 RID: 17012 RVA: 0x001AA83C File Offset: 0x001A8A3C
		private bool CanTinker(Character character)
		{
			if (!this.IsTinkerable(character))
			{
				return false;
			}
			Affliction tinkerExhaustion = this.GetTinkerExhaustion(character);
			return tinkerExhaustion == null || tinkerExhaustion.Strength > tinkerExhaustion.Prefab.MaxStrength;
		}

		// Token: 0x06004275 RID: 17013 RVA: 0x001AA875 File Offset: 0x001A8A75
		public void AdjustPowerConsumption(ref float powerConsumption)
		{
			if (this.IsBelowRepairThreshold)
			{
				powerConsumption *= MathHelper.Lerp(1.5f, 1f, this.item.Condition / this.item.MaxCondition);
			}
		}

		// Token: 0x06004276 RID: 17014 RVA: 0x001AA8AC File Offset: 0x001A8AAC
		private bool ShouldDeteriorate()
		{
			if (this.ForceDeteriorationTimer > 0f)
			{
				return true;
			}
			if (Level.IsLoadedFriendlyOutpost)
			{
				return false;
			}
			if ((double)this.LastActiveTime > Timing.TotalTime)
			{
				return true;
			}
			foreach (ItemComponent ic in this.item.Components)
			{
				if (ic is Fabricator || ic is Deconstructor)
				{
					return false;
				}
				PowerTransfer pt = ic as PowerTransfer;
				if (pt != null)
				{
					if (pt.Voltage > 0.1f)
					{
						return true;
					}
				}
				else
				{
					PowerContainer pc = ic as PowerContainer;
					if (pc != null)
					{
						if (Math.Abs(pc.CurrPowerConsumption) > 0.1f || Math.Abs(pc.CurrPowerOutput) > 0.1f)
						{
							return true;
						}
					}
					else
					{
						Engine engine = ic as Engine;
						if (engine != null)
						{
							if (Math.Abs(engine.Force) > 1f)
							{
								return true;
							}
						}
						else
						{
							Pump pump = ic as Pump;
							if (pump != null)
							{
								if (Math.Abs(pump.FlowPercentage) > 1f && pump.IsActive && pump.HasPower)
								{
									return true;
								}
							}
							else
							{
								Reactor reactor = ic as Reactor;
								if (reactor != null)
								{
									if (reactor.Temperature > 0.1f)
									{
										return true;
									}
								}
								else
								{
									OxygenGenerator oxyGenerator = ic as OxygenGenerator;
									if (oxyGenerator != null)
									{
										if (oxyGenerator.CurrFlow > 0.1f)
										{
											return true;
										}
									}
									else
									{
										Powered powered = ic as Powered;
										if (powered != null && !(powered is LightComponent) && powered.HasPower)
										{
											return true;
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06004277 RID: 17015 RVA: 0x001AAA6C File Offset: 0x001A8C6C
		private float GetDeteriorationDelayMultiplier()
		{
			foreach (ItemComponent ic in this.item.Components)
			{
				Engine engine = ic as Engine;
				if (engine != null)
				{
					return Math.Abs(engine.Force) / 100f;
				}
				Pump pump = ic as Pump;
				if (pump != null)
				{
					return Math.Abs(pump.FlowPercentage) / 100f;
				}
				Reactor reactor = ic as Reactor;
				if (reactor != null)
				{
					return (reactor.FissionRate + reactor.TurbineOutput) / 200f;
				}
			}
			return 1f;
		}

		// Token: 0x06004278 RID: 17016 RVA: 0x001AAB28 File Offset: 0x001A8D28
		private void UpdateFixAnimation(Character character)
		{
			if (character == null || character.IsDead || character.IsIncapacitated)
			{
				return;
			}
			character.AnimController.UpdateUseItem(false, this.item.WorldPosition + new Vector2(0f, 100f) * (this.item.Condition / this.item.MaxCondition % 0.1f));
		}

		// Token: 0x06004279 RID: 17017 RVA: 0x001AAB96 File Offset: 0x001A8D96
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x0600427A RID: 17018 RVA: 0x001AAB98 File Offset: 0x001A8D98
		[CompilerGenerated]
		internal static void <CheckCharacterSuccess>g__ApplyStatusEffectsAndCreateEntityEvent|106_1(ItemComponent ic, ActionType actionType, Character character)
		{
			ic.ApplyStatusEffects(actionType, 1f, character, null, null, null, null, 1f);
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer && ic.statusEffectLists != null && ic.statusEffectLists.ContainsKey(actionType))
			{
				GameMain.NetworkMember.CreateEntityEvent(ic.Item, new Item.ApplyStatusEffectEventData(actionType, ic, character, null, null, null));
			}
		}

		// Token: 0x0600427B RID: 17019 RVA: 0x001AAC12 File Offset: 0x001A8E12
		[CompilerGenerated]
		internal static Item <StartRepairing>g__GetBestRepairItem|110_0(Character character)
		{
			return (from i in character.HeldItems
			orderby i.Prefab.AddedRepairSpeedMultiplier descending
			select i).FirstOrDefault<Item>();
		}

		// Token: 0x04001FD0 RID: 8144
		private Character prevLoggedFixer;

		// Token: 0x04001FD1 RID: 8145
		private Repairable.FixActions prevLoggedFixAction;

		// Token: 0x04001FD2 RID: 8146
		private readonly LocalizedString header;

		// Token: 0x04001FD3 RID: 8147
		private float deteriorationTimer;

		// Token: 0x04001FD5 RID: 8149
		private int updateDeteriorationCounter;

		// Token: 0x04001FD6 RID: 8150
		private const int UpdateDeteriorationInterval = 10;

		// Token: 0x04001FD7 RID: 8151
		private int prevSentConditionValue;

		// Token: 0x04001FD8 RID: 8152
		private string conditionSignal;

		// Token: 0x04001FD9 RID: 8153
		private bool wasBroken;

		// Token: 0x04001FDA RID: 8154
		private bool wasGoodCondition;

		// Token: 0x04001FDB RID: 8155
		public float LastActiveTime;

		// Token: 0x04001FE9 RID: 8169
		private float skillRequirementMultiplier;

		// Token: 0x04001FEA RID: 8170
		private bool isTinkering;

		// Token: 0x04001FEC RID: 8172
		private Item currentRepairItem;

		// Token: 0x04001FED RID: 8173
		private float tinkeringDuration;

		// Token: 0x04001FEE RID: 8174
		private float tinkeringStrength;

		// Token: 0x04001FF0 RID: 8176
		private bool tinkeringPowersDevices;

		// Token: 0x04001FF1 RID: 8177
		private Repairable.FixActions currentFixerAction;

		// Token: 0x02000DCC RID: 3532
		public enum FixActions
		{
			// Token: 0x040040BE RID: 16574
			None,
			// Token: 0x040040BF RID: 16575
			Repair,
			// Token: 0x040040C0 RID: 16576
			Sabotage,
			// Token: 0x040040C1 RID: 16577
			Tinker
		}
	}
}
