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
	// Token: 0x0200008E RID: 142
	internal class PetBehavior
	{
		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x06001253 RID: 4691 RVA: 0x000A2E3E File Offset: 0x000A103E
		// (set) Token: 0x06001254 RID: 4692 RVA: 0x000A2E46 File Offset: 0x000A1046
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

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x06001255 RID: 4693 RVA: 0x000A2E5F File Offset: 0x000A105F
		// (set) Token: 0x06001256 RID: 4694 RVA: 0x000A2E67 File Offset: 0x000A1067
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

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x06001257 RID: 4695 RVA: 0x000A2E80 File Offset: 0x000A1080
		// (set) Token: 0x06001258 RID: 4696 RVA: 0x000A2E88 File Offset: 0x000A1088
		public float UnhappyThreshold { get; set; }

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x06001259 RID: 4697 RVA: 0x000A2E91 File Offset: 0x000A1091
		// (set) Token: 0x0600125A RID: 4698 RVA: 0x000A2E99 File Offset: 0x000A1099
		public float HappyThreshold { get; set; }

		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x0600125B RID: 4699 RVA: 0x000A2EA2 File Offset: 0x000A10A2
		// (set) Token: 0x0600125C RID: 4700 RVA: 0x000A2EAA File Offset: 0x000A10AA
		public float MaxHappiness { get; set; }

		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x0600125D RID: 4701 RVA: 0x000A2EB3 File Offset: 0x000A10B3
		// (set) Token: 0x0600125E RID: 4702 RVA: 0x000A2EBB File Offset: 0x000A10BB
		public bool HideStatusIndicators { get; set; }

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x0600125F RID: 4703 RVA: 0x000A2EC4 File Offset: 0x000A10C4
		// (set) Token: 0x06001260 RID: 4704 RVA: 0x000A2ECC File Offset: 0x000A10CC
		public float HungryThreshold { get; set; }

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x06001261 RID: 4705 RVA: 0x000A2ED5 File Offset: 0x000A10D5
		// (set) Token: 0x06001262 RID: 4706 RVA: 0x000A2EDD File Offset: 0x000A10DD
		public float MaxHunger { get; set; }

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x06001263 RID: 4707 RVA: 0x000A2EE6 File Offset: 0x000A10E6
		// (set) Token: 0x06001264 RID: 4708 RVA: 0x000A2EEE File Offset: 0x000A10EE
		public float HappinessDecreaseRate { get; set; }

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06001265 RID: 4709 RVA: 0x000A2EF7 File Offset: 0x000A10F7
		// (set) Token: 0x06001266 RID: 4710 RVA: 0x000A2EFF File Offset: 0x000A10FF
		public float HungerIncreaseRate { get; set; }

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x06001267 RID: 4711 RVA: 0x000A2F08 File Offset: 0x000A1108
		// (set) Token: 0x06001268 RID: 4712 RVA: 0x000A2F10 File Offset: 0x000A1110
		public float PlayForce { get; set; }

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x06001269 RID: 4713 RVA: 0x000A2F19 File Offset: 0x000A1119
		// (set) Token: 0x0600126A RID: 4714 RVA: 0x000A2F21 File Offset: 0x000A1121
		public float PlayTimer { get; set; }

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x0600126B RID: 4715 RVA: 0x000A2F2A File Offset: 0x000A112A
		// (set) Token: 0x0600126C RID: 4716 RVA: 0x000A2F32 File Offset: 0x000A1132
		public float PlayCooldown { get; set; }

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x0600126D RID: 4717 RVA: 0x000A2F3B File Offset: 0x000A113B
		// (set) Token: 0x0600126E RID: 4718 RVA: 0x000A2F43 File Offset: 0x000A1143
		public bool ToggleOwner { get; set; }

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x0600126F RID: 4719 RVA: 0x000A2F4C File Offset: 0x000A114C
		// (set) Token: 0x06001270 RID: 4720 RVA: 0x000A2F54 File Offset: 0x000A1154
		private float? UnstunY { get; set; }

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x06001271 RID: 4721 RVA: 0x000A2F5D File Offset: 0x000A115D
		// (set) Token: 0x06001272 RID: 4722 RVA: 0x000A2F65 File Offset: 0x000A1165
		public EnemyAIController AIController { get; private set; }

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06001273 RID: 4723 RVA: 0x000A2F6E File Offset: 0x000A116E
		// (set) Token: 0x06001274 RID: 4724 RVA: 0x000A2F76 File Offset: 0x000A1176
		public Character Owner { get; set; }

		// Token: 0x06001275 RID: 4725 RVA: 0x000A2F80 File Offset: 0x000A1180
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

		// Token: 0x06001276 RID: 4726 RVA: 0x000A32D4 File Offset: 0x000A14D4
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

		// Token: 0x06001277 RID: 4727 RVA: 0x000A3314 File Offset: 0x000A1514
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

		// Token: 0x06001278 RID: 4728 RVA: 0x000A33CC File Offset: 0x000A15CC
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

		// Token: 0x06001279 RID: 4729 RVA: 0x000A3494 File Offset: 0x000A1694
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

		// Token: 0x0600127A RID: 4730 RVA: 0x000A34E8 File Offset: 0x000A16E8
		public bool OnEat(Identifier tag)
		{
			for (int i = 0; i < this.foods.Count; i++)
			{
				if (tag == this.foods[i].Tag)
				{
					this.Hunger += this.foods[i].Hunger;
					this.Happiness += this.foods[i].Happiness;
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600127B RID: 4731 RVA: 0x000A3564 File Offset: 0x000A1764
		public bool CanPlayWith(Character player)
		{
			return this.AIController.Character.IsOnFriendlyTeam(player);
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x000A3578 File Offset: 0x000A1778
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
		}

		// Token: 0x0600127D RID: 4733 RVA: 0x000A3654 File Offset: 0x000A1854
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

		// Token: 0x0600127E RID: 4734 RVA: 0x000A36E4 File Offset: 0x000A18E4
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

		// Token: 0x0600127F RID: 4735 RVA: 0x000A3A20 File Offset: 0x000A1C20
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

		// Token: 0x06001280 RID: 4736 RVA: 0x000A3C8C File Offset: 0x000A1E8C
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

		// Token: 0x040008B4 RID: 2228
		private float hunger = 50f;

		// Token: 0x040008B5 RID: 2229
		private float happiness = 50f;

		// Token: 0x040008C5 RID: 2245
		private readonly List<PetBehavior.ItemProduction> itemsToProduce = new List<PetBehavior.ItemProduction>();

		// Token: 0x040008C6 RID: 2246
		private readonly List<PetBehavior.Food> foods = new List<PetBehavior.Food>();

		// Token: 0x0200083D RID: 2109
		public enum StatusIndicatorType
		{
			// Token: 0x04002F0B RID: 12043
			None,
			// Token: 0x04002F0C RID: 12044
			Happy,
			// Token: 0x04002F0D RID: 12045
			Sad,
			// Token: 0x04002F0E RID: 12046
			Hungry
		}

		// Token: 0x0200083E RID: 2110
		private class ItemProduction
		{
			// Token: 0x06005451 RID: 21585 RVA: 0x001F02B0 File Offset: 0x001EE4B0
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

			// Token: 0x06005452 RID: 21586 RVA: 0x001F052C File Offset: 0x001EE72C
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

			// Token: 0x04002F0F RID: 12047
			public List<PetBehavior.ItemProduction.Item> Items;

			// Token: 0x04002F10 RID: 12048
			public Vector2 HungerRange;

			// Token: 0x04002F11 RID: 12049
			public Vector2 HappinessRange;

			// Token: 0x04002F12 RID: 12050
			public float Rate;

			// Token: 0x04002F13 RID: 12051
			public float HungerRate;

			// Token: 0x04002F14 RID: 12052
			public float InvHungerRate;

			// Token: 0x04002F15 RID: 12053
			public float HappinessRate;

			// Token: 0x04002F16 RID: 12054
			public float InvHappinessRate;

			// Token: 0x04002F17 RID: 12055
			private readonly float totalCommonness;

			// Token: 0x04002F18 RID: 12056
			private float timer;

			// Token: 0x02000E7D RID: 3709
			public struct Item
			{
				// Token: 0x0400427B RID: 17019
				public ItemPrefab Prefab;

				// Token: 0x0400427C RID: 17020
				public float Commonness;
			}
		}

		// Token: 0x0200083F RID: 2111
		private class Food
		{
			// Token: 0x04002F19 RID: 12057
			public Identifier Tag;

			// Token: 0x04002F1A RID: 12058
			public Vector2 HungerRange;

			// Token: 0x04002F1B RID: 12059
			public float Hunger;

			// Token: 0x04002F1C RID: 12060
			public float Happiness;

			// Token: 0x04002F1D RID: 12061
			public float Priority;

			// Token: 0x04002F1E RID: 12062
			public bool IgnoreContained;

			// Token: 0x04002F1F RID: 12063
			public CharacterParams.TargetParams TargetParams;
		}
	}
}
