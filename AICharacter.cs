using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000024 RID: 36
	internal class AICharacter : Character
	{
		// Token: 0x06000305 RID: 773 RVA: 0x0001B7A1 File Offset: 0x000199A1
		public override void DrawFront(SpriteBatch spriteBatch, Camera cam)
		{
			base.DrawFront(spriteBatch, cam);
			if (GameMain.DebugDraw && !base.IsDead)
			{
				this.aiController.DebugDraw(spriteBatch);
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000306 RID: 774 RVA: 0x0001B7C6 File Offset: 0x000199C6
		public override AIController AIController
		{
			get
			{
				return this.aiController;
			}
		}

		// Token: 0x06000307 RID: 775 RVA: 0x0001B7D0 File Offset: 0x000199D0
		public AICharacter(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo = null, ushort id = 0, bool isNetworkPlayer = false, RagdollParams ragdoll = null, bool spawnInitialItems = true) : base(prefab, position, seed, characterInfo, id, isNetworkPlayer, ragdoll, spawnInitialItems)
		{
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0001B7F0 File Offset: 0x000199F0
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

		// Token: 0x06000309 RID: 777 RVA: 0x0001B85C File Offset: 0x00019A5C
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

		// Token: 0x040001F8 RID: 504
		private AIController aiController;
	}
}
