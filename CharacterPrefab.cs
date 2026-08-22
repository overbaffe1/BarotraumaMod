using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C2 RID: 450
	internal class CharacterPrefab : PrefabWithUintIdentifier, IImplementsVariants<CharacterPrefab>
	{
		// Token: 0x06003176 RID: 12662 RVA: 0x00204638 File Offset: 0x00202838
		public override void Dispose()
		{
			Character.RemoveByPrefab(this);
		}

		// Token: 0x17000CF6 RID: 3318
		// (get) Token: 0x06003177 RID: 12663 RVA: 0x00204640 File Offset: 0x00202840
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000CF7 RID: 3319
		// (get) Token: 0x06003178 RID: 12664 RVA: 0x0020464D File Offset: 0x0020284D
		public Identifier VariantOf { get; }

		// Token: 0x17000CF8 RID: 3320
		// (get) Token: 0x06003179 RID: 12665 RVA: 0x00204655 File Offset: 0x00202855
		// (set) Token: 0x0600317A RID: 12666 RVA: 0x0020465D File Offset: 0x0020285D
		public CharacterPrefab ParentPrefab { get; set; }

		// Token: 0x0600317B RID: 12667 RVA: 0x00204668 File Offset: 0x00202868
		public Identifier GetBaseCharacterSpeciesName(Identifier speciesName)
		{
			if (!this.VariantOf.IsEmpty)
			{
				speciesName = this.VariantOf;
				CharacterPrefab parentPrefab = this.ParentPrefab;
				if (parentPrefab != null && !parentPrefab.VariantOf.IsEmpty)
				{
					speciesName = parentPrefab.GetBaseCharacterSpeciesName(speciesName);
				}
			}
			return speciesName;
		}

		// Token: 0x17000CF9 RID: 3321
		// (get) Token: 0x0600317C RID: 12668 RVA: 0x002046B1 File Offset: 0x002028B1
		// (set) Token: 0x0600317D RID: 12669 RVA: 0x002046B9 File Offset: 0x002028B9
		public bool HasCharacterInfo { get; private set; }

		// Token: 0x17000CFA RID: 3322
		// (get) Token: 0x0600317E RID: 12670 RVA: 0x002046C2 File Offset: 0x002028C2
		// (set) Token: 0x0600317F RID: 12671 RVA: 0x002046CA File Offset: 0x002028CA
		public Identifier Group { get; private set; }

		// Token: 0x06003180 RID: 12672 RVA: 0x002046D4 File Offset: 0x002028D4
		public bool MatchesSpeciesNameOrGroup(Identifier speciesNameOrGroup)
		{
			if (!(this.Identifier == speciesNameOrGroup))
			{
				Identifier group = this.Group;
				return group == speciesNameOrGroup;
			}
			return true;
		}

		// Token: 0x06003181 RID: 12673 RVA: 0x00204702 File Offset: 0x00202902
		public void InheritFrom(CharacterPrefab parent)
		{
			this.ConfigElement = CharacterParams.CreateVariantXml(this.originalElement, parent.ConfigElement).FromPackage(this.ConfigElement.ContentPackage);
			this.ParseConfigElement();
		}

		// Token: 0x06003182 RID: 12674 RVA: 0x00204734 File Offset: 0x00202934
		private void ParseConfigElement()
		{
			ContentXElement headsElement = this.ConfigElement.GetChildElement("Heads");
			ContentXElement varsElement = this.ConfigElement.GetChildElement("Vars");
			ContentXElement menuCategoryElement = this.ConfigElement.GetChildElement("MenuCategory");
			ContentXElement pronounsElement = this.ConfigElement.GetChildElement("Pronouns");
			ContentXElement contentXElement = null;
			this.HasCharacterInfo = (headsElement != contentXElement || this.ConfigElement.GetAttributeBool("HasCharacterInfo", false));
			if (this.HasCharacterInfo)
			{
				this.CharacterInfoPrefab = new CharacterInfoPrefab(this, headsElement, varsElement, menuCategoryElement, pronounsElement);
			}
			this.Group = this.ConfigElement.GetAttributeIdentifier("Group", Identifier.Empty);
		}

		// Token: 0x17000CFB RID: 3323
		// (get) Token: 0x06003183 RID: 12675 RVA: 0x002047EF File Offset: 0x002029EF
		// (set) Token: 0x06003184 RID: 12676 RVA: 0x002047F7 File Offset: 0x002029F7
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x17000CFC RID: 3324
		// (get) Token: 0x06003185 RID: 12677 RVA: 0x00204800 File Offset: 0x00202A00
		// (set) Token: 0x06003186 RID: 12678 RVA: 0x00204808 File Offset: 0x00202A08
		public CharacterInfoPrefab CharacterInfoPrefab { get; private set; }

		// Token: 0x17000CFD RID: 3325
		// (get) Token: 0x06003187 RID: 12679 RVA: 0x00204811 File Offset: 0x00202A11
		public static IEnumerable<ContentXElement> ConfigElements
		{
			get
			{
				return from p in CharacterPrefab.Prefabs
				select p.ConfigElement;
			}
		}

		// Token: 0x17000CFE RID: 3326
		// (get) Token: 0x06003188 RID: 12680 RVA: 0x0020483C File Offset: 0x00202A3C
		public static CharacterFile HumanConfigFile
		{
			get
			{
				return CharacterPrefab.HumanPrefab.ContentFile as CharacterFile;
			}
		}

		// Token: 0x17000CFF RID: 3327
		// (get) Token: 0x06003189 RID: 12681 RVA: 0x0020484D File Offset: 0x00202A4D
		public static CharacterPrefab HumanPrefab
		{
			get
			{
				return CharacterPrefab.FindBySpeciesName(CharacterPrefab.HumanSpeciesName);
			}
		}

		// Token: 0x0600318A RID: 12682 RVA: 0x00204859 File Offset: 0x00202A59
		public static CharacterPrefab FindBySpeciesName(Identifier speciesName)
		{
			if (!CharacterPrefab.Prefabs.ContainsKey(speciesName))
			{
				return null;
			}
			return CharacterPrefab.Prefabs[speciesName];
		}

		// Token: 0x0600318B RID: 12683 RVA: 0x00204878 File Offset: 0x00202A78
		public static CharacterPrefab FindByFilePath(string filePath)
		{
			return CharacterPrefab.Prefabs.Find((CharacterPrefab p) => p.ContentFile.Path == filePath);
		}

		// Token: 0x0600318C RID: 12684 RVA: 0x002048A8 File Offset: 0x00202AA8
		public static CharacterPrefab Find(Predicate<CharacterPrefab> predicate)
		{
			return CharacterPrefab.Prefabs.Find(predicate);
		}

		// Token: 0x0600318D RID: 12685 RVA: 0x002048B5 File Offset: 0x00202AB5
		public CharacterPrefab(ContentXElement mainElement, CharacterFile file) : base(file, CharacterPrefab.ParseName(mainElement, file))
		{
			this.originalElement = mainElement;
			this.ConfigElement = mainElement;
			this.VariantOf = mainElement.VariantOf();
			this.ParseConfigElement();
		}

		// Token: 0x0600318E RID: 12686 RVA: 0x002048EC File Offset: 0x00202AEC
		public static Identifier ParseName(XElement element, CharacterFile file)
		{
			string name = element.GetAttributeString("name", null);
			if (!string.IsNullOrEmpty(name))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
				defaultInterpolatedStringHandler.AppendLiteral(": 'name' is deprecated! Use 'speciesname' instead.");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Orange), false);
			}
			else
			{
				name = element.GetAttributeString("speciesname", string.Empty);
			}
			return new Identifier(name);
		}

		// Token: 0x0600318F RID: 12687 RVA: 0x00204970 File Offset: 0x00202B70
		public static bool CheckSpeciesName(XElement mainElement, CharacterFile file, out Identifier name)
		{
			name = CharacterPrefab.ParseName(mainElement, file);
			if (name == Identifier.Empty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No species name defined for: ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, file.ContentPackage, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x040019C3 RID: 6595
		public static readonly PrefabCollection<CharacterPrefab> Prefabs = new PrefabCollection<CharacterPrefab>();

		// Token: 0x040019C8 RID: 6600
		private readonly ContentXElement originalElement;

		// Token: 0x040019CB RID: 6603
		public static readonly Identifier HumanSpeciesName = "human".ToIdentifier();

		// Token: 0x040019CC RID: 6604
		public static readonly Identifier HumanGroup = "human".ToIdentifier();
	}
}
