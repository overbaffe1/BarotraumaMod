using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F9 RID: 1529
	internal class InheritConditionFromLinkedWall : ItemComponent
	{
		// Token: 0x060063B9 RID: 25529 RVA: 0x0033E2DD File Offset: 0x0033C4DD
		public InheritConditionFromLinkedWall(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060063BA RID: 25530 RVA: 0x0033E2F4 File Offset: 0x0033C4F4
		public override void OnMapLoaded()
		{
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Structure structure = linkedTo as Structure;
				if (structure != null && structure.HasBody)
				{
					this.linkedWalls.Add(structure);
					Structure structure2 = structure;
					structure2.OnHealthChanged = (Structure.OnHealthChangedHandler)Delegate.Combine(structure2.OnHealthChanged, new Structure.OnHealthChangedHandler(delegate(Character _, float _)
					{
						this.UpdateCondition();
					}));
				}
			}
			if (this.linkedWalls.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 3);
				defaultInterpolatedStringHandler.AppendLiteral("The item ");
				defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(") is not linked to any walls with a physics body. The ");
				defaultInterpolatedStringHandler.AppendFormatted("InheritConditionFromLinkedWall");
				defaultInterpolatedStringHandler.AppendLiteral(" component will do nothing.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
		}

		// Token: 0x060063BB RID: 25531 RVA: 0x0033E410 File Offset: 0x0033C610
		private void UpdateCondition()
		{
			float lowestHealthPercent = 1f;
			foreach (Structure wall in this.linkedWalls)
			{
				foreach (WallSection section in wall.Sections)
				{
					lowestHealthPercent = Math.Min(lowestHealthPercent, 1f - section.damage / wall.MaxHealth);
				}
			}
			this.item.Condition = this.item.MaxCondition * lowestHealthPercent;
		}

		// Token: 0x040033B2 RID: 13234
		private readonly List<Structure> linkedWalls = new List<Structure>();
	}
}
