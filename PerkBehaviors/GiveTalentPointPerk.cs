using System;
using System.Collections.Generic;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020003AC RID: 940
	internal class GiveTalentPointPerk : PerkBase
	{
		// Token: 0x170011F7 RID: 4599
		// (get) Token: 0x060045BD RID: 17853 RVA: 0x0026981E File Offset: 0x00267A1E
		// (set) Token: 0x060045BE RID: 17854 RVA: 0x00269826 File Offset: 0x00267A26
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int Amount { get; set; }

		// Token: 0x060045BF RID: 17855 RVA: 0x0026982F File Offset: 0x00267A2F
		public GiveTalentPointPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x060045C0 RID: 17856 RVA: 0x0026983C File Offset: 0x00267A3C
		public override void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, Submarine teamSubmarine)
		{
			foreach (Character character in teamCharacters)
			{
				character.Info.AdditionalTalentPoints += this.Amount;
			}
		}
	}
}
