using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000401 RID: 1025
	internal class CharacterAbilityGivePermanentStat : CharacterAbility
	{
		// Token: 0x17001223 RID: 4643
		// (get) Token: 0x060046CF RID: 18127 RVA: 0x0026E0C1 File Offset: 0x0026C2C1
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17001224 RID: 4644
		// (get) Token: 0x060046D0 RID: 18128 RVA: 0x0026E0C4 File Offset: 0x0026C2C4
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046D1 RID: 18129 RVA: 0x0026E0C8 File Offset: 0x0026C2C8
		public CharacterAbilityGivePermanentStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statIdentifier = abilityElement.GetAttributeIdentifier("statidentifier", Identifier.Empty);
			if (this.statIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent \"" + base.CharacterTalent.DebugIdentifier + "\" - stat identifier not defined.", null, abilityElement.ContentPackage, false, false);
			}
			string statTypeName = abilityElement.GetAttributeString("stattype", string.Empty);
			this.statType = (string.IsNullOrEmpty(statTypeName) ? StatTypes.None : CharacterAbilityGroup.ParseStatType(statTypeName, base.CharacterTalent.DebugIdentifier));
			this.value = abilityElement.GetAttributeFloat("value", 0f);
			this.maxValue = abilityElement.GetAttributeFloat("maxvalue", float.MaxValue);
			this.targetAllies = abilityElement.GetAttributeBool("targetallies", false);
			this.removeOnDeath = abilityElement.GetAttributeBool("removeondeath", false);
			this.giveOnAddingFirstTime = abilityElement.GetAttributeBool("giveonaddingfirsttime", characterAbilityGroup.AbilityEffectType == AbilityEffectType.None);
			this.setValue = abilityElement.GetAttributeBool("setvalue", false);
			string key = "placeholder";
			PermanentStatPlaceholder permanentStatPlaceholder = PermanentStatPlaceholder.None;
			this.placeholder = abilityElement.GetAttributeEnum<PermanentStatPlaceholder>(key, permanentStatPlaceholder);
			this.targetAbilityTarget = abilityElement.GetAttributeBool("targetAbilityTarget", false);
		}

		// Token: 0x060046D2 RID: 18130 RVA: 0x0026E1FE File Offset: 0x0026C3FE
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (this.giveOnAddingFirstTime && addingFirstTime)
			{
				this.ApplyEffectSpecific(null);
			}
		}

		// Token: 0x060046D3 RID: 18131 RVA: 0x0026E211 File Offset: 0x0026C411
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific(abilityObject);
		}

		// Token: 0x060046D4 RID: 18132 RVA: 0x0026E21A File Offset: 0x0026C41A
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(null);
		}

		// Token: 0x060046D5 RID: 18133 RVA: 0x0026E224 File Offset: 0x0026C424
		private void ApplyEffectSpecific(AbilityObject abilityObject)
		{
			Identifier identifier = CharacterAbilityGivePermanentStat.HandlePlaceholders(this.placeholder, this.statIdentifier);
			if (this.targetAllies)
			{
				using (IEnumerator<Character> enumerator = Character.GetFriendlyCrew(base.Character).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character c = enumerator.Current;
						c.Info.ChangeSavedStatValue(this.statType, this.value, identifier, this.removeOnDeath, this.maxValue, this.setValue);
					}
					return;
				}
			}
			Character character;
			if (!this.targetAbilityTarget)
			{
				character = base.Character;
			}
			else
			{
				IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
				character = (((abilityCharacter != null) ? abilityCharacter.Character : null) ?? base.Character);
			}
			Character targetCharacter = character;
			if (targetCharacter == null)
			{
				DebugConsole.ThrowError("Error in ApplyEffectSpecific: character was null.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (((targetCharacter != null) ? targetCharacter.Info : null) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(107, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("ApplyEffectSpecific");
				defaultInterpolatedStringHandler.AppendLiteral(": character ");
				defaultInterpolatedStringHandler.AppendFormatted<Character>(targetCharacter);
				defaultInterpolatedStringHandler.AppendLiteral(" has no CharacterInfo. Are you trying to use the condition on a non-player character?\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			targetCharacter.Info.ChangeSavedStatValue(this.statType, this.value, identifier, this.removeOnDeath, this.maxValue, this.setValue);
		}

		// Token: 0x060046D6 RID: 18134 RVA: 0x0026E3A0 File Offset: 0x0026C5A0
		public static Identifier HandlePlaceholders(PermanentStatPlaceholder placeholder, Identifier original)
		{
			GameSession gameSession = GameMain.GameSession;
			Map map2;
			if (gameSession == null)
			{
				map2 = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				map2 = ((campaign != null) ? campaign.Map : null);
			}
			Map map = map2;
			if (map == null)
			{
				return original;
			}
			if (placeholder != PermanentStatPlaceholder.LocationName)
			{
				if (placeholder == PermanentStatPlaceholder.LocationIndex)
				{
					return original.Replace("[placeholder]", map.CurrentLocationIndex.ToString());
				}
			}
			else
			{
				Location location = map.CurrentLocation;
				if (location != null)
				{
					return original.Replace("[placeholder]", location.NameIdentifier.Value);
				}
			}
			return original;
		}

		// Token: 0x040024B5 RID: 9397
		private readonly Identifier statIdentifier;

		// Token: 0x040024B6 RID: 9398
		private readonly StatTypes statType;

		// Token: 0x040024B7 RID: 9399
		private readonly float value;

		// Token: 0x040024B8 RID: 9400
		private readonly float maxValue;

		// Token: 0x040024B9 RID: 9401
		private readonly bool targetAllies;

		// Token: 0x040024BA RID: 9402
		private readonly bool removeOnDeath;

		// Token: 0x040024BB RID: 9403
		private readonly bool giveOnAddingFirstTime;

		// Token: 0x040024BC RID: 9404
		private readonly bool setValue;

		// Token: 0x040024BD RID: 9405
		private readonly PermanentStatPlaceholder placeholder;

		// Token: 0x040024BE RID: 9406
		private readonly bool targetAbilityTarget;
	}
}
