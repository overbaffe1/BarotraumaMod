using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032D RID: 813
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityApplyStatusEffectsToApprenticeship : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x06003267 RID: 12903 RVA: 0x001554F8 File Offset: 0x001536F8
		public CharacterAbilityApplyStatusEffectsToApprenticeship(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.invert = abilityElement.GetAttributeBool("invert", false);
		}

		// Token: 0x06003268 RID: 12904 RVA: 0x00155524 File Offset: 0x00153724
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

		// Token: 0x06003269 RID: 12905 RVA: 0x0015560C File Offset: 0x0015380C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x0600326A RID: 12906 RVA: 0x00155614 File Offset: 0x00153814
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

		// Token: 0x040018D4 RID: 6356
		private readonly bool invert;

		// Token: 0x040018D5 RID: 6357
		private readonly ImmutableHashSet<JobPrefab> jobPrefabList = JobPrefab.Prefabs.ToImmutableHashSet<JobPrefab>();
	}
}
