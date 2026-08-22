using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000122 RID: 290
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxWire : CircuitBoxSelectable, ICircuitBoxIdentifiable
	{
		// Token: 0x1700081A RID: 2074
		// (get) Token: 0x06001B8D RID: 7053 RVA: 0x000CCBF5 File Offset: 0x000CADF5
		public ushort ID { get; }

		// Token: 0x06001B8E RID: 7054 RVA: 0x000CCC00 File Offset: 0x000CAE00
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
			this.EnsureWireConnected();
		}

		// Token: 0x06001B8F RID: 7055 RVA: 0x000CCC4C File Offset: 0x000CAE4C
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

		// Token: 0x06001B90 RID: 7056 RVA: 0x000CCD20 File Offset: 0x000CAF20
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

		// Token: 0x06001B91 RID: 7057 RVA: 0x000CCFA4 File Offset: 0x000CB1A4
		public void EnsureWireConnected()
		{
			CircuitBoxWire.<EnsureWireConnected>g__EnsureExternalConnection|11_0(this.From, this.To);
			CircuitBoxWire.<EnsureWireConnected>g__EnsureExternalConnection|11_0(this.To, this.From);
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

		// Token: 0x06001B92 RID: 7058 RVA: 0x000CD040 File Offset: 0x000CB240
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

		// Token: 0x1700081B RID: 2075
		// (get) Token: 0x06001B93 RID: 7059 RVA: 0x000CD0CE File Offset: 0x000CB2CE
		public static ItemPrefab DefaultWirePrefab
		{
			get
			{
				return ItemPrefab.Prefabs[Tags.RedWire];
			}
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x000CD0FC File Offset: 0x000CB2FC
		[CompilerGenerated]
		internal static void <EnsureWireConnected>g__EnsureExternalConnection|11_0(CircuitBoxConnection one, CircuitBoxConnection two)
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

		// Token: 0x04000CF1 RID: 3313
		public CircuitBoxConnection From;

		// Token: 0x04000CF2 RID: 3314
		public CircuitBoxConnection To;

		// Token: 0x04000CF3 RID: 3315
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<Item> BackingWire;

		// Token: 0x04000CF4 RID: 3316
		public readonly Color Color;

		// Token: 0x04000CF5 RID: 3317
		public readonly ItemPrefab UsedItemPrefab;

		// Token: 0x04000CF7 RID: 3319
		public static ItemPrefab SelectedWirePrefab = CircuitBoxWire.DefaultWirePrefab;

		// Token: 0x04000CF8 RID: 3320
		public static readonly Color DefaultWireColor = CircuitBoxWire.DefaultWirePrefab.SpriteColor;
	}
}
