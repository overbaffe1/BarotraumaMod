using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000481 RID: 1153
	internal static class NetConfig
	{
		// Token: 0x06004DD0 RID: 19920 RVA: 0x002ABA24 File Offset: 0x002A9C24
		public static Vector2 InterpolateSimPositionError(Vector2 simPositionError, float? smoothingFactor = null)
		{
			float lengthSqr = simPositionError.LengthSquared();
			if (lengthSqr > 100f)
			{
				return Vector2.Zero;
			}
			float positionSmoothingFactor = smoothingFactor ?? MathHelper.Lerp(0.95f, 0.8f, MathHelper.Clamp(lengthSqr, 0f, 1f));
			return simPositionError *= positionSmoothingFactor;
		}

		// Token: 0x06004DD1 RID: 19921 RVA: 0x002ABA88 File Offset: 0x002A9C88
		public static float InterpolateRotationError(float rotationError)
		{
			if (rotationError > 6.2831855f)
			{
				return 0f;
			}
			float rotationSmoothingFactor = MathHelper.Lerp(0.95f, 0.8f, Math.Min(Math.Abs(rotationError), 1f));
			return rotationError *= rotationSmoothingFactor;
		}

		// Token: 0x06004DD2 RID: 19922 RVA: 0x002ABACC File Offset: 0x002A9CCC
		public static Vector2 InterpolateCursorPositionError(Vector2 cursorPositionError)
		{
			float lengthSqr = cursorPositionError.LengthSquared();
			if (lengthSqr > 1000f)
			{
				return Vector2.Zero;
			}
			return cursorPositionError *= 0.7f;
		}

		// Token: 0x06004DD3 RID: 19923 RVA: 0x002ABAFD File Offset: 0x002A9CFD
		public static Vector2 Quantize(Vector2 value, float min, float max, int numberOfBits)
		{
			return new Vector2(NetConfig.Quantize(value.X, min, max, numberOfBits), NetConfig.Quantize(value.Y, min, max, numberOfBits));
		}

		// Token: 0x06004DD4 RID: 19924 RVA: 0x002ABB20 File Offset: 0x002A9D20
		public static float Quantize(float value, float min, float max, int numberOfBits)
		{
			value = MathHelper.Clamp(value, min, max);
			float step = (max - min) / (float)((1 << numberOfBits) - 1);
			if (Math.Abs(value) < step + 1E-05f)
			{
				return 0f;
			}
			return MathUtils.RoundTowardsClosest(value - min, step) + min;
		}

		// Token: 0x04002903 RID: 10499
		public const int DefaultPort = 27015;

		// Token: 0x04002904 RID: 10500
		public const int DefaultQueryPort = 27016;

		// Token: 0x04002905 RID: 10501
		public static int MaxPlayers = 256;

		// Token: 0x04002906 RID: 10502
		public static int ServerNameMaxLength = 60;

		// Token: 0x04002907 RID: 10503
		public static int ServerMessageMaxLength = 2000;

		// Token: 0x04002908 RID: 10504
		public const float MaxPhysicsBodyVelocity = 64f;

		// Token: 0x04002909 RID: 10505
		public const float MaxPhysicsBodyAngularVelocity = 16f;

		// Token: 0x0400290A RID: 10506
		public static float MaxHealthUpdateInterval = 2f;

		// Token: 0x0400290B RID: 10507
		public static float MaxHealthUpdateIntervalDead = 10f;

		// Token: 0x0400290C RID: 10508
		public static float HighPrioCharacterPositionUpdateDistance = 1000f;

		// Token: 0x0400290D RID: 10509
		public static float LowPrioCharacterPositionUpdateDistance = 10000f;

		// Token: 0x0400290E RID: 10510
		public static float HighPrioCharacterPositionUpdateInterval = 0f;

		// Token: 0x0400290F RID: 10511
		public static float LowPrioCharacterPositionUpdateInterval = 1f;

		// Token: 0x04002910 RID: 10512
		public static float FreezeCharacterIfPositionDataMissingDelay = 2f;

		// Token: 0x04002911 RID: 10513
		public static float DisableCharacterIfPositionDataMissingDelay = 3.5f;

		// Token: 0x04002912 RID: 10514
		public static float DeleteDisconnectedTime = 20f;

		// Token: 0x04002913 RID: 10515
		public static float ItemConditionUpdateInterval = 0.15f;

		// Token: 0x04002914 RID: 10516
		public static float LevelObjectUpdateInterval = 0.5f;

		// Token: 0x04002915 RID: 10517
		public static float HullUpdateInterval = 0.5f;

		// Token: 0x04002916 RID: 10518
		public static float SparseHullUpdateInterval = 5f;

		// Token: 0x04002917 RID: 10519
		public static float HullUpdateDistance = 20000f;

		// Token: 0x04002918 RID: 10520
		public static int MaxEventPacketsPerUpdate = 4;

		// Token: 0x04002919 RID: 10521
		public static bool UseLenientHandshake;
	}
}
