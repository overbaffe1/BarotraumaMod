using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CA RID: 202
	internal class AfflictionSpaceHerpes : Affliction
	{
		// Token: 0x06001659 RID: 5721 RVA: 0x000BD115 File Offset: 0x000BB315
		public AfflictionSpaceHerpes(AfflictionPrefab prefab, float strength) : base(prefab, strength)
		{
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x000BD138 File Offset: 0x000BB338
		public override void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			base.Update(characterHealth, targetLimb, deltaTime);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			this.invertControlsCooldown -= deltaTime;
			if (this.invertControlsCooldown <= 0f)
			{
				this.invertControlsCooldown = (180f - this.Strength) * Rand.Range(0.7f, 1.3f, Rand.RandSync.Unsynced);
				this.invertControlsTimer = MathHelper.Lerp(10f, 60f, this.Strength / 100f) * Rand.Range(0.7f, 1.3f, Rand.RandSync.Unsynced);
			}
			else if (this.invertControlsTimer > 0f)
			{
				this.invertControlsToggleTimer -= deltaTime;
				if (this.invertControlsToggleTimer <= 0f)
				{
					this.invertControlsToggleTimer = 5f;
					if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.5f)
					{
						characterHealth.ReduceAfflictionOnAllLimbs("invertcontrols".ToIdentifier(), 100f, null, null);
					}
					else
					{
						AfflictionPrefab invertControlsAffliction = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == "invertcontrols");
						characterHealth.ApplyAffliction(null, new Affliction(invertControlsAffliction, 5f), true, false, true);
					}
				}
				this.invertControlsTimer -= deltaTime;
			}
			if (this.Strength > 50f)
			{
				this.stunCoolDown -= deltaTime;
				if (this.stunCoolDown <= 0f)
				{
					this.stunCoolDown = (180f - this.Strength) * Rand.Range(0.7f, 1.3f, Rand.RandSync.Unsynced);
					float stunDuration = MathHelper.Lerp(3f, 10f, this.Strength / 100f) * Rand.Range(0.7f, 1.3f, Rand.RandSync.Unsynced);
					characterHealth.Character.SetStun(stunDuration, false, false);
				}
			}
		}

		// Token: 0x04000AE0 RID: 2784
		private float invertControlsCooldown = 60f;

		// Token: 0x04000AE1 RID: 2785
		private float stunCoolDown = 60f;

		// Token: 0x04000AE2 RID: 2786
		private float invertControlsTimer;

		// Token: 0x04000AE3 RID: 2787
		private float invertControlsToggleTimer;
	}
}
