using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001DF RID: 479
	internal abstract class FishGroundedParams : GroundedMovementParams, IFishAnimation
	{
		// Token: 0x060032FD RID: 13053 RVA: 0x0020AFD0 File Offset: 0x002091D0
		protected static bool Check(Character character)
		{
			if (!character.AnimController.CanWalk)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral(" cannot use run animations!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, character.Prefab.ContentPackage, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x17000D72 RID: 3442
		// (get) Token: 0x060032FE RID: 13054 RVA: 0x0020B02A File Offset: 0x0020922A
		// (set) Token: 0x060032FF RID: 13055 RVA: 0x0020B032 File Offset: 0x00209232
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character be flipped depending on which direction it faces. Should usually be enabled on all characters that have distinctive upper and lower sides.", "", false)]
		public bool Flip { get; set; }

		// Token: 0x17000D73 RID: 3443
		// (get) Token: 0x06003300 RID: 13056 RVA: 0x0020B03B File Offset: 0x0020923B
		// (set) Token: 0x06003301 RID: 13057 RVA: 0x0020B043 File Offset: 0x00209243
		[Serialize(1f, IsPropertySaveable.Yes, "Reduces continuous flipping when the character abruptly changes direction.", "", false)]
		[Editable]
		public float FlipCooldown { get; set; }

		// Token: 0x17000D74 RID: 3444
		// (get) Token: 0x06003302 RID: 13058 RVA: 0x0020B04C File Offset: 0x0020924C
		// (set) Token: 0x06003303 RID: 13059 RVA: 0x0020B054 File Offset: 0x00209254
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much it takes before the character flips. The timer starts when the character starts to move in the different direction.", "", false)]
		[Editable]
		public float FlipDelay { get; set; }

		// Token: 0x17000D75 RID: 3445
		// (get) Token: 0x06003304 RID: 13060 RVA: 0x0020B05D File Offset: 0x0020925D
		// (set) Token: 0x06003305 RID: 13061 RVA: 0x0020B065 File Offset: 0x00209265
		[Serialize(10f, IsPropertySaveable.Yes, "How much force is used to move the head to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float HeadMoveForce { get; set; }

		// Token: 0x17000D76 RID: 3446
		// (get) Token: 0x06003306 RID: 13062 RVA: 0x0020B06E File Offset: 0x0020926E
		// (set) Token: 0x06003307 RID: 13063 RVA: 0x0020B076 File Offset: 0x00209276
		[Serialize(10f, IsPropertySaveable.Yes, "How much force is used to move the torso to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float TorsoMoveForce { get; set; }

		// Token: 0x17000D77 RID: 3447
		// (get) Token: 0x06003308 RID: 13064 RVA: 0x0020B07F File Offset: 0x0020927F
		// (set) Token: 0x06003309 RID: 13065 RVA: 0x0020B087 File Offset: 0x00209287
		[Serialize(8f, IsPropertySaveable.Yes, "How much force is used to move the feet to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FootMoveForce { get; set; }

		// Token: 0x17000D78 RID: 3448
		// (get) Token: 0x0600330A RID: 13066 RVA: 0x0020B090 File Offset: 0x00209290
		// (set) Token: 0x0600330B RID: 13067 RVA: 0x0020B098 File Offset: 0x00209298
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the tail to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float TailTorque { get; set; }

		// Token: 0x17000D79 RID: 3449
		// (get) Token: 0x0600330C RID: 13068 RVA: 0x0020B0A1 File Offset: 0x002092A1
		// (set) Token: 0x0600330D RID: 13069 RVA: 0x0020B0A9 File Offset: 0x002092A9
		[Serialize(0f, IsPropertySaveable.Yes, "Optional torque that's constantly applied to legs.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float LegTorque { get; set; }

		// Token: 0x17000D7A RID: 3450
		// (get) Token: 0x0600330E RID: 13070 RVA: 0x0020B0B2 File Offset: 0x002092B2
		// (set) Token: 0x0600330F RID: 13071 RVA: 0x0020B0BF File Offset: 0x002092BF
		[Serialize(0f, IsPropertySaveable.Yes, "The angle of the character's collider when standing.", "", false)]
		[Editable(MinValueFloat = -360f, MaxValueFloat = 360f)]
		public float ColliderStandAngle
		{
			get
			{
				return MathHelper.ToDegrees(this.ColliderStandAngleInRadians);
			}
			set
			{
				this.ColliderStandAngleInRadians = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000D7B RID: 3451
		// (get) Token: 0x06003310 RID: 13072 RVA: 0x0020B0CD File Offset: 0x002092CD
		// (set) Token: 0x06003311 RID: 13073 RVA: 0x0020B0D5 File Offset: 0x002092D5
		public float ColliderStandAngleInRadians { get; private set; }

		// Token: 0x17000D7C RID: 3452
		// (get) Token: 0x06003312 RID: 13074 RVA: 0x0020B0DE File Offset: 0x002092DE
		// (set) Token: 0x06003313 RID: 13075 RVA: 0x0020B0EB File Offset: 0x002092EB
		[Serialize(null, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string FootAngles
		{
			get
			{
				return AnimationParams.ParseFootAngles(this.FootAnglesInRadians);
			}
			set
			{
				AnimationParams.SetFootAngles(this.FootAnglesInRadians, value);
			}
		}

		// Token: 0x17000D7D RID: 3453
		// (get) Token: 0x06003314 RID: 13076 RVA: 0x0020B0F9 File Offset: 0x002092F9
		// (set) Token: 0x06003315 RID: 13077 RVA: 0x0020B101 File Offset: 0x00209301
		public Dictionary<int, float> FootAnglesInRadians { get; set; } = new Dictionary<int, float>();

		// Token: 0x17000D7E RID: 3454
		// (get) Token: 0x06003316 RID: 13078 RVA: 0x0020B10A File Offset: 0x0020930A
		// (set) Token: 0x06003317 RID: 13079 RVA: 0x0020B12A File Offset: 0x0020932A
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float TailAngle
		{
			get
			{
				if (!float.IsNaN(this.TailAngleInRadians))
				{
					return MathHelper.ToDegrees(this.TailAngleInRadians);
				}
				return float.NaN;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this.TailAngleInRadians = MathHelper.ToRadians(value);
				}
			}
		}

		// Token: 0x17000D7F RID: 3455
		// (get) Token: 0x06003318 RID: 13080 RVA: 0x0020B140 File Offset: 0x00209340
		// (set) Token: 0x06003319 RID: 13081 RVA: 0x0020B148 File Offset: 0x00209348
		public float TailAngleInRadians { get; private set; } = float.NaN;
	}
}
