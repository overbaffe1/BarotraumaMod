using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000408 RID: 1032
	internal sealed class CharacterAbilityMarkAsLooted : CharacterAbility
	{
		// Token: 0x060046E7 RID: 18151 RVA: 0x0026E980 File Offset: 0x0026CB80
		public CharacterAbilityMarkAsLooted(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.identifier = abilityElement.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (this.identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", identifier is empty in ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityMarkAsLooted");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060046E8 RID: 18152 RVA: 0x0026EA1C File Offset: 0x0026CC1C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			if (abilityCharacter != null)
			{
				Character character = abilityCharacter.Character;
				if (character != null)
				{
					character.MarkedAsLooted.Add(this.identifier);
					return;
				}
			}
		}

		// Token: 0x040024C9 RID: 9417
		private readonly Identifier identifier;
	}
}
