using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Steam;

namespace Barotrauma
{
	// Token: 0x02000141 RID: 321
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct ModListPreset
	{
		// Token: 0x060029B0 RID: 10672 RVA: 0x001CEA14 File Offset: 0x001CCC14
		public ModListPreset(XDocument doc)
		{
			this.Name = doc.Root.GetAttributeString("name", "");
			ModListPreset.<>c__DisplayClass5_0 CS$<>8__locals1;
			CS$<>8__locals1.corePackage = ContentPackageManager.VanillaCorePackage;
			CS$<>8__locals1.regularPackages = new List<RegularPackage>();
			foreach (XElement element in doc.Root.Elements())
			{
				ModListPreset.ModType mt;
				switch (Enum.TryParse<ModListPreset.ModType>(element.Name.LocalName, true, out mt) ? mt : ModListPreset.ModType.Local)
				{
				case ModListPreset.ModType.Vanilla:
					this.CorePackage = ContentPackageManager.VanillaCorePackage;
					break;
				case ModListPreset.ModType.Local:
				{
					string name = element.GetAttributeString("name", "");
					if (!name.IsNullOrEmpty())
					{
						ContentPackage pkg = ContentPackageManager.LocalPackages.FirstOrDefault((ContentPackage p) => p.NameMatches(name));
						if (pkg != null)
						{
							ModListPreset.<.ctor>g__addPkg|5_0(pkg, ref CS$<>8__locals1);
						}
					}
					break;
				}
				case ModListPreset.ModType.Workshop:
				{
					ulong id = element.GetAttributeUInt64("id", 0UL);
					if (id != 0UL)
					{
						ContentPackage pkg2 = ContentPackageManager.WorkshopPackages.FirstOrDefault(delegate(ContentPackage p)
						{
							SteamWorkshopId workshopId;
							return p.TryExtractSteamWorkshopId(out workshopId) && workshopId.Value == id;
						});
						if (pkg2 != null)
						{
							ModListPreset.<.ctor>g__addPkg|5_0(pkg2, ref CS$<>8__locals1);
						}
					}
					break;
				}
				}
			}
			this.CorePackage = CS$<>8__locals1.corePackage;
			this.RegularPackages = CS$<>8__locals1.regularPackages.ToImmutableArray<RegularPackage>();
		}

		// Token: 0x060029B1 RID: 10673 RVA: 0x001CEB98 File Offset: 0x001CCD98
		public ModListPreset(string name, CorePackage corePackage, IReadOnlyList<RegularPackage> regularPackages)
		{
			this.Name = name;
			this.CorePackage = corePackage;
			this.RegularPackages = regularPackages.ToImmutableArray<RegularPackage>();
		}

		// Token: 0x060029B2 RID: 10674 RVA: 0x001CEBB4 File Offset: 0x001CCDB4
		public RichString GetTooltip()
		{
			LocalizedString retVal = "‖color:gui.orange‖" + this.Name + "‖end‖\n  " + TextManager.AddPunctuation(':', new LocalizedString[]
			{
				TextManager.Get("CorePackage")
			}) + "\n   - " + this.CorePackage.Name;
			if (this.RegularPackages.Any<RegularPackage>())
			{
				retVal += "\n  " + TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("RegularPackages")
				}) + "\n   - " + LocalizedString.Join("\n   - ", from p in this.RegularPackages
				select p.Name);
			}
			return RichString.Rich(retVal, null);
		}

		// Token: 0x060029B3 RID: 10675 RVA: 0x001CECAC File Offset: 0x001CCEAC
		public void Save()
		{
			ModListPreset.<>c__DisplayClass8_0 CS$<>8__locals1 = new ModListPreset.<>c__DisplayClass8_0();
			XDocument newDoc = new XDocument();
			CS$<>8__locals1.newRoot = new XElement("mods", new XAttribute("name", this.Name));
			newDoc.Add(CS$<>8__locals1.newRoot);
			CS$<>8__locals1.<Save>g__writePkgElem|1(this.CorePackage);
			this.RegularPackages.ForEach(new Action<RegularPackage>(CS$<>8__locals1.<Save>g__writePkgElem|1));
			if (!Directory.Exists("ModLists"))
			{
				Directory.CreateDirectory("ModLists", false);
			}
			newDoc.SaveSafe(Path.Combine(new string[]
			{
				"ModLists",
				ToolBox.RemoveInvalidFileNameChars(this.Name + ".xml")
			}), SaveOptions.None, false, 0);
		}

		// Token: 0x060029B4 RID: 10676 RVA: 0x001CED70 File Offset: 0x001CCF70
		[CompilerGenerated]
		internal static void <.ctor>g__addPkg|5_0(ContentPackage pkg, ref ModListPreset.<>c__DisplayClass5_0 A_1)
		{
			CorePackage core = pkg as CorePackage;
			if (core != null)
			{
				A_1.corePackage = core;
				return;
			}
			RegularPackage reg = pkg as RegularPackage;
			if (reg != null)
			{
				A_1.regularPackages.Add(reg);
			}
		}

		// Token: 0x060029B5 RID: 10677 RVA: 0x001CEDA5 File Offset: 0x001CCFA5
		[CompilerGenerated]
		internal static ModListPreset.ModType <Save>g__determineType|8_0(ContentPackage pkg)
		{
			if (pkg == ContentPackageManager.VanillaCorePackage)
			{
				return ModListPreset.ModType.Vanilla;
			}
			if (ContentPackageManager.WorkshopPackages.Contains(pkg))
			{
				return ModListPreset.ModType.Workshop;
			}
			return ModListPreset.ModType.Local;
		}

		// Token: 0x040015BD RID: 5565
		public const string SavePath = "ModLists";

		// Token: 0x040015BE RID: 5566
		public readonly string Name;

		// Token: 0x040015BF RID: 5567
		public readonly CorePackage CorePackage;

		// Token: 0x040015C0 RID: 5568
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<RegularPackage> RegularPackages;

		// Token: 0x02000DAB RID: 3499
		[NullableContext(0)]
		public enum ModType
		{
			// Token: 0x04005038 RID: 20536
			Vanilla,
			// Token: 0x04005039 RID: 20537
			Local,
			// Token: 0x0400503A RID: 20538
			Workshop
		}
	}
}
