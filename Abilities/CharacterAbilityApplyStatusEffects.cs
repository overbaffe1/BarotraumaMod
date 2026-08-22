using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F1 RID: 1009
	internal class CharacterAbilityApplyStatusEffects : CharacterAbility
	{
		// Token: 0x1700121D RID: 4637
		// (get) Token: 0x06004694 RID: 18068 RVA: 0x0026CDF6 File Offset: 0x0026AFF6
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700121E RID: 4638
		// (get) Token: 0x06004695 RID: 18069 RVA: 0x0026CDF9 File Offset: 0x0026AFF9
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004696 RID: 18070 RVA: 0x0026CDFC File Offset: 0x0026AFFC
		public CharacterAbilityApplyStatusEffects(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statusEffects = CharacterAbilityGroup.ParseStatusEffects(base.CharacterTalent, abilityElement.GetChildElement("statuseffects"));
			this.applyToSelf = abilityElement.GetAttributeBool("applytoself", false);
			this.applyToSelected = abilityElement.GetAttributeBool("applytoselected", false);
			this.nearbyCharactersAppliesToSelf = abilityElement.GetAttributeBool("nearbycharactersappliestoself", true);
			this.nearbyCharactersAppliesToAllies = abilityElement.GetAttributeBool("nearbycharactersappliestoallies", true);
			this.nearbyCharactersAppliesToEnemies = abilityElement.GetAttributeBool("nearbycharactersappliestoenemies", true);
			this.setUser = abilityElement.GetAttributeBool("setuser", true);
		}

		// Token: 0x06004697 RID: 18071 RVA: 0x0026CEA4 File Offset: 0x0026B0A4
		protected void ApplyEffectSpecific(Character targetCharacter, Limb targetLimb = null)
		{
			if (this.effectBeingApplied)
			{
				return;
			}
			this.effectBeingApplied = true;
			try
			{
				foreach (StatusEffect statusEffect in this.statusEffects)
				{
					if (statusEffect.HasTargetType(StatusEffect.TargetType.UseTarget))
					{
						if (this.setUser)
						{
							statusEffect.SetUser(targetCharacter);
						}
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, targetCharacter, targetCharacter, null);
					}
					else if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
					{
						this.targets.Clear();
						statusEffect.AddNearbyTargets(targetCharacter.WorldPosition, this.targets);
						if (!this.nearbyCharactersAppliesToSelf)
						{
							this.targets.RemoveAll((ISerializableEntity c) => c == base.Character);
						}
						if (!this.nearbyCharactersAppliesToAllies)
						{
							this.targets.RemoveAll(delegate(ISerializableEntity c)
							{
								Character otherCharacter = c as Character;
								return otherCharacter != null && HumanAIController.IsFriendly(otherCharacter, base.Character, false, false);
							});
						}
						if (!this.nearbyCharactersAppliesToEnemies)
						{
							this.targets.RemoveAll(delegate(ISerializableEntity c)
							{
								Character otherCharacter = c as Character;
								return otherCharacter != null && !HumanAIController.IsFriendly(otherCharacter, base.Character, false, false);
							});
						}
						if (this.setUser)
						{
							statusEffect.SetUser(base.Character);
						}
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, targetCharacter, this.targets, null);
					}
					else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb) && targetLimb != null)
					{
						if (this.setUser)
						{
							statusEffect.SetUser(base.Character);
						}
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, base.Character, targetLimb, null);
					}
					else if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
					{
						if (this.setUser)
						{
							statusEffect.SetUser(base.Character);
						}
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, base.Character, targetCharacter, null);
					}
					else
					{
						if (this.setUser)
						{
							statusEffect.SetUser(base.Character);
						}
						statusEffect.Apply(ActionType.OnAbility, base.EffectDeltaTime, base.Character, base.Character, null);
					}
				}
			}
			finally
			{
				this.effectBeingApplied = false;
			}
		}

		// Token: 0x06004698 RID: 18072 RVA: 0x0026D0E4 File Offset: 0x0026B2E4
		protected override void ApplyEffect()
		{
			if (this.applyToSelected)
			{
				Character selectedCharacter = base.Character.SelectedCharacter;
				if (selectedCharacter != null)
				{
					this.ApplyEffectSpecific(selectedCharacter, null);
					return;
				}
			}
			this.ApplyEffectSpecific(base.Character, null);
		}

		// Token: 0x06004699 RID: 18073 RVA: 0x0026D120 File Offset: 0x0026B320
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character targetCharacter = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (targetCharacter != null && !this.applyToSelf)
			{
				Character targetCharacter2 = targetCharacter;
				AbilityApplyTreatment abilityApplyTreatment = abilityObject as AbilityApplyTreatment;
				this.ApplyEffectSpecific(targetCharacter2, (abilityApplyTreatment != null) ? abilityApplyTreatment.TargetLimb : null);
				return;
			}
			this.ApplyEffect();
		}

		// Token: 0x04002487 RID: 9351
		protected readonly List<StatusEffect> statusEffects;

		// Token: 0x04002488 RID: 9352
		private readonly bool applyToSelf;

		// Token: 0x04002489 RID: 9353
		private readonly bool nearbyCharactersAppliesToSelf;

		// Token: 0x0400248A RID: 9354
		private readonly bool nearbyCharactersAppliesToAllies;

		// Token: 0x0400248B RID: 9355
		private readonly bool nearbyCharactersAppliesToEnemies;

		// Token: 0x0400248C RID: 9356
		private readonly bool applyToSelected;

		// Token: 0x0400248D RID: 9357
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x0400248E RID: 9358
		private bool effectBeingApplied;

		// Token: 0x0400248F RID: 9359
		private readonly bool setUser;
	}
}
