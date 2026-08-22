using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020002EA RID: 746
	internal class SubItemSwapPerk : PerkBase
	{
		// Token: 0x17000E11 RID: 3601
		// (get) Token: 0x060031A2 RID: 12706 RVA: 0x001521E8 File Offset: 0x001503E8
		// (set) Token: 0x060031A3 RID: 12707 RVA: 0x001521F0 File Offset: 0x001503F0
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier TargetItem { get; set; }

		// Token: 0x17000E12 RID: 3602
		// (get) Token: 0x060031A4 RID: 12708 RVA: 0x001521F9 File Offset: 0x001503F9
		// (set) Token: 0x060031A5 RID: 12709 RVA: 0x00152201 File Offset: 0x00150401
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier ReplacementItem { get; set; }

		// Token: 0x17000E13 RID: 3603
		// (get) Token: 0x060031A6 RID: 12710 RVA: 0x0015220A File Offset: 0x0015040A
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x060031A7 RID: 12711 RVA: 0x0015220D File Offset: 0x0015040D
		public SubItemSwapPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x060031A8 RID: 12712 RVA: 0x00152218 File Offset: 0x00150418
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

		// Token: 0x060031A9 RID: 12713 RVA: 0x001522AC File Offset: 0x001504AC
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
