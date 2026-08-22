using System;

namespace Barotrauma
{
	// Token: 0x020001C5 RID: 453
	internal class AfflictionBleeding : Affliction
	{
		// Token: 0x060031D2 RID: 12754 RVA: 0x00205AF8 File Offset: 0x00203CF8
		public AfflictionBleeding(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x060031D3 RID: 12755 RVA: 0x00205B04 File Offset: 0x00203D04
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
