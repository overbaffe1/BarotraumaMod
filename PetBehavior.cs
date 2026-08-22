using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000194 RID: 404
	internal class PetBehavior
	{
		// Token: 0x17000C2D RID: 3117
		// (get) Token: 0x06002F5B RID: 12123 RVA: 0x001F5CD6 File Offset: 0x001F3ED6
		// (set) Token: 0x06002F5C RID: 12124 RVA: 0x001F5CDE File Offset: 0x001F3EDE
		public float Hunger
		{
			get
			{
				return this.hunger;
			}
			set
			{
				this.hunger = MathHelper.Clamp(value, 0f, this.MaxHunger);
			}
		}

		// Token: 0x17000C2E RID: 3118
		// (get) Token: 0x06002F5D RID: 12125 RVA: 0x001F5CF7 File Offset: 0x001F3EF7
		// (set) Token: 0x06002F5E RID: 12126 RVA: 0x001F5CFF File Offset: 0x001F3EFF
		public float Happiness
		{
			get
			{
				return this.happiness;
			}
			set
			{
				this.happiness = MathHelper.Clamp(value, 0f, this.MaxHappiness);
			}
		}

		// Token: 0x17000C2F RID: 3119
		// (get) Token: 0x06002F5F RID: 12127 RVA: 0x001F5D18 File Offset: 0x001F3F18
		// (set) Token: 0x06002F60 RID: 12128 RVA: 0x001F5D20 File Offset: 0x001F3F20
		public float UnhappyThreshold { get; set; }

		// Token: 0x17000C30 RID: 3120
		// (get) Token: 0x06002F61 RID: 12129 RVA: 0x001F5D29 File Offset: 0x001F3F29
		// (set) Token: 0x06002F62 RID: 12130 RVA: 0x001F5D31 File Offset: 0x001F3F31
		public float HappyThreshold { get; set; }

		// Token: 0x17000C31 RID: 3121
		// (get) Token: 0x06002F63 RID: 12131 RVA: 0x001F5D3A File Offset: 0x001F3F3A
		// (set) Token: 0x06002F64 RID: 12132 RVA: 0x001F5D42 File Offset: 0x001F3F42
		public float MaxHappiness { get; set; }

		// Token: 0x17000C32 RID: 3122
		// (get) Token: 0x06002F65 RID: 12133 RVA: 0x001F5D4B File Offset: 0x001F3F4B
		// (set) Token: 0x06002F66 RID: 12134 RVA: 0x001F5D53 File Offset: 0x001F3F53
		public bool HideStatusIndicators { get; set; }

		// Token: 0x17000C33 RID: 3123
		// (get) Token: 0x06002F67 RID: 12135 RVA: 0x001F5D5C File Offset: 0x001F3F5C
		// (set) Token: 0x06002F68 RID: 12136 RVA: 0x001F5D64 File Offset: 0x001F3F64
		public float HungryThreshold { get; set; }

		// Token: 0x17000C34 RID: 3124
		// (get) Token: 0x06002F69 RID: 12137 RVA: 0x001F5D6D File Offset: 0x001F3F6D
		// (set) Token: 0x06002F6A RID: 12138 RVA: 0x001F5D75 File Offset: 0x001F3F75
		public float MaxHunger { get; set; }

		// Token: 0x17000C35 RID: 3125
		// (get) Token: 0x06002F6B RID: 12139 RVA: 0x001F5D7E File Offset: 0x001F3F7E
		// (set) Token: 0x06002F6C RID: 12140 RVA: 0x001F5D86 File Offset: 0x001F3F86
		public float HappinessDecreaseRate { get; set; }

		// Token: 0x17000C36 RID: 3126
		// (get) Token: 0x06002F6D RID: 12141 RVA: 0x001F5D8F File Offset: 0x001F3F8F
		// (set) Token: 0x06002F6E RID: 12142 RVA: 0x001F5D97 File Offset: 0x001F3F97
		public float HungerIncreaseRate { get; set; }

		// Token: 0x17000C37 RID: 3127
		// (get) Token: 0x06002F6F RID: 12143 RVA: 0x001F5DA0 File Offset: 0x001F3FA0
		// (set) Token: 0x06002F70 RID: 12144 RVA: 0x001F5DA8 File Offset: 0x001F3FA8
		public float PlayForce { get; set; }

		// Token: 0x17000C38 RID: 3128
		// (get) Token: 0x06002F71 RID: 12145 RVA: 0x001F5DB1 File Offset: 0x001F3FB1
		// (set) Token: 0x06002F72 RID: 12146 RVA: 0x001F5DB9 File Offset: 0x001F3FB9
		public float PlayTimer { get; set; }

		// Token: 0x17000C39 RID: 3129
		// (get) Token: 0x06002F73 RID: 12147 RVA: 0x001F5DC2 File Offset: 0x001F3FC2
		// (set) Token: 0x06002F74 RID: 12148 RVA: 0x001F5DCA File Offset: 0x001F3FCA
		public float PlayCooldown { get; set; }

		// Token: 0x17000C3A RID: 3130
		// (get) Token: 0x06002F75 RID: 12149 RVA: 0x001F5DD3 File Offset: 0x001F3FD3
		// (set) Token: 0x06002F76 RID: 12150 RVA: 0x001F5DDB File Offset: 0x001F3FDB
		public bool ToggleOwner { get; set; }

		// Token: 0x17000C3B RID: 3131
		// (get) Token: 0x06002F77 RID: 12151 RVA: 0x001F5DE4 File Offset: 0x001F3FE4
		// (set) Token: 0x06002F78 RID: 12152 RVA: 0x001F5DEC File Offset: 0x001F3FEC
		private float? UnstunY { get; set; }

		// Token: 0x17000C3C RID: 3132
		// (get) Token: 0x06002F79 RID: 12153 RVA: 0x001F5DF5 File Offset: 0x001F3FF5
		// (set) Token: 0x06002F7A RID: 12154 RVA: 0x001F5DFD File Offset: 0x001F3FFD
		public EnemyAIController AIController { get; private set; }

		// Token: 0x17000C3D RID: 3133
		// (get) Token: 0x06002F7B RID: 12155 RVA: 0x001F5E06 File Offset: 0x001F4006
		// (set) Token: 0x06002F7C RID: 12156 RVA: 0x001F5E0E File Offset: 0x001F400E
		public Character Owner { get; set; }

		// Token: 0x06002F7D RID: 12157 RVA: 0x001F5E18 File Offset: 0x001F4018
		public PetBehavior(XElement element, EnemyAIController aiController)
		{
			this.AIController = aiController;
			this.MaxHappiness = element.GetAttributeFloat("MaxHappiness", 100f);
			this.UnhappyThreshold = element.GetAttributeFloat("UnhappyThreshold", this.MaxHappiness * 0.25f);
			this.HappyThreshold = element.GetAttributeFloat("HappyThreshold", this.MaxHappiness * 0.8f);
			this.HideStatusIndicators = element.GetAttributeBool("HideStatusIndicators", false);
			this.MaxHunger = element.GetAttributeFloat("MaxHunger", 100f);
			this.HungryThreshold = element.GetAttributeFloat("HungryThreshold", this.MaxHunger * 0.5f);
			this.Happiness = this.MaxHappiness * 0.5f;
			this.Hunger = this.MaxHunger * 0.5f;
			this.HappinessDecreaseRate = element.GetAttributeFloat("HappinessDecreaseRate", 0.1f);
			this.HungerIncreaseRate = element.GetAttributeFloat("HungerIncreaseRate", 0.25f);
			this.PlayForce = element.GetAttributeFloat("PlayForce", 15f);
			this.PlayCooldown = element.GetAttributeFloat("PlayCooldown", 5f);
			this.ToggleOwner = element.GetAttributeBool("ToggleOwner", false);
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.LocalName.ToLowerInvariant();
				if (!(a == "itemproduction"))
				{
					if (a == "eat")
					{
						PetBehavior.Food food = new PetBehavior.Food
						{
							Tag = subElement.GetAttributeIdentifier("tag", Identifier.Empty),
							Hunger = subElement.GetAttributeFloat("hunger", -1f),
							Happiness = subElement.GetAttributeFloat("happiness", 1f),
							Priority = subElement.GetAttributeFloat("priority", 100f),
							IgnoreContained = subElement.GetAttributeBool("ignorecontained", true)
						};
						string[] requiredHungerStr = subElement.GetAttributeString("requiredhunger", "0-100").Split('-', StringSplitOptions.None);
						food.HungerRange = new Vector2(0f, 100f);
						if (requiredHungerStr.Length >= 2)
						{
							float tempF;
							if (float.TryParse(requiredHungerStr[0], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
							{
								food.HungerRange.X = tempF;
							}
							if (float.TryParse(requiredHungerStr[1], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
							{
								food.HungerRange.Y = tempF;
							}
						}
						this.foods.Add(food);
					}
				}
				else
				{
					this.itemsToProduce.Add(new PetBehavior.ItemProduction(subElement));
				}
			}
			string str = "MicroInteraction:";
			GameSession gameSession = GameMain.GameSession;
			string text;
			if (gameSession == null)
			{
				text = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
			}
			GameAnalyticsManager.AddDesignEvent(str + (text ?? "null") + ":PetSpawned:" + aiController.Character.SpeciesName.ToString());
		}

		// Token: 0x06002F7E RID: 12158 RVA: 0x001F616C File Offset: 0x001F436C
		public PetBehavior.StatusIndicatorType GetCurrentStatusIndicatorType()
		{
			if (this.HideStatusIndicators)
			{
				return PetBehavior.StatusIndicatorType.None;
			}
			if (this.Hunger > this.HungryThreshold)
			{
				return PetBehavior.StatusIndicatorType.Hungry;
			}
			if (this.Happiness > this.HappyThreshold)
			{
				return PetBehavior.StatusIndicatorType.Happy;
			}
			if (this.Happiness < this.UnhappyThreshold)
			{
				return PetBehavior.StatusIndicatorType.Sad;
			}
			return PetBehavior.StatusIndicatorType.None;
		}

		// Token: 0x06002F7F RID: 12159 RVA: 0x001F61AC File Offset: 0x001F43AC
		public bool OnEat(Item item)
		{
			bool success = this.OnEat(item.GetTags());
			if (success)
			{
				string[] array = new string[6];
				array[0] = "MicroInteraction:";
				int num = 1;
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				array[num] = (text ?? "null");
				array[2] = ":PetEat:";
				array[3] = this.AIController.Character.SpeciesName.ToString();
				array[4] = ":";
				array[5] = item.Prefab.Identifier.ToString();
				GameAnalyticsManager.AddDesignEvent(string.Concat(array));
			}
			return success;
		}

		// Token: 0x06002F80 RID: 12160 RVA: 0x001F6264 File Offset: 0x001F4464
		public bool OnEat(Character character)
		{
			if (character == null || !character.IsDead)
			{
				return false;
			}
			bool success = this.OnEat("dead".ToIdentifier());
			if (success)
			{
				string[] array = new string[6];
				array[0] = "MicroInteraction:";
				int num = 1;
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				array[num] = (text ?? "null");
				array[2] = ":PetEat:";
				array[3] = this.AIController.Character.SpeciesName.ToString();
				array[4] = ":";
				array[5] = character.SpeciesName.ToString();
				GameAnalyticsManager.AddDesignEvent(string.Concat(array));
			}
			return success;
		}

		// Token: 0x06002F81 RID: 12161 RVA: 0x001F632C File Offset: 0x001F452C
		private bool OnEat(IEnumerable<Identifier> tags)
		{
			foreach (Identifier tag in tags)
			{
				if (this.OnEat(tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002F82 RID: 12162 RVA: 0x001F6380 File Offset: 0x001F4580
		public bool OnEat(Identifier tag)
		{
			for (int i = 0; i < this.foods.Count; i++)
			{
				if (tag == this.foods[i].Tag)
				{
					this.Hunger += this.foods[i].Hunger;
					this.Happiness += this.foods[i].Happiness;
					this.AIController.Character.PlaySound(CharacterSound.SoundType.Happy, 0.5f, 0f);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002F83 RID: 12163 RVA: 0x001F641A File Offset: 0x001F461A
		public bool CanPlayWith(Character player)
		{
			return this.AIController.Character.IsOnFriendlyTeam(player);
		}

		// Token: 0x06002F84 RID: 12164 RVA: 0x001F6430 File Offset: 0x001F4630
		public void Play(Character player)
		{
			if (this.PlayTimer > 0f)
			{
				return;
			}
			if (!this.CanPlayWith(player))
			{
				return;
			}
			if (this.ToggleOwner)
			{
				this.Owner = ((this.Owner == player) ? null : player);
			}
			else if (this.Owner == null)
			{
				this.Owner = player;
			}
			this.PlayTimer = this.PlayCooldown;
			this.AIController.Character.IsRagdolled = true;
			this.Happiness += 10f;
			this.AIController.Character.AnimController.MainLimb.body.LinearVelocity += new Vector2(0f, this.PlayForce);
			this.UnstunY = new float?(this.AIController.Character.SimPosition.Y);
			this.AIController.Character.PlaySound((this.Owner == null) ? CharacterSound.SoundType.Unhappy : CharacterSound.SoundType.Happy, 1f, 0f);
		}

		// Token: 0x06002F85 RID: 12165 RVA: 0x001F6534 File Offset: 0x001F4734
		public string GetTagName()
		{
			if (this.AIController.Character.Inventory != null)
			{
				foreach (Item item in this.AIController.Character.Inventory.AllItems)
				{
					NameTag tag = item.GetComponent<NameTag>();
					if (tag != null && !string.IsNullOrWhiteSpace(tag.WrittenName))
					{
						return tag.WrittenName;
					}
				}
			}
			return string.Empty;
		}

		// Token: 0x06002F86 RID: 12166 RVA: 0x001F65C4 File Offset: 0x001F47C4
		public void Update(float deltaTime)
		{
			Character character = this.AIController.Character;
			if (character == null || character.Removed)
			{
				return;
			}
			if (this.UnstunY != null)
			{
				if (this.PlayTimer > this.PlayCooldown - 1f)
				{
					float extent = character.AnimController.MainLimb.body.GetMaxExtent();
					if (character.SimPosition.Y < this.UnstunY.Value + extent * 3f && character.AnimController.MainLimb.body.LinearVelocity.Y < 0f)
					{
						character.IsRagdolled = false;
						this.UnstunY = null;
					}
					else
					{
						character.IsRagdolled = true;
					}
				}
				else
				{
					character.IsRagdolled = false;
					this.UnstunY = null;
				}
			}
			this.PlayTimer -= deltaTime;
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.Owner != null && (this.Owner.Removed || this.Owner.IsDead))
			{
				this.Owner = null;
			}
			this.Hunger += this.HungerIncreaseRate * deltaTime;
			this.Happiness -= this.HappinessDecreaseRate * deltaTime;
			for (int i = 0; i < this.foods.Count; i++)
			{
				PetBehavior.Food food = this.foods[i];
				if (this.Hunger >= food.HungerRange.X && this.Hunger <= food.HungerRange.Y)
				{
					if (food.TargetParams == null)
					{
						IEnumerable<CharacterParams.TargetParams> existingTargetParams;
						if (this.AIController.AIParams.TryGetTargets(food.Tag, out existingTargetParams))
						{
							using (IEnumerator<CharacterParams.TargetParams> enumerator = existingTargetParams.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									CharacterParams.TargetParams targetParams = enumerator.Current;
									food.TargetParams = targetParams;
								}
								goto IL_212;
							}
							goto IL_1E6;
						}
						goto IL_1E6;
						IL_212:
						if (food.TargetParams != null)
						{
							food.TargetParams.State = AIState.Eat;
							food.TargetParams.Priority = food.Priority;
							food.TargetParams.IgnoreContained = food.IgnoreContained;
							goto IL_279;
						}
						goto IL_279;
						IL_1E6:
						CharacterParams.TargetParams targetParams2;
						if (this.AIController.AIParams.TryAddNewTarget(food.Tag, AIState.Eat, food.Priority, out targetParams2))
						{
							food.TargetParams = targetParams2;
							goto IL_212;
						}
						goto IL_212;
					}
				}
				else if (food.TargetParams != null)
				{
					this.AIController.AIParams.RemoveTarget(food.TargetParams);
					food.TargetParams = null;
				}
				IL_279:;
			}
			if (this.Hunger >= this.MaxHunger * 0.99f)
			{
				character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, new Affliction(AfflictionPrefab.InternalDamage, 8f * deltaTime), true, false, true);
			}
			if (character.SelectedBy != null)
			{
				character.IsRagdolled = true;
				this.UnstunY = new float?(character.SimPosition.Y);
			}
			for (int j = 0; j < this.itemsToProduce.Count; j++)
			{
				this.itemsToProduce[j].Update(this, deltaTime);
			}
		}

		// Token: 0x06002F87 RID: 12167 RVA: 0x001F6900 File Offset: 0x001F4B00
		public static void SavePets(XElement petsElement)
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.IsPet && !c.IsDead && c.Submarine != null)
				{
					EnemyAIController enemyAIController = c.AIController as EnemyAIController;
					PetBehavior petBehavior = (enemyAIController != null) ? enemyAIController.PetBehavior : null;
					if (petBehavior != null && c.TeamID != CharacterTeamType.None && c.TeamID != CharacterTeamType.Team2 && c.Submarine != null)
					{
						Submarine submarine = c.Submarine;
						if (submarine != null)
						{
							SubmarineInfo info = submarine.Info;
							if (info != null && info.IsPlayer)
							{
								goto IL_B6;
							}
						}
						Character owner = petBehavior.Owner;
						if (owner == null || !owner.IsOnPlayerTeam)
						{
							continue;
						}
						IL_B6:
						XName name = "pet";
						object[] array = new object[3];
						array[0] = new XAttribute("speciesname", c.SpeciesName);
						int num = 1;
						XName name2 = "ownerhash";
						Character owner2 = petBehavior.Owner;
						int? num2;
						if (owner2 == null)
						{
							num2 = null;
						}
						else
						{
							CharacterInfo info2 = owner2.Info;
							num2 = ((info2 != null) ? new int?(info2.GetIdentifier()) : null);
						}
						int? num3 = num2;
						array[num] = new XAttribute(name2, num3.GetValueOrDefault());
						array[2] = new XAttribute("seed", c.Seed);
						XElement petElement = new XElement(name, array);
						XElement petBehaviorElement = new XElement("petbehavior", new object[]
						{
							new XAttribute("hunger", petBehavior.Hunger.ToString("G", CultureInfo.InvariantCulture)),
							new XAttribute("happiness", petBehavior.Happiness.ToString("G", CultureInfo.InvariantCulture))
						});
						petElement.Add(petBehaviorElement);
						XElement healthElement = new XElement("health");
						c.CharacterHealth.Save(healthElement);
						petElement.Add(healthElement);
						if (c.Inventory != null)
						{
							XElement inventoryElement = new XElement("inventory");
							Character.SaveInventory(c.Inventory, inventoryElement);
							petElement.Add(inventoryElement);
						}
						petsElement.Add(petElement);
					}
				}
			}
		}

		// Token: 0x06002F88 RID: 12168 RVA: 0x001F6B6C File Offset: 0x001F4D6C
		public static void LoadPets(XElement petsElement)
		{
			foreach (XElement subElement in petsElement.Elements())
			{
				string speciesName = subElement.GetAttributeString("speciesname", "");
				string seed = subElement.GetAttributeString("seed", "123");
				int ownerHash = subElement.GetAttributeInt("ownerhash", 0);
				Vector2 spawnPos = Vector2.Zero;
				Character owner = Character.CharacterList.Find(delegate(Character c)
				{
					CharacterInfo info = c.Info;
					return info != null && info.GetIdentifier() == ownerHash;
				});
				if (owner == null)
				{
					goto IL_A1;
				}
				Submarine submarine = owner.Submarine;
				if (submarine == null || submarine.Info.Type != SubmarineType.Player)
				{
					goto IL_A1;
				}
				spawnPos = owner.WorldPosition;
				IL_133:
				CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName.ToIdentifier());
				if (characterPrefab == null)
				{
					DebugConsole.ThrowError("Failed to load the pet \"" + speciesName + "\". Character prefab not found.", null, null, false, false);
					continue;
				}
				Character pet = Character.Create(characterPrefab, spawnPos, seed, null, 0, false, true, true, null, false);
				if (pet != null)
				{
					EnemyAIController enemyAIController = pet.AIController as EnemyAIController;
					PetBehavior petBehavior = (enemyAIController != null) ? enemyAIController.PetBehavior : null;
					if (petBehavior != null)
					{
						petBehavior.Owner = owner;
						XElement petBehaviorElement = subElement.Element("petbehavior");
						if (petBehaviorElement != null)
						{
							petBehavior.Hunger = petBehaviorElement.GetAttributeFloat("hunger", 50f);
							petBehavior.Happiness = petBehaviorElement.GetAttributeFloat("happiness", 50f);
						}
					}
				}
				XElement inventoryElement = subElement.Element("inventory");
				if (inventoryElement != null)
				{
					pet.SpawnInventoryItems(pet.Inventory, inventoryElement.FromPackage(null));
					continue;
				}
				continue;
				IL_A1:
				WayPoint spawnPoint = null;
				if (Submarine.MainSub != null)
				{
					spawnPoint = (from wp in WayPoint.WayPointList
					where wp.SpawnType == SpawnType.Human && wp.Submarine == Submarine.MainSub
					select wp).GetRandomUnsynced<WayPoint>();
				}
				if (spawnPoint == null)
				{
					spawnPoint = WayPoint.WayPointList.Where(delegate(WayPoint wp)
					{
						if (wp.SpawnType == SpawnType.Human)
						{
							Submarine submarine2 = wp.Submarine;
							return submarine2 != null && submarine2.Info.Type == SubmarineType.Player;
						}
						return false;
					}).GetRandomUnsynced<WayPoint>();
				}
				Vector2 vector;
				if (spawnPoint == null)
				{
					Submarine mainSub = Submarine.MainSub;
					vector = ((mainSub != null) ? mainSub.WorldPosition : Vector2.Zero);
				}
				else
				{
					vector = spawnPoint.WorldPosition;
				}
				spawnPos = vector;
				goto IL_133;
			}
		}

		// Token: 0x040018AE RID: 6318
		private float hunger = 50f;

		// Token: 0x040018AF RID: 6319
		private float happiness = 50f;

		// Token: 0x040018BF RID: 6335
		private readonly List<PetBehavior.ItemProduction> itemsToProduce = new List<PetBehavior.ItemProduction>();

		// Token: 0x040018C0 RID: 6336
		private readonly List<PetBehavior.Food> foods = new List<PetBehavior.Food>();

		// Token: 0x02000E85 RID: 3717
		public enum StatusIndicatorType
		{
			// Token: 0x0400526B RID: 21099
			None,
			// Token: 0x0400526C RID: 21100
			Happy,
			// Token: 0x0400526D RID: 21101
			Sad,
			// Token: 0x0400526E RID: 21102
			Hungry
		}

		// Token: 0x02000E86 RID: 3718
		private class ItemProduction
		{
			// Token: 0x0600848D RID: 33933 RVA: 0x0039FBD4 File Offset: 0x0039DDD4
			public ItemProduction(XElement element)
			{
				this.Items = new List<PetBehavior.ItemProduction.Item>();
				this.HungerRate = element.GetAttributeFloat("hungerrate", 0f);
				this.InvHungerRate = element.GetAttributeFloat("invhungerrate", 0f);
				this.HappinessRate = element.GetAttributeFloat("happinessrate", 0f);
				this.InvHappinessRate = element.GetAttributeFloat("invhappinessrate", 0f);
				string[] requiredHappinessStr = element.GetAttributeString("requiredhappiness", "0-100").Split('-', StringSplitOptions.None);
				string[] requiredHungerStr = element.GetAttributeString("requiredhunger", "0-100").Split('-', StringSplitOptions.None);
				this.HappinessRange = new Vector2(0f, 100f);
				this.HungerRange = new Vector2(0f, 100f);
				if (requiredHappinessStr.Length >= 2)
				{
					float tempF;
					if (float.TryParse(requiredHappinessStr[0], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
					{
						this.HappinessRange.X = tempF;
					}
					if (float.TryParse(requiredHappinessStr[1], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
					{
						this.HappinessRange.Y = tempF;
					}
				}
				if (requiredHungerStr.Length >= 2)
				{
					float tempF;
					if (float.TryParse(requiredHungerStr[0], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
					{
						this.HungerRange.X = tempF;
					}
					if (float.TryParse(requiredHungerStr[1], NumberStyles.Any, CultureInfo.InvariantCulture, out tempF))
					{
						this.HungerRange.Y = tempF;
					}
				}
				this.Rate = element.GetAttributeFloat("rate", 0.016f);
				this.totalCommonness = 0f;
				foreach (XElement subElement in element.Elements())
				{
					string a = subElement.Name.LocalName.ToLowerInvariant();
					if (a == "item")
					{
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						PetBehavior.ItemProduction.Item newItemToProduce = new PetBehavior.ItemProduction.Item
						{
							Prefab = (identifier.IsEmpty ? null : ItemPrefab.Find("", subElement.GetAttributeIdentifier("identifier", Identifier.Empty))),
							Commonness = subElement.GetAttributeFloat("commonness", 0f)
						};
						this.totalCommonness += newItemToProduce.Commonness;
						this.Items.Add(newItemToProduce);
					}
				}
				this.timer = 1f;
			}

			// Token: 0x0600848E RID: 33934 RVA: 0x0039FE50 File Offset: 0x0039E050
			public void Update(PetBehavior pet, float deltaTime)
			{
				if (pet.Happiness < this.HappinessRange.X || pet.Happiness > this.HappinessRange.Y)
				{
					return;
				}
				if (pet.Hunger < this.HungerRange.X || pet.Hunger > this.HungerRange.Y)
				{
					return;
				}
				float currentRate = this.Rate;
				currentRate += this.HappinessRate * (pet.Happiness - this.HappinessRange.X) / (this.HappinessRange.Y - this.HappinessRange.X);
				currentRate += this.InvHappinessRate * (1f - (pet.Happiness - this.HappinessRange.X) / (this.HappinessRange.Y - this.HappinessRange.X));
				currentRate += this.HungerRate * (pet.Hunger - this.HungerRange.X) / (this.HungerRange.Y - this.HungerRange.X);
				currentRate += this.InvHungerRate * (1f - (pet.Hunger - this.HungerRange.X) / (this.HungerRange.Y - this.HungerRange.X));
				this.timer -= currentRate * deltaTime;
				if (this.timer <= 0f)
				{
					this.timer = 1f;
					float r = Rand.Range(0f, this.totalCommonness, Rand.RandSync.Unsynced);
					float aggregate = 0f;
					int i = 0;
					while (i < this.Items.Count)
					{
						aggregate += this.Items[i].Commonness;
						if (aggregate >= r && this.Items[i].Prefab != null)
						{
							EntitySpawner spawner = Entity.Spawner;
							if (spawner == null)
							{
								return;
							}
							spawner.AddItemToSpawnQueue(this.Items[i].Prefab, pet.AIController.Character.WorldPosition, null, null, null);
							return;
						}
						else
						{
							i++;
						}
					}
				}
			}

			// Token: 0x0400526F RID: 21103
			public List<PetBehavior.ItemProduction.Item> Items;

			// Token: 0x04005270 RID: 21104
			public Vector2 HungerRange;

			// Token: 0x04005271 RID: 21105
			public Vector2 HappinessRange;

			// Token: 0x04005272 RID: 21106
			public float Rate;

			// Token: 0x04005273 RID: 21107
			public float HungerRate;

			// Token: 0x04005274 RID: 21108
			public float InvHungerRate;

			// Token: 0x04005275 RID: 21109
			public float HappinessRate;

			// Token: 0x04005276 RID: 21110
			public float InvHappinessRate;

			// Token: 0x04005277 RID: 21111
			private readonly float totalCommonness;

			// Token: 0x04005278 RID: 21112
			private float timer;

			// Token: 0x02001552 RID: 5458
			public struct Item
			{
				// Token: 0x0400681A RID: 26650
				public ItemPrefab Prefab;

				// Token: 0x0400681B RID: 26651
				public float Commonness;
			}
		}

		// Token: 0x02000E87 RID: 3719
		private class Food
		{
			// Token: 0x04005279 RID: 21113
			public Identifier Tag;

			// Token: 0x0400527A RID: 21114
			public Vector2 HungerRange;

			// Token: 0x0400527B RID: 21115
			public float Hunger;

			// Token: 0x0400527C RID: 21116
			public float Happiness;

			// Token: 0x0400527D RID: 21117
			public float Priority;

			// Token: 0x0400527E RID: 21118
			public bool IgnoreContained;

			// Token: 0x0400527F RID: 21119
			public CharacterParams.TargetParams TargetParams;
		}
	}
}
