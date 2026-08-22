using System;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032C RID: 812
	internal class CharacterAbilityApplyStatusEffectsToAllies : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x17000E2F RID: 3631
		// (get) Token: 0x06003263 RID: 12899 RVA: 0x0015534A File Offset: 0x0015354A
		public override bool AllowClientSimulation { get; }

		// Token: 0x06003264 RID: 12900 RVA: 0x00155354 File Offset: 0x00153554
		public CharacterAbilityApplyStatusEffectsToAllies(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.allowSelf = abilityElement.GetAttributeBool("allowself", true);
			this.maxDistance = abilityElement.GetAttributeFloat("maxdistance", float.MaxValue);
			this.inSameRoom = abilityElement.GetAttributeBool("insameroom", false);
			this.jobIdentifiers = abilityElement.GetAttributeIdentifierImmutableHashSet("jobs", ImmutableHashSet<Identifier>.Empty, true);
			this.AllowClientSimulation = abilityElement.GetAttributeBool("allowclientsimulation", true);
		}

		// Token: 0x06003265 RID: 12901 RVA: 0x001553D8 File Offset: 0x001535D8
		protected override void ApplyEffect()
		{
			foreach (Character character in Character.GetFriendlyCrew(base.Character))
			{
				if (this.allowSelf || character != base.Character)
				{
					if (!this.jobIdentifiers.IsEmpty)
					{
						bool hadJob = false;
						foreach (Identifier job in this.jobIdentifiers)
						{
							if (character.HasJob(job.Value))
							{
								hadJob = true;
								break;
							}
						}
						if (!hadJob)
						{
							continue;
						}
					}
					if ((!this.inSameRoom || character.IsInSameRoomAs(base.Character)) && (this.maxDistance >= 3.4028235E+38f || Vector2.DistanceSquared(character.WorldPosition, base.Character.WorldPosition) <= this.maxDistance * this.maxDistance))
					{
						base.ApplyEffectSpecific(character, null);
					}
				}
			}
		}

		// Token: 0x06003266 RID: 12902 RVA: 0x001554F0 File Offset: 0x001536F0
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x040018CF RID: 6351
		private readonly bool allowSelf;

		// Token: 0x040018D0 RID: 6352
		private readonly float maxDistance = float.MaxValue;

		// Token: 0x040018D1 RID: 6353
		private readonly bool inSameRoom;

		// Token: 0x040018D2 RID: 6354
		private readonly ImmutableHashSet<Identifier> jobIdentifiers;
	}
}
