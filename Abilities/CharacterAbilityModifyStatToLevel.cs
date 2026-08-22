using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040F RID: 1039
	internal class CharacterAbilityModifyStatToLevel : CharacterAbility
	{
		// Token: 0x1700122A RID: 4650
		// (get) Token: 0x060046FA RID: 18170 RVA: 0x0026EFE4 File Offset: 0x0026D1E4
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046FB RID: 18171 RVA: 0x0026EFE8 File Offset: 0x0026D1E8
		public CharacterAbilityModifyStatToLevel(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.statPerLevel = abilityElement.GetAttributeFloat("statperlevel", 0f);
			this.maxLevel = abilityElement.GetAttributeInt("maxlevel", int.MaxValue);
		}

		// Token: 0x060046FC RID: 18172 RVA: 0x0026F050 File Offset: 0x0026D250
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

		// Token: 0x040024DC RID: 9436
		private readonly StatTypes statType;

		// Token: 0x040024DD RID: 9437
		private readonly float statPerLevel;

		// Token: 0x040024DE RID: 9438
		private readonly int maxLevel;

		// Token: 0x040024DF RID: 9439
		private float lastValue;
	}
}
