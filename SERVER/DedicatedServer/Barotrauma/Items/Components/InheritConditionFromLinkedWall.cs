using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004CC RID: 1228
	internal class InheritConditionFromLinkedWall : ItemComponent
	{
		// Token: 0x06004613 RID: 17939 RVA: 0x001C0189 File Offset: 0x001BE389
		public InheritConditionFromLinkedWall(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004614 RID: 17940 RVA: 0x001C01A0 File Offset: 0x001BE3A0
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

		// Token: 0x06004615 RID: 17941 RVA: 0x001C02BC File Offset: 0x001BE4BC
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

		// Token: 0x040021AB RID: 8619
		private readonly List<Structure> linkedWalls = new List<Structure>();
	}
}
