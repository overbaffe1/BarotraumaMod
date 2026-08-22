using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CB RID: 203
	internal class BuffDurationIncrease : Affliction
	{
		// Token: 0x0600165B RID: 5723 RVA: 0x000BD31B File Offset: 0x000BB51B
		public BuffDurationIncrease(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x000BD328 File Offset: 0x000BB528
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

		// Token: 0x0600165D RID: 5725 RVA: 0x000BD44C File Offset: 0x000BB64C
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
