using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000D1 RID: 209
	internal class ItemAssemblyPrefab : MapEntityPrefab
	{
		// Token: 0x06001C18 RID: 7192 RVA: 0x00118908 File Offset: 0x00116B08
		public void DrawIcon(SpriteBatch spriteBatch, GUICustomComponent guiComponent)
		{
			Rectangle drawArea = guiComponent.Rect;
			float scale = Math.Min((float)drawArea.Width / (float)this.Bounds.Width, (float)drawArea.Height / (float)this.Bounds.Height) * 0.9f;
			foreach (ItemAssemblyPrefab.DisplayEntity displayEntity in this.DisplayEntities)
			{
				MapEntityPrefab entityPrefab = MapEntityPrefab.FindByIdentifier(displayEntity.Identifier);
				if (!(entityPrefab is CoreEntityPrefab) && entityPrefab != null)
				{
					Rectangle drawRect = new Rectangle((int)((float)displayEntity.Rect.X * scale) + drawArea.Center.X, (int)((float)displayEntity.Rect.Y * scale) - drawArea.Center.Y, (int)((float)displayEntity.Rect.Width * scale), (int)((float)displayEntity.Rect.Height * scale));
					entityPrefab.DrawPlacing(spriteBatch, drawRect, entityPrefab.Scale * scale, displayEntity.RotationRad, SpriteEffects.None);
				}
			}
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x00118A10 File Offset: 0x00116C10
		public override void DrawPlacing(SpriteBatch spriteBatch, Camera cam)
		{
			base.DrawPlacing(spriteBatch, cam);
			this.Draw(spriteBatch, (MapEntityPrefab.placePosition != Vector2.Zero) ? MapEntityPrefab.placePosition : Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false));
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x00118A5C File Offset: 0x00116C5C
		public void Draw(SpriteBatch spriteBatch, Vector2 pos)
		{
			foreach (ItemAssemblyPrefab.DisplayEntity displayEntity in this.DisplayEntities)
			{
				MapEntityPrefab entityPrefab = MapEntityPrefab.FindByIdentifier(displayEntity.Identifier);
				if (entityPrefab != null)
				{
					Rectangle drawRect = displayEntity.Rect;
					drawRect.Location += pos.ToPoint();
					entityPrefab.DrawPlacing(spriteBatch, drawRect, entityPrefab.Scale, displayEntity.RotationRad, SpriteEffects.None);
				}
			}
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x00118AD0 File Offset: 0x00116CD0
		public static XElement Save(List<MapEntity> entities, string name, string description, bool hideInMenus = false)
		{
			XElement element = new XElement("ItemAssembly", new object[]
			{
				new XAttribute("name", name),
				new XAttribute("description", description),
				new XAttribute("hideinmenus", hideInMenus)
			});
			List<MapEntity> assemblyEntities = MapEntity.CopyEntities(entities);
			int i = 0;
			while (i < assemblyEntities.Count && i < entities.Count)
			{
				assemblyEntities[i].Layer = entities[i].Layer;
				i++;
			}
			List<MapEntity> disabledEntities = new List<MapEntity>();
			foreach (MapEntity mapEntity in assemblyEntities)
			{
				Item item = mapEntity as Item;
				if (item != null)
				{
					Wire wire = item.GetComponent<Wire>();
					if (item.ParentInventory == null)
					{
						if (wire == null)
						{
							continue;
						}
						if (!wire.Connections.Any((Connection c) => c != null))
						{
							continue;
						}
					}
					item.SetTransform(Vector2.Zero, 0f, true, true, null);
					disabledEntities.Add(mapEntity);
				}
			}
			float minX = 2.1474836E+09f;
			float maxX = -2.1474836E+09f;
			float minY = 2.1474836E+09f;
			float maxY = -2.1474836E+09f;
			foreach (MapEntity mapEntity2 in assemblyEntities)
			{
				if (!disabledEntities.Contains(mapEntity2))
				{
					minX = Math.Min(minX, (float)mapEntity2.WorldRect.X);
					maxX = Math.Max(maxX, (float)mapEntity2.WorldRect.Right);
					minY = Math.Min(minY, (float)(mapEntity2.WorldRect.Y - mapEntity2.WorldRect.Height));
					maxY = Math.Max(maxY, (float)mapEntity2.WorldRect.Y);
				}
			}
			Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
			if (Submarine.MainSub != null)
			{
				center -= Submarine.MainSub.HiddenSubPosition;
			}
			Vector2 offsetFromGrid = new Vector2(MathUtils.RoundTowardsClosest(center.X, Submarine.GridSize.X) - center.X, MathUtils.RoundTowardsClosest(center.Y, Submarine.GridSize.Y) - center.Y - Submarine.GridSize.Y / 2f);
			MapEntity.SelectedList.Clear();
			assemblyEntities.ForEach(delegate(MapEntity e)
			{
				MapEntity.AddSelection(e);
			});
			foreach (MapEntity mapEntity3 in assemblyEntities)
			{
				mapEntity3.Move(-center - offsetFromGrid, true);
				mapEntity3.Submarine = Submarine.MainSub;
				XElement entityElement = mapEntity3.Save(element);
				if (disabledEntities.Contains(mapEntity3))
				{
					entityElement.Add(new XAttribute("hideinassemblypreview", "true"));
				}
			}
			MapEntity.SelectedList.Clear();
			entities.ForEach(delegate(MapEntity e)
			{
				MapEntity.AddSelection(e);
			});
			return element;
		}

		// Token: 0x1700073B RID: 1851
		// (get) Token: 0x06001C1C RID: 7196 RVA: 0x00118E58 File Offset: 0x00117058
		public override LocalizedString Name { get; }

		// Token: 0x1700073C RID: 1852
		// (get) Token: 0x06001C1D RID: 7197 RVA: 0x00118E60 File Offset: 0x00117060
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700073D RID: 1853
		// (get) Token: 0x06001C1E RID: 7198 RVA: 0x00118E63 File Offset: 0x00117063
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x1700073E RID: 1854
		// (get) Token: 0x06001C1F RID: 7199 RVA: 0x00118E70 File Offset: 0x00117070
		public override ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x1700073F RID: 1855
		// (get) Token: 0x06001C20 RID: 7200 RVA: 0x00118E78 File Offset: 0x00117078
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000740 RID: 1856
		// (get) Token: 0x06001C21 RID: 7201 RVA: 0x00118E7B File Offset: 0x0011707B
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.ItemAssembly;
			}
		}

		// Token: 0x17000741 RID: 1857
		// (get) Token: 0x06001C22 RID: 7202 RVA: 0x00118E82 File Offset: 0x00117082
		public override ImmutableHashSet<string> Aliases
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x00118E85 File Offset: 0x00117085
		protected override Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("identifier", element.GetAttributeIdentifier("name", ""));
		}

		// Token: 0x06001C24 RID: 7204 RVA: 0x00118EA4 File Offset: 0x001170A4
		public ItemAssemblyPrefab(ContentXElement element, ItemAssemblyFile file) : base(element, file)
		{
			this.configElement = element;
			SerializableProperty.DeserializeProperties(this, this.configElement);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("EntityName.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(element.GetAttributeString("name", ""), true);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("EntityDescription.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
			base.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			this.Tags = Enumerable.Empty<Identifier>().ToImmutableHashSet<Identifier>();
			string description = element.GetAttributeString("description", string.Empty);
			if (!description.IsNullOrEmpty())
			{
				base.Description = base.Description.Fallback(description, true);
			}
			List<ushort> containedItemIDs = new List<ushort>();
			foreach (ContentXElement cxe in element.Elements())
			{
				XElement entityElement = cxe;
				XElement containerElement = entityElement.GetChildElement("itemcontainer", StringComparison.OrdinalIgnoreCase);
				if (containerElement != null)
				{
					string containedString = containerElement.GetAttributeString("contained", "");
					string[] itemIdStrings = containedString.Split(',', StringSplitOptions.None);
					List<ushort>[] itemIds = new List<ushort>[itemIdStrings.Length];
					for (int i = 0; i < itemIdStrings.Length; i++)
					{
						List<ushort>[] array = itemIds;
						int num = i;
						if (array[num] == null)
						{
							array[num] = new List<ushort>();
						}
						foreach (string idStr in itemIdStrings[i].Split(';', StringSplitOptions.None))
						{
							int id;
							if (int.TryParse(idStr, out id))
							{
								itemIds[i].Add((ushort)id);
								containedItemIDs.Add((ushort)id);
							}
						}
					}
				}
			}
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			List<ItemAssemblyPrefab.DisplayEntity> displayEntities = new List<ItemAssemblyPrefab.DisplayEntity>();
			foreach (ContentXElement cxe2 in element.Elements())
			{
				XElement entityElement2 = cxe2;
				ushort id2 = (ushort)entityElement2.GetAttributeInt("ID", 0);
				if (id2 <= 0 || !containedItemIDs.Contains(id2))
				{
					if (!entityElement2.Elements().Any((XElement e) => e.Name.LocalName.Equals("wire", StringComparison.OrdinalIgnoreCase)))
					{
						Identifier identifier = entityElement2.GetAttributeIdentifier("identifier", entityElement2.Name.ToString().ToLowerInvariant());
						Rectangle rect = entityElement2.GetAttributeRect("rect", Rectangle.Empty);
						float scale = entityElement2.GetAttributeFloat("scale", 1f);
						float rotation = MathHelper.ToRadians(entityElement2.GetAttributeFloat("rotation", 0f));
						if (!entityElement2.GetAttributeBool("hideinassemblypreview", false))
						{
							displayEntities.Add(new ItemAssemblyPrefab.DisplayEntity(identifier, rect, rotation));
						}
						minX = Math.Min(minX, rect.X);
						minY = Math.Min(minY, rect.Y - rect.Height);
						maxX = Math.Max(maxX, rect.Right);
						maxY = Math.Max(maxY, rect.Y);
					}
				}
			}
			this.DisplayEntities = displayEntities.ToImmutableArray<ItemAssemblyPrefab.DisplayEntity>();
			this.Bounds = ((minX == int.MaxValue) ? new Rectangle(0, 0, 1, 1) : new Rectangle(minX, minY, maxX - minX, maxY - minY));
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x00119268 File Offset: 0x00117468
		protected override void CreateInstance(Rectangle rect)
		{
			List<MapEntity> loaded = this.CreateInstance(rect.Location.ToVector2(), Submarine.MainSub, Screen.Selected == GameMain.SubEditorScreen);
			if (Screen.Selected is SubEditorScreen && loaded.Any<MapEntity>())
			{
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(loaded, false, false));
			}
		}

		// Token: 0x06001C26 RID: 7206 RVA: 0x001192C0 File Offset: 0x001174C0
		public List<MapEntity> CreateInstance(Vector2 position, Submarine sub, bool selectInstance = false)
		{
			List<MapEntity> retVal = ItemAssemblyPrefab.PasteEntities(position, sub, this.configElement, this.ContentFile.Path.Value, selectInstance);
			SubEditorScreen subEditorScreen = GameMain.SubEditorScreen;
			if (subEditorScreen != null)
			{
				subEditorScreen.ReconstructLayers();
			}
			return retVal;
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x00119300 File Offset: 0x00117500
		public static List<MapEntity> PasteEntities(Vector2 position, Submarine sub, XElement configElement, string filePath = null, bool selectInstance = false)
		{
			int idOffset = Entity.FindFreeIdBlock(configElement.Elements().Count<XElement>());
			List<MapEntity> entities = MapEntity.LoadAll(sub, configElement, filePath, idOffset);
			if (entities.Count == 0)
			{
				return entities;
			}
			Vector2 offset = (sub != null) ? sub.HiddenSubPosition : Vector2.Zero;
			foreach (MapEntity me in entities)
			{
				me.Move(position, true);
				me.Submarine = sub;
				Item item = me as Item;
				if (item != null)
				{
					Wire wire = item.GetComponent<Wire>();
					if (wire != null)
					{
						if (sub != null && Vector2.Distance(me.Position, sub.HiddenSubPosition) > sub.HiddenSubPosition.Length() / 2f)
						{
							me.Move(position, true);
						}
						wire.MoveNodes(position - offset);
					}
				}
			}
			MapEntity.MapLoaded(entities, true);
			if (Screen.Selected == GameMain.SubEditorScreen && selectInstance)
			{
				MapEntity.SelectedList.Clear();
				List<MapEntity> list = entities;
				Action<MapEntity> action;
				if ((action = ItemAssemblyPrefab.<>O.<0>__AddSelection) == null)
				{
					action = (ItemAssemblyPrefab.<>O.<0>__AddSelection = new Action<MapEntity>(MapEntity.AddSelection));
				}
				list.ForEach(action);
			}
			return entities;
		}

		// Token: 0x06001C28 RID: 7208 RVA: 0x00119430 File Offset: 0x00117630
		public void Delete()
		{
			ItemAssemblyPrefab.Prefabs.Remove(this);
			try
			{
				ContentPackage contentPackage = base.ContentPackage;
				if (contentPackage != null && contentPackage.Files.Length == 1 && ContentPackageManager.LocalPackages.Contains(base.ContentPackage))
				{
					Directory.Delete(base.ContentPackage.Dir, true, false);
					ContentPackageManager.LocalPackages.Refresh();
					ContentPackageManager.EnabledPackages.DisableRemovedMods();
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowErrorLocalized("Deleting item assembly \"" + this.Name + "\" failed.", e, null, false, false);
			}
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x001194D8 File Offset: 0x001176D8
		public override void Dispose()
		{
		}

		// Token: 0x04000E6B RID: 3691
		public static readonly PrefabCollection<ItemAssemblyPrefab> Prefabs = new PrefabCollection<ItemAssemblyPrefab>();

		// Token: 0x04000E6C RID: 3692
		private readonly XElement configElement;

		// Token: 0x04000E6D RID: 3693
		public readonly ImmutableArray<ItemAssemblyPrefab.DisplayEntity> DisplayEntities;

		// Token: 0x04000E6E RID: 3694
		public readonly Rectangle Bounds;

		// Token: 0x02000AD1 RID: 2769
		public readonly struct DisplayEntity : IEquatable<ItemAssemblyPrefab.DisplayEntity>
		{
			// Token: 0x06007675 RID: 30325 RVA: 0x0037898B File Offset: 0x00376B8B
			public DisplayEntity(Identifier Identifier, Rectangle Rect, float RotationRad)
			{
				this.Identifier = Identifier;
				this.Rect = Rect;
				this.RotationRad = RotationRad;
			}

			// Token: 0x17001AA5 RID: 6821
			// (get) Token: 0x06007676 RID: 30326 RVA: 0x003789A2 File Offset: 0x00376BA2
			// (set) Token: 0x06007677 RID: 30327 RVA: 0x003789AA File Offset: 0x00376BAA
			public Identifier Identifier { get; set; }

			// Token: 0x17001AA6 RID: 6822
			// (get) Token: 0x06007678 RID: 30328 RVA: 0x003789B3 File Offset: 0x00376BB3
			// (set) Token: 0x06007679 RID: 30329 RVA: 0x003789BB File Offset: 0x00376BBB
			public Rectangle Rect { get; set; }

			// Token: 0x17001AA7 RID: 6823
			// (get) Token: 0x0600767A RID: 30330 RVA: 0x003789C4 File Offset: 0x00376BC4
			// (set) Token: 0x0600767B RID: 30331 RVA: 0x003789CC File Offset: 0x00376BCC
			public float RotationRad { get; set; }

			// Token: 0x0600767C RID: 30332 RVA: 0x003789D8 File Offset: 0x00376BD8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("DisplayEntity");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600767D RID: 30333 RVA: 0x00378A24 File Offset: 0x00376C24
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Identifier = ");
				builder.Append(this.Identifier.ToString());
				builder.Append(", Rect = ");
				builder.Append(this.Rect.ToString());
				builder.Append(", RotationRad = ");
				builder.Append(this.RotationRad.ToString());
				return true;
			}

			// Token: 0x0600767E RID: 30334 RVA: 0x00378AA7 File Offset: 0x00376CA7
			[CompilerGenerated]
			public static bool operator !=(ItemAssemblyPrefab.DisplayEntity left, ItemAssemblyPrefab.DisplayEntity right)
			{
				return !(left == right);
			}

			// Token: 0x0600767F RID: 30335 RVA: 0x00378AB3 File Offset: 0x00376CB3
			[CompilerGenerated]
			public static bool operator ==(ItemAssemblyPrefab.DisplayEntity left, ItemAssemblyPrefab.DisplayEntity right)
			{
				return left.Equals(right);
			}

			// Token: 0x06007680 RID: 30336 RVA: 0x00378ABD File Offset: 0x00376CBD
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<Rectangle>.Default.GetHashCode(this.<Rect>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<RotationRad>k__BackingField);
			}

			// Token: 0x06007681 RID: 30337 RVA: 0x00378AFD File Offset: 0x00376CFD
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemAssemblyPrefab.DisplayEntity && this.Equals((ItemAssemblyPrefab.DisplayEntity)obj);
			}

			// Token: 0x06007682 RID: 30338 RVA: 0x00378B18 File Offset: 0x00376D18
			[CompilerGenerated]
			public bool Equals(ItemAssemblyPrefab.DisplayEntity other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<Rectangle>.Default.Equals(this.<Rect>k__BackingField, other.<Rect>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<RotationRad>k__BackingField, other.<RotationRad>k__BackingField);
			}

			// Token: 0x06007683 RID: 30339 RVA: 0x00378B6D File Offset: 0x00376D6D
			[CompilerGenerated]
			public void Deconstruct(out Identifier Identifier, out Rectangle Rect, out float RotationRad)
			{
				Identifier = this.Identifier;
				Rect = this.Rect;
				RotationRad = this.RotationRad;
			}
		}

		// Token: 0x02000AD2 RID: 2770
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040045A6 RID: 17830
			public static Action<MapEntity> <0>__AddSelection;
		}
	}
}
