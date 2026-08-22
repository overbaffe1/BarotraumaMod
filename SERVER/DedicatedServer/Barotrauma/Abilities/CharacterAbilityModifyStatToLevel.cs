using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x02000349 RID: 841
	internal class CharacterAbilityModifyStatToLevel : CharacterAbility
	{
		// Token: 0x17000E3A RID: 3642
		// (get) Token: 0x060032C0 RID: 12992 RVA: 0x00157164 File Offset: 0x00155364
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032C1 RID: 12993 RVA: 0x00157168 File Offset: 0x00155368
		public CharacterAbilityModifyStatToLevel(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.statPerLevel = abilityElement.GetAttributeFloat("statperlevel", 0f);
			this.maxLevel = abilityElement.GetAttributeInt("maxlevel", int.MaxValue);
		}

		// Token: 0x060032C2 RID: 12994 RVA: 0x001571D0 File Offset: 0x001553D0
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			base.Character.ChangeStat(this.statType, -this.lastValue);
			if (conditionsMatched)
			{
				Character character = base.Character;
				int level = MathHelper.Min((character != null) ? character.Info.GetCurrentLevel() : 0, this.maxLevel);
				this.lastValue = this.statPerLevel * (float)level;
				base.Character.ChangeStat(this.statType, this.lastValue);
				return;
			}
			this.lastValue = 0f;
		}

		// Token: 0x0400191B RID: 6427
		private readonly StatTypes statType;

		// Token: 0x0400191C RID: 6428
		private readonly float statPerLevel;

		// Token: 0x0400191D RID: 6429
		private readonly int maxLevel;

		// Token: 0x0400191E RID: 6430
		private float lastValue;
	}
}
