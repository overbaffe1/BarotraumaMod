using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009D RID: 157
	internal class SwarmBehavior
	{
		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x060012F0 RID: 4848 RVA: 0x000A5C81 File Offset: 0x000A3E81
		// (set) Token: 0x060012F1 RID: 4849 RVA: 0x000A5C89 File Offset: 0x000A3E89
		public bool ForceActive { get; private set; }

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x060012F2 RID: 4850 RVA: 0x000A5C92 File Offset: 0x000A3E92
		// (set) Token: 0x060012F3 RID: 4851 RVA: 0x000A5C9A File Offset: 0x000A3E9A
		public List<AICharacter> Members { get; private set; } = new List<AICharacter>();

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x060012F4 RID: 4852 RVA: 0x000A5CA3 File Offset: 0x000A3EA3
		// (set) Token: 0x060012F5 RID: 4853 RVA: 0x000A5CAB File Offset: 0x000A3EAB
		public HashSet<AICharacter> ActiveMembers { get; private set; } = new HashSet<AICharacter>();

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x060012F6 RID: 4854 RVA: 0x000A5CB4 File Offset: 0x000A3EB4
		// (set) Token: 0x060012F7 RID: 4855 RVA: 0x000A5CBC File Offset: 0x000A3EBC
		public bool IsActive { get; set; }

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x060012F8 RID: 4856 RVA: 0x000A5CC5 File Offset: 0x000A3EC5
		public bool IsEnoughMembers
		{
			get
			{
				return this.ActiveMembers.Count > 1;
			}
		}

		// Token: 0x060012F9 RID: 4857 RVA: 0x000A5CD8 File Offset: 0x000A3ED8
		public SwarmBehavior(XElement element, EnemyAIController ai)
		{
			this.ai = ai;
			this.minDistFromClosest = ConvertUnits.ToSimUnits(element.GetAttributeFloat("minDistFromClosest", 10f));
			this.maxDistFromCenter = ConvertUnits.ToSimUnits(element.GetAttributeFloat("maxDistFromCenter", 1000f));
			this.cohesion = element.GetAttributeFloat("cohesion", 1f) / 10f;
			this.ForceActive = element.GetAttributeBool("ForceActive", false);
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x000A5D6C File Offset: 0x000A3F6C
		public static void CreateSwarm(IEnumerable<AICharacter> swarm)
		{
			List<EnemyAIController> aiControllers = new List<EnemyAIController>();
			foreach (AICharacter character in swarm)
			{
				EnemyAIController enemyAI = character.AIController as EnemyAIController;
				if (enemyAI != null && enemyAI.SwarmBehavior != null)
				{
					aiControllers.Add(enemyAI);
				}
			}
			IEnumerable<AICharacter> filteredMembers = from m in aiControllers
			select m.Character as AICharacter into m
			where m != null
			select m;
			foreach (EnemyAIController ai in aiControllers)
			{
				ai.SwarmBehavior.Members = filteredMembers.ToList<AICharacter>();
			}
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x000A5E6C File Offset: 0x000A406C
		public void Refresh()
		{
			this.Members.RemoveAll(delegate(AICharacter m)
			{
				if (!m.IsDead && !m.Removed)
				{
					EnemyAIController ai = m.AIController as EnemyAIController;
					return ai != null && ai.State == AIState.Flee;
				}
				return true;
			});
			foreach (AICharacter member in this.Members)
			{
				if ((!member.AIController.Enabled && member.IsRemotePlayer) || Character.Controlled == member || !((EnemyAIController)member.AIController).SwarmBehavior.IsActive)
				{
					this.ActiveMembers.Remove(member);
				}
				else
				{
					this.ActiveMembers.Add(member);
				}
			}
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x000A5F34 File Offset: 0x000A4134
		public void UpdateSteering(float deltaTime)
		{
			if (!this.IsActive)
			{
				return;
			}
			if (!this.IsEnoughMembers)
			{
				return;
			}
			float closestDistSqr = float.MaxValue;
			Vector2 center = Vector2.Zero;
			AICharacter closest = null;
			foreach (AICharacter member in this.Members)
			{
				center += member.SimPosition;
				if (member != this.ai.Character)
				{
					float distSqr = Vector2.DistanceSquared(member.SimPosition, this.ai.Character.SimPosition);
					if (distSqr < closestDistSqr)
					{
						closestDistSqr = distSqr;
						closest = member;
					}
				}
			}
			center /= (float)this.Members.Count;
			if (closest == null)
			{
				return;
			}
			float closestDist = (float)Math.Sqrt((double)closestDistSqr);
			if (closestDist < this.minDistFromClosest)
			{
				Vector2 diff = closest.SimPosition - this.ai.SimPosition;
				if (diff.LengthSquared() < 0.0001f)
				{
					diff = Vector2.UnitX;
				}
				this.ai.SteeringManager.SteeringManual(deltaTime, -diff);
			}
			else if (Vector2.DistanceSquared(center, this.ai.SimPosition) > this.maxDistFromCenter * this.maxDistFromCenter)
			{
				float distFromCenter = Vector2.Distance(center, this.ai.SimPosition);
				this.ai.SteeringManager.SteeringSeek(center, (distFromCenter - this.maxDistFromCenter) / 10f);
			}
			if (this.cohesion > 0f)
			{
				Vector2 avgVel = Vector2.Zero;
				foreach (AICharacter member2 in this.Members)
				{
					avgVel += member2.AnimController.TargetMovement;
				}
				avgVel /= (float)this.Members.Count;
				this.ai.SteeringManager.SteeringManual(deltaTime, avgVel * this.cohesion);
			}
		}

		// Token: 0x04000901 RID: 2305
		private readonly float minDistFromClosest;

		// Token: 0x04000902 RID: 2306
		private readonly float maxDistFromCenter;

		// Token: 0x04000903 RID: 2307
		private readonly float cohesion;

		// Token: 0x04000907 RID: 2311
		private EnemyAIController ai;
	}
}
