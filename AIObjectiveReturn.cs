using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200018D RID: 397
	internal class AIObjectiveReturn : AIObjective
	{
		// Token: 0x17000BF6 RID: 3062
		// (get) Token: 0x06002ECA RID: 11978 RVA: 0x001F275F File Offset: 0x001F095F
		// (set) Token: 0x06002ECB RID: 11979 RVA: 0x001F2767 File Offset: 0x001F0967
		public override Identifier Identifier { get; set; } = "return".ToIdentifier();

		// Token: 0x17000BF7 RID: 3063
		// (get) Token: 0x06002ECC RID: 11980 RVA: 0x001F2770 File Offset: 0x001F0970
		public Submarine Target { get; }

		// Token: 0x17000BF8 RID: 3064
		// (get) Token: 0x06002ECD RID: 11981 RVA: 0x001F2778 File Offset: 0x001F0978
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BF9 RID: 3065
		// (get) Token: 0x06002ECE RID: 11982 RVA: 0x001F277B File Offset: 0x001F097B
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002ECF RID: 11983 RVA: 0x001F2780 File Offset: 0x001F0980
		public AIObjectiveReturn(Character character, Character orderGiver, AIObjectiveManager objectiveManager, float priorityModifier = 1f)
		{
			AIObjectiveReturn.<>c__DisplayClass15_0 CS$<>8__locals1;
			CS$<>8__locals1.orderGiver = orderGiver;
			CS$<>8__locals1.character = character;
			base..ctor(CS$<>8__locals1.character, objectiveManager, priorityModifier, default(Identifier));
			this.Target = (AIObjectiveReturn.<.ctor>g__GetReturnTarget|15_0(Submarine.MainSubs, ref CS$<>8__locals1) ?? AIObjectiveReturn.<.ctor>g__GetReturnTarget|15_0(Submarine.Loaded, ref CS$<>8__locals1));
			if (this.Target == null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode))
				{
					DebugConsole.AddWarning("(" + CS$<>8__locals1.character.DisplayName + ") No suitable return target found. Cannot return back to the main sub.", null);
				}
				base.Abandon = true;
			}
		}

		// Token: 0x06002ED0 RID: 11984 RVA: 0x001F2830 File Offset: 0x001F0A30
		protected override float GetPriority()
		{
			if (!base.Abandon && !base.IsCompleted && this.objectiveManager.IsOrder(this))
			{
				base.Priority = this.objectiveManager.GetOrderPriority(this);
			}
			else
			{
				base.Priority = 59f;
			}
			return base.Priority;
		}

		// Token: 0x06002ED1 RID: 11985 RVA: 0x001F2880 File Offset: 0x001F0A80
		protected override void Act(float deltaTime)
		{
			if (this.Target == null)
			{
				base.Abandon = true;
				return;
			}
			bool shouldUseEscapeBehavior = false;
			if (this.character.CurrentHull != null || this.isSteeringThroughGap)
			{
				if (this.character.Submarine == null || !this.character.Submarine.IsConnectedTo(this.Target))
				{
					shouldUseEscapeBehavior = true;
					if (!this.usingEscapeBehavior)
					{
						base.HumanAIController.ResetEscape();
					}
					this.isSteeringThroughGap = base.HumanAIController.Escape(deltaTime);
					if (!this.isSteeringThroughGap && (base.HumanAIController.EscapeTarget == null || base.HumanAIController.IsCurrentPathUnreachable))
					{
						base.Abandon = true;
					}
				}
				else if (this.character.Submarine != this.Target)
				{
					if (this.moveInsideObjective == null)
					{
						Hull targetHull = null;
						foreach (DockingPort d in this.Target.ConnectedDockingPorts.Values)
						{
							if (d.Docked && d.DockingTarget != null && d.DockingTarget.Item.Submarine == this.character.Submarine)
							{
								targetHull = d.Item.CurrentHull;
								break;
							}
						}
						if (targetHull != null && !targetHull.IsAirlock)
						{
							float closestDist = 0f;
							Hull airlock = null;
							foreach (Hull hull in Hull.HullList)
							{
								if (hull.Submarine == targetHull.Submarine && hull.IsAirlock)
								{
									float dist = Vector2.DistanceSquared(targetHull.Position, hull.Position);
									if (airlock == null || closestDist <= 0f || dist < closestDist)
									{
										airlock = hull;
										closestDist = dist;
									}
								}
							}
							if (airlock != null)
							{
								targetHull = airlock;
							}
						}
						if (targetHull != null)
						{
							base.RemoveSubObjective<AIObjectiveGoTo>(ref this.moveOutsideObjective);
							Func<PathNode, bool> <>9__3;
							base.TryAddSubObjective<AIObjectiveGoTo>(ref this.moveInsideObjective, delegate
							{
								AIObjectiveGoTo aiobjectiveGoTo = new AIObjectiveGoTo(targetHull, this.character, this.objectiveManager, false, true, 1f, 0f);
								aiobjectiveGoTo.AllowGoingOutside = true;
								Func<PathNode, bool> endNodeFilter;
								if ((endNodeFilter = <>9__3) == null)
								{
									endNodeFilter = (<>9__3 = ((PathNode n) => n.Waypoint.Submarine == targetHull.Submarine));
								}
								aiobjectiveGoTo.endNodeFilter = endNodeFilter;
								return aiobjectiveGoTo;
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.moveInsideObjective);
							}, delegate
							{
								base.Abandon = true;
							});
						}
					}
				}
				else
				{
					base.IsCompleted = true;
				}
			}
			else if (!this.isSteeringThroughGap && this.moveOutsideObjective == null)
			{
				Hull targetHull = null;
				float targetDistanceSquared = float.MaxValue;
				bool targetIsAirlock = false;
				foreach (Hull hull2 in this.Target.GetHulls(false))
				{
					bool hullIsAirlock = hull2.IsAirlock;
					if (hullIsAirlock || (!targetIsAirlock && hull2.LeadsOutside(this.character)))
					{
						float distanceSquared = Vector2.DistanceSquared(this.character.WorldPosition, hull2.WorldPosition);
						if (targetHull == null || distanceSquared < targetDistanceSquared)
						{
							targetHull = hull2;
							targetDistanceSquared = distanceSquared;
							targetIsAirlock = hullIsAirlock;
						}
					}
				}
				if (targetHull != null)
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.moveInsideObjective);
					base.TryAddSubObjective<AIObjectiveGoTo>(ref this.moveOutsideObjective, () => new AIObjectiveGoTo(targetHull, this.character, this.objectiveManager, false, true, 1f, 0f)
					{
						AllowGoingOutside = true
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.moveOutsideObjective);
					}, delegate
					{
						base.Abandon = true;
					});
				}
			}
			this.usingEscapeBehavior = shouldUseEscapeBehavior;
		}

		// Token: 0x06002ED2 RID: 11986 RVA: 0x001F2C30 File Offset: 0x001F0E30
		protected override bool CheckObjectiveState()
		{
			if (this.Target == null)
			{
				base.Abandon = true;
				return false;
			}
			if (this.character.Submarine == this.Target)
			{
				base.IsCompleted = true;
			}
			return base.IsCompleted;
		}

		// Token: 0x06002ED3 RID: 11987 RVA: 0x001F2C63 File Offset: 0x001F0E63
		public override void Reset()
		{
			base.Reset();
			this.moveInsideObjective = null;
			this.moveOutsideObjective = null;
			this.usingEscapeBehavior = false;
			this.isSteeringThroughGap = false;
			base.HumanAIController.ResetEscape();
		}

		// Token: 0x06002ED4 RID: 11988 RVA: 0x001F2C94 File Offset: 0x001F0E94
		protected override void OnAbandon()
		{
			base.OnAbandon();
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager != null)
			{
				steeringManager.Reset();
			}
			if (this.character.IsOnPlayerTeam && this.objectiveManager.CurrentOrder == this.objectiveManager.CurrentObjective)
			{
				string msg = TextManager.Get("dialogcannotreturn").Value;
				if (!msg.IsNullOrEmpty())
				{
					Character character = this.character;
					string message = msg;
					Identifier identifier = "dialogcannotreturn".ToIdentifier();
					character.Speak(message, null, 0f, identifier, 5f);
				}
			}
		}

		// Token: 0x06002ED5 RID: 11989 RVA: 0x001F2D20 File Offset: 0x001F0F20
		[CompilerGenerated]
		internal static Submarine <.ctor>g__GetReturnTarget|15_0(IEnumerable<Submarine> subs, ref AIObjectiveReturn.<>c__DisplayClass15_0 A_1)
		{
			Character orderGiver = A_1.orderGiver;
			CharacterTeamType? characterTeamType;
			if (orderGiver == null)
			{
				Character character = A_1.character;
				characterTeamType = ((character != null) ? new CharacterTeamType?(character.TeamID) : null);
			}
			else
			{
				characterTeamType = new CharacterTeamType?(orderGiver.TeamID);
			}
			CharacterTeamType? requiredTeamID = characterTeamType;
			Submarine returnTarget = null;
			foreach (Submarine sub in subs)
			{
				if (sub != null)
				{
					CharacterTeamType teamID = sub.TeamID;
					CharacterTeamType? characterTeamType2 = requiredTeamID;
					if (teamID == characterTeamType2.GetValueOrDefault() & characterTeamType2 != null)
					{
						returnTarget = sub;
						break;
					}
				}
			}
			return returnTarget;
		}

		// Token: 0x04001858 RID: 6232
		private AIObjectiveGoTo moveInsideObjective;

		// Token: 0x04001859 RID: 6233
		private AIObjectiveGoTo moveOutsideObjective;

		// Token: 0x0400185A RID: 6234
		private bool usingEscapeBehavior;

		// Token: 0x0400185B RID: 6235
		private bool isSteeringThroughGap;
	}
}
