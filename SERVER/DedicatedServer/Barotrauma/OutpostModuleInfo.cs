using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000257 RID: 599
	internal class OutpostModuleInfo : ISerializableEntity
	{
		// Token: 0x17000CC5 RID: 3269
		// (get) Token: 0x06002B08 RID: 11016 RVA: 0x0011B668 File Offset: 0x00119868
		public IEnumerable<Identifier> ModuleFlags
		{
			get
			{
				return this.moduleFlags;
			}
		}

		// Token: 0x17000CC6 RID: 3270
		// (get) Token: 0x06002B09 RID: 11017 RVA: 0x0011B670 File Offset: 0x00119870
		public IEnumerable<Identifier> AllowAttachToModules
		{
			get
			{
				return this.allowAttachToModules;
			}
		}

		// Token: 0x17000CC7 RID: 3271
		// (get) Token: 0x06002B0A RID: 11018 RVA: 0x0011B678 File Offset: 0x00119878
		public IEnumerable<Identifier> AllowedLocationTypes
		{
			get
			{
				return this.allowedLocationTypes;
			}
		}

		// Token: 0x17000CC8 RID: 3272
		// (get) Token: 0x06002B0B RID: 11019 RVA: 0x0011B680 File Offset: 0x00119880
		// (set) Token: 0x06002B0C RID: 11020 RVA: 0x0011B688 File Offset: 0x00119888
		[Serialize(100, IsPropertySaveable.Yes, "How many instances of this module can be used in one outpost.", "", false)]
		[Editable]
		public int MaxCount { get; set; }

		// Token: 0x17000CC9 RID: 3273
		// (get) Token: 0x06002B0D RID: 11021 RVA: 0x0011B691 File Offset: 0x00119891
		// (set) Token: 0x06002B0E RID: 11022 RVA: 0x0011B699 File Offset: 0x00119899
		[Serialize(10f, IsPropertySaveable.Yes, "How likely it is for the module to get picked when selecting from a set of modules during the outpost generation.", "", false)]
		[Editable]
		public float Commonness { get; set; }

		// Token: 0x17000CCA RID: 3274
		// (get) Token: 0x06002B0F RID: 11023 RVA: 0x0011B6A2 File Offset: 0x001198A2
		// (set) Token: 0x06002B10 RID: 11024 RVA: 0x0011B6AA File Offset: 0x001198AA
		[Serialize(OutpostModuleInfo.GapPosition.None, IsPropertySaveable.Yes, "Which sides of the module have gaps on them (i.e. from which sides the module can be attached to other modules). Center = no gaps available.", "", false)]
		public OutpostModuleInfo.GapPosition GapPositions { get; set; }

		// Token: 0x17000CCB RID: 3275
		// (get) Token: 0x06002B11 RID: 11025 RVA: 0x0011B6B3 File Offset: 0x001198B3
		// (set) Token: 0x06002B12 RID: 11026 RVA: 0x0011B6BB File Offset: 0x001198BB
		[Serialize(OutpostModuleInfo.GapPosition.Right | OutpostModuleInfo.GapPosition.Left | OutpostModuleInfo.GapPosition.Top | OutpostModuleInfo.GapPosition.Bottom, IsPropertySaveable.Yes, "Which sides of this module are allowed to attach to the previously placed module. E.g. if you want a module to always attach to the left side of the docking module, you could set this to Right.", "", false)]
		public OutpostModuleInfo.GapPosition CanAttachToPrevious { get; set; }

		// Token: 0x17000CCC RID: 3276
		// (get) Token: 0x06002B13 RID: 11027 RVA: 0x0011B6C4 File Offset: 0x001198C4
		// (set) Token: 0x06002B14 RID: 11028 RVA: 0x0011B6CC File Offset: 0x001198CC
		public string Name { get; private set; }

		// Token: 0x17000CCD RID: 3277
		// (get) Token: 0x06002B15 RID: 11029 RVA: 0x0011B6D5 File Offset: 0x001198D5
		// (set) Token: 0x06002B16 RID: 11030 RVA: 0x0011B6DD File Offset: 0x001198DD
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06002B17 RID: 11031 RVA: 0x0011B6E8 File Offset: 0x001198E8
		public OutpostModuleInfo(SubmarineInfo submarineInfo, XElement element)
		{
			this.Name = "OutpostModuleInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.SetFlags(element.GetAttributeIdentifierArray("flags", null, true) ?? element.GetAttributeIdentifierArray("moduletypes", Array.Empty<Identifier>(), true));
			this.SetAllowAttachTo(element.GetAttributeIdentifierArray("allowattachto", Array.Empty<Identifier>(), true));
			this.allowedLocationTypes = new HashSet<Identifier>(element.GetAttributeIdentifierArray("allowedlocationtypes", Array.Empty<Identifier>(), true));
		}

		// Token: 0x06002B18 RID: 11032 RVA: 0x0011B7A0 File Offset: 0x001199A0
		public OutpostModuleInfo(SubmarineInfo submarineInfo)
		{
			this.Name = "OutpostModuleInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
		}

		// Token: 0x06002B19 RID: 11033 RVA: 0x0011B7FC File Offset: 0x001199FC
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

		// Token: 0x06002B1A RID: 11034 RVA: 0x0011B90C File Offset: 0x00119B0C
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

		// Token: 0x06002B1B RID: 11035 RVA: 0x0011BA3C File Offset: 0x00119C3C
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

		// Token: 0x06002B1C RID: 11036 RVA: 0x0011BACC File Offset: 0x00119CCC
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

		// Token: 0x06002B1D RID: 11037 RVA: 0x0011BB34 File Offset: 0x00119D34
		public bool IsAllowedInAnyLocationType()
		{
			return this.allowedLocationTypes.None(null) || this.allowedLocationTypes.Contains("Any".ToIdentifier());
		}

		// Token: 0x06002B1E RID: 11038 RVA: 0x0011BB5C File Offset: 0x00119D5C
		public bool IsAllowedInLocationType(LocationType locationType, bool requireLocationTypeSpecific = false)
		{
			return locationType == null || (!requireLocationTypeSpecific && this.IsAllowedInAnyLocationType()) || this.allowedLocationTypes.Contains(locationType.Identifier) || (!locationType.UseOutpostModulesOfLocationType.IsEmpty && this.allowedLocationTypes.Contains(locationType.UseOutpostModulesOfLocationType));
		}

		// Token: 0x06002B1F RID: 11039 RVA: 0x0011BBB4 File Offset: 0x00119DB4
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

		// Token: 0x06002B20 RID: 11040 RVA: 0x0011BD18 File Offset: 0x00119F18
		public void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
			element.SetAttributeValue("flags", string.Join<Identifier>(",", this.ModuleFlags));
			element.SetAttributeValue("allowattachto", string.Join<Identifier>(",", this.AllowAttachToModules));
			element.SetAttributeValue("allowedlocationtypes", string.Join<Identifier>(",", this.AllowedLocationTypes));
		}

		// Token: 0x04001515 RID: 5397
		private readonly HashSet<Identifier> moduleFlags = new HashSet<Identifier>();

		// Token: 0x04001516 RID: 5398
		private readonly HashSet<Identifier> allowAttachToModules = new HashSet<Identifier>();

		// Token: 0x04001517 RID: 5399
		private readonly HashSet<Identifier> allowedLocationTypes = new HashSet<Identifier>();

		// Token: 0x02000AB7 RID: 2743
		[Flags]
		public enum GapPosition
		{
			// Token: 0x040036FC RID: 14076
			None = 0,
			// Token: 0x040036FD RID: 14077
			Right = 1,
			// Token: 0x040036FE RID: 14078
			Left = 2,
			// Token: 0x040036FF RID: 14079
			Top = 4,
			// Token: 0x04003700 RID: 14080
			Bottom = 8
		}
	}
}
