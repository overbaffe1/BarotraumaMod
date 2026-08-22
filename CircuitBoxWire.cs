using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200003D RID: 61
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxWire : CircuitBoxSelectable, ICircuitBoxIdentifiable
	{
		// Token: 0x0600096D RID: 2413 RVA: 0x00055C67 File Offset: 0x00053E67
		public void Update()
		{
			this.Renderer.Recompute(this.From.AnchorPoint, this.To.AnchorPoint, this.Color);
		}

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x0600096E RID: 2414 RVA: 0x00055C90 File Offset: 0x00053E90
		public ushort ID { get; }

		// Token: 0x0600096F RID: 2415 RVA: 0x00055C98 File Offset: 0x00053E98
		public CircuitBoxWire(CircuitBox circuitBox, ushort Id, [Nullable(new byte[]
		{
			0,
			1
		})] Option<Item> backingItem, CircuitBoxConnection from, CircuitBoxConnection to, ItemPrefab prefab)
		{
			this.ID = Id;
			this.From = from;
			this.To = to;
			this.BackingWire = backingItem;
			this.Color = prefab.SpriteColor;
			this.UsedItemPrefab = prefab;
			this.Renderer = new CircuitBoxWireRenderer(Option.Some<CircuitBoxWire>(this), to.AnchorPoint, from.AnchorPoint, this.Color, circuitBox.WireSprite);
			this.EnsureWireConnected();
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x00055D10 File Offset: 0x00053F10
		public XElement Save()
		{
			Item item;
			XElement element = new XElement("Wire", new object[]
			{
				new XAttribute("id", this.ID),
				new XAttribute("backingitemid", this.BackingWire.TryUnwrap(out item) ? ItemSlotIndexPair.Serialize(item) : string.Empty),
				new XAttribute("prefab", this.UsedItemPrefab.Identifier)
			});
			XElement fromElement = CircuitBoxConnectorIdentifier.FromConnection(this.From).Save("From");
			XElement toElement = CircuitBoxConnectorIdentifier.FromConnection(this.To).Save("To");
			element.Add(fromElement);
			element.Add(toElement);
			return element;
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x00055DE4 File Offset: 0x00053FE4
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<CircuitBoxWire> TryLoadFromXML(ContentXElement element, CircuitBox circuitBox)
		{
			ushort id = element.GetAttributeUInt16("id", ushort.MaxValue);
			Option<ItemSlotIndexPair> backingItemIdOption = ItemSlotIndexPair.TryDeserializeFromXML(element, "backingitemid");
			Identifier usedPrefabIdentifier = element.GetAttributeIdentifier("prefab", Identifier.Empty);
			ItemPrefab itemPrefab;
			Option.UnspecifiedNone none;
			if (!ItemPrefab.Prefabs.TryGet(usedPrefabIdentifier, out itemPrefab))
			{
				string gaIdentifier = "CircuitBoxWire.TryLoadFromXML:PrefabNotFound";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find prefab used to create wire with identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(usedPrefabIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" for CircuitBoxWire with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
				DebugConsole.ThrowErrorAndLogToGA(gaIdentifier, defaultInterpolatedStringHandler.ToStringAndClear());
				none = Option.None;
				return none;
			}
			none = Option.None;
			Option<Item> backingItem = none;
			ItemSlotIndexPair backingItemIdPair;
			if (backingItemIdOption.TryUnwrap(out backingItemIdPair))
			{
				Item item = backingItemIdPair.FindItemInContainer(circuitBox.WireContainer);
				if (item == null)
				{
					string gaIdentifier2 = "CircuitBoxWire.TryLoadFromXML:IdNotFound";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to find item with ID ");
					defaultInterpolatedStringHandler2.AppendFormatted<ItemSlotIndexPair>(backingItemIdPair);
					defaultInterpolatedStringHandler2.AppendLiteral(" for CircuitBoxWire with ID ");
					defaultInterpolatedStringHandler2.AppendFormatted<ushort>(id);
					DebugConsole.ThrowErrorAndLogToGA(gaIdentifier2, defaultInterpolatedStringHandler2.ToStringAndClear());
					none = Option.None;
					return none;
				}
				backingItem = Option.Some<Item>(item);
			}
			none = Option.None;
			Option<CircuitBoxConnection> From = none;
			none = Option.None;
			Option<CircuitBoxConnection> To = none;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				CircuitBoxConnection fromConnection;
				if (!(a == "from"))
				{
					if (a == "to")
					{
						CircuitBoxConnection toConnection;
						if (CircuitBoxConnectorIdentifier.Load(subElement).FindConnection(circuitBox).TryUnwrap(out toConnection))
						{
							To = Option.Some<CircuitBoxConnection>(toConnection);
						}
					}
				}
				else if (CircuitBoxConnectorIdentifier.Load(subElement).FindConnection(circuitBox).TryUnwrap(out fromConnection))
				{
					From = Option.Some<CircuitBoxConnection>(fromConnection);
				}
			}
			CircuitBoxConnection from;
			CircuitBoxConnection to;
			if (From.TryUnwrap(out from) && To.TryUnwrap(out to))
			{
				return Option.Some<CircuitBoxWire>(new CircuitBoxWire(circuitBox, id, backingItem, from, to, itemPrefab));
			}
			string gaIdentifier3 = "CircuitBoxWire.TryLoadFromXML:MissingFromOrTo";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(74, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("Failed to load CircuitBoxWire with ID ");
			defaultInterpolatedStringHandler3.AppendFormatted<ushort>(id);
			defaultInterpolatedStringHandler3.AppendLiteral(", missing \"From\" or \"To\" connection.");
			DebugConsole.ThrowErrorAndLogToGA(gaIdentifier3, defaultInterpolatedStringHandler3.ToStringAndClear());
			none = Option.None;
			return none;
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x00056068 File Offset: 0x00054268
		public void EnsureWireConnected()
		{
			CircuitBoxWire.<EnsureWireConnected>g__EnsureExternalConnection|13_0(this.From, this.To);
			CircuitBoxWire.<EnsureWireConnected>g__EnsureExternalConnection|13_0(this.To, this.From);
			Item item;
			if (this.BackingWire.TryUnwrap(out item))
			{
				Wire wire = item.GetComponent<Wire>();
				if (wire != null)
				{
					wire.DropOnConnect = false;
					this.From.Connection.ConnectWire(wire);
					this.To.Connection.ConnectWire(wire);
					wire.Connect(this.From.Connection, 0, false, false);
					wire.Connect(this.To.Connection, 1, false, false);
					return;
				}
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x00056104 File Offset: 0x00054304
		public void Remove()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			Item wireItem;
			if (!this.BackingWire.TryUnwrap(out wireItem))
			{
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner != null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					spawner.AddEntityToRemoveQueue(wireItem);
					return;
				}
			}
			Wire wire = wireItem.GetComponent<Wire>();
			if (wire != null)
			{
				this.From.Connection.DisconnectWire(wire);
				this.To.Connection.DisconnectWire(wire);
			}
			if (!wireItem.Removed)
			{
				wireItem.Remove();
			}
		}

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000974 RID: 2420 RVA: 0x00056192 File Offset: 0x00054392
		public static ItemPrefab DefaultWirePrefab
		{
			get
			{
				return ItemPrefab.Prefabs[Tags.RedWire];
			}
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x000561C0 File Offset: 0x000543C0
		[CompilerGenerated]
		internal static void <EnsureWireConnected>g__EnsureExternalConnection|13_0(CircuitBoxConnection one, CircuitBoxConnection two)
		{
			CircuitBoxInputConnection input = one as CircuitBoxInputConnection;
			if (input == null)
			{
				CircuitBoxOutputConnection output = one as CircuitBoxOutputConnection;
				if (output == null)
				{
					CircuitBoxNodeConnection node = one as CircuitBoxNodeConnection;
					if (node == null)
					{
						return;
					}
					CircuitBoxOutputConnection output2 = two as CircuitBoxOutputConnection;
					if (output2 != null)
					{
						if (!node.Connection.CircuitBoxConnections.Contains(output2))
						{
							node.Connection.CircuitBoxConnections.Add(output2);
							return;
						}
					}
					else
					{
						CircuitBoxNodeConnection node2 = node;
						CircuitBoxInputConnection input2 = two as CircuitBoxInputConnection;
						if (input2 != null)
						{
							if (!node2.Connection.CircuitBoxConnections.Contains(input2))
							{
								node2.Connection.CircuitBoxConnections.Add(input2);
							}
							if (!node2.ExternallyConnectedFrom.Contains(input2))
							{
								node2.ExternallyConnectedFrom.Add(input2);
							}
						}
					}
				}
				else if (!output.ExternallyConnectedFrom.Contains(two))
				{
					output.ExternallyConnectedFrom.Add(two);
					return;
				}
			}
			else if (!input.ExternallyConnectedTo.Contains(two))
			{
				input.ExternallyConnectedTo.Add(two);
				return;
			}
		}

		// Token: 0x040004E2 RID: 1250
		public CircuitBoxWireRenderer Renderer;

		// Token: 0x040004E3 RID: 1251
		public CircuitBoxConnection From;

		// Token: 0x040004E4 RID: 1252
		public CircuitBoxConnection To;

		// Token: 0x040004E5 RID: 1253
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<Item> BackingWire;

		// Token: 0x040004E6 RID: 1254
		public readonly Color Color;

		// Token: 0x040004E7 RID: 1255
		public readonly ItemPrefab UsedItemPrefab;

		// Token: 0x040004E9 RID: 1257
		public static ItemPrefab SelectedWirePrefab = CircuitBoxWire.DefaultWirePrefab;

		// Token: 0x040004EA RID: 1258
		public static readonly Color DefaultWireColor = CircuitBoxWire.DefaultWirePrefab.SpriteColor;
	}
}
