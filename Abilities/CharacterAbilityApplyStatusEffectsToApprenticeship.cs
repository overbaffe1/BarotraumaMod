using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F3 RID: 1011
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityApplyStatusEffectsToApprenticeship : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x060046A1 RID: 18081 RVA: 0x0026D378 File Offset: 0x0026B578
		public CharacterAbilityApplyStatusEffectsToApprenticeship(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.invert = abilityElement.GetAttributeBool("invert", false);
		}

		// Token: 0x060046A2 RID: 18082 RVA: 0x0026D3A4 File Offset: 0x0026B5A4
		protected override void ApplyEffect()
		{
			base.ApplyEffectSpecific(base.Character, null);
			JobPrefab apprenticeJob = CharacterAbilityApplyStatusEffectsToApprenticeship.GetApprenticeJob(base.Character, this.jobPrefabList);
			if (apprenticeJob == null)
			{
				DebugConsole.ThrowError("CharacterAbilityUnlockApprenticeshipTalentTree: Could not find apprentice job for character " + base.Character.Name, null, base.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			foreach (Character character in Character.GetFriendlyCrew(base.Character))
			{
				Job job = character.Info.Job;
				JobPrefab characterJob = (job != null) ? job.Prefab : null;
				if (characterJob != null)
				{
					bool flag = characterJob.Identifier == apprenticeJob.Identifier;
					if (flag)
					{
						if (this.invert)
						{
							continue;
						}
					}
					else if (!this.invert)
					{
						continue;
					}
					base.ApplyEffectSpecific(character, null);
				}
			}
		}

		// Token: 0x060046A3 RID: 18083 RVA: 0x0026D48C File Offset: 0x0026B68C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x060046A4 RID: 18084 RVA: 0x0026D494 File Offset: 0x0026B694
		[return: Nullable(2)]
		public static JobPrefab GetApprenticeJob(Character character, IReadOnlyCollection<JobPrefab> jobList)
		{
			foreach (JobPrefab prefab in jobList)
			{
				if (character.Info.GetSavedStatValue(StatTypes.Apprenticeship, prefab.Identifier) > 0f)
				{
					return prefab;
				}
			}
			return null;
		}

		// Token: 0x04002495 RID: 9365
		private readonly bool invert;

		// Token: 0x04002496 RID: 9366
		private readonly ImmutableHashSet<JobPrefab> jobPrefabList = JobPrefab.Prefabs.ToImmutableHashSet<JobPrefab>();
	}
}
