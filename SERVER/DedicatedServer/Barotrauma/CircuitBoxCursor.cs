using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000103 RID: 259
	internal sealed class CircuitBoxCursor
	{
		// Token: 0x06001A0B RID: 6667 RVA: 0x000C8D58 File Offset: 0x000C6F58
		public CircuitBoxCursor(NetCircuitBoxCursorInfo info)
		{
			Option.UnspecifiedNone none = Option.None;
			this.HeldPrefab = none;
			this.Color = Color.White;
			base..ctor();
			Character c = Entity.FindEntityByID(info.CharacterID) as Character;
			if (c != null)
			{
				this.Color = CircuitBoxCursor.GenerateColor(c.Name);
			}
			this.UpdateInfo(info);
		}

		// Token: 0x06001A0C RID: 6668 RVA: 0x000C8DB8 File Offset: 0x000C6FB8
		public void UpdateInfo(NetCircuitBoxCursorInfo newInfo)
		{
			this.Info = newInfo;
			newInfo.HeldItem.Match(delegate(Identifier newIdentifier)
			{
				this.HeldPrefab.Match(delegate(ItemPrefab oldPrefab)
				{
					if (oldPrefab.Identifier == newIdentifier)
					{
						return;
					}
					this.<UpdateInfo>g__SetHeldPrefab|2_2(newIdentifier);
				}, delegate()
				{
					this.<UpdateInfo>g__SetHeldPrefab|2_2(newIdentifier);
				});
			}, delegate()
			{
				Option.UnspecifiedNone none = Option.None;
				this.HeldPrefab = none;
			});
			this.prevPosition = this.DrawPosition;
		}

		// Token: 0x170007CD RID: 1997
		// (get) Token: 0x06001A0D RID: 6669 RVA: 0x000C8DFF File Offset: 0x000C6FFF
		// (set) Token: 0x06001A0E RID: 6670 RVA: 0x000C8E07 File Offset: 0x000C7007
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<ItemPrefab> HeldPrefab { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] private set; }

		// Token: 0x06001A0F RID: 6671 RVA: 0x000C8E10 File Offset: 0x000C7010
		[NullableContext(1)]
		public static Color GenerateColor(string name)
		{
			Random random = new Random(ToolBox.StringToInt(name));
			return ToolBoxCore.HSVToRGB(random.NextSingle() * 360f, 1f, 1f);
		}

		// Token: 0x170007CE RID: 1998
		// (get) Token: 0x06001A10 RID: 6672 RVA: 0x000C8E44 File Offset: 0x000C7044
		public bool IsActive
		{
			get
			{
				return this.updateTimer < 5f;
			}
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x000C8E54 File Offset: 0x000C7054
		public void Update(float deltaTime)
		{
			this.updateTimer += deltaTime;
			Vector2[] recordedPositions = this.Info.RecordedPositions;
			Vector2 finalPosition = recordedPositions[recordedPositions.Length - 1];
			if (this.positionTimer > 1f)
			{
				this.DrawPosition = finalPosition;
				this.prevPosition = Vector2.Zero;
				return;
			}
			this.positionTimer += deltaTime;
			float stepTimer = this.positionTimer * 10f;
			int targetPositonIndex = (int)MathF.Floor(stepTimer);
			int prevPosIndex = targetPositonIndex - 1;
			Vector2 targetPosition = CircuitBoxCursor.<Update>g__IsInRange|16_0(targetPositonIndex, this.Info.RecordedPositions.Length) ? this.Info.RecordedPositions[targetPositonIndex] : finalPosition;
			Vector2 prevTargetPosition = CircuitBoxCursor.<Update>g__IsInRange|16_0(prevPosIndex, this.Info.RecordedPositions.Length) ? this.Info.RecordedPositions[prevPosIndex] : this.prevPosition;
			this.DrawPosition = Vector2.Lerp(prevTargetPosition, targetPosition, MathHelper.Clamp(stepTimer % 1f, 0f, 1f));
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x000C8F4C File Offset: 0x000C714C
		public void ResetTimers()
		{
			this.positionTimer = 0f;
			this.updateTimer = 0f;
		}

		// Token: 0x06001A15 RID: 6677 RVA: 0x000C8FCC File Offset: 0x000C71CC
		[CompilerGenerated]
		private void <UpdateInfo>g__SetHeldPrefab|2_2(Identifier identifier)
		{
			ItemPrefab prefab2 = ItemPrefab.Prefabs.Find((ItemPrefab prefab) => prefab.Identifier.Equals(identifier));
			Option<ItemPrefab> heldPrefab;
			if (prefab2 != null)
			{
				heldPrefab = Option.Some<ItemPrefab>(prefab2);
			}
			else
			{
				Option.UnspecifiedNone none = Option.None;
				heldPrefab = none;
			}
			this.HeldPrefab = heldPrefab;
		}

		// Token: 0x06001A16 RID: 6678 RVA: 0x000C901B File Offset: 0x000C721B
		[CompilerGenerated]
		internal static bool <Update>g__IsInRange|16_0(int index, int length)
		{
			return index >= 0 && index < length;
		}

		// Token: 0x04000C75 RID: 3189
		public NetCircuitBoxCursorInfo Info;

		// Token: 0x04000C77 RID: 3191
		public Color Color;

		// Token: 0x04000C78 RID: 3192
		private const float UpdateTimeout = 5f;

		// Token: 0x04000C79 RID: 3193
		private float updateTimer;

		// Token: 0x04000C7A RID: 3194
		private float positionTimer;

		// Token: 0x04000C7B RID: 3195
		private Vector2 prevPosition;

		// Token: 0x04000C7C RID: 3196
		public Vector2 DrawPosition;
	}
}
