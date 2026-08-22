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
	// Token: 0x020000FE RID: 254
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxComponent : CircuitBoxNode, ICircuitBoxIdentifiable
	{
		// Token: 0x170007C4 RID: 1988
		// (get) Token: 0x060019EB RID: 6635 RVA: 0x000C8704 File Offset: 0x000C6904
		public ushort ID { get; }

		// Token: 0x060019EC RID: 6636 RVA: 0x000C870C File Offset: 0x000C690C
		public CircuitBoxComponent(ushort id, Item item, Vector2 position, CircuitBox circuitBox, ItemPrefab usedResource) : base(circuitBox)
		{
			CircuitBoxComponent <>4__this = this;
			if (item.Connections == null)
			{
				string paramName = "Connections";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to load a CircuitBoxNode with an item \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(item.Prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" that has no connections.");
				throw new ArgumentNullException(paramName, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			List<CircuitBoxNodeConnection> conns = (from connection in item.Connections
			select new CircuitBoxNodeConnection(Vector2.Zero, <>4__this, connection, circuitBox)).ToList<CircuitBoxNodeConnection>();
			Vector2 size = CircuitBoxNode.CalculateSize(conns);
			this.ID = id;
			this.Item = item;
			this.Size = size;
			this.Connectors = conns.Cast<CircuitBoxConnection>().ToImmutableArray<CircuitBoxConnection>();
			base.Position = position;
			this.UsedResource = usedResource;
			base.UpdatePositions();
		}

		// Token: 0x060019ED RID: 6637 RVA: 0x000C87E8 File Offset: 0x000C69E8
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<CircuitBoxComponent> TryLoadFromXML(ContentXElement element, CircuitBox circuitBox)
		{
			ushort id = element.GetAttributeUInt16("id", ushort.MaxValue);
			string key = "position";
			Vector2 zero = Vector2.Zero;
			Vector2 position = element.GetAttributeVector2(key, zero);
			Option<ItemSlotIndexPair> itemIdOption = ItemSlotIndexPair.TryDeserializeFromXML(element, "backingitemid");
			Identifier usedResourceIdentifier = element.GetAttributeIdentifier("usedresource", Identifier.Empty);
			ItemSlotIndexPair itemId;
			Option.UnspecifiedNone none;
			if (itemIdOption.TryUnwrap(out itemId))
			{
				Item backingItem = itemId.FindItemInContainer(circuitBox.ComponentContainer);
				if (backingItem != null)
				{
					ItemPrefab usedResource;
					if (!ItemPrefab.Prefabs.TryGet(usedResourceIdentifier, out usedResource))
					{
						string gaIdentifier = "CircuitBoxComponent.TryLoadXML:UsedResourceNotFound";
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to find item prefab with identifier ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(usedResourceIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral(" for CircuitBoxNode with ID ");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
						DebugConsole.ThrowErrorAndLogToGA(gaIdentifier, defaultInterpolatedStringHandler.ToStringAndClear());
						none = Option.None;
						return none;
					}
					return Option.Some<CircuitBoxComponent>(new CircuitBoxComponent(id, backingItem, position, circuitBox, usedResource));
				}
			}
			string gaIdentifier2 = "CircuitBoxComponent.TryLoadFromXML:IdNotFound";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Failed to find item with ID ");
			defaultInterpolatedStringHandler2.AppendFormatted<ItemSlotIndexPair>(itemId);
			defaultInterpolatedStringHandler2.AppendLiteral(" for CircuitBoxNode with ID ");
			defaultInterpolatedStringHandler2.AppendFormatted<ushort>(id);
			DebugConsole.ThrowErrorAndLogToGA(gaIdentifier2, defaultInterpolatedStringHandler2.ToStringAndClear());
			none = Option.None;
			return none;
		}

		// Token: 0x060019EE RID: 6638 RVA: 0x000C891C File Offset: 0x000C6B1C
		public XElement Save()
		{
			return new XElement("Component", new object[]
			{
				new XAttribute("id", this.ID),
				new XAttribute("position", XMLExtensions.Vector2ToString(base.Position)),
				new XAttribute("backingitemid", ItemSlotIndexPair.Serialize(this.Item)),
				new XAttribute("usedresource", this.UsedResource.Identifier)
			});
		}

		// Token: 0x060019EF RID: 6639 RVA: 0x000C89B8 File Offset: 0x000C6BB8
		public void Remove()
		{
			EntitySpawner spawner = Entity.Spawner;
			if (spawner != null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					spawner.AddEntityToRemoveQueue(this.Item);
					return;
				}
			}
			this.Item.Remove();
		}

		// Token: 0x04000C6C RID: 3180
		public readonly Item Item;

		// Token: 0x04000C6E RID: 3182
		public readonly ItemPrefab UsedResource;
	}
}
