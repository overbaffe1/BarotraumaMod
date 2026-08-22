using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B8 RID: 952
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class AbilityConditionCharacterUnconcious : AbilityConditionCharacter
	{
		// Token: 0x06004602 RID: 17922 RVA: 0x0026ACD2 File Offset: 0x00268ED2
		public AbilityConditionCharacterUnconcious(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004603 RID: 17923 RVA: 0x0026ACDC File Offset: 0x00268EDC
		protected override bool MatchesCharacter(Character character)
		{
			return character != null && character.IsUnconscious;
		}
	}
}
