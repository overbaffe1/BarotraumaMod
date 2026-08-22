using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003D1 RID: 977
	internal class AbilityConditionHasPermanentStat : AbilityConditionCharacter
	{
		// Token: 0x0600463F RID: 17983 RVA: 0x0026BE38 File Offset: 0x0026A038
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

		// Token: 0x06004640 RID: 17984 RVA: 0x0026BF24 File Offset: 0x0026A124
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

		// Token: 0x0400246C RID: 9324
		private readonly Identifier statIdentifier;

		// Token: 0x0400246D RID: 9325
		private readonly StatTypes statType;

		// Token: 0x0400246E RID: 9326
		private readonly float min;

		// Token: 0x0400246F RID: 9327
		private readonly PermanentStatPlaceholder placeholder;
	}
}
