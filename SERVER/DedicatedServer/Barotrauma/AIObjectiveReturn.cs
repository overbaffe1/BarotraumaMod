using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000087 RID: 135
	internal class AIObjectiveReturn : AIObjective
	{
		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x060011C2 RID: 4546 RVA: 0x0009F8C7 File Offset: 0x0009DAC7
		// (set) Token: 0x060011C3 RID: 4547 RVA: 0x0009F8CF File Offset: 0x0009DACF
		public override Identifier Identifier { get; set; } = "return".ToIdentifier();

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x060011C4 RID: 4548 RVA: 0x0009F8D8 File Offset: 0x0009DAD8
		public Submarine Target { get; }

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x060011C5 RID: 4549 RVA: 0x0009F8E0 File Offset: 0x0009DAE0
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x060011C6 RID: 4550 RVA: 0x0009F8E3 File Offset: 0x0009DAE3
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060011C7 RID: 4551 RVA: 0x0009F8E8 File Offset: 0x0009DAE8
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

		// Token: 0x060011C8 RID: 4552 RVA: 0x0009F998 File Offset: 0x0009DB98
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

		// Token: 0x060011C9 RID: 4553 RVA: 0x0009F9E8 File Offset: 0x0009DBE8
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

		// Token: 0x060011CA RID: 4554 RVA: 0x0009FD98 File Offset: 0x0009DF98
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

		// Token: 0x060011CB RID: 4555 RVA: 0x0009FDCB File Offset: 0x0009DFCB
		public override void Reset()
		{
			base.Reset();
			this.moveInsideObjective = null;
			this.moveOutsideObjective = null;
			this.usingEscapeBehavior = false;
			this.isSteeringThroughGap = false;
			base.HumanAIController.ResetEscape();
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x0009FDFC File Offset: 0x0009DFFC
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

		// Token: 0x060011CD RID: 4557 RVA: 0x0009FE88 File Offset: 0x0009E088
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

		// Token: 0x0400085E RID: 2142
		private AIObjectiveGoTo moveInsideObjective;

		// Token: 0x0400085F RID: 2143
		private AIObjectiveGoTo moveOutsideObjective;

		// Token: 0x04000860 RID: 2144
		private bool usingEscapeBehavior;

		// Token: 0x04000861 RID: 2145
		private bool isSteeringThroughGap;
	}
}
