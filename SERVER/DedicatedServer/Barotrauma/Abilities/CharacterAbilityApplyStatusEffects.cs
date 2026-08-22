using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032B RID: 811
	internal class CharacterAbilityApplyStatusEffects : CharacterAbility
	{
		// Token: 0x17000E2D RID: 3629
		// (get) Token: 0x0600325A RID: 12890 RVA: 0x00154F76 File Offset: 0x00153176
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000E2E RID: 3630
		// (get) Token: 0x0600325B RID: 12891 RVA: 0x00154F79 File Offset: 0x00153179
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600325C RID: 12892 RVA: 0x00154F7C File Offset: 0x0015317C
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

		// Token: 0x0600325D RID: 12893 RVA: 0x00155024 File Offset: 0x00153224
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

		// Token: 0x0600325E RID: 12894 RVA: 0x00155264 File Offset: 0x00153464
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

		// Token: 0x0600325F RID: 12895 RVA: 0x001552A0 File Offset: 0x001534A0
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

		// Token: 0x040018C6 RID: 6342
		protected readonly List<StatusEffect> statusEffects;

		// Token: 0x040018C7 RID: 6343
		private readonly bool applyToSelf;

		// Token: 0x040018C8 RID: 6344
		private readonly bool nearbyCharactersAppliesToSelf;

		// Token: 0x040018C9 RID: 6345
		private readonly bool nearbyCharactersAppliesToAllies;

		// Token: 0x040018CA RID: 6346
		private readonly bool nearbyCharactersAppliesToEnemies;

		// Token: 0x040018CB RID: 6347
		private readonly bool applyToSelected;

		// Token: 0x040018CC RID: 6348
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x040018CD RID: 6349
		private bool effectBeingApplied;

		// Token: 0x040018CE RID: 6350
		private readonly bool setUser;
	}
}
