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
	// Token: 0x0200002B RID: 43
	internal class SalvageMission : Mission
	{
		// Token: 0x06000524 RID: 1316 RVA: 0x0002F1AC File Offset: 0x0002D3AC
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteByte((byte)this.characters.Count);
			foreach (Character character in this.characters)
			{
				character.WriteSpawnData(msg, character.ID, false);
				List<Item> items = this.characterItems[character];
				msg.WriteUInt16((ushort)items.Count);
				foreach (Item item in items)
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
			foreach (SalvageMission.Target target in this.targets)
			{
				SalvageMission.SpawnInfo sInfo;
				bool targetFound = this.spawnInfo.TryGetValue(target, out sInfo) && target.Item != null;
				msg.WriteBoolean(targetFound);
				if (targetFound)
				{
					msg.WriteBoolean(sInfo.UsedExistingItem);
					if (sInfo.UsedExistingItem)
					{
						msg.WriteUInt16(target.Item.ID);
					}
					else
					{
						target.Item.WriteSpawnData(msg, target.Item.ID, sInfo.OriginalInventoryID, sInfo.OriginalItemContainerIndex, sInfo.OriginalSlotIndex);
						SalvageMission.Target parentTarget = target.ParentTarget;
						ushort? num3;
						if (parentTarget == null)
						{
							num3 = null;
						}
						else
						{
							Item item3 = parentTarget.Item;
							num3 = ((item3 != null) ? new ushort?(item3.ID) : null);
						}
						ushort? num2 = num3;
						msg.WriteUInt16(num2.GetValueOrDefault());
					}
					msg.WriteByte((byte)sInfo.ExecutedEffectIndices.Count);
					foreach (ValueTuple<int, int> valueTuple in sInfo.ExecutedEffectIndices)
					{
						int listIndex = valueTuple.Item1;
						int effectIndex = valueTuple.Item2;
						msg.WriteByte((byte)listIndex);
						msg.WriteByte((byte)effectIndex);
					}
				}
			}
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0002F48C File Offset: 0x0002D68C
		public override void ServerWrite(IWriteMessage msg)
		{
			base.ServerWrite(msg);
			msg.WriteByte((byte)this.targets.Count);
			foreach (SalvageMission.Target t in this.targets)
			{
				msg.WriteByte((byte)t.State);
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000526 RID: 1318 RVA: 0x0002F500 File Offset: 0x0002D700
		public bool AnyTargetNeedsToBeRetrievedToSub
		{
			get
			{
				return this.targets.Any((SalvageMission.Target t) => t.RequiredRetrievalState == SalvageMission.Target.RetrievalState.RetrievedToSub && !t.Retrieved);
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x0002F52C File Offset: 0x0002D72C
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
				SalvageMission.<get_SonarLabels>d__15 <get_SonarLabels>d__ = new SalvageMission.<get_SonarLabels>d__15(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0002F54C File Offset: 0x0002D74C
		public SalvageMission(MissionPrefab prefab, Location[] locations, Submarine sub)
		{
			SalvageMission.<>c__DisplayClass16_0 CS$<>8__locals1;
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
			this.partiallyRetrievedMessage = this.<.ctor>g__GetMessage|16_0("partiallyRetrievedMessage", ref CS$<>8__locals1);
			this.allRetrievedMessage = this.<.ctor>g__GetMessage|16_0("allRetrievedMessage", ref CS$<>8__locals1);
			this.pickedUpMessage = this.<.ctor>g__GetMessage|16_0("pickedUpMessage", ref CS$<>8__locals1);
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

		// Token: 0x06000529 RID: 1321 RVA: 0x0002F710 File Offset: 0x0002D910
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

		// Token: 0x0600052A RID: 1322 RVA: 0x0002F820 File Offset: 0x0002DA20
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

		// Token: 0x0600052B RID: 1323 RVA: 0x0002F86C File Offset: 0x0002DA6C
		protected override void StartMissionSpecific(Level level)
		{
			this.spawnInfo.Clear();
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
					bool usedExistingItem = false;
					ushort originalInventoryID = 0;
					byte originalItemContainerIndex = 0;
					int originalSlotIndex = 0;
					List<ValueTuple<int, int>> executedEffectIndices = new List<ValueTuple<int, int>>();
					target.Reset();
					if (!Mission.IsClient)
					{
						Level.PositionType spawnPositionType = target.SpawnPositionType;
						if (spawnPositionType <= Level.PositionType.Ruin)
						{
							if (spawnPositionType != Level.PositionType.Cave && spawnPositionType != Level.PositionType.Ruin)
							{
								goto IL_E8;
							}
							goto IL_DF;
						}
						else
						{
							if (spawnPositionType == Level.PositionType.Wreck || spawnPositionType == Level.PositionType.Outpost)
							{
								goto IL_DF;
							}
							goto IL_E8;
						}
						IL_100:
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
										goto IL_333;
									}
									goto IL_224;
								}
								else
								{
									if (spawnPositionType2 != Level.PositionType.Ruin && spawnPositionType2 != Level.PositionType.Wreck)
									{
										goto IL_333;
									}
									goto IL_28A;
								}
							}
							else if (spawnPositionType2 <= Level.PositionType.Abyss)
							{
								if (spawnPositionType2 == Level.PositionType.BeaconStation)
								{
									goto IL_28A;
								}
								if (spawnPositionType2 != Level.PositionType.Abyss)
								{
									goto IL_333;
								}
								target.Item = suitableItems.FirstOrDefault((Item it) => Level.IsPositionInAbyss(it.WorldPosition));
							}
							else
							{
								if (spawnPositionType2 == Level.PositionType.AbyssCave)
								{
									goto IL_224;
								}
								if (spawnPositionType2 != Level.PositionType.Outpost)
								{
									goto IL_333;
								}
								goto IL_28A;
							}
							IL_34B:
							usedExistingItem = (target.Item != null);
							goto IL_361;
							IL_28A:
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
								goto IL_34B;
							}
							goto IL_333;
							IL_224:
							target.Item = suitableItems.FirstOrDefault((Item it) => Vector2.DistanceSquared(it.WorldPosition, position) < 1000f);
							goto IL_34B;
							IL_333:
							target.Item = suitableItems.FirstOrDefault<Item>();
							goto IL_34B;
						}
						IL_361:
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
								executedEffectIndices.Add(new ValueTuple<int, int>(i, effectIndex));
							}
						}
						target.Item.IsSalvageMissionItem = true;
						if (target.ContainerTag.IsEmpty || target.Item.ParentInventory != null)
						{
							goto IL_732;
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
						if (!validContainers.Any<ItemContainer>())
						{
							goto IL_732;
						}
						ItemContainer selectedContainer = validContainers.GetRandomUnsynced<ItemContainer>();
						if (selectedContainer.Combine(target.Item, null))
						{
							originalInventoryID = selectedContainer.Item.ID;
							originalItemContainerIndex = (byte)selectedContainer.Item.GetComponentIndex(selectedContainer);
							Inventory parentInventory = target.Item.ParentInventory;
							originalSlotIndex = ((parentInventory != null) ? parentInventory.FindIndex(target.Item) : -1);
							goto IL_732;
						}
						goto IL_732;
						IL_DF:
						num = 0f;
						goto IL_100;
						IL_E8:
						num = (float)Level.Loaded.Size.X * 0.3f;
						goto IL_100;
					}
					IL_732:
					this.spawnInfo.Add(target, new SalvageMission.SpawnInfo(usedExistingItem, originalInventoryID, originalItemContainerIndex, originalSlotIndex, executedEffectIndices));
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

		// Token: 0x0600052C RID: 1324 RVA: 0x0003021C File Offset: 0x0002E41C
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

		// Token: 0x0600052D RID: 1325 RVA: 0x000302AC File Offset: 0x0002E4AC
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			SalvageMission.<>c__DisplayClass21_0 CS$<>8__locals1;
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
			SalvageMission.<>c__DisplayClass21_1 CS$<>8__locals2;
			CS$<>8__locals2.i = 0;
			while (CS$<>8__locals2.i < this.targets.Count)
			{
				SalvageMission.<>c__DisplayClass21_2 CS$<>8__locals3;
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
							this.<UpdateMissionSpecific>g__TrySetRetrievalState|21_1(SalvageMission.Target.RetrievalState.RetrievedToSub, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
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
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|21_1(SalvageMission.Target.RetrievalState.Interact, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
					Item item2 = CS$<>8__locals3.target.Item;
					Item root2 = ((item2 != null) ? item2.RootContainer : null) ?? CS$<>8__locals3.target.Item;
					Inventory parentInventory = root2.ParentInventory;
					Character character2 = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
					if (character2 != null && character2.TeamID == CharacterTeamType.Team1)
					{
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|21_1(SalvageMission.Target.RetrievalState.PickedUp, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
					if (inPlayerSub)
					{
						this.<UpdateMissionSpecific>g__TrySetRetrievalState|21_1(SalvageMission.Target.RetrievalState.RetrievedToSub, ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
				}
				int i = CS$<>8__locals2.i;
				CS$<>8__locals2.i = i + 1;
			}
			if (this.targets.All((SalvageMission.Target t) => t.Retrieved))
			{
				this.State = this.targets.Count + 1;
			}
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00030600 File Offset: 0x0002E800
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			if (this.requiredDeliveryAmount < 1f)
			{
				IEnumerable<SalvageMission.Target> source = this.targets;
				Func<SalvageMission.Target, bool> predicate;
				if ((predicate = SalvageMission.<>O.<0>__IsTargetRetrieved) == null)
				{
					predicate = (SalvageMission.<>O.<0>__IsTargetRetrieved = new Func<SalvageMission.Target, bool>(SalvageMission.<DetermineCompleted>g__IsTargetRetrieved|22_0));
				}
				return (float)source.Count(predicate) / (float)this.targets.Count >= this.requiredDeliveryAmount;
			}
			IEnumerable<SalvageMission.Target> source2 = this.targets;
			Func<SalvageMission.Target, bool> predicate2;
			if ((predicate2 = SalvageMission.<>O.<0>__IsTargetRetrieved) == null)
			{
				predicate2 = (SalvageMission.<>O.<0>__IsTargetRetrieved = new Func<SalvageMission.Target, bool>(SalvageMission.<DetermineCompleted>g__IsTargetRetrieved|22_0));
			}
			return source2.All(predicate2);
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00030680 File Offset: 0x0002E880
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

		// Token: 0x06000530 RID: 1328 RVA: 0x000307A8 File Offset: 0x0002E9A8
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

		// Token: 0x06000531 RID: 1329 RVA: 0x00030872 File Offset: 0x0002EA72
		private static SubmarineInfo GetRandomWreckByTags(ImmutableArray<Identifier> tags, LevelData levelData)
		{
			return Mission.GetRandomSubmarineByTagsAndDifficulty(tags, levelData, (SubmarineInfo s) => s.IsWreck, "wreck");
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x000308A4 File Offset: 0x0002EAA4
		[CompilerGenerated]
		private LocalizedString <.ctor>g__GetMessage|16_0(string attributeName, ref SalvageMission.<>c__DisplayClass16_0 A_2)
		{
			if (A_2.prefab.ConfigElement.GetAttribute(attributeName) != null)
			{
				string msgTag = A_2.prefab.ConfigElement.GetAttributeString(attributeName, string.Empty);
				return base.ReplaceVariablesInMissionMessage(TextManager.Get(msgTag).Fallback(msgTag, true), A_2.sub, true);
			}
			return string.Empty;
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x00030944 File Offset: 0x0002EB44
		[CompilerGenerated]
		private void <UpdateMissionSpecific>g__TrySetRetrievalState|21_1(SalvageMission.Target.RetrievalState retrievalState, ref SalvageMission.<>c__DisplayClass21_0 A_2, ref SalvageMission.<>c__DisplayClass21_1 A_3, ref SalvageMission.<>c__DisplayClass21_2 A_4)
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

		// Token: 0x06000535 RID: 1333 RVA: 0x000309BB File Offset: 0x0002EBBB
		[CompilerGenerated]
		internal static bool <DetermineCompleted>g__IsTargetRetrieved|22_0(SalvageMission.Target target)
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

		// Token: 0x040002A0 RID: 672
		private readonly Dictionary<SalvageMission.Target, SalvageMission.SpawnInfo> spawnInfo = new Dictionary<SalvageMission.Target, SalvageMission.SpawnInfo>();

		// Token: 0x040002A1 RID: 673
		private readonly List<SalvageMission.Target> targets = new List<SalvageMission.Target>();

		// Token: 0x040002A2 RID: 674
		private readonly float requiredDeliveryAmount;

		// Token: 0x040002A3 RID: 675
		private readonly ImmutableArray<Identifier> wreckTags;

		// Token: 0x040002A4 RID: 676
		private LocalizedString pickedUpMessage;

		// Token: 0x040002A5 RID: 677
		private LocalizedString partiallyRetrievedMessage;

		// Token: 0x040002A6 RID: 678
		private LocalizedString allRetrievedMessage;

		// Token: 0x040002A7 RID: 679
		private readonly MTRandom rng;

		// Token: 0x020005FF RID: 1535
		private struct SpawnInfo
		{
			// Token: 0x06004CE2 RID: 19682 RVA: 0x001DE1A7 File Offset: 0x001DC3A7
			public SpawnInfo(bool usedExistingItem, ushort originalInventoryID, byte originalItemContainerIndex, int originalSlotIndex, [TupleElementNames(new string[]
			{
				"listIndex",
				"effectIndex"
			})] List<ValueTuple<int, int>> executedEffectIndices)
			{
				this.UsedExistingItem = usedExistingItem;
				this.OriginalInventoryID = originalInventoryID;
				this.OriginalItemContainerIndex = originalItemContainerIndex;
				this.OriginalSlotIndex = originalSlotIndex;
				this.ExecutedEffectIndices = executedEffectIndices;
			}

			// Token: 0x04002825 RID: 10277
			public readonly bool UsedExistingItem;

			// Token: 0x04002826 RID: 10278
			public readonly ushort OriginalInventoryID;

			// Token: 0x04002827 RID: 10279
			public readonly byte OriginalItemContainerIndex;

			// Token: 0x04002828 RID: 10280
			public readonly int OriginalSlotIndex;

			// Token: 0x04002829 RID: 10281
			[TupleElementNames(new string[]
			{
				"listIndex",
				"effectIndex"
			})]
			public readonly List<ValueTuple<int, int>> ExecutedEffectIndices;
		}

		// Token: 0x02000600 RID: 1536
		private class Target
		{
			// Token: 0x170013D4 RID: 5076
			// (get) Token: 0x06004CE3 RID: 19683 RVA: 0x001DE1D0 File Offset: 0x001DC3D0
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

			// Token: 0x170013D5 RID: 5077
			// (get) Token: 0x06004CE4 RID: 19684 RVA: 0x001DE231 File Offset: 0x001DC431
			// (set) Token: 0x06004CE5 RID: 19685 RVA: 0x001DE23C File Offset: 0x001DC43C
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
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.UpdateMissionState(this.mission);
					}
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

			// Token: 0x06004CE6 RID: 19686 RVA: 0x001DE298 File Offset: 0x001DC498
			private void OnTargetRetrieved()
			{
				Item item = this.Item;
			}

			// Token: 0x06004CE7 RID: 19687 RVA: 0x001DE2A1 File Offset: 0x001DC4A1
			private void OnTargetPickedUp()
			{
				Item item = this.Item;
			}

			// Token: 0x06004CE8 RID: 19688 RVA: 0x001DE2AC File Offset: 0x001DC4AC
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

			// Token: 0x06004CE9 RID: 19689 RVA: 0x001DE70C File Offset: 0x001DC90C
			public void Reset()
			{
				this.state = SalvageMission.Target.RetrievalState.None;
				this.Item = null;
			}

			// Token: 0x0400282A RID: 10282
			public Item Item;

			// Token: 0x0400282B RID: 10283
			public SalvageMission.Target ParentTarget;

			// Token: 0x0400282C RID: 10284
			public readonly ItemPrefab ItemPrefab;

			// Token: 0x0400282D RID: 10285
			public readonly Level.PositionType SpawnPositionType;

			// Token: 0x0400282E RID: 10286
			public readonly Identifier ContainerTag;

			// Token: 0x0400282F RID: 10287
			public readonly Identifier ExistingItemTag;

			// Token: 0x04002830 RID: 10288
			public readonly bool PointToSub;

			// Token: 0x04002831 RID: 10289
			public readonly bool RemoveItem;

			// Token: 0x04002832 RID: 10290
			public readonly LocalizedString SonarLabel;

			// Token: 0x04002833 RID: 10291
			public readonly bool AllowContinueBeforeRetrieved;

			// Token: 0x04002834 RID: 10292
			public readonly SalvageMission.Target.RetrievalState RequiredRetrievalState;

			// Token: 0x04002835 RID: 10293
			public readonly bool HideLabelAfterRetrieved;

			// Token: 0x04002836 RID: 10294
			public readonly bool HideLabelWhenFound;

			// Token: 0x04002837 RID: 10295
			public readonly bool HideLabelWhenNotFound;

			// Token: 0x04002838 RID: 10296
			private SalvageMission.Target.RetrievalState state;

			// Token: 0x04002839 RID: 10297
			public bool Interacted;

			// Token: 0x0400283A RID: 10298
			private readonly SalvageMission mission;

			// Token: 0x0400283B RID: 10299
			public readonly bool RequireInsideOriginalContainer;

			// Token: 0x0400283C RID: 10300
			public Item OriginalContainer;

			// Token: 0x0400283D RID: 10301
			public bool PlacingInsideParentTargetFailed;

			// Token: 0x0400283E RID: 10302
			public readonly List<List<StatusEffect>> StatusEffects = new List<List<StatusEffect>>();

			// Token: 0x02000E74 RID: 3700
			public enum RetrievalState
			{
				// Token: 0x04004262 RID: 16994
				None,
				// Token: 0x04004263 RID: 16995
				Interact,
				// Token: 0x04004264 RID: 16996
				PickedUp,
				// Token: 0x04004265 RID: 16997
				RetrievedToSub
			}
		}

		// Token: 0x02000601 RID: 1537
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400283F RID: 10303
			public static Func<SalvageMission.Target, bool> <0>__IsTargetRetrieved;
		}
	}
}
