using System;

namespace Barotrauma
{
	// Token: 0x020000C9 RID: 201
	internal class AfflictionPsychosis : Affliction
	{
		// Token: 0x06001657 RID: 5719 RVA: 0x000BD100 File Offset: 0x000BB300
		public AfflictionPsychosis(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x000BD10A File Offset: 0x000BB30A
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			base.Update(characterHealth, targetLimb, deltaTime);
		}
	}
}
