using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000108 RID: 264
	[NullableContext(1)]
	[Nullable(0)]
	internal class EditorNode
	{
		// Token: 0x170009DB RID: 2523
		// (get) Token: 0x06002494 RID: 9364 RVA: 0x001725A4 File Offset: 0x001707A4
		// (set) Token: 0x06002495 RID: 9365 RVA: 0x001725AC File Offset: 0x001707AC
		public Vector2 Position { get; set; }

		// Token: 0x170009DC RID: 2524
		// (get) Token: 0x06002496 RID: 9366 RVA: 0x001725B5 File Offset: 0x001707B5
		// (set) Token: 0x06002497 RID: 9367 RVA: 0x001725BD File Offset: 0x001707BD
		public Vector2 Size { get; set; }

		// Token: 0x170009DD RID: 2525
		// (get) Token: 0x06002498 RID: 9368 RVA: 0x001725C8 File Offset: 0x001707C8
		public Rectangle HeaderRectangle
		{
			get
			{
				return new Rectangle(this.Position.ToPoint(), new Point((int)this.Size.X, 32));
			}
		}

		// Token: 0x170009DE RID: 2526
		// (get) Token: 0x06002499 RID: 9369 RVA: 0x001725FC File Offset: 0x001707FC
		public Rectangle Rectangle
		{
			get
			{
				return new Rectangle(new Point((int)this.Position.X, (int)this.Position.Y + 32), this.Size.ToPoint());
			}
		}

		// Token: 0x170009DF RID: 2527
		// (get) Token: 0x0600249A RID: 9370 RVA: 0x0017263C File Offset: 0x0017083C
		// (set) Token: 0x0600249B RID: 9371 RVA: 0x00172644 File Offset: 0x00170844
		public string Name { get; protected set; }

		// Token: 0x170009E0 RID: 2528
		// (get) Token: 0x0600249C RID: 9372 RVA: 0x0017264D File Offset: 0x0017084D
		// (set) Token: 0x0600249D RID: 9373 RVA: 0x00172655 File Offset: 0x00170855
		public bool CanAddConnections { get; set; }

		// Token: 0x0600249E RID: 9374 RVA: 0x0017265E File Offset: 0x0017085E
		protected EditorNode(string name)
		{
			this.Name = name;
			this.Position = Vector2.Zero;
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x0017268E File Offset: 0x0017088E
		public virtual XElement Save()
		{
			throw new NotImplementedException();
		}

		// Token: 0x060024A0 RID: 9376 RVA: 0x00172698 File Offset: 0x00170898
		public XElement SaveConnections()
		{
			XElement allConnections = new XElement("Connections", new XAttribute("i", this.ID));
			foreach (EventEditorNodeConnection connection in this.Connections)
			{
				XElement connectionElement = new XElement("Connection");
				connectionElement.Add(new XAttribute("i", connection.ID));
				connectionElement.Add(new XAttribute("type", connection.Type.Label));
				if (connection.EndConversation)
				{
					connectionElement.Add(new XAttribute("endconversation", connection.EndConversation));
				}
				if (!string.IsNullOrWhiteSpace(connection.OptionText))
				{
					connectionElement.Add(new XAttribute("optiontext", connection.OptionText));
				}
				object overrideValue = connection.OverrideValue;
				if (overrideValue != null)
				{
					object overrideValue2 = connection.OverrideValue;
					if (!string.IsNullOrWhiteSpace((overrideValue2 != null) ? overrideValue2.ToString() : null))
					{
						connectionElement.Add(new XAttribute("overridevalue", overrideValue.ToString() ?? string.Empty));
						connectionElement.Add(new XAttribute("valuetype", overrideValue.GetType().ToString()));
					}
				}
				foreach (EventEditorNodeConnection nodeConnection in connection.ConnectedTo)
				{
					XElement connectedTo = new XElement("ConnectedTo", new object[]
					{
						new XAttribute("i", nodeConnection.ID),
						new XAttribute("node", nodeConnection.Parent.ID)
					});
					connectionElement.Add(connectedTo);
				}
				allConnections.Add(connectionElement);
			}
			return allConnections;
		}

		// Token: 0x060024A1 RID: 9377 RVA: 0x001728DC File Offset: 0x00170ADC
		public void LoadConnections(XElement element)
		{
			foreach (XElement subElement in element.Elements())
			{
				int id = subElement.GetAttributeInt("i", -1);
				string connectionType = subElement.GetAttributeString("type", null);
				bool endConversation = subElement.GetAttributeBool("endconversation", false);
				if (id >= 0)
				{
					EventEditorNodeConnection connection = this.Connections.Find((EventEditorNodeConnection c) => c.ID == id);
					if (connection == null)
					{
						if (!string.Equals(connectionType, NodeConnectionType.Option.Label, StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						connection = new EventEditorNodeConnection(this, NodeConnectionType.Option, "", null, null)
						{
							ID = id,
							EndConversation = endConversation
						};
						this.Connections.Add(connection);
					}
					string optionText = subElement.GetAttributeString("optiontext", null);
					string overrideValue = subElement.GetAttributeString("overridevalue", null);
					string valueType = subElement.GetAttributeString("valuetype", null);
					if (optionText != null)
					{
						connection.OptionText = optionText;
					}
					if (overrideValue != null && valueType != null)
					{
						Type type = Type.GetType(valueType);
						if (type != null)
						{
							if (type.IsEnum)
							{
								Array enums = Enum.GetValues(type);
								using (IEnumerator enumerator2 = enums.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										object @enum = enumerator2.Current;
										if (string.Equals((@enum != null) ? @enum.ToString() : null, overrideValue, StringComparison.InvariantCultureIgnoreCase))
										{
											connection.OverrideValue = @enum;
										}
									}
									goto IL_18E;
								}
							}
							connection.OverrideValue = EventEditorScreen.ChangeType(overrideValue, type);
						}
					}
					IL_18E:
					foreach (XElement connectedTo in subElement.Elements())
					{
						int id2 = connectedTo.GetAttributeInt("i", -1);
						int node = connectedTo.GetAttributeInt("node", -1);
						if (id2 >= 0 && node >= 0)
						{
							EditorNode otherNode = EventEditorScreen.nodeList.Find((EditorNode editorNode) => editorNode.ID == node);
							EventEditorNodeConnection otherConnection = (otherNode != null) ? otherNode.Connections.Find((EventEditorNodeConnection c) => c.ID == id2) : null;
							if (otherConnection != null)
							{
								connection.ConnectedTo.Add(otherConnection);
							}
						}
					}
				}
			}
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x00172BA0 File Offset: 0x00170DA0
		[return: Nullable(2)]
		public static EditorNode Load(XElement element)
		{
			string a = element.Name.ToString().ToLowerInvariant();
			EditorNode result;
			if (!(a == "eventnode"))
			{
				if (!(a == "valuenode"))
				{
					if (!(a == "customnode"))
					{
						result = null;
					}
					else
					{
						result = CustomNode.LoadCustomNode(element);
					}
				}
				else
				{
					result = ValueNode.LoadValueNode(element);
				}
			}
			else
			{
				result = EventNode.LoadEventNode(element);
			}
			return result;
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x00172C08 File Offset: 0x00170E08
		[NullableContext(2)]
		public virtual XElement ToXML()
		{
			XElement newElement = new XElement(this.Name);
			foreach (EventEditorNodeConnection connection in this.Connections)
			{
				if (connection.Type == NodeConnectionType.Value)
				{
					object connValue = connection.GetValue();
					if (connValue != null)
					{
						newElement.Add(new XAttribute(connection.Attribute.ToLowerInvariant(), connValue));
					}
				}
			}
			newElement.Add(new XAttribute("_npos", XMLExtensions.Vector2ToString(this.Position)));
			return newElement;
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x00172CBC File Offset: 0x00170EBC
		public void Connect(EditorNode otherNode, NodeConnectionType type)
		{
			EventEditorNodeConnection conn = this.Connections.Find((EventEditorNodeConnection connection) => connection.Type == type && !connection.ConnectedTo.Any<EventEditorNodeConnection>());
			EventEditorNodeConnection found = otherNode.Connections.Find((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Activate);
			if (found != null && conn != null)
			{
				conn.ConnectedTo.Add(found);
			}
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x00172D2B File Offset: 0x00170F2B
		public static void Connect(EventEditorNodeConnection connection, EventEditorNodeConnection ownConnection)
		{
			connection.ConnectedTo.Add(ownConnection);
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x00172D3C File Offset: 0x00170F3C
		public static void Disconnect(EventEditorNodeConnection conn)
		{
			IEnumerable<EditorNode> nodeList = EventEditorScreen.nodeList;
			Func<EditorNode, IEnumerable<EventEditorNodeConnection>> <>9__0;
			Func<EditorNode, IEnumerable<EventEditorNodeConnection>> selector;
			if ((selector = <>9__0) == null)
			{
				Func<EventEditorNodeConnection, bool> <>9__1;
				EventEditorNodeConnection connection;
				selector = (<>9__0 = delegate(EditorNode editorNode)
				{
					IEnumerable<EventEditorNodeConnection> connections = editorNode.Connections;
					Func<EventEditorNodeConnection, bool> predicate;
					if ((predicate = <>9__1) == null)
					{
						predicate = (<>9__1 = ((EventEditorNodeConnection connection) => connection.ConnectedTo.Contains(conn)));
					}
					return connections.Where(predicate);
				});
			}
			foreach (EventEditorNodeConnection connection in nodeList.SelectMany(selector))
			{
				EventEditorNodeConnection connection;
				connection.ConnectedTo.Remove(conn);
			}
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x00172DC4 File Offset: 0x00170FC4
		public void ClearConnections()
		{
			foreach (EventEditorNodeConnection conn in this.Connections)
			{
				conn.ClearConnections();
			}
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x00172E18 File Offset: 0x00171018
		public virtual Rectangle GetDrawRectangle()
		{
			return this.Rectangle;
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x00172E20 File Offset: 0x00171020
		[NullableContext(2)]
		public EventEditorNodeConnection GetConnectionOnMouse(Vector2 mousePos)
		{
			return this.Connections.FirstOrDefault((EventEditorNodeConnection eventNodeConnection) => eventNodeConnection.DrawRectangle.Contains(mousePos));
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x00172E51 File Offset: 0x00171051
		public void Draw(SpriteBatch spriteBatch)
		{
			this.DrawBack(spriteBatch);
			this.DrawFront(spriteBatch);
		}

		// Token: 0x060024AB RID: 9387 RVA: 0x00172E61 File Offset: 0x00171061
		protected virtual void DrawFront(SpriteBatch spriteBatch)
		{
		}

		// Token: 0x170009E1 RID: 2529
		// (get) Token: 0x060024AC RID: 9388 RVA: 0x00172E63 File Offset: 0x00171063
		protected virtual Color BackgroundColor
		{
			get
			{
				return new Color(150, 150, 150);
			}
		}

		// Token: 0x060024AD RID: 9389 RVA: 0x00172E7C File Offset: 0x0017107C
		protected virtual void DrawBack(SpriteBatch spriteBatch)
		{
			Color outlineColor = Color.White * 0.8f;
			Color fontColor = Color.White;
			Color headerColor = this.IsHighlighted ? new Color(100, 100, 100) : new Color(120, 120, 120);
			if (this.IsSelected)
			{
				headerColor = new Color(80, 80, 80);
			}
			EventEditorScreen eventEditor = Screen.Selected as EventEditorScreen;
			float camZoom = (eventEditor != null) ? eventEditor.Cam.Zoom : 1f;
			Rectangle bodyRect = this.GetDrawRectangle();
			GUI.DrawRectangle(spriteBatch, this.HeaderRectangle, headerColor, true, 1f, 1f);
			GUI.DrawRectangle(spriteBatch, bodyRect, this.BackgroundColor, true, 1f, 1f);
			GUI.DrawRectangle(spriteBatch, this.HeaderRectangle, outlineColor, false, 1f, (float)((int)Math.Max(1f, 1.25f / camZoom)));
			GUI.DrawRectangle(spriteBatch, bodyRect, outlineColor, false, 1f, (float)((int)Math.Max(1f, 1.25f / camZoom)));
			this.DrawConnections(spriteBatch);
			Vector2 headerSize = GUIStyle.SubHeadingFont.MeasureString(this.Name, false);
			GUIStyle.SubHeadingFont.DrawString(spriteBatch, this.Name, this.HeaderRectangle.Location.ToVector2() + this.HeaderRectangle.Size.ToVector2() / 2f - headerSize / 2f, fontColor, ForceUpperCase.Inherit, false);
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x00173000 File Offset: 0x00171200
		protected virtual void DrawConnections(SpriteBatch spriteBatch)
		{
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
							connection.Draw(spriteBatch, this.Rectangle, x);
							x++;
						}
					}
					else
					{
						connection.Draw(spriteBatch, this.Rectangle, y);
						y++;
					}
				}
			}
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x00173098 File Offset: 0x00171298
		protected virtual bool ShouldDrawConnection(EventEditorNodeConnection connection)
		{
			return true;
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x0017309B File Offset: 0x0017129B
		public void AddConnection(NodeConnectionType connectionType)
		{
			this.Connections.Add(new EventEditorNodeConnection(this, connectionType, "", null, null));
		}

		// Token: 0x060024B1 RID: 9393 RVA: 0x001730B6 File Offset: 0x001712B6
		public virtual void AddOption()
		{
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Option, "", null, null));
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x001730D8 File Offset: 0x001712D8
		public void RemoveOption(EventEditorNodeConnection connection)
		{
			int index = this.Connections.IndexOf(connection);
			foreach (EventEditorNodeConnection nodeConnection in this.Connections.Skip(index))
			{
				EventEditorNodeConnection eventEditorNodeConnection = nodeConnection;
				int id = eventEditorNodeConnection.ID;
				eventEditorNodeConnection.ID = id - 1;
			}
			this.Connections.Remove(connection);
		}

		// Token: 0x060024B3 RID: 9395 RVA: 0x00173150 File Offset: 0x00171350
		[NullableContext(2)]
		public EditorNode GetNext()
		{
			EventEditorNodeConnection nextNode = this.Connections.Find((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Next);
			if (nextNode == null)
			{
				return null;
			}
			EventEditorNodeConnection eventEditorNodeConnection = nextNode.ConnectedTo.FirstOrDefault<EventEditorNodeConnection>();
			if (eventEditorNodeConnection == null)
			{
				return null;
			}
			return eventEditorNodeConnection.Parent;
		}

		// Token: 0x060024B4 RID: 9396 RVA: 0x001731A4 File Offset: 0x001713A4
		[return: Nullable(2)]
		public EditorNode GetNext(NodeConnectionType type)
		{
			EventEditorNodeConnection nextNode = this.Connections.Find((EventEditorNodeConnection connection) => connection.Type == type);
			if (nextNode == null)
			{
				return null;
			}
			EventEditorNodeConnection eventEditorNodeConnection = nextNode.ConnectedTo.FirstOrDefault<EventEditorNodeConnection>();
			if (eventEditorNodeConnection == null)
			{
				return null;
			}
			return eventEditorNodeConnection.Parent;
		}

		// Token: 0x060024B5 RID: 9397 RVA: 0x001731F1 File Offset: 0x001713F1
		public static bool IsInstanceOf(Type type1, Type type2)
		{
			return type1.IsAssignableFrom(type2) || type1.IsSubclassOf(type2);
		}

		// Token: 0x060024B6 RID: 9398 RVA: 0x00173208 File Offset: 0x00171408
		[NullableContext(2)]
		public EditorNode GetParent()
		{
			EventEditorNodeConnection myNode = this.Connections.Find((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Activate);
			if (myNode == null)
			{
				return null;
			}
			Func<EventEditorNodeConnection, bool> <>9__2;
			foreach (EditorNode editorNode in EventEditorScreen.nodeList)
			{
				List<EventEditorNodeConnection> childConnection = (from connection in editorNode.Connections
				where connection.Type == NodeConnectionType.Next || connection.Type == NodeConnectionType.Option || connection.Type == NodeConnectionType.Failure || connection.Type == NodeConnectionType.Success || connection.Type == NodeConnectionType.Add
				select connection).ToList<EventEditorNodeConnection>();
				IEnumerable<EventEditorNodeConnection> source = childConnection;
				Func<EventEditorNodeConnection, bool> predicate;
				if ((predicate = <>9__2) == null)
				{
					predicate = (<>9__2 = ((EventEditorNodeConnection connection) => connection != null && connection.ConnectedTo.Contains(myNode)));
				}
				if (source.Any(predicate))
				{
					return editorNode;
				}
			}
			return null;
		}

		// Token: 0x04001241 RID: 4673
		public int ID;

		// Token: 0x04001242 RID: 4674
		private const int HeaderSize = 32;

		// Token: 0x04001245 RID: 4677
		public readonly List<EventEditorNodeConnection> Connections = new List<EventEditorNodeConnection>();

		// Token: 0x04001246 RID: 4678
		public readonly List<NodeConnectionType> RemovableTypes = new List<NodeConnectionType>();

		// Token: 0x04001247 RID: 4679
		public bool IsHighlighted;

		// Token: 0x04001248 RID: 4680
		public bool IsSelected;
	}
}
