using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000022 RID: 34
	internal abstract class AIObjective
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000281 RID: 641 RVA: 0x00019522 File Offset: 0x00017722
		public static Color ObjectiveIconColor
		{
			get
			{
				return Color.LightGray;
			}
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0001952C File Offset: 0x0001772C
		public static Sprite GetSprite(Identifier identifier, Identifier option, Entity targetEntity)
		{
			if (identifier == Identifier.Empty)
			{
				return null;
			}
			if (OrderPrefab.Prefabs.ContainsKey(identifier))
			{
				OrderPrefab orderPrefab = OrderPrefab.Prefabs[identifier];
				Sprite optionSprite;
				if (option != Identifier.Empty && orderPrefab.OptionSprites.TryGetValue(option, out optionSprite))
				{
					return optionSprite;
				}
				Item targetItem = targetEntity as Item;
				if (targetItem != null && targetItem.Prefab.MinimapIcon != null)
				{
					return targetItem.Prefab.MinimapIcon;
				}
				return orderPrefab.SymbolSprite;
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("objectiveicon");
				GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
				if (componentStyle == null)
				{
					return null;
				}
				return componentStyle.GetDefaultSprite();
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x000195E1 File Offset: 0x000177E1
		public Sprite GetSprite()
		{
			Identifier identifier = this.Identifier;
			Identifier option = this.Option;
			AIObjectiveOperateItem aiobjectiveOperateItem = this as AIObjectiveOperateItem;
			return AIObjective.GetSprite(identifier, option, (aiobjectiveOperateItem != null) ? aiobjectiveOperateItem.OperateTarget : null);
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000284 RID: 644 RVA: 0x00019606 File Offset: 0x00017806
		public virtual float Devotion
		{
			get
			{
				return 5f;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000285 RID: 645
		// (set) Token: 0x06000286 RID: 646
		public abstract Identifier Identifier { get; set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000287 RID: 647 RVA: 0x00019610 File Offset: 0x00017810
		public virtual string DebugTag
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000288 RID: 648 RVA: 0x0001962B File Offset: 0x0001782B
		public virtual bool ForceRun
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000289 RID: 649 RVA: 0x0001962E File Offset: 0x0001782E
		public virtual bool IgnoreUnsafeHulls
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x0600028A RID: 650 RVA: 0x00019631 File Offset: 0x00017831
		public virtual bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600028B RID: 651 RVA: 0x00019634 File Offset: 0x00017834
		public virtual bool AllowSubObjectiveSorting
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600028C RID: 652 RVA: 0x00019637 File Offset: 0x00017837
		public virtual bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600028D RID: 653 RVA: 0x0001963A File Offset: 0x0001783A
		public virtual bool AllowMultipleInstances
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600028E RID: 654 RVA: 0x0001963D File Offset: 0x0001783D
		protected virtual bool ConcurrentObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600028F RID: 655 RVA: 0x00019640 File Offset: 0x00017840
		public virtual bool KeepDivingGearOn
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000290 RID: 656 RVA: 0x00019643 File Offset: 0x00017843
		public virtual bool KeepDivingGearOnAlsoWhenInactive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000291 RID: 657 RVA: 0x00019646 File Offset: 0x00017846
		public virtual bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000292 RID: 658 RVA: 0x00019649 File Offset: 0x00017849
		protected virtual bool AllowOutsideSubmarine
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000293 RID: 659 RVA: 0x0001964C File Offset: 0x0001784C
		protected virtual bool AllowInFriendlySubs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000294 RID: 660 RVA: 0x0001964F File Offset: 0x0001784F
		protected virtual bool AllowInAnySub
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000295 RID: 661 RVA: 0x00019652 File Offset: 0x00017852
		protected virtual bool AllowWhileHandcuffed
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000296 RID: 662 RVA: 0x00019655 File Offset: 0x00017855
		protected virtual bool AbandonIfDisallowed
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000297 RID: 663 RVA: 0x00019658 File Offset: 0x00017858
		public virtual bool CanBeCompleted
		{
			get
			{
				return !this.Abandon;
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00019663 File Offset: 0x00017863
		protected virtual float MaxDevotion
		{
			get
			{
				return 10f;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000299 RID: 665 RVA: 0x0001966A File Offset: 0x0001786A
		// (set) Token: 0x0600029A RID: 666 RVA: 0x00019672 File Offset: 0x00017872
		protected float CumulatedDevotion
		{
			get
			{
				return this._cumulatedDevotion;
			}
			set
			{
				this._cumulatedDevotion = MathHelper.Clamp(value, 0f, this.MaxDevotion);
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x0600029B RID: 667 RVA: 0x0001968B File Offset: 0x0001788B
		// (set) Token: 0x0600029C RID: 668 RVA: 0x00019693 File Offset: 0x00017893
		public float Priority { get; set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0001969C File Offset: 0x0001789C
		// (set) Token: 0x0600029E RID: 670 RVA: 0x000196A4 File Offset: 0x000178A4
		public float BasePriority { get; set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x0600029F RID: 671 RVA: 0x000196AD File Offset: 0x000178AD
		// (set) Token: 0x060002A0 RID: 672 RVA: 0x000196B5 File Offset: 0x000178B5
		public float PriorityModifier { get; private set; } = 1f;

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x060002A1 RID: 673 RVA: 0x000196BE File Offset: 0x000178BE
		// (set) Token: 0x060002A2 RID: 674 RVA: 0x000196C6 File Offset: 0x000178C6
		public bool ForceHighestPriority
		{
			get
			{
				return this._forceHighestPriority;
			}
			set
			{
				if (this._forceHighestPriority == value)
				{
					return;
				}
				this._forceHighestPriority = value;
				if (this._forceHighestPriority)
				{
					this.resetPriorityTimer = this.resetPriorityTime;
				}
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x000196ED File Offset: 0x000178ED
		// (set) Token: 0x060002A4 RID: 676 RVA: 0x000196F5 File Offset: 0x000178F5
		public bool ForceWalkTemporarily { get; set; }

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060002A5 RID: 677 RVA: 0x000196FE File Offset: 0x000178FE
		// (set) Token: 0x060002A6 RID: 678 RVA: 0x00019706 File Offset: 0x00017906
		public bool ForceWalkPermanently { get; set; }

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060002A7 RID: 679 RVA: 0x0001970F File Offset: 0x0001790F
		public bool ForceWalk
		{
			get
			{
				return this.ForceWalkTemporarily || this.ForceWalkPermanently;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060002A8 RID: 680 RVA: 0x00019721 File Offset: 0x00017921
		// (set) Token: 0x060002A9 RID: 681 RVA: 0x00019729 File Offset: 0x00017929
		public bool IgnoreAtOutpost { get; set; }

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060002AA RID: 682 RVA: 0x00019732 File Offset: 0x00017932
		// (set) Token: 0x060002AB RID: 683 RVA: 0x0001973A File Offset: 0x0001793A
		public bool Abandon
		{
			get
			{
				return this._abandon;
			}
			set
			{
				this._abandon = value;
				if (this._abandon)
				{
					this.OnAbandon();
				}
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060002AC RID: 684 RVA: 0x00019751 File Offset: 0x00017951
		public IEnumerable<AIObjective> SubObjectives
		{
			get
			{
				return this.subObjectives;
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060002AD RID: 685 RVA: 0x00019759 File Offset: 0x00017959
		public AIObjective CurrentSubObjective
		{
			get
			{
				return this.subObjectives.FirstOrDefault<AIObjective>();
			}
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00019768 File Offset: 0x00017968
		public IEnumerable<AIObjective> GetSubObjectivesRecursive(bool includingSelf = false)
		{
			this.all.Clear();
			if (includingSelf)
			{
				this.all.Add(this);
			}
			foreach (AIObjective subObjective in this.subObjectives)
			{
				this.all.AddRange(subObjective.GetSubObjectivesRecursive(true));
			}
			return this.all;
		}

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x060002AF RID: 687 RVA: 0x000197E8 File Offset: 0x000179E8
		// (remove) Token: 0x060002B0 RID: 688 RVA: 0x00019820 File Offset: 0x00017A20
		public event Action Completed;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060002B1 RID: 689 RVA: 0x00019858 File Offset: 0x00017A58
		// (remove) Token: 0x060002B2 RID: 690 RVA: 0x00019890 File Offset: 0x00017A90
		public event Action Abandoned;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060002B3 RID: 691 RVA: 0x000198C8 File Offset: 0x00017AC8
		// (remove) Token: 0x060002B4 RID: 692 RVA: 0x00019900 File Offset: 0x00017B00
		public event Action Selected;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060002B5 RID: 693 RVA: 0x00019938 File Offset: 0x00017B38
		// (remove) Token: 0x060002B6 RID: 694 RVA: 0x00019970 File Offset: 0x00017B70
		public event Action Deselected;

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x000199A5 File Offset: 0x00017BA5
		protected HumanAIController HumanAIController
		{
			get
			{
				return this.character.AIController as HumanAIController;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x000199B7 File Offset: 0x00017BB7
		protected IndoorsSteeringManager PathSteering
		{
			get
			{
				return this.HumanAIController.PathSteering;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x000199C4 File Offset: 0x00017BC4
		protected SteeringManager SteeringManager
		{
			get
			{
				return this.HumanAIController.SteeringManager;
			}
		}

		// Token: 0x060002BA RID: 698 RVA: 0x000199D4 File Offset: 0x00017BD4
		public AIObjective GetActiveObjective()
		{
			AIObjective subObjective = this.CurrentSubObjective;
			if (subObjective != null)
			{
				return subObjective.GetActiveObjective();
			}
			return this;
		}

		// Token: 0x060002BB RID: 699 RVA: 0x000199F4 File Offset: 0x00017BF4
		public AIObjective(Character character, AIObjectiveManager objectiveManager, float priorityModifier, Identifier option = default(Identifier))
		{
			this.objectiveManager = objectiveManager;
			this.character = character;
			this.Option = option;
			this.PriorityModifier = priorityModifier;
		}

		// Token: 0x060002BC RID: 700 RVA: 0x00019A50 File Offset: 0x00017C50
		public void TryComplete(float deltaTime)
		{
			if (this.isCompleted)
			{
				return;
			}
			if (this.CheckState())
			{
				return;
			}
			for (int i = 0; i < this.subObjectives.Count; i++)
			{
				this.subObjectives[i].TryComplete(deltaTime);
				if (!this.ConcurrentObjectives)
				{
					return;
				}
			}
			this.Act(deltaTime);
		}

		// Token: 0x060002BD RID: 701 RVA: 0x00019AA8 File Offset: 0x00017CA8
		public void AddSubObjective(AIObjective objective, bool addFirst = false)
		{
			Type type = objective.GetType();
			objective.SourceObjective = this;
			this.subObjectives.RemoveAll((AIObjective o) => o.GetType() == type);
			if (addFirst)
			{
				this.subObjectives.Insert(0, objective);
				return;
			}
			this.subObjectives.Add(objective);
		}

		// Token: 0x060002BE RID: 702 RVA: 0x00019B04 File Offset: 0x00017D04
		public void RemoveSubObjective<T>(ref T objective) where T : AIObjective
		{
			if (objective != null)
			{
				if (this.subObjectives.Contains(objective))
				{
					this.subObjectives.Remove(objective);
				}
				objective = default(T);
			}
		}

		// Token: 0x060002BF RID: 703 RVA: 0x00019B54 File Offset: 0x00017D54
		public void SortSubObjectives()
		{
			if (!this.AllowSubObjectiveSorting)
			{
				return;
			}
			if (this.subObjectives.None(null))
			{
				return;
			}
			AIObjective previousSubObjective = this.subObjectives.First<AIObjective>();
			this.subObjectives.ForEach(delegate(AIObjective so)
			{
				so.GetPriority();
			});
			this.subObjectives.Sort((AIObjective x, AIObjective y) => y.Priority.CompareTo(x.Priority));
			if (this.ConcurrentObjectives)
			{
				this.subObjectives.ForEach(delegate(AIObjective so)
				{
					so.SortSubObjectives();
				});
				return;
			}
			AIObjective currentSubObjective = this.subObjectives.First<AIObjective>();
			if (previousSubObjective != currentSubObjective)
			{
				previousSubObjective.OnDeselected();
				currentSubObjective.OnSelected();
			}
			currentSubObjective.SortSubObjectives();
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060002C0 RID: 704 RVA: 0x00019C30 File Offset: 0x00017E30
		public bool IsAllowed
		{
			get
			{
				return (this.AllowWhileHandcuffed || !this.character.LockHands) && (this.AllowOutsideSubmarine || this.character.Submarine != null) && !this.IsIgnoredAtOutpost() && (this.AllowInAnySub || ((this.AllowInFriendlySubs && this.character.Submarine.TeamID == CharacterTeamType.FriendlyNPC) || this.character.IsEscorted) || this.character.Submarine.TeamID == this.character.TeamID || this.character.Submarine.TeamID == this.character.OriginalTeamID);
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00019CE8 File Offset: 0x00017EE8
		public bool IsIgnoredAtOutpost()
		{
			if (!this.IgnoreAtOutpost)
			{
				return false;
			}
			if (!Level.IsLoadedFriendlyOutpost && !(GameMain.GameSession.GameMode is TestGameMode))
			{
				return false;
			}
			if (!this.character.IsOnPlayerTeam || this.character.IsFriendlyNPCTurnedHostile)
			{
				return false;
			}
			Submarine submarine = this.character.Submarine;
			return ((submarine != null) ? submarine.Info : null) != null && this.character.Submarine.Info.IsOutpost && this.character.Submarine.TeamID == CharacterTeamType.FriendlyNPC;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x00019D7C File Offset: 0x00017F7C
		protected void HandleDisallowed()
		{
			this.Priority = 0f;
			if (this.AbandonIfDisallowed && !this.IsIgnoredAtOutpost())
			{
				this.Abandon = true;
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00019DA0 File Offset: 0x00017FA0
		protected virtual float GetPriority()
		{
			if (!this.IsAllowed)
			{
				this.HandleDisallowed();
				return this.Priority;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				this.Priority = this.objectiveManager.GetOrderPriority(this);
			}
			else
			{
				this.Priority = this.BasePriority + this.CumulatedDevotion;
			}
			return this.Priority;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00019DFD File Offset: 0x00017FFD
		public float CalculatePriority()
		{
			this.ForceWalkTemporarily = false;
			this.Priority = this.GetPriority();
			this.ForceHighestPriority = false;
			return this.Priority;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00019E20 File Offset: 0x00018020
		public static float GetDistanceFactor(Vector2 selfPos, Vector2 targetWorldPos, float factorAtMaxDistance, float verticalDistanceMultiplier = 3f, float maxDistance = 10000f, float factorAtMinDistance = 1f)
		{
			float yDist = Math.Abs(selfPos.Y - targetWorldPos.Y);
			yDist = ((yDist > 100f) ? (yDist * verticalDistanceMultiplier) : 0f);
			float distance = Math.Abs(selfPos.X - targetWorldPos.X) + yDist;
			float distanceFactor = MathHelper.Lerp(factorAtMinDistance, factorAtMaxDistance, MathUtils.InverseLerp(0f, maxDistance, distance));
			if (factorAtMinDistance <= factorAtMaxDistance)
			{
				return MathHelper.Clamp(distanceFactor, factorAtMinDistance, factorAtMaxDistance);
			}
			return MathHelper.Clamp(distanceFactor, factorAtMaxDistance, factorAtMinDistance);
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00019E96 File Offset: 0x00018096
		protected float GetDistanceFactor(Vector2 targetWorldPos, float factorAtMaxDistance, float verticalDistanceMultiplier = 3f, float maxDistance = 10000f, float factorAtMinDistance = 1f)
		{
			return AIObjective.GetDistanceFactor(this.character.WorldPosition, targetWorldPos, factorAtMaxDistance, verticalDistanceMultiplier, maxDistance, factorAtMinDistance);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00019EB0 File Offset: 0x000180B0
		private void UpdateDevotion(float deltaTime)
		{
			AIObjective currentObjective = this.objectiveManager.CurrentObjective;
			if (currentObjective != null && (currentObjective == this || currentObjective.subObjectives.FirstOrDefault<AIObjective>() == this))
			{
				this.CumulatedDevotion += this.Devotion * deltaTime;
			}
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x00019EF3 File Offset: 0x000180F3
		public virtual bool IsDuplicate<T>(T otherObjective) where T : AIObjective
		{
			return otherObjective.Option == this.Option;
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00019F0C File Offset: 0x0001810C
		public virtual void Update(float deltaTime)
		{
			if (this.resetPriorityTimer > 0f)
			{
				this.resetPriorityTimer -= deltaTime;
			}
			else
			{
				this.ForceHighestPriority = false;
			}
			if (!this.objectiveManager.IsOrder(this) && this.objectiveManager.WaitTimer <= 0f)
			{
				this.UpdateDevotion(deltaTime);
			}
			this.subObjectives.ForEach(delegate(AIObjective so)
			{
				so.Update(deltaTime);
			});
		}

		// Token: 0x060002CA RID: 714 RVA: 0x00019F94 File Offset: 0x00018194
		protected virtual void SyncRemovedObjectives<T1, T2>(Dictionary<T1, T2> dictionary, IEnumerable<T1> collection) where T2 : AIObjective
		{
			foreach (T1 key in collection)
			{
				T2 objective;
				if (dictionary.TryGetValue(key, out objective) && !this.subObjectives.Contains(objective))
				{
					dictionary.Remove(key);
				}
			}
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00019FFC File Offset: 0x000181FC
		protected bool TryAddSubObjective<T>(ref T objective, Func<T> constructor, Action onCompleted = null, Action onAbandon = null) where T : AIObjective
		{
			if (objective != null)
			{
				if (!this.subObjectives.Contains(objective))
				{
					objective = default(T);
				}
				return false;
			}
			objective = constructor();
			if (!this.subObjectives.Contains(objective))
			{
				if (objective.AllowMultipleInstances)
				{
					objective.SourceObjective = this;
					this.subObjectives.Add(objective);
				}
				else
				{
					this.AddSubObjective(objective, false);
				}
				if (onCompleted != null)
				{
					objective.Completed += onCompleted;
				}
				if (onAbandon != null)
				{
					objective.Abandoned += onAbandon;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0001A0C9 File Offset: 0x000182C9
		public virtual void OnSelected()
		{
			this.Reset();
			Action selected = this.Selected;
			if (selected != null)
			{
				selected();
			}
			this.Selected = null;
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0001A0E9 File Offset: 0x000182E9
		public virtual void OnDeselected()
		{
			this.CumulatedDevotion = 0f;
			Action deselected = this.Deselected;
			if (deselected != null)
			{
				deselected();
			}
			this.Deselected = null;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0001A10E File Offset: 0x0001830E
		protected virtual void OnCompleted()
		{
			Action completed = this.Completed;
			if (completed != null)
			{
				completed();
			}
			this.Completed = null;
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0001A128 File Offset: 0x00018328
		protected virtual void OnAbandon()
		{
			Action abandoned = this.Abandoned;
			if (abandoned != null)
			{
				abandoned();
			}
			this.Abandoned = null;
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0001A142 File Offset: 0x00018342
		public virtual void Reset()
		{
			this.subObjectives.Clear();
			this.isCompleted = false;
			this.hasBeenChecked = false;
			this._abandon = false;
			this.CumulatedDevotion = 0f;
		}

		// Token: 0x060002D1 RID: 721
		protected abstract void Act(float deltaTime);

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060002D2 RID: 722 RVA: 0x0001A16F File Offset: 0x0001836F
		// (set) Token: 0x060002D3 RID: 723 RVA: 0x0001A186 File Offset: 0x00018386
		public bool IsCompleted
		{
			get
			{
				if (!this.hasBeenChecked)
				{
					this.CheckState();
				}
				return this.isCompleted;
			}
			protected set
			{
				if (this.isCompleted == value)
				{
					return;
				}
				this.isCompleted = value;
				if (this.isCompleted)
				{
					this.OnCompleted();
				}
			}
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0001A1A7 File Offset: 0x000183A7
		private bool Check()
		{
			if (this.isCompleted)
			{
				return true;
			}
			if (this.AbortCondition != null && this.AbortCondition(this))
			{
				this.Abandon = true;
				return false;
			}
			return this.CheckObjectiveState();
		}

		// Token: 0x060002D5 RID: 725
		protected abstract bool CheckObjectiveState();

		// Token: 0x060002D6 RID: 726 RVA: 0x0001A1D8 File Offset: 0x000183D8
		private bool CheckState()
		{
			this.hasBeenChecked = true;
			this.CheckSubObjectives();
			if ((this.subObjectives.None(null) || this.ConcurrentObjectives) && this.Check())
			{
				this.IsCompleted = true;
			}
			return this.isCompleted;
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0001A214 File Offset: 0x00018414
		private void CheckSubObjectives()
		{
			for (int i = 0; i < this.subObjectives.Count; i++)
			{
				AIObjective subObjective = this.subObjectives[i];
				subObjective.CheckState();
				if (subObjective.IsCompleted)
				{
					this.subObjectives.Remove(subObjective);
				}
				else if (!subObjective.CanBeCompleted)
				{
					this.subObjectives.Remove(subObjective);
					if (this.AbandonWhenCannotCompleteSubObjectives)
					{
						if (this.objectiveManager.IsOrder(this))
						{
							this.Reset();
						}
						else
						{
							this.Abandon = true;
						}
					}
				}
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0001A29C File Offset: 0x0001849C
		protected static bool CanPutInInventory(Character character, Item item, bool allowWearing)
		{
			if (item == null)
			{
				return false;
			}
			bool canEquip = false;
			if (item.AllowedSlots.Contains(InvSlotType.Any) && character.Inventory.IsAnySlotAvailable(item))
			{
				canEquip = true;
			}
			if (!canEquip)
			{
				CharacterInventory inv = character.Inventory;
				foreach (InvSlotType allowedSlot in item.AllowedSlots)
				{
					if (allowWearing || allowedSlot.HasFlag(InvSlotType.RightHand) || allowedSlot.HasFlag(InvSlotType.LeftHand))
					{
						foreach (InvSlotType slotType in inv.SlotTypes)
						{
							if (allowedSlot.HasFlag(slotType))
							{
								for (int i = 0; i < inv.Capacity; i++)
								{
									canEquip = true;
									if (allowedSlot.HasFlag(inv.SlotTypes[i]) && inv.GetItemAt(i) != null)
									{
										canEquip = false;
										break;
									}
								}
							}
						}
					}
				}
			}
			return canEquip && character.Inventory.CanBePut(item);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0001A3CC File Offset: 0x000185CC
		protected bool CanEquip(Item item, bool allowWearing)
		{
			return AIObjective.CanPutInInventory(this.character, item, allowWearing);
		}

		// Token: 0x040001D0 RID: 464
		public EventAction SourceEventAction;

		// Token: 0x040001D1 RID: 465
		public AIObjective SourceObjective;

		// Token: 0x040001D2 RID: 466
		protected readonly List<AIObjective> subObjectives = new List<AIObjective>();

		// Token: 0x040001D3 RID: 467
		private float _cumulatedDevotion;

		// Token: 0x040001D7 RID: 471
		private float resetPriorityTimer;

		// Token: 0x040001D8 RID: 472
		private readonly float resetPriorityTime = 1f;

		// Token: 0x040001D9 RID: 473
		private bool _forceHighestPriority;

		// Token: 0x040001DD RID: 477
		public readonly Character character;

		// Token: 0x040001DE RID: 478
		public readonly AIObjectiveManager objectiveManager;

		// Token: 0x040001DF RID: 479
		public readonly Identifier Option;

		// Token: 0x040001E0 RID: 480
		private bool _abandon;

		// Token: 0x040001E1 RID: 481
		private readonly List<AIObjective> all = new List<AIObjective>();

		// Token: 0x040001E2 RID: 482
		public Func<AIObjective, bool> AbortCondition;

		// Token: 0x040001E7 RID: 487
		private bool isCompleted;

		// Token: 0x040001E8 RID: 488
		private bool hasBeenChecked;
	}
}
