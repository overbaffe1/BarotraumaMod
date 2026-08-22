using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000250 RID: 592
	internal abstract class ExtraSubmarineInfo : ISerializableEntity
	{
		// Token: 0x17000C9F RID: 3231
		// (get) Token: 0x06002A84 RID: 10884 RVA: 0x001157CC File Offset: 0x001139CC
		// (set) Token: 0x06002A85 RID: 10885 RVA: 0x001157D4 File Offset: 0x001139D4
		public string Name { get; protected set; }

		// Token: 0x17000CA0 RID: 3232
		// (get) Token: 0x06002A86 RID: 10886 RVA: 0x001157DD File Offset: 0x001139DD
		// (set) Token: 0x06002A87 RID: 10887 RVA: 0x001157E5 File Offset: 0x001139E5
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x17000CA1 RID: 3233
		// (get) Token: 0x06002A88 RID: 10888 RVA: 0x001157EE File Offset: 0x001139EE
		public HashSet<Identifier> MissionTags { get; } = new HashSet<Identifier>();

		// Token: 0x17000CA2 RID: 3234
		// (get) Token: 0x06002A89 RID: 10889 RVA: 0x001157F6 File Offset: 0x001139F6
		// (set) Token: 0x06002A8A RID: 10890 RVA: 0x001157FE File Offset: 0x001139FE
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float MinLevelDifficulty { get; set; }

		// Token: 0x17000CA3 RID: 3235
		// (get) Token: 0x06002A8B RID: 10891 RVA: 0x00115807 File Offset: 0x00113A07
		// (set) Token: 0x06002A8C RID: 10892 RVA: 0x0011580F File Offset: 0x00113A0F
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float MaxLevelDifficulty { get; set; }

		// Token: 0x06002A8D RID: 10893 RVA: 0x00115818 File Offset: 0x00113A18
		public ExtraSubmarineInfo(SubmarineInfo submarineInfo, XElement element)
		{
			this.Name = "ExtraSubmarineInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			foreach (Identifier missionTag in element.GetAttributeIdentifierArray("MissionTags", Array.Empty<Identifier>(), true))
			{
				this.MissionTags.Add(missionTag);
			}
		}

		// Token: 0x06002A8E RID: 10894 RVA: 0x00115893 File Offset: 0x00113A93
		public ExtraSubmarineInfo(SubmarineInfo submarineInfo)
		{
			this.Name = "ExtraSubmarineInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x001158D0 File Offset: 0x00113AD0
		public ExtraSubmarineInfo(ExtraSubmarineInfo original)
		{
			this.Name = original.Name;
			this.SerializableProperties = new Dictionary<Identifier, SerializableProperty>();
			foreach (KeyValuePair<Identifier, SerializableProperty> kvp in original.SerializableProperties)
			{
				this.SerializableProperties.Add(kvp.Key, kvp.Value);
				if (SerializableProperty.GetSupportedTypeName(kvp.Value.PropertyType) != null)
				{
					kvp.Value.TrySetValue(this, kvp.Value.GetValue(original));
				}
			}
			foreach (Identifier missionTag in original.MissionTags)
			{
				this.MissionTags.Add(missionTag);
			}
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x001159D4 File Offset: 0x00113BD4
		public virtual void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
			element.SetAttributeValue("MissionTags", string.Join<Identifier>(',', this.MissionTags));
		}
	}
}
