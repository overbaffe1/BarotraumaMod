using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001F5 RID: 501
	internal class AbilityApplyTreatment : AbilityObject, IAbilityCharacter, IAbilityItem
	{
		// Token: 0x17000A48 RID: 2632
		// (get) Token: 0x060023BB RID: 9147 RVA: 0x000EEBEC File Offset: 0x000ECDEC
		// (set) Token: 0x060023BC RID: 9148 RVA: 0x000EEBF4 File Offset: 0x000ECDF4
		public Character Character { get; set; }

		// Token: 0x17000A49 RID: 2633
		// (get) Token: 0x060023BD RID: 9149 RVA: 0x000EEBFD File Offset: 0x000ECDFD
		// (set) Token: 0x060023BE RID: 9150 RVA: 0x000EEC05 File Offset: 0x000ECE05
		public Character User { get; set; }

		// Token: 0x17000A4A RID: 2634
		// (get) Token: 0x060023BF RID: 9151 RVA: 0x000EEC0E File Offset: 0x000ECE0E
		// (set) Token: 0x060023C0 RID: 9152 RVA: 0x000EEC16 File Offset: 0x000ECE16
		public Item Item { get; set; }

		// Token: 0x17000A4B RID: 2635
		// (get) Token: 0x060023C1 RID: 9153 RVA: 0x000EEC1F File Offset: 0x000ECE1F
		// (set) Token: 0x060023C2 RID: 9154 RVA: 0x000EEC27 File Offset: 0x000ECE27
		public Limb TargetLimb { get; set; }

		// Token: 0x060023C3 RID: 9155 RVA: 0x000EEC30 File Offset: 0x000ECE30
		public AbilityApplyTreatment(Character user, Character target, Item item, Limb limb)
		{
			this.Character = target;
			this.User = user;
			this.Item = item;
			this.TargetLimb = limb;
		}
	}
}
