using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200030B RID: 779
	internal class AbilityConditionHasPermanentStat : AbilityConditionCharacter
	{
		// Token: 0x06003205 RID: 12805 RVA: 0x00153FB8 File Offset: 0x001521B8
		public AbilityConditionHasPermanentStat(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.statIdentifier = conditionElement.GetAttributeIdentifier("statidentifier", Identifier.Empty);
			if (this.statIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
				defaultInterpolatedStringHandler.AppendLiteral("No stat identifier defined for ");
				defaultInterpolatedStringHandler.AppendFormatted<AbilityConditionHasPermanentStat>(this);
				defaultInterpolatedStringHandler.AppendLiteral(" in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(characterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, conditionElement.ContentPackage, false, false);
			}
			string statTypeName = conditionElement.GetAttributeString("stattype", string.Empty);
			this.statType = (string.IsNullOrEmpty(statTypeName) ? StatTypes.None : CharacterAbilityGroup.ParseStatType(statTypeName, characterTalent.DebugIdentifier));
			this.min = conditionElement.GetAttributeFloat("min", 0f);
			string key = "placeholder";
			PermanentStatPlaceholder permanentStatPlaceholder = PermanentStatPlaceholder.None;
			this.placeholder = conditionElement.GetAttributeEnum<PermanentStatPlaceholder>(key, permanentStatPlaceholder);
		}

		// Token: 0x06003206 RID: 12806 RVA: 0x001540A4 File Offset: 0x001522A4
		protected override bool MatchesCharacter(Character character)
		{
			if (((character != null) ? character.Info : null) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(107, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("MatchesCharacter");
				defaultInterpolatedStringHandler.AppendLiteral(": character ");
				defaultInterpolatedStringHandler.AppendFormatted<Character>(character);
				defaultInterpolatedStringHandler.AppendLiteral(" has no CharacterInfo. Are you trying to use the condition on a non-player character?\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return false;
			}
			Identifier identifier = CharacterAbilityGivePermanentStat.HandlePlaceholders(this.placeholder, this.statIdentifier);
			return character.Info.GetSavedStatValue(this.statType, identifier) >= this.min;
		}

		// Token: 0x040018AB RID: 6315
		private readonly Identifier statIdentifier;

		// Token: 0x040018AC RID: 6316
		private readonly StatTypes statType;

		// Token: 0x040018AD RID: 6317
		private readonly float min;

		// Token: 0x040018AE RID: 6318
		private readonly PermanentStatPlaceholder placeholder;
	}
}
