using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D2 RID: 1234
	internal class Vent : ItemComponent
	{
		// Token: 0x170012DF RID: 4831
		// (get) Token: 0x06004654 RID: 18004 RVA: 0x001C159E File Offset: 0x001BF79E
		// (set) Token: 0x06004655 RID: 18005 RVA: 0x001C15A6 File Offset: 0x001BF7A6
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

		// Token: 0x06004656 RID: 18006 RVA: 0x001C15B9 File Offset: 0x001BF7B9
		public Vent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004657 RID: 18007 RVA: 0x001C15C4 File Offset: 0x001BF7C4
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

		// Token: 0x040021D4 RID: 8660
		private float oxygenFlow;
	}
}
