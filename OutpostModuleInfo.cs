using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200032E RID: 814
	internal class OutpostModuleInfo : ISerializableEntity
	{
		// Token: 0x17001148 RID: 4424
		// (get) Token: 0x0600410A RID: 16650 RVA: 0x00243938 File Offset: 0x00241B38
		public IEnumerable<Identifier> ModuleFlags
		{
			get
			{
				return this.moduleFlags;
			}
		}

		// Token: 0x17001149 RID: 4425
		// (get) Token: 0x0600410B RID: 16651 RVA: 0x00243940 File Offset: 0x00241B40
		public IEnumerable<Identifier> AllowAttachToModules
		{
			get
			{
				return this.allowAttachToModules;
			}
		}

		// Token: 0x1700114A RID: 4426
		// (get) Token: 0x0600410C RID: 16652 RVA: 0x00243948 File Offset: 0x00241B48
		public IEnumerable<Identifier> AllowedLocationTypes
		{
			get
			{
				return this.allowedLocationTypes;
			}
		}

		// Token: 0x1700114B RID: 4427
		// (get) Token: 0x0600410D RID: 16653 RVA: 0x00243950 File Offset: 0x00241B50
		// (set) Token: 0x0600410E RID: 16654 RVA: 0x00243958 File Offset: 0x00241B58
		[Serialize(100, IsPropertySaveable.Yes, "How many instances of this module can be used in one outpost.", "", false)]
		[Editable]
		public int MaxCount { get; set; }

		// Token: 0x1700114C RID: 4428
		// (get) Token: 0x0600410F RID: 16655 RVA: 0x00243961 File Offset: 0x00241B61
		// (set) Token: 0x06004110 RID: 16656 RVA: 0x00243969 File Offset: 0x00241B69
		[Serialize(10f, IsPropertySaveable.Yes, "How likely it is for the module to get picked when selecting from a set of modules during the outpost generation.", "", false)]
		[Editable]
		public float Commonness { get; set; }

		// Token: 0x1700114D RID: 4429
		// (get) Token: 0x06004111 RID: 16657 RVA: 0x00243972 File Offset: 0x00241B72
		// (set) Token: 0x06004112 RID: 16658 RVA: 0x0024397A File Offset: 0x00241B7A
		[Serialize(OutpostModuleInfo.GapPosition.None, IsPropertySaveable.Yes, "Which sides of the module have gaps on them (i.e. from which sides the module can be attached to other modules). Center = no gaps available.", "", false)]
		public OutpostModuleInfo.GapPosition GapPositions { get; set; }

		// Token: 0x1700114E RID: 4430
		// (get) Token: 0x06004113 RID: 16659 RVA: 0x00243983 File Offset: 0x00241B83
		// (set) Token: 0x06004114 RID: 16660 RVA: 0x0024398B File Offset: 0x00241B8B
		[Serialize(OutpostModuleInfo.GapPosition.Right | OutpostModuleInfo.GapPosition.Left | OutpostModuleInfo.GapPosition.Top | OutpostModuleInfo.GapPosition.Bottom, IsPropertySaveable.Yes, "Which sides of this module are allowed to attach to the previously placed module. E.g. if you want a module to always attach to the left side of the docking module, you could set this to Right.", "", false)]
		public OutpostModuleInfo.GapPosition CanAttachToPrevious { get; set; }

		// Token: 0x1700114F RID: 4431
		// (get) Token: 0x06004115 RID: 16661 RVA: 0x00243994 File Offset: 0x00241B94
		// (set) Token: 0x06004116 RID: 16662 RVA: 0x0024399C File Offset: 0x00241B9C
		public string Name { get; private set; }

		// Token: 0x17001150 RID: 4432
		// (get) Token: 0x06004117 RID: 16663 RVA: 0x002439A5 File Offset: 0x00241BA5
		// (set) Token: 0x06004118 RID: 16664 RVA: 0x002439AD File Offset: 0x00241BAD
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06004119 RID: 16665 RVA: 0x002439B8 File Offset: 0x00241BB8
		public OutpostModuleInfo(SubmarineInfo submarineInfo, XElement element)
		{
			this.Name = "OutpostModuleInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.SetFlags(element.GetAttributeIdentifierArray("flags", null, true) ?? element.GetAttributeIdentifierArray("moduletypes", Array.Empty<Identifier>(), true));
			this.SetAllowAttachTo(element.GetAttributeIdentifierArray("allowattachto", Array.Empty<Identifier>(), true));
			this.allowedLocationTypes = new HashSet<Identifier>(element.GetAttributeIdentifierArray("allowedlocationtypes", Array.Empty<Identifier>(), true));
		}

		// Token: 0x0600411A RID: 16666 RVA: 0x00243A70 File Offset: 0x00241C70
		public OutpostModuleInfo(SubmarineInfo submarineInfo)
		{
			this.Name = "OutpostModuleInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
		}

		// Token: 0x0600411B RID: 16667 RVA: 0x00243ACC File Offset: 0x00241CCC
		public OutpostModuleInfo(OutpostModuleInfo original)
		{
			this.Name = original.Name;
			this.moduleFlags = new HashSet<Identifier>(original.moduleFlags);
			this.allowAttachToModules = new HashSet<Identifier>(original.allowAttachToModules);
			this.allowedLocationTypes = new HashSet<Identifier>(original.allowedLocationTypes);
			this.SerializableProperties = new Dictionary<Identifier, SerializableProperty>();
			this.GapPositions = original.GapPositions;
			foreach (KeyValuePair<Identifier, SerializableProperty> kvp in original.SerializableProperties)
			{
				this.SerializableProperties.Add(kvp.Key, kvp.Value);
				if (SerializableProperty.GetSupportedTypeName(kvp.Value.PropertyType) != null)
				{
					kvp.Value.TrySetValue(this, kvp.Value.GetValue(original));
				}
			}
		}

		// Token: 0x0600411C RID: 16668 RVA: 0x00243BDC File Offset: 0x00241DDC
		public void SetFlags(IEnumerable<Identifier> newFlags)
		{
			this.moduleFlags.Clear();
			if (newFlags.Contains("hallwayhorizontal".ToIdentifier()))
			{
				this.moduleFlags.Add("hallwayhorizontal".ToIdentifier());
				if (newFlags.Contains("ruin".ToIdentifier()))
				{
					this.moduleFlags.Add("ruin".ToIdentifier());
				}
			}
			if (newFlags.Contains("hallwayvertical".ToIdentifier()))
			{
				this.moduleFlags.Add("hallwayvertical".ToIdentifier());
				if (newFlags.Contains("ruin".ToIdentifier()))
				{
					this.moduleFlags.Add("ruin".ToIdentifier());
				}
			}
			if (!newFlags.Any<Identifier>())
			{
				this.moduleFlags.Add("none".ToIdentifier());
			}
			foreach (Identifier flag in newFlags)
			{
				if (!(flag == "none") || newFlags.Count<Identifier>() <= 1)
				{
					this.moduleFlags.Add(flag);
				}
			}
		}

		// Token: 0x0600411D RID: 16669 RVA: 0x00243D0C File Offset: 0x00241F0C
		public void SetAllowAttachTo(IEnumerable<Identifier> allowAttachTo)
		{
			this.allowAttachToModules.Clear();
			if (!allowAttachTo.Any<Identifier>())
			{
				this.allowAttachToModules.Add("any".ToIdentifier());
			}
			foreach (Identifier flag in allowAttachTo)
			{
				if (!(flag == "any") || allowAttachTo.Count<Identifier>() <= 1)
				{
					this.allowAttachToModules.Add(flag);
				}
			}
		}

		// Token: 0x0600411E RID: 16670 RVA: 0x00243D9C File Offset: 0x00241F9C
		public void SetAllowedLocationTypes(IEnumerable<Identifier> allowedLocationTypes)
		{
			this.allowedLocationTypes.Clear();
			foreach (Identifier locationType in allowedLocationTypes)
			{
				if (!(locationType == "any"))
				{
					this.allowedLocationTypes.Add(locationType);
				}
			}
		}

		// Token: 0x0600411F RID: 16671 RVA: 0x00243E04 File Offset: 0x00242004
		public bool IsAllowedInAnyLocationType()
		{
			return this.allowedLocationTypes.None(null) || this.allowedLocationTypes.Contains("Any".ToIdentifier());
		}

		// Token: 0x06004120 RID: 16672 RVA: 0x00243E2C File Offset: 0x0024202C
		public bool IsAllowedInLocationType(LocationType locationType, bool requireLocationTypeSpecific = false)
		{
			return locationType == null || (!requireLocationTypeSpecific && this.IsAllowedInAnyLocationType()) || this.allowedLocationTypes.Contains(locationType.Identifier) || (!locationType.UseOutpostModulesOfLocationType.IsEmpty && this.allowedLocationTypes.Contains(locationType.UseOutpostModulesOfLocationType));
		}

		// Token: 0x06004121 RID: 16673 RVA: 0x00243E84 File Offset: 0x00242084
		public void DetermineGapPositions(Submarine sub)
		{
			this.GapPositions = OutpostModuleInfo.GapPosition.None;
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.Submarine == sub && gap.linkedTo.Count == 1 && (gap.ConnectedDoor == null || gap.ConnectedDoor.UseBetweenOutpostModules))
				{
					bool portFound = false;
					foreach (DockingPort port in DockingPort.List)
					{
						if (Submarine.RectContains(gap.WorldRect, port.Item.WorldPosition, false))
						{
							portFound = true;
							break;
						}
					}
					if (!portFound)
					{
						this.GapPositions |= (gap.IsHorizontal ? ((gap.linkedTo[0].WorldPosition.X < gap.WorldPosition.X) ? OutpostModuleInfo.GapPosition.Right : OutpostModuleInfo.GapPosition.Left) : ((gap.linkedTo[0].WorldPosition.Y < gap.WorldPosition.Y) ? OutpostModuleInfo.GapPosition.Top : OutpostModuleInfo.GapPosition.Bottom));
					}
				}
			}
		}

		// Token: 0x06004122 RID: 16674 RVA: 0x00243FE8 File Offset: 0x002421E8
		public void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
			element.SetAttributeValue("flags", string.Join<Identifier>(",", this.ModuleFlags));
			element.SetAttributeValue("allowattachto", string.Join<Identifier>(",", this.AllowAttachToModules));
			element.SetAttributeValue("allowedlocationtypes", string.Join<Identifier>(",", this.AllowedLocationTypes));
		}

		// Token: 0x040021D8 RID: 8664
		private readonly HashSet<Identifier> moduleFlags = new HashSet<Identifier>();

		// Token: 0x040021D9 RID: 8665
		private readonly HashSet<Identifier> allowAttachToModules = new HashSet<Identifier>();

		// Token: 0x040021DA RID: 8666
		private readonly HashSet<Identifier> allowedLocationTypes = new HashSet<Identifier>();

		// Token: 0x0200104E RID: 4174
		[Flags]
		public enum GapPosition
		{
			// Token: 0x04005807 RID: 22535
			None = 0,
			// Token: 0x04005808 RID: 22536
			Right = 1,
			// Token: 0x04005809 RID: 22537
			Left = 2,
			// Token: 0x0400580A RID: 22538
			Top = 4,
			// Token: 0x0400580B RID: 22539
			Bottom = 8
		}
	}
}
