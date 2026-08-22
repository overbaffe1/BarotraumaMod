using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002BB RID: 699
	internal class ScriptedEvent : Event
	{
		// Token: 0x17000FD5 RID: 4053
		// (get) Token: 0x06003C3C RID: 15420 RVA: 0x002286C0 File Offset: 0x002268C0
		// (set) Token: 0x06003C3D RID: 15421 RVA: 0x002286C8 File Offset: 0x002268C8
		public int CurrentActionIndex { get; private set; }

		// Token: 0x17000FD6 RID: 4054
		// (get) Token: 0x06003C3E RID: 15422 RVA: 0x002286D1 File Offset: 0x002268D1
		public List<EventAction> Actions { get; } = new List<EventAction>();

		// Token: 0x17000FD7 RID: 4055
		// (get) Token: 0x06003C3F RID: 15423 RVA: 0x002286D9 File Offset: 0x002268D9
		public Dictionary<Identifier, List<Entity>> Targets { get; } = new Dictionary<Identifier, List<Entity>>();

		// Token: 0x17000FD8 RID: 4056
		// (get) Token: 0x06003C40 RID: 15424 RVA: 0x002286E1 File Offset: 0x002268E1
		protected virtual IEnumerable<Identifier> NonActionChildElementNames
		{
			get
			{
				return Enumerable.Empty<Identifier>();
			}
		}

		// Token: 0x06003C41 RID: 15425 RVA: 0x002286E8 File Offset: 0x002268E8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("ScriptedEvent");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003C42 RID: 15426 RVA: 0x0022873C File Offset: 0x0022693C
		public ScriptedEvent(EventPrefab prefab, int seed) : base(prefab, seed)
		{
			foreach (ContentXElement element in prefab.ConfigElement.Elements())
			{
				Identifier elementId = element.Name.ToIdentifier<XName>();
				if (!this.NonActionChildElementNames.Contains(elementId))
				{
					if (elementId == "OnRoundEndAction")
					{
						this.OnRoundEndAction = (EventAction.Instantiate(this, element) as OnRoundEndAction);
					}
					else if (elementId == "statuseffect")
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(139, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in event prefab \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Status effect configured as an action. Please configure status effects as child elements of a StatusEffectAction.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, prefab.ContentPackage, false, false);
					}
					else
					{
						EventAction action2 = EventAction.Instantiate(this, element);
						if (action2 != null)
						{
							this.Actions.Add(action2);
						}
					}
				}
			}
			if (!this.Actions.Any<EventAction>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Scripted event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" has no actions. The event will do nothing.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, prefab.ContentPackage, false, false);
			}
			this.requiredDestinationTypes = prefab.ConfigElement.GetAttributeIdentifierArray("requireddestinationtypes", Array.Empty<Identifier>(), true);
			this.RequireBeaconStation = prefab.ConfigElement.GetAttributeBool("requirebeaconstation", false);
			this.RequiredDestinationFaction = prefab.ConfigElement.GetAttributeIdentifier("RequiredDestinationFaction", Identifier.Empty);
			List<ValueTuple<int, EventAction>> allActionsWithIndent = this.GetAllActions();
			IEnumerable<EventAction> allActions = from a in allActionsWithIndent
			select a.Item2;
			using (IEnumerator<EventAction> enumerator2 = allActions.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					EventAction action = enumerator2.Current;
					ConversationAction conversationAction = action as ConversationAction;
					if (conversationAction != null && conversationAction.Options.Any<ConversationAction.OptionActionGroup>())
					{
						int thisActionIndex = allActionsWithIndent.FindIndex(([TupleElementNames(new string[]
						{
							"indent",
							"action"
						})] ValueTuple<int, EventAction> a) => a.Item2 == action);
						int thisIndentationLevel = allActionsWithIndent[thisActionIndex].Item1;
						bool isLast = true;
						foreach (ValueTuple<int, EventAction> actionWithIndent in allActionsWithIndent.Skip(thisActionIndex + 1))
						{
							if (actionWithIndent.Item2 is ConversationAction && actionWithIndent.Item1 == thisIndentationLevel)
							{
								isLast = false;
								break;
							}
							if (actionWithIndent.Item1 < thisIndentationLevel)
							{
								break;
							}
						}
						if (isLast)
						{
							foreach (ConversationAction.OptionActionGroup option in conversationAction.Options)
							{
								if (!conversationAction.GetEndingOptions().Contains(conversationAction.Options.IndexOf(option)))
								{
									if (option.Actions.None(delegate(EventAction a)
									{
										if (!(a is ConversationAction) && !ScriptedEvent.<.ctor>g__HasConversationSubAction|25_1(a))
										{
											GoTo goTo = a as GoTo;
											return goTo != null && !goTo.EndConversation;
										}
										return true;
									}))
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(139, 4);
										defaultInterpolatedStringHandler3.AppendLiteral("Potential error in event \"");
										defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(prefab.Identifier);
										defaultInterpolatedStringHandler3.AppendLiteral("\": ");
										defaultInterpolatedStringHandler3.AppendFormatted("ConversationAction");
										defaultInterpolatedStringHandler3.AppendLiteral(" (");
										defaultInterpolatedStringHandler3.AppendFormatted(conversationAction.Text);
										defaultInterpolatedStringHandler3.AppendLiteral(") has an option (");
										defaultInterpolatedStringHandler3.AppendFormatted(option.Text);
										defaultInterpolatedStringHandler3.AppendLiteral(") that doesn't end the conversation, but could not find any follow-ups to the conversation.");
										DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
									}
								}
							}
						}
					}
				}
			}
			using (IEnumerator<Label> enumerator5 = allActions.OfType<Label>().GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					Label label = enumerator5.Current;
					if (allActions.None(delegate(EventAction a)
					{
						GoTo gotoAction = a as GoTo;
						return gotoAction != null && label.Name == gotoAction.Name;
					}))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(63, 2);
						defaultInterpolatedStringHandler4.AppendLiteral("Error in event \"");
						defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler4.AppendLiteral("\". Could not find a GoTo matching the Label \"");
						defaultInterpolatedStringHandler4.AppendFormatted(label.Name);
						defaultInterpolatedStringHandler4.AppendLiteral("\".");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), prefab.ContentPackage);
					}
				}
			}
			using (IEnumerator<GoTo> enumerator6 = allActions.OfType<GoTo>().GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					GoTo gotoAction = enumerator6.Current;
					int labelCount = allActions.Count(delegate(EventAction a)
					{
						Label label = a as Label;
						return label != null && label.Name == gotoAction.Name;
					});
					if (labelCount == 0)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(63, 2);
						defaultInterpolatedStringHandler5.AppendLiteral("Error in event \"");
						defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler5.AppendLiteral("\". Could not find a label matching the GoTo \"");
						defaultInterpolatedStringHandler5.AppendFormatted(gotoAction.Name);
						defaultInterpolatedStringHandler5.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, prefab.ContentPackage, false, false);
					}
					else if (labelCount > 1)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(52, 2);
						defaultInterpolatedStringHandler6.AppendLiteral("Error in event \"");
						defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler6.AppendLiteral("\". Multiple labels with the name \"");
						defaultInterpolatedStringHandler6.AppendFormatted(gotoAction.Name);
						defaultInterpolatedStringHandler6.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler6.ToStringAndClear(), null, prefab.ContentPackage, false, false);
					}
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler7.AppendLiteral("ScriptedEvent:");
			defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(prefab.Identifier);
			defaultInterpolatedStringHandler7.AppendLiteral(":Start");
			GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler7.ToStringAndClear());
		}

		// Token: 0x06003C43 RID: 15427 RVA: 0x00228DEC File Offset: 0x00226FEC
		public override string GetDebugInfo()
		{
			EventAction currentAction = (!base.IsFinished) ? this.Actions[this.CurrentActionIndex] : null;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Finished: ");
			defaultInterpolatedStringHandler.AppendFormatted(base.IsFinished.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Action index: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.CurrentActionIndex.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Current action: ");
			defaultInterpolatedStringHandler.AppendFormatted(((currentAction != null) ? currentAction.ToDebugString() : null) ?? ToolBox.ColorizeObject((object)null));
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			string text = defaultInterpolatedStringHandler.ToStringAndClear();
			text += "All actions:\n";
			text += this.GetAllActions().Aggregate(string.Empty, (string current, [TupleElementNames(new string[]
			{
				"indent",
				"action"
			})] ValueTuple<int, EventAction> action) => current + new string(' ', action.Item1 * 6) + action.Item2.ToDebugString() + "\n");
			text += "Targets:\n";
			foreach (KeyValuePair<Identifier, List<Entity>> keyValuePair in this.Targets)
			{
				Identifier identifier;
				List<Entity> list;
				keyValuePair.Deconstruct(out identifier, out list);
				Identifier key = identifier;
				List<Entity> value = list;
				string str = text;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(7, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("    ");
				defaultInterpolatedStringHandler2.AppendFormatted(key.ColorizeObject());
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted(value.Aggregate(string.Empty, (string current, Entity entity) => current + entity.ColorizeObject() + " "));
				defaultInterpolatedStringHandler2.AppendLiteral("\n");
				text = str + defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			return text;
		}

		// Token: 0x06003C44 RID: 15428 RVA: 0x00228FE4 File Offset: 0x002271E4
		public virtual string GetTextForReplacementElement(string tag)
		{
			if (!tag.StartsWith("eventtag:"))
			{
				return string.Empty;
			}
			string targetTag = tag.Substring("eventtag:".Length);
			Entity target = this.GetTargets(targetTag.ToIdentifier()).FirstOrDefault<Entity>();
			if (target == null)
			{
				return "[target \"" + targetTag + "\" not found]";
			}
			Item item = target as Item;
			if (item != null)
			{
				return item.Name;
			}
			Character character = target as Character;
			if (character != null)
			{
				return character.Name;
			}
			Hull hull = target as Hull;
			if (hull != null)
			{
				return hull.DisplayName.Value;
			}
			Submarine sub = target as Submarine;
			if (sub != null)
			{
				return sub.Info.DisplayName.Value;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Failed to get the name of the event target ");
			defaultInterpolatedStringHandler.AppendFormatted<Entity>(target);
			defaultInterpolatedStringHandler.AppendLiteral(" as a replacement for the tag ");
			defaultInterpolatedStringHandler.AppendFormatted(tag);
			defaultInterpolatedStringHandler.AppendLiteral(" in an event text.");
			DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.prefab.ContentPackage);
			return target.ToString();
		}

		// Token: 0x06003C45 RID: 15429 RVA: 0x002290F5 File Offset: 0x002272F5
		public virtual LocalizedString ReplaceVariablesInEventText(LocalizedString str)
		{
			return str;
		}

		// Token: 0x06003C46 RID: 15430 RVA: 0x002290F8 File Offset: 0x002272F8
		[return: TupleElementNames(new string[]
		{
			"indent",
			"action"
		})]
		public List<ValueTuple<int, EventAction>> GetAllActions()
		{
			List<ValueTuple<int, EventAction>> list = new List<ValueTuple<int, EventAction>>();
			foreach (EventAction eventAction in this.Actions)
			{
				list.AddRange(ScriptedEvent.<GetAllActions>g__FindActionsRecursive|29_0(eventAction, 1));
			}
			return list;
		}

		// Token: 0x06003C47 RID: 15431 RVA: 0x00229158 File Offset: 0x00227358
		public void AddTarget(Identifier tag, Entity target)
		{
			if (target == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Target was null (tag: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(tag);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (target.Removed)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Target has been removed (tag: ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(tag);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				throw new ArgumentException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			if (this.Targets.ContainsKey(tag))
			{
				if (!this.Targets[tag].Contains(target))
				{
					this.Targets[tag].Add(target);
				}
			}
			else
			{
				this.Targets.Add(tag, new List<Entity>
				{
					target
				});
			}
			if (this.cachedTargets.ContainsKey(tag))
			{
				if (!this.cachedTargets[tag].Contains(target))
				{
					this.cachedTargets[tag].Add(target);
				}
			}
			else
			{
				this.cachedTargets.Add(tag, this.Targets[tag].ToList<Entity>());
			}
			if (!this.initialAmounts.ContainsKey(tag))
			{
				this.initialAmounts.Add(tag, this.cachedTargets[tag].Count);
			}
		}

		// Token: 0x06003C48 RID: 15432 RVA: 0x002292AC File Offset: 0x002274AC
		public void AddTargetPredicate(Identifier tag, ScriptedEvent.TargetPredicate.EntityType entityType, Predicate<Entity> predicate)
		{
			if (!this.targetPredicates.ContainsKey(tag))
			{
				this.targetPredicates.Add(tag, new List<ScriptedEvent.TargetPredicate>());
			}
			this.targetPredicates[tag].Add(new ScriptedEvent.TargetPredicate(entityType, predicate));
			if (this.cachedTargets.ContainsKey(tag))
			{
				this.cachedTargets.Remove(tag);
			}
		}

		// Token: 0x06003C49 RID: 15433 RVA: 0x0022930C File Offset: 0x0022750C
		public int GetInitialTargetCount(Identifier tag)
		{
			int count;
			if (this.initialAmounts.TryGetValue(tag, out count))
			{
				return count;
			}
			return 0;
		}

		// Token: 0x06003C4A RID: 15434 RVA: 0x0022932C File Offset: 0x0022752C
		public IEnumerable<Entity> GetTargets(Identifier tag)
		{
			if (this.cachedTargets.ContainsKey(tag))
			{
				if (!this.cachedTargets[tag].Any((Entity t) => t.Removed))
				{
					return this.cachedTargets[tag];
				}
				this.cachedTargets.Clear();
			}
			List<Entity> targetsToReturn = new List<Entity>();
			if (this.Targets.ContainsKey(tag))
			{
				foreach (Entity e in this.Targets[tag])
				{
					if (!e.Removed)
					{
						targetsToReturn.Add(e);
					}
				}
			}
			if (this.targetPredicates.ContainsKey(tag))
			{
				foreach (ScriptedEvent.TargetPredicate targetPredicate in this.targetPredicates[tag])
				{
					IEnumerable<Entity> enumerable;
					switch (targetPredicate.Type)
					{
					case ScriptedEvent.TargetPredicate.EntityType.Character:
						enumerable = Character.CharacterList;
						break;
					case ScriptedEvent.TargetPredicate.EntityType.Hull:
						enumerable = Hull.HullList;
						break;
					case ScriptedEvent.TargetPredicate.EntityType.Item:
						enumerable = Item.ItemList;
						break;
					case ScriptedEvent.TargetPredicate.EntityType.Structure:
						enumerable = from m in MapEntity.MapEntityList
						where m is Structure
						select m;
						break;
					case ScriptedEvent.TargetPredicate.EntityType.Submarine:
						enumerable = Submarine.Loaded;
						break;
					default:
						enumerable = Entity.GetEntities();
						break;
					}
					IEnumerable<Entity> entityList = enumerable;
					foreach (Entity entity in entityList)
					{
						if (!targetsToReturn.Contains(entity) && targetPredicate.Predicate(entity))
						{
							targetsToReturn.Add(entity);
						}
					}
				}
			}
			foreach (WayPoint wayPoint in WayPoint.WayPointList)
			{
				if (wayPoint.Tags.Contains(tag))
				{
					targetsToReturn.Add(wayPoint);
				}
			}
			Level loaded = Level.Loaded;
			List<Character> outpostNPCs;
			if (((loaded != null) ? loaded.StartOutpost : null) != null && Level.Loaded.StartOutpost.Info.OutpostNPCs.TryGetValue(tag, out outpostNPCs))
			{
				foreach (Character npc in outpostNPCs)
				{
					if (!npc.Removed && !targetsToReturn.Contains(npc))
					{
						targetsToReturn.Add(npc);
					}
				}
			}
			this.cachedTargets.Add(tag, targetsToReturn);
			if (!this.initialAmounts.ContainsKey(tag))
			{
				this.initialAmounts.Add(tag, targetsToReturn.Count);
			}
			return targetsToReturn;
		}

		// Token: 0x06003C4B RID: 15435 RVA: 0x00229638 File Offset: 0x00227838
		public void InheritTags(Entity originalEntity, Entity newEntity)
		{
			foreach (KeyValuePair<Identifier, List<Entity>> kvp in this.Targets)
			{
				if (kvp.Value.Contains(originalEntity))
				{
					kvp.Value.Add(newEntity);
				}
			}
		}

		// Token: 0x06003C4C RID: 15436 RVA: 0x002296A0 File Offset: 0x002278A0
		public void RemoveTag(Identifier tag)
		{
			if (tag.IsEmpty)
			{
				return;
			}
			if (this.Targets.ContainsKey(tag))
			{
				this.Targets.Remove(tag);
			}
			if (this.cachedTargets.ContainsKey(tag))
			{
				this.cachedTargets.Remove(tag);
			}
			if (this.targetPredicates.ContainsKey(tag))
			{
				this.targetPredicates.Remove(tag);
			}
		}

		// Token: 0x06003C4D RID: 15437 RVA: 0x00229708 File Offset: 0x00227908
		public override void Update(float deltaTime)
		{
			int botCount = 0;
			int playerCount = 0;
			foreach (Character c in Character.CharacterList)
			{
				if (c.IsPlayer)
				{
					playerCount++;
				}
				else if (c.IsBot)
				{
					botCount++;
				}
			}
			if (botCount != this.prevBotCount || playerCount != this.prevPlayerCount || this.prevControlled != Character.Controlled || this.NeedsToRefreshCachedTargets())
			{
				this.cachedTargets.Clear();
				this.newEntitySpawned = false;
				this.prevBotCount = botCount;
				this.prevPlayerCount = playerCount;
				this.prevControlled = Character.Controlled;
			}
			if (!this.Actions.Any<EventAction>())
			{
				this.Finish();
				return;
			}
			EventAction currentAction = this.Actions[this.CurrentActionIndex];
			if (!currentAction.CanBeFinished())
			{
				this.Finish();
				return;
			}
			string goTo = null;
			if (currentAction.IsFinished(ref goTo))
			{
				if (string.IsNullOrEmpty(goTo))
				{
					int currentActionIndex = this.CurrentActionIndex;
					this.CurrentActionIndex = currentActionIndex + 1;
				}
				else
				{
					this.CurrentActionIndex = -1;
					this.Actions.ForEach(delegate(EventAction a)
					{
						a.Reset();
					});
					for (int i = 0; i < this.Actions.Count; i++)
					{
						if (this.Actions[i].SetGoToTarget(goTo))
						{
							this.CurrentActionIndex = i;
							break;
						}
					}
					if (this.CurrentActionIndex == -1)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Could not find the GoTo label \"");
						defaultInterpolatedStringHandler.AppendFormatted(goTo);
						defaultInterpolatedStringHandler.AppendLiteral("\" in the event \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(base.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Ending the event.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.prefab.ContentPackage);
					}
				}
				if (this.CurrentActionIndex >= this.Actions.Count || this.CurrentActionIndex < 0)
				{
					this.Finish();
					return;
				}
			}
			else
			{
				currentAction.Update(deltaTime);
			}
		}

		// Token: 0x06003C4E RID: 15438 RVA: 0x00229928 File Offset: 0x00227B28
		private bool NeedsToRefreshCachedTargets()
		{
			if (this.newEntitySpawned)
			{
				return true;
			}
			foreach (List<Entity> cachedTargetList in this.cachedTargets.Values)
			{
				foreach (Entity target in cachedTargetList)
				{
					if (target.Removed)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003C4F RID: 15439 RVA: 0x002299CC File Offset: 0x00227BCC
		public void EntitySpawned(Entity entity)
		{
			if (this.newEntitySpawned)
			{
				return;
			}
			Character character = entity as Character;
			if (character != null)
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.StartOutpost : null) != null && Level.Loaded.StartOutpost.Info.OutpostNPCs.Values.Any((List<Character> npcList) => npcList.Contains(character)))
				{
					this.newEntitySpawned = true;
					return;
				}
			}
			foreach (List<ScriptedEvent.TargetPredicate> targetPredicateList in this.targetPredicates.Values)
			{
				foreach (ScriptedEvent.TargetPredicate targetPredicate in targetPredicateList)
				{
					if (targetPredicate.Predicate(entity))
					{
						this.newEntitySpawned = true;
						return;
					}
				}
			}
		}

		// Token: 0x06003C50 RID: 15440 RVA: 0x00229AD8 File Offset: 0x00227CD8
		public override bool LevelMeetsRequirements()
		{
			GameSession gameSession = GameMain.GameSession;
			Location location;
			if (gameSession == null)
			{
				location = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				location = ((campaign != null) ? campaign.Map.CurrentLocation : null);
			}
			Location currLocation = location;
			if (((currLocation != null) ? currLocation.Connections : null) == null)
			{
				return true;
			}
			foreach (LocationConnection c in currLocation.Connections)
			{
				if (!this.RequireBeaconStation || c.LevelData.HasBeaconStation)
				{
					Location otherLocation = c.OtherLocation(currLocation);
					if (!this.RequiredDestinationFaction.IsEmpty)
					{
						Faction faction = otherLocation.Faction;
						Identifier? identifier;
						Identifier? identifier2;
						if (faction == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(faction.Prefab.Identifier);
						}
						identifier = identifier2;
						Identifier? identifier3 = new Identifier?(this.RequiredDestinationFaction);
						if (identifier != identifier3)
						{
							continue;
						}
					}
					if (this.requiredDestinationTypes.Contains(Tags.AnyOutpost) && otherLocation.HasOutpost() && otherLocation.Type.IsAnyOutpost)
					{
						return true;
					}
					if (this.requiredDestinationTypes.Any((Identifier t) => otherLocation.Type.Identifier == t))
					{
						return true;
					}
				}
			}
			return this.RequiredDestinationFaction.IsEmpty && this.requiredDestinationTypes.None(null);
		}

		// Token: 0x06003C51 RID: 15441 RVA: 0x00229C50 File Offset: 0x00227E50
		public override void Finish()
		{
			base.Finish();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
			defaultInterpolatedStringHandler.AppendLiteral("ScriptedEvent:");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(":Finished:");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.CurrentActionIndex);
			GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06003C52 RID: 15442 RVA: 0x00229CB0 File Offset: 0x00227EB0
		[CompilerGenerated]
		internal static bool <.ctor>g__HasConversationSubAction|25_1(EventAction action)
		{
			foreach (EventAction subAction in action.GetSubActions())
			{
				if (subAction is ConversationAction)
				{
					return true;
				}
				if (ScriptedEvent.<.ctor>g__HasConversationSubAction|25_1(subAction))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003C53 RID: 15443 RVA: 0x00229D14 File Offset: 0x00227F14
		[CompilerGenerated]
		[return: TupleElementNames(new string[]
		{
			"indent",
			"action"
		})]
		internal static List<ValueTuple<int, EventAction>> <GetAllActions>g__FindActionsRecursive|29_0(EventAction eventAction, int indent = 1)
		{
			List<ValueTuple<int, EventAction>> eventActions = new List<ValueTuple<int, EventAction>>
			{
				new ValueTuple<int, EventAction>(indent, eventAction)
			};
			indent++;
			foreach (EventAction action in eventAction.GetSubActions())
			{
				eventActions.AddRange(ScriptedEvent.<GetAllActions>g__FindActionsRecursive|29_0(action, indent));
			}
			return eventActions;
		}

		// Token: 0x04001EF9 RID: 7929
		private readonly Dictionary<Identifier, List<ScriptedEvent.TargetPredicate>> targetPredicates = new Dictionary<Identifier, List<ScriptedEvent.TargetPredicate>>();

		// Token: 0x04001EFA RID: 7930
		private readonly Dictionary<Identifier, List<Entity>> cachedTargets = new Dictionary<Identifier, List<Entity>>();

		// Token: 0x04001EFB RID: 7931
		private readonly Dictionary<Identifier, int> initialAmounts = new Dictionary<Identifier, int>();

		// Token: 0x04001EFC RID: 7932
		private bool newEntitySpawned;

		// Token: 0x04001EFD RID: 7933
		private int prevPlayerCount;

		// Token: 0x04001EFE RID: 7934
		private int prevBotCount;

		// Token: 0x04001EFF RID: 7935
		private Character prevControlled;

		// Token: 0x04001F00 RID: 7936
		public readonly OnRoundEndAction OnRoundEndAction;

		// Token: 0x04001F01 RID: 7937
		private readonly Identifier[] requiredDestinationTypes;

		// Token: 0x04001F02 RID: 7938
		public readonly bool RequireBeaconStation;

		// Token: 0x04001F03 RID: 7939
		public readonly Identifier RequiredDestinationFaction;

		// Token: 0x02000F64 RID: 3940
		public sealed class TargetPredicate : IEquatable<ScriptedEvent.TargetPredicate>
		{
			// Token: 0x060088EA RID: 35050 RVA: 0x003A71A3 File Offset: 0x003A53A3
			public TargetPredicate(ScriptedEvent.TargetPredicate.EntityType Type, Predicate<Entity> Predicate)
			{
				this.Type = Type;
				this.Predicate = Predicate;
				base..ctor();
			}

			// Token: 0x17001C20 RID: 7200
			// (get) Token: 0x060088EB RID: 35051 RVA: 0x003A71B9 File Offset: 0x003A53B9
			[Nullable(1)]
			[CompilerGenerated]
			private Type EqualityContract
			{
				[NullableContext(1)]
				[CompilerGenerated]
				get
				{
					return typeof(ScriptedEvent.TargetPredicate);
				}
			}

			// Token: 0x17001C21 RID: 7201
			// (get) Token: 0x060088EC RID: 35052 RVA: 0x003A71C5 File Offset: 0x003A53C5
			// (set) Token: 0x060088ED RID: 35053 RVA: 0x003A71CD File Offset: 0x003A53CD
			public ScriptedEvent.TargetPredicate.EntityType Type { get; set; }

			// Token: 0x17001C22 RID: 7202
			// (get) Token: 0x060088EE RID: 35054 RVA: 0x003A71D6 File Offset: 0x003A53D6
			// (set) Token: 0x060088EF RID: 35055 RVA: 0x003A71DE File Offset: 0x003A53DE
			public Predicate<Entity> Predicate { get; set; }

			// Token: 0x060088F0 RID: 35056 RVA: 0x003A71E8 File Offset: 0x003A53E8
			[NullableContext(1)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("TargetPredicate");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060088F1 RID: 35057 RVA: 0x003A7234 File Offset: 0x003A5434
			[NullableContext(1)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
				builder.Append("Type = ");
				builder.Append(this.Type.ToString());
				builder.Append(", Predicate = ");
				builder.Append(this.Predicate);
				return true;
			}

			// Token: 0x060088F2 RID: 35058 RVA: 0x003A7287 File Offset: 0x003A5487
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator !=(ScriptedEvent.TargetPredicate left, ScriptedEvent.TargetPredicate right)
			{
				return !(left == right);
			}

			// Token: 0x060088F3 RID: 35059 RVA: 0x003A7293 File Offset: 0x003A5493
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator ==(ScriptedEvent.TargetPredicate left, ScriptedEvent.TargetPredicate right)
			{
				return left == right || (left != null && left.Equals(right));
			}

			// Token: 0x060088F4 RID: 35060 RVA: 0x003A72A7 File Offset: 0x003A54A7
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<ScriptedEvent.TargetPredicate.EntityType>.Default.GetHashCode(this.<Type>k__BackingField)) * -1521134295 + EqualityComparer<Predicate<Entity>>.Default.GetHashCode(this.<Predicate>k__BackingField);
			}

			// Token: 0x060088F5 RID: 35061 RVA: 0x003A72E7 File Offset: 0x003A54E7
			[NullableContext(2)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return this.Equals(obj as ScriptedEvent.TargetPredicate);
			}

			// Token: 0x060088F6 RID: 35062 RVA: 0x003A72F8 File Offset: 0x003A54F8
			[NullableContext(2)]
			[CompilerGenerated]
			public bool Equals(ScriptedEvent.TargetPredicate other)
			{
				return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<ScriptedEvent.TargetPredicate.EntityType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Predicate<Entity>>.Default.Equals(this.<Predicate>k__BackingField, other.<Predicate>k__BackingField));
			}

			// Token: 0x060088F8 RID: 35064 RVA: 0x003A7359 File Offset: 0x003A5559
			[CompilerGenerated]
			private TargetPredicate([Nullable(1)] ScriptedEvent.TargetPredicate original)
			{
				this.Type = original.<Type>k__BackingField;
				this.Predicate = original.<Predicate>k__BackingField;
			}

			// Token: 0x060088F9 RID: 35065 RVA: 0x003A7379 File Offset: 0x003A5579
			[CompilerGenerated]
			public void Deconstruct(out ScriptedEvent.TargetPredicate.EntityType Type, out Predicate<Entity> Predicate)
			{
				Type = this.Type;
				Predicate = this.Predicate;
			}

			// Token: 0x02001567 RID: 5479
			public enum EntityType
			{
				// Token: 0x04006860 RID: 26720
				Character,
				// Token: 0x04006861 RID: 26721
				Hull,
				// Token: 0x04006862 RID: 26722
				Item,
				// Token: 0x04006863 RID: 26723
				Structure,
				// Token: 0x04006864 RID: 26724
				Submarine
			}
		}
	}
}
