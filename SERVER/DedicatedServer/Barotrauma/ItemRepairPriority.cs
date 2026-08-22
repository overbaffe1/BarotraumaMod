using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020000D1 RID: 209
	internal class ItemRepairPriority : Prefab
	{
		// Token: 0x06001730 RID: 5936 RVA: 0x000C2460 File Offset: 0x000C0660
		public ItemRepairPriority(XElement element, JobsFile file) : base(file, element.GetAttributeIdentifier("tag", Identifier.Empty))
		{
			this.Priority = element.GetAttributeFloat("priority", -1f);
			if (this.Priority < 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The 'priority' attribute is missing from the the item repair priorities definition in ");
				defaultInterpolatedStringHandler.AppendFormatted<XElement>(element);
				defaultInterpolatedStringHandler.AppendLiteral(" of ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), base.ContentPackage);
			}
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x000C24FC File Offset: 0x000C06FC
		public override void Dispose()
		{
		}

		// Token: 0x04000B30 RID: 2864
		public static readonly PrefabCollection<ItemRepairPriority> Prefabs = new PrefabCollection<ItemRepairPriority>();

		// Token: 0x04000B31 RID: 2865
		public readonly float Priority;
	}
}
