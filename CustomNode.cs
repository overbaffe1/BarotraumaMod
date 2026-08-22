using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010C RID: 268
	[NullableContext(1)]
	[Nullable(0)]
	internal class CustomNode : SpecialNode
	{
		// Token: 0x060024CC RID: 9420 RVA: 0x00173CA4 File Offset: 0x00171EA4
		public CustomNode(string name) : base(name)
		{
			base.CanAddConnections = true;
			this.RemovableTypes.Add(NodeConnectionType.Value);
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Activate, "", null, null));
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Next, "", null, null));
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Add, "", null, null));
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x00173D26 File Offset: 0x00171F26
		public CustomNode() : this("Custom")
		{
			CustomNode.Prompt(delegate(string s)
			{
				base.Name = s;
				return true;
			});
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x00173D44 File Offset: 0x00171F44
		public override void AddOption()
		{
			CustomNode.Prompt(delegate(string s)
			{
				this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Value, s, typeof(string), null));
				return true;
			});
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x00173D58 File Offset: 0x00171F58
		public override XElement Save()
		{
			XElement newElement = new XElement("CustomNode");
			newElement.Add(new XAttribute("i", this.ID));
			newElement.Add(new XAttribute("name", base.Name));
			newElement.Add(new XAttribute("xpos", base.Position.X));
			newElement.Add(new XAttribute("ypos", base.Position.Y));
			foreach (EventEditorNodeConnection connection2 in this.Connections.FindAll((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Value))
			{
				newElement.Add(new XElement("Value", new XAttribute("name", connection2.Attribute)));
			}
			return newElement;
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x00173E88 File Offset: 0x00172088
		[return: Nullable(2)]
		public static EditorNode LoadCustomNode(XElement element)
		{
			if (!string.Equals(element.Name.ToString(), "CustomNode", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			CustomNode newNode = new CustomNode(element.GetAttributeString("name", string.Empty))
			{
				ID = element.GetAttributeInt("i", -1)
			};
			float posX = element.GetAttributeFloat("xpos", 0f);
			float posY = element.GetAttributeFloat("ypos", 0f);
			newNode.Position = new Vector2(posX, posY);
			foreach (XElement valueElement in element.Elements())
			{
				newNode.Connections.Add(new EventEditorNodeConnection(newNode, NodeConnectionType.Value, valueElement.GetAttributeString("name", string.Empty), typeof(string), null));
			}
			return newNode;
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x00173F74 File Offset: 0x00172174
		private static void Prompt(Func<string, bool> OnAccepted)
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("Name"), "", new LocalizedString[]
			{
				TextManager.Get("Ok"),
				TextManager.Get("Cancel")
			}, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUITextBox nameInput = new GUITextBox(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				OnAccepted(nameInput.Text);
				msgBox.Close();
				return true;
			};
		}
	}
}
