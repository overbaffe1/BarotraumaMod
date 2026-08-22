using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D3 RID: 979
	[NullableContext(2)]
	[Nullable(0)]
	internal class BallastFloraBranch : VineTile
	{
		// Token: 0x17000F89 RID: 3977
		// (get) Token: 0x060038A1 RID: 14497 RVA: 0x0017A7F0 File Offset: 0x001789F0
		// (set) Token: 0x060038A2 RID: 14498 RVA: 0x0017A7F8 File Offset: 0x001789F8
		public float Health
		{
			get
			{
				return this.health;
			}
			set
			{
				this.health = MathHelper.Clamp(value, 0f, this.MaxHealth);
			}
		}

		// Token: 0x17000F8A RID: 3978
		// (get) Token: 0x060038A3 RID: 14499 RVA: 0x0017A811 File Offset: 0x00178A11
		// (set) Token: 0x060038A4 RID: 14500 RVA: 0x0017A819 File Offset: 0x00178A19
		public BallastFloraBranch ParentBranch
		{
			get
			{
				return this.parentBranch;
			}
			set
			{
				if (value != this.parentBranch)
				{
					this.parentBranch = value;
					if (this.parentBranch != null)
					{
						this.BranchDepth = this.parentBranch.BranchDepth + 1;
					}
				}
			}
		}

		// Token: 0x17000F8B RID: 3979
		// (get) Token: 0x060038A5 RID: 14501 RVA: 0x0017A846 File Offset: 0x00178A46
		// (set) Token: 0x060038A6 RID: 14502 RVA: 0x0017A84E File Offset: 0x00178A4E
		public int BranchDepth { get; private set; }

		// Token: 0x060038A7 RID: 14503 RVA: 0x0017A858 File Offset: 0x00178A58
		public BallastFloraBranch(BallastFloraBehavior parent, BallastFloraBranch parentBranch, Vector2 position, VineTileType type, FoliageConfig? flowerConfig = null, FoliageConfig? leafConfig = null, Rectangle? rect = null) : base(null, position, type, flowerConfig, leafConfig, rect)
		{
			this.ParentBranch = parentBranch;
			this.ParentBallastFlora = parent;
		}

		// Token: 0x060038A8 RID: 14504 RVA: 0x0017A8E0 File Offset: 0x00178AE0
		public void UpdateHealth()
		{
			if (this.MaxHealth <= this.Health)
			{
				return;
			}
			Color healthColor = Color.White * (1f - this.Health / this.MaxHealth);
			this.HealthColor = Color.Lerp(this.HealthColor, healthColor, 0.05f);
		}

		// Token: 0x060038A9 RID: 14505 RVA: 0x0017A934 File Offset: 0x00178B34
		public void UpdatePulse(float deltaTime, float inflateSpeed, float deflateSpeed, float delay)
		{
			if (this.ParentBallastFlora == null || this.DisconnectedFromRoot)
			{
				return;
			}
			if (this.pulseDelay > 0f)
			{
				this.pulseDelay -= deltaTime;
				return;
			}
			if (this.inflate)
			{
				this.Pulse += inflateSpeed * deltaTime;
				if (this.Pulse > 1.25f)
				{
					this.inflate = false;
					return;
				}
			}
			else
			{
				this.Pulse -= deflateSpeed * deltaTime;
				if (this.Pulse < 1f)
				{
					this.inflate = true;
					this.pulseDelay = delay;
				}
			}
		}

		// Token: 0x04001C75 RID: 7285
		public readonly BallastFloraBehavior ParentBallastFlora;

		// Token: 0x04001C76 RID: 7286
		public int ID = -1;

		// Token: 0x04001C77 RID: 7287
		public Item ClaimedItem;

		// Token: 0x04001C78 RID: 7288
		public int ClaimedItemId = -1;

		// Token: 0x04001C79 RID: 7289
		public float MaxHealth = 100f;

		// Token: 0x04001C7A RID: 7290
		private float health = 100f;

		// Token: 0x04001C7B RID: 7291
		public float RemoveTimer = 60f;

		// Token: 0x04001C7C RID: 7292
		public bool SpawningItem;

		// Token: 0x04001C7D RID: 7293
		public Item AttackItem;

		// Token: 0x04001C7E RID: 7294
		public bool IsRoot;

		// Token: 0x04001C7F RID: 7295
		public bool IsRootGrowth;

		// Token: 0x04001C80 RID: 7296
		public bool Removed;

		// Token: 0x04001C81 RID: 7297
		public bool DisconnectedFromRoot;

		// Token: 0x04001C82 RID: 7298
		public Hull CurrentHull;

		// Token: 0x04001C83 RID: 7299
		public float Pulse = 1f;

		// Token: 0x04001C84 RID: 7300
		private bool inflate;

		// Token: 0x04001C85 RID: 7301
		private float pulseDelay = Rand.Range(0f, 3f, Rand.RandSync.Unsynced);

		// Token: 0x04001C86 RID: 7302
		private BallastFloraBranch parentBranch;

		// Token: 0x04001C88 RID: 7304
		public float AccumulatedDamage;

		// Token: 0x04001C89 RID: 7305
		public float DamageVisualizationTimer;

		// Token: 0x04001C8A RID: 7306
		[Nullable(1)]
		public readonly Dictionary<TileSide, BallastFloraBranch> Connections = new Dictionary<TileSide, BallastFloraBranch>();
	}
}
