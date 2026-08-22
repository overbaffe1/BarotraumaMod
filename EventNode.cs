using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000109 RID: 265
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventNode : EditorNode
	{
		// Token: 0x170009E2 RID: 2530
		// (get) Token: 0x060024B7 RID: 9399 RVA: 0x001732F8 File Offset: 0x001714F8
		protected override Color BackgroundColor
		{
			get
			{
				if (!EventEditorScreen.ConversationMode || EditorNode.IsInstanceOf(this.type, typeof(ConversationAction)))
				{
					return new Color(150, 150, 150);
				}
				return new Color(80, 80, 80);
			}
		}

		// Token: 0x060024B8 RID: 9400 RVA: 0x00173338 File Offset: 0x00171538
		public EventNode(Type type, string name) : base(name)
		{
			this.type = type;
			base.Size = new Vector2(256f, 256f);
			PropertyInfo[] properties = (from info in type.GetProperties()
			where info.CustomAttributes.Any((CustomAttributeData data) => data.AttributeType == typeof(Serialize))
			select info).ToArray<PropertyInfo>();
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Activate, "", null, null));
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Next, "", null, null));
			foreach (PropertyInfo property in properties)
			{
				this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Value, property.Name, property.PropertyType, property));
			}
			if (EditorNode.IsInstanceOf(type, typeof(BinaryOptionAction)))
			{
				this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Success, "", null, null));
				this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Failure, "", null, null));
			}
			if (EditorNode.IsInstanceOf(type, typeof(ConversationAction)))
			{
				base.CanAddConnections = true;
				this.RemovableTypes.Add(NodeConnectionType.Option);
			}
			if (EditorNode.IsInstanceOf(type, typeof(StatusEffectAction)) || EditorNode.IsInstanceOf(type, typeof(MissionAction)))
			{
				this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Add, "", null, null));
			}
		}

		// Token: 0x060024B9 RID: 9401 RVA: 0x001734C0 File Offset: 0x001716C0
		public override XElement Save()
		{
			return new XElement("EventNode", new object[]
			{
				new XAttribute("i", this.ID),
				new XAttribute("type", this.type.ToString()),
				new XAttribute("name", base.Name),
				new XAttribute("xpos", base.Position.X),
				new XAttribute("ypos", base.Position.Y)
			});
		}

		// Token: 0x060024BA RID: 9402 RVA: 0x0017357C File Offset: 0x0017177C
		[return: Nullable(2)]
		public static EditorNode LoadEventNode(XElement element)
		{
			if (!string.Equals(element.Name.ToString(), "EventNode", StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}
			Type t = Type.GetType(element.GetAttributeString("type", string.Empty));
			if (t == null)
			{
				return null;
			}
			string name = element.GetAttributeString("name", string.Empty);
			int id = element.GetAttributeInt("i", -1);
			EventNode eventNode;
			if (!EditorNode.IsInstanceOf(t, typeof(ConversationAction)))
			{
				(eventNode = new EventNode(t, name)).ID = id;
			}
			else
			{
				(eventNode = new EventConversationNode(t, name)).ID = id;
			}
			EditorNode newNode = eventNode;
			float posX = element.GetAttributeFloat("xpos", 0f);
			float posY = element.GetAttributeFloat("ypos", 0f);
			newNode.Position = new Vector2(posX, posY);
			return newNode;
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x00173647 File Offset: 0x00171847
		public override Rectangle GetDrawRectangle()
		{
			return EventNode.ScaleRectFromConnections(this.Connections, base.Rectangle);
		}

		// Token: 0x060024BC RID: 9404 RVA: 0x0017365C File Offset: 0x0017185C
		public static Rectangle ScaleRectFromConnections(List<EventEditorNodeConnection> connections, Rectangle baseRect)
		{
			int y = connections.Count((EventEditorNodeConnection connection) => connection.Type.NodeSide == NodeConnectionType.Side.Left);
			int x = connections.Count((EventEditorNodeConnection connection) => connection.Type.NodeSide == NodeConnectionType.Side.Right);
			int maxHeight = Math.Max(x, y);
			Rectangle bodyRect = baseRect;
			bodyRect.Height = bodyRect.Height / 8 * maxHeight;
			return bodyRect;
		}

		// Token: 0x060024BD RID: 9405 RVA: 0x001736D4 File Offset: 0x001718D4
		[return: Nullable(new byte[]
		{
			1,
			1,
			2,
			2
		})]
		public Tuple<EditorNode, string, bool>[] GetOptions()
		{
			IEnumerable<EventEditorNodeConnection> myNode = (from connection in this.Connections
			where connection.Type == NodeConnectionType.Option
			select connection).ToArray<EventEditorNodeConnection>();
			List<Tuple<EditorNode, string, bool>> list = new List<Tuple<EditorNode, string, bool>>();
			if (myNode != null)
			{
				foreach (EventEditorNodeConnection connection2 in myNode)
				{
					if (connection2.ConnectedTo.Any<EventEditorNodeConnection>())
					{
						using (List<EventEditorNodeConnection>.Enumerator enumerator2 = connection2.ConnectedTo.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								EventEditorNodeConnection nodeConnection = enumerator2.Current;
								list.Add(Tuple.Create<EditorNode, string, bool>(nodeConnection.Parent, connection2.OptionText, connection2.EndConversation));
							}
							continue;
						}
					}
					list.Add(Tuple.Create<EditorNode, string, bool>(null, connection2.OptionText, connection2.EndConversation));
				}
			}
			return list.ToArray();
		}

		// Token: 0x04001249 RID: 4681
		private readonly Type type;
	}
}
