using System;

namespace Barotrauma
{
	// Token: 0x020000C4 RID: 196
	internal class AfflictionBleeding : Affliction
	{
		// Token: 0x06001612 RID: 5650 RVA: 0x000BACE8 File Offset: 0x000B8EE8
		public AfflictionBleeding(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x000BACF4 File Offset: 0x000B8EF4
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			base.Update(characterHealth, targetLimb, deltaTime);
			float bloodlossResistance = characterHealth.GetResistance(characterHealth.BloodlossAffliction.Prefab, (targetLimb != null) ? targetLimb.type : LimbType.None);
			characterHealth.BloodlossAmount += this.Strength * (1f - bloodlossResistance) / 60f * deltaTime;
			if (this.Source != null)
			{
				characterHealth.BloodlossAffliction.Source = this.Source;
			}
		}
	}
}
