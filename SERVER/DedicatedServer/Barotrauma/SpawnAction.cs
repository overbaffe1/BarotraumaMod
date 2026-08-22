using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001B4 RID: 436
	internal class SpawnAction : EventAction
	{
		// Token: 0x1700093A RID: 2362
		// (get) Token: 0x06002033 RID: 8243 RVA: 0x000DB155 File Offset: 0x000D9355
		// (set) Token: 0x06002034 RID: 8244 RVA: 0x000DB15D File Offset: 0x000D935D
		[Serialize("", IsPropertySaveable.Yes, "Species name of the character to spawn.", "", false)]
		public Identifier SpeciesName { get; set; }

		// Token: 0x1700093B RID: 2363
		// (get) Token: 0x06002035 RID: 8245 RVA: 0x000DB166 File Offset: 0x000D9366
		// (set) Token: 0x06002036 RID: 8246 RVA: 0x000DB16E File Offset: 0x000D936E
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the NPC set to choose from.", "", false)]
		public Identifier NPCSetIdentifier { get; set; }

		// Token: 0x1700093C RID: 2364
		// (get) Token: 0x06002037 RID: 8247 RVA: 0x000DB177 File Offset: 0x000D9377
		// (set) Token: 0x06002038 RID: 8248 RVA: 0x000DB17F File Offset: 0x000D937F
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the NPC.", "", false)]
		public Identifier NPCIdentifier { get; set; }

		// Token: 0x1700093D RID: 2365
		// (get) Token: 0x06002039 RID: 8249 RVA: 0x000DB188 File Offset: 0x000D9388
		// (set) Token: 0x0600203A RID: 8250 RVA: 0x000DB190 File Offset: 0x000D9390
		[Serialize(true, IsPropertySaveable.Yes, "Should taking the items of this npc be considered as stealing?", "", false)]
		public bool LootingIsStealing { get; set; }

		// Token: 0x1700093E RID: 2366
		// (get) Token: 0x0600203B RID: 8251 RVA: 0x000DB199 File Offset: 0x000D9399
		// (set) Token: 0x0600203C RID: 8252 RVA: 0x000DB1A1 File Offset: 0x000D93A1
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item to spawn.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x1700093F RID: 2367
		// (get) Token: 0x0600203D RID: 8253 RVA: 0x000DB1AA File Offset: 0x000D93AA
		// (set) Token: 0x0600203E RID: 8254 RVA: 0x000DB1B2 File Offset: 0x000D93B2
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to spawn.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000940 RID: 2368
		// (get) Token: 0x0600203F RID: 8255 RVA: 0x000DB1BB File Offset: 0x000D93BB
		// (set) Token: 0x06002040 RID: 8256 RVA: 0x000DB1C3 File Offset: 0x000D93C3
		[Serialize("", IsPropertySaveable.Yes, "The spawned entity will be assigned this tag. The tag can be used to refer to the entity by other actions of the event.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000941 RID: 2369
		// (get) Token: 0x06002041 RID: 8257 RVA: 0x000DB1CC File Offset: 0x000D93CC
		// (set) Token: 0x06002042 RID: 8258 RVA: 0x000DB1D4 File Offset: 0x000D93D4
		[Serialize("", IsPropertySaveable.Yes, "Tag of an entity with an inventory to spawn the item into.", "", false)]
		public Identifier TargetInventory { get; set; }

		// Token: 0x17000942 RID: 2370
		// (get) Token: 0x06002043 RID: 8259 RVA: 0x000DB1DD File Offset: 0x000D93DD
		// (set) Token: 0x06002044 RID: 8260 RVA: 0x000DB1E5 File Offset: 0x000D93E5
		[Serialize(SpawnAction.SpawnLocationType.Any, IsPropertySaveable.Yes, "Where should the entity spawn? This can be restricted further with the other spawn point options.", "", false)]
		public SpawnAction.SpawnLocationType SpawnLocation { get; set; }

		// Token: 0x17000943 RID: 2371
		// (get) Token: 0x06002045 RID: 8261 RVA: 0x000DB1EE File Offset: 0x000D93EE
		// (set) Token: 0x06002046 RID: 8262 RVA: 0x000DB1F6 File Offset: 0x000D93F6
		[Serialize(SpawnType.Human, IsPropertySaveable.Yes, "Type of spawnpoint to spawn the entity at. Ignored if SpawnPointTag is set.", "", false)]
		public SpawnType SpawnPointType { get; set; }

		// Token: 0x17000944 RID: 2372
		// (get) Token: 0x06002047 RID: 8263 RVA: 0x000DB1FF File Offset: 0x000D93FF
		// (set) Token: 0x06002048 RID: 8264 RVA: 0x000DB207 File Offset: 0x000D9407
		[Serialize("", IsPropertySaveable.Yes, "Tag of a spawnpoint to spawn the entity at.", "", false)]
		public Identifier SpawnPointTag { get; set; }

		// Token: 0x17000945 RID: 2373
		// (get) Token: 0x06002049 RID: 8265 RVA: 0x000DB210 File Offset: 0x000D9410
		// (set) Token: 0x0600204A RID: 8266 RVA: 0x000DB218 File Offset: 0x000D9418
		[Serialize(CharacterTeamType.FriendlyNPC, IsPropertySaveable.Yes, "Team of the NPC to spawn. Only valid when spawning a character.", "", false)]
		public CharacterTeamType TeamID { get; protected set; }

		// Token: 0x17000946 RID: 2374
		// (get) Token: 0x0600204B RID: 8267 RVA: 0x000DB221 File Offset: 0x000D9421
		// (set) Token: 0x0600204C RID: 8268 RVA: 0x000DB229 File Offset: 0x000D9429
		[Serialize(false, IsPropertySaveable.Yes, "Should we spawn the entity even when no spawn points with matching tags were found?", "", false)]
		public bool RequireSpawnPointTag { get; set; }

		// Token: 0x17000947 RID: 2375
		// (get) Token: 0x0600204D RID: 8269 RVA: 0x000DB232 File Offset: 0x000D9432
		// (set) Token: 0x0600204E RID: 8270 RVA: 0x000DB23A File Offset: 0x000D943A
		[Serialize(true, IsPropertySaveable.Yes, "If false, we won't spawn another character if one with the same identifier has already been spawned.", "", false)]
		public bool AllowDuplicates { get; set; }

		// Token: 0x17000948 RID: 2376
		// (get) Token: 0x0600204F RID: 8271 RVA: 0x000DB243 File Offset: 0x000D9443
		// (set) Token: 0x06002050 RID: 8272 RVA: 0x000DB24B File Offset: 0x000D944B
		[Serialize(1, IsPropertySaveable.Yes, "Number of entities to spawn.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000949 RID: 2377
		// (get) Token: 0x06002051 RID: 8273 RVA: 0x000DB254 File Offset: 0x000D9454
		// (set) Token: 0x06002052 RID: 8274 RVA: 0x000DB25C File Offset: 0x000D945C
		[Serialize(true, IsPropertySaveable.Yes, "Should the item be spawned even if the target inventory is full (just spawning it at the position of the target)? Only valid if spawning an item in an inventory.", "", false)]
		public bool SpawnIfInventoryFull { get; set; }

		// Token: 0x1700094A RID: 2378
		// (get) Token: 0x06002053 RID: 8275 RVA: 0x000DB265 File Offset: 0x000D9465
		// (set) Token: 0x06002054 RID: 8276 RVA: 0x000DB26D File Offset: 0x000D946D
		[Serialize(100f, IsPropertySaveable.Yes, "Random offset to add to the spawn position.", "", false)]
		public float Offset { get; set; }

		// Token: 0x1700094B RID: 2379
		// (get) Token: 0x06002055 RID: 8277 RVA: 0x000DB276 File Offset: 0x000D9476
		// (set) Token: 0x06002056 RID: 8278 RVA: 0x000DB288 File Offset: 0x000D9488
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

		// Token: 0x1700094C RID: 2380
		// (get) Token: 0x06002057 RID: 8279 RVA: 0x000DB2D8 File Offset: 0x000D94D8
		// (set) Token: 0x06002058 RID: 8280 RVA: 0x000DB2E0 File Offset: 0x000D94E0
		[Serialize(false, IsPropertySaveable.Yes, "Should the AI ignore this item. This will prevent outpost NPCs cleaning up or otherwise using important items intended to be left for the players.", "", false)]
		public bool IgnoreByAI { get; set; }

		// Token: 0x1700094D RID: 2381
		// (get) Token: 0x06002059 RID: 8281 RVA: 0x000DB2E9 File Offset: 0x000D94E9
		// (set) Token: 0x0600205A RID: 8282 RVA: 0x000DB2F1 File Offset: 0x000D94F1
		[Serialize(true, IsPropertySaveable.Yes, "If disabled, the action will choose a spawn position away from players' views if one is available.", "", false)]
		public bool AllowInPlayerView { get; set; }

		// Token: 0x1700094E RID: 2382
		// (get) Token: 0x0600205B RID: 8283 RVA: 0x000DB2FA File Offset: 0x000D94FA
		// (set) Token: 0x0600205C RID: 8284 RVA: 0x000DB302 File Offset: 0x000D9502
		[Serialize(false, IsPropertySaveable.Yes, "Should the event continue even if the entity failed to spawn for whatever reason?", "", false)]
		public bool ContinueIfFailedToSpawn { get; set; }

		// Token: 0x0600205D RID: 8285 RVA: 0x000DB30C File Offset: 0x000D950C
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

		// Token: 0x0600205E RID: 8286 RVA: 0x000DB422 File Offset: 0x000D9622
		public override bool IsFinished(ref string goTo)
		{
			return (this.spawnedEntity != null || this.ContinueIfFailedToSpawn) && this.spawned;
		}

		// Token: 0x0600205F RID: 8287 RVA: 0x000DB43C File Offset: 0x000D963C
		public override void Reset()
		{
			this.spawned = false;
			this.spawnedEntity = null;
		}

		// Token: 0x06002060 RID: 8288 RVA: 0x000DB44C File Offset: 0x000D964C
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
									newCharacter.LoadTalents();
									GameMain.NetworkMember.CreateEntityEvent(newCharacter, default(Character.UpdateTalentsEventData));
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

		// Token: 0x06002061 RID: 8289 RVA: 0x000DB8EC File Offset: 0x000D9AEC
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

		// Token: 0x06002062 RID: 8290 RVA: 0x000DB980 File Offset: 0x000D9B80
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

		// Token: 0x06002063 RID: 8291 RVA: 0x000DBA84 File Offset: 0x000D9C84
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

		// Token: 0x06002064 RID: 8292 RVA: 0x000DBB50 File Offset: 0x000D9D50
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

		// Token: 0x06002065 RID: 8293 RVA: 0x000DBBAC File Offset: 0x000D9DAC
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

		// Token: 0x06002066 RID: 8294 RVA: 0x000DBC1C File Offset: 0x000D9E1C
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

		// Token: 0x06002067 RID: 8295 RVA: 0x000DC238 File Offset: 0x000DA438
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

		// Token: 0x06002068 RID: 8296 RVA: 0x000DC2AC File Offset: 0x000DA4AC
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

		// Token: 0x0600206D RID: 8301 RVA: 0x000DC44C File Offset: 0x000DA64C
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

		// Token: 0x04000F55 RID: 3925
		private readonly HashSet<Identifier> targetModuleTags = new HashSet<Identifier>();

		// Token: 0x04000F5D RID: 3933
		private bool spawned;

		// Token: 0x04000F5E RID: 3934
		private Entity spawnedEntity;

		// Token: 0x04000F5F RID: 3935
		private readonly bool ignoreSpawnPointType;

		// Token: 0x02000922 RID: 2338
		public enum SpawnLocationType
		{
			// Token: 0x0400321E RID: 12830
			Any,
			// Token: 0x0400321F RID: 12831
			MainSub,
			// Token: 0x04003220 RID: 12832
			Outpost,
			// Token: 0x04003221 RID: 12833
			MainPath,
			// Token: 0x04003222 RID: 12834
			Cave,
			// Token: 0x04003223 RID: 12835
			AbyssCave,
			// Token: 0x04003224 RID: 12836
			Ruin,
			// Token: 0x04003225 RID: 12837
			Wreck,
			// Token: 0x04003226 RID: 12838
			BeaconStation,
			// Token: 0x04003227 RID: 12839
			NearMainSub
		}
	}
}
