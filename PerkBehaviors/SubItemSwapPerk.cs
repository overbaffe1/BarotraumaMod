using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020003B0 RID: 944
	internal class SubItemSwapPerk : PerkBase
	{
		// Token: 0x17001201 RID: 4609
		// (get) Token: 0x060045DC RID: 17884 RVA: 0x0026A067 File Offset: 0x00268267
		// (set) Token: 0x060045DD RID: 17885 RVA: 0x0026A06F File Offset: 0x0026826F
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier TargetItem { get; set; }

		// Token: 0x17001202 RID: 4610
		// (get) Token: 0x060045DE RID: 17886 RVA: 0x0026A078 File Offset: 0x00268278
		// (set) Token: 0x060045DF RID: 17887 RVA: 0x0026A080 File Offset: 0x00268280
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier ReplacementItem { get; set; }

		// Token: 0x17001203 RID: 4611
		// (get) Token: 0x060045E0 RID: 17888 RVA: 0x0026A089 File Offset: 0x00268289
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x060045E1 RID: 17889 RVA: 0x0026A08C File Offset: 0x0026828C
		public SubItemSwapPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x060045E2 RID: 17890 RVA: 0x0026A098 File Offset: 0x00268298
		public override bool CanApply(SubmarineInfo submarine)
		{
			XElement subElement = submarine.SubmarineElement;
			foreach (XElement element in subElement.Elements())
			{
				if (element.Name.ToString().Equals("Item", StringComparison.OrdinalIgnoreCase))
				{
					Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
					Identifier targetItem = this.TargetItem;
					if (identifier == targetItem)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060045E3 RID: 17891 RVA: 0x0026A12C File Offset: 0x0026832C
		public override void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, Submarine teamSubmarine)
		{
			if (teamSubmarine == null)
			{
				return;
			}
			List<Item> items = teamSubmarine.GetItems(true);
			ItemPrefab itemToInstall = ItemPrefab.Find(null, this.ReplacementItem);
			if (itemToInstall == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ReplacementItem);
				defaultInterpolatedStringHandler.AppendLiteral("\" to swap with \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TargetItem);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			foreach (Item item in items)
			{
				Prefab prefab = item.Prefab;
				Identifier targetItem = this.TargetItem;
				if (prefab.Identifier == targetItem)
				{
					item.ReplaceWithLinkedItems(itemToInstall);
					break;
				}
			}
		}
	}
}
