using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020002DF RID: 735
	internal class AbilityApplyTreatment : AbilityObject, IAbilityCharacter, IAbilityItem
	{
		// Token: 0x1700103C RID: 4156
		// (get) Token: 0x06003D61 RID: 15713 RVA: 0x0022E350 File Offset: 0x0022C550
		// (set) Token: 0x06003D62 RID: 15714 RVA: 0x0022E358 File Offset: 0x0022C558
		public Character Character { get; set; }

		// Token: 0x1700103D RID: 4157
		// (get) Token: 0x06003D63 RID: 15715 RVA: 0x0022E361 File Offset: 0x0022C561
		// (set) Token: 0x06003D64 RID: 15716 RVA: 0x0022E369 File Offset: 0x0022C569
		public Character User { get; set; }

		// Token: 0x1700103E RID: 4158
		// (get) Token: 0x06003D65 RID: 15717 RVA: 0x0022E372 File Offset: 0x0022C572
		// (set) Token: 0x06003D66 RID: 15718 RVA: 0x0022E37A File Offset: 0x0022C57A
		public Item Item { get; set; }

		// Token: 0x1700103F RID: 4159
		// (get) Token: 0x06003D67 RID: 15719 RVA: 0x0022E383 File Offset: 0x0022C583
		// (set) Token: 0x06003D68 RID: 15720 RVA: 0x0022E38B File Offset: 0x0022C58B
		public Limb TargetLimb { get; set; }

		// Token: 0x06003D69 RID: 15721 RVA: 0x0022E394 File Offset: 0x0022C594
		public AbilityApplyTreatment(Character user, Character target, Item item, Limb limb)
		{
			this.Character = target;
			this.User = user;
			this.Item = item;
			this.TargetLimb = limb;
		}
	}
}
