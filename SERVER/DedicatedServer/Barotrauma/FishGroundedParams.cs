using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000E3 RID: 227
	internal abstract class FishGroundedParams : GroundedMovementParams, IFishAnimation
	{
		// Token: 0x06001801 RID: 6145 RVA: 0x000C47DC File Offset: 0x000C29DC
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

		// Token: 0x1700070B RID: 1803
		// (get) Token: 0x06001802 RID: 6146 RVA: 0x000C4836 File Offset: 0x000C2A36
		// (set) Token: 0x06001803 RID: 6147 RVA: 0x000C483E File Offset: 0x000C2A3E
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the character be flipped depending on which direction it faces. Should usually be enabled on all characters that have distinctive upper and lower sides.", "", false)]
		public bool Flip { get; set; }

		// Token: 0x1700070C RID: 1804
		// (get) Token: 0x06001804 RID: 6148 RVA: 0x000C4847 File Offset: 0x000C2A47
		// (set) Token: 0x06001805 RID: 6149 RVA: 0x000C484F File Offset: 0x000C2A4F
		[Serialize(1f, IsPropertySaveable.Yes, "Reduces continuous flipping when the character abruptly changes direction.", "", false)]
		[Editable]
		public float FlipCooldown { get; set; }

		// Token: 0x1700070D RID: 1805
		// (get) Token: 0x06001806 RID: 6150 RVA: 0x000C4858 File Offset: 0x000C2A58
		// (set) Token: 0x06001807 RID: 6151 RVA: 0x000C4860 File Offset: 0x000C2A60
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much it takes before the character flips. The timer starts when the character starts to move in the different direction.", "", false)]
		[Editable]
		public float FlipDelay { get; set; }

		// Token: 0x1700070E RID: 1806
		// (get) Token: 0x06001808 RID: 6152 RVA: 0x000C4869 File Offset: 0x000C2A69
		// (set) Token: 0x06001809 RID: 6153 RVA: 0x000C4871 File Offset: 0x000C2A71
		[Serialize(10f, IsPropertySaveable.Yes, "How much force is used to move the head to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float HeadMoveForce { get; set; }

		// Token: 0x1700070F RID: 1807
		// (get) Token: 0x0600180A RID: 6154 RVA: 0x000C487A File Offset: 0x000C2A7A
		// (set) Token: 0x0600180B RID: 6155 RVA: 0x000C4882 File Offset: 0x000C2A82
		[Serialize(10f, IsPropertySaveable.Yes, "How much force is used to move the torso to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float TorsoMoveForce { get; set; }

		// Token: 0x17000710 RID: 1808
		// (get) Token: 0x0600180C RID: 6156 RVA: 0x000C488B File Offset: 0x000C2A8B
		// (set) Token: 0x0600180D RID: 6157 RVA: 0x000C4893 File Offset: 0x000C2A93
		[Serialize(8f, IsPropertySaveable.Yes, "How much force is used to move the feet to the correct position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float FootMoveForce { get; set; }

		// Token: 0x17000711 RID: 1809
		// (get) Token: 0x0600180E RID: 6158 RVA: 0x000C489C File Offset: 0x000C2A9C
		// (set) Token: 0x0600180F RID: 6159 RVA: 0x000C48A4 File Offset: 0x000C2AA4
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the tail to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float TailTorque { get; set; }

		// Token: 0x17000712 RID: 1810
		// (get) Token: 0x06001810 RID: 6160 RVA: 0x000C48AD File Offset: 0x000C2AAD
		// (set) Token: 0x06001811 RID: 6161 RVA: 0x000C48B5 File Offset: 0x000C2AB5
		[Serialize(0f, IsPropertySaveable.Yes, "Optional torque that's constantly applied to legs.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float LegTorque { get; set; }

		// Token: 0x17000713 RID: 1811
		// (get) Token: 0x06001812 RID: 6162 RVA: 0x000C48BE File Offset: 0x000C2ABE
		// (set) Token: 0x06001813 RID: 6163 RVA: 0x000C48CB File Offset: 0x000C2ACB
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

		// Token: 0x17000714 RID: 1812
		// (get) Token: 0x06001814 RID: 6164 RVA: 0x000C48D9 File Offset: 0x000C2AD9
		// (set) Token: 0x06001815 RID: 6165 RVA: 0x000C48E1 File Offset: 0x000C2AE1
		public float ColliderStandAngleInRadians { get; private set; }

		// Token: 0x17000715 RID: 1813
		// (get) Token: 0x06001816 RID: 6166 RVA: 0x000C48EA File Offset: 0x000C2AEA
		// (set) Token: 0x06001817 RID: 6167 RVA: 0x000C48F7 File Offset: 0x000C2AF7
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

		// Token: 0x17000716 RID: 1814
		// (get) Token: 0x06001818 RID: 6168 RVA: 0x000C4905 File Offset: 0x000C2B05
		// (set) Token: 0x06001819 RID: 6169 RVA: 0x000C490D File Offset: 0x000C2B0D
		public Dictionary<int, float> FootAnglesInRadians { get; set; } = new Dictionary<int, float>();

		// Token: 0x17000717 RID: 1815
		// (get) Token: 0x0600181A RID: 6170 RVA: 0x000C4916 File Offset: 0x000C2B16
		// (set) Token: 0x0600181B RID: 6171 RVA: 0x000C4936 File Offset: 0x000C2B36
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

		// Token: 0x17000718 RID: 1816
		// (get) Token: 0x0600181C RID: 6172 RVA: 0x000C494C File Offset: 0x000C2B4C
		// (set) Token: 0x0600181D RID: 6173 RVA: 0x000C4954 File Offset: 0x000C2B54
		public float TailAngleInRadians { get; private set; } = float.NaN;
	}
}
