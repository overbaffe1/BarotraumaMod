using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000035 RID: 53
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxComponent : CircuitBoxNode, ICircuitBoxIdentifiable
	{
		// Token: 0x17000298 RID: 664
		// (get) Token: 0x060008E3 RID: 2275 RVA: 0x000500A4 File Offset: 0x0004E2A4
		private Sprite Sprite
		{
			get
			{
				return this.Item.Prefab.InventoryIcon ?? this.Item.Prefab.Sprite;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x060008E4 RID: 2276 RVA: 0x000500CC File Offset: 0x0004E2CC
		private CircuitBoxLabel Label
		{
			get
			{
				CircuitBoxLabel? circuitBoxLabel = this.label;
				if (circuitBoxLabel != null)
				{
					return circuitBoxLabel.GetValueOrDefault();
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("circuitboxnode.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Item.Prefab.Identifier);
				LocalizedString name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback("[FALLBACK] " + this.Item.Name, true);
				this.label = new CircuitBoxLabel?(new CircuitBoxLabel(name, GUIStyle.LargeFont));
				return this.label.Value;
			}
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x00050170 File Offset: 0x0004E370
		public void UpdateEditing(RectTransform parent)
		{
			GUIComponent editor;
			if (CircuitBoxComponent.EditingHUD.TryUnwrap(out editor))
			{
				if (editor.UserData == this)
				{
					return;
				}
				CircuitBoxComponent.RemoveEditingHUD();
			}
			CircuitBoxComponent.EditingHUD = Option.Some<GUIComponent>(this.CreateEditingHUD(parent));
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x000501AC File Offset: 0x0004E3AC
		public static void RemoveEditingHUD()
		{
			GUIComponent editor;
			if (!CircuitBoxComponent.EditingHUD.TryUnwrap(out editor))
			{
				return;
			}
			editor.RectTransform.Parent = null;
			Option.UnspecifiedNone none = Option.None;
			CircuitBoxComponent.EditingHUD = none;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x000501E8 File Offset: 0x0004E3E8
		public GUIComponent CreateEditingHUD(RectTransform parent)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.3f), parent, Anchor.TopRight, null, null, null, ScaleBasis.Normal), "", null)
			{
				UserData = this
			};
			GUIListBox listBox = new GUIListBox(new RectTransform(ToolBox.PaddingSizeParentRelative(frame.RectTransform, 0.8f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				KeepSpaceForScrollBar = true,
				AutoHideScrollBar = false,
				CanTakeKeyBoardFocus = false
			};
			Screen selected = Screen.Selected;
			bool isEditor = selected != null && selected.IsEditor;
			GUILayoutGroup titleHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.3f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(Vector2.One, titleHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.Item.Prefab.Name;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			guitextBlock.TextColor = Color.White;
			guitextBlock.Color = Color.Black;
			int fieldCount = 0;
			using (List<ItemComponent>.Enumerator enumerator = this.Item.Components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemComponent ic = enumerator.Current;
					if (!(ic is Holdable))
					{
						if (!ic.AllowInGameEditing)
						{
							selected = Screen.Selected;
							if (selected == null || !selected.IsEditor)
							{
								continue;
							}
						}
						if (SerializableProperty.GetProperties<InGameEditable>(ic).Count != 0 || SerializableProperty.GetProperties<ConditionallyEditable>(ic).Any((SerializableProperty p) => p.GetAttribute<ConditionallyEditable>().IsEditable(ic)))
						{
							new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
							SerializableEntityEditor componentEditor = new SerializableEntityEditor(listBox.Content.RectTransform, ic, !isEditor, false, "", 24, GUIStyle.SubHeadingFont, true)
							{
								Readonly = this.CircuitBox.IsLocked()
							};
							fieldCount += componentEditor.Fields.Count;
							ic.CreateEditingHUD(componentEditor);
							componentEditor.Recalculate();
						}
					}
				}
			}
			if (fieldCount == 0)
			{
				frame.Visible = false;
			}
			return frame;
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x00050528 File Offset: 0x0004E728
		public override void DrawHeader(SpriteBatch spriteBatch, RectangleF drawRect, Color color)
		{
			Vector2 scale = new Vector2(drawRect.Height / MathF.Min(this.Sprite.size.X, this.Sprite.size.Y));
			Vector2 spritePosition = new Vector2(drawRect.Left, drawRect.Top);
			float spriteWidth = this.Sprite.size.X * scale.X;
			this.Sprite.Draw(spriteBatch, spritePosition, Color.White, Vector2.Zero, 0f, scale, SpriteEffects.None, null);
			Vector2 pos = new Vector2(spritePosition.X + spriteWidth + 8f, drawRect.Center.Y - this.Label.Size.Y / 2f);
			LocalizedString value = this.Label.Value;
			Color color2 = GUIStyle.TextColorNormal;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUI.DrawString(spriteBatch, pos, value, color2, null, 0, largeFont, ForceUpperCase.Inherit);
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x060008E9 RID: 2281 RVA: 0x0005062A File Offset: 0x0004E82A
		public ushort ID { get; }

		// Token: 0x060008EA RID: 2282 RVA: 0x00050634 File Offset: 0x0004E834
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

		// Token: 0x060008EB RID: 2283 RVA: 0x00050710 File Offset: 0x0004E910
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

		// Token: 0x060008EC RID: 2284 RVA: 0x00050844 File Offset: 0x0004EA44
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

		// Token: 0x060008ED RID: 2285 RVA: 0x000508E0 File Offset: 0x0004EAE0
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

		// Token: 0x060008EE RID: 2286 RVA: 0x00050920 File Offset: 0x0004EB20
		// Note: this type is marked as 'beforefieldinit'.
		static CircuitBoxComponent()
		{
			Option.UnspecifiedNone none = Option.None;
			CircuitBoxComponent.EditingHUD = none;
		}

		// Token: 0x04000498 RID: 1176
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<GUIComponent> EditingHUD;

		// Token: 0x04000499 RID: 1177
		private CircuitBoxLabel? label;

		// Token: 0x0400049A RID: 1178
		public readonly Item Item;

		// Token: 0x0400049C RID: 1180
		public readonly ItemPrefab UsedResource;
	}
}
