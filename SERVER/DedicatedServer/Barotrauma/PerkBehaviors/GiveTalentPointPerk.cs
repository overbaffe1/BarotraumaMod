using System;
using System.Collections.Generic;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020002E6 RID: 742
	internal class GiveTalentPointPerk : PerkBase
	{
		// Token: 0x17000E07 RID: 3591
		// (get) Token: 0x06003183 RID: 12675 RVA: 0x0015198C File Offset: 0x0014FB8C
		// (set) Token: 0x06003184 RID: 12676 RVA: 0x00151994 File Offset: 0x0014FB94
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int Amount { get; set; }

		// Token: 0x06003185 RID: 12677 RVA: 0x0015199D File Offset: 0x0014FB9D
		public GiveTalentPointPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x06003186 RID: 12678 RVA: 0x001519A8 File Offset: 0x0014FBA8
		public override void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, Submarine teamSubmarine)
		{
			foreach (Character character in teamCharacters)
			{
				character.Info.AdditionalTalentPoints += this.Amount;
			}
		}
	}
}
