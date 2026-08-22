using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000058 RID: 88
	internal abstract class Mission
	{
		// Token: 0x17000342 RID: 834
		// (get) Token: 0x06000BCA RID: 3018 RVA: 0x0006E50B File Offset: 0x0006C70B
		public IEnumerable<LocalizedString> ShownMessages
		{
			get
			{
				return this.shownMessages;
			}
		}

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x06000BCB RID: 3019 RVA: 0x0006E513 File Offset: 0x0006C713
		public bool DisplayTargetHudIcons
		{
			get
			{
				return this.Prefab.DisplayTargetHudIcons;
			}
		}

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x06000BCC RID: 3020 RVA: 0x0006E520 File Offset: 0x0006C720
		public virtual IEnumerable<Entity> HudIconTargets
		{
			get
			{
				return Enumerable.Empty<Entity>();
			}
		}

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x06000BCD RID: 3021
		public abstract bool DisplayAsCompleted { get; }

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x06000BCE RID: 3022
		public abstract bool DisplayAsFailed { get; }

		// Token: 0x06000BCF RID: 3023 RVA: 0x0006E528 File Offset: 0x0006C728
		public Color GetDifficultyColor()
		{
			return Mission.GetDifficultyColor(this.Difficulty.GetValueOrDefault(1));
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x0006E54C File Offset: 0x0006C74C
		public static Color GetDifficultyColor(int difficulty)
		{
			float t = MathUtils.InverseLerp(1f, 4f, (float)difficulty);
			return ToolBox.GradientLerp(t, new Color[]
			{
				GUIStyle.Green,
				GUIStyle.Orange,
				GUIStyle.Red
			});
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x0006E5AC File Offset: 0x0006C7AC
		protected LocalizedString GetRewardAmountText(Submarine sub)
		{
			int baseReward = this.GetReward(sub);
			int finalReward = this.GetFinalReward(sub);
			string rewardAmountText = string.Format(CultureInfo.InvariantCulture, "{0:N0}", baseReward);
			if (finalReward > baseReward)
			{
				rewardAmountText = rewardAmountText + " + " + string.Format(CultureInfo.InvariantCulture, "{0:N0}", finalReward - baseReward);
			}
			return TextManager.GetWithVariable("currencyformat", "[credits]", rewardAmountText, FormatCapitals.No);
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x0006E61C File Offset: 0x0006C81C
		public virtual RichString GetMissionRewardText(Submarine sub)
		{
			LocalizedString rewardText = this.GetRewardAmountText(sub);
			return RichString.Rich(TextManager.GetWithVariable("missionreward", "[reward]", "‖color:gui.orange‖" + rewardText + "‖end‖", FormatCapitals.No), null);
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x0006E668 File Offset: 0x0006C868
		public RichString GetDifficultyToolTipText()
		{
			float xpBonusMultiplier = this.CalculateDifficultyXPMultiplier();
			float xpBonusPercentage = (xpBonusMultiplier - 1f) * 100f;
			LocalizedString tooltipText = TextManager.GetWithVariable("missiondifficultyxpbonustooltip", "[bonus]", ((int)Math.Round((double)xpBonusPercentage)).ToString(), FormatCapitals.No);
			return RichString.Rich(tooltipText, null);
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x0006E6B8 File Offset: 0x0006C8B8
		public RichString GetReputationRewardText()
		{
			Mission.<>c__DisplayClass16_0 CS$<>8__locals1;
			CS$<>8__locals1.reputationRewardTexts = new List<LocalizedString>();
			foreach (MissionPrefab.ReputationReward reputationReward in this.ReputationRewards)
			{
				FactionPrefab factionPrefab;
				if (reputationReward.FactionIdentifier == "location")
				{
					Faction faction = this.OriginLocation.Faction;
					factionPrefab = ((faction != null) ? faction.Prefab : null);
				}
				else
				{
					FactionPrefab.Prefabs.TryGet(reputationReward.FactionIdentifier, out factionPrefab);
				}
				if (factionPrefab != null)
				{
					Mission.<GetReputationRewardText>g__AddReputationText|16_0(factionPrefab, reputationReward.Amount, ref CS$<>8__locals1);
					FactionPrefab opposingFactionPrefab;
					if (!MathUtils.NearlyEqual(reputationReward.AmountForOpposingFaction, 0f, 0.0001f) && FactionPrefab.Prefabs.TryGet(factionPrefab.OpposingFaction, out opposingFactionPrefab))
					{
						Mission.<GetReputationRewardText>g__AddReputationText|16_0(opposingFactionPrefab, reputationReward.AmountForOpposingFaction, ref CS$<>8__locals1);
					}
				}
			}
			if (CS$<>8__locals1.reputationRewardTexts.Any<LocalizedString>())
			{
				return RichString.Rich(TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("reputation"),
					LocalizedString.Join(", ", CS$<>8__locals1.reputationRewardTexts)
				}), null);
			}
			return string.Empty;
		}

		// Token: 0x06000BD5 RID: 3029 RVA: 0x0006E7EC File Offset: 0x0006C9EC
		private IEnumerable<CoroutineStatus> ShowMessageBoxWhenRoundSummaryIsNotActive(LocalizedString header, LocalizedString message)
		{
			Mission.<ShowMessageBoxWhenRoundSummaryIsNotActive>d__17 <ShowMessageBoxWhenRoundSummaryIsNotActive>d__ = new Mission.<ShowMessageBoxWhenRoundSummaryIsNotActive>d__17(-2);
			<ShowMessageBoxWhenRoundSummaryIsNotActive>d__.<>4__this = this;
			<ShowMessageBoxWhenRoundSummaryIsNotActive>d__.<>3__header = header;
			<ShowMessageBoxWhenRoundSummaryIsNotActive>d__.<>3__message = message;
			return <ShowMessageBoxWhenRoundSummaryIsNotActive>d__;
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x0006E80C File Offset: 0x0006CA0C
		protected void CreateMessageBox(LocalizedString header, LocalizedString message)
		{
			this.shownMessages.Add(message);
			RichString headerText = RichString.Rich(header, null);
			RichString text = RichString.Rich(message, null);
			LocalizedString[] buttons = Array.Empty<LocalizedString>();
			Sprite icon = this.Prefab.Icon;
			new GUIMessageBox(headerText, text, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.InGame, "", icon, "", null, null, false).IconColor = this.Prefab.IconColor;
		}

		// Token: 0x06000BD7 RID: 3031 RVA: 0x0006E87C File Offset: 0x0006CA7C
		public Identifier GetOverrideMusicType()
		{
			return this.Prefab.GetOverrideMusicType(this.State);
		}

		// Token: 0x06000BD8 RID: 3032 RVA: 0x0006E88F File Offset: 0x0006CA8F
		public virtual void ClientRead(IReadMessage msg)
		{
			this.State = (int)msg.ReadInt16();
		}

		// Token: 0x06000BD9 RID: 3033 RVA: 0x0006E89D File Offset: 0x0006CA9D
		public virtual void ClientReadInitial(IReadMessage msg)
		{
			this.state = (int)msg.ReadInt16();
		}

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x06000BDA RID: 3034 RVA: 0x0006E8AB File Offset: 0x0006CAAB
		// (set) Token: 0x06000BDB RID: 3035 RVA: 0x0006E8B4 File Offset: 0x0006CAB4
		public virtual int State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state != value)
				{
					int previousState = this.state;
					this.state = value;
					this.TryTriggerEvents(this.state);
					if (this.Prefab.ShowProgressBar)
					{
						CharacterHUD.ShowMissionProgressBar(this);
					}
					this.ShowMessage(this.State);
					Action<Mission> onMissionStateChanged = this.OnMissionStateChanged;
					if (onMissionStateChanged != null)
					{
						onMissionStateChanged(this);
					}
					this.MissionStateChanged(previousState);
				}
			}
		}

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06000BDC RID: 3036 RVA: 0x0006E91C File Offset: 0x0006CB1C
		// (set) Token: 0x06000BDD RID: 3037 RVA: 0x0006E924 File Offset: 0x0006CB24
		public int TimesAttempted { get; set; }

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06000BDE RID: 3038 RVA: 0x0006E92D File Offset: 0x0006CB2D
		protected static bool IsClient
		{
			get
			{
				return GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient;
			}
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x06000BDF RID: 3039 RVA: 0x0006E942 File Offset: 0x0006CB42
		public virtual LocalizedString Name
		{
			get
			{
				return this.Prefab.Name;
			}
		}

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x06000BE0 RID: 3040 RVA: 0x0006E94F File Offset: 0x0006CB4F
		public virtual LocalizedString SuccessMessage
		{
			get
			{
				return this.successMessage;
			}
		}

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06000BE1 RID: 3041 RVA: 0x0006E957 File Offset: 0x0006CB57
		public virtual LocalizedString FailureMessage
		{
			get
			{
				return this.failureMessage;
			}
		}

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06000BE2 RID: 3042 RVA: 0x0006E95F File Offset: 0x0006CB5F
		public virtual LocalizedString Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06000BE3 RID: 3043 RVA: 0x0006E967 File Offset: 0x0006CB67
		public virtual bool AllowUndocking
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x06000BE4 RID: 3044 RVA: 0x0006E96A File Offset: 0x0006CB6A
		public virtual int Reward
		{
			get
			{
				return this.Prefab.Reward;
			}
		}

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06000BE5 RID: 3045 RVA: 0x0006E977 File Offset: 0x0006CB77
		public ImmutableList<MissionPrefab.ReputationReward> ReputationRewards
		{
			get
			{
				return this.Prefab.ReputationRewards;
			}
		}

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x06000BE6 RID: 3046 RVA: 0x0006E984 File Offset: 0x0006CB84
		// (set) Token: 0x06000BE7 RID: 3047 RVA: 0x0006E98C File Offset: 0x0006CB8C
		public bool Completed
		{
			get
			{
				return this.completed;
			}
			set
			{
				this.completed = value;
			}
		}

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06000BE8 RID: 3048 RVA: 0x0006E995 File Offset: 0x0006CB95
		public bool Failed
		{
			get
			{
				return this.failed || this.ForceFailure;
			}
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x06000BE9 RID: 3049 RVA: 0x0006E9A7 File Offset: 0x0006CBA7
		public virtual bool AllowRespawning
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x06000BEA RID: 3050 RVA: 0x0006E9AA File Offset: 0x0006CBAA
		public virtual int TeamCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x06000BEB RID: 3051 RVA: 0x0006E9AD File Offset: 0x0006CBAD
		public virtual SubmarineInfo EnemySubmarineInfo
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06000BEC RID: 3052 RVA: 0x0006E9B0 File Offset: 0x0006CBB0
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public virtual IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				return Enumerable.Empty<ValueTuple<LocalizedString, Vector2>>();
			}
		}

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06000BED RID: 3053 RVA: 0x0006E9B7 File Offset: 0x0006CBB7
		public Identifier SonarIconIdentifier
		{
			get
			{
				return this.Prefab.SonarIconIdentifier;
			}
		}

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x06000BEE RID: 3054 RVA: 0x0006E9C4 File Offset: 0x0006CBC4
		public int? Difficulty
		{
			get
			{
				return this.Prefab.Difficulty;
			}
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x0006E9D4 File Offset: 0x0006CBD4
		public Mission(MissionPrefab prefab, Location[] locations, Submarine sub)
		{
			this.Prefab = prefab;
			this.description = prefab.Description.Value;
			this.successMessage = prefab.SuccessMessage.Value;
			this.failureMessage = prefab.FailureMessage.Value;
			this.Headers = prefab.Headers;
			LocalizedString[] messages = prefab.Messages.ToArray<LocalizedString>();
			this.OriginLocation = locations[0];
			this.Locations = locations;
			ContentXElement endConditionElement = prefab.ConfigElement.GetChildElement("completeCheckDataAction");
			ContentXElement contentXElement = null;
			if (endConditionElement != contentXElement)
			{
				ContentXElement element = endConditionElement;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Mission (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				this.completeCheckDataAction = new CheckDataAction(element, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.descriptionWithoutReward = this.ReplaceVariablesInMissionMessage(this.description, sub, false);
			this.description = this.ReplaceVariablesInMissionMessage(this.description, sub, true);
			this.successMessage = this.ReplaceVariablesInMissionMessage(this.successMessage, sub, true);
			this.failureMessage = this.ReplaceVariablesInMissionMessage(this.failureMessage, sub, true);
			for (int i = 0; i < messages.Length; i++)
			{
				messages[i] = this.ReplaceVariablesInMissionMessage(messages[i], sub, true);
			}
			this.Messages = messages.ToImmutableArray<LocalizedString>();
			this.characterConfig = prefab.ConfigElement.GetChildElement("Characters");
			if (prefab.ConfigElement.GetChildElements("Characters").Count<ContentXElement>() > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(89, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in mission ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(": multiple <Characters> elements found. Only the first one will be used.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), prefab.ContentPackage);
			}
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x0006EBDC File Offset: 0x0006CDDC
		public LocalizedString ReplaceVariablesInMissionMessage(LocalizedString message, Submarine sub, bool replaceReward = true)
		{
			for (int locationIndex = 0; locationIndex < 2; locationIndex++)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.orange‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Locations[locationIndex].DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
				string locationName = defaultInterpolatedStringHandler.ToStringAndClear();
				message = message.Replace("[location" + (locationIndex + 1).ToString() + "]", locationName, StringComparison.Ordinal);
			}
			if (replaceReward)
			{
				string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.GetReward(sub)) + "‖end‖";
				message = message.Replace("[reward]", rewardText, StringComparison.Ordinal);
			}
			return message;
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x0006ECA1 File Offset: 0x0006CEA1
		protected virtual void MissionStateChanged(int previousState)
		{
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x0006ECA3 File Offset: 0x0006CEA3
		public virtual void SetLevel(LevelData level)
		{
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x0006ECA5 File Offset: 0x0006CEA5
		public static Mission LoadRandom(Location[] locations, string seed, bool requireCorrectLocationType, IEnumerable<Identifier> missionTypes, bool isSinglePlayer = false, float? difficultyLevel = null)
		{
			return Mission.LoadRandom(locations, new MTRandom(ToolBox.StringToInt(seed)), requireCorrectLocationType, missionTypes, isSinglePlayer, difficultyLevel);
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x0006ECC0 File Offset: 0x0006CEC0
		public static Mission LoadRandom(Location[] locations, MTRandom rand, bool requireCorrectLocationType, IEnumerable<Identifier> missionTypes, bool isSinglePlayer = false, float? difficultyLevel = null)
		{
			List<MissionPrefab> allowedMissions = new List<MissionPrefab>();
			if (missionTypes.None(null))
			{
				return null;
			}
			allowedMissions.AddRange(from m in MissionPrefab.Prefabs
			where missionTypes.Contains(m.Type)
			select m);
			allowedMissions.RemoveAll(delegate(MissionPrefab m)
			{
				if (!isSinglePlayer)
				{
					return m.SingleplayerOnly;
				}
				return m.MultiplayerOnly;
			});
			if (requireCorrectLocationType)
			{
				allowedMissions.RemoveAll((MissionPrefab m) => !m.IsAllowed(locations[0], locations[1]));
			}
			if (difficultyLevel != null)
			{
				allowedMissions.RemoveAll((MissionPrefab m) => !m.IsAllowedDifficulty(difficultyLevel.Value));
			}
			if (allowedMissions.Count == 0)
			{
				return null;
			}
			MissionPrefab missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(allowedMissions, (MissionPrefab m) => (float)m.Commonness, rand);
			return missionPrefab.Instantiate(locations, Submarine.MainSub);
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x0006EDAE File Offset: 0x0006CFAE
		public virtual float GetBaseReward(Submarine sub)
		{
			return (float)this.Prefab.Reward;
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0006EDBC File Offset: 0x0006CFBC
		public int GetReward(Submarine sub)
		{
			float reward = this.GetBaseReward(sub);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				reward *= campaign.Settings.MissionRewardMultiplier;
			}
			return (int)Math.Round((double)reward);
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x0006EDFC File Offset: 0x0006CFFC
		protected void InitCharacters(Submarine submarine)
		{
			this.characters.Clear();
			this.characterItems.Clear();
			ContentXElement contentXElement = null;
			if (this.characterConfig != contentXElement)
			{
				foreach (ContentXElement cxe in this.characterConfig.Elements())
				{
					XElement element = cxe;
					if (GameMain.NetworkMember != null || !element.GetAttributeBool("multiplayeronly", false))
					{
						int defaultCount = element.GetAttributeInt("count", -1);
						if (defaultCount < 0)
						{
							defaultCount = element.GetAttributeInt("amount", 1);
						}
						int min = Math.Min(element.GetAttributeInt("min", defaultCount), 255);
						int max = Math.Min(Math.Max(min, element.GetAttributeInt("max", defaultCount)), 255);
						int count = Rand.Range(min, max + 1, Rand.RandSync.Unsynced);
						if (element.Attribute("identifier") != null && element.Attribute("from") != null)
						{
							HumanPrefab humanPrefab = this.GetHumanPrefabFromElement(element);
							if (humanPrefab == null)
							{
								DebugConsole.ThrowError("Couldn't spawn a human character for a mission: human prefab \"" + element.GetAttributeString("identifier", string.Empty) + "\" not found", null, this.Prefab.ContentPackage, false, false);
							}
							else
							{
								for (int i = 0; i < count; i++)
								{
									this.LoadHuman(humanPrefab, element, submarine);
								}
							}
						}
						else
						{
							Identifier speciesName = element.GetAttributeIdentifier("character", element.GetAttributeIdentifier("identifier", Identifier.Empty));
							CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
							if (characterPrefab == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
								defaultInterpolatedStringHandler.AppendLiteral("Couldn't spawn a character for a mission: character prefab \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
								defaultInterpolatedStringHandler.AppendLiteral("\" not found");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
							}
							else
							{
								for (int j = 0; j < count; j++)
								{
									this.LoadMonster(characterPrefab, element, submarine);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0006F01C File Offset: 0x0006D21C
		private SpawnAction.SpawnLocationType GetSpawnLocationTypeFromSubmarineType(Submarine sub)
		{
			switch (sub.Info.Type)
			{
			case SubmarineType.Player:
				return SpawnAction.SpawnLocationType.MainSub;
			case SubmarineType.Outpost:
			case SubmarineType.OutpostModule:
				return SpawnAction.SpawnLocationType.Outpost;
			case SubmarineType.Wreck:
				return SpawnAction.SpawnLocationType.Wreck;
			case SubmarineType.BeaconStation:
				return SpawnAction.SpawnLocationType.BeaconStation;
			case SubmarineType.Ruin:
				return SpawnAction.SpawnLocationType.Ruin;
			}
			return SpawnAction.SpawnLocationType.Any;
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x0006F070 File Offset: 0x0006D270
		protected virtual Character LoadHuman(HumanPrefab humanPrefab, XElement element, Submarine submarine)
		{
			Identifier[] moduleFlags = element.GetAttributeIdentifierArray("moduleflags", null, true);
			Identifier[] spawnPointTags = element.GetAttributeIdentifierArray("spawnpointtags", null, true);
			SpawnType spawnPointType = element.GetAttributeEnum("spawnpointtype", SpawnType.Human);
			SpawnAction.SpawnLocationType spawnLocationTypeFromSubmarineType = this.GetSpawnLocationTypeFromSubmarineType(submarine);
			SpawnType? spawnPointType2 = new SpawnType?(spawnPointType);
			IEnumerable<Identifier> enumerable = moduleFlags;
			IEnumerable<Identifier> moduleFlags2 = enumerable ?? humanPrefab.GetModuleFlags();
			enumerable = spawnPointTags;
			ISpatialEntity spawnPos = SpawnAction.GetSpawnPos(spawnLocationTypeFromSubmarineType, spawnPointType2, moduleFlags2, enumerable ?? humanPrefab.GetSpawnPointTags(), element.GetAttributeBool("asfaraspossible", false), false, true);
			if (spawnPos == null)
			{
				spawnPos = submarine.GetHulls(false).GetRandomUnsynced<Hull>();
			}
			CharacterTeamType teamId = element.GetAttributeEnum("teamid", CharacterTeamType.None);
			Submarine startOutpost = Level.Loaded.StartOutpost;
			CharacterTeamType originalTeam = (startOutpost != null) ? startOutpost.TeamID : teamId;
			Character spawnedCharacter = Mission.CreateHuman(humanPrefab, this.characters, this.characterItems, submarine, originalTeam, spawnPos, Rand.RandSync.ServerAndClient);
			if (teamId != originalTeam)
			{
				spawnedCharacter.SetOriginalTeamAndChangeTeam(teamId, true);
			}
			if (element.GetAttribute("color", StringComparison.OrdinalIgnoreCase) != null)
			{
				spawnedCharacter.UniqueNameColor = new Color?(element.GetAttributeColor("color", Color.Red));
			}
			SubmarineInfo outPostInfo = submarine.Info;
			if (outPostInfo != null && outPostInfo.IsOutpost)
			{
				outPostInfo.AddOutpostNPCIdentifierOrTag(spawnedCharacter, humanPrefab.Identifier);
				foreach (Identifier tag in humanPrefab.GetTags())
				{
					outPostInfo.AddOutpostNPCIdentifierOrTag(spawnedCharacter, tag);
				}
			}
			WayPoint wp = spawnPos as WayPoint;
			if (wp != null)
			{
				spawnedCharacter.GiveIdCardTags(wp, false);
			}
			this.InitCharacter(spawnedCharacter, element);
			return spawnedCharacter;
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x0006F200 File Offset: 0x0006D400
		protected virtual Character LoadMonster(CharacterPrefab monsterPrefab, XElement element, Submarine submarine)
		{
			Identifier[] moduleFlags = element.GetAttributeIdentifierArray("moduleflags", null, true);
			Identifier[] spawnPointTags = element.GetAttributeIdentifierArray("spawnpointtags", null, true);
			ISpatialEntity spawnPos = SpawnAction.GetSpawnPos(SpawnAction.SpawnLocationType.Outpost, new SpawnType?(SpawnType.Enemy), moduleFlags, spawnPointTags, element.GetAttributeBool("asfaraspossible", false), false, true);
			if (spawnPos == null)
			{
				spawnPos = submarine.GetHulls(false).GetRandomUnsynced<Hull>();
			}
			Character spawnedCharacter = Character.Create(monsterPrefab.Identifier, spawnPos.WorldPosition, ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true);
			this.characters.Add(spawnedCharacter);
			if (spawnedCharacter.Inventory != null)
			{
				this.characterItems.Add(spawnedCharacter, spawnedCharacter.Inventory.FindAllItems(null, true, null));
			}
			EnemyAIController enemyAi = spawnedCharacter.AIController as EnemyAIController;
			if (enemyAi != null && submarine != null)
			{
				enemyAi.SetUnattackableSubmarines(submarine, true, true, true);
			}
			this.InitCharacter(spawnedCharacter, element);
			return spawnedCharacter;
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x0006F2CC File Offset: 0x0006D4CC
		protected virtual void InitCharacter(Character character, XElement element)
		{
			if (element.GetAttributeBool(Tags.IgnoredByAI.Value, false))
			{
				character.AddAbilityFlag(AbilityFlags.IgnoredByEnemyAI);
			}
			float playDeadProbability = element.GetAttributeFloat("playdeadprobability", -1f);
			if (playDeadProbability >= 0f)
			{
				character.EvaluatePlayDeadProbability(new float?(playDeadProbability));
			}
			float huskProbability = element.GetAttributeFloat("huskprobability", 0f);
			if (huskProbability > 0f && Rand.Value(Rand.RandSync.Unsynced) <= huskProbability)
			{
				character.TurnIntoHusk(null, null);
				return;
			}
			if (element.GetAttributeBool("corpse", false))
			{
				character.Kill(CauseOfDeathType.Unknown, null, false, false);
			}
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x0006F364 File Offset: 0x0006D564
		public void Start(Level level)
		{
			this.state = 0;
			this.shownMessages.Clear();
			this.delayedTriggerEvents.Clear();
			using (List<string>.Enumerator enumerator = this.Prefab.UnhideEntitySubCategories.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string categoryToShow = enumerator.Current;
					IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
					Func<MapEntity, bool> predicate;
					Func<MapEntity, bool> <>9__0;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = delegate(MapEntity me)
						{
							MapEntityPrefab prefab = me.Prefab;
							return prefab != null && prefab.HasSubCategory(categoryToShow);
						});
					}
					foreach (MapEntity entityToShow in mapEntityList.Where(predicate))
					{
						entityToShow.IsLayerHidden = false;
					}
				}
			}
			this.level = level;
			this.TryTriggerEvents(0);
			this.StartMissionSpecific(level);
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x0006F454 File Offset: 0x0006D654
		protected virtual void StartMissionSpecific(Level level)
		{
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x0006F458 File Offset: 0x0006D658
		public void Update(float deltaTime)
		{
			for (int i = this.delayedTriggerEvents.Count - 1; i >= 0; i--)
			{
				this.delayedTriggerEvents[i].Delay -= deltaTime;
				if (this.delayedTriggerEvents[i].Delay <= 0f)
				{
					this.TriggerEvent(this.delayedTriggerEvents[i].TriggerEvent);
					this.delayedTriggerEvents.RemoveAt(i);
				}
			}
			this.UpdateMissionSpecific(deltaTime);
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x0006F4D8 File Offset: 0x0006D6D8
		protected virtual void UpdateMissionSpecific(float deltaTime)
		{
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x0006F4DA File Offset: 0x0006D6DA
		protected void ShowMessage(int missionState)
		{
			this.ShowMessageProjSpecific(missionState);
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x0006F4E4 File Offset: 0x0006D6E4
		private void ShowMessageProjSpecific(int missionState)
		{
			int messageIndex = missionState - 1;
			if (messageIndex >= this.Headers.Length && messageIndex >= this.Messages.Length)
			{
				return;
			}
			if (messageIndex < 0)
			{
				return;
			}
			LocalizedString header = (messageIndex < this.Headers.Length) ? this.Headers[messageIndex] : "";
			LocalizedString message = (messageIndex < this.Messages.Length) ? this.Messages[messageIndex] : "";
			if (!message.IsNullOrEmpty())
			{
				message = this.ModifyMessage(message, true);
			}
			CoroutineManager.StartCoroutine(this.ShowMessageBoxWhenRoundSummaryIsNotActive(header, message), "");
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x0006F589 File Offset: 0x0006D789
		protected virtual LocalizedString ModifyMessage(LocalizedString message, bool color = true)
		{
			return message;
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0006F58C File Offset: 0x0006D78C
		private void TryTriggerEvents(int state)
		{
			foreach (MissionPrefab.TriggerEvent triggerEvent in this.Prefab.TriggerEvents)
			{
				if (triggerEvent.State == state)
				{
					this.TryTriggerEvent(triggerEvent);
				}
			}
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x0006F5F0 File Offset: 0x0006D7F0
		private void TryTriggerEvent(MissionPrefab.TriggerEvent trigger)
		{
			if (trigger.CampaignOnly)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					return;
				}
			}
			if (trigger.Delay > 0f || trigger.State == 0)
			{
				if (!this.delayedTriggerEvents.Any((Mission.DelayedTriggerEvent t) => t.TriggerEvent == trigger))
				{
					this.delayedTriggerEvents.Add(new Mission.DelayedTriggerEvent(trigger, trigger.Delay));
					return;
				}
			}
			else
			{
				this.TriggerEvent(trigger);
			}
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x0006F694 File Offset: 0x0006D894
		private void TriggerEvent(MissionPrefab.TriggerEvent trigger)
		{
			if (trigger.CampaignOnly)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					return;
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			EventPrefab eventPrefab = EventPrefab.FindEventPrefab(trigger.EventIdentifier, trigger.EventTag, this.Prefab.ContentPackage);
			if (eventPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Mission ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" failed to trigger an event (identifier: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(trigger.EventIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", tag: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(trigger.EventTag);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			GameSession gameSession2 = GameMain.GameSession;
			if (((gameSession2 != null) ? gameSession2.EventManager : null) != null)
			{
				Event newEvent = eventPrefab.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
				newEvent.TriggeringMission = this;
				GameMain.GameSession.EventManager.ActivateEvent(newEvent);
			}
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x0006F7B4 File Offset: 0x0006D9B4
		public void End(CampaignMode.TransitionType transitionType)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient)
			{
				this.completed = (!this.ForceFailure && this.DetermineCompleted(transitionType) && (this.completeCheckDataAction == null || this.completeCheckDataAction.GetSuccess()));
			}
			if (this.completed)
			{
				if (this.Prefab.LocationTypeChangeOnCompleted != null)
				{
					this.ChangeLocationType(this.Prefab.LocationTypeChangeOnCompleted);
				}
				try
				{
					this.GiveReward();
				}
				catch (Exception e)
				{
					string errorMsg = "Unknown error while giving mission rewards.";
					DebugConsole.ThrowError(errorMsg, e, this.Prefab.ContentPackage, false, false);
					GameAnalyticsManager.AddErrorEventOnce("Mission.End:GiveReward", GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + e.StackTrace);
				}
			}
			int timesAttempted = this.TimesAttempted;
			this.TimesAttempted = timesAttempted + 1;
			this.EndMissionSpecific(this.completed);
			if (this.ForceFailure)
			{
				this.failed = true;
			}
		}

		// Token: 0x06000C07 RID: 3079
		protected abstract bool DetermineCompleted(CampaignMode.TransitionType transitionType);

		// Token: 0x06000C08 RID: 3080 RVA: 0x0006F8A8 File Offset: 0x0006DAA8
		protected virtual void EndMissionSpecific(bool completed)
		{
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x0006F8AC File Offset: 0x0006DAAC
		public int GetFinalReward(Submarine sub)
		{
			int? num = this.finalReward;
			if (num == null)
			{
				return this.GetReward(sub);
			}
			return num.GetValueOrDefault();
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x0006F8D8 File Offset: 0x0006DAD8
		private void CalculateFinalReward(Submarine sub)
		{
			int reward = this.GetReward(sub);
			IEnumerable<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			AbilityMissionMoneyGainMultiplier missionMoneyGainMultiplier = new AbilityMissionMoneyGainMultiplier(this, 1f);
			CharacterTalent.CheckTalentsForCrew(crewCharacters, AbilityEffectType.OnGainMissionMoney, missionMoneyGainMultiplier);
			crewCharacters.ForEach(delegate(Character c)
			{
				missionMoneyGainMultiplier.Value += c.GetStatValue(StatTypes.MissionMoneyGainMultiplier, true);
			});
			this.finalReward = new int?((int)((float)reward * missionMoneyGainMultiplier.Value));
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x0006F948 File Offset: 0x0006DB48
		private float CalculateDifficultyXPMultiplier()
		{
			float selectedMissionDifficulty = MathUtils.InverseLerp(1f, 4f, (float)this.Prefab.Difficulty.GetValueOrDefault());
			return MathHelper.Lerp(1f, 1.3f, selectedMissionDifficulty);
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x0006F98C File Offset: 0x0006DB8C
		private void GiveReward()
		{
			Mission.<>c__DisplayClass113_0 CS$<>8__locals1 = new Mission.<>c__DisplayClass113_0();
			GameMode gameMode = GameMain.GameSession.GameMode;
			CS$<>8__locals1.campaign = (gameMode as CampaignMode);
			if (CS$<>8__locals1.campaign == null)
			{
				return;
			}
			float xpReward = this.GetBaseReward(Submarine.MainSub) * this.Prefab.ExperienceMultiplier * CS$<>8__locals1.campaign.Settings.ExperienceRewardMultiplier;
			float xpGain = xpReward * this.level.LevelData.Biome.ExperienceFromMissionRewards * this.CalculateDifficultyXPMultiplier();
			IEnumerable<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			CS$<>8__locals1.experienceGainMultiplier = new AbilityMissionExperienceGainMultiplier(this, 1f, null);
			crewCharacters.ForEach(delegate(Character c)
			{
				CS$<>8__locals1.experienceGainMultiplier.Value += c.GetStatValue(StatTypes.MissionExperienceGainMultiplier, true);
			});
			this.DistributeExperienceToCrew(crewCharacters, (int)(xpGain * CS$<>8__locals1.experienceGainMultiplier.Value));
			this.CalculateFinalReward(Submarine.MainSub);
			bool flag;
			if (!GameMain.IsSingleplayer)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				flag = (networkMember != null && networkMember.IsServer);
			}
			else
			{
				flag = true;
			}
			bool isSingleplayerOrServer = flag;
			if (isSingleplayerOrServer)
			{
				int? num = this.finalReward;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					CS$<>8__locals1.campaign.Bank.Give(this.finalReward.Value);
				}
				foreach (Character character in crewCharacters)
				{
					character.Info.MissionsCompletedSinceDeath++;
				}
				using (ImmutableList<MissionPrefab.ReputationReward>.Enumerator enumerator2 = this.ReputationRewards.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						MissionPrefab.ReputationReward reputationReward = enumerator2.Current;
						AbilityMissionReputationGainMultiplier reputationGainMultiplier = new AbilityMissionReputationGainMultiplier(this, 1f, null);
						foreach (Character c2 in crewCharacters)
						{
							c2.CheckTalents(AbilityEffectType.OnCrewGainMissionReputation, reputationGainMultiplier);
						}
						float amount = reputationReward.Amount * reputationGainMultiplier.Value;
						if (reputationReward.FactionIdentifier == "location")
						{
							Reputation reputation = this.OriginLocation.Reputation;
							if (reputation != null)
							{
								reputation.AddReputation(amount, float.MaxValue);
							}
							CS$<>8__locals1.<GiveReward>g__TryGiveReputationForOpposingFaction|1(this.OriginLocation.Faction, reputationReward.AmountForOpposingFaction);
						}
						else
						{
							Faction faction = CS$<>8__locals1.campaign.Factions.Find((Faction faction1) => faction1.Prefab.Identifier == reputationReward.FactionIdentifier);
							if (faction != null)
							{
								faction.Reputation.AddReputation(amount, float.MaxValue);
								CS$<>8__locals1.<GiveReward>g__TryGiveReputationForOpposingFaction|1(faction, reputationReward.AmountForOpposingFaction);
							}
						}
					}
				}
			}
			if (this.Prefab.DataRewards != null)
			{
				foreach (ValueTuple<Identifier, object, SetDataAction.OperationType> valueTuple in this.Prefab.DataRewards)
				{
					Identifier identifier = valueTuple.Item1;
					object value = valueTuple.Item2;
					SetDataAction.OperationType operation = valueTuple.Item3;
					SetDataAction.PerformOperation(CS$<>8__locals1.campaign.CampaignMetadata, identifier, value, operation);
				}
			}
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x0006FD04 File Offset: 0x0006DF04
		private void DistributeExperienceToCrew(IEnumerable<Character> crew, int experienceGain)
		{
			Mission.<>c__DisplayClass114_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.crew = crew;
			CS$<>8__locals1.experienceGain = experienceGain;
			foreach (Character character in CS$<>8__locals1.crew)
			{
				this.<DistributeExperienceToCrew>g__GiveMissionExperience|114_0(character.Info, ref CS$<>8__locals1);
			}
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x0006FD70 File Offset: 0x0006DF70
		public static int GetRewardDistibutionSum(IEnumerable<Character> crew, int rewardDistribution = 0)
		{
			return crew.Sum((Character c) => c.Wallet.RewardDistribution) + rewardDistribution;
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x0006FD9C File Offset: 0x0006DF9C
		[return: TupleElementNames(new string[]
		{
			"Amount",
			"Percentage",
			"Sum"
		})]
		public static ValueTuple<int, int, float> GetRewardShare(int rewardDistribution, IEnumerable<Character> crew, Option<int> reward)
		{
			float sum = (float)Mission.GetRewardDistibutionSum(crew, rewardDistribution);
			if (MathUtils.NearlyEqual(sum, 0f, 0.0001f))
			{
				return new ValueTuple<int, int, float>(0, 0, sum);
			}
			float rewardWeight = (sum > 100f) ? ((float)rewardDistribution / sum) : ((float)rewardDistribution / 100f);
			int rewardPercentage = (int)(rewardWeight * 100f);
			int a;
			int amount = reward.TryUnwrap(out a) ? a : 0;
			return new ValueTuple<int, int, float>((int)((float)amount * rewardWeight), rewardPercentage, sum);
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x0006FE0C File Offset: 0x0006E00C
		protected void ChangeLocationType(LocationTypeChange change)
		{
			if (change == null)
			{
				throw new ArgumentException();
			}
			CampaignMode campaign = GameMain.GameSession.GameMode as CampaignMode;
			if (campaign != null && !Mission.IsClient)
			{
				int srcIndex = -1;
				for (int i = 0; i < this.Locations.Length; i++)
				{
					if (this.Locations[i].Type.Identifier == change.CurrentType)
					{
						srcIndex = i;
						break;
					}
				}
				if (srcIndex == -1)
				{
					return;
				}
				Location location = this.Locations[srcIndex];
				if (location.LocationTypeChangesBlocked)
				{
					return;
				}
				if (change.RequiredDurationRange.X > 0)
				{
					location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(change, Rand.Range(change.RequiredDurationRange.X, change.RequiredDurationRange.Y, Rand.RandSync.Unsynced), this.Prefab));
					return;
				}
				location.ChangeType(campaign, LocationType.Prefabs[change.ChangeToType], true, true);
				location.LocationTypeChangeCooldown = change.CooldownAfterChange;
			}
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x0006FEF8 File Offset: 0x0006E0F8
		public virtual void AdjustLevelData(LevelData levelData)
		{
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x0006FEFC File Offset: 0x0006E0FC
		protected HumanPrefab GetHumanPrefabFromElement(XElement element)
		{
			if (element.Attribute("name") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - use character identifiers instead of names to configure the characters.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			Identifier characterIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			Identifier characterFrom = element.GetAttributeIdentifier("from", Identifier.Empty);
			HumanPrefab humanPrefab = NPCSet.Get(characterFrom, characterIdentifier, true, this.Prefab.ContentPackage);
			if (humanPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn character for mission: character prefab \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(characterIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" not found in the NPC set \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(characterFrom);
				defaultInterpolatedStringHandler2.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			return humanPrefab;
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0006FFFC File Offset: 0x0006E1FC
		protected static Character CreateHuman(HumanPrefab humanPrefab, List<Character> characters, Dictionary<Character, List<Item>> characterItems, Submarine submarine, CharacterTeamType teamType, ISpatialEntity positionToStayIn = null, Rand.RandSync humanPrefabRandSync = Rand.RandSync.ServerAndClient)
		{
			CharacterInfo characterInfo = humanPrefab.CreateCharacterInfo(Rand.RandSync.ServerAndClient);
			characterInfo.TeamID = teamType;
			if (positionToStayIn == null)
			{
				SpawnType spawnType = SpawnType.Human;
				Job job = characterInfo.Job;
				positionToStayIn = (WayPoint.GetRandom(spawnType, (job != null) ? job.Prefab : null, submarine, false, null, false) ?? WayPoint.GetRandom(SpawnType.Human, null, submarine, false, null, false));
			}
			Character spawnedCharacter = Character.Create(characterInfo.SpeciesName, positionToStayIn.WorldPosition, ToolBox.RandomSeed(8), characterInfo, 0, false, true, false, null, true, true);
			spawnedCharacter.HumanPrefab = humanPrefab;
			humanPrefab.InitializeCharacter(spawnedCharacter, positionToStayIn);
			humanPrefab.GiveItems(spawnedCharacter, submarine, positionToStayIn as WayPoint, Rand.RandSync.ServerAndClient, false);
			characters.Add(spawnedCharacter);
			characterItems.Add(spawnedCharacter, spawnedCharacter.Inventory.FindAllItems(null, true, null));
			return spawnedCharacter;
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x000700AC File Offset: 0x0006E2AC
		protected ItemPrefab FindItemPrefab(XElement element)
		{
			ItemPrefab itemPrefab;
			if (element.Attribute("name") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - use item identifiers instead of names to configure the items");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				string itemName = element.GetAttributeString("name", "");
				itemPrefab = (MapEntityPrefab.Find(itemName, null, true) as ItemPrefab);
				if (itemPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn item for mission \"");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\": item prefab \"");
					defaultInterpolatedStringHandler2.AppendFormatted(itemName);
					defaultInterpolatedStringHandler2.AppendLiteral("\" not found");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			else
			{
				string itemIdentifier = element.GetAttributeString("identifier", "");
				itemPrefab = (MapEntityPrefab.Find(null, itemIdentifier, true) as ItemPrefab);
				if (itemPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Couldn't spawn item for mission \"");
					defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(this.Name);
					defaultInterpolatedStringHandler3.AppendLiteral("\": item prefab \"");
					defaultInterpolatedStringHandler3.AppendFormatted(itemIdentifier);
					defaultInterpolatedStringHandler3.AppendLiteral("\" not found");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			return itemPrefab;
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x00070224 File Offset: 0x0006E424
		protected Vector2? GetCargoSpawnPosition(ItemPrefab itemPrefab, out Submarine cargoRoomSub)
		{
			cargoRoomSub = null;
			WayPoint cargoSpawnPos = WayPoint.GetRandom(SpawnType.Cargo, null, Submarine.MainSub, true, null, false);
			if (cargoSpawnPos == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Couldn't spawn items for mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\": no waypoints marked as Cargo were found");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			Hull cargoRoom = cargoSpawnPos.CurrentHull;
			if (cargoRoom == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(91, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn items for mission \"");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\": waypoints marked as Cargo must be placed inside a room");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			cargoRoomSub = cargoRoom.Submarine;
			return new Vector2?(new Vector2(cargoSpawnPos.Position.X + Rand.Range(-20f, 20f, Rand.RandSync.ServerAndClient), (float)(cargoRoom.Rect.Y - cargoRoom.Rect.Height) + itemPrefab.Size.Y / 2f));
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x00070350 File Offset: 0x0006E550
		protected static SubmarineInfo GetRandomSubmarineByTagsAndDifficulty(IEnumerable<Identifier> tags, LevelData levelData, Func<SubmarineInfo, bool> submarineSelector, string submarineTypeName)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
			float levelDifficulty = levelData.Difficulty;
			List<SubmarineInfo> submarinesWithTags = SubmarineInfo.SavedSubmarines.Where(submarineSelector).Where(delegate(SubmarineInfo s)
			{
				ExtraSubmarineInfo extraInfo = s.GetExtraSubmarineInfo;
				return extraInfo != null && (tags.None(null) || tags.Any((Identifier t) => extraInfo.MissionTags.Contains(t)));
			}).ToList<SubmarineInfo>();
			List<SubmarineInfo> matchingSubmarines = submarinesWithTags.Where(delegate(SubmarineInfo s)
			{
				ExtraSubmarineInfo extraInfo = s.GetExtraSubmarineInfo;
				return extraInfo != null && levelDifficulty >= extraInfo.MinLevelDifficulty && levelDifficulty <= extraInfo.MaxLevelDifficulty;
			}).ToList<SubmarineInfo>();
			if (matchingSubmarines.Count == 0)
			{
				if (submarinesWithTags.Count > 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Found ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(submarinesWithTags.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(submarineTypeName);
					defaultInterpolatedStringHandler.AppendLiteral("(s) with matching tags \"");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join<Identifier>(", ", tags));
					defaultInterpolatedStringHandler.AppendLiteral("\", but none are suitable for level difficulty ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(levelDifficulty, "F1");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return null;
			}
			return matchingSubmarines[rand.Next(matchingSubmarines.Count)];
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x00070480 File Offset: 0x0006E680
		[CompilerGenerated]
		internal static void <GetReputationRewardText>g__AddReputationText|16_0(FactionPrefab factionPrefab, float amount, ref Mission.<>c__DisplayClass16_0 A_2)
		{
			if (factionPrefab == null)
			{
				return;
			}
			float totalReputationChange = amount;
			GameSession gameSession = GameMain.GameSession;
			Faction faction2;
			if (gameSession == null)
			{
				faction2 = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				faction2 = ((campaign != null) ? campaign.Factions.Find((Faction f) => f.Prefab == factionPrefab) : null);
			}
			Faction faction = faction2;
			if (faction != null)
			{
				totalReputationChange = amount * faction.Reputation.GetReputationChangeMultiplier(amount);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(factionPrefab.IconColor.ToStringHex());
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(factionPrefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
			LocalizedString name = defaultInterpolatedStringHandler.ToStringAndClear();
			float normalizedValue = MathUtils.InverseLerp(-100f, 100f, totalReputationChange);
			string formattedValue = ((int)Math.Round((double)totalReputationChange)).ToString("+#;-#;0");
			string tag = "reputationformat";
			ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
			array[0] = new ValueTuple<string, LocalizedString>("[reputationname]", name);
			int num = 1;
			string item = "[reputationvalue]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler2.AppendFormatted(Reputation.GetReputationColor(normalizedValue).ToStringHex());
			defaultInterpolatedStringHandler2.AppendLiteral("‖");
			defaultInterpolatedStringHandler2.AppendFormatted(formattedValue);
			defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
			array[num] = new ValueTuple<string, LocalizedString>(item, defaultInterpolatedStringHandler2.ToStringAndClear());
			LocalizedString rewardText = TextManager.GetWithVariables(tag, array);
			A_2.reputationRewardTexts.Add(rewardText.Value);
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x00070618 File Offset: 0x0006E818
		[CompilerGenerated]
		private void <DistributeExperienceToCrew>g__GiveMissionExperience|114_0(CharacterInfo info, ref Mission.<>c__DisplayClass114_0 A_2)
		{
			if (info == null)
			{
				return;
			}
			AbilityMissionExperienceGainMultiplier experienceGainMultiplierIndividual = new AbilityMissionExperienceGainMultiplier(this, 1f, info.Character);
			foreach (Character c in A_2.crew)
			{
				if (c != info.Character)
				{
					c.CheckTalents(AbilityEffectType.OnAllyGainMissionExperience, experienceGainMultiplierIndividual);
				}
			}
			Character character = info.Character;
			if (character != null)
			{
				character.CheckTalents(AbilityEffectType.OnGainMissionExperience, experienceGainMultiplierIndividual);
			}
			info.GiveExperience((int)((float)A_2.experienceGain * experienceGainMultiplierIndividual.Value));
		}

		// Token: 0x04000622 RID: 1570
		private readonly List<LocalizedString> shownMessages = new List<LocalizedString>();

		// Token: 0x04000623 RID: 1571
		public readonly MissionPrefab Prefab;

		// Token: 0x04000624 RID: 1572
		private bool completed;

		// Token: 0x04000625 RID: 1573
		protected bool failed;

		// Token: 0x04000626 RID: 1574
		protected Level level;

		// Token: 0x04000627 RID: 1575
		protected int state;

		// Token: 0x04000629 RID: 1577
		protected readonly CheckDataAction completeCheckDataAction;

		// Token: 0x0400062A RID: 1578
		public readonly ImmutableArray<LocalizedString> Headers;

		// Token: 0x0400062B RID: 1579
		public readonly ImmutableArray<LocalizedString> Messages;

		// Token: 0x0400062C RID: 1580
		private int? finalReward;

		// Token: 0x0400062D RID: 1581
		private readonly LocalizedString successMessage;

		// Token: 0x0400062E RID: 1582
		private readonly LocalizedString failureMessage;

		// Token: 0x0400062F RID: 1583
		protected LocalizedString description;

		// Token: 0x04000630 RID: 1584
		protected LocalizedString descriptionWithoutReward;

		// Token: 0x04000631 RID: 1585
		public bool ForceFailure;

		// Token: 0x04000632 RID: 1586
		public Location OriginLocation;

		// Token: 0x04000633 RID: 1587
		public readonly Location[] Locations;

		// Token: 0x04000634 RID: 1588
		private readonly List<Mission.DelayedTriggerEvent> delayedTriggerEvents = new List<Mission.DelayedTriggerEvent>();

		// Token: 0x04000635 RID: 1589
		public Action<Mission> OnMissionStateChanged;

		// Token: 0x04000636 RID: 1590
		protected readonly ContentXElement characterConfig;

		// Token: 0x04000637 RID: 1591
		protected readonly List<Character> characters = new List<Character>();

		// Token: 0x04000638 RID: 1592
		protected readonly Dictionary<Character, List<Item>> characterItems = new Dictionary<Character, List<Item>>();

		// Token: 0x020007DF RID: 2015
		private class DelayedTriggerEvent
		{
			// Token: 0x06006C09 RID: 27657 RVA: 0x0035E597 File Offset: 0x0035C797
			public DelayedTriggerEvent(MissionPrefab.TriggerEvent triggerEvent, float delay)
			{
				this.TriggerEvent = triggerEvent;
				this.Delay = delay;
			}

			// Token: 0x04003C03 RID: 15363
			public readonly MissionPrefab.TriggerEvent TriggerEvent;

			// Token: 0x04003C04 RID: 15364
			public float Delay;
		}
	}
}
