using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000385 RID: 901
	internal static class NetConfig
	{
		// Token: 0x06003614 RID: 13844 RVA: 0x0017305C File Offset: 0x0017125C
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

		// Token: 0x06003615 RID: 13845 RVA: 0x001730C0 File Offset: 0x001712C0
		public static float InterpolateRotationError(float rotationError)
		{
			if (rotationError > 6.2831855f)
			{
				return 0f;
			}
			float rotationSmoothingFactor = MathHelper.Lerp(0.95f, 0.8f, Math.Min(Math.Abs(rotationError), 1f));
			return rotationError *= rotationSmoothingFactor;
		}

		// Token: 0x06003616 RID: 13846 RVA: 0x00173104 File Offset: 0x00171304
		public static Vector2 InterpolateCursorPositionError(Vector2 cursorPositionError)
		{
			float lengthSqr = cursorPositionError.LengthSquared();
			if (lengthSqr > 1000f)
			{
				return Vector2.Zero;
			}
			return cursorPositionError *= 0.7f;
		}

		// Token: 0x06003617 RID: 13847 RVA: 0x00173135 File Offset: 0x00171335
		public static Vector2 Quantize(Vector2 value, float min, float max, int numberOfBits)
		{
			return new Vector2(NetConfig.Quantize(value.X, min, max, numberOfBits), NetConfig.Quantize(value.Y, min, max, numberOfBits));
		}

		// Token: 0x06003618 RID: 13848 RVA: 0x00173158 File Offset: 0x00171358
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

		// Token: 0x04001B18 RID: 6936
		public const int DefaultPort = 27015;

		// Token: 0x04001B19 RID: 6937
		public const int DefaultQueryPort = 27016;

		// Token: 0x04001B1A RID: 6938
		public static int MaxPlayers = 256;

		// Token: 0x04001B1B RID: 6939
		public static int ServerNameMaxLength = 60;

		// Token: 0x04001B1C RID: 6940
		public static int ServerMessageMaxLength = 2000;

		// Token: 0x04001B1D RID: 6941
		public const float MaxPhysicsBodyVelocity = 64f;

		// Token: 0x04001B1E RID: 6942
		public const float MaxPhysicsBodyAngularVelocity = 16f;

		// Token: 0x04001B1F RID: 6943
		public static float MaxHealthUpdateInterval = 2f;

		// Token: 0x04001B20 RID: 6944
		public static float MaxHealthUpdateIntervalDead = 10f;

		// Token: 0x04001B21 RID: 6945
		public static float HighPrioCharacterPositionUpdateDistance = 1000f;

		// Token: 0x04001B22 RID: 6946
		public static float LowPrioCharacterPositionUpdateDistance = 10000f;

		// Token: 0x04001B23 RID: 6947
		public static float HighPrioCharacterPositionUpdateInterval = 0f;

		// Token: 0x04001B24 RID: 6948
		public static float LowPrioCharacterPositionUpdateInterval = 1f;

		// Token: 0x04001B25 RID: 6949
		public static float FreezeCharacterIfPositionDataMissingDelay = 2f;

		// Token: 0x04001B26 RID: 6950
		public static float DisableCharacterIfPositionDataMissingDelay = 3.5f;

		// Token: 0x04001B27 RID: 6951
		public static float DeleteDisconnectedTime = 20f;

		// Token: 0x04001B28 RID: 6952
		public static float ItemConditionUpdateInterval = 0.15f;

		// Token: 0x04001B29 RID: 6953
		public static float LevelObjectUpdateInterval = 0.5f;

		// Token: 0x04001B2A RID: 6954
		public static float HullUpdateInterval = 0.5f;

		// Token: 0x04001B2B RID: 6955
		public static float SparseHullUpdateInterval = 5f;

		// Token: 0x04001B2C RID: 6956
		public static float HullUpdateDistance = 20000f;

		// Token: 0x04001B2D RID: 6957
		public static int MaxEventPacketsPerUpdate = 4;

		// Token: 0x04001B2E RID: 6958
		public static bool UseLenientHandshake;
	}
}
