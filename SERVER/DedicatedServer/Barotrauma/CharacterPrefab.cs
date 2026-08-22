using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C1 RID: 193
	internal class CharacterPrefab : PrefabWithUintIdentifier, IImplementsVariants<CharacterPrefab>
	{
		// Token: 0x060015B6 RID: 5558 RVA: 0x000B9830 File Offset: 0x000B7A30
		public override void Dispose()
		{
			Character.RemoveByPrefab(this);
		}

		// Token: 0x17000650 RID: 1616
		// (get) Token: 0x060015B7 RID: 5559 RVA: 0x000B9838 File Offset: 0x000B7A38
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000651 RID: 1617
		// (get) Token: 0x060015B8 RID: 5560 RVA: 0x000B9845 File Offset: 0x000B7A45
		public Identifier VariantOf { get; }

		// Token: 0x17000652 RID: 1618
		// (get) Token: 0x060015B9 RID: 5561 RVA: 0x000B984D File Offset: 0x000B7A4D
		// (set) Token: 0x060015BA RID: 5562 RVA: 0x000B9855 File Offset: 0x000B7A55
		public CharacterPrefab ParentPrefab { get; set; }

		// Token: 0x060015BB RID: 5563 RVA: 0x000B9860 File Offset: 0x000B7A60
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

		// Token: 0x17000653 RID: 1619
		// (get) Token: 0x060015BC RID: 5564 RVA: 0x000B98A9 File Offset: 0x000B7AA9
		// (set) Token: 0x060015BD RID: 5565 RVA: 0x000B98B1 File Offset: 0x000B7AB1
		public bool HasCharacterInfo { get; private set; }

		// Token: 0x17000654 RID: 1620
		// (get) Token: 0x060015BE RID: 5566 RVA: 0x000B98BA File Offset: 0x000B7ABA
		// (set) Token: 0x060015BF RID: 5567 RVA: 0x000B98C2 File Offset: 0x000B7AC2
		public Identifier Group { get; private set; }

		// Token: 0x060015C0 RID: 5568 RVA: 0x000B98CC File Offset: 0x000B7ACC
		public bool MatchesSpeciesNameOrGroup(Identifier speciesNameOrGroup)
		{
			if (!(this.Identifier == speciesNameOrGroup))
			{
				Identifier group = this.Group;
				return group == speciesNameOrGroup;
			}
			return true;
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x000B98FA File Offset: 0x000B7AFA
		public void InheritFrom(CharacterPrefab parent)
		{
			this.ConfigElement = CharacterParams.CreateVariantXml(this.originalElement, parent.ConfigElement).FromPackage(this.ConfigElement.ContentPackage);
			this.ParseConfigElement();
		}

		// Token: 0x060015C2 RID: 5570 RVA: 0x000B992C File Offset: 0x000B7B2C
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

		// Token: 0x17000655 RID: 1621
		// (get) Token: 0x060015C3 RID: 5571 RVA: 0x000B99E7 File Offset: 0x000B7BE7
		// (set) Token: 0x060015C4 RID: 5572 RVA: 0x000B99EF File Offset: 0x000B7BEF
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x17000656 RID: 1622
		// (get) Token: 0x060015C5 RID: 5573 RVA: 0x000B99F8 File Offset: 0x000B7BF8
		// (set) Token: 0x060015C6 RID: 5574 RVA: 0x000B9A00 File Offset: 0x000B7C00
		public CharacterInfoPrefab CharacterInfoPrefab { get; private set; }

		// Token: 0x17000657 RID: 1623
		// (get) Token: 0x060015C7 RID: 5575 RVA: 0x000B9A09 File Offset: 0x000B7C09
		public static IEnumerable<ContentXElement> ConfigElements
		{
			get
			{
				return from p in CharacterPrefab.Prefabs
				select p.ConfigElement;
			}
		}

		// Token: 0x17000658 RID: 1624
		// (get) Token: 0x060015C8 RID: 5576 RVA: 0x000B9A34 File Offset: 0x000B7C34
		public static CharacterFile HumanConfigFile
		{
			get
			{
				return CharacterPrefab.HumanPrefab.ContentFile as CharacterFile;
			}
		}

		// Token: 0x17000659 RID: 1625
		// (get) Token: 0x060015C9 RID: 5577 RVA: 0x000B9A45 File Offset: 0x000B7C45
		public static CharacterPrefab HumanPrefab
		{
			get
			{
				return CharacterPrefab.FindBySpeciesName(CharacterPrefab.HumanSpeciesName);
			}
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x000B9A51 File Offset: 0x000B7C51
		public static CharacterPrefab FindBySpeciesName(Identifier speciesName)
		{
			if (!CharacterPrefab.Prefabs.ContainsKey(speciesName))
			{
				return null;
			}
			return CharacterPrefab.Prefabs[speciesName];
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x000B9A70 File Offset: 0x000B7C70
		public static CharacterPrefab FindByFilePath(string filePath)
		{
			return CharacterPrefab.Prefabs.Find((CharacterPrefab p) => p.ContentFile.Path == filePath);
		}

		// Token: 0x060015CC RID: 5580 RVA: 0x000B9AA0 File Offset: 0x000B7CA0
		public static CharacterPrefab Find(Predicate<CharacterPrefab> predicate)
		{
			return CharacterPrefab.Prefabs.Find(predicate);
		}

		// Token: 0x060015CD RID: 5581 RVA: 0x000B9AAD File Offset: 0x000B7CAD
		public CharacterPrefab(ContentXElement mainElement, CharacterFile file) : base(file, CharacterPrefab.ParseName(mainElement, file))
		{
			this.originalElement = mainElement;
			this.ConfigElement = mainElement;
			this.VariantOf = mainElement.VariantOf();
			this.ParseConfigElement();
		}

		// Token: 0x060015CE RID: 5582 RVA: 0x000B9AE4 File Offset: 0x000B7CE4
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

		// Token: 0x060015CF RID: 5583 RVA: 0x000B9B68 File Offset: 0x000B7D68
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

		// Token: 0x04000A58 RID: 2648
		public static readonly PrefabCollection<CharacterPrefab> Prefabs = new PrefabCollection<CharacterPrefab>();

		// Token: 0x04000A5D RID: 2653
		private readonly ContentXElement originalElement;

		// Token: 0x04000A60 RID: 2656
		public static readonly Identifier HumanSpeciesName = "human".ToIdentifier();

		// Token: 0x04000A61 RID: 2657
		public static readonly Identifier HumanGroup = "human".ToIdentifier();
	}
}
