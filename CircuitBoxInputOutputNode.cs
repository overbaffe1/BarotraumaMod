using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000037 RID: 55
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxInputOutputNode : CircuitBoxNode
	{
		// Token: 0x06000901 RID: 2305 RVA: 0x00050FB0 File Offset: 0x0004F1B0
		[NullableContext(0)]
		public void PromptEdit(GUIComponent parent)
		{
			CircuitBoxUI ui = this.CircuitBox.UI;
			if (ui != null)
			{
				ui.SetMenuVisibility(false);
			}
			GUIFrame backgroundBlocker = new GUIFrame(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				UserData = "InputOutputEditPrompt"
			};
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 0.8f), backgroundBlocker.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			GUIFrame labelArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.8f), mainLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup labelLayout = new GUILayoutGroup(new RectTransform(Vector2.One, labelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			GUIListBox labelList = new GUIListBox(new RectTransform(ToolBox.PaddingSizeParentRelative(labelLayout.RectTransform, 0.9f), labelLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			Dictionary<string, GUITextBox> textBoxes = new Dictionary<string, GUITextBox>();
			foreach (CircuitBoxConnection conn in this.Connectors)
			{
				string labelOverride;
				bool found = this.ConnectionLabelOverrides.TryGetValue(conn.Name, out labelOverride);
				GUILayoutGroup connLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.12f), labelList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
				RectTransform rectT = new RectTransform(new Vector2(0.4f, 1f), connLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = conn.Connection.DisplayName;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
				GUITextBox box = GUI.CreateTextBoxWithPlaceholder(new RectTransform(new Vector2(0.6f, 1f), connLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), found ? labelOverride : string.Empty, conn.Connection.DefaultDisplayName.Value);
				box.MaxTextLength = new int?(32);
				textBoxes.Add(conn.Name, box);
			}
			new GUIButton(new RectTransform(new Vector2(0.5f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("confirm"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				Dictionary<string, string> newOverrides = textBoxes.ToDictionary((KeyValuePair<string, GUITextBox> pair) => pair.Key, (KeyValuePair<string, GUITextBox> pair) => pair.Value.Text);
				foreach (KeyValuePair<string, string> keyValuePair in newOverrides.ToImmutableDictionary<string, string>())
				{
					string text2;
					string text3;
					keyValuePair.Deconstruct(out text2, out text3);
					string key = text2;
					string value = text3;
					string newValue;
					if (this.ConnectionLabelOverrides.TryGetValue(key, out newValue))
					{
						if (newValue == value)
						{
							newOverrides.Remove(key);
						}
					}
					else if (string.IsNullOrWhiteSpace(value))
					{
						newOverrides.Remove(key);
					}
				}
				this.CircuitBox.SetConnectionLabelOverrides(this, newOverrides);
				this.RemoveEditPrompt(parent);
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				this.RemoveEditPrompt(parent);
				return true;
			};
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x000513C4 File Offset: 0x0004F5C4
		[NullableContext(0)]
		public void RemoveEditPrompt(GUIComponent parent)
		{
			GUIComponent promptParent = parent.FindChild("InputOutputEditPrompt", false);
			if (promptParent == null)
			{
				return;
			}
			parent.RemoveChild(promptParent);
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x000513E9 File Offset: 0x0004F5E9
		public CircuitBoxInputOutputNode(IReadOnlyList<CircuitBoxConnection> conns, Vector2 initialPosition, CircuitBoxInputOutputNode.Type type, CircuitBox circuitBox) : base(circuitBox)
		{
			this.InitSize(conns);
			this.Connectors = conns.ToImmutableArray<CircuitBoxConnection>();
			base.Position = initialPosition;
			this.NodeType = type;
			base.UpdatePositions();
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x00051428 File Offset: 0x0004F628
		public void ReplaceAllConnectionLabelOverrides(Dictionary<string, string> replace)
		{
			foreach (KeyValuePair<string, string> keyValuePair in replace)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text, out text2);
				string value = text2;
				if (value.Length > 32)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Label override value \"");
					defaultInterpolatedStringHandler.AppendFormatted(value);
					defaultInterpolatedStringHandler.AppendLiteral("\" is too long (max ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(32);
					defaultInterpolatedStringHandler.AppendLiteral(" characters)");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
			}
			foreach (KeyValuePair<string, string> keyValuePair in replace)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text2, out text);
				string name = text2;
				string value2 = text;
				if (string.IsNullOrWhiteSpace(value2))
				{
					this.ConnectionLabelOverrides.Remove(name);
				}
				else
				{
					this.ConnectionLabelOverrides[name] = value2;
				}
			}
			this.InitSize(this.Connectors);
			base.UpdatePositions();
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x00051564 File Offset: 0x0004F764
		private void InitSize(IReadOnlyList<CircuitBoxConnection> conns)
		{
			foreach (CircuitBoxConnection conn in conns)
			{
				string value;
				if (this.ConnectionLabelOverrides.TryGetValue(conn.Name, out value))
				{
					LocalizedString newLabel = string.IsNullOrWhiteSpace(value) ? conn.Connection.DisplayName : TextManager.Get(value).Fallback(value, true);
					conn.SetLabel(newLabel, this);
					conn.Connection.DisplayNameOverride = (string.IsNullOrWhiteSpace(value) ? null : newLabel);
				}
				else
				{
					conn.Connection.DisplayNameOverride = null;
					conn.SetLabel(conn.Connection.DisplayName, this);
				}
			}
			this.Size = CircuitBoxNode.CalculateSize(conns);
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x00051634 File Offset: 0x0004F834
		public XElement Save()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
			defaultInterpolatedStringHandler.AppendFormatted<CircuitBoxInputOutputNode.Type>(this.NodeType);
			defaultInterpolatedStringHandler.AppendLiteral("Node");
			XElement element = new XElement(defaultInterpolatedStringHandler.ToStringAndClear(), new XAttribute("pos", XMLExtensions.Vector2ToString(base.Position)));
			foreach (KeyValuePair<string, string> keyValuePair in this.ConnectionLabelOverrides)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text, out text2);
				string name = text;
				string value = text2;
				element.Add(new XElement("ConnectionLabelOverride", new object[]
				{
					new XAttribute("name", name),
					new XAttribute("value", value)
				}));
			}
			return element;
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x00051724 File Offset: 0x0004F924
		public void Load(ContentXElement element)
		{
			string key = "pos";
			Vector2 zero = Vector2.Zero;
			base.Position = element.GetAttributeVector2(key, zero);
			Dictionary<string, string> loadedOverrides = new Dictionary<string, string>();
			foreach (ContentXElement subElement in element.Elements())
			{
				if (!(subElement.Name != "ConnectionLabelOverride"))
				{
					string name = subElement.GetAttributeString("name", string.Empty);
					string value = subElement.GetAttributeString("value", string.Empty);
					loadedOverrides[name] = value;
				}
			}
			this.ConnectionLabelOverrides = loadedOverrides;
			this.InitSize(this.Connectors);
			base.UpdatePositions();
		}

		// Token: 0x040004A9 RID: 1193
		[Nullable(0)]
		private const string PromptUserData = "InputOutputEditPrompt";

		// Token: 0x040004AA RID: 1194
		public readonly CircuitBoxInputOutputNode.Type NodeType;

		// Token: 0x040004AB RID: 1195
		private const int MaxConnectionLabelLength = 32;

		// Token: 0x040004AC RID: 1196
		private const string ConnectionLabelOverrideElementName = "ConnectionLabelOverride";

		// Token: 0x040004AD RID: 1197
		public Dictionary<string, string> ConnectionLabelOverrides = new Dictionary<string, string>();

		// Token: 0x0200071E RID: 1822
		[NullableContext(0)]
		public enum Type
		{
			// Token: 0x040038C8 RID: 14536
			Invalid,
			// Token: 0x040038C9 RID: 14537
			Input,
			// Token: 0x040038CA RID: 14538
			Output
		}
	}
}
