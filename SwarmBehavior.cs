using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A3 RID: 419
	internal class SwarmBehavior
	{
		// Token: 0x17000C63 RID: 3171
		// (get) Token: 0x06002FF8 RID: 12280 RVA: 0x001F8B61 File Offset: 0x001F6D61
		// (set) Token: 0x06002FF9 RID: 12281 RVA: 0x001F8B69 File Offset: 0x001F6D69
		public bool ForceActive { get; private set; }

		// Token: 0x17000C64 RID: 3172
		// (get) Token: 0x06002FFA RID: 12282 RVA: 0x001F8B72 File Offset: 0x001F6D72
		// (set) Token: 0x06002FFB RID: 12283 RVA: 0x001F8B7A File Offset: 0x001F6D7A
		public List<AICharacter> Members { get; private set; } = new List<AICharacter>();

		// Token: 0x17000C65 RID: 3173
		// (get) Token: 0x06002FFC RID: 12284 RVA: 0x001F8B83 File Offset: 0x001F6D83
		// (set) Token: 0x06002FFD RID: 12285 RVA: 0x001F8B8B File Offset: 0x001F6D8B
		public HashSet<AICharacter> ActiveMembers { get; private set; } = new HashSet<AICharacter>();

		// Token: 0x17000C66 RID: 3174
		// (get) Token: 0x06002FFE RID: 12286 RVA: 0x001F8B94 File Offset: 0x001F6D94
		// (set) Token: 0x06002FFF RID: 12287 RVA: 0x001F8B9C File Offset: 0x001F6D9C
		public bool IsActive { get; set; }

		// Token: 0x17000C67 RID: 3175
		// (get) Token: 0x06003000 RID: 12288 RVA: 0x001F8BA5 File Offset: 0x001F6DA5
		public bool IsEnoughMembers
		{
			get
			{
				return this.ActiveMembers.Count > 1;
			}
		}

		// Token: 0x06003001 RID: 12289 RVA: 0x001F8BB8 File Offset: 0x001F6DB8
		public SwarmBehavior(XElement element, EnemyAIController ai)
		{
			this.ai = ai;
			this.minDistFromClosest = ConvertUnits.ToSimUnits(element.GetAttributeFloat("minDistFromClosest", 10f));
			this.maxDistFromCenter = ConvertUnits.ToSimUnits(element.GetAttributeFloat("maxDistFromCenter", 1000f));
			this.cohesion = element.GetAttributeFloat("cohesion", 1f) / 10f;
			this.ForceActive = element.GetAttributeBool("ForceActive", false);
		}

		// Token: 0x06003002 RID: 12290 RVA: 0x001F8C4C File Offset: 0x001F6E4C
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

		// Token: 0x06003003 RID: 12291 RVA: 0x001F8D4C File Offset: 0x001F6F4C
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

		// Token: 0x06003004 RID: 12292 RVA: 0x001F8E14 File Offset: 0x001F7014
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

		// Token: 0x040018FB RID: 6395
		private readonly float minDistFromClosest;

		// Token: 0x040018FC RID: 6396
		private readonly float maxDistFromCenter;

		// Token: 0x040018FD RID: 6397
		private readonly float cohesion;

		// Token: 0x04001901 RID: 6401
		private EnemyAIController ai;
	}
}
