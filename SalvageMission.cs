using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005E RID: 94
	internal class SalvageMission : Mission
	{
		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06000CD2 RID: 3282 RVA: 0x00075633 File Offset: 0x00073833
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x06000CD3 RID: 3283 RVA: 0x00075636 File Offset: 0x00073836
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x00075639 File Offset: 0x00073839
		private void TryShowPickedUpMessage()
		{
			this.HandleMessage(ref this.pickedUpMessage);
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00075647 File Offset: 0x00073847
		private void TryShowRetrievedMessage()
		{
			if (this.DetermineCompleted(CampaignMode.TransitionType.None))
			{
				this.HandleMessage(ref this.allRetrievedMessage);
				return;
			}
			this.HandleMessage(ref this.partiallyRetrievedMessage);
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x0007566B File Offset: 0x0007386B
		private void HandleMessage(ref LocalizedString message)
		{
			if (!message.IsNullOrEmpty())
			{
				base.CreateMessageBox(string.Empty, message);
			}
			message = string.Empty;
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x00075694 File Offset: 0x00073894
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			byte characterCount = msg.ReadByte();
			for (int i = 0; i < (int)characterCount; i++)
			{
				Character character = Character.ReadSpawnData(msg);
				this.characters.Add(character);
				ushort itemCount = msg.ReadUInt16();
				for (int j = 0; j < (int)itemCount; j++)
				{
					Item.ReadSpawnData(msg, true);
				}
			}
			if (this.characters.Contains(null))
			{
				throw new Exception("Error in SalvageMission.ClientReadInitial: character list contains null (mission: " + this.Prefab.Identifier.ToString() + ")");
			}
			if (this.characters.Count != (int)characterCount)
			{
				string[] array = new string[7];
				array[0] = "Error in SalvageMission.ClientReadInitial: character count does not match the server count (";
				int num = 1;
				List<Character> characters = this.characters;
				array[num] = ((characters != null) ? characters.ToString() : null);
				array[2] = " != ";
				array[3] = this.characters.Count.ToString();
				array[4] = "mission: ";
				array[5] = this.Prefab.Identifier.ToString();
				array[6] = ")";
				throw new Exception(string.Concat(array));
			}
			foreach (SalvageMission.Target target in this.targets)
			{
				bool targetFound = msg.ReadBoolean();
				if (targetFound)
				{
					bool usedExistingItem = msg.ReadBoolean();
					if (usedExistingItem)
					{
						ushort id = msg.ReadUInt16();
						target.Item = (Entity.FindEntityByID(id) as Item);
						if (target.Item == null)
						{
							throw new Exception(string.Concat(new string[]
							{
								"Error in SalvageMission.ClientReadInitial: failed to find item ",
								id.ToString(),
								" (mission: ",
								this.Prefab.Identifier.ToString(),
								")"
							}));
						}
					}
					else
					{
						target.Item = Item.ReadSpawnData(msg, true);
						target.Item.HighlightColor = new Color?(GUIStyle.Orange);
						target.Item.ExternalHighlight = true;
						ushort parentTargetId = msg.ReadUInt16();
						if (parentTargetId != 0)
						{
							target.OriginalContainer = (Entity.FindEntityByID(parentTargetId) as Item);
						}
						if (target.Item == null)
						{
							throw new Exception("Error in SalvageMission.ClientReadInitial: spawned item was null (mission: " + this.Prefab.Identifier.ToString() + ")");
						}
					}
					int executedEffectCount = (int)msg.ReadByte();
					for (int k = 0; k < executedEffectCount; k++)
					{
						int listIndex = (int)msg.ReadByte();
						int effectIndex = (int)msg.ReadByte();
						StatusEffect selectedEffect = target.StatusEffects[listIndex][effectIndex];
						target.Item.ApplyStatusEffect(selectedEffect, selectedEffect.type, 1f, null, null, null, false, true, new Vector2?(target.Item.Position));
					}
					if (target.Item.body != null && target.Item.CurrentHull == null)
					{
						target.Item.body.FarseerBody.BodyType = BodyType.Kinematic;
					}
				}
			}
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x000759B8 File Offset: 0x00073BB8
		public override void ClientRead(IReadMessage msg)
		{
			base.ClientRead(msg);
			bool atLeastOneTargetWasRetrieved = false;
			bool showPickedUpMsg = false;
			int targetCount = (int)msg.ReadByte();
			for (int i = 0; i < targetCount; i++)
			{
				SalvageMission.Target.RetrievalState state = (SalvageMission.Target.RetrievalState)msg.ReadByte();
				if (i < this.targets.Count)
				{
					SalvageMission.Target target = this.targets[i];
					bool wasRetrieved = target.Retrieved;
					bool wasPickedUp = target.State == SalvageMission.Target.RetrievalState.PickedUp;
					this.targets[i].State = state;
					if (!wasRetrieved && target.Retrieved)
					{
						atLeastOneTargetWasRetrieved = true;
					}
					else if (!wasPickedUp && target.State == SalvageMission.Target.RetrievalState.PickedUp)
					{
						showPickedUpMsg = true;
					}
				}
			}
			if (atLeastOneTargetWasRetrieved)
			{
				this.TryShowRetrievedMessage();
			}
			if (showPickedUpMsg)
			{
				this.TryShowPickedUpMessage();
			}
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x06000CD9 RID: 3289 RVA: 0x00075A64 File Offset: 0x00073C64
		public override IEnumerable<Entity> HudIconTargets
		{
			get
			{
				return from t in this.targets.Where(delegate(SalvageMission.Target t)
				{
					if (t.Item != null && !t.Retrieved)
					{
						Item item = t.Item;
						Character character = ((item != null) ? item.GetRootInventoryOwner() : null) as Character;
						return character == null || !character.IsLocalPlayer;
					}
					return false;
				})
				select t.Item;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06000CDA RID: 3290 RVA: 0x00075ABF File Offset: 0x00073CBF
		public bool AnyTargetNeedsToBeRetrievedToSub
		{
			get
			{
				return this.targets.Any((SalvageMission.Target t) => t.RequiredRetrievalState == SalvageMission.Target.RetrievalState.RetrievedToSub && !t.Retrieved);
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06000CDB RID: 3291 RVA: 0x00075AEC File Offset: 0x00073CEC
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				SalvageMission.<get_SonarLabels>d__22 <get_SonarLabels>d__ = new SalvageMission.<get_SonarLabels>d__22(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x00075B0C File Offset: 0x00073D0C
		public SalvageMission(MissionPrefab prefab, Location[] locations, Submarine sub)
		{
			SalvageMission.<>c__DisplayClass23_0 CS$<>8__locals1;
			CS$<>8__locals1.prefab = prefab;
			CS$<>8__locals1.sub = sub;
			base..ctor(CS$<>8__locals1.prefab, locations, CS$<>8__locals1.sub);
			CS$<>8__locals1.<>4__this = this;
			this.requiredDeliveryAmount = CS$<>8__locals1.prefab.ConfigElement.GetAttributeFloat("requiredDeliveryAmount", 0.98f);
			LevelData levelData = locations[0].LevelData;
			string str;
			if ((str = ((levelData != null) ? levelData.Seed : null)) == null)
			{
				Identifier identifier = locations[0].NameIdentifier;
				string value = identifier.Value;
				LevelData levelData2 = locations[1].LevelData;
				str = value + ((levelData2 != null) ? levelData2.Seed : null);
			}
			this.rng = new MTRandom(ToolBox.StringToInt(str));
			this.wreckTags = CS$<>8__locals1.prefab.ConfigElement.GetAttributeIdentifierArray("wrecktags", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.partiallyRetrievedMessage = this.<.ctor>g__GetMessage|23_0("partiallyRetrievedMessage", ref CS$<>8__locals1);
			this.allRetrievedMessage = this.<.ctor>g__GetMessage|23_0("allRetrievedMessage", ref CS$<>8__locals1);
			this.pickedUpMessage = this.<.ctor>g__GetMessage|23_0("pickedUpMessage", ref CS$<>8__locals1);
			foreach (ContentXElement subElement in CS$<>8__locals1.prefab.ConfigElement.Elements())
			{
				Identifier identifier = subElement.NameAsIdentifier();
				if (!(identifier == "target"))
				{
					Identifier identifier2 = subElement.NameAsIdentifier();
					if (!(identifier2 == "chooserandom"))
					{
						continue;
					}
				}
				this.LoadTarget(subElement, null);
			}
			if (!this.targets.Any<SalvageMission.Target>())
			{
				this.targets.Add(new SalvageMission.Target(CS$<>8__locals1.prefab.ConfigElement, this, null));
			}
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x00075CC4 File Offset: 0x00073EC4
		private void LoadTarget(ContentXElement element, SalvageMission.Target parentTarget)
		{
			ContentXElement chosenElement = element;
			Identifier identifier = element.NameAsIdentifier();
			if (identifier == "chooserandom")
			{
				if (element.Elements().Any(delegate(ContentXElement e)
				{
					Identifier identifier3 = e.NameAsIdentifier();
					return identifier3 == "statuseffect";
				}))
				{
					return;
				}
				chosenElement = element.Elements().ToList<ContentXElement>().GetRandom(this.rng);
			}
			int amount = this.GetAmount(chosenElement);
			for (int i = 0; i < amount; i++)
			{
				SalvageMission.Target target = new SalvageMission.Target(chosenElement, this, parentTarget);
				this.targets.Add(target);
				foreach (ContentXElement subElement in chosenElement.Elements())
				{
					identifier = subElement.NameAsIdentifier();
					if (!(identifier == "target"))
					{
						Identifier identifier2 = subElement.NameAsIdentifier();
						if (!(identifier2 == "chooserandom"))
						{
							continue;
						}
					}
					this.LoadTarget(subElement, target);
				}
			}
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x00075DD4 File Offset: 0x00073FD4
		private int GetAmount(ContentXElement targetElement)
		{
			int amount = targetElement.GetAttributeInt("amount", 1);
			int minAmount = targetElement.GetAttributeInt("minamount", amount);
			int maxAmount = targetElement.GetAttributeInt("maxamount", amount);
			if (minAmount < maxAmount)
			{
				amount = this.rng.Next(minAmount, maxAmount + 1);
			}
			return amount;
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x00075E20 File Offset: 0x00074020
		protected override void StartMissionSpecific(Level level)
		{
			if (!Mission.IsClient)
			{
				SalvageMission.Target firstTarget = this.targets.First<SalvageMission.Target>();
				Submarine submarine = Submarine.Loaded.Find((Submarine s) => SalvageMission.IsValidSubmarine(s, firstTarget.SpawnPositionType));
				if (submarine != null)
				{
					base.InitCharacters(submarine);
				}
			}
			using (List<SalvageMission.Target>.Enumerator enumerator = this.targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SalvageMission.Target target = enumerator.Current;
					List<ValueTuple<int, int>> executedEffectIndices = new List<ValueTuple<int, int>>();
					target.Reset();
					if (!Mission.IsClient)
					{
						Level.PositionType spawnPositionType = target.SpawnPositionType;
						if (spawnPositionType <= Level.PositionType.Ruin)
						{
							if (spawnPositionType != Level.PositionType.Cave && spawnPositionType != Level.PositionType.Ruin)
							{
								goto IL_DD;
							}
							goto IL_D4;
						}
						else
						{
							if (spawnPositionType == Level.PositionType.Wreck || spawnPositionType == Level.PositionType.Outpost)
							{
								goto IL_D4;
							}
							goto IL_DD;
						}
						IL_F5:
						float num;
						float minDistance = num;
						Vector2 position = (target.SpawnPositionType == Level.PositionType.None) ? Vector2.Zero : Level.Loaded.GetRandomItemPos(target.SpawnPositionType, 100f, minDistance, 30f, null);
						if (!target.ExistingItemTag.IsEmpty)
						{
							IEnumerable<Item> suitableItems = from it in Item.ItemList
							where it.HasTag(target.ExistingItemTag)
							select it;
							GameSession gameSession = GameMain.GameSession;
							if (((gameSession != null) ? gameSession.Missions : null) != null)
							{
								suitableItems = suitableItems.Where(delegate(Item it)
								{
									Func<SalvageMission.Target, bool> <>9__4;
									return GameMain.GameSession.Missions.None(delegate(Mission m)
									{
										if (m != this)
										{
											SalvageMission salvageMission = m as SalvageMission;
											if (salvageMission != null)
											{
												IEnumerable<SalvageMission.Target> source = salvageMission.targets;
												Func<SalvageMission.Target, bool> predicate;
												if ((predicate = <>9__4) == null)
												{
													predicate = (<>9__4 = ((SalvageMission.Target t) => t.Item == it));
												}
												return source.Any(predicate);
											}
										}
										return false;
									});
								});
							}
							Level.PositionType spawnPositionType2 = target.SpawnPositionType;
							if (spawnPositionType2 <= Level.PositionType.Wreck)
							{
								if (spawnPositionType2 <= Level.PositionType.Cave)
								{
									if (spawnPositionType2 - Level.PositionType.MainPath > 1 && spawnPositionType2 != Level.PositionType.Cave)
									{
										goto IL_328;
									}
								}
								else
								{
									if (spawnPositionType2 != Level.PositionType.Ruin && spawnPositionType2 != Level.PositionType.Wreck)
									{
										goto IL_328;
									}
									goto IL_27F;
								}
							}
							else if (spawnPositionType2 <= Level.PositionType.Abyss)
							{
								if (spawnPositionType2 == Level.PositionType.BeaconStation)
								{
									goto IL_27F;
								}
								if (spawnPositionType2 != Level.PositionType.Abyss)
								{
									goto IL_328;
								}
								target.Item = suitableItems.FirstOrDefault((Item it) => Level.IsPositionInAbyss(it.WorldPosition));
								goto IL_340;
							}
							else if (spawnPositionType2 != Level.PositionType.AbyssCave)
							{
								if (spawnPositionType2 != Level.PositionType.Outpost)
								{
									goto IL_328;
								}
								goto IL_27F;
							}
							target.Item = suitableItems.FirstOrDefault((Item it) => Vector2.DistanceSquared(it.WorldPosition, position) < 1000f);
							goto IL_340;
							IL_27F:
							using (IEnumerator<Item> enumerator2 = suitableItems.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									Item it3 = enumerator2.Current;
									Submarine sub = it3.Submarine;
									if (sub != null && SalvageMission.IsValidSubmarine(sub, target.SpawnPositionType))
									{
										Rectangle worldBorders = sub.Borders;
										worldBorders.Location += sub.WorldPosition.ToPoint();
										if (Submarine.RectContains(worldBorders, it3.WorldPosition, false))
										{
											target.Item = it3;
											break;
										}
									}
								}
								goto IL_340;
							}
							IL_328:
							target.Item = suitableItems.FirstOrDefault<Item>();
						}
						IL_340:
						if (target.Item == null)
						{
							if (target.ItemPrefab == null && target.ContainerTag.IsEmpty)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Failed to find a target item for the mission \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral("\". Item tag: ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(target.ExistingItemTag);
								DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
								continue;
							}
							target.Item = new Item(target.ItemPrefab, position, null, 0, true);
							target.Item.HighlightColor = new Color?(GUIStyle.Orange);
							target.Item.ExternalHighlight = true;
							target.Item.UpdateTransform();
							if (target.Item.CurrentHull == null && target.Item.body != null)
							{
								target.Item.body.FarseerBody.BodyType = BodyType.Kinematic;
							}
						}
						if (target.RequiredRetrievalState == SalvageMission.Target.RetrievalState.Interact)
						{
							Item item = target.Item;
							item.OnInteract = (Action)Delegate.Combine(item.OnInteract, new Action(delegate()
							{
								target.Interacted = true;
							}));
						}
						for (int i = 0; i < target.StatusEffects.Count; i++)
						{
							List<StatusEffect> effectList = target.StatusEffects[i];
							if (effectList.Count != 0)
							{
								int effectIndex = Rand.Int(effectList.Count, Rand.RandSync.Unsynced);
								StatusEffect selectedEffect = effectList[effectIndex];
								target.Item.ApplyStatusEffect(selectedEffect, selectedEffect.type, 1f, null, null, null, false, true, new Vector2?(target.Item.Position));
							}
						}
						target.Item.IsSalvageMissionItem = true;
						if (target.ContainerTag.IsEmpty || target.Item.ParentInventory != null)
						{
							continue;
						}
						List<ItemContainer> validContainers = new List<ItemContainer>();
						foreach (Item it2 in Item.ItemList)
						{
							if (it2.HasTag(target.ContainerTag) && it2.IsPlayerTeamInteractable && SalvageMission.IsValidSubmarine(it2.Submarine, target.SpawnPositionType))
							{
								ItemContainer itemContainer = it2.GetComponent<ItemContainer>();
								if (itemContainer != null && itemContainer.Inventory.CanBePut(target.Item))
								{
									validContainers.Add(itemContainer);
								}
							}
						}
						if (validContainers.Any<ItemContainer>())
						{
							ItemContainer selectedContainer = validContainers.GetRandomUnsynced<ItemContainer>();
							selectedContainer.Combine(target.Item, null);
							continue;
						}
						continue;
						IL_DD:
						num = (float)Level.Loaded.Size.X * 0.3f;
						goto IL_F5;
						IL_D4:
						num = 0f;
						goto IL_F5;
					}
				}
			}
			if (!Mission.IsClient)
			{
				foreach (SalvageMission.Target target2 in this.targets)
				{
					if (target2.ParentTarget != null)
					{
						if (target2.Item == null)
						{
							DebugConsole.ThrowError("Error in salvage mission " + this.Prefab.Identifier.ToString() + " (item was null)", null, this.Prefab.ContentPackage, false, false);
						}
						else if (target2.ParentTarget.Item == null)
						{
							DebugConsole.ThrowError("Error in salvage mission " + this.Prefab.Identifier.ToString() + " (parent item was null)", null, this.Prefab.ContentPackage, false, false);
						}
						else
						{
							target2.Item.DontCleanUp = true;
							ItemContainer container = target2.ParentTarget.Item.GetComponent<ItemContainer>();
							if (container != null)
							{
								if (!container.Inventory.TryPutItem(target2.Item, null, null, true, false, true))
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(59, 3);
									defaultInterpolatedStringHandler2.AppendLiteral("Error in salvage mission ");
									defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
									defaultInterpolatedStringHandler2.AppendLiteral(": failed to put the item ");
									defaultInterpolatedStringHandler2.AppendFormatted(target2.Item.Name);
									defaultInterpolatedStringHandler2.AppendLiteral(" inside ");
									defaultInterpolatedStringHandler2.AppendFormatted(target2.ParentTarget.Item.Name);
									defaultInterpolatedStringHandler2.AppendLiteral(".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
									target2.PlacingInsideParentTargetFailed = true;
								}
								target2.OriginalContainer = target2.ParentTarget.Item;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x00076764 File Offset: 0x00074964
		private static bool IsValidSubmarine(Submarine sub, Level.PositionType spawnPosType)
		{
			if (sub == null)
			{
				if (spawnPosType <= Level.PositionType.Wreck)
				{
					if (spawnPosType != Level.PositionType.Ruin && spawnPosType != Level.PositionType.Wreck)
					{
						goto IL_24;
					}
				}
				else if (spawnPosType != Level.PositionType.BeaconStation && spawnPosType != Level.PositionType.Outpost)
				{
					goto IL_24;
				}
				return false;
				IL_24:
				return true;
			}
			if (spawnPosType <= Level.PositionType.Wreck)
			{
				if (spawnPosType == Level.PositionType.Ruin)
				{
					return sub.Info.IsRuin;
				}
				if (spawnPosType == Level.PositionType.Wreck)
				{
					return sub.Info.IsWreck;
				}
			}
			else
			{
				if (spawnPosType == Level.PositionType.BeaconStation)
				{
					return sub.Info.IsBeacon;
				}
				if (spawnPosType == Level.PositionType.Outpost)
				{
					return sub.Info.IsOutpost;
				}
			}
			return false;
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x000767F4 File Offset: 0x000749F4
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			SalvageMission.<>c__DisplayClass28_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			foreach (SalvageMission.Target target in this.targets)
			{
				Item item = target.Item;
				Item root = ((item != null) ? item.RootContainer : null) ?? target.Item;
				if (root != null && target.Item.ParentInventory != null && target.Item.body != null)
				{
					target.Item.body.FarseerBody.BodyType = BodyType.Dynamic;
				}
			}
			if (Mission.IsClient)
			{
				return;
			}
			CS$<>8__locals1.atLeastOneTargetWasRetrieved = false;
			SalvageMission.<>c__DisplayClass28_1 CS$<>8__locals2;
			CS$<>8__locals2.i = 0;
			while (CS$<>8__locals2.i < this.targets.Count)
			{
				SalvageMission.<>c__DisplayClass28_2 CS$<>8__locals3;
				CS$<>8__locals3.target = this.targets[CS$<>8__locals2.i];
				if (CS$<>8__locals2.i > 0 && !this.targets[CS$<>8__locals2.i - 1].AllowContinueBeforeRetrieved && !this.targets[CS$<>8__locals2.i - 1].Retrieved)
				{
					break;
				}
				if (CS$<>8__locals3.target.Item == null)
				{
					return;
				}
				Entity rootInventoryOwner = CS$<>8__locals3.target.Item.GetRootInventoryOwner();
				Hull currentHull = CS$<>8__locals3.target.Item.CurrentHull;
				Submarine parentSub = ((currentHull != null) ? currentHull.Submarine : null) ?? ((rootInventoryOwner != null) ? rootInventoryOwner.Submarine : null);
				bool inPlayerSub = parentSub != null && parentSub.Info.Type == SubmarineType.Player;
				SalvageMission.Target.RetrievalState state = CS$<>8__locals3.target.State;
				if (state != SalvageMission.Target.RetrievalState.None)
				{
					if (state - SalvageMission.Target.RetrievalState.PickedUp <= 1)
					{
						bool inPlayerInventory = false;
						bool playerInFriendlySub = false;
						Character character = rootInventoryOwner as Character;
						if (character != null && character.TeamID == CharacterTeamType.Team1)
						{
							inPlayerInventory = true;
							if (character.Submarine != null)
							{
								bool flag;
								if (!character.IsInFriendlySub)
								{
									Submarine submarine = character.Submarine;
									Level loaded = Level.Loaded;
									if (submarine == ((loaded != null) ? loaded.StartOutpost : null) && Level.IsLoadedFriendlyOutpost)
									{
										GameSession gameSession = GameMain.GameSession;
										Location location = (gameSession != null) ? gameSession.Campaign.CurrentLocation : null;
										flag = (location == null || !location.IsFactionHostile);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									flag = true;
								}
								playerInFriendlySub = flag;
							}
						}
						if (inPlayerSub || (inPlayerInventory && playerInFriendlySub))
						{
							this.<UpdateMissionSpecific>g__TrySetRetrievalState|28_1(SalvageMission.Target.RetrievalState.RetrievedToSub, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
						}
						else
						{
							CS$<>8__locals3.target.State = SalvageMission.Target.RetrievalState.PickedUp;
						}
					}
				}
				else
				{
					if (CS$<>8__locals3.target.Interacted)
					{
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|28_1(SalvageMission.Target.RetrievalState.Interact, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
					Item item2 = CS$<>8__locals3.target.Item;
					Item root2 = ((item2 != null) ? item2.RootContainer : null) ?? CS$<>8__locals3.target.Item;
					Inventory parentInventory = root2.ParentInventory;
					Character character2 = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
					if (character2 != null && character2.TeamID == CharacterTeamType.Team1)
					{
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|28_1(SalvageMission.Target.RetrievalState.PickedUp, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
						this.TryShowPickedUpMessage();
					}
					if (inPlayerSub)
					{
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|28_1(SalvageMission.Target.RetrievalState.RetrievedToSub, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
				}
				int i = CS$<>8__locals2.i;
				CS$<>8__locals2.i = i + 1;
			}
			if (CS$<>8__locals1.atLeastOneTargetWasRetrieved)
			{
				this.TryShowRetrievedMessage();
			}
			if (this.targets.All((SalvageMission.Target t) => t.Retrieved))
			{
				this.State = this.targets.Count + 1;
			}
		}

		// Token: 0x06000CE2 RID: 3298 RVA: 0x00076B5C File Offset: 0x00074D5C
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			if (this.requiredDeliveryAmount < 1f)
			{
				IEnumerable<SalvageMission.Target> source = this.targets;
				Func<SalvageMission.Target, bool> predicate;
				if ((predicate = SalvageMission.<>O.<0>__IsTargetRetrieved) == null)
				{
					predicate = (SalvageMission.<>O.<0>__IsTargetRetrieved = new Func<SalvageMission.Target, bool>(SalvageMission.<DetermineCompleted>g__IsTargetRetrieved|29_0));
				}
				return (float)source.Count(predicate) / (float)this.targets.Count >= this.requiredDeliveryAmount;
			}
			IEnumerable<SalvageMission.Target> source2 = this.targets;
			Func<SalvageMission.Target, bool> predicate2;
			if ((predicate2 = SalvageMission.<>O.<0>__IsTargetRetrieved) == null)
			{
				predicate2 = (SalvageMission.<>O.<0>__IsTargetRetrieved = new Func<SalvageMission.Target, bool>(SalvageMission.<DetermineCompleted>g__IsTargetRetrieved|29_0));
			}
			return source2.All(predicate2);
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x00076BDC File Offset: 0x00074DDC
		protected override void EndMissionSpecific(bool completed)
		{
			bool failed;
			if (!completed)
			{
				failed = this.targets.Any((SalvageMission.Target t) => t.State >= SalvageMission.Target.RetrievalState.PickedUp);
			}
			else
			{
				failed = false;
			}
			this.failed = failed;
			List<SalvageMission.Target> targetsToRemove = new List<SalvageMission.Target>();
			using (List<SalvageMission.Target>.Enumerator enumerator = this.targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SalvageMission.Target target = enumerator.Current;
					if (target.RemoveItem || this.targets.Any(delegate(SalvageMission.Target t)
					{
						if (t.RemoveItem)
						{
							Item item2 = target.Item;
							object obj;
							if (item2 == null)
							{
								obj = null;
							}
							else
							{
								Inventory parentInventory = item2.ParentInventory;
								obj = ((parentInventory != null) ? parentInventory.Owner : null);
							}
							return obj as Item == t.Item;
						}
						return false;
					}))
					{
						targetsToRemove.Add(target);
					}
				}
			}
			foreach (SalvageMission.Target target2 in targetsToRemove)
			{
				Item item = target2.Item;
				if (item != null && !item.Removed)
				{
					target2.Item.Remove();
				}
				target2.Reset();
			}
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x00076D04 File Offset: 0x00074F04
		public override void AdjustLevelData(LevelData levelData)
		{
			if (this.wreckTags.Length > 0)
			{
				SubmarineInfo selectedWreck = SalvageMission.GetRandomWreckByTags(this.wreckTags, levelData);
				if (selectedWreck != null)
				{
					levelData.ForceWreck = selectedWreck;
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Salvage mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" could not find a suitable wreck with wrecktags \"");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join<Identifier>(", ", this.wreckTags));
				defaultInterpolatedStringHandler.AppendLiteral("\" for level difficulty ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(levelData.Difficulty, "F1");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x00076DCE File Offset: 0x00074FCE
		private static SubmarineInfo GetRandomWreckByTags(ImmutableArray<Identifier> tags, LevelData levelData)
		{
			return Mission.GetRandomSubmarineByTagsAndDifficulty(tags, levelData, (SubmarineInfo s) => s.IsWreck, "wreck");
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x00076E00 File Offset: 0x00075000
		[CompilerGenerated]
		private LocalizedString <.ctor>g__GetMessage|23_0(string attributeName, ref SalvageMission.<>c__DisplayClass23_0 A_2)
		{
			if (A_2.prefab.ConfigElement.GetAttribute(attributeName) != null)
			{
				string msgTag = A_2.prefab.ConfigElement.GetAttributeString(attributeName, string.Empty);
				return base.ReplaceVariablesInMissionMessage(TextManager.Get(msgTag).Fallback(msgTag, true), A_2.sub, true);
			}
			return string.Empty;
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x00076EA0 File Offset: 0x000750A0
		[CompilerGenerated]
		private void <UpdateMissionSpecific>g__TrySetRetrievalState|28_1(SalvageMission.Target.RetrievalState retrievalState, ref SalvageMission.<>c__DisplayClass28_0 A_2, ref SalvageMission.<>c__DisplayClass28_1 A_3, ref SalvageMission.<>c__DisplayClass28_2 A_4)
		{
			if (retrievalState < A_4.target.State || A_4.target.State == retrievalState)
			{
				return;
			}
			bool wasRetrieved = A_4.target.Retrieved;
			A_4.target.State = retrievalState;
			if (!wasRetrieved && A_4.target.Retrieved)
			{
				this.State = Math.Max(A_3.i + 1, this.State);
				A_2.atLeastOneTargetWasRetrieved = true;
			}
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x00076F17 File Offset: 0x00075117
		[CompilerGenerated]
		internal static bool <DetermineCompleted>g__IsTargetRetrieved|29_0(SalvageMission.Target target)
		{
			if (target.State < target.RequiredRetrievalState)
			{
				return false;
			}
			if (target.RequireInsideOriginalContainer)
			{
				ItemInventory parentInventory = target.Item.ParentInventory;
				Item originalContainer = target.OriginalContainer;
				if (parentInventory != ((originalContainer != null) ? originalContainer.OwnInventory : null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x040006A7 RID: 1703
		private readonly List<SalvageMission.Target> targets = new List<SalvageMission.Target>();

		// Token: 0x040006A8 RID: 1704
		private readonly float requiredDeliveryAmount;

		// Token: 0x040006A9 RID: 1705
		private readonly ImmutableArray<Identifier> wreckTags;

		// Token: 0x040006AA RID: 1706
		private LocalizedString pickedUpMessage;

		// Token: 0x040006AB RID: 1707
		private LocalizedString partiallyRetrievedMessage;

		// Token: 0x040006AC RID: 1708
		private LocalizedString allRetrievedMessage;

		// Token: 0x040006AD RID: 1709
		private readonly MTRandom rng;

		// Token: 0x02000806 RID: 2054
		private class Target
		{
			// Token: 0x17001A10 RID: 6672
			// (get) Token: 0x06006C8D RID: 27789 RVA: 0x0035F3EC File Offset: 0x0035D5EC
			public bool Retrieved
			{
				get
				{
					if (this.PlacingInsideParentTargetFailed)
					{
						return true;
					}
					bool result;
					switch (this.RequiredRetrievalState)
					{
					case SalvageMission.Target.RetrievalState.None:
						result = true;
						break;
					case SalvageMission.Target.RetrievalState.Interact:
					case SalvageMission.Target.RetrievalState.PickedUp:
						result = (this.State >= this.RequiredRetrievalState);
						break;
					case SalvageMission.Target.RetrievalState.RetrievedToSub:
						result = (this.State == SalvageMission.Target.RetrievalState.RetrievedToSub);
						break;
					default:
						throw new NotImplementedException();
					}
					return result;
				}
			}

			// Token: 0x17001A11 RID: 6673
			// (get) Token: 0x06006C8E RID: 27790 RVA: 0x0035F44D File Offset: 0x0035D64D
			// (set) Token: 0x06006C8F RID: 27791 RVA: 0x0035F458 File Offset: 0x0035D658
			public SalvageMission.Target.RetrievalState State
			{
				get
				{
					return this.state;
				}
				set
				{
					if (value == this.state)
					{
						return;
					}
					bool wasRetrieved = this.Retrieved;
					this.state = value;
					if (!wasRetrieved && this.Retrieved)
					{
						this.OnTargetRetrieved();
						return;
					}
					if (this.state == SalvageMission.Target.RetrievalState.PickedUp)
					{
						this.OnTargetPickedUp();
					}
				}
			}

			// Token: 0x06006C90 RID: 27792 RVA: 0x0035F49E File Offset: 0x0035D69E
			private void OnTargetRetrieved()
			{
				if (this.Item == null)
				{
					return;
				}
				SteamTimelineManager.OnMissionTargetRetrieved(this.Item, this.mission);
			}

			// Token: 0x06006C91 RID: 27793 RVA: 0x0035F4BA File Offset: 0x0035D6BA
			private void OnTargetPickedUp()
			{
				if (this.Item == null)
				{
					return;
				}
				SteamTimelineManager.OnMissionTargetPickedUp(this.Item, this.mission);
			}

			// Token: 0x06006C92 RID: 27794 RVA: 0x0035F4D8 File Offset: 0x0035D6D8
			public Target(ContentXElement element, SalvageMission mission, SalvageMission.Target parentTarget)
			{
				this.mission = mission;
				this.ParentTarget = parentTarget;
				this.ContainerTag = element.GetAttributeIdentifier("containertag", Identifier.Empty);
				string key = "requireretrieval";
				SalvageMission.Target.RetrievalState retrievalState = (parentTarget != null) ? parentTarget.RequiredRetrievalState : SalvageMission.Target.RetrievalState.RetrievedToSub;
				this.RequiredRetrievalState = element.GetAttributeEnum<SalvageMission.Target.RetrievalState>(key, retrievalState);
				this.AllowContinueBeforeRetrieved = element.GetAttributeBool("allowcontinuebeforeretrieved", parentTarget != null);
				this.HideLabelAfterRetrieved = element.GetAttributeBool("hidelabelafterretrieved", parentTarget != null && parentTarget.HideLabelAfterRetrieved);
				this.HideLabelWhenFound = element.GetAttributeBool("HideLabelWhenFound", parentTarget != null && parentTarget.HideLabelWhenFound);
				this.HideLabelWhenNotFound = element.GetAttributeBool("HideLabelWhenNotFound", parentTarget != null && parentTarget.HideLabelWhenNotFound);
				this.PointToSub = element.GetAttributeBool("PointToSub", parentTarget != null && parentTarget.PointToSub);
				this.RequireInsideOriginalContainer = element.GetAttributeBool("requireinsideoriginalcontainer", false);
				string sonarLabelTag = element.GetAttributeString("sonarlabel", "");
				if (!string.IsNullOrEmpty(sonarLabelTag))
				{
					this.SonarLabel = TextManager.Get("MissionSonarLabel." + sonarLabelTag).Fallback(TextManager.Get(sonarLabelTag), true).Fallback(element.GetAttributeString("sonarlabel", ""), true);
				}
				this.ExistingItemTag = element.GetAttributeIdentifier("existingitemtag", Identifier.Empty);
				this.RemoveItem = element.GetAttributeBool("removeitem", true);
				if (element.GetAttribute("itemname") != null)
				{
					DebugConsole.ThrowError("Error in SalvageMission - use item identifier instead of the name of the item.", null, element.ContentPackage, false, false);
					string itemName = element.GetAttributeString("itemname", "");
					this.ItemPrefab = (MapEntityPrefab.Find(itemName, null, true) as ItemPrefab);
					if (this.ItemPrefab == null && this.ExistingItemTag.IsEmpty)
					{
						DebugConsole.ThrowError("Error in SalvageMission: couldn't find an item prefab with the name \"" + itemName + "\"", null, element.ContentPackage, false, false);
					}
				}
				else
				{
					Identifier itemIdentifier = element.GetAttributeIdentifier("itemidentifier", Identifier.Empty);
					if (!itemIdentifier.IsEmpty)
					{
						this.ItemPrefab = (MapEntityPrefab.FindByIdentifier(itemIdentifier.ToIdentifier<Identifier>()) as ItemPrefab);
					}
					if (this.ItemPrefab == null)
					{
						string itemTag = element.GetAttributeString("itemtag", "");
						this.ItemPrefab = (MapEntityPrefab.GetRandom((MapEntityPrefab p) => p.Tags.Contains(itemTag), Rand.RandSync.Unsynced) as ItemPrefab);
					}
					if (this.ItemPrefab == null && this.ExistingItemTag.IsEmpty)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in SalvageMission - couldn't find an item prefab with the identifier \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(itemIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					}
				}
				string key2 = "spawntype";
				Level.PositionType positionType = (parentTarget != null) ? parentTarget.SpawnPositionType : (Level.PositionType.Cave | Level.PositionType.Ruin);
				this.SpawnPositionType = element.GetAttributeEnum<Level.PositionType>(key2, positionType);
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "statuseffect"))
					{
						if (a == "chooserandom")
						{
							if (subElement.Elements().Any(delegate(ContentXElement e)
							{
								Identifier identifier = e.NameAsIdentifier();
								return identifier == "statuseffect";
							}))
							{
								this.StatusEffects.Add(new List<StatusEffect>());
								foreach (ContentXElement effectElement in subElement.Elements())
								{
									StatusEffect newEffect = StatusEffect.Load(effectElement, mission.Prefab.Name.Value);
									if (newEffect != null)
									{
										this.StatusEffects.Last<List<StatusEffect>>().Add(newEffect);
									}
								}
							}
						}
					}
					else
					{
						StatusEffect newEffect2 = StatusEffect.Load(subElement, mission.Prefab.Name.Value);
						if (newEffect2 != null)
						{
							this.StatusEffects.Add(new List<StatusEffect>
							{
								newEffect2
							});
						}
					}
				}
			}

			// Token: 0x06006C93 RID: 27795 RVA: 0x0035F938 File Offset: 0x0035DB38
			public void Reset()
			{
				this.state = SalvageMission.Target.RetrievalState.None;
				this.Item = null;
			}

			// Token: 0x04003C63 RID: 15459
			public Item Item;

			// Token: 0x04003C64 RID: 15460
			public SalvageMission.Target ParentTarget;

			// Token: 0x04003C65 RID: 15461
			public readonly ItemPrefab ItemPrefab;

			// Token: 0x04003C66 RID: 15462
			public readonly Level.PositionType SpawnPositionType;

			// Token: 0x04003C67 RID: 15463
			public readonly Identifier ContainerTag;

			// Token: 0x04003C68 RID: 15464
			public readonly Identifier ExistingItemTag;

			// Token: 0x04003C69 RID: 15465
			public readonly bool PointToSub;

			// Token: 0x04003C6A RID: 15466
			public readonly bool RemoveItem;

			// Token: 0x04003C6B RID: 15467
			public readonly LocalizedString SonarLabel;

			// Token: 0x04003C6C RID: 15468
			public readonly bool AllowContinueBeforeRetrieved;

			// Token: 0x04003C6D RID: 15469
			public readonly SalvageMission.Target.RetrievalState RequiredRetrievalState;

			// Token: 0x04003C6E RID: 15470
			public readonly bool HideLabelAfterRetrieved;

			// Token: 0x04003C6F RID: 15471
			public readonly bool HideLabelWhenFound;

			// Token: 0x04003C70 RID: 15472
			public readonly bool HideLabelWhenNotFound;

			// Token: 0x04003C71 RID: 15473
			private SalvageMission.Target.RetrievalState state;

			// Token: 0x04003C72 RID: 15474
			public bool Interacted;

			// Token: 0x04003C73 RID: 15475
			private readonly SalvageMission mission;

			// Token: 0x04003C74 RID: 15476
			public readonly bool RequireInsideOriginalContainer;

			// Token: 0x04003C75 RID: 15477
			public Item OriginalContainer;

			// Token: 0x04003C76 RID: 15478
			public bool PlacingInsideParentTargetFailed;

			// Token: 0x04003C77 RID: 15479
			public readonly List<List<StatusEffect>> StatusEffects = new List<List<StatusEffect>>();

			// Token: 0x0200151E RID: 5406
			public enum RetrievalState
			{
				// Token: 0x0400677F RID: 26495
				None,
				// Token: 0x04006780 RID: 26496
				Interact,
				// Token: 0x04006781 RID: 26497
				PickedUp,
				// Token: 0x04006782 RID: 26498
				RetrievedToSub
			}
		}

		// Token: 0x02000807 RID: 2055
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003C78 RID: 15480
			public static Func<SalvageMission.Target, bool> <0>__IsTargetRetrieved;
		}
	}
}
