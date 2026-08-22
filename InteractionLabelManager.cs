using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000030 RID: 48
	[NullableContext(1)]
	[Nullable(0)]
	public static class InteractionLabelManager
	{
		// Token: 0x1700022A RID: 554
		// (get) Token: 0x060007D9 RID: 2009 RVA: 0x0004821F File Offset: 0x0004641F
		private static float LabelScale
		{
			get
			{
				return 1f / GUI.Scale;
			}
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x0004822C File Offset: 0x0004642C
		// (set) Token: 0x060007DB RID: 2011 RVA: 0x00048233 File Offset: 0x00046433
		[Nullable(2)]
		internal static Item HoveredItem { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x060007DC RID: 2012 RVA: 0x0004823B File Offset: 0x0004643B
		internal static void RefreshInteractablesInRange(List<Item> interactables)
		{
			InteractionLabelManager.interactablesInRange.Clear();
			InteractionLabelManager.interactablesInRange.AddRange(interactables);
			InteractionLabelManager.shouldRecalculate = true;
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00048258 File Offset: 0x00046458
		private static void RecalculateLabelPositions(Camera cam, Character character)
		{
			if (InteractionLabelManager.recalculateEverything)
			{
				InteractionLabelManager.labels.Clear();
				InteractionLabelManager.recalculateEverything = false;
			}
			InteractionLabelManager.labels.RemoveAll((InteractionLabelManager.LabelData l) => !InteractionLabelManager.interactablesInRange.Contains(l.Item));
			using (List<Item>.Enumerator enumerator = InteractionLabelManager.interactablesInRange.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item interactableInRange = enumerator.Current;
					if (!interactableInRange.HasTag(Tags.HiddenItemContainer))
					{
						InteractionLabelDisplayMode interactionLabelDisplayMode = InteractionLabelManager.displayMode;
						if (interactionLabelDisplayMode != InteractionLabelDisplayMode.InteractionAvailable)
						{
							if (interactionLabelDisplayMode == InteractionLabelDisplayMode.LooseItems)
							{
								if (!InteractionLabelManager.IsLooseItem(interactableInRange))
								{
									continue;
								}
							}
						}
						else if (!interactableInRange.HasVisibleInteraction(character))
						{
							continue;
						}
						RectangleF textRect = InteractionLabelManager.GetLabelRect(interactableInRange, cam);
						InteractionLabelManager.LabelData existingLabel = InteractionLabelManager.labels.FirstOrDefault((InteractionLabelManager.LabelData l) => l.Item == interactableInRange);
						if (existingLabel == null)
						{
							InteractionLabelManager.LabelData labelData = new InteractionLabelManager.LabelData(interactableInRange, textRect, RichString.Rich(interactableInRange.Prefab.Name, null), cam);
							InteractionLabelManager.labels.Add(labelData);
						}
						else if (existingLabel.TextRect.Size != textRect.Size)
						{
							existingLabel.TextRect = textRect;
						}
					}
				}
			}
			InteractionLabelManager.PreventInteractionLabelOverlap(character.Position);
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x000483C8 File Offset: 0x000465C8
		private static bool IsLooseItem(Item item)
		{
			PhysicsBody body = item.body;
			bool hasActivePhysics = body != null && body.Enabled;
			bool hasPickableComponent = item.GetComponent<Pickable>() != null;
			return hasActivePhysics && hasPickableComponent;
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x000483F8 File Offset: 0x000465F8
		private static RectangleF GetLabelRect(Item item, Camera cam)
		{
			string nameText = RichString.Rich(item.Prefab.Name, null).SanitizedValue;
			ScalableFont font = GUIStyle.SubHeadingFont.GetFontForStr(nameText);
			Vector2 itemTextSizeScreen = font.MeasureString(nameText, false) * InteractionLabelManager.LabelScale;
			Vector2 interactablePosScreen = cam.WorldToScreen(item.Position);
			RectangleF textRect = new RectangleF(interactablePosScreen.X, interactablePosScreen.Y, itemTextSizeScreen.X, itemTextSizeScreen.Y);
			textRect.X -= textRect.Width / 2f;
			textRect.Y += textRect.Height / 2f;
			textRect.Inflate(4f * InteractionLabelManager.LabelScale, 4f * InteractionLabelManager.LabelScale);
			textRect.Location = cam.ScreenToWorld(textRect.Location);
			return textRect;
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x000484CC File Offset: 0x000466CC
		private static void PreventInteractionLabelOverlap(Vector2 centerPos)
		{
			InteractionLabelManager.labels.Sort((InteractionLabelManager.LabelData l1, InteractionLabelManager.LabelData l2) => Vector2.DistanceSquared(l1.TextRect.Center, centerPos).CompareTo(Vector2.DistanceSquared(l2.TextRect.Center, centerPos)));
			bool intersections = true;
			int iterations = 0;
			int maxIterations = Math.Max(InteractionLabelManager.labels.Count * InteractionLabelManager.labels.Count, 100);
			while (intersections && iterations < maxIterations)
			{
				intersections = false;
				foreach (InteractionLabelManager.LabelData label in InteractionLabelManager.labels)
				{
					if (!label.OverlapPreventionDone)
					{
						foreach (InteractionLabelManager.LabelData otherLabel in InteractionLabelManager.labels)
						{
							if (label != otherLabel && (label.Item.Prefab != otherLabel.Item.Prefab || Vector2.DistanceSquared(label.Item.WorldPosition, otherLabel.Item.WorldPosition) >= 1f) && label.TextRect.Intersects(otherLabel.TextRect))
							{
								intersections = true;
								Vector2 moveAmount = Vector2.Normalize(label.TextRect.Center - centerPos) * 10f;
								label.TextRect = new RectangleF(label.TextRect.Location + moveAmount, label.TextRect.Size);
							}
						}
						if (intersections)
						{
							break;
						}
					}
				}
				iterations++;
			}
			foreach (InteractionLabelManager.LabelData labelData in InteractionLabelManager.labels)
			{
				labelData.OverlapPreventionDone = true;
			}
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x000486F4 File Offset: 0x000468F4
		private static int GetMouseHoveredLabelIndex(Camera cam)
		{
			for (int i = 0; i < InteractionLabelManager.labels.Count; i++)
			{
				InteractionLabelManager.LabelData labelData = InteractionLabelManager.labels[i];
				if (labelData.GetScreenDrawRect(cam).Contains(PlayerInput.MousePosition))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x0004873C File Offset: 0x0004693C
		private static bool RefreshSettings()
		{
			bool settingsChanged = false;
			if (GameSettings.CurrentConfig.InteractionLabelDisplayMode != InteractionLabelManager.displayMode)
			{
				InteractionLabelManager.displayMode = GameSettings.CurrentConfig.InteractionLabelDisplayMode;
				settingsChanged = true;
			}
			if (GameMain.GraphicsWidth != InteractionLabelManager.graphicsWidth || GameMain.GraphicsHeight != InteractionLabelManager.graphicsHeight)
			{
				InteractionLabelManager.graphicsWidth = GameMain.GraphicsWidth;
				InteractionLabelManager.graphicsHeight = GameMain.GraphicsHeight;
				settingsChanged = true;
			}
			return settingsChanged;
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x0004879C File Offset: 0x0004699C
		internal static void Update(Character character, Camera cam)
		{
			if (InteractionLabelManager.RefreshSettings())
			{
				InteractionLabelManager.shouldRecalculate = true;
				InteractionLabelManager.recalculateEverything = true;
			}
			if (InteractionLabelManager.shouldRecalculate)
			{
				InteractionLabelManager.RecalculateLabelPositions(cam, character);
			}
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x000487C0 File Offset: 0x000469C0
		internal static void DrawLabels(SpriteBatch spriteBatch, Camera cam, Character character)
		{
			foreach (InteractionLabelManager.LabelData label in InteractionLabelManager.labels)
			{
				if (Vector2.DistanceSquared(label.OriginalItemPosition, label.Item.Position) > 22500f)
				{
					label.TextRect = InteractionLabelManager.GetLabelRect(label.Item, cam);
				}
			}
			int mouseOnLabelIndex = InteractionLabelManager.GetMouseHoveredLabelIndex(cam);
			bool isMouseOnLabel = mouseOnLabelIndex >= 0;
			if (!isMouseOnLabel)
			{
				InteractionLabelManager.HoveredItem = null;
			}
			for (int i = 0; i < InteractionLabelManager.labels.Count; i++)
			{
				if (i != mouseOnLabelIndex)
				{
					InteractionLabelManager.DrawLineForLabel(spriteBatch, cam, InteractionLabelManager.labels[i], GUIStyle.InteractionLabelColor * 0.5f);
				}
			}
			for (int j = 0; j < InteractionLabelManager.labels.Count; j++)
			{
				if (j != mouseOnLabelIndex)
				{
					InteractionLabelManager.DrawLabelForItem(spriteBatch, cam, InteractionLabelManager.labels[j], GUIStyle.InteractionLabelColor);
				}
			}
			if (isMouseOnLabel)
			{
				InteractionLabelManager.LabelData labelData = InteractionLabelManager.labels[mouseOnLabelIndex];
				InteractionLabelManager.HoveredItem = labelData.Item;
				InteractionLabelManager.DrawLineForLabel(spriteBatch, cam, labelData, GUIStyle.InteractionLabelHoverColor * 0.5f);
				InteractionLabelManager.DrawLabelForItem(spriteBatch, cam, labelData, GUIStyle.InteractionLabelHoverColor);
			}
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x00048914 File Offset: 0x00046B14
		private static void DrawLineForLabel(SpriteBatch spriteBatch, Camera cam, InteractionLabelManager.LabelData labelData, Color color)
		{
			RectangleF drawRect = labelData.GetScreenDrawRect(cam);
			float deflateAmount = 1f * GUI.Scale;
			deflateAmount = MathHelper.Max(deflateAmount * Screen.Selected.Cam.Zoom, 1f);
			drawRect.Inflate(-deflateAmount, -deflateAmount);
			Vector2 itemDrawPosScreen = labelData.GetInteractableDrawPositionScreen();
			if (drawRect.Contains(itemDrawPosScreen))
			{
				return;
			}
			Vector2 textLineAnchorScreenPos = new Vector2(MathHelper.Clamp(itemDrawPosScreen.X, drawRect.Left, drawRect.Right), MathHelper.Clamp(itemDrawPosScreen.Y, drawRect.Top, drawRect.Bottom));
			GUI.DrawLine(spriteBatch, textLineAnchorScreenPos, itemDrawPosScreen, color, 0f, 2f);
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x000489BC File Offset: 0x00046BBC
		private static void DrawLabelForItem(SpriteBatch spriteBatch, Camera cam, InteractionLabelManager.LabelData labelData, Color color)
		{
			float scale = Screen.Selected.Cam.Zoom * InteractionLabelManager.LabelScale;
			RectangleF textDrawRect = labelData.GetScreenDrawRect(cam);
			RectangleF backgroundRect = textDrawRect;
			textDrawRect.Inflate(-4f * scale, -4f * scale);
			Vector2 textDrawPosScreen = new Vector2(textDrawRect.X, textDrawRect.Y);
			GUIStyle.InteractionLabelBackground.Draw(spriteBatch, backgroundRect, color * 0.7f, SpriteEffects.None);
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			string sanitizedValue = labelData.Text.SanitizedValue;
			Vector2 position = textDrawPosScreen;
			float rotation = 0f;
			Vector2 zero = Vector2.Zero;
			float scale2 = scale;
			SpriteEffects spriteEffects = SpriteEffects.None;
			float layerDepth = 0f;
			ImmutableArray<RichTextData>? richTextData = labelData.Text.RichTextData;
			subHeadingFont.DrawStringWithColors(spriteBatch, sanitizedValue, position, color, rotation, zero, scale2, spriteEffects, layerDepth, richTextData, 0, Alignment.TopLeft, ForceUpperCase.No);
		}

		// Token: 0x04000419 RID: 1049
		private static readonly List<InteractionLabelManager.LabelData> labels = new List<InteractionLabelManager.LabelData>();

		// Token: 0x0400041A RID: 1050
		private const int TextBoxMarginPx = 4;

		// Token: 0x0400041B RID: 1051
		private static InteractionLabelDisplayMode displayMode;

		// Token: 0x0400041C RID: 1052
		private static int graphicsWidth;

		// Token: 0x0400041D RID: 1053
		private static int graphicsHeight;

		// Token: 0x0400041E RID: 1054
		private static bool shouldRecalculate;

		// Token: 0x0400041F RID: 1055
		private static bool recalculateEverything;

		// Token: 0x04000420 RID: 1056
		private static readonly List<Item> interactablesInRange = new List<Item>();

		// Token: 0x02000707 RID: 1799
		[Nullable(0)]
		private class LabelData
		{
			// Token: 0x170019D5 RID: 6613
			// (get) Token: 0x0600677A RID: 26490 RVA: 0x0034A590 File Offset: 0x00348790
			// (set) Token: 0x0600677B RID: 26491 RVA: 0x0034A598 File Offset: 0x00348798
			public RectangleF TextRect { get; set; }

			// Token: 0x0600677C RID: 26492 RVA: 0x0034A5A1 File Offset: 0x003487A1
			public LabelData(Item item, RectangleF textRect, RichString text, Camera drawCamera)
			{
				this.Item = item;
				this.Text = text;
				this.TextRect = textRect;
				this.OriginalItemPosition = item.Position;
				this.drawCamera = drawCamera;
			}

			// Token: 0x0600677D RID: 26493 RVA: 0x0034A5D4 File Offset: 0x003487D4
			public RectangleF GetScreenDrawRect(Camera cam)
			{
				float scale = cam.Zoom;
				RectangleF screenDrawRect = this.TextRect;
				Camera camera = this.drawCamera;
				Vector2 location = screenDrawRect.Location;
				Submarine submarine = this.Item.Submarine;
				screenDrawRect.Location = camera.WorldToScreen(location + ((submarine != null) ? submarine.DrawPosition : Vector2.Zero));
				return new RectangleF(screenDrawRect.X, screenDrawRect.Y, screenDrawRect.Width * scale, screenDrawRect.Height * scale);
			}

			// Token: 0x0600677E RID: 26494 RVA: 0x0034A649 File Offset: 0x00348849
			public Vector2 GetInteractableDrawPositionScreen()
			{
				return this.drawCamera.WorldToScreen(this.Item.DrawPosition);
			}

			// Token: 0x04003883 RID: 14467
			private readonly Camera drawCamera;

			// Token: 0x04003884 RID: 14468
			public readonly Item Item;

			// Token: 0x04003886 RID: 14470
			public RichString Text;

			// Token: 0x04003887 RID: 14471
			public readonly Vector2 OriginalItemPosition;

			// Token: 0x04003888 RID: 14472
			public bool OverlapPreventionDone;
		}
	}
}
