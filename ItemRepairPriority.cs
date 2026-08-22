using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001CF RID: 463
	internal class ItemRepairPriority : Prefab
	{
		// Token: 0x0600326D RID: 12909 RVA: 0x00209860 File Offset: 0x00207A60
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

		// Token: 0x0600326E RID: 12910 RVA: 0x002098FC File Offset: 0x00207AFC
		public override void Dispose()
		{
		}

		// Token: 0x04001A74 RID: 6772
		public static readonly PrefabCollection<ItemRepairPriority> Prefabs = new PrefabCollection<ItemRepairPriority>();

		// Token: 0x04001A75 RID: 6773
		public readonly float Priority;
	}
}
