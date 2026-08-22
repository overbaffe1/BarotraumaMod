using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000111 RID: 273
	[NullableContext(2)]
	[Nullable(0)]
	internal class EventEditorNodeConnection
	{
		// Token: 0x170009EE RID: 2542
		// (get) Token: 0x060024E9 RID: 9449 RVA: 0x00174F0F File Offset: 0x0017310F
		[Nullable(1)]
		public string Attribute { [NullableContext(1)] get; }

		// Token: 0x170009EF RID: 2543
		// (get) Token: 0x060024EA RID: 9450 RVA: 0x00174F17 File Offset: 0x00173117
		// (set) Token: 0x060024EB RID: 9451 RVA: 0x00174F1F File Offset: 0x0017311F
		public int ID { get; set; }

		// Token: 0x170009F0 RID: 2544
		// (get) Token: 0x060024EC RID: 9452 RVA: 0x00174F28 File Offset: 0x00173128
		// (set) Token: 0x060024ED RID: 9453 RVA: 0x00174F30 File Offset: 0x00173130
		public bool EndConversation { get; set; }

		// Token: 0x170009F1 RID: 2545
		// (get) Token: 0x060024EE RID: 9454 RVA: 0x00174F39 File Offset: 0x00173139
		// (set) Token: 0x060024EF RID: 9455 RVA: 0x00174F44 File Offset: 0x00173144
		public string OptionText
		{
			get
			{
				return this.optionText;
			}
			set
			{
				this.optionText = value;
				if (value != null)
				{
					this.actualValue = (this.WrappedValue = TextManager.Get(value).Fallback(value, true).Value);
					return;
				}
				this.WrappedValue = value;
				this.actualValue = value;
			}
		}

		// Token: 0x170009F2 RID: 2546
		// (get) Token: 0x060024F0 RID: 9456 RVA: 0x00174F92 File Offset: 0x00173192
		[Nullable(1)]
		public NodeConnectionType Type { [NullableContext(1)] get; }

		// Token: 0x170009F3 RID: 2547
		// (get) Token: 0x060024F1 RID: 9457 RVA: 0x00174F9A File Offset: 0x0017319A
		public Type ValueType { get; }

		// Token: 0x170009F4 RID: 2548
		// (get) Token: 0x060024F2 RID: 9458 RVA: 0x00174FA2 File Offset: 0x001731A2
		// (set) Token: 0x060024F3 RID: 9459 RVA: 0x00174FAC File Offset: 0x001731AC
		public object OverrideValue
		{
			get
			{
				return this.overrideValue;
			}
			set
			{
				this.overrideValue = value;
				string str = value as string;
				if (str != null)
				{
					this.actualValue = (this.WrappedValue = TextManager.Get(str).Fallback(str, true).Value);
					return;
				}
				this.actualValue = (this.WrappedValue = (((value != null) ? value.ToString() : null) ?? string.Empty));
			}
		}

		// Token: 0x170009F5 RID: 2549
		// (get) Token: 0x060024F4 RID: 9460 RVA: 0x00175015 File Offset: 0x00173215
		// (set) Token: 0x060024F5 RID: 9461 RVA: 0x00175020 File Offset: 0x00173220
		private string WrappedValue
		{
			get
			{
				return this.wrappedValue;
			}
			set
			{
				string valueText = value ?? string.Empty;
				if (string.IsNullOrWhiteSpace(valueText))
				{
					this.wrappedValue = null;
					return;
				}
				Vector2 textSize = GUIStyle.SmallFont.MeasureString(valueText, false);
				bool wasWrapped = false;
				while (textSize.X > 96f)
				{
					wasWrapped = true;
					valueText = (valueText + "...").Substring(0, valueText.Length - 4);
					textSize = GUIStyle.SmallFont.MeasureString(valueText + "...", false);
				}
				if (wasWrapped)
				{
					valueText = valueText.TrimEnd(' ') + "...";
				}
				this.wrappedValue = valueText;
			}
		}

		// Token: 0x170009F6 RID: 2550
		// (get) Token: 0x060024F6 RID: 9462 RVA: 0x001750C0 File Offset: 0x001732C0
		public PropertyInfo PropertyInfo { get; }

		// Token: 0x060024F7 RID: 9463 RVA: 0x001750C8 File Offset: 0x001732C8
		public object GetValue()
		{
			if (this.OverrideValue != null)
			{
				return this.OverrideValue;
			}
			foreach (EditorNode editorNode in EventEditorScreen.nodeList)
			{
				EventEditorNodeConnection outNode = editorNode.Connections.Find((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Out);
				if (outNode != null && outNode.ConnectedTo.Contains(this))
				{
					ValueNode valueNode = outNode.Parent as ValueNode;
					return (valueNode != null) ? valueNode.Value : null;
				}
			}
			return null;
		}

		// Token: 0x060024F8 RID: 9464 RVA: 0x0017517C File Offset: 0x0017337C
		public void ClearConnections()
		{
			EventEditorNodeConnection connection;
			using (IEnumerator<EventEditorNodeConnection> enumerator = EventEditorScreen.nodeList.SelectMany((EditorNode editorNode) => from connection in editorNode.Connections
			where connection.ConnectedTo.Contains(this)
			select connection).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					connection = enumerator.Current;
					connection.ConnectedTo.Remove(this);
				}
			}
			this.ConnectedTo.Clear();
		}

		// Token: 0x060024F9 RID: 9465 RVA: 0x001751EC File Offset: 0x001733EC
		[NullableContext(1)]
		public EventEditorNodeConnection(EditorNode parent, NodeConnectionType type, string attribute = "", [Nullable(2)] Type valueType = null, [Nullable(2)] PropertyInfo propertyInfo = null)
		{
			this.Type = type;
			this.ValueType = valueType;
			this.Attribute = attribute;
			this.PropertyInfo = propertyInfo;
			this.Parent = parent;
			this.ID = parent.Connections.Count;
		}

		// Token: 0x060024FA RID: 9466 RVA: 0x00175278 File Offset: 0x00173478
		private Point GetRenderPos(Rectangle parentRectangle, int yOffset)
		{
			int x = (this.Type.NodeSide == NodeConnectionType.Side.Left) ? (parentRectangle.Left - 15) : (parentRectangle.Right - 1);
			return new Point(x, parentRectangle.Y + 8 + parentRectangle.Height / 8 * yOffset);
		}

		// Token: 0x060024FB RID: 9467 RVA: 0x001752C4 File Offset: 0x001734C4
		[NullableContext(1)]
		public void Draw(SpriteBatch spriteBatch, Rectangle parentRectangle, int yOffset)
		{
			EventEditorScreen eventEditor = Screen.Selected as EventEditorScreen;
			float camZoom = (eventEditor != null) ? eventEditor.Cam.Zoom : 1f;
			Point pos = this.GetRenderPos(parentRectangle, yOffset);
			this.DrawRectangle = new Rectangle(pos, new Point(16, 16));
			GUI.DrawRectangle(spriteBatch, this.DrawRectangle, this.bgColor, true, 0f, 1f);
			GUI.DrawRectangle(spriteBatch, this.DrawRectangle, this.EndConversation ? GUIStyle.Red : this.outlineColor, false, 0f, (float)((int)Math.Max(1f, 1.25f / camZoom)));
			string label = string.IsNullOrWhiteSpace(this.Attribute) ? this.Type.Label : this.Attribute;
			float xPos = (parentRectangle.Center.X > pos.X) ? 24f : (-8f - GUIStyle.SmallFont.MeasureString(label, false).X);
			if (this.Type != NodeConnectionType.Out)
			{
				Vector2 size = GUIStyle.SmallFont.MeasureString(label, false);
				Vector2 positon = new Vector2((float)pos.X + xPos, (float)pos.Y);
				Rectangle bgRect = new Rectangle(positon.ToPoint(), size.ToPoint());
				bgRect.Inflate(4, 4);
				GUI.DrawRectangle(spriteBatch, bgRect, Color.Black * 0.6f, true, 0f, 1f);
				Vector2 pos2 = positon;
				string text = label;
				Color propertyColor = EventEditorNodeConnection.GetPropertyColor(this.ValueType);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch, pos2, text, propertyColor, null, 0, smallFont, ForceUpperCase.Inherit);
				Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
				mousePos.Y = -mousePos.Y;
				if (bgRect.Contains(mousePos))
				{
					PropertyInfo propertyInfo = this.PropertyInfo;
					CustomAttributeData attribute = (propertyInfo != null) ? propertyInfo.CustomAttributes.FirstOrDefault<CustomAttributeData>() : null;
					if (((attribute != null) ? attribute.AttributeType : null) == typeof(Serialize) && attribute.ConstructorArguments.Count > 2)
					{
						string description = attribute.ConstructorArguments[2].Value as string;
						if (!string.IsNullOrWhiteSpace(description))
						{
							EventEditorScreen.DrawnTooltip = description;
						}
					}
				}
			}
			if (this.OverrideValue != null)
			{
				Vector2 pos3 = new Vector2((float)(this.DrawRectangle.Center.X - 96), (float)(pos.Y + this.DrawRectangle.Height / 2 - 10));
				string text2 = this.WrappedValue ?? "null";
				object obj = this.actualValue;
				this.DrawLabel(spriteBatch, pos3, text2, ((obj != null) ? obj.ToString() : null) ?? string.Empty);
			}
			if (this.OptionText != null)
			{
				Vector2 pos4 = new Vector2((float)this.DrawRectangle.Center.X, (float)(pos.Y + this.DrawRectangle.Height / 2 - 10));
				string text3 = this.WrappedValue ?? "null";
				object obj2 = this.actualValue;
				this.DrawLabel(spriteBatch, pos4, text3, ((obj2 != null) ? obj2.ToString() : null) ?? string.Empty);
			}
			if (this.Parent.IsHighlighted)
			{
				this.DrawConnections(spriteBatch, yOffset, Math.Max(8f, 8f / camZoom), new Color?(Color.Red));
			}
			this.DrawConnections(spriteBatch, yOffset, Math.Max(2f, 2f / camZoom), null);
			if (EventEditorScreen.DraggedConnection == this)
			{
				this.DrawSquareLine(spriteBatch, EventEditorScreen.DraggingPosition, yOffset, Math.Max(2f, 2f / camZoom), null);
			}
		}

		// Token: 0x060024FC RID: 9468 RVA: 0x00175664 File Offset: 0x00173864
		[NullableContext(1)]
		private void DrawConnections(SpriteBatch spriteBatch, int yOffset, float width = 2f, Color? overrideColor = null)
		{
			foreach (EventEditorNodeConnection eventNodeConnection in this.ConnectedTo)
			{
				if (eventNodeConnection != null)
				{
					this.DrawSquareLine(spriteBatch, new Vector2((float)(eventNodeConnection.DrawRectangle.Left + 1), (float)eventNodeConnection.DrawRectangle.Center.Y), yOffset, width, overrideColor);
				}
			}
		}

		// Token: 0x060024FD RID: 9469 RVA: 0x001756E4 File Offset: 0x001738E4
		[NullableContext(1)]
		private void DrawLabel(SpriteBatch spriteBatch, Vector2 pos, string text, string fullText)
		{
			EventEditorScreen eventEditor = Screen.Selected as EventEditorScreen;
			float camZoom = (eventEditor != null) ? eventEditor.Cam.Zoom : 1f;
			Rectangle valueRect = new Rectangle((int)pos.X, (int)pos.Y, 96, 20);
			Vector2 textSize = GUIStyle.SmallFont.MeasureString(text, false);
			Vector2 position = valueRect.Location.ToVector2() + valueRect.Size.ToVector2() / 2f - textSize / 2f;
			Rectangle drawRect = valueRect;
			drawRect.Inflate(4, 4);
			GUI.DrawRectangle(spriteBatch, drawRect, new Color(50, 50, 50), true, 0f, 1f);
			GUI.DrawRectangle(spriteBatch, drawRect, this.EndConversation ? GUIStyle.Red : this.outlineColor, false, 0f, (float)((int)Math.Max(1f, 1.25f / camZoom)));
			Vector2 pos2 = position;
			Color propertyColor = EventEditorNodeConnection.GetPropertyColor(this.ValueType);
			GUIFont smallFont = GUIStyle.SmallFont;
			GUI.DrawString(spriteBatch, pos2, text, propertyColor, null, 0, smallFont, ForceUpperCase.Inherit);
			this.DrawRectangle = Rectangle.Union(this.DrawRectangle, drawRect);
			if (!string.IsNullOrWhiteSpace(fullText))
			{
				Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
				mousePos.Y = -mousePos.Y;
				if (this.DrawRectangle.Contains(mousePos))
				{
					EventEditorScreen.DrawnTooltip = fullText;
				}
			}
		}

		// Token: 0x060024FE RID: 9470 RVA: 0x00175864 File Offset: 0x00173A64
		[NullableContext(1)]
		private void DrawSquareLine(SpriteBatch spriteBatch, Vector2 position, int yOffset, float width = 2f, Color? overrideColor = null)
		{
			float knobLength = (float)(24 * (yOffset + 1));
			Vector2 start = new Vector2((float)this.DrawRectangle.Right, (float)this.DrawRectangle.Center.Y);
			Vector2[] array;
			SquareLine.LineType lineType;
			ToolBox.GetSquareLineBetweenPoints(start, position, knobLength).Deconstruct(out array, out lineType);
			Vector2[] points = array;
			Color drawColor = (this.Parent is ValueNode) ? EventEditorNodeConnection.GetPropertyColor(this.ValueType) : GUIStyle.Red;
			if (overrideColor != null)
			{
				drawColor = overrideColor.Value;
			}
			GUI.DrawLine(spriteBatch, points[0], points[1], drawColor, 0f, (float)((int)width));
			GUI.DrawLine(spriteBatch, points[1], points[2], drawColor, 0f, (float)((int)width));
			GUI.DrawLine(spriteBatch, points[2], points[3], drawColor, 0f, (float)((int)width));
			GUI.DrawLine(spriteBatch, points[3], points[4], drawColor, 0f, (float)((int)width));
			GUI.DrawLine(spriteBatch, points[4], points[5], drawColor, 0f, (float)((int)width));
		}

		// Token: 0x060024FF RID: 9471 RVA: 0x00175984 File Offset: 0x00173B84
		public static Color GetPropertyColor(Type valueType)
		{
			Color color = EventEditorNodeConnection.defaultColor;
			if (valueType == typeof(bool))
			{
				color = EventEditorNodeConnection.pinkColor;
			}
			else if (valueType == typeof(string))
			{
				color = EventEditorNodeConnection.yellowColor;
			}
			else if (valueType == typeof(int) || valueType == typeof(float) || valueType == typeof(double))
			{
				color = EventEditorNodeConnection.purpleColor;
			}
			else if (valueType == null)
			{
				color = Color.White;
			}
			return color;
		}

		// Token: 0x06002500 RID: 9472 RVA: 0x00175A19 File Offset: 0x00173C19
		[NullableContext(1)]
		public bool CanConnect(EventEditorNodeConnection otherNode)
		{
			return otherNode.OverrideValue == null && (this.Type.AllowedConnections == null || this.Type.AllowedConnections.Contains(otherNode.Type));
		}

		// Token: 0x0400125C RID: 4700
		private string optionText;

		// Token: 0x0400125F RID: 4703
		private object overrideValue;

		// Token: 0x04001260 RID: 4704
		private object actualValue;

		// Token: 0x04001261 RID: 4705
		private string wrappedValue;

		// Token: 0x04001263 RID: 4707
		public Rectangle DrawRectangle = Rectangle.Empty;

		// Token: 0x04001264 RID: 4708
		[Nullable(1)]
		public readonly EditorNode Parent;

		// Token: 0x04001265 RID: 4709
		[Nullable(1)]
		public readonly List<EventEditorNodeConnection> ConnectedTo = new List<EventEditorNodeConnection>();

		// Token: 0x04001266 RID: 4710
		private readonly Color bgColor = Color.DarkGray * 0.8f;

		// Token: 0x04001267 RID: 4711
		private readonly Color outlineColor = Color.White * 0.8f;

		// Token: 0x04001268 RID: 4712
		private static readonly Color defaultColor = new Color(139, 233, 253);

		// Token: 0x04001269 RID: 4713
		private static readonly Color yellowColor = new Color(241, 250, 140);

		// Token: 0x0400126A RID: 4714
		private static readonly Color pinkColor = new Color(255, 121, 198);

		// Token: 0x0400126B RID: 4715
		private static readonly Color purpleColor = new Color(189, 147, 249);
	}
}
