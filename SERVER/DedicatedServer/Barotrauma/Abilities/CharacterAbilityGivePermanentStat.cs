using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200033B RID: 827
	internal class CharacterAbilityGivePermanentStat : CharacterAbility
	{
		// Token: 0x17000E33 RID: 3635
		// (get) Token: 0x06003295 RID: 12949 RVA: 0x00156241 File Offset: 0x00154441
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000E34 RID: 3636
		// (get) Token: 0x06003296 RID: 12950 RVA: 0x00156244 File Offset: 0x00154444
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06003297 RID: 12951 RVA: 0x00156248 File Offset: 0x00154448
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

		// Token: 0x06003298 RID: 12952 RVA: 0x0015637E File Offset: 0x0015457E
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (this.giveOnAddingFirstTime && addingFirstTime)
			{
				this.ApplyEffectSpecific(null);
			}
		}

		// Token: 0x06003299 RID: 12953 RVA: 0x00156391 File Offset: 0x00154591
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific(abilityObject);
		}

		// Token: 0x0600329A RID: 12954 RVA: 0x0015639A File Offset: 0x0015459A
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(null);
		}

		// Token: 0x0600329B RID: 12955 RVA: 0x001563A4 File Offset: 0x001545A4
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

		// Token: 0x0600329C RID: 12956 RVA: 0x00156520 File Offset: 0x00154720
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

		// Token: 0x040018F4 RID: 6388
		private readonly Identifier statIdentifier;

		// Token: 0x040018F5 RID: 6389
		private readonly StatTypes statType;

		// Token: 0x040018F6 RID: 6390
		private readonly float value;

		// Token: 0x040018F7 RID: 6391
		private readonly float maxValue;

		// Token: 0x040018F8 RID: 6392
		private readonly bool targetAllies;

		// Token: 0x040018F9 RID: 6393
		private readonly bool removeOnDeath;

		// Token: 0x040018FA RID: 6394
		private readonly bool giveOnAddingFirstTime;

		// Token: 0x040018FB RID: 6395
		private readonly bool setValue;

		// Token: 0x040018FC RID: 6396
		private readonly PermanentStatPlaceholder placeholder;

		// Token: 0x040018FD RID: 6397
		private readonly bool targetAbilityTarget;
	}
}
