using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200022A RID: 554
	internal class DummyFireSource : FireSource
	{
		// Token: 0x17000ADE RID: 2782
		// (get) Token: 0x060025EA RID: 9706 RVA: 0x000F6409 File Offset: 0x000F4609
		protected override float SpreadToOtherHullsProbability
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x060025EB RID: 9707 RVA: 0x000F6410 File Offset: 0x000F4610
		public DummyFireSource(Vector2 maxSize, Vector2 worldPosition, Hull spawningHull = null, bool isNetworkMessage = false) : base(worldPosition, spawningHull, null, isNetworkMessage)
		{
			this.maxSize = maxSize;
			base.DamagesItems = false;
			base.DamagesCharacters = true;
		}

		// Token: 0x17000ADF RID: 2783
		// (get) Token: 0x060025EC RID: 9708 RVA: 0x000F6432 File Offset: 0x000F4632
		public override float DamageRange
		{
			get
			{
				return 5f;
			}
		}

		// Token: 0x060025ED RID: 9709 RVA: 0x000F643C File Offset: 0x000F463C
		protected override void LimitSize()
		{
			base.LimitSize();
			this.size.X = Math.Min(this.maxSize.X, this.size.X);
			this.size.Y = Math.Min(this.maxSize.Y, this.size.Y);
		}

		// Token: 0x060025EE RID: 9710 RVA: 0x000F649B File Offset: 0x000F469B
		protected override void AdjustXPos(float growModifier, float deltaTime)
		{
		}

		// Token: 0x060025EF RID: 9711 RVA: 0x000F649D File Offset: 0x000F469D
		protected override void ReduceOxygen(float deltaTime)
		{
		}

		// Token: 0x04001271 RID: 4721
		private Vector2 maxSize;

		// Token: 0x04001272 RID: 4722
		public bool CausedByPsychosis;
	}
}
