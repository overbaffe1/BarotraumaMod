using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A1 RID: 161
	internal class AICharacter : Character
	{
		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x06001362 RID: 4962 RVA: 0x000A7AAE File Offset: 0x000A5CAE
		public override AIController AIController
		{
			get
			{
				return this.aiController;
			}
		}

		// Token: 0x06001363 RID: 4963 RVA: 0x000A7AB8 File Offset: 0x000A5CB8
		public AICharacter(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isNetworkPlayer = false, RagdollParams ragdoll = null, bool spawnInitialItems = true) : base(prefab, position, seed, characterInfo, id, isNetworkPlayer, ragdoll, spawnInitialItems)
		{
		}

		// Token: 0x06001364 RID: 4964 RVA: 0x000A7AD8 File Offset: 0x000A5CD8
		public void SetAI(AIController aiController)
		{
			if (this.AIController != null)
			{
				this.OnAttacked = (Character.OnAttackedHandler)Delegate.Remove(this.OnAttacked, new Character.OnAttackedHandler(this.AIController.OnAttacked));
			}
			this.aiController = aiController;
			if (aiController != null)
			{
				this.OnAttacked = (Character.OnAttackedHandler)Delegate.Combine(this.OnAttacked, new Character.OnAttackedHandler(aiController.OnAttacked));
			}
		}

		// Token: 0x06001365 RID: 4965 RVA: 0x000A7B44 File Offset: 0x000A5D44
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (!base.Enabled)
			{
				return;
			}
			if (!base.IsRemotePlayer)
			{
				EnemyAIController enemyAi = this.AIController as EnemyAIController;
				if (enemyAi != null)
				{
					PetBehavior petBehavior = enemyAi.PetBehavior;
					if (petBehavior != null)
					{
						petBehavior.Update(deltaTime);
					}
				}
			}
			if (base.IsDead || base.IsUnconscious || base.IsIncapacitated || base.CharacterHealth.Stun > 0f)
			{
				this.AnimController.SimplePhysicsEnabled = false;
				return;
			}
			if (!base.IsRemotePlayer && !(this.AIController is HumanAIController))
			{
				float characterDistSqr = base.GetDistanceSqrToClosestPlayer();
				if (characterDistSqr > MathUtils.Pow2(this.Params.DisableDistance * 0.5f))
				{
					this.AnimController.SimplePhysicsEnabled = true;
				}
				else if (characterDistSqr < MathUtils.Pow2(this.Params.DisableDistance * 0.5f * 0.9f))
				{
					this.AnimController.SimplePhysicsEnabled = false;
				}
			}
			else
			{
				this.AnimController.SimplePhysicsEnabled = false;
			}
			if (GameMain.NetworkMember != null && !GameMain.NetworkMember.IsServer)
			{
				return;
			}
			if (Character.Controlled == this)
			{
				return;
			}
			if (!base.IsRemotelyControlled && this.aiController != null && this.aiController.Enabled)
			{
				this.aiController.Update(deltaTime);
			}
		}

		// Token: 0x04000933 RID: 2355
		private AIController aiController;
	}
}
