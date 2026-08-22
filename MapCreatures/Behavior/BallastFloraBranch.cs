using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004DB RID: 1243
	[NullableContext(2)]
	[Nullable(0)]
	internal class BallastFloraBranch : VineTile
	{
		// Token: 0x170014C8 RID: 5320
		// (get) Token: 0x0600514D RID: 20813 RVA: 0x002BC478 File Offset: 0x002BA678
		// (set) Token: 0x0600514E RID: 20814 RVA: 0x002BC480 File Offset: 0x002BA680
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

		// Token: 0x170014C9 RID: 5321
		// (get) Token: 0x0600514F RID: 20815 RVA: 0x002BC499 File Offset: 0x002BA699
		// (set) Token: 0x06005150 RID: 20816 RVA: 0x002BC4A1 File Offset: 0x002BA6A1
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

		// Token: 0x170014CA RID: 5322
		// (get) Token: 0x06005151 RID: 20817 RVA: 0x002BC4CE File Offset: 0x002BA6CE
		// (set) Token: 0x06005152 RID: 20818 RVA: 0x002BC4D6 File Offset: 0x002BA6D6
		public int BranchDepth { get; private set; }

		// Token: 0x06005153 RID: 20819 RVA: 0x002BC4E0 File Offset: 0x002BA6E0
		public BallastFloraBranch(BallastFloraBehavior parent, BallastFloraBranch parentBranch, Vector2 position, VineTileType type, FoliageConfig? flowerConfig = null, FoliageConfig? leafConfig = null, Rectangle? rect = null) : base(null, position, type, flowerConfig, leafConfig, rect)
		{
			this.ParentBranch = parentBranch;
			this.ParentBallastFlora = parent;
		}

		// Token: 0x06005154 RID: 20820 RVA: 0x002BC568 File Offset: 0x002BA768
		public void UpdateHealth()
		{
			if (this.MaxHealth <= this.Health)
			{
				return;
			}
			Color healthColor = Color.White * (1f - this.Health / this.MaxHealth);
			this.HealthColor = Color.Lerp(this.HealthColor, healthColor, 0.05f);
		}

		// Token: 0x06005155 RID: 20821 RVA: 0x002BC5BC File Offset: 0x002BA7BC
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

		// Token: 0x04002B0D RID: 11021
		public readonly BallastFloraBehavior ParentBallastFlora;

		// Token: 0x04002B0E RID: 11022
		public int ID = -1;

		// Token: 0x04002B0F RID: 11023
		public Item ClaimedItem;

		// Token: 0x04002B10 RID: 11024
		public int ClaimedItemId = -1;

		// Token: 0x04002B11 RID: 11025
		public float MaxHealth = 100f;

		// Token: 0x04002B12 RID: 11026
		private float health = 100f;

		// Token: 0x04002B13 RID: 11027
		public float RemoveTimer = 60f;

		// Token: 0x04002B14 RID: 11028
		public bool SpawningItem;

		// Token: 0x04002B15 RID: 11029
		public Item AttackItem;

		// Token: 0x04002B16 RID: 11030
		public bool IsRoot;

		// Token: 0x04002B17 RID: 11031
		public bool IsRootGrowth;

		// Token: 0x04002B18 RID: 11032
		public bool Removed;

		// Token: 0x04002B19 RID: 11033
		public bool DisconnectedFromRoot;

		// Token: 0x04002B1A RID: 11034
		public Hull CurrentHull;

		// Token: 0x04002B1B RID: 11035
		public float Pulse = 1f;

		// Token: 0x04002B1C RID: 11036
		private bool inflate;

		// Token: 0x04002B1D RID: 11037
		private float pulseDelay = Rand.Range(0f, 3f, Rand.RandSync.Unsynced);

		// Token: 0x04002B1E RID: 11038
		private BallastFloraBranch parentBranch;

		// Token: 0x04002B20 RID: 11040
		public float AccumulatedDamage;

		// Token: 0x04002B21 RID: 11041
		public float DamageVisualizationTimer;

		// Token: 0x04002B22 RID: 11042
		public Vector2 ShakeAmount;

		// Token: 0x04002B23 RID: 11043
		[Nullable(1)]
		public readonly Dictionary<TileSide, BallastFloraBranch> Connections = new Dictionary<TileSide, BallastFloraBranch>();
	}
}
