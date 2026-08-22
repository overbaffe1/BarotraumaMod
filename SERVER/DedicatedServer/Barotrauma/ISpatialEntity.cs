using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000232 RID: 562
	internal interface ISpatialEntity
	{
		// Token: 0x17000B11 RID: 2833
		// (get) Token: 0x0600268E RID: 9870
		Vector2 Position { get; }

		// Token: 0x17000B12 RID: 2834
		// (get) Token: 0x0600268F RID: 9871
		Vector2 WorldPosition { get; }

		// Token: 0x17000B13 RID: 2835
		// (get) Token: 0x06002690 RID: 9872
		Vector2 SimPosition { get; }

		// Token: 0x17000B14 RID: 2836
		// (get) Token: 0x06002691 RID: 9873
		Submarine Submarine { get; }

		// Token: 0x06002692 RID: 9874 RVA: 0x000FC8E8 File Offset: 0x000FAAE8
		public static bool IsTargetVisible(ISpatialEntity target, ISpatialEntity seeingEntity, bool seeThroughWindows = false, bool checkFacing = false)
		{
			Character seeingCharacter = seeingEntity as Character;
			if (seeingCharacter != null)
			{
				return seeingCharacter.CanSeeTarget(target, null, seeThroughWindows, checkFacing);
			}
			Character targetCharacter = target as Character;
			if (targetCharacter != null)
			{
				return ISpatialEntity.IsCharacterVisible(targetCharacter, seeingEntity, seeThroughWindows, checkFacing);
			}
			return ISpatialEntity.CheckVisibility(target, seeingEntity, seeThroughWindows, checkFacing);
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x000FC928 File Offset: 0x000FAB28
		public static bool IsCharacterVisible(Character target, ISpatialEntity seeingEntity, bool seeThroughWindows = false, bool checkFacing = false)
		{
			if (target == null || target.Removed)
			{
				return false;
			}
			if (seeingEntity == null)
			{
				return false;
			}
			if (ISpatialEntity.CheckVisibility(target, seeingEntity, seeThroughWindows, checkFacing))
			{
				return true;
			}
			if (!target.AnimController.SimplePhysicsEnabled)
			{
				Limb leftExtremity = null;
				Limb rightExtremity = null;
				float leftMostDot = 0f;
				float rightMostDot = 0f;
				Vector2 dir = target.WorldPosition - seeingEntity.WorldPosition;
				Vector2 leftDir = new Vector2(dir.Y, -dir.X);
				Vector2 rightDir = new Vector2(-dir.Y, dir.X);
				foreach (Limb limb in target.AnimController.Limbs)
				{
					if (!limb.IsSevered && limb != target.AnimController.MainLimb && !limb.Hidden)
					{
						Vector2 limbDir = limb.WorldPosition - seeingEntity.WorldPosition;
						float leftDot = Vector2.Dot(limbDir, leftDir);
						if (leftDot > leftMostDot)
						{
							leftMostDot = leftDot;
							leftExtremity = limb;
						}
						else
						{
							float rightDot = Vector2.Dot(limbDir, rightDir);
							if (rightDot > rightMostDot)
							{
								rightMostDot = rightDot;
								rightExtremity = limb;
							}
						}
					}
				}
				if (leftExtremity != null && ISpatialEntity.CheckVisibility(leftExtremity, seeingEntity, seeThroughWindows, checkFacing))
				{
					return true;
				}
				if (rightExtremity != null && ISpatialEntity.CheckVisibility(rightExtremity, seeingEntity, seeThroughWindows, checkFacing))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x000FCA60 File Offset: 0x000FAC60
		public static bool CheckVisibility(ISpatialEntity target, ISpatialEntity seeingEntity, bool seeThroughWindows = true, bool checkFacing = false)
		{
			ISpatialEntity.<>c__DisplayClass10_0 CS$<>8__locals1 = new ISpatialEntity.<>c__DisplayClass10_0();
			CS$<>8__locals1.seeThroughWindows = seeThroughWindows;
			CS$<>8__locals1.target = target;
			if (CS$<>8__locals1.target == null)
			{
				return false;
			}
			if (seeingEntity == null)
			{
				return false;
			}
			Vector2 diff = ConvertUnits.ToSimUnits(CS$<>8__locals1.target.WorldPosition - seeingEntity.WorldPosition);
			if (checkFacing)
			{
				Character seeingCharacter = seeingEntity as Character;
				if (seeingCharacter != null && (float)Math.Sign(diff.X) != seeingCharacter.AnimController.Dir)
				{
					return false;
				}
			}
			if (CS$<>8__locals1.target.Submarine == seeingEntity.Submarine || CS$<>8__locals1.target.Submarine == null)
			{
				return Submarine.CheckVisibility(seeingEntity.SimPosition, seeingEntity.SimPosition + diff, false, false, true, true, true, new Predicate<Fixture>(CS$<>8__locals1.<CheckVisibility>g__IsBlocking|0)) == null;
			}
			if (seeingEntity.Submarine == null)
			{
				return Submarine.CheckVisibility(CS$<>8__locals1.target.SimPosition, CS$<>8__locals1.target.SimPosition - diff, false, false, true, true, true, new Predicate<Fixture>(CS$<>8__locals1.<CheckVisibility>g__IsBlocking|0)) == null;
			}
			return Submarine.CheckVisibility(seeingEntity.SimPosition, seeingEntity.SimPosition + diff, false, false, true, true, true, new Predicate<Fixture>(CS$<>8__locals1.<CheckVisibility>g__IsBlocking|0)) == null && Submarine.CheckVisibility(CS$<>8__locals1.target.SimPosition, CS$<>8__locals1.target.SimPosition - diff, false, false, true, true, true, new Predicate<Fixture>(CS$<>8__locals1.<CheckVisibility>g__IsBlocking|0)) == null;
		}
	}
}
