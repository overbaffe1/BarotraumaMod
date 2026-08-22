using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000327 RID: 807
	internal abstract class ExtraSubmarineInfo : ISerializableEntity
	{
		// Token: 0x17001122 RID: 4386
		// (get) Token: 0x06004086 RID: 16518 RVA: 0x0023DB21 File Offset: 0x0023BD21
		// (set) Token: 0x06004087 RID: 16519 RVA: 0x0023DB29 File Offset: 0x0023BD29
		public string Name { get; protected set; }

		// Token: 0x17001123 RID: 4387
		// (get) Token: 0x06004088 RID: 16520 RVA: 0x0023DB32 File Offset: 0x0023BD32
		// (set) Token: 0x06004089 RID: 16521 RVA: 0x0023DB3A File Offset: 0x0023BD3A
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x17001124 RID: 4388
		// (get) Token: 0x0600408A RID: 16522 RVA: 0x0023DB43 File Offset: 0x0023BD43
		public HashSet<Identifier> MissionTags { get; } = new HashSet<Identifier>();

		// Token: 0x17001125 RID: 4389
		// (get) Token: 0x0600408B RID: 16523 RVA: 0x0023DB4B File Offset: 0x0023BD4B
		// (set) Token: 0x0600408C RID: 16524 RVA: 0x0023DB53 File Offset: 0x0023BD53
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float MinLevelDifficulty { get; set; }

		// Token: 0x17001126 RID: 4390
		// (get) Token: 0x0600408D RID: 16525 RVA: 0x0023DB5C File Offset: 0x0023BD5C
		// (set) Token: 0x0600408E RID: 16526 RVA: 0x0023DB64 File Offset: 0x0023BD64
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float MaxLevelDifficulty { get; set; }

		// Token: 0x0600408F RID: 16527 RVA: 0x0023DB70 File Offset: 0x0023BD70
		public ExtraSubmarineInfo(SubmarineInfo submarineInfo, XElement element)
		{
			this.Name = "ExtraSubmarineInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			foreach (Identifier missionTag in element.GetAttributeIdentifierArray("MissionTags", Array.Empty<Identifier>(), true))
			{
				this.MissionTags.Add(missionTag);
			}
		}

		// Token: 0x06004090 RID: 16528 RVA: 0x0023DBEB File Offset: 0x0023BDEB
		public ExtraSubmarineInfo(SubmarineInfo submarineInfo)
		{
			this.Name = "ExtraSubmarineInfo (" + submarineInfo.Name + ")";
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
		}

		// Token: 0x06004091 RID: 16529 RVA: 0x0023DC28 File Offset: 0x0023BE28
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

		// Token: 0x06004092 RID: 16530 RVA: 0x0023DD2C File Offset: 0x0023BF2C
		public virtual void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
			element.SetAttributeValue("MissionTags", string.Join<Identifier>(',', this.MissionTags));
		}
	}
}
