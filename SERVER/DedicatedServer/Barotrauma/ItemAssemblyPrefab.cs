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

namespace Barotrauma
{
	// Token: 0x02000234 RID: 564
	internal class ItemAssemblyPrefab : MapEntityPrefab
	{
		// Token: 0x17000B16 RID: 2838
		// (get) Token: 0x06002698 RID: 9880 RVA: 0x000FCBBF File Offset: 0x000FADBF
		public override LocalizedString Name { get; }

		// Token: 0x17000B17 RID: 2839
		// (get) Token: 0x06002699 RID: 9881 RVA: 0x000FCBC7 File Offset: 0x000FADC7
		public override Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000B18 RID: 2840
		// (get) Token: 0x0600269A RID: 9882 RVA: 0x000FCBCA File Offset: 0x000FADCA
		public override string OriginalName
		{
			get
			{
				return this.Name.Value;
			}
		}

		// Token: 0x17000B19 RID: 2841
		// (get) Token: 0x0600269B RID: 9883 RVA: 0x000FCBD7 File Offset: 0x000FADD7
		public override ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x17000B1A RID: 2842
		// (get) Token: 0x0600269C RID: 9884 RVA: 0x000FCBDF File Offset: 0x000FADDF
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000B1B RID: 2843
		// (get) Token: 0x0600269D RID: 9885 RVA: 0x000FCBE2 File Offset: 0x000FADE2
		public override MapEntityCategory Category
		{
			get
			{
				return MapEntityCategory.ItemAssembly;
			}
		}

		// Token: 0x17000B1C RID: 2844
		// (get) Token: 0x0600269E RID: 9886 RVA: 0x000FCBE9 File Offset: 0x000FADE9
		public override ImmutableHashSet<string> Aliases
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600269F RID: 9887 RVA: 0x000FCBEC File Offset: 0x000FADEC
		protected override Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("identifier", element.GetAttributeIdentifier("name", ""));
		}

		// Token: 0x060026A0 RID: 9888 RVA: 0x000FCC0C File Offset: 0x000FAE0C
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

		// Token: 0x060026A1 RID: 9889 RVA: 0x000FCFD0 File Offset: 0x000FB1D0
		protected override void CreateInstance(Rectangle rect)
		{
			List<MapEntity> loaded = this.CreateInstance(rect.Location.ToVector2(), Submarine.MainSub, false);
		}

		// Token: 0x060026A2 RID: 9890 RVA: 0x000FCFFC File Offset: 0x000FB1FC
		public List<MapEntity> CreateInstance(Vector2 position, Submarine sub, bool selectInstance = false)
		{
			return ItemAssemblyPrefab.PasteEntities(position, sub, this.configElement, this.ContentFile.Path.Value, selectInstance);
		}

		// Token: 0x060026A3 RID: 9891 RVA: 0x000FD02C File Offset: 0x000FB22C
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
			return entities;
		}

		// Token: 0x060026A4 RID: 9892 RVA: 0x000FD120 File Offset: 0x000FB320
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

		// Token: 0x060026A5 RID: 9893 RVA: 0x000FD1C8 File Offset: 0x000FB3C8
		public override void Dispose()
		{
		}

		// Token: 0x040012DF RID: 4831
		public static readonly PrefabCollection<ItemAssemblyPrefab> Prefabs = new PrefabCollection<ItemAssemblyPrefab>();

		// Token: 0x040012E0 RID: 4832
		private readonly XElement configElement;

		// Token: 0x040012E1 RID: 4833
		public readonly ImmutableArray<ItemAssemblyPrefab.DisplayEntity> DisplayEntities;

		// Token: 0x040012E2 RID: 4834
		public readonly Rectangle Bounds;

		// Token: 0x02000A01 RID: 2561
		public readonly struct DisplayEntity : IEquatable<ItemAssemblyPrefab.DisplayEntity>
		{
			// Token: 0x06005B83 RID: 23427 RVA: 0x001FECD7 File Offset: 0x001FCED7
			public DisplayEntity(Identifier Identifier, Rectangle Rect, float RotationRad)
			{
				this.Identifier = Identifier;
				this.Rect = Rect;
				this.RotationRad = RotationRad;
			}

			// Token: 0x17001578 RID: 5496
			// (get) Token: 0x06005B84 RID: 23428 RVA: 0x001FECEE File Offset: 0x001FCEEE
			// (set) Token: 0x06005B85 RID: 23429 RVA: 0x001FECF6 File Offset: 0x001FCEF6
			public Identifier Identifier { get; set; }

			// Token: 0x17001579 RID: 5497
			// (get) Token: 0x06005B86 RID: 23430 RVA: 0x001FECFF File Offset: 0x001FCEFF
			// (set) Token: 0x06005B87 RID: 23431 RVA: 0x001FED07 File Offset: 0x001FCF07
			public Rectangle Rect { get; set; }

			// Token: 0x1700157A RID: 5498
			// (get) Token: 0x06005B88 RID: 23432 RVA: 0x001FED10 File Offset: 0x001FCF10
			// (set) Token: 0x06005B89 RID: 23433 RVA: 0x001FED18 File Offset: 0x001FCF18
			public float RotationRad { get; set; }

			// Token: 0x06005B8A RID: 23434 RVA: 0x001FED24 File Offset: 0x001FCF24
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

			// Token: 0x06005B8B RID: 23435 RVA: 0x001FED70 File Offset: 0x001FCF70
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

			// Token: 0x06005B8C RID: 23436 RVA: 0x001FEDF3 File Offset: 0x001FCFF3
			[CompilerGenerated]
			public static bool operator !=(ItemAssemblyPrefab.DisplayEntity left, ItemAssemblyPrefab.DisplayEntity right)
			{
				return !(left == right);
			}

			// Token: 0x06005B8D RID: 23437 RVA: 0x001FEDFF File Offset: 0x001FCFFF
			[CompilerGenerated]
			public static bool operator ==(ItemAssemblyPrefab.DisplayEntity left, ItemAssemblyPrefab.DisplayEntity right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005B8E RID: 23438 RVA: 0x001FEE09 File Offset: 0x001FD009
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<Rectangle>.Default.GetHashCode(this.<Rect>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<RotationRad>k__BackingField);
			}

			// Token: 0x06005B8F RID: 23439 RVA: 0x001FEE49 File Offset: 0x001FD049
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemAssemblyPrefab.DisplayEntity && this.Equals((ItemAssemblyPrefab.DisplayEntity)obj);
			}

			// Token: 0x06005B90 RID: 23440 RVA: 0x001FEE64 File Offset: 0x001FD064
			[CompilerGenerated]
			public bool Equals(ItemAssemblyPrefab.DisplayEntity other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<Rectangle>.Default.Equals(this.<Rect>k__BackingField, other.<Rect>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<RotationRad>k__BackingField, other.<RotationRad>k__BackingField);
			}

			// Token: 0x06005B91 RID: 23441 RVA: 0x001FEEB9 File Offset: 0x001FD0B9
			[CompilerGenerated]
			public void Deconstruct(out Identifier Identifier, out Rectangle Rect, out float RotationRad)
			{
				Identifier = this.Identifier;
				Rect = this.Rect;
				RotationRad = this.RotationRad;
			}
		}
	}
}
