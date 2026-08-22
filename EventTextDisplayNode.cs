using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200010D RID: 269
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class EventTextDisplayNode : EventNode
	{
		// Token: 0x060024D4 RID: 9428 RVA: 0x00174122 File Offset: 0x00172322
		protected EventTextDisplayNode(Type type, string name) : base(type, name)
		{
		}

		// Token: 0x170009E7 RID: 2535
		// (get) Token: 0x060024D5 RID: 9429 RVA: 0x0017412C File Offset: 0x0017232C
		protected virtual bool ShowOptions
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170009E8 RID: 2536
		// (get) Token: 0x060024D6 RID: 9430 RVA: 0x00174130 File Offset: 0x00172330
		private new Rectangle HeaderRectangle
		{
			get
			{
				if (!EventEditorScreen.ConversationMode)
				{
					return base.HeaderRectangle;
				}
				Rectangle drawRect = this.GetDrawRectangle();
				return new Rectangle(base.Position.ToPoint(), new Point(drawRect.Width, 32));
			}
		}

		// Token: 0x060024D7 RID: 9431 RVA: 0x00174174 File Offset: 0x00172374
		public override Rectangle GetDrawRectangle()
		{
			if (!EventEditorScreen.ConversationMode)
			{
				return base.GetDrawRectangle();
			}
			EventEditorNodeConnection textConnection = this.Connections.Find((EventEditorNodeConnection c) => string.Equals(c.Attribute, "Text", StringComparison.OrdinalIgnoreCase));
			IEnumerable<EventEditorNodeConnection> enumerable2;
			if (!this.ShowOptions)
			{
				IEnumerable<EventEditorNodeConnection> enumerable = Enumerable.Empty<EventEditorNodeConnection>();
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = from c in this.Connections
				where c.Type == NodeConnectionType.Option
				select c;
			}
			IEnumerable<EventEditorNodeConnection> optionConnections = enumerable2;
			int height = 50;
			if (textConnection != null)
			{
				string textContent = this.GetTextContent(textConnection);
				if (!string.IsNullOrEmpty(textContent) && GUIStyle.Font.Value != null)
				{
					string wrappedText = ToolBox.WrapText(textContent, 284f, GUIStyle.Font.Value, 1f);
					Vector2 textSize = GUIStyle.Font.MeasureString(wrappedText, false);
					height += (int)textSize.Y + 10;
				}
				else
				{
					height += 25;
				}
			}
			if (this.ShowOptions)
			{
				int optionIndex = 0;
				foreach (EventEditorNodeConnection option in optionConnections)
				{
					string optionText = EventTextDisplayNode.GetOptionText(option, optionIndex);
					if (GUIStyle.Font.Value != null)
					{
						string wrappedOption = ToolBox.WrapText(optionText, 260f, GUIStyle.Font.Value, 1f);
						Vector2 optionSize = GUIStyle.Font.MeasureString(wrappedOption, false);
						height += (int)optionSize.Y + 20;
					}
					else
					{
						height += 40;
					}
					optionIndex++;
				}
			}
			Rectangle rect = base.Rectangle;
			return new Rectangle(rect.X, rect.Y, 300, height);
		}

		// Token: 0x060024D8 RID: 9432 RVA: 0x0017432C File Offset: 0x0017252C
		protected override void DrawBack(SpriteBatch spriteBatch)
		{
			if (!EventEditorScreen.ConversationMode)
			{
				base.DrawBack(spriteBatch);
				return;
			}
			Rectangle bodyRect = this.GetDrawRectangle();
			Color headerColor = this.IsSelected ? new Color(100, 150, 200) : new Color(120, 170, 220);
			Color bodyColor = new Color(90, 120, 150);
			Color borderColor = Color.LightBlue;
			GUI.DrawRectangle(spriteBatch, this.HeaderRectangle, headerColor, true, 1f, 1f);
			GUI.DrawRectangle(spriteBatch, bodyRect, bodyColor, true, 1f, 1f);
			GUI.DrawRectangle(spriteBatch, this.HeaderRectangle, borderColor, false, 1f, 1f);
			GUI.DrawRectangle(spriteBatch, bodyRect, borderColor, false, 1f, 1f);
			Vector2 headerSize = GUIStyle.SubHeadingFont.MeasureString(base.Name, false);
			Vector2 headerPos = this.HeaderRectangle.Location.ToVector2() + this.HeaderRectangle.Size.ToVector2() / 2f - headerSize / 2f;
			GUIStyle.SubHeadingFont.DrawString(spriteBatch, base.Name, headerPos, Color.White, ForceUpperCase.Inherit, false);
			this.DrawTextContent(spriteBatch, bodyRect);
			this.DrawConnections(spriteBatch);
		}

		// Token: 0x060024D9 RID: 9433 RVA: 0x0017447C File Offset: 0x0017267C
		protected virtual void DrawTextContent(SpriteBatch spriteBatch, Rectangle bodyRect)
		{
			EventEditorNodeConnection textConnection = this.Connections.Find((EventEditorNodeConnection c) => string.Equals(c.Attribute, "Text", StringComparison.OrdinalIgnoreCase));
			IEnumerable<EventEditorNodeConnection> enumerable2;
			if (!this.ShowOptions)
			{
				IEnumerable<EventEditorNodeConnection> enumerable = Enumerable.Empty<EventEditorNodeConnection>();
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = from c in this.Connections
				where c.Type == NodeConnectionType.Option
				select c;
			}
			IEnumerable<EventEditorNodeConnection> optionConnections = enumerable2;
			Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
			mousePos.Y = -mousePos.Y;
			int currentY = bodyRect.Y + 8 + 30;
			if (textConnection != null)
			{
				string textContent = this.GetTextContent(textConnection);
				string wrappedText = textContent;
				int textHeight = 25;
				if (GUIStyle.Font.Value != null)
				{
					wrappedText = ToolBox.WrapText(textContent, (float)(bodyRect.Width - 24), GUIStyle.Font.Value, 1f);
					Vector2 textSize = GUIStyle.Font.MeasureString(wrappedText, false);
					textHeight = (int)textSize.Y + 10;
				}
				Rectangle textRect = new Rectangle(bodyRect.X + 8, currentY, bodyRect.Width - 16, textHeight);
				GUI.DrawRectangle(spriteBatch, textRect, new Color(70, 100, 130), true, 0f, 1f);
				GUI.DrawRectangle(spriteBatch, textRect, Color.CornflowerBlue, false, 0f, 1f);
				Vector2 textPos = new Vector2((float)(textRect.X + 4), (float)(textRect.Y + 4));
				Vector2 pos = textPos;
				string text = wrappedText;
				Color yellow = Color.Yellow;
				GUIFont font = GUIStyle.Font;
				GUI.DrawString(spriteBatch, pos, text, yellow, null, 0, font, ForceUpperCase.Inherit);
				if (textRect.Contains(mousePos))
				{
					string rawTextKey = this.GetRawTextKey(textConnection);
					if (!string.IsNullOrEmpty(rawTextKey))
					{
						EventEditorScreen.DrawnTooltip = rawTextKey;
					}
				}
				currentY += textHeight + 5;
			}
			if (this.ShowOptions)
			{
				EventTextDisplayNode.DrawOptions(spriteBatch, bodyRect, optionConnections, currentY);
			}
		}

		// Token: 0x060024DA RID: 9434 RVA: 0x00174658 File Offset: 0x00172858
		protected static void DrawOptions(SpriteBatch spriteBatch, Rectangle bodyRect, IEnumerable<EventEditorNodeConnection> optionConnections, int startY)
		{
			Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
			mousePos.Y = -mousePos.Y;
			int currentY = startY;
			int optionIndex = 0;
			foreach (EventEditorNodeConnection option in optionConnections)
			{
				string optionText = EventTextDisplayNode.GetOptionText(option, optionIndex);
				string wrappedOption = optionText;
				int optionHeight = 30;
				if (GUIStyle.Font.Value != null)
				{
					wrappedOption = ToolBox.WrapText(optionText, (float)(bodyRect.Width - 40), GUIStyle.Font.Value, 1f);
					Vector2 optionSize = GUIStyle.Font.MeasureString(wrappedOption, false);
					optionHeight = (int)optionSize.Y + 16;
				}
				Rectangle optionRect = new Rectangle(bodyRect.X + 8, currentY, bodyRect.Width - 16, optionHeight);
				Color optionBg = option.EndConversation ? new Color(120, 80, 80) : new Color(80, 80, 120);
				GUI.DrawRectangle(spriteBatch, optionRect, optionBg, true, 0f, 1f);
				GUI.DrawRectangle(spriteBatch, optionRect, Color.White, false, 0f, 1f);
				Vector2 optionPos = new Vector2((float)(optionRect.X + 4), (float)(optionRect.Y + 4));
				Vector2 pos = optionPos;
				string text = wrappedOption;
				Color white = Color.White;
				GUIFont font = GUIStyle.Font;
				GUI.DrawString(spriteBatch, pos, text, white, null, 0, font, ForceUpperCase.Inherit);
				if (optionRect.Contains(mousePos))
				{
					string rawOptionKey = option.OptionText ?? "";
					if (!string.IsNullOrEmpty(rawOptionKey))
					{
						EventEditorScreen.DrawnTooltip = rawOptionKey;
					}
				}
				Rectangle connRect = new Rectangle(bodyRect.Right - 1, optionRect.Y + optionHeight / 2 - 8, 16, 16);
				GUI.DrawRectangle(spriteBatch, connRect, Color.DarkGray, true, 0f, 1f);
				GUI.DrawRectangle(spriteBatch, connRect, Color.White, false, 0f, 1f);
				option.DrawRectangle = connRect;
				foreach (EventEditorNodeConnection connected in option.ConnectedTo)
				{
					Vector2 start = new Vector2((float)connRect.Right, (float)connRect.Center.Y);
					Vector2 end = new Vector2((float)connected.DrawRectangle.Left, (float)connected.DrawRectangle.Center.Y);
					float knobLength = 24f;
					Vector2[] array;
					SquareLine.LineType lineType;
					ToolBox.GetSquareLineBetweenPoints(start, end, knobLength).Deconstruct(out array, out lineType);
					Vector2[] points = array;
					Color lineColor = GUIStyle.Red;
					float val = 2f;
					float num = 2f;
					EventEditorScreen eventEditor = Screen.Selected as EventEditorScreen;
					float lineWidth = Math.Max(val, num / ((eventEditor != null) ? eventEditor.Cam.Zoom : 1f));
					GUI.DrawLine(spriteBatch, points[0], points[1], lineColor, 0f, (float)((int)lineWidth));
					GUI.DrawLine(spriteBatch, points[1], points[2], lineColor, 0f, (float)((int)lineWidth));
					GUI.DrawLine(spriteBatch, points[2], points[3], lineColor, 0f, (float)((int)lineWidth));
					GUI.DrawLine(spriteBatch, points[3], points[4], lineColor, 0f, (float)((int)lineWidth));
					GUI.DrawLine(spriteBatch, points[4], points[5], lineColor, 0f, (float)((int)lineWidth));
				}
				currentY += optionHeight + 5;
				optionIndex++;
			}
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x00174A0C File Offset: 0x00172C0C
		private static string GetOptionText(EventEditorNodeConnection option, int optionIndex)
		{
			string text;
			if ((text = option.OptionText) == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Option ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(optionIndex + 1);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string optionTextKey = text;
			IEnumerable<string> allVariants = TextManager.GetAll(optionTextKey);
			int variantCount = allVariants.Count<string>();
			string result;
			if (variantCount <= 1)
			{
				if (variantCount != 1)
				{
					result = optionTextKey;
				}
				else
				{
					result = allVariants.First<string>();
				}
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(variantCount);
				defaultInterpolatedStringHandler2.AppendLiteral(" variants] ");
				defaultInterpolatedStringHandler2.AppendFormatted(string.Join(" / ", allVariants));
				result = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			return result;
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x00174AB8 File Offset: 0x00172CB8
		private string GetTextContent(EventEditorNodeConnection textConnection)
		{
			string textContent = "";
			if (textConnection.OverrideValue != null)
			{
				textContent = (textConnection.OverrideValue.ToString() ?? "");
			}
			else
			{
				object connectedValue = textConnection.GetValue();
				if (connectedValue != null)
				{
					textContent = (connectedValue.ToString() ?? "");
				}
			}
			if (string.IsNullOrEmpty(textContent))
			{
				EventEditorNodeConnection addConnection = this.Connections.FirstOrDefault((EventEditorNodeConnection c) => c.Type == NodeConnectionType.Add);
				if (addConnection != null && addConnection.ConnectedTo.Any<EventEditorNodeConnection>())
				{
					EventEditorNodeConnection connectedNode = addConnection.ConnectedTo.First<EventEditorNodeConnection>();
					EditorNode parent = connectedNode.Parent;
					if (((parent != null) ? parent.Name : null) == "Text")
					{
						EventEditorNodeConnection textNodeConnection = connectedNode.Parent.Connections.FirstOrDefault((EventEditorNodeConnection c) => string.Equals(c.Attribute, "tag", StringComparison.OrdinalIgnoreCase));
						if (textNodeConnection != null && textNodeConnection.OverrideValue != null)
						{
							textContent = (textNodeConnection.OverrideValue.ToString() ?? "");
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(textContent))
			{
				LocalizedString translated = TextManager.Get(textContent);
				if (translated.Loaded)
				{
					textContent = translated.Value;
				}
			}
			return textContent;
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x00174BF0 File Offset: 0x00172DF0
		private string GetRawTextKey(EventEditorNodeConnection textConnection)
		{
			string textKey = "";
			if (textConnection.OverrideValue != null)
			{
				textKey = (textConnection.OverrideValue.ToString() ?? "");
			}
			else
			{
				object connectedValue = textConnection.GetValue();
				if (connectedValue != null)
				{
					textKey = (connectedValue.ToString() ?? "");
				}
			}
			if (string.IsNullOrEmpty(textKey))
			{
				EventEditorNodeConnection addConnection = this.Connections.FirstOrDefault((EventEditorNodeConnection c) => c.Type == NodeConnectionType.Add);
				if (addConnection != null && addConnection.ConnectedTo.Any<EventEditorNodeConnection>())
				{
					EventEditorNodeConnection connectedNode = addConnection.ConnectedTo.First<EventEditorNodeConnection>();
					EditorNode parent = connectedNode.Parent;
					if (((parent != null) ? parent.Name : null) == "Text")
					{
						EventEditorNodeConnection textNodeConnection = connectedNode.Parent.Connections.FirstOrDefault((EventEditorNodeConnection c) => string.Equals(c.Attribute, "tag", StringComparison.OrdinalIgnoreCase));
						if (textNodeConnection != null && textNodeConnection.OverrideValue != null)
						{
							textKey = (textNodeConnection.OverrideValue.ToString() ?? "");
						}
					}
				}
			}
			return textKey;
		}

		// Token: 0x060024DE RID: 9438 RVA: 0x00174D04 File Offset: 0x00172F04
		protected override bool ShouldDrawConnection(EventEditorNodeConnection connection)
		{
			if (!EventEditorScreen.ConversationMode)
			{
				return base.ShouldDrawConnection(connection);
			}
			return connection.Type == NodeConnectionType.Activate || connection.Type == NodeConnectionType.Next;
		}

		// Token: 0x060024DF RID: 9439 RVA: 0x00174D34 File Offset: 0x00172F34
		protected override void DrawConnections(SpriteBatch spriteBatch)
		{
			if (!EventEditorScreen.ConversationMode)
			{
				base.DrawConnections(spriteBatch);
				return;
			}
			Rectangle correctRect = this.GetDrawRectangle();
			int x = 0;
			int y = 0;
			foreach (EventEditorNodeConnection connection in this.Connections)
			{
				if (this.ShouldDrawConnection(connection))
				{
					NodeConnectionType.Side nodeSide = connection.Type.NodeSide;
					if (nodeSide != NodeConnectionType.Side.Left)
					{
						if (nodeSide == NodeConnectionType.Side.Right)
						{
							connection.Draw(spriteBatch, correctRect, x);
							x++;
						}
					}
					else
					{
						connection.Draw(spriteBatch, correctRect, y);
						y++;
					}
				}
			}
		}
	}
}
