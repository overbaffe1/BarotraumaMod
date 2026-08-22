using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001CB RID: 459
	internal class BuffDurationIncrease : Affliction
	{
		// Token: 0x0600321A RID: 12826 RVA: 0x0020834F File Offset: 0x0020654F
		public BuffDurationIncrease(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x0600321B RID: 12827 RVA: 0x0020835C File Offset: 0x0020655C
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			base.Update(characterHealth, targetLimb, deltaTime);
			IReadOnlyCollection<Affliction> afflictions = characterHealth.GetAllAfflictions();
			if (this.Strength <= 0f)
			{
				using (IEnumerator<Affliction> enumerator = afflictions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Affliction affliction = enumerator.Current;
						if (affliction.Prefab.IsBuff && affliction != this && affliction.StrengthDiminishMultiplier.Item2 == this)
						{
							affliction.StrengthDiminishMultiplier.Item2 = null;
							affliction.StrengthDiminishMultiplier.Item1 = 1f;
						}
					}
					return;
				}
			}
			foreach (Affliction affliction2 in afflictions)
			{
				if (affliction2.Prefab.IsBuff && affliction2 != this)
				{
					float multiplier = this.GetDiminishMultiplier();
					if (affliction2.StrengthDiminishMultiplier.Item1 >= multiplier || affliction2.StrengthDiminishMultiplier.Item2 == this)
					{
						affliction2.StrengthDiminishMultiplier.Item2 = this;
						affliction2.StrengthDiminishMultiplier.Item1 = multiplier;
					}
				}
			}
		}

		// Token: 0x0600321C RID: 12828 RVA: 0x00208480 File Offset: 0x00206680
		private float GetDiminishMultiplier()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 1f;
			}
			AfflictionPrefab.Effect currentEffect = base.GetActiveEffect();
			if (currentEffect == null)
			{
				return 1f;
			}
			float multiplier = MathHelper.Lerp(currentEffect.MinBuffMultiplier, currentEffect.MaxBuffMultiplier, currentEffect.GetStrengthFactor(this));
			return 1f / Math.Max(multiplier, 0.001f);
		}
	}
}
