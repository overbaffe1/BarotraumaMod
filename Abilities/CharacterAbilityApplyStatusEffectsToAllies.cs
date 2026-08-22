using System;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F2 RID: 1010
	internal class CharacterAbilityApplyStatusEffectsToAllies : CharacterAbilityApplyStatusEffects
	{
		// Token: 0x1700121F RID: 4639
		// (get) Token: 0x0600469D RID: 18077 RVA: 0x0026D1CA File Offset: 0x0026B3CA
		public override bool AllowClientSimulation { get; }

		// Token: 0x0600469E RID: 18078 RVA: 0x0026D1D4 File Offset: 0x0026B3D4
		public CharacterAbilityApplyStatusEffectsToAllies(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.allowSelf = abilityElement.GetAttributeBool("allowself", true);
			this.maxDistance = abilityElement.GetAttributeFloat("maxdistance", float.MaxValue);
			this.inSameRoom = abilityElement.GetAttributeBool("insameroom", false);
			this.jobIdentifiers = abilityElement.GetAttributeIdentifierImmutableHashSet("jobs", ImmutableHashSet<Identifier>.Empty, true);
			this.AllowClientSimulation = abilityElement.GetAttributeBool("allowclientsimulation", true);
		}

		// Token: 0x0600469F RID: 18079 RVA: 0x0026D258 File Offset: 0x0026B458
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

		// Token: 0x060046A0 RID: 18080 RVA: 0x0026D370 File Offset: 0x0026B570
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x04002490 RID: 9360
		private readonly bool allowSelf;

		// Token: 0x04002491 RID: 9361
		private readonly float maxDistance = float.MaxValue;

		// Token: 0x04002492 RID: 9362
		private readonly bool inSameRoom;

		// Token: 0x04002493 RID: 9363
		private readonly ImmutableHashSet<Identifier> jobIdentifiers;
	}
}
