using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D8 RID: 1496
	internal class Connection
	{
		// Token: 0x17001858 RID: 6232
		// (get) Token: 0x06006073 RID: 24691 RVA: 0x00322CD4 File Offset: 0x00320ED4
		// (set) Token: 0x06006074 RID: 24692 RVA: 0x00322CDC File Offset: 0x00320EDC
		public float FlashTimer { get; private set; }

		// Token: 0x17001859 RID: 6233
		// (get) Token: 0x06006075 RID: 24693 RVA: 0x00322CE5 File Offset: 0x00320EE5
		// (set) Token: 0x06006076 RID: 24694 RVA: 0x00322CEC File Offset: 0x00320EEC
		public static Wire DraggingConnected { get; private set; }

		// Token: 0x1700185A RID: 6234
		// (get) Token: 0x06006077 RID: 24695 RVA: 0x00322CF4 File Offset: 0x00320EF4
		private static float ConnectionSpriteSize
		{
			get
			{
				return 35f * GUI.Scale;
			}
		}

		// Token: 0x06006078 RID: 24696 RVA: 0x00322D04 File Offset: 0x00320F04
		public static void DrawConnections(SpriteBatch spriteBatch, ConnectionPanel panel, Rectangle dragArea, Character character, [TupleElementNames(new string[]
		{
			"tooltipPos",
			"text"
		})] out ValueTuple<Vector2, LocalizedString> tooltip)
		{
			Wire draggingConnected = Connection.DraggingConnected;
			bool? flag;
			if (draggingConnected == null)
			{
				flag = null;
			}
			else
			{
				Item item2 = draggingConnected.Item;
				flag = ((item2 != null) ? new bool?(item2.Removed) : null);
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault(true))
			{
				Connection.DraggingConnected = null;
			}
			Rectangle panelRect = panel.GuiFrame.Rect;
			int x = panelRect.X;
			int y = panelRect.Y;
			int width = panelRect.Width;
			int height = panelRect.Height;
			bool mouseInRect = panelRect.Contains(PlayerInput.MousePosition);
			int totalWireCount = 0;
			foreach (Connection c3 in panel.Connections)
			{
				totalWireCount += c3.Wires.Count;
			}
			Wire equippedWire = null;
			NetworkMember networkMember = GameMain.NetworkMember;
			bool allowRewiring = ((networkMember != null) ? networkMember.ServerSettings : null) == null || GameMain.NetworkMember.ServerSettings.AllowRewiring || panel.AlwaysAllowRewiring;
			if (allowRewiring && ((!panel.Locked && !panel.TemporarilyLocked) || Screen.Selected == GameMain.SubEditorScreen))
			{
				foreach (Item item in character.HeldItems)
				{
					Wire wireComponent = item.GetComponent<Wire>();
					if (wireComponent != null)
					{
						equippedWire = wireComponent;
						Connection connectedEnd = equippedWire.OtherConnection(null);
						if (((connectedEnd != null) ? connectedEnd.Item.Submarine : null) != null && panel.Item.Submarine != connectedEnd.Item.Submarine)
						{
							equippedWire = null;
						}
					}
				}
			}
			tooltip = new ValueTuple<Vector2, LocalizedString>(Vector2.Zero, string.Empty);
			for (int i = 0; i < 2; i++)
			{
				Vector2 rightPos = Connection.GetRightPos(x, y, width);
				Vector2 leftPos = Connection.GetLeftPos(x, y);
				Vector2 rightWirePos = new Vector2((float)(x + width) - 5f * GUI.Scale, (float)y + 30f * GUI.Scale);
				Vector2 leftWirePos = new Vector2((float)x + 5f * GUI.Scale, (float)y + 30f * GUI.Scale);
				int wireInterval = (height - (int)(20f * GUI.Scale)) / Math.Max(totalWireCount, 1);
				int connectorIntervalLeft = Connection.GetConnectorIntervalLeft(height, panel);
				int connectorIntervalRight = Connection.GetConnectorIntervalRight(height, panel);
				foreach (Connection c2 in from c in panel.Connections
				orderby c.DisplayOrder
				select c)
				{
					if (Connection.DraggingConnected != null && i == 1 && (Screen.Selected == GameMain.SubEditorScreen || (Connection.DraggingConnected.Connections[0] == null && Connection.DraggingConnected.Connections[1] == null) || (Connection.DraggingConnected.Connections.Contains(c2) && Connection.DraggingConnected.Connections.Contains(null))) && (c2.FindWireByItem(Connection.DraggingConnected.Item) != null || panel.DisconnectedWires.Contains(Connection.DraggingConnected)))
					{
						Inventory.DraggingItems.Clear();
						Inventory.DraggingItems.Add(Connection.DraggingConnected.Item);
					}
					Vector2 position = c2.IsOutput ? rightPos : leftPos;
					if (ConnectionPanel.ShouldDebugDrawWiring)
					{
						LocalizedString tooltipText;
						Connection.DrawConnectionDebugInfo(spriteBatch, c2, position, GUI.Scale, out tooltipText);
						if (!tooltipText.IsNullOrEmpty())
						{
							bool mouseOn = Vector2.DistanceSquared(position, PlayerInput.MousePosition) < MathUtils.Pow2(35f * GUI.Scale);
							if (mouseOn)
							{
								tooltip = new ValueTuple<Vector2, LocalizedString>(position, tooltipText);
							}
						}
					}
					if (c2.IsOutput)
					{
						if (i == 0)
						{
							c2.DrawConnection(spriteBatch, panel, rightPos, Connection.GetOutputLabelPosition(rightPos, panel, c2));
						}
						else
						{
							c2.DrawWires(spriteBatch, panel, rightPos, rightWirePos, mouseInRect, equippedWire, (float)wireInterval);
						}
						rightPos.Y += (float)connectorIntervalLeft;
						rightWirePos.Y += (float)(c2.Wires.Count * wireInterval);
					}
					else
					{
						if (i == 0)
						{
							c2.DrawConnection(spriteBatch, panel, leftPos, Connection.GetInputLabelPosition(leftPos, panel, c2));
						}
						else
						{
							c2.DrawWires(spriteBatch, panel, leftPos, leftWirePos, mouseInRect, equippedWire, (float)wireInterval);
						}
						leftPos.Y += (float)connectorIntervalRight;
						leftWirePos.Y += (float)(c2.Wires.Count * wireInterval);
					}
				}
			}
			if (Connection.DraggingConnected != null)
			{
				if (mouseInRect)
				{
					Vector2 wireDragPos = new Vector2(MathHelper.Clamp(PlayerInput.MousePosition.X, (float)dragArea.X, (float)dragArea.Right), MathHelper.Clamp(PlayerInput.MousePosition.Y, (float)dragArea.Y, (float)dragArea.Bottom));
					Connection.DrawWire(spriteBatch, Connection.DraggingConnected, wireDragPos, new Vector2((float)(x + width / 2), (float)(y + height - 10)), null, panel, "");
				}
				panel.TriggerRewiringSound();
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					if (GameMain.NetworkMember != null || panel.CheckCharacterSuccess(character))
					{
						Connection connection = Connection.DraggingConnected.Connections[0];
						if (((connection != null) ? connection.ConnectionPanel : null) != panel)
						{
							Connection connection2 = Connection.DraggingConnected.Connections[1];
							if (((connection2 != null) ? connection2.ConnectionPanel : null) != panel)
							{
								goto IL_5C6;
							}
						}
						Connection.DraggingConnected.RemoveConnection(panel.Item);
						if (Connection.DraggingConnected.Item.ParentInventory == null)
						{
							panel.DisconnectedWires.Add(Connection.DraggingConnected);
						}
						else if (Connection.DraggingConnected.Connections[0] == null && Connection.DraggingConnected.Connections[1] == null)
						{
							Connection.DraggingConnected.ClearConnections(Character.Controlled);
						}
					}
					IL_5C6:
					if (GameMain.Client != null)
					{
						panel.Item.CreateClientEvent<ConnectionPanel>(panel);
					}
					Connection.DraggingConnected = null;
				}
			}
			if (equippedWire != null && (Connection.DraggingConnected != equippedWire || !mouseInRect) && panel.Connections.Find((Connection c) => c.Wires.Contains(equippedWire)) == null)
			{
				Connection.DrawWire(spriteBatch, equippedWire, new Vector2((float)(x + width / 2), (float)(y + height) - 150f * GUI.Scale), new Vector2((float)(x + width / 2), (float)(y + height)), null, panel, Connection.GetWireLabel(null, equippedWire));
				if (Connection.DraggingConnected == equippedWire)
				{
					Inventory.DraggingItems.Clear();
					Inventory.DraggingItems.Add(equippedWire.Item);
				}
			}
			float step = (float)width * 0.75f / (float)panel.DisconnectedWires.Count;
			x = (int)((float)(x + width / 2) - step * (float)(panel.DisconnectedWires.Count - 1) / 2f);
			foreach (Wire wire in panel.DisconnectedWires)
			{
				if ((wire != Connection.DraggingConnected || !mouseInRect) && (!wire.HiddenInGame || Screen.Selected != GameMain.GameScreen))
				{
					Connection.DrawWire(spriteBatch, wire, new Vector2((float)x, (float)(y + height) - 100f * GUI.Scale), new Vector2((float)x, (float)(y + height)), null, panel, Connection.GetWireLabel(null, wire));
					x += (int)step;
				}
			}
			if (!mouseInRect)
			{
				GUIComponent mouseOn2 = GUI.MouseOn;
				if (!(((mouseOn2 != null) ? mouseOn2.UserData : null) is ConnectionPanel) || !GUI.MouseOn.MouseRect.Contains(PlayerInput.MousePosition))
				{
					return;
				}
			}
			Inventory.DraggingItems.Clear();
		}

		// Token: 0x06006079 RID: 24697 RVA: 0x0032350C File Offset: 0x0032170C
		public static void DrawConnectionDebugInfo(SpriteBatch spriteBatch, Connection c, Vector2 position, float scale, out LocalizedString tooltip)
		{
			Color highlightColor = Color.Transparent;
			if (c.IsPower)
			{
				highlightColor = Connection.<DrawConnectionDebugInfo>g__VisualizeSignal|18_0(0.0, highlightColor, Color.Red);
			}
			else
			{
				highlightColor = Connection.<DrawConnectionDebugInfo>g__VisualizeSignal|18_0(c.LastReceivedSignal.TimeSinceCreated, highlightColor, Color.LightGreen);
				highlightColor = Connection.<DrawConnectionDebugInfo>g__VisualizeSignal|18_0(c.LastSentSignal.TimeSinceCreated, highlightColor, Color.Orange);
			}
			LocalizedString toolTipText = c.GetToolTip();
			if (!toolTipText.IsNullOrEmpty())
			{
				Sprite glowSprite = GUIStyle.UIGlowCircular.Value.Sprite;
				glowSprite.Draw(spriteBatch, position, highlightColor, glowSprite.size / 2f, 0f, 45f / glowSprite.size.X * scale, SpriteEffects.None, null);
			}
			tooltip = toolTipText;
		}

		// Token: 0x0600607A RID: 24698 RVA: 0x003235D4 File Offset: 0x003217D4
		private void DrawConnection(SpriteBatch spriteBatch, ConnectionPanel panel, Vector2 position, Vector2 labelPos)
		{
			string text = this.DisplayName.Value.ToUpperInvariant();
			GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("ConnectionPanelLabel");
			UISprite labelSprite = (componentStyle != null) ? componentStyle.Sprites.Values.First<List<UISprite>>().First<UISprite>() : null;
			if (labelSprite != null)
			{
				Rectangle labelArea = Connection.GetLabelArea(labelPos, text);
				labelSprite.Draw(spriteBatch, labelArea, this.IsPower ? GUIStyle.Red : Color.SteelBlue, SpriteEffects.None, null);
			}
			Vector2 pos = labelPos + Vector2.UnitY;
			string text2 = text;
			Color color = Color.Black * 0.8f;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUI.DrawString(spriteBatch, pos, text2, color, null, 0, smallFont, ForceUpperCase.Inherit);
			string text3 = text;
			Color color2 = GUIStyle.TextColorBright;
			smallFont = GUIStyle.SmallFont;
			GUI.DrawString(spriteBatch, labelPos, text3, color2, null, 0, smallFont, ForceUpperCase.Inherit);
			float connectorSpriteScale = Connection.ConnectionSpriteSize / (float)Connection.connectionSprite.SourceRect.Width;
			Connection.connectionSprite.Draw(spriteBatch, position, 0f, connectorSpriteScale, SpriteEffects.None);
		}

		// Token: 0x0600607B RID: 24699 RVA: 0x003236DC File Offset: 0x003218DC
		private void DrawWires(SpriteBatch spriteBatch, ConnectionPanel panel, Vector2 position, Vector2 wirePosition, bool mouseIn, Wire equippedWire, float wireInterval)
		{
			float connectorSpriteScale = Connection.ConnectionSpriteSize / (float)Connection.connectionSprite.SourceRect.Width;
			foreach (Wire wire in this.wires)
			{
				if (!wire.Hidden && (Connection.DraggingConnected != wire || (!mouseIn && Screen.Selected != GameMain.SubEditorScreen)) && (!wire.HiddenInGame || Screen.Selected != GameMain.GameScreen))
				{
					Connection.DrawWire(spriteBatch, wire, position, wirePosition, equippedWire, panel, Connection.GetWireLabel(this, wire));
					wirePosition.Y += wireInterval;
				}
			}
			bool isMouseOn = Vector2.Distance(position, PlayerInput.MousePosition) < 20f * GUI.Scale;
			if (isMouseOn)
			{
				Connection.connectionSpriteHighlight.Draw(spriteBatch, position, 0f, connectorSpriteScale, SpriteEffects.None);
			}
			if (Connection.DraggingConnected != null && isMouseOn && !PlayerInput.PrimaryMouseButtonHeld())
			{
				if ((GameMain.NetworkMember != null || panel.CheckCharacterSuccess(Character.Controlled)) && this.Wires.Count < this.MaxPlayerConnectableWires && this.WireSlotsAvailable() && !this.Wires.Contains(Connection.DraggingConnected))
				{
					bool alreadyConnected = Connection.DraggingConnected.IsConnectedTo(panel.Item);
					Connection.DraggingConnected.RemoveConnection(panel.Item);
					if (Connection.DraggingConnected.TryConnect(this, !alreadyConnected, true))
					{
						Connection otherConnection = Connection.DraggingConnected.OtherConnection(this);
						this.ConnectWire(Connection.DraggingConnected);
					}
				}
				if (GameMain.Client != null)
				{
					panel.Item.CreateClientEvent<ConnectionPanel>(panel);
				}
				Connection.DraggingConnected = null;
			}
			if (this.FlashTimer > 0f)
			{
				int flashCycleCount = (int)Math.Max(this.flashDuration, 1f);
				float flashCycleDuration = this.flashDuration / (float)flashCycleCount;
				Connection.connectionSpriteHighlight.Draw(spriteBatch, position, this.flashColor * (float)Math.Sin((double)(this.FlashTimer % flashCycleDuration / flashCycleDuration * 3.1415927f * 0.8f)), 0f, connectorSpriteScale, SpriteEffects.None, null);
			}
			if (this.Wires.Any((Wire w) => w != Connection.DraggingConnected && !w.Hidden && (!w.HiddenInGame || Screen.Selected != GameMain.GameScreen)))
			{
				int screwIndex = (int)Math.Floor((double)(position.Y / 30f)) % Connection.screwSprites.Count;
				Connection.screwSprites[screwIndex].Draw(spriteBatch, position, 0f, connectorSpriteScale, SpriteEffects.None);
			}
		}

		// Token: 0x0600607C RID: 24700 RVA: 0x0032395C File Offset: 0x00321B5C
		private static LocalizedString GetWireLabel(Connection connection, Wire wire)
		{
			Connection recipient = wire.OtherConnection(connection);
			LocalizedString label;
			if (wire.Item.IsLayerHidden)
			{
				label = TextManager.Get("ConnectionLocked");
			}
			else
			{
				string value;
				if (recipient != null)
				{
					string name = recipient.item.Name;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(recipient.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					value = name + defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					value = "";
				}
				label = value;
				if (wire.Locked)
				{
					label += "\n" + TextManager.Get("ConnectionLocked");
				}
			}
			return label;
		}

		// Token: 0x0600607D RID: 24701 RVA: 0x00323A09 File Offset: 0x00321C09
		public void Flash(Color? color = null, float flashDuration = 1.5f)
		{
			this.FlashTimer = flashDuration;
			this.flashDuration = flashDuration;
			this.flashColor = ((color == null) ? GUIStyle.Red : color.Value);
		}

		// Token: 0x0600607E RID: 24702 RVA: 0x00323A3B File Offset: 0x00321C3B
		public void UpdateFlashTimer(float deltaTime)
		{
			if (this.FlashTimer <= 0f)
			{
				return;
			}
			this.FlashTimer -= deltaTime;
		}

		// Token: 0x0600607F RID: 24703 RVA: 0x00323A5C File Offset: 0x00321C5C
		private LocalizedString GetToolTip()
		{
			if (this.LastReceivedSignal.TimeSinceCreated < 1.0)
			{
				return this.<GetToolTip>g__getSignalTooltip|26_0(this.LastReceivedSignal, "receivedsignal");
			}
			if (this.LastSentSignal.TimeSinceCreated < 1.0)
			{
				return this.<GetToolTip>g__getSignalTooltip|26_0(this.LastSentSignal, "sentsignal");
			}
			if (this.IsPower)
			{
				Powered powered = this.item.GetComponent<Powered>();
				if (powered != null)
				{
					if (this.IsOutput)
					{
						if (powered.CurrPowerConsumption < 0f)
						{
							return this.<GetToolTip>g__getPowerTooltip|26_1(-(int)powered.CurrPowerConsumption, "reactoroutput");
						}
						if (powered is PowerTransfer || powered is PowerContainer)
						{
							GridInfo grid = this.Grid;
							return this.<GetToolTip>g__getPowerTooltip|26_1((int)((grid != null) ? grid.Power : 0f), "reactoroutput");
						}
					}
					else if (!this.IsOutput)
					{
						float powerConsumption = powered.GetCurrentPowerConsumption(this);
						if (!MathUtils.NearlyEqual((float)((int)powerConsumption), 0f, 0.0001f))
						{
							return this.<GetToolTip>g__getPowerTooltip|26_1((int)powerConsumption, "reactorload");
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06006080 RID: 24704 RVA: 0x00323B6C File Offset: 0x00321D6C
		private static void DrawWire(SpriteBatch spriteBatch, Wire wire, Vector2 end, Vector2 start, Wire equippedWire, ConnectionPanel panel, LocalizedString label)
		{
			int textX = (int)start.X;
			if (start.X < end.X)
			{
				textX -= 10;
			}
			else
			{
				textX += 10;
			}
			bool canDrag = equippedWire == null || equippedWire == wire;
			float alpha = canDrag ? 1f : 0.5f;
			bool mouseOn = canDrag && !(GUI.MouseOn is GUIDragHandle) && ((PlayerInput.MousePosition.X > Math.Min(start.X, end.X) && PlayerInput.MousePosition.X < Math.Max(start.X, end.X) && MathUtils.LineToPointDistanceSquared(start, end, PlayerInput.MousePosition) < 36f) || Vector2.Distance(end, PlayerInput.MousePosition) < 20f || new Rectangle((start.X < end.X) ? (textX - 100) : textX, (int)start.Y - 5, 100, 14).Contains(PlayerInput.MousePosition));
			if (!label.IsNullOrEmpty())
			{
				if (start.Y > (float)panel.GuiFrame.Rect.Bottom - 1f)
				{
					GUIStyle.Font.DrawString(spriteBatch, label, start + Vector2.UnitY * 20f * GUI.Scale, Color.White, 45f, Vector2.Zero, 1f, SpriteEffects.None, 0f, Alignment.TopLeft);
				}
				else
				{
					GUI.DrawString(spriteBatch, new Vector2((start.X < end.X) ? ((float)textX - GUIStyle.SmallFont.MeasureString(label, false).X) : ((float)textX), start.Y - 5f), label, wire.Locked ? GUIStyle.TextColorDim : (mouseOn ? Wire.higlightColor : GUIStyle.TextColorNormal), new Color?(Color.Black * 0.9f), 3, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				}
			}
			Vector2 wireEnd = end + Vector2.Normalize(start - end) * 30f * GUI.Scale;
			float dist = Vector2.Distance(start, wireEnd);
			float wireWidth = 12f * GUI.Scale;
			float highlight = 5f * GUI.Scale;
			if (mouseOn)
			{
				spriteBatch.Draw(Connection.wireVertical.Texture, new Rectangle(wireEnd.ToPoint(), new Point((int)(wireWidth + highlight), (int)dist)), new Rectangle?(Connection.wireVertical.SourceRect), Wire.higlightColor, MathUtils.VectorToAngle(end - start) + 1.5707964f, new Vector2(Connection.wireVertical.size.X / 2f, 0f), SpriteEffects.None, 0f);
			}
			spriteBatch.Draw(Connection.wireVertical.Texture, new Rectangle(wireEnd.ToPoint(), new Point((int)wireWidth, (int)dist)), new Rectangle?(Connection.wireVertical.SourceRect), wire.Item.Color * alpha, MathUtils.VectorToAngle(end - start) + 1.5707964f, new Vector2(Connection.wireVertical.size.X / 2f, 0f), SpriteEffects.None, 0f);
			float connectorScale = wireWidth / (float)Connection.wireVertical.SourceRect.Width;
			Connection.connector.Draw(spriteBatch, end, Color.White, Connection.connector.Origin, MathUtils.VectorToAngle(end - start) + 1.5707964f, connectorScale, SpriteEffects.None, null);
			if (Connection.DraggingConnected == null && canDrag && mouseOn)
			{
				ConnectionPanel.HighlightedWire = wire;
				NetworkMember networkMember = GameMain.NetworkMember;
				bool allowRewiring = ((networkMember != null) ? networkMember.ServerSettings : null) == null || GameMain.NetworkMember.ServerSettings.AllowRewiring || panel.AlwaysAllowRewiring;
				if (allowRewiring && ((!wire.Locked && !wire.Item.IsLayerHidden && !panel.Locked && !panel.TemporarilyLocked) || Screen.Selected == GameMain.SubEditorScreen) && PlayerInput.PrimaryMouseButtonHeld())
				{
					Connection.DraggingConnected = wire;
				}
			}
		}

		// Token: 0x06006081 RID: 24705 RVA: 0x00323F84 File Offset: 0x00322184
		public static bool CheckConnectionLabelOverlap(ConnectionPanel panel, out Point newRectSize)
		{
			Rectangle panelRect = panel.GuiFrame.Rect;
			int x = panelRect.X;
			int y = panelRect.Y;
			Vector2 rightPos = Connection.GetRightPos(x, y, panelRect.Width);
			Vector2 leftPos = Connection.GetLeftPos(x, y);
			int connectorIntervalLeft = Connection.GetConnectorIntervalLeft(panelRect.Height, panel);
			int connectorIntervalRight = Connection.GetConnectorIntervalRight(panelRect.Height, panel);
			newRectSize = panelRect.Size;
			float rightMostInput = (float)panelRect.Center.X;
			float leftMostOutput = (float)panelRect.Center.X;
			foreach (Connection c in panel.Connections)
			{
				if (c.IsOutput)
				{
					Rectangle labelArea = Connection.GetLabelArea(Connection.GetOutputLabelPosition(rightPos, panel, c), c.DisplayName.Value.ToUpperInvariant());
					leftMostOutput = Math.Min(leftMostOutput, (float)labelArea.X);
					rightPos.Y += (float)connectorIntervalLeft;
				}
				else
				{
					rightMostInput = Math.Max(rightMostInput, (float)Connection.GetLabelArea(Connection.GetInputLabelPosition(leftPos, panel, c), c.DisplayName.Value.ToUpperInvariant()).Right);
					leftPos.Y += (float)connectorIntervalRight;
				}
			}
			if (leftMostOutput < rightMostInput)
			{
				newRectSize += new Point((int)(rightMostInput - leftMostOutput) + GUI.IntScale(15f), 0);
			}
			while ((float)Connection.GetConnectorIntervalLeft(newRectSize.Y, panel) < Connection.ConnectionSpriteSize || (float)Connection.GetConnectorIntervalRight(newRectSize.Y, panel) < Connection.ConnectionSpriteSize)
			{
				newRectSize.Y += 10;
			}
			return newRectSize.X != panel.GuiFrame.Rect.Width || newRectSize.Y > panel.GuiFrame.Rect.Height;
		}

		// Token: 0x06006082 RID: 24706 RVA: 0x00324174 File Offset: 0x00322374
		private static Vector2 GetInputLabelPosition(Vector2 connectorPosition, ConnectionPanel panel, Connection connection)
		{
			return new Vector2(connectorPosition.X + 25f * GUI.Scale, connectorPosition.Y - GUIStyle.SmallFont.MeasureString(connection.DisplayName.ToUpper(), false).Y / 2f);
		}

		// Token: 0x06006083 RID: 24707 RVA: 0x003241C0 File Offset: 0x003223C0
		private static Vector2 GetOutputLabelPosition(Vector2 connectorPosition, ConnectionPanel panel, Connection connection)
		{
			return new Vector2(connectorPosition.X - 25f * GUI.Scale - GUIStyle.SmallFont.MeasureString(connection.DisplayName.ToUpper(), false).X, connectorPosition.Y - GUIStyle.SmallFont.MeasureString(connection.DisplayName.ToUpper(), false).Y / 2f);
		}

		// Token: 0x06006084 RID: 24708 RVA: 0x00324228 File Offset: 0x00322428
		private static Rectangle GetLabelArea(Vector2 labelPos, string text)
		{
			Vector2 textSize = GUIStyle.SmallFont.MeasureString(text, false);
			Rectangle labelArea = new Rectangle(labelPos.ToPoint(), textSize.ToPoint());
			labelArea.Inflate(GUI.IntScale(10f), GUI.IntScale(3f));
			return labelArea;
		}

		// Token: 0x06006085 RID: 24709 RVA: 0x00324278 File Offset: 0x00322478
		private static Vector2 GetLeftPos(int x, int y)
		{
			return new Vector2((float)x + 80f * GUI.Scale, (float)y + 60f * GUI.Scale);
		}

		// Token: 0x06006086 RID: 24710 RVA: 0x0032429B File Offset: 0x0032249B
		private static Vector2 GetRightPos(int x, int y, int width)
		{
			return new Vector2((float)(x + width) - 80f * GUI.Scale, (float)y + 60f * GUI.Scale);
		}

		// Token: 0x06006087 RID: 24711 RVA: 0x003242C0 File Offset: 0x003224C0
		private static int GetConnectorIntervalLeft(int height, ConnectionPanel panel)
		{
			return (height - GUI.IntScale(60f)) / Math.Max(panel.Connections.Count((Connection c) => c.IsOutput), 1);
		}

		// Token: 0x06006088 RID: 24712 RVA: 0x003242FF File Offset: 0x003224FF
		private static int GetConnectorIntervalRight(int height, ConnectionPanel panel)
		{
			return (height - GUI.IntScale(60f)) / Math.Max(panel.Connections.Count((Connection c) => !c.IsOutput), 1);
		}

		// Token: 0x1700185B RID: 6235
		// (get) Token: 0x06006089 RID: 24713 RVA: 0x0032433E File Offset: 0x0032253E
		// (set) Token: 0x0600608A RID: 24714 RVA: 0x00324350 File Offset: 0x00322550
		public LocalizedString DisplayName
		{
			get
			{
				return this.DisplayNameOverride ?? this._displayName;
			}
			private set
			{
				this._displayName = value;
			}
		}

		// Token: 0x1700185C RID: 6236
		// (get) Token: 0x0600608B RID: 24715 RVA: 0x00324359 File Offset: 0x00322559
		public LocalizedString DefaultDisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		// Token: 0x1700185D RID: 6237
		// (get) Token: 0x0600608C RID: 24716 RVA: 0x00324361 File Offset: 0x00322561
		public IReadOnlyCollection<Wire> Wires
		{
			get
			{
				return this.wires;
			}
		}

		// Token: 0x1700185E RID: 6238
		// (get) Token: 0x0600608D RID: 24717 RVA: 0x00324369 File Offset: 0x00322569
		// (set) Token: 0x0600608E RID: 24718 RVA: 0x00324371 File Offset: 0x00322571
		public Signal LastSentSignal { get; private set; }

		// Token: 0x1700185F RID: 6239
		// (get) Token: 0x0600608F RID: 24719 RVA: 0x0032437A File Offset: 0x0032257A
		// (set) Token: 0x06006090 RID: 24720 RVA: 0x00324382 File Offset: 0x00322582
		public Signal LastReceivedSignal { get; private set; }

		// Token: 0x17001860 RID: 6240
		// (get) Token: 0x06006091 RID: 24721 RVA: 0x0032438B File Offset: 0x0032258B
		// (set) Token: 0x06006092 RID: 24722 RVA: 0x00324393 File Offset: 0x00322593
		public bool IsPower { get; private set; }

		// Token: 0x17001861 RID: 6241
		// (get) Token: 0x06006093 RID: 24723 RVA: 0x0032439C File Offset: 0x0032259C
		public List<Connection> Recipients
		{
			get
			{
				if (this.recipientsDirty)
				{
					this.RefreshRecipients();
				}
				return this.recipients;
			}
		}

		// Token: 0x17001862 RID: 6242
		// (get) Token: 0x06006094 RID: 24724 RVA: 0x003243B2 File Offset: 0x003225B2
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x17001863 RID: 6243
		// (get) Token: 0x06006095 RID: 24725 RVA: 0x003243BA File Offset: 0x003225BA
		// (set) Token: 0x06006096 RID: 24726 RVA: 0x003243C2 File Offset: 0x003225C2
		public ConnectionPanel ConnectionPanel { get; private set; }

		// Token: 0x06006097 RID: 24727 RVA: 0x003243CB File Offset: 0x003225CB
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"Connection (",
				this.item.Name,
				", ",
				this.Name,
				")"
			});
		}

		// Token: 0x06006098 RID: 24728 RVA: 0x00324408 File Offset: 0x00322608
		public Connection(ContentXElement element, int connectionIndex, ConnectionPanel connectionPanel, IdRemap idRemap, bool isItemSwap)
		{
			if (Connection.connector == null)
			{
				Connection.connector = GUIStyle.GetComponentStyle("ConnectionPanelConnector").GetDefaultSprite();
				Connection.wireVertical = GUIStyle.GetComponentStyle("ConnectionPanelWire").GetDefaultSprite();
				Connection.connectionSprite = GUIStyle.GetComponentStyle("ConnectionPanelConnection").GetDefaultSprite();
				Connection.connectionSpriteHighlight = GUIStyle.GetComponentStyle("ConnectionPanelConnection").GetSprite(GUIComponent.ComponentState.Hover);
				Connection.screwSprites = (from s in GUIStyle.GetComponentStyle("ConnectionPanelScrew").Sprites[GUIComponent.ComponentState.None]
				select s.Sprite).ToList<Sprite>();
			}
			this.ConnectionPanel = connectionPanel;
			this.item = connectionPanel.Item;
			this.MaxWires = element.GetAttributeInt("maxwires", 5);
			this.MaxWires = Math.Max(element.Elements().Count((ContentXElement e) => e.Name.ToString().Equals("link", StringComparison.OrdinalIgnoreCase)), this.MaxWires);
			this.MaxPlayerConnectableWires = element.GetAttributeInt("maxplayerconnectablewires", this.MaxWires);
			this.wires = new HashSet<Wire>();
			this.IsOutput = (element.Name.ToString() == "output");
			this.Name = element.GetAttributeString("name", this.IsOutput ? "output" : "input");
			XAttribute displayOrderAttr = element.GetAttribute("displayorderoverride");
			int displayOrder;
			if (displayOrderAttr == null)
			{
				IEnumerable<Connection> sameElements = from c in connectionPanel.Connections
				where c.IsOutput == this.IsOutput
				select c;
				int num;
				if (sameElements.Any<Connection>())
				{
					num = sameElements.Max((Connection c) => c.DisplayOrder) + 1;
				}
				else
				{
					num = 0;
				}
				displayOrder = num;
			}
			else
			{
				displayOrder = displayOrderAttr.GetAttributeInt(0);
			}
			this.DisplayOrder = displayOrder;
			string displayNameTag = "";
			string fallbackTag = "";
			if (element.GetAttribute("displayname") == null)
			{
				using (IEnumerator<ContentXElement> enumerator = this.item.Prefab.ConfigElement.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement subElement = enumerator.Current;
						if (subElement.Name.ToString().Equals("connectionpanel", StringComparison.OrdinalIgnoreCase))
						{
							int prefabConnectionIndex = 0;
							foreach (ContentXElement cxe in subElement.Elements())
							{
								XElement connectionElement = cxe;
								string prefabConnectionName = connectionElement.GetAttributeString("name", null);
								if (!prefabConnectionName.IsNullOrEmpty())
								{
									string[] aliases = connectionElement.GetAttributeStringArray("aliases", Array.Empty<string>(), true, false);
									if (prefabConnectionName == this.Name || aliases.Contains(this.Name) || (isItemSwap && connectionIndex == prefabConnectionIndex))
									{
										displayNameTag = connectionElement.GetAttributeString("displayname", "");
										fallbackTag = connectionElement.GetAttributeString("fallbackdisplayname", "");
									}
									prefabConnectionIndex++;
								}
							}
						}
					}
					goto IL_370;
				}
			}
			displayNameTag = element.GetAttributeString("displayname", "");
			fallbackTag = element.GetAttributeString("fallbackdisplayname", null);
			IL_370:
			if (!string.IsNullOrEmpty(displayNameTag))
			{
				string text;
				if (displayNameTag == null)
				{
					text = null;
				}
				else
				{
					string[] array = displayNameTag.Split('~', StringSplitOptions.None);
					text = ((array != null) ? array.FirstOrDefault<string>() : null);
				}
				string tagWithoutVariables = text;
				string text2;
				if (fallbackTag == null)
				{
					text2 = null;
				}
				else
				{
					string[] array2 = fallbackTag.Split('~', StringSplitOptions.None);
					text2 = ((array2 != null) ? array2.FirstOrDefault<string>() : null);
				}
				string fallbackTagWithoutVariables = text2;
				if (TextManager.ContainsTag(tagWithoutVariables))
				{
					this.DisplayName = TextManager.GetServerMessage(displayNameTag);
				}
				else if (TextManager.ContainsTag(fallbackTagWithoutVariables))
				{
					this.DisplayName = TextManager.GetServerMessage(fallbackTag);
				}
			}
			if (this.DisplayName.IsNullOrEmpty())
			{
				this.DisplayName = this.Name;
			}
			string name = this.Name;
			bool def = name == "power_in" || name == "power" || name == "power_out";
			this.IsPower = element.GetAttributeBool("ispower", def);
			this.LoadedWires = new List<ValueTuple<ushort, int?>>();
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string a = subElement2.Name.ToString().ToLowerInvariant();
				if (!(a == "link"))
				{
					if (a == "statuseffect")
					{
						if (this.Effects == null)
						{
							this.Effects = new List<StatusEffect>();
						}
						this.Effects.Add(StatusEffect.Load(subElement2, this.item.Name + ", connection " + this.Name));
					}
				}
				else
				{
					int id = subElement2.GetAttributeInt("w", 0);
					int? i = null;
					if (subElement2.GetAttribute("i") != null)
					{
						i = new int?(subElement2.GetAttributeInt("i", 0));
					}
					if (id < 0)
					{
						id = 0;
					}
					if (this.LoadedWires.Count < this.MaxWires)
					{
						this.LoadedWires.Add(new ValueTuple<ushort, int?>(idRemap.GetOffsetId(id), i));
					}
				}
			}
		}

		// Token: 0x06006099 RID: 24729 RVA: 0x003249D4 File Offset: 0x00322BD4
		public bool IsConnectedToSomething()
		{
			return this.wires.Count > 0 || this.CircuitBoxConnections.Count > 0;
		}

		// Token: 0x0600609A RID: 24730 RVA: 0x003249F4 File Offset: 0x00322BF4
		public void SetRecipientsDirty()
		{
			this.recipientsDirty = true;
			if (this.IsPower)
			{
				Powered.ChangedConnections.Add(this);
			}
		}

		// Token: 0x0600609B RID: 24731 RVA: 0x00324A14 File Offset: 0x00322C14
		private void RefreshRecipients()
		{
			this.recipients.Clear();
			foreach (Wire wire in this.wires)
			{
				Connection recipient = wire.OtherConnection(this);
				if (recipient != null)
				{
					this.recipients.Add(recipient);
				}
			}
			this.recipientsDirty = false;
		}

		// Token: 0x0600609C RID: 24732 RVA: 0x00324A8C File Offset: 0x00322C8C
		public Wire FindWireByItem(Item it)
		{
			return this.Wires.FirstOrDefault((Wire w) => w.Item == it);
		}

		// Token: 0x0600609D RID: 24733 RVA: 0x00324ABD File Offset: 0x00322CBD
		public bool WireSlotsAvailable()
		{
			return this.wires.Count < this.MaxWires;
		}

		// Token: 0x0600609E RID: 24734 RVA: 0x00324AD2 File Offset: 0x00322CD2
		public bool TryAddLink(Wire wire)
		{
			if (wire == null || this.wires.Contains(wire) || !this.WireSlotsAvailable())
			{
				return false;
			}
			this.wires.Add(wire);
			return true;
		}

		// Token: 0x0600609F RID: 24735 RVA: 0x00324B00 File Offset: 0x00322D00
		public void DisconnectWire(Wire wire)
		{
			if (wire == null || !this.wires.Contains(wire))
			{
				return;
			}
			Connection prevOtherConnection = wire.OtherConnection(this);
			if (prevOtherConnection != null)
			{
				if (this.IsPower && prevOtherConnection.IsPower && this.Grid != null)
				{
					if (prevOtherConnection.recipients.Count > 1 && this.recipients.Count > 1)
					{
						Powered.ChangedConnections.Add(prevOtherConnection);
						Powered.ChangedConnections.Add(this);
					}
					else if (this.recipients.Count > 1)
					{
						GridInfo grid = prevOtherConnection.Grid;
						if (grid != null)
						{
							grid.RemoveConnection(prevOtherConnection);
						}
						prevOtherConnection.Grid = null;
					}
					else if (prevOtherConnection.recipients.Count > 1)
					{
						GridInfo grid2 = this.Grid;
						if (grid2 != null)
						{
							grid2.RemoveConnection(this);
						}
						this.Grid = null;
					}
					else if (this.Grid.Connections.Count == 2)
					{
						Powered.Grids.Remove(this.Grid.ID);
						this.Grid = null;
						prevOtherConnection.Grid = null;
					}
				}
				prevOtherConnection.recipientsDirty = true;
			}
			if (this.enumeratingWires)
			{
				this.removedWires.Add(wire);
			}
			else
			{
				this.wires.Remove(wire);
			}
			this.recipientsDirty = true;
		}

		// Token: 0x060060A0 RID: 24736 RVA: 0x00324C44 File Offset: 0x00322E44
		public void ConnectWire(Wire wire)
		{
			if (wire == null || !this.TryAddLink(wire))
			{
				return;
			}
			this.ConnectionPanel.DisconnectedWires.Remove(wire);
			Connection otherConnection = wire.OtherConnection(this);
			if (otherConnection != null)
			{
				if (Powered.ValidPowerConnection(this, otherConnection))
				{
					if (this.Grid == null && otherConnection.Grid != null)
					{
						otherConnection.Grid.AddConnection(this);
						this.Grid = otherConnection.Grid;
					}
					else if (this.Grid != null && otherConnection.Grid == null)
					{
						this.Grid.AddConnection(otherConnection);
						otherConnection.Grid = this.Grid;
					}
					else
					{
						Powered.ChangedConnections.Add(this);
						Powered.ChangedConnections.Add(otherConnection);
					}
				}
				otherConnection.recipientsDirty = true;
			}
			this.recipientsDirty = true;
		}

		// Token: 0x060060A1 RID: 24737 RVA: 0x00324D00 File Offset: 0x00322F00
		public void SendSignal(Signal signal)
		{
			this.LastSentSignal = signal;
			this.enumeratingWires = true;
			foreach (Wire wire in this.wires)
			{
				Connection recipient = wire.OtherConnection(this);
				if (recipient != null && recipient.item != this.item)
				{
					Item source = signal.source;
					if (((source != null) ? source.LastSentSignalRecipients.LastOrDefault<Connection>() : null) != recipient)
					{
						Item source2 = signal.source;
						if (source2 != null)
						{
							source2.LastSentSignalRecipients.Add(recipient);
						}
						wire.RegisterSignal(signal, this);
						Connection.SendSignalIntoConnection(signal, recipient);
					}
				}
			}
			foreach (CircuitBoxConnection connection in this.CircuitBoxConnections)
			{
				connection.ReceiveSignal(signal);
			}
			this.enumeratingWires = false;
			foreach (Wire removedWire in this.removedWires)
			{
				this.wires.Remove(removedWire);
			}
			this.removedWires.Clear();
		}

		// Token: 0x060060A2 RID: 24738 RVA: 0x00324E54 File Offset: 0x00323054
		public static void SendSignalIntoConnection(Signal signal, Connection conn)
		{
			conn.LastReceivedSignal = signal;
			foreach (ItemComponent ic in conn.item.Components)
			{
				ic.ReceiveSignal(signal, conn);
			}
			if (conn.Effects == null || signal.value == "0")
			{
				return;
			}
			foreach (StatusEffect effect in conn.Effects)
			{
				conn.Item.ApplyStatusEffect(effect, ActionType.OnUse, 0.016666668f, null, null, null, false, true, null);
			}
		}

		// Token: 0x060060A3 RID: 24739 RVA: 0x00324F2C File Offset: 0x0032312C
		public void ClearConnections()
		{
			if (this.IsPower && this.Grid != null)
			{
				Powered.ChangedConnections.Add(this);
				foreach (Connection c in this.recipients)
				{
					Powered.ChangedConnections.Add(c);
				}
			}
			foreach (Wire wire in this.wires)
			{
				wire.RemoveConnection(this);
				this.recipientsDirty = true;
			}
			if (this.enumeratingWires)
			{
				using (HashSet<Wire>.Enumerator enumerator3 = this.wires.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Wire wire2 = enumerator3.Current;
						this.removedWires.Add(wire2);
					}
					return;
				}
			}
			this.wires.Clear();
		}

		// Token: 0x060060A4 RID: 24740 RVA: 0x00325048 File Offset: 0x00323248
		public void InitializeFromLoaded()
		{
			if (this.LoadedWires.Count == 0)
			{
				return;
			}
			foreach (ValueTuple<ushort, int?> valueTuple in this.LoadedWires)
			{
				ushort wireId = valueTuple.Item1;
				int? connectionIndex = valueTuple.Item2;
				Item wireItem = Entity.FindEntityByID(wireId) as Item;
				if (wireItem != null)
				{
					Wire wire = wireItem.GetComponent<Wire>();
					if (wire != null && this.TryAddLink(wire))
					{
						if (wire.Item.body != null)
						{
							wire.Item.body.Enabled = false;
						}
						if (connectionIndex != null)
						{
							wire.Connect(this, connectionIndex.Value, false, false);
						}
						else
						{
							wire.TryConnect(this, false, false);
						}
						wire.FixNodeEnds();
						this.recipientsDirty = true;
					}
				}
			}
			this.LoadedWires.Clear();
		}

		// Token: 0x060060A5 RID: 24741 RVA: 0x0032513C File Offset: 0x0032333C
		public void Save(XElement parentElement)
		{
			XElement newElement = new XElement(this.IsOutput ? "output" : "input", new XAttribute("name", this.Name));
			foreach (Wire wire in from w in this.wires
			orderby w.Item.ID
			select w)
			{
				newElement.Add(new XElement("link", new object[]
				{
					new XAttribute("w", wire.Item.ID.ToString()),
					new XAttribute("i", (wire.Connections[0] != this) ? 1 : 0)
				}));
			}
			parentElement.Add(newElement);
		}

		// Token: 0x060060A6 RID: 24742 RVA: 0x00325244 File Offset: 0x00323444
		[CompilerGenerated]
		internal static Color <DrawConnectionDebugInfo>g__VisualizeSignal|18_0(double timeSinceCreated, Color defaultColor, Color color)
		{
			if (timeSinceCreated < 1.0)
			{
				float pulseAmount = (MathF.Sin((float)Timing.TotalTimeUnpaused * 10f) + 3f) / 4f;
				Color targetColor = Color.Lerp(defaultColor, color, pulseAmount);
				return Color.Lerp(targetColor, defaultColor, (float)timeSinceCreated);
			}
			return defaultColor;
		}

		// Token: 0x060060A7 RID: 24743 RVA: 0x00325290 File Offset: 0x00323490
		[CompilerGenerated]
		private LocalizedString <GetToolTip>g__getSignalTooltip|26_0(Signal signal, string textTag)
		{
			if (this.lastSignalToolTip.Item1 == signal.value && !this.lastSignalToolTip.Item2.IsNullOrEmpty())
			{
				return this.lastSignalToolTip.Item2;
			}
			this.lastSignalToolTip = new ValueTuple<string, LocalizedString>(signal.value, TextManager.GetWithVariable(textTag, "[signal]", signal.value, FormatCapitals.No));
			return this.lastSignalToolTip.Item2;
		}

		// Token: 0x060060A8 RID: 24744 RVA: 0x00325308 File Offset: 0x00323508
		[CompilerGenerated]
		private LocalizedString <GetToolTip>g__getPowerTooltip|26_1(int powerValue, string textTag)
		{
			if (this.lastPowerToolTip.Item1 == powerValue && !this.lastPowerToolTip.Item2.IsNullOrEmpty())
			{
				return this.lastPowerToolTip.Item2;
			}
			this.lastPowerToolTip = new ValueTuple<int, LocalizedString>(powerValue, TextManager.GetWithVariable(textTag, "[kw]", powerValue.ToString(), FormatCapitals.No));
			return this.lastPowerToolTip.Item2;
		}

		// Token: 0x040031CC RID: 12748
		private static Sprite connector;

		// Token: 0x040031CD RID: 12749
		private static Sprite wireVertical;

		// Token: 0x040031CE RID: 12750
		private static Sprite connectionSprite;

		// Token: 0x040031CF RID: 12751
		private static Sprite connectionSpriteHighlight;

		// Token: 0x040031D0 RID: 12752
		private static List<Sprite> screwSprites;

		// Token: 0x040031D1 RID: 12753
		private Color flashColor;

		// Token: 0x040031D2 RID: 12754
		private float flashDuration = 1.5f;

		// Token: 0x040031D5 RID: 12757
		[TupleElementNames(new string[]
		{
			"signal",
			"tooltip"
		})]
		private ValueTuple<string, LocalizedString> lastSignalToolTip;

		// Token: 0x040031D6 RID: 12758
		[TupleElementNames(new string[]
		{
			"powerValue",
			"tooltip"
		})]
		private ValueTuple<int, LocalizedString> lastPowerToolTip;

		// Token: 0x040031D7 RID: 12759
		private const int DefaultMaxWires = 5;

		// Token: 0x040031D8 RID: 12760
		public readonly int MaxPlayerConnectableWires = 5;

		// Token: 0x040031D9 RID: 12761
		public readonly int MaxWires = 5;

		// Token: 0x040031DA RID: 12762
		public readonly int DisplayOrder;

		// Token: 0x040031DB RID: 12763
		public readonly string Name;

		// Token: 0x040031DC RID: 12764
		private readonly LocalizedString _displayName;

		// Token: 0x040031DD RID: 12765
		public LocalizedString DisplayNameOverride;

		// Token: 0x040031DE RID: 12766
		private readonly HashSet<Wire> wires;

		// Token: 0x040031DF RID: 12767
		public List<CircuitBoxConnection> CircuitBoxConnections = new List<CircuitBoxConnection>();

		// Token: 0x040031E0 RID: 12768
		private bool enumeratingWires;

		// Token: 0x040031E1 RID: 12769
		private readonly HashSet<Wire> removedWires = new HashSet<Wire>();

		// Token: 0x040031E2 RID: 12770
		private readonly Item item;

		// Token: 0x040031E3 RID: 12771
		public readonly bool IsOutput;

		// Token: 0x040031E4 RID: 12772
		public readonly List<StatusEffect> Effects;

		// Token: 0x040031E5 RID: 12773
		[TupleElementNames(new string[]
		{
			"wireId",
			"connectionIndex"
		})]
		public readonly List<ValueTuple<ushort, int?>> LoadedWires;

		// Token: 0x040031E6 RID: 12774
		public GridInfo Grid;

		// Token: 0x040031E7 RID: 12775
		public PowerPriority Priority;

		// Token: 0x040031EB RID: 12779
		private bool recipientsDirty = true;

		// Token: 0x040031EC RID: 12780
		private readonly List<Connection> recipients = new List<Connection>();
	}
}
