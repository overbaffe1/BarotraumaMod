using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000025 RID: 37
	internal class EscortMission : Mission
	{
		// Token: 0x06000493 RID: 1171 RVA: 0x0002861C File Offset: 0x0002681C
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			if (this.characters.Count == 0)
			{
				throw new InvalidOperationException("Server attempted to write escort mission data when no characters had been spawned.");
			}
			msg.WriteByte((byte)this.characters.Count);
			foreach (Character character in this.characters)
			{
				character.WriteSpawnData(msg, character.ID, false);
				msg.WriteBoolean(this.terroristCharacters.Contains(character));
				msg.WriteUInt16((ushort)this.characterItems[character].Count<Item>());
				foreach (Item item in this.characterItems[character])
				{
					Item item2 = item;
					ushort id = item.ID;
					Inventory parentInventory = item.ParentInventory;
					ushort? num;
					if (parentInventory == null)
					{
						num = null;
					}
					else
					{
						Entity owner = parentInventory.Owner;
						num = ((owner != null) ? new ushort?(owner.ID) : null);
					}
					ushort? num2 = num;
					ushort valueOrDefault = num2.GetValueOrDefault();
					byte originalItemContainerIndex = 0;
					Inventory parentInventory2 = item.ParentInventory;
					item2.WriteSpawnData(msg, id, valueOrDefault, originalItemContainerIndex, (parentInventory2 != null) ? parentInventory2.FindIndex(item) : -1);
				}
			}
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x00028778 File Offset: 0x00026978
		public EscortMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.missionSub = sub;
			this.baseEscortedCharacters = prefab.ConfigElement.GetAttributeInt("baseescortedcharacters", 1);
			this.scalingEscortedCharacters = prefab.ConfigElement.GetAttributeFloat("scalingescortedcharacters", 0f);
			this.terroristChance = prefab.ConfigElement.GetAttributeFloat("terroristchance", 0f);
			this.terroristItemConfig = prefab.ConfigElement.GetChildElement("TerroristItems");
			this.terroristAnnounceDialogTag = prefab.ConfigElement.GetAttributeString("dialogterroristannounce", prefab.ConfigElement.GetAttributeString("terroristAnnounceDialogTag", string.Empty));
			this.CalculateReward();
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00028840 File Offset: 0x00026A40
		private void CalculateReward()
		{
			if (this.missionSub == null)
			{
				this.calculatedReward = this.Prefab.Reward;
				return;
			}
			int multiplier = 1;
			this.calculatedReward = this.Prefab.Reward * multiplier;
			string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", base.GetReward(this.missionSub)) + "‖end‖";
			if (this.descriptionWithoutReward != null)
			{
				this.description = this.descriptionWithoutReward.Replace("[reward]", rewardText, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x000288D7 File Offset: 0x00026AD7
		public override float GetBaseReward(Submarine sub)
		{
			if (sub != this.missionSub)
			{
				this.missionSub = sub;
				this.CalculateReward();
			}
			return (float)this.calculatedReward;
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x000288F8 File Offset: 0x00026AF8
		private int CalculateScalingEscortedCharacterCount(bool inMission = false)
		{
			if (this.missionSub == null || this.missionSub.Info == null)
			{
				if (inMission)
				{
					DebugConsole.ThrowError("MainSub was null when trying to retrieve submarine size for determining escorted character count!", null, this.Prefab.ContentPackage, false, false);
				}
				return 1;
			}
			return (int)Math.Round((double)((float)this.baseEscortedCharacters + this.scalingEscortedCharacters * (float)(this.missionSub.Info.RecommendedCrewSizeMin + this.missionSub.Info.RecommendedCrewSizeMax) / 2f));
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00028978 File Offset: 0x00026B78
		private void InitEscort()
		{
			this.characters.Clear();
			this.characterItems.Clear();
			WayPoint explicitStayInHullPos = WayPoint.GetRandom(SpawnType.Human, null, Submarine.MainSub, false, null, false);
			Rand.RandSync randSync = Rand.RandSync.ServerAndClient;
			if (this.terroristChance > 0f)
			{
				randSync = Rand.RandSync.Unsynced;
			}
			List<ValueTuple<HumanPrefab, List<StatusEffect>>> humanPrefabsToSpawn = new List<ValueTuple<HumanPrefab, List<StatusEffect>>>();
			foreach (ContentXElement characterElement in this.characterConfig.Elements())
			{
				int count = this.CalculateScalingEscortedCharacterCount(true);
				HumanPrefab humanPrefab = base.GetHumanPrefabFromElement(characterElement);
				for (int i = 0; i < count; i++)
				{
					List<StatusEffect> characterStatusEffects = new List<StatusEffect>();
					foreach (ContentXElement element in characterElement.Elements())
					{
						Identifier identifier = element.NameAsIdentifier();
						if (identifier == "statuseffect")
						{
							StatusEffect newEffect = StatusEffect.Load(element, this.Prefab.Name.Value);
							if (newEffect != null)
							{
								characterStatusEffects.Add(newEffect);
							}
						}
					}
					humanPrefabsToSpawn.Add(new ValueTuple<HumanPrefab, List<StatusEffect>>(humanPrefab, characterStatusEffects));
				}
			}
			foreach (ValueTuple<HumanPrefab, List<StatusEffect>> valueTuple in humanPrefabsToSpawn)
			{
				HumanPrefab humanPrefab2 = valueTuple.Item1;
				List<StatusEffect> statusEffectList = valueTuple.Item2;
				if (humanPrefab2 != null)
				{
					Identifier identifier = humanPrefab2.Job;
					if (!identifier.IsEmpty)
					{
						identifier = humanPrefab2.Job;
						if (!(identifier == "any"))
						{
							JobPrefab jobPrefab = humanPrefab2.GetJobPrefab(randSync, null);
							if (jobPrefab != null)
							{
								WayPoint jobSpecificSpawnPos = WayPoint.GetRandom(SpawnType.Human, jobPrefab, Submarine.MainSub, false, null, false);
								if (jobSpecificSpawnPos != null)
								{
									explicitStayInHullPos = jobSpecificSpawnPos;
									break;
								}
							}
						}
					}
				}
			}
			foreach (ValueTuple<HumanPrefab, List<StatusEffect>> valueTuple2 in humanPrefabsToSpawn)
			{
				HumanPrefab humanPrefab3 = valueTuple2.Item1;
				List<StatusEffect> statusEffectList2 = valueTuple2.Item2;
				Character spawnedCharacter = Mission.CreateHuman(humanPrefab3, this.characters, this.characterItems, Submarine.MainSub, CharacterTeamType.FriendlyNPC, explicitStayInHullPos, randSync);
				HumanAIController humanAI = spawnedCharacter.AIController as HumanAIController;
				if (humanAI != null)
				{
					humanAI.InitMentalStateManager();
				}
				foreach (StatusEffect statusEffect in statusEffectList2)
				{
					statusEffect.Apply(statusEffect.type, 1f, spawnedCharacter, spawnedCharacter, null);
				}
			}
			if (this.terroristChance > 0f)
			{
				int terroristCount = (int)Math.Ceiling((double)(this.terroristChance * Rand.Range(0.8f, 1.2f, Rand.RandSync.Unsynced) * (float)this.characters.Count));
				terroristCount = Math.Clamp(terroristCount, 1, this.characters.Count);
				this.terroristCharacters.Clear();
				this.characters.GetRange(0, terroristCount).ForEach(delegate(Character c)
				{
					this.terroristCharacters.Add(c);
				});
				this.terroristCharacters.ForEach(delegate(Character c)
				{
					c.IsHostileEscortee = true;
				});
				this.terroristDistanceSquared = Vector2.DistanceSquared(Level.Loaded.StartPosition, Level.Loaded.EndPosition) * Rand.Range(0.35f, 0.65f, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00028D14 File Offset: 0x00026F14
		private void InitCharacters()
		{
			int scalingCharacterCount = this.CalculateScalingEscortedCharacterCount(true);
			if (scalingCharacterCount * this.characterConfig.Elements().Count<ContentXElement>() != this.characters.Count)
			{
				DebugConsole.AddWarning("Character count did not match expected character count in InitCharacters of EscortMission", this.Prefab.ContentPackage);
				return;
			}
			int i = 0;
			foreach (ContentXElement element in this.characterConfig.Elements())
			{
				string escortIdentifier = element.GetAttributeString("escortidentifier", string.Empty);
				for (int j = 0; j < scalingCharacterCount; j++)
				{
					this.characters[j + i].IsEscorted = true;
					if (escortIdentifier != string.Empty && escortIdentifier == "vip")
					{
						this.vipCharacter = this.characters[j + i];
					}
					Character character = this.characters[j + i];
					ContentXElement contentXElement = element;
					string key = "color";
					Color lightGreen = Color.LightGreen;
					character.UniqueNameColor = new Color?(contentXElement.GetAttributeColor(key, lightGreen));
				}
				i++;
			}
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00028E40 File Offset: 0x00027040
		protected override void StartMissionSpecific(Level level)
		{
			if (this.characters.Count > 0)
			{
				DebugConsole.AddWarning("Character list was not empty at the start of a escort mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.characters.Clear();
			}
			ContentXElement contentXElement = null;
			if (this.characterConfig == contentXElement)
			{
				DebugConsole.ThrowError("Failed to initialize characters for escort mission (characterConfig == null)", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			if (this.missionSub == null)
			{
				this.missionSub = Submarine.MainSub;
				this.CalculateReward();
			}
			if (!Mission.IsClient)
			{
				this.InitEscort();
				this.InitCharacters();
			}
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00028EC8 File Offset: 0x000270C8
		private void TryToTriggerTerrorists()
		{
			if (this.terroristsShouldAct)
			{
				using (List<Character>.Enumerator enumerator = this.terroristCharacters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character character = enumerator.Current;
						character.IsHostileEscortee = true;
						if (!character.HasTeamChange("terrorist") && EscortMission.IsAlive(character) && !character.IsIncapacitated && !character.LockHands)
						{
							character.TryAddNewTeamChange("terrorist", new ActiveTeamChange(CharacterTeamType.None, ActiveTeamChange.TeamChangePriorities.Willful, true));
							if (!string.IsNullOrEmpty(this.terroristAnnounceDialogTag))
							{
								Character character3 = character;
								string value = TextManager.Get(this.terroristAnnounceDialogTag).Value;
								ChatMessageType? messageType = null;
								float delay = Rand.Range(0.5f, 3f, Rand.RandSync.Unsynced);
								Identifier identifier = default(Identifier);
								character3.Speak(value, messageType, delay, identifier, 0f);
							}
							foreach (ContentXElement itemElement in this.terroristItemConfig.Elements())
							{
								EscortMission.<>c__DisplayClass21_0 CS$<>8__locals1 = new EscortMission.<>c__DisplayClass21_0();
								EscortMission.<>c__DisplayClass21_0 CS$<>8__locals2 = CS$<>8__locals1;
								Level loaded = Level.Loaded;
								CS$<>8__locals2.levelDifficulty = ((loaded != null) ? loaded.Difficulty : 0f);
								ContentXElement selectedItemElement = itemElement;
								Identifier identifier = itemElement.NameAsIdentifier();
								Identifier identifier2 = "chooserandom".ToIdentifier();
								if (identifier == identifier2)
								{
									selectedItemElement = itemElement.Elements().GetRandomUnsynced((ContentXElement e) => e.GetAttributeFloat(0f, new string[]
									{
										"mindifficulty"
									}) <= CS$<>8__locals1.levelDifficulty);
								}
								ContentXElement contentXElement = null;
								if (selectedItemElement != contentXElement && CS$<>8__locals1.levelDifficulty >= selectedItemElement.GetAttributeFloat(0f, new string[]
								{
									"mindifficulty"
								}))
								{
									HumanPrefab.InitializeItem(character, selectedItemElement, character.Submarine, null, null, null, true);
								}
							}
						}
					}
					return;
				}
			}
			if (Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, Level.Loaded.EndPosition) < this.terroristDistanceSquared)
			{
				foreach (Character character2 in this.terroristCharacters)
				{
					HumanAIController humanAI = character2.AIController as HumanAIController;
					if (humanAI != null)
					{
						humanAI.ObjectiveManager.AddObjective<AIObjectiveEscapeHandcuffs>(new AIObjectiveEscapeHandcuffs(character2, humanAI.ObjectiveManager, false, true, 1f));
					}
				}
				this.terroristsShouldAct = true;
			}
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00029164 File Offset: 0x00027364
		private bool NonTerroristsStillAlive(IEnumerable<Character> characterList)
		{
			return characterList.All((Character c) => this.terroristCharacters.Contains(c) || EscortMission.IsAlive(c));
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00029178 File Offset: 0x00027378
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (!Mission.IsClient)
			{
				int newState = this.State;
				this.TryToTriggerTerrorists();
				switch (this.State)
				{
				case 0:
					if (!this.NonTerroristsStillAlive(this.characters))
					{
						newState = 1;
					}
					if (this.terroristCharacters.Any<Character>())
					{
						if (this.terroristCharacters.All((Character c) => !EscortMission.IsAlive(c)))
						{
							newState = 2;
						}
					}
					break;
				case 2:
					if (!this.NonTerroristsStillAlive(this.characters))
					{
						newState = 1;
					}
					break;
				}
				this.State = newState;
			}
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0002921C File Offset: 0x0002741C
		private static bool Survived(Character character)
		{
			if (EscortMission.IsAlive(character))
			{
				Hull currentHull = character.CurrentHull;
				if (((currentHull != null) ? currentHull.Submarine : null) != null)
				{
					return character.CurrentHull.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(character.CurrentHull.Submarine);
				}
			}
			return false;
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x00029275 File Offset: 0x00027475
		private static bool IsAlive(Character character)
		{
			return character != null && !character.Removed && !character.IsDead;
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00029290 File Offset: 0x00027490
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			if (Submarine.MainSub != null && Submarine.MainSub.AtEndExit)
			{
				bool friendliesSurvived = this.characters.Except(this.terroristCharacters).All((Character c) => EscortMission.Survived(c));
				bool vipDied = false;
				if (this.vipCharacter != null)
				{
					vipDied = !EscortMission.Survived(this.vipCharacter);
				}
				if (friendliesSurvived && !vipDied)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00029308 File Offset: 0x00027508
		protected override void EndMissionSpecific(bool completed)
		{
			if (!Mission.IsClient)
			{
				foreach (Character character in this.characters)
				{
					if (character.Inventory != null)
					{
						using (IEnumerator<Item> enumerator2 = character.Inventory.AllItemsMod.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								Item item = enumerator2.Current;
								if (!this.characterItems.Any((KeyValuePair<Character, List<Item>> c) => c.Value.Contains(item)))
								{
									item.Drop(character, true, true);
								}
							}
						}
					}
				}
				foreach (KeyValuePair<Character, List<Item>> characterItem in this.characterItems)
				{
					if (EscortMission.Survived(characterItem.Key) || !completed)
					{
						foreach (Item item2 in characterItem.Value)
						{
							if (!item2.Removed)
							{
								item2.Remove();
							}
						}
					}
				}
			}
			this.characters.Clear();
			this.characterItems.Clear();
			this.failed = !completed;
		}

		// Token: 0x0400024F RID: 591
		private readonly ContentXElement terroristItemConfig;

		// Token: 0x04000250 RID: 592
		private readonly Dictionary<HumanPrefab, List<StatusEffect>> characterStatusEffects = new Dictionary<HumanPrefab, List<StatusEffect>>();

		// Token: 0x04000251 RID: 593
		private readonly int baseEscortedCharacters;

		// Token: 0x04000252 RID: 594
		private readonly float scalingEscortedCharacters;

		// Token: 0x04000253 RID: 595
		private readonly float terroristChance;

		// Token: 0x04000254 RID: 596
		private readonly string terroristAnnounceDialogTag;

		// Token: 0x04000255 RID: 597
		private int calculatedReward;

		// Token: 0x04000256 RID: 598
		private Submarine missionSub;

		// Token: 0x04000257 RID: 599
		private Character vipCharacter;

		// Token: 0x04000258 RID: 600
		private readonly List<Character> terroristCharacters = new List<Character>();

		// Token: 0x04000259 RID: 601
		private bool terroristsShouldAct;

		// Token: 0x0400025A RID: 602
		private float terroristDistanceSquared;

		// Token: 0x0400025B RID: 603
		private const string TerroristTeamChangeIdentifier = "terrorist";
	}
}
