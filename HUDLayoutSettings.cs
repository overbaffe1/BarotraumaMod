using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000A8 RID: 168
	internal static class HUDLayoutSettings
	{
		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001560 RID: 5472 RVA: 0x000C973D File Offset: 0x000C793D
		// (set) Token: 0x06001561 RID: 5473 RVA: 0x000C9744 File Offset: 0x000C7944
		public static int InventoryTopY
		{
			get
			{
				return HUDLayoutSettings.inventoryTopY;
			}
			set
			{
				if (value == HUDLayoutSettings.inventoryTopY)
				{
					return;
				}
				HUDLayoutSettings.inventoryTopY = value;
				HUDLayoutSettings.CreateAreas();
			}
		}

		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x06001562 RID: 5474 RVA: 0x000C975A File Offset: 0x000C795A
		// (set) Token: 0x06001563 RID: 5475 RVA: 0x000C9761 File Offset: 0x000C7961
		public static Rectangle ButtonAreaTop { get; private set; }

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x06001564 RID: 5476 RVA: 0x000C9769 File Offset: 0x000C7969
		// (set) Token: 0x06001565 RID: 5477 RVA: 0x000C9770 File Offset: 0x000C7970
		public static Rectangle TutorialObjectiveListArea { get; private set; }

		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x06001566 RID: 5478 RVA: 0x000C9778 File Offset: 0x000C7978
		// (set) Token: 0x06001567 RID: 5479 RVA: 0x000C977F File Offset: 0x000C797F
		public static Rectangle MessageAreaTop { get; private set; }

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001568 RID: 5480 RVA: 0x000C9787 File Offset: 0x000C7987
		// (set) Token: 0x06001569 RID: 5481 RVA: 0x000C978E File Offset: 0x000C798E
		public static Rectangle CrewArea { get; private set; }

		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x0600156A RID: 5482 RVA: 0x000C9796 File Offset: 0x000C7996
		// (set) Token: 0x0600156B RID: 5483 RVA: 0x000C979D File Offset: 0x000C799D
		public static Rectangle ChatBoxArea { get; private set; }

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x0600156C RID: 5484 RVA: 0x000C97A5 File Offset: 0x000C79A5
		// (set) Token: 0x0600156D RID: 5485 RVA: 0x000C97AC File Offset: 0x000C79AC
		public static Rectangle ObjectiveAnchor { get; private set; }

		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x0600156E RID: 5486 RVA: 0x000C97B4 File Offset: 0x000C79B4
		// (set) Token: 0x0600156F RID: 5487 RVA: 0x000C97BB File Offset: 0x000C79BB
		public static Rectangle InventoryAreaLower { get; private set; }

		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06001570 RID: 5488 RVA: 0x000C97C3 File Offset: 0x000C79C3
		// (set) Token: 0x06001571 RID: 5489 RVA: 0x000C97CA File Offset: 0x000C79CA
		public static Rectangle HealthBarArea { get; private set; }

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x06001572 RID: 5490 RVA: 0x000C97D2 File Offset: 0x000C79D2
		// (set) Token: 0x06001573 RID: 5491 RVA: 0x000C97D9 File Offset: 0x000C79D9
		public static Rectangle BottomRightInfoArea { get; private set; }

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x06001574 RID: 5492 RVA: 0x000C97E1 File Offset: 0x000C79E1
		// (set) Token: 0x06001575 RID: 5493 RVA: 0x000C97E8 File Offset: 0x000C79E8
		public static Rectangle HealthBarAfflictionArea { get; private set; }

		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x06001576 RID: 5494 RVA: 0x000C97F0 File Offset: 0x000C79F0
		// (set) Token: 0x06001577 RID: 5495 RVA: 0x000C97F7 File Offset: 0x000C79F7
		public static Rectangle HealthWindowAreaLeft { get; private set; }

		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001578 RID: 5496 RVA: 0x000C97FF File Offset: 0x000C79FF
		// (set) Token: 0x06001579 RID: 5497 RVA: 0x000C9806 File Offset: 0x000C7A06
		public static Rectangle PortraitArea { get; private set; }

		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x0600157A RID: 5498 RVA: 0x000C980E File Offset: 0x000C7A0E
		// (set) Token: 0x0600157B RID: 5499 RVA: 0x000C9815 File Offset: 0x000C7A15
		public static Rectangle VotingArea { get; private set; }

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x0600157C RID: 5500 RVA: 0x000C981D File Offset: 0x000C7A1D
		// (set) Token: 0x0600157D RID: 5501 RVA: 0x000C9824 File Offset: 0x000C7A24
		public static Rectangle ItemHUDArea { get; private set; }

		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x0600157E RID: 5502 RVA: 0x000C982C File Offset: 0x000C7A2C
		// (set) Token: 0x0600157F RID: 5503 RVA: 0x000C9833 File Offset: 0x000C7A33
		public static int Padding { get; private set; }

		// Token: 0x06001580 RID: 5504 RVA: 0x000C983B File Offset: 0x000C7A3B
		static HUDLayoutSettings()
		{
			if (GameMain.Instance != null)
			{
				GameMain.Instance.ResolutionChanged += HUDLayoutSettings.CreateAreas;
				HUDLayoutSettings.CreateAreas();
				CharacterInfo.Init();
			}
		}

		// Token: 0x06001581 RID: 5505 RVA: 0x000C9864 File Offset: 0x000C7A64
		public static RectTransform ToRectTransform(Rectangle rect, RectTransform parent)
		{
			return new RectTransform(new Vector2((float)rect.Width / (float)GameMain.GraphicsWidth, (float)rect.Height / (float)GameMain.GraphicsHeight), parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2((float)rect.X / (float)GameMain.GraphicsWidth, (float)rect.Y / (float)GameMain.GraphicsHeight)
			};
		}

		// Token: 0x06001582 RID: 5506 RVA: 0x000C98DC File Offset: 0x000C7ADC
		public static void CreateAreas()
		{
			HUDLayoutSettings.Padding = (int)(11f * GUI.Scale);
			if (HUDLayoutSettings.inventoryTopY == 0)
			{
				HUDLayoutSettings.inventoryTopY = GameMain.GraphicsHeight - 30;
			}
			HUDLayoutSettings.ButtonAreaTop = new Rectangle(HUDLayoutSettings.Padding, HUDLayoutSettings.Padding, GameMain.GraphicsWidth - HUDLayoutSettings.Padding * 2, (int)(50f * GUI.Scale));
			int infoAreaWidth = (int)(142f * GUI.Scale);
			int infoAreaHeight = (int)(98f * GUI.Scale);
			int portraitSize = (int)((float)infoAreaHeight * 0.95f);
			HUDLayoutSettings.BottomRightInfoArea = new Rectangle(GameMain.GraphicsWidth - HUDLayoutSettings.Padding * 2 - infoAreaWidth, GameMain.GraphicsHeight - HUDLayoutSettings.Padding * 2 - infoAreaHeight, infoAreaWidth, infoAreaHeight);
			HUDLayoutSettings.PortraitArea = new Rectangle(GameMain.GraphicsWidth - portraitSize, HUDLayoutSettings.BottomRightInfoArea.Bottom - portraitSize + HUDLayoutSettings.Padding / 2, portraitSize, portraitSize);
			int afflictionAreaHeight = (int)(50f * GUI.Scale);
			int healthBarWidth = HUDLayoutSettings.BottomRightInfoArea.Width;
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("CharacterHealthBar");
			Dictionary<Identifier, GUIComponentStyle> healthBarChildStyles = (componentStyle != null) ? componentStyle.ChildStyles : null;
			GUIComponentStyle style;
			List<UISprite> uiSprites;
			if (healthBarChildStyles != null && healthBarChildStyles.TryGetValue("GUIFrame".ToIdentifier(), out style) && style.Sprites.TryGetValue(GUIComponent.ComponentState.None, out uiSprites))
			{
				UISprite uiSprite = uiSprites.FirstOrDefault<UISprite>();
				if (uiSprite != null)
				{
					healthBarWidth += (int)((float)uiSprite.NonSliceSize.X * Math.Min(GUI.Scale, 1f));
				}
			}
			int healthBarHeight = (int)(50f * GUI.Scale);
			HUDLayoutSettings.HealthBarArea = new Rectangle(HUDLayoutSettings.BottomRightInfoArea.Right - healthBarWidth + (int)Math.Floor((double)(1f / GUI.Scale)), HUDLayoutSettings.BottomRightInfoArea.Y - healthBarHeight + GUI.IntScale(10f), healthBarWidth, healthBarHeight);
			HUDLayoutSettings.HealthBarAfflictionArea = new Rectangle(HUDLayoutSettings.HealthBarArea.X, HUDLayoutSettings.HealthBarArea.Y - HUDLayoutSettings.Padding - afflictionAreaHeight, HUDLayoutSettings.HealthBarArea.Width, afflictionAreaHeight);
			int messageAreaWidth = GameMain.GraphicsWidth / 3;
			HUDLayoutSettings.MessageAreaTop = new Rectangle((GameMain.GraphicsWidth - messageAreaWidth) / 2, HUDLayoutSettings.ButtonAreaTop.Bottom + HUDLayoutSettings.ButtonAreaTop.Height, messageAreaWidth, HUDLayoutSettings.ButtonAreaTop.Height);
			int chatBoxWidth = (int)(475f * GUI.Scale * GUI.AspectRatioAdjustment);
			int chatBoxHeight = (int)Math.Max((float)GameMain.GraphicsHeight * 0.25f, 150f);
			HUDLayoutSettings.ChatBoxArea = new Rectangle(HUDLayoutSettings.Padding, GameMain.GraphicsHeight - HUDLayoutSettings.Padding - chatBoxHeight, chatBoxWidth, chatBoxHeight);
			int objectiveAnchorWidth = (int)(250f * GUI.Scale);
			int objectiveAnchorOffsetY = (int)(150f * GUI.Scale);
			HUDLayoutSettings.ObjectiveAnchor = new Rectangle(HUDLayoutSettings.Padding, HUDLayoutSettings.ChatBoxArea.Y - objectiveAnchorOffsetY, objectiveAnchorWidth, 0);
			int crewAreaY = HUDLayoutSettings.ButtonAreaTop.Bottom + HUDLayoutSettings.Padding;
			int crewAreaHeight = HUDLayoutSettings.ObjectiveAnchor.Top - HUDLayoutSettings.Padding - crewAreaY;
			HUDLayoutSettings.CrewArea = new Rectangle(HUDLayoutSettings.Padding, crewAreaY, (int)MathHelper.Clamp(400f * GUI.Scale, 220f, (float)GameMain.GraphicsHeight * 0.4f), crewAreaHeight);
			HUDLayoutSettings.InventoryAreaLower = new Rectangle(HUDLayoutSettings.ChatBoxArea.Right + HUDLayoutSettings.Padding * 7, HUDLayoutSettings.inventoryTopY, GameMain.GraphicsWidth - HUDLayoutSettings.Padding * 9 - HUDLayoutSettings.ChatBoxArea.Width, GameMain.GraphicsHeight - HUDLayoutSettings.inventoryTopY);
			int healthWindowWidth = (int)((float)GameMain.GraphicsWidth * 0.5f);
			int healthWindowHeight = (int)((float)GameMain.GraphicsWidth * 0.5f * 0.65f);
			int healthWindowX = GameMain.GraphicsWidth / 2 - healthWindowWidth / 2;
			int healthWindowY = GameMain.GraphicsHeight / 2 - healthWindowHeight / 2;
			HUDLayoutSettings.HealthWindowAreaLeft = new Rectangle(healthWindowX, healthWindowY, healthWindowWidth, healthWindowHeight);
			int objectiveListAreaX = HUDLayoutSettings.HealthWindowAreaLeft.Right + HUDLayoutSettings.Padding;
			int objectiveListAreaY = HUDLayoutSettings.ButtonAreaTop.Bottom + HUDLayoutSettings.Padding;
			HUDLayoutSettings.TutorialObjectiveListArea = new Rectangle(objectiveListAreaX, objectiveListAreaY, GameMain.GraphicsWidth - HUDLayoutSettings.Padding - objectiveListAreaX, HUDLayoutSettings.HealthBarAfflictionArea.Top - HUDLayoutSettings.Padding - objectiveListAreaY);
			int votingAreaWidth = (int)(400f * GUI.Scale);
			int votingAreaX = GameMain.GraphicsWidth - HUDLayoutSettings.Padding - votingAreaWidth;
			int votingAreaY = HUDLayoutSettings.Padding + HUDLayoutSettings.ButtonAreaTop.Height;
			HUDLayoutSettings.VotingArea = new Rectangle(votingAreaX, votingAreaY, votingAreaWidth, 0);
			HUDLayoutSettings.ItemHUDArea = new Rectangle(0, HUDLayoutSettings.ButtonAreaTop.Bottom, GameMain.GraphicsWidth, GameMain.GraphicsHeight - HUDLayoutSettings.ButtonAreaTop.Bottom - HUDLayoutSettings.InventoryAreaLower.Height);
		}

		// Token: 0x06001583 RID: 5507 RVA: 0x000C9D70 File Offset: 0x000C7F70
		public static void Draw(SpriteBatch spriteBatch)
		{
			HUDLayoutSettings.<>c__DisplayClass68_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("ButtonAreaTop", HUDLayoutSettings.ButtonAreaTop, Color.White * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("TutorialObjectiveListArea", HUDLayoutSettings.TutorialObjectiveListArea, GUIStyle.Blue * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("MessageAreaTop", HUDLayoutSettings.MessageAreaTop, GUIStyle.Orange * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("CrewArea", HUDLayoutSettings.CrewArea, Color.Blue * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("ChatBoxArea", HUDLayoutSettings.ChatBoxArea, Color.Cyan * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("HealthBarArea", HUDLayoutSettings.HealthBarArea, Color.Red * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("HealthBarAfflictionArea", HUDLayoutSettings.HealthBarAfflictionArea, Color.Red * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("InventoryAreaLower", HUDLayoutSettings.InventoryAreaLower, Color.Yellow * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("HealthWindowAreaLeft", HUDLayoutSettings.HealthWindowAreaLeft, Color.Red * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("BottomRightInfoArea", HUDLayoutSettings.BottomRightInfoArea, Color.Green * 0.5f, ref CS$<>8__locals1);
			HUDLayoutSettings.<Draw>g__DrawRectangle|68_0("ItemHUDArea", HUDLayoutSettings.ItemHUDArea, Color.Magenta * 0.3f, ref CS$<>8__locals1);
		}

		// Token: 0x06001584 RID: 5508 RVA: 0x000C9EE8 File Offset: 0x000C80E8
		[CompilerGenerated]
		internal static void <Draw>g__DrawRectangle|68_0(string label, Rectangle r, Color c, ref HUDLayoutSettings.<>c__DisplayClass68_0 A_3)
		{
			if (!label.IsNullOrEmpty())
			{
				SpriteBatch spriteBatch = A_3.spriteBatch;
				Vector2 pos = r.Location.ToVector2() + Vector2.One * 3f;
				GUIFont smallFont = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch, pos, label, c, null, 0, smallFont, ForceUpperCase.Inherit);
			}
			GUI.DrawRectangle(A_3.spriteBatch, r, c, false, 0f, 1f);
		}

		// Token: 0x04000ABB RID: 2747
		public static bool DebugDraw;

		// Token: 0x04000ABC RID: 2748
		private static int inventoryTopY;
	}
}
