using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002A6 RID: 678
	internal class SpawnAction : EventAction
	{
		// Token: 0x17000F80 RID: 3968
		// (get) Token: 0x06003ADF RID: 15071 RVA: 0x00220C71 File Offset: 0x0021EE71
		// (set) Token: 0x06003AE0 RID: 15072 RVA: 0x00220C79 File Offset: 0x0021EE79
		[Serialize("", IsPropertySaveable.Yes, "Species name of the character to spawn.", "", false)]
		public Identifier SpeciesName { get; set; }

		// Token: 0x17000F81 RID: 3969
		// (get) Token: 0x06003AE1 RID: 15073 RVA: 0x00220C82 File Offset: 0x0021EE82
		// (set) Token: 0x06003AE2 RID: 15074 RVA: 0x00220C8A File Offset: 0x0021EE8A
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the NPC set to choose from.", "", false)]
		public Identifier NPCSetIdentifier { get; set; }

		// Token: 0x17000F82 RID: 3970
		// (get) Token: 0x06003AE3 RID: 15075 RVA: 0x00220C93 File Offset: 0x0021EE93
		// (set) Token: 0x06003AE4 RID: 15076 RVA: 0x00220C9B File Offset: 0x0021EE9B
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the NPC.", "", false)]
		public Identifier NPCIdentifier { get; set; }

		// Token: 0x17000F83 RID: 3971
		// (get) Token: 0x06003AE5 RID: 15077 RVA: 0x00220CA4 File Offset: 0x0021EEA4
		// (set) Token: 0x06003AE6 RID: 15078 RVA: 0x00220CAC File Offset: 0x0021EEAC
		[Serialize(true, IsPropertySaveable.Yes, "Should taking the items of this npc be considered as stealing?", "", false)]
		public bool LootingIsStealing { get; set; }

		// Token: 0x17000F84 RID: 3972
		// (get) Token: 0x06003AE7 RID: 15079 RVA: 0x00220CB5 File Offset: 0x0021EEB5
		// (set) Token: 0x06003AE8 RID: 15080 RVA: 0x00220CBD File Offset: 0x0021EEBD
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item to spawn.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x17000F85 RID: 3973
		// (get) Token: 0x06003AE9 RID: 15081 RVA: 0x00220CC6 File Offset: 0x0021EEC6
		// (set) Token: 0x06003AEA RID: 15082 RVA: 0x00220CCE File Offset: 0x0021EECE
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to spawn.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000F86 RID: 3974
		// (get) Token: 0x06003AEB RID: 15083 RVA: 0x00220CD7 File Offset: 0x0021EED7
		// (set) Token: 0x06003AEC RID: 15084 RVA: 0x00220CDF File Offset: 0x0021EEDF
		[Serialize("", IsPropertySaveable.Yes, "The spawned entity will be assigned this tag. The tag can be used to refer to the entity by other actions of the event.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F87 RID: 3975
		// (get) Token: 0x06003AED RID: 15085 RVA: 0x00220CE8 File Offset: 0x0021EEE8
		// (set) Token: 0x06003AEE RID: 15086 RVA: 0x00220CF0 File Offset: 0x0021EEF0
		[Serialize("", IsPropertySaveable.Yes, "Tag of an entity with an inventory to spawn the item into.", "", false)]
		public Identifier TargetInventory { get; set; }

		// Token: 0x17000F88 RID: 3976
		// (get) Token: 0x06003AEF RID: 15087 RVA: 0x00220CF9 File Offset: 0x0021EEF9
		// (set) Token: 0x06003AF0 RID: 15088 RVA: 0x00220D01 File Offset: 0x0021EF01
		[Serialize(SpawnAction.SpawnLocationType.Any, IsPropertySaveable.Yes, "Where should the entity spawn? This can be restricted further with the other spawn point options.", "", false)]
		public SpawnAction.SpawnLocationType SpawnLocation { get; set; }

		// Token: 0x17000F89 RID: 3977
		// (get) Token: 0x06003AF1 RID: 15089 RVA: 0x00220D0A File Offset: 0x0021EF0A
		// (set) Token: 0x06003AF2 RID: 15090 RVA: 0x00220D12 File Offset: 0x0021EF12
		[Serialize(SpawnType.Human, IsPropertySaveable.Yes, "Type of spawnpoint to spawn the entity at. Ignored if SpawnPointTag is set.", "", false)]
		public SpawnType SpawnPointType { get; set; }

		// Token: 0x17000F8A RID: 3978
		// (get) Token: 0x06003AF3 RID: 15091 RVA: 0x00220D1B File Offset: 0x0021EF1B
		// (set) Token: 0x06003AF4 RID: 15092 RVA: 0x00220D23 File Offset: 0x0021EF23
		[Serialize("", IsPropertySaveable.Yes, "Tag of a spawnpoint to spawn the entity at.", "", false)]
		public Identifier SpawnPointTag { get; set; }

		// Token: 0x17000F8B RID: 3979
		// (get) Token: 0x06003AF5 RID: 15093 RVA: 0x00220D2C File Offset: 0x0021EF2C
		// (set) Token: 0x06003AF6 RID: 15094 RVA: 0x00220D34 File Offset: 0x0021EF34
		[Serialize(CharacterTeamType.FriendlyNPC, IsPropertySaveable.Yes, "Team of the NPC to spawn. Only valid when spawning a character.", "", false)]
		public CharacterTeamType TeamID { get; protected set; }

		// Token: 0x17000F8C RID: 3980
		// (get) Token: 0x06003AF7 RID: 15095 RVA: 0x00220D3D File Offset: 0x0021EF3D
		// (set) Token: 0x06003AF8 RID: 15096 RVA: 0x00220D45 File Offset: 0x0021EF45
		[Serialize(false, IsPropertySaveable.Yes, "Should we spawn the entity even when no spawn points with matching tags were found?", "", false)]
		public bool RequireSpawnPointTag { get; set; }

		// Token: 0x17000F8D RID: 3981
		// (get) Token: 0x06003AF9 RID: 15097 RVA: 0x00220D4E File Offset: 0x0021EF4E
		// (set) Token: 0x06003AFA RID: 15098 RVA: 0x00220D56 File Offset: 0x0021EF56
		[Serialize(true, IsPropertySaveable.Yes, "If false, we won't spawn another character if one with the same identifier has already been spawned.", "", false)]
		public bool AllowDuplicates { get; set; }

		// Token: 0x17000F8E RID: 3982
		// (get) Token: 0x06003AFB RID: 15099 RVA: 0x00220D5F File Offset: 0x0021EF5F
		// (set) Token: 0x06003AFC RID: 15100 RVA: 0x00220D67 File Offset: 0x0021EF67
		[Serialize(1, IsPropertySaveable.Yes, "Number of entities to spawn.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000F8F RID: 3983
		// (get) Token: 0x06003AFD RID: 15101 RVA: 0x00220D70 File Offset: 0x0021EF70
		// (set) Token: 0x06003AFE RID: 15102 RVA: 0x00220D78 File Offset: 0x0021EF78
		[Serialize(true, IsPropertySaveable.Yes, "Should the item be spawned even if the target inventory is full (just spawning it at the position of the target)? Only valid if spawning an item in an inventory.", "", false)]
		public bool SpawnIfInventoryFull { get; set; }

		// Token: 0x17000F90 RID: 3984
		// (get) Token: 0x06003AFF RID: 15103 RVA: 0x00220D81 File Offset: 0x0021EF81
		// (set) Token: 0x06003B00 RID: 15104 RVA: 0x00220D89 File Offset: 0x0021EF89
		[Serialize(100f, IsPropertySaveable.Yes, "Random offset to add to the spawn position.", "", false)]
		public float Offset { get; set; }

		// Token: 0x17000F91 RID: 3985
		// (get) Token: 0x06003B01 RID: 15105 RVA: 0x00220D92 File Offset: 0x0021EF92
		// (set) Token: 0x06003B02 RID: 15106 RVA: 0x00220DA4 File Offset: 0x0021EFA4
		[Serialize("", IsPropertySaveable.Yes, "What outpost module tags does the entity prefer to spawn in.", "", false)]
		public string TargetModuleTags
		{
			get
			{
				return string.Join<Identifier>(",", this.targetModuleTags);
			}
			set
			{
				this.targetModuleTags.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitTags = value.Split(',', StringSplitOptions.None);
					foreach (string s in splitTags)
					{
						this.targetModuleTags.Add(s.ToIdentifier());
					}
				}
			}
		}

		// Token: 0x17000F92 RID: 3986
		// (get) Token: 0x06003B03 RID: 15107 RVA: 0x00220DF4 File Offset: 0x0021EFF4
		// (set) Token: 0x06003B04 RID: 15108 RVA: 0x00220DFC File Offset: 0x0021EFFC
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI ignore this item. This will prevent outpost NPCs cleaning up or otherwise using important items intended to be left for the players.", "", false)]
		public bool IgnoreByAI { get; set; }

		// Token: 0x17000F93 RID: 3987
		// (get) Token: 0x06003B05 RID: 15109 RVA: 0x00220E05 File Offset: 0x0021F005
		// (set) Token: 0x06003B06 RID: 15110 RVA: 0x00220E0D File Offset: 0x0021F00D
		[Serialize(true, IsPropertySaveable.Yes, "If disabled, the action will choose a spawn position away from players' views if one is available.", "", false)]
		public bool AllowInPlayerView { get; set; }

		// Token: 0x17000F94 RID: 3988
		// (get) Token: 0x06003B07 RID: 15111 RVA: 0x00220E16 File Offset: 0x0021F016
		// (set) Token: 0x06003B08 RID: 15112 RVA: 0x00220E1E File Offset: 0x0021F01E
		[Serialize(false, IsPropertySaveable.Yes, "Should the event continue even if the entity failed to spawn for whatever reason?", "", false)]
		public bool ContinueIfFailedToSpawn { get; set; }

		// Token: 0x06003B09 RID: 15113 RVA: 0x00220E28 File Offset: 0x0021F028
		public SpawnAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.ignoreSpawnPointType = (element.GetAttribute("spawnpointtype") == null);
			string key = "teamtag";
			string key2 = "team";
			CharacterTeamType teamID = this.TeamID;
			CharacterTeamType attributeEnum = element.GetAttributeEnum<CharacterTeamType>(key2, teamID);
			this.TeamID = element.GetAttributeEnum<CharacterTeamType>(key, attributeEnum);
			if (element.GetAttribute("submarinetype") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in even \"");
				EventPrefab prefab = parentEvent.Prefab;
				defaultInterpolatedStringHandler.AppendFormatted(((prefab != null) ? prefab.Identifier.ToString() : null) ?? "unknown");
				defaultInterpolatedStringHandler.AppendLiteral("\". ");
				defaultInterpolatedStringHandler.AppendLiteral("The attribute \"submarinetype\" is not valid in ");
				defaultInterpolatedStringHandler.AppendFormatted("SpawnAction");
				defaultInterpolatedStringHandler.AppendLiteral(". Did you mean ");
				defaultInterpolatedStringHandler.AppendFormatted("SpawnLocation");
				defaultInterpolatedStringHandler.AppendLiteral("?");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x06003B0A RID: 15114 RVA: 0x00220F3E File Offset: 0x0021F13E
		public override bool IsFinished(ref string goTo)
		{
			return (this.spawnedEntity != null || this.ContinueIfFailedToSpawn) && this.spawned;
		}

		// Token: 0x06003B0B RID: 15115 RVA: 0x00220F58 File Offset: 0x0021F158
		public override void Reset()
		{
			this.spawned = false;
			this.spawnedEntity = null;
		}

		// Token: 0x06003B0C RID: 15116 RVA: 0x00220F68 File Offset: 0x0021F168
		public override void Update(float deltaTime)
		{
			if (this.spawned)
			{
				return;
			}
			if (!this.NPCSetIdentifier.IsEmpty && !this.NPCIdentifier.IsEmpty)
			{
				HumanPrefab humanPrefab = null;
				Level loaded = Level.Loaded;
				Location startLocation = (loaded != null) ? loaded.StartLocation : null;
				if (startLocation != null)
				{
					humanPrefab = (this.<Update>g__TryFindHumanPrefab|91_0(startLocation.Faction) ?? this.<Update>g__TryFindHumanPrefab|91_0(startLocation.SecondaryFaction));
				}
				if (humanPrefab == null)
				{
					humanPrefab = NPCSet.Get(this.NPCSetIdentifier, this.NPCIdentifier, true, this.ParentEvent.Prefab.ContentPackage);
				}
				if (humanPrefab != null)
				{
					if (!this.AllowDuplicates && Character.CharacterList.Any(delegate(Character c)
					{
						CharacterInfo info = c.Info;
						Identifier? identifier;
						Identifier? identifier2;
						if (info == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(info.HumanPrefabIds.Item2);
						}
						identifier = identifier2;
						Identifier? identifier3 = new Identifier?(this.NPCIdentifier);
						if (identifier == identifier3)
						{
							CharacterInfo info2 = c.Info;
							Identifier? identifier4;
							Identifier? identifier5;
							if (info2 == null)
							{
								identifier4 = null;
								identifier5 = identifier4;
							}
							else
							{
								identifier5 = new Identifier?(info2.HumanPrefabIds.Item1);
							}
							identifier4 = identifier5;
							Identifier? identifier6 = new Identifier?(this.NPCSetIdentifier);
							return identifier4 == identifier6;
						}
						return false;
					}))
					{
						this.spawned = true;
						return;
					}
					ISpatialEntity spawnPos = this.GetSpawnPos();
					if (spawnPos != null)
					{
						Action<Character> <>9__2;
						for (int i = 0; i < this.Amount; i++)
						{
							EntitySpawner spawner = Entity.Spawner;
							Identifier humanSpeciesName = CharacterPrefab.HumanSpeciesName;
							Vector2 worldPosition = SpawnAction.OffsetSpawnPos(spawnPos.WorldPosition, Rand.Range(0f, this.Offset, Rand.RandSync.Unsynced));
							CharacterInfo characterInfo = humanPrefab.CreateCharacterInfo(Rand.RandSync.Unsynced);
							Action<Character> onSpawn;
							if ((onSpawn = <>9__2) == null)
							{
								onSpawn = (<>9__2 = delegate(Character newCharacter)
								{
									if (newCharacter == null)
									{
										return;
									}
									newCharacter.HumanPrefab = humanPrefab;
									newCharacter.SetOriginalTeamAndChangeTeam(this.TeamID, true);
									newCharacter.EnableDespawn = false;
									humanPrefab.GiveItems(newCharacter, newCharacter.Submarine, spawnPos as WayPoint, Rand.RandSync.Unsynced, true);
									if (this.LootingIsStealing)
									{
										foreach (Item item2 in newCharacter.Inventory.FindAllItems(null, true, null))
										{
											item2.SpawnedInCurrentOutpost = true;
											item2.AllowStealing = false;
										}
									}
									humanPrefab.InitializeCharacter(newCharacter, spawnPos);
									if (!this.TargetTag.IsEmpty && newCharacter != null)
									{
										this.ParentEvent.AddTarget(this.TargetTag, newCharacter);
									}
									this.spawnedEntity = newCharacter;
									Level loaded2 = Level.Loaded;
									SubmarineInfo submarineInfo;
									if (loaded2 == null)
									{
										submarineInfo = null;
									}
									else
									{
										Submarine startOutpost = loaded2.StartOutpost;
										submarineInfo = ((startOutpost != null) ? startOutpost.Info : null);
									}
									SubmarineInfo outPostInfo = submarineInfo;
									if (outPostInfo != null)
									{
										outPostInfo.AddOutpostNPCIdentifierOrTag(newCharacter, humanPrefab.Identifier);
										foreach (Identifier tag in humanPrefab.GetTags())
										{
											outPostInfo.AddOutpostNPCIdentifierOrTag(newCharacter, tag);
										}
									}
								});
							}
							spawner.AddCharacterToSpawnQueue(humanSpeciesName, worldPosition, characterInfo, onSpawn);
						}
					}
				}
			}
			else if (!this.SpeciesName.IsEmpty)
			{
				if (!this.AllowDuplicates && Character.CharacterList.Any(delegate(Character c)
				{
					Identifier speciesName = c.SpeciesName;
					Identifier speciesName2 = this.SpeciesName;
					return speciesName == speciesName2;
				}))
				{
					this.spawned = true;
					return;
				}
				ISpatialEntity spawnPos3 = this.GetSpawnPos();
				if (spawnPos3 != null)
				{
					for (int j = 0; j < this.Amount; j++)
					{
						Entity.Spawner.AddCharacterToSpawnQueue(this.SpeciesName, SpawnAction.OffsetSpawnPos(spawnPos3.WorldPosition, Rand.Range(0f, this.Offset, Rand.RandSync.Unsynced)), delegate(Character newCharacter)
						{
							if (!this.TargetTag.IsEmpty && newCharacter != null)
							{
								this.ParentEvent.AddTarget(this.TargetTag, newCharacter);
							}
							this.spawnedEntity = newCharacter;
							if (newCharacter != null)
							{
								EnemyAIController enemyAi = newCharacter.AIController as EnemyAIController;
								if (enemyAi != null)
								{
									Submarine ownSub = newCharacter.Submarine;
									if (ownSub != null)
									{
										enemyAi.SetUnattackableSubmarines(ownSub, true, true, true);
									}
								}
							}
						});
					}
				}
			}
			else if (!this.ItemIdentifier.IsEmpty || !this.ItemTag.IsEmpty)
			{
				ItemPrefab itemPrefab = null;
				if (!this.ItemIdentifier.IsEmpty)
				{
					itemPrefab = (MapEntityPrefab.FindByIdentifier(this.ItemIdentifier) as ItemPrefab);
					if (itemPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in SpawnAction (item prefab \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" not found)");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
					}
				}
				else if (!this.ItemTag.IsEmpty)
				{
					itemPrefab = (from ip in ItemPrefab.Prefabs
					where ip.Tags.Contains(this.ItemTag)
					select ip).GetRandom(Rand.RandSync.Unsynced);
				}
				Inventory spawnInventory = null;
				if (!this.TargetInventory.IsEmpty)
				{
					IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetInventory);
					if (targets.Any<Entity>())
					{
						Entity target = targets.First((Entity t) => t is Item || t is Character);
						Character character = target as Character;
						if (character != null)
						{
							spawnInventory = character.Inventory;
						}
						else
						{
							Item item = target as Item;
							if (item != null)
							{
								spawnInventory = item.OwnInventory;
							}
						}
					}
					if (spawnInventory == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(70, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Could not spawn \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.ItemIdentifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\" in target inventory \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TargetInventory);
						defaultInterpolatedStringHandler2.AppendLiteral("\" - matching target not found.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
					}
				}
				if (spawnInventory == null)
				{
					ISpatialEntity spawnPos2 = this.GetSpawnPos();
					if (spawnPos2 != null)
					{
						for (int k = 0; k < this.Amount; k++)
						{
							EntitySpawner spawner2 = Entity.Spawner;
							ItemPrefab itemPrefab2 = itemPrefab;
							Vector2 worldPosition2 = SpawnAction.OffsetSpawnPos(spawnPos2.WorldPosition, Rand.Range(0f, this.Offset, Rand.RandSync.Unsynced));
							Action<Item> onSpawned = new Action<Item>(this.<Update>g__onSpawned|91_6);
							spawner2.AddItemToSpawnQueue(itemPrefab2, worldPosition2, null, null, onSpawned);
						}
					}
				}
				else
				{
					for (int l = 0; l < this.Amount; l++)
					{
						EntitySpawner spawner3 = Entity.Spawner;
						ItemPrefab itemPrefab3 = itemPrefab;
						Inventory inventory = spawnInventory;
						bool spawnIfInventoryFull = this.SpawnIfInventoryFull;
						Action<Item> onSpawned = new Action<Item>(this.<Update>g__onSpawned|91_6);
						spawner3.AddItemToSpawnQueue(itemPrefab3, inventory, null, null, onSpawned, spawnIfInventoryFull, false, InvSlotType.None);
					}
				}
			}
			this.spawned = true;
		}

		// Token: 0x06003B0D RID: 15117 RVA: 0x00221408 File Offset: 0x0021F608
		public static Vector2 OffsetSpawnPos(Vector2 pos, float offset)
		{
			Hull hull = Hull.FindHull(pos, null, true, true);
			pos += Rand.Vector(offset, Rand.RandSync.Unsynced);
			if (hull != null)
			{
				float margin = 50f;
				pos = new Vector2(MathHelper.Clamp(pos.X, (float)hull.WorldRect.X + margin, (float)hull.WorldRect.Right - margin), MathHelper.Clamp(pos.Y, (float)(hull.WorldRect.Y - hull.WorldRect.Height) + margin, (float)hull.WorldRect.Y - margin));
			}
			return pos;
		}

		// Token: 0x06003B0E RID: 15118 RVA: 0x0022149C File Offset: 0x0021F69C
		private ISpatialEntity GetSpawnPos()
		{
			if (!this.SpawnPointTag.IsEmpty)
			{
				IEnumerable<Item> potentialItems = from it in Item.ItemList
				where SpawnAction.IsValidSubmarineType(this.SpawnLocation, it.Submarine)
				select it;
				if (!this.AllowInPlayerView)
				{
					potentialItems = SpawnAction.GetEntitiesNotInPlayerView<Item>(potentialItems);
				}
				Item item = (from it in potentialItems
				where it.HasTag(this.SpawnPointTag)
				select it).GetRandomUnsynced<Item>();
				if (item != null)
				{
					return item;
				}
				IEnumerable<Entity> potentialTargets = from t in this.ParentEvent.GetTargets(this.SpawnPointTag)
				where SpawnAction.IsValidSubmarineType(this.SpawnLocation, t.Submarine)
				select t;
				if (!this.AllowInPlayerView)
				{
					potentialTargets = SpawnAction.GetEntitiesNotInPlayerView<Entity>(potentialTargets);
				}
				Entity target = potentialTargets.GetRandomUnsynced<Entity>();
				if (target != null)
				{
					return target;
				}
			}
			SpawnType? spawnPointType = null;
			if (!this.ignoreSpawnPointType)
			{
				spawnPointType = new SpawnType?(this.SpawnPointType);
			}
			return SpawnAction.GetSpawnPos(this.SpawnLocation, spawnPointType, this.targetModuleTags, this.SpawnPointTag.IsEmpty ? null : this.SpawnPointTag.ToEnumerable<Identifier>(), false, this.RequireSpawnPointTag, this.AllowInPlayerView);
		}

		// Token: 0x06003B0F RID: 15119 RVA: 0x002215A0 File Offset: 0x0021F7A0
		private static bool IsValidSubmarineType(SpawnAction.SpawnLocationType spawnLocation, Submarine submarine)
		{
			bool result;
			switch (spawnLocation)
			{
			case SpawnAction.SpawnLocationType.Any:
				result = true;
				break;
			case SpawnAction.SpawnLocationType.MainSub:
				result = (submarine == Submarine.MainSub);
				break;
			case SpawnAction.SpawnLocationType.Outpost:
			{
				bool flag;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null)
					{
						flag = info.IsOutpost;
						goto IL_62;
					}
				}
				flag = false;
				IL_62:
				result = flag;
				break;
			}
			case SpawnAction.SpawnLocationType.MainPath:
			case SpawnAction.SpawnLocationType.Cave:
			case SpawnAction.SpawnLocationType.AbyssCave:
			case SpawnAction.SpawnLocationType.NearMainSub:
				result = (submarine == null);
				break;
			case SpawnAction.SpawnLocationType.Ruin:
			{
				bool flag2;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null)
					{
						flag2 = info.IsRuin;
						goto IL_94;
					}
				}
				flag2 = false;
				IL_94:
				result = flag2;
				break;
			}
			case SpawnAction.SpawnLocationType.Wreck:
			{
				bool flag3;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null)
					{
						flag3 = info.IsWreck;
						goto IL_7B;
					}
				}
				flag3 = false;
				IL_7B:
				result = flag3;
				break;
			}
			case SpawnAction.SpawnLocationType.BeaconStation:
			{
				object obj;
				if (submarine == null)
				{
					obj = null;
				}
				else
				{
					SubmarineInfo info2 = submarine.Info;
					obj = ((info2 != null) ? info2.BeaconStationInfo : null);
				}
				result = (obj != null);
				break;
			}
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		// Token: 0x06003B10 RID: 15120 RVA: 0x0022166C File Offset: 0x0021F86C
		private static IEnumerable<T> GetEntitiesNotInPlayerView<T>(IEnumerable<T> entities) where T : ISpatialEntity
		{
			if (entities.Any((T e) => !SpawnAction.IsInPlayerView(e)))
			{
				return from e in entities
				where !SpawnAction.IsInPlayerView(e)
				select e;
			}
			return entities;
		}

		// Token: 0x06003B11 RID: 15121 RVA: 0x002216C8 File Offset: 0x0021F8C8
		private static bool IsInPlayerView(ISpatialEntity entity)
		{
			foreach (Character character in Character.CharacterList)
			{
				if (character.IsPlayer && !character.IsDead && character.CanSeeTarget(entity, null, false, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003B12 RID: 15122 RVA: 0x00221738 File Offset: 0x0021F938
		public static WayPoint GetSpawnPos(SpawnAction.SpawnLocationType spawnLocation, SpawnType? spawnPointType, IEnumerable<Identifier> moduleFlags = null, IEnumerable<Identifier> spawnpointTags = null, bool asFarAsPossibleFromAirlock = false, bool requireTaggedSpawnPoint = false, bool allowInPlayerView = true)
		{
			SpawnAction.<>c__DisplayClass97_0 CS$<>8__locals1 = new SpawnAction.<>c__DisplayClass97_0();
			CS$<>8__locals1.spawnLocation = spawnLocation;
			CS$<>8__locals1.spawnPointType = spawnPointType;
			CS$<>8__locals1.moduleFlags = moduleFlags;
			CS$<>8__locals1.spawnpointTags = spawnpointTags;
			CS$<>8__locals1.requireHull = (CS$<>8__locals1.spawnLocation == SpawnAction.SpawnLocationType.MainSub || CS$<>8__locals1.spawnLocation == SpawnAction.SpawnLocationType.Outpost);
			CS$<>8__locals1.potentialSpawnPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => SpawnAction.IsValidSubmarineType(CS$<>8__locals1.spawnLocation, wp.Submarine) && (wp.CurrentHull != null || !CS$<>8__locals1.requireHull));
			CS$<>8__locals1.potentialSpawnPoints = from wp in CS$<>8__locals1.potentialSpawnPoints
			where wp.ConnectedDoor == null && wp.Ladders == null && wp.IsTraversable
			select wp;
			IEnumerable<WayPoint> spawnPointsWithCorrectType;
			if (CS$<>8__locals1.spawnPointType != null)
			{
				spawnPointsWithCorrectType = from wp in CS$<>8__locals1.potentialSpawnPoints
				where CS$<>8__locals1.spawnPointType.Value.HasFlag(wp.SpawnType) && (wp.SpawnType != SpawnType.Path || CS$<>8__locals1.spawnPointType.Value == SpawnType.Path)
				select wp;
			}
			else
			{
				spawnPointsWithCorrectType = CS$<>8__locals1.potentialSpawnPoints;
			}
			if (spawnPointsWithCorrectType.Any<WayPoint>())
			{
				CS$<>8__locals1.potentialSpawnPoints = spawnPointsWithCorrectType;
			}
			if (CS$<>8__locals1.moduleFlags != null && CS$<>8__locals1.moduleFlags.Any<Identifier>())
			{
				IEnumerable<WayPoint> spawnPointsWithCorrectFlags = CS$<>8__locals1.potentialSpawnPoints.Where(delegate(WayPoint wp)
				{
					Hull h = wp.CurrentHull;
					return h != null && h.OutpostModuleTags.Any(new Func<Identifier, bool>(CS$<>8__locals1.moduleFlags.Contains<Identifier>));
				});
				if (spawnPointsWithCorrectFlags.Any<WayPoint>())
				{
					CS$<>8__locals1.potentialSpawnPoints = spawnPointsWithCorrectFlags.ToList<WayPoint>();
				}
			}
			if (CS$<>8__locals1.spawnpointTags != null && CS$<>8__locals1.spawnpointTags.Any<Identifier>())
			{
				IEnumerable<WayPoint> spawnPointsWithTag = from wp in CS$<>8__locals1.potentialSpawnPoints
				where CS$<>8__locals1.spawnpointTags.Any((Identifier tag) => wp.Tags.Contains(tag) && wp.ConnectedDoor == null && wp.IsTraversable)
				select wp;
				if (requireTaggedSpawnPoint || spawnPointsWithTag.Any<WayPoint>())
				{
					CS$<>8__locals1.potentialSpawnPoints = spawnPointsWithTag.ToList<WayPoint>();
				}
				else
				{
					CS$<>8__locals1.<GetSpawnPos>g__TryGetSpawnPointsWithNoTag|3();
				}
			}
			else
			{
				CS$<>8__locals1.<GetSpawnPos>g__TryGetSpawnPointsWithNoTag|3();
			}
			if (CS$<>8__locals1.potentialSpawnPoints.None(null))
			{
				if (requireTaggedSpawnPoint && CS$<>8__locals1.spawnpointTags != null && CS$<>8__locals1.spawnpointTags.Any<Identifier>())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find a spawn point for a SpawnAction (spawn location: ");
					defaultInterpolatedStringHandler.AppendFormatted<SpawnAction.SpawnLocationType>(CS$<>8__locals1.spawnLocation);
					defaultInterpolatedStringHandler.AppendLiteral(" (tag: ");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join<Identifier>(",", CS$<>8__locals1.spawnpointTags));
					defaultInterpolatedStringHandler.AppendLiteral("), skipping.");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.White), false);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(65, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find a spawn point for a SpawnAction (spawn location: ");
					defaultInterpolatedStringHandler2.AppendFormatted<SpawnAction.SpawnLocationType>(CS$<>8__locals1.spawnLocation);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
				return null;
			}
			IEnumerable<WayPoint> nonPathSpawnPoints = from wp in CS$<>8__locals1.potentialSpawnPoints
			where wp.SpawnType > SpawnType.Path
			select wp;
			IEnumerable<WayPoint> enumerable;
			if (nonPathSpawnPoints.Any<WayPoint>())
			{
				SpawnType? spawnPointType2 = CS$<>8__locals1.spawnPointType;
				SpawnType spawnType = SpawnType.Path;
				if (!(spawnPointType2.GetValueOrDefault() == spawnType & spawnPointType2 != null))
				{
					enumerable = nonPathSpawnPoints;
					goto IL_297;
				}
			}
			enumerable = CS$<>8__locals1.potentialSpawnPoints;
			IL_297:
			IEnumerable<WayPoint> validSpawnPoints = enumerable;
			IEnumerable<WayPoint> airlockSpawnPoints = CS$<>8__locals1.potentialSpawnPoints.Where(delegate(WayPoint wp)
			{
				Hull currentHull = wp.CurrentHull;
				return currentHull != null && currentHull.OutpostModuleTags.Contains("airlock".ToIdentifier());
			});
			if (airlockSpawnPoints.Count<WayPoint>() < validSpawnPoints.Count<WayPoint>())
			{
				validSpawnPoints = validSpawnPoints.Except(airlockSpawnPoints);
			}
			if (validSpawnPoints.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(109, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Could not find a spawn point of the correct type for a SpawnAction (spawn location: ");
				defaultInterpolatedStringHandler3.AppendFormatted<SpawnAction.SpawnLocationType>(CS$<>8__locals1.spawnLocation);
				defaultInterpolatedStringHandler3.AppendLiteral(", type: ");
				defaultInterpolatedStringHandler3.AppendFormatted<SpawnType?>(CS$<>8__locals1.spawnPointType);
				defaultInterpolatedStringHandler3.AppendLiteral(", module flags: ");
				defaultInterpolatedStringHandler3.AppendFormatted((CS$<>8__locals1.moduleFlags == null || !CS$<>8__locals1.moduleFlags.Any<Identifier>()) ? "none" : string.Join<Identifier>(", ", CS$<>8__locals1.moduleFlags));
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
				return CS$<>8__locals1.potentialSpawnPoints.GetRandomUnsynced<WayPoint>();
			}
			switch (CS$<>8__locals1.spawnLocation)
			{
			case SpawnAction.SpawnLocationType.MainPath:
			case SpawnAction.SpawnLocationType.NearMainSub:
				validSpawnPoints = from p in validSpawnPoints
				where Submarine.Loaded.None((Submarine s) => ToolBox.GetWorldBounds(s.Borders.Center, s.Borders.Size).ContainsWorld(p.WorldPosition))
				select p;
				if (Level.Loaded != null)
				{
					validSpawnPoints = from p in validSpawnPoints
					where p.WorldPosition.Y > (float)Level.Loaded.AbyssStart && p.Cave == null && p.Ruin == null
					select p;
				}
				break;
			case SpawnAction.SpawnLocationType.Cave:
				validSpawnPoints = from p in validSpawnPoints
				where p.WorldPosition.Y > (float)Level.Loaded.AbyssStart && p.Cave != null
				select p;
				break;
			case SpawnAction.SpawnLocationType.AbyssCave:
				validSpawnPoints = from p in validSpawnPoints
				where p.WorldPosition.Y < (float)Level.Loaded.AbyssStart && p.Cave != null
				select p;
				break;
			}
			if (validSpawnPoints.Any((WayPoint wp) => wp.SpawnType > SpawnType.Path))
			{
				validSpawnPoints = from wp in validSpawnPoints
				where wp.SpawnType > SpawnType.Path
				select wp;
			}
			if (CS$<>8__locals1.spawnpointTags == null || CS$<>8__locals1.spawnpointTags.None(null))
			{
				IEnumerable<WayPoint> spawnPoints = from wp in validSpawnPoints
				where !wp.Tags.Any<Identifier>()
				select wp;
				if (spawnPoints.Any<WayPoint>())
				{
					validSpawnPoints = spawnPoints.ToList<WayPoint>();
				}
			}
			if (!allowInPlayerView)
			{
				validSpawnPoints = SpawnAction.GetEntitiesNotInPlayerView<WayPoint>(validSpawnPoints);
			}
			if (CS$<>8__locals1.spawnLocation == SpawnAction.SpawnLocationType.NearMainSub && Submarine.MainSub != null)
			{
				WayPoint closestPoint = validSpawnPoints.First<WayPoint>();
				float closestDist = float.PositiveInfinity;
				foreach (WayPoint wp2 in validSpawnPoints)
				{
					float dist = Vector2.DistanceSquared(wp2.WorldPosition, Submarine.MainSub.WorldPosition);
					if (dist < closestDist)
					{
						closestDist = dist;
						closestPoint = wp2;
					}
				}
				return closestPoint;
			}
			if (asFarAsPossibleFromAirlock && airlockSpawnPoints.Any<WayPoint>())
			{
				WayPoint furthestPoint = validSpawnPoints.First<WayPoint>();
				float furthestDist = 0f;
				foreach (WayPoint waypoint in validSpawnPoints)
				{
					float dist2 = Vector2.DistanceSquared(waypoint.WorldPosition, airlockSpawnPoints.First<WayPoint>().WorldPosition);
					if (dist2 > furthestDist)
					{
						furthestDist = dist2;
						furthestPoint = waypoint;
					}
				}
				return furthestPoint;
			}
			return validSpawnPoints.GetRandomUnsynced<WayPoint>();
		}

		// Token: 0x06003B13 RID: 15123 RVA: 0x00221D54 File Offset: 0x0021FF54
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.spawned, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SpawnAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Spawned entity: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.spawnedEntity.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003B14 RID: 15124 RVA: 0x00221DC8 File Offset: 0x0021FFC8
		[CompilerGenerated]
		private HumanPrefab <Update>g__TryFindHumanPrefab|91_0(Faction faction)
		{
			if (faction == null)
			{
				return null;
			}
			Identifier npcsetIdentifier = this.NPCSetIdentifier;
			Identifier npcidentifier = this.NPCIdentifier;
			Identifier identifier = "[faction]".ToIdentifier();
			HumanPrefab result;
			if ((result = NPCSet.Get(npcsetIdentifier, npcidentifier.Replace(identifier, faction.Prefab.Identifier), false, null)) == null)
			{
				Identifier npcsetIdentifier2 = this.NPCSetIdentifier;
				npcidentifier = this.NPCIdentifier;
				Identifier identifier2 = "[faction]".ToIdentifier();
				Identifier identifier3 = "coalition".ToIdentifier();
				result = NPCSet.Get(npcsetIdentifier2, npcidentifier.Replace(identifier2, identifier3), false, null);
			}
			return result;
		}

		// Token: 0x06003B19 RID: 15129 RVA: 0x00221F68 File Offset: 0x00220168
		[CompilerGenerated]
		private void <Update>g__onSpawned|91_6(Item newItem)
		{
			if (newItem != null)
			{
				if (!this.TargetTag.IsEmpty)
				{
					this.ParentEvent.AddTarget(this.TargetTag, newItem);
				}
				if (this.IgnoreByAI)
				{
					newItem.AddTag("ignorebyai");
				}
			}
			this.spawnedEntity = newItem;
		}

		// Token: 0x04001E3E RID: 7742
		private readonly HashSet<Identifier> targetModuleTags = new HashSet<Identifier>();

		// Token: 0x04001E46 RID: 7750
		private bool spawned;

		// Token: 0x04001E47 RID: 7751
		private Entity spawnedEntity;

		// Token: 0x04001E48 RID: 7752
		private readonly bool ignoreSpawnPointType;

		// Token: 0x02000F30 RID: 3888
		public enum SpawnLocationType
		{
			// Token: 0x040054E4 RID: 21732
			Any,
			// Token: 0x040054E5 RID: 21733
			MainSub,
			// Token: 0x040054E6 RID: 21734
			Outpost,
			// Token: 0x040054E7 RID: 21735
			MainPath,
			// Token: 0x040054E8 RID: 21736
			Cave,
			// Token: 0x040054E9 RID: 21737
			AbyssCave,
			// Token: 0x040054EA RID: 21738
			Ruin,
			// Token: 0x040054EB RID: 21739
			Wreck,
			// Token: 0x040054EC RID: 21740
			BeaconStation,
			// Token: 0x040054ED RID: 21741
			NearMainSub
		}
	}
}
