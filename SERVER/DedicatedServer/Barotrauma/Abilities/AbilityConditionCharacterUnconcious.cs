using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F2 RID: 754
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionCharacterUnconcious : AbilityConditionCharacter
	{
		// Token: 0x060031C8 RID: 12744 RVA: 0x00152E52 File Offset: 0x00151052
		public AbilityConditionCharacterUnconcious(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031C9 RID: 12745 RVA: 0x00152E5C File Offset: 0x0015105C
		protected override bool MatchesCharacter(Character character)
		{
			return character != null && character.IsUnconscious;
		}
	}
}
