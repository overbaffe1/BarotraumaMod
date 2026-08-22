using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000342 RID: 834
	internal sealed class CharacterAbilityMarkAsLooted : CharacterAbility
	{
		// Token: 0x060032AD RID: 12973 RVA: 0x00156B00 File Offset: 0x00154D00
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

		// Token: 0x060032AE RID: 12974 RVA: 0x00156B9C File Offset: 0x00154D9C
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

		// Token: 0x04001908 RID: 6408
		private readonly Identifier identifier;
	}
}
