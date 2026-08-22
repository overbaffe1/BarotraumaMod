using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005FC RID: 1532
	internal class Vent : ItemComponent
	{
		// Token: 0x17001943 RID: 6467
		// (get) Token: 0x060063CB RID: 25547 RVA: 0x0033EA32 File Offset: 0x0033CC32
		// (set) Token: 0x060063CC RID: 25548 RVA: 0x0033EA3A File Offset: 0x0033CC3A
		public float OxygenFlow
		{
			get
			{
				return this.oxygenFlow;
			}
			set
			{
				this.oxygenFlow = Math.Max(value, 0f);
			}
		}

		// Token: 0x060063CD RID: 25549 RVA: 0x0033EA4D File Offset: 0x0033CC4D
		public Vent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060063CE RID: 25550 RVA: 0x0033EA58 File Offset: 0x0033CC58
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.CurrentHull == null || this.item.InWater)
			{
				return;
			}
			if (this.oxygenFlow > 0f)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
			this.item.CurrentHull.Oxygen += this.oxygenFlow * deltaTime;
			this.OxygenFlow -= deltaTime * 1000f;
		}

		// Token: 0x040033BC RID: 13244
		private float oxygenFlow;
	}
}
