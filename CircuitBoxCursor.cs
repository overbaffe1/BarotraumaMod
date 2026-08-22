using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FE RID: 510
	internal sealed class CircuitBoxCursor
	{
		// Token: 0x06003507 RID: 13575 RVA: 0x0020F4A4 File Offset: 0x0020D6A4
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

		// Token: 0x06003508 RID: 13576 RVA: 0x0020F504 File Offset: 0x0020D704
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

		// Token: 0x17000E34 RID: 3636
		// (get) Token: 0x06003509 RID: 13577 RVA: 0x0020F54B File Offset: 0x0020D74B
		// (set) Token: 0x0600350A RID: 13578 RVA: 0x0020F553 File Offset: 0x0020D753
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

		// Token: 0x0600350B RID: 13579 RVA: 0x0020F55C File Offset: 0x0020D75C
		[NullableContext(1)]
		public static Color GenerateColor(string name)
		{
			Random random = new Random(ToolBox.StringToInt(name));
			return ToolBoxCore.HSVToRGB(random.NextSingle() * 360f, 1f, 1f);
		}

		// Token: 0x17000E35 RID: 3637
		// (get) Token: 0x0600350C RID: 13580 RVA: 0x0020F590 File Offset: 0x0020D790
		public bool IsActive
		{
			get
			{
				return this.updateTimer < 5f;
			}
		}

		// Token: 0x0600350D RID: 13581 RVA: 0x0020F5A0 File Offset: 0x0020D7A0
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

		// Token: 0x0600350E RID: 13582 RVA: 0x0020F698 File Offset: 0x0020D898
		public void ResetTimers()
		{
			this.positionTimer = 0f;
			this.updateTimer = 0f;
		}

		// Token: 0x06003511 RID: 13585 RVA: 0x0020F718 File Offset: 0x0020D918
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

		// Token: 0x06003512 RID: 13586 RVA: 0x0020F767 File Offset: 0x0020D967
		[CompilerGenerated]
		internal static bool <Update>g__IsInRange|16_0(int index, int length)
		{
			return index >= 0 && index < length;
		}

		// Token: 0x04001B99 RID: 7065
		public NetCircuitBoxCursorInfo Info;

		// Token: 0x04001B9B RID: 7067
		public Color Color;

		// Token: 0x04001B9C RID: 7068
		private const float UpdateTimeout = 5f;

		// Token: 0x04001B9D RID: 7069
		private float updateTimer;

		// Token: 0x04001B9E RID: 7070
		private float positionTimer;

		// Token: 0x04001B9F RID: 7071
		private Vector2 prevPosition;

		// Token: 0x04001BA0 RID: 7072
		public Vector2 DrawPosition;
	}
}
