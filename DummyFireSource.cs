using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000312 RID: 786
	internal class DummyFireSource : FireSource
	{
		// Token: 0x17001076 RID: 4214
		// (get) Token: 0x06003ED1 RID: 16081 RVA: 0x00233DC1 File Offset: 0x00231FC1
		protected override float SpreadToOtherHullsProbability
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x06003ED2 RID: 16082 RVA: 0x00233DC8 File Offset: 0x00231FC8
		public DummyFireSource(Vector2 maxSize, Vector2 worldPosition, Hull spawningHull = null, bool isNetworkMessage = false) : base(worldPosition, spawningHull, null, isNetworkMessage)
		{
			this.maxSize = maxSize;
			base.DamagesItems = false;
			base.DamagesCharacters = true;
		}

		// Token: 0x17001077 RID: 4215
		// (get) Token: 0x06003ED3 RID: 16083 RVA: 0x00233DEA File Offset: 0x00231FEA
		public override float DamageRange
		{
			get
			{
				return 5f;
			}
		}

		// Token: 0x06003ED4 RID: 16084 RVA: 0x00233DF4 File Offset: 0x00231FF4
		protected override void LimitSize()
		{
			base.LimitSize();
			this.size.X = Math.Min(this.maxSize.X, this.size.X);
			this.size.Y = Math.Min(this.maxSize.Y, this.size.Y);
		}

		// Token: 0x06003ED5 RID: 16085 RVA: 0x00233E53 File Offset: 0x00232053
		protected override void AdjustXPos(float growModifier, float deltaTime)
		{
		}

		// Token: 0x06003ED6 RID: 16086 RVA: 0x00233E55 File Offset: 0x00232055
		protected override void ReduceOxygen(float deltaTime)
		{
		}

		// Token: 0x0400209D RID: 8349
		private Vector2 maxSize;

		// Token: 0x0400209E RID: 8350
		public bool CausedByPsychosis;
	}
}
