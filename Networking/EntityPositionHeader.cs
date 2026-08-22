using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x0200048B RID: 1163
	[NetworkSerialize(119)]
	internal readonly struct EntityPositionHeader : INetSerializableStruct, IEquatable<EntityPositionHeader>
	{
		// Token: 0x06004DE8 RID: 19944 RVA: 0x002AC07D File Offset: 0x002AA27D
		public EntityPositionHeader(bool IsItem, uint PrefabUintIdentifier, ushort EntityId)
		{
			this.IsItem = IsItem;
			this.PrefabUintIdentifier = PrefabUintIdentifier;
			this.EntityId = EntityId;
		}

		// Token: 0x170013F9 RID: 5113
		// (get) Token: 0x06004DE9 RID: 19945 RVA: 0x002AC094 File Offset: 0x002AA294
		// (set) Token: 0x06004DEA RID: 19946 RVA: 0x002AC09C File Offset: 0x002AA29C
		public bool IsItem { get; set; }

		// Token: 0x170013FA RID: 5114
		// (get) Token: 0x06004DEB RID: 19947 RVA: 0x002AC0A5 File Offset: 0x002AA2A5
		// (set) Token: 0x06004DEC RID: 19948 RVA: 0x002AC0AD File Offset: 0x002AA2AD
		public uint PrefabUintIdentifier { get; set; }

		// Token: 0x170013FB RID: 5115
		// (get) Token: 0x06004DED RID: 19949 RVA: 0x002AC0B6 File Offset: 0x002AA2B6
		// (set) Token: 0x06004DEE RID: 19950 RVA: 0x002AC0BE File Offset: 0x002AA2BE
		public ushort EntityId { get; set; }

		// Token: 0x06004DEF RID: 19951 RVA: 0x002AC0C8 File Offset: 0x002AA2C8
		public static EntityPositionHeader FromEntity(Entity entity)
		{
			bool isItem = entity is Item;
			MapEntity me = entity as MapEntity;
			return new EntityPositionHeader(isItem, (me != null) ? me.Prefab.UintIdentifier : 0U, entity.ID);
		}

		// Token: 0x06004DF0 RID: 19952 RVA: 0x002AC104 File Offset: 0x002AA304
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("EntityPositionHeader");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06004DF1 RID: 19953 RVA: 0x002AC150 File Offset: 0x002AA350
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("IsItem = ");
			builder.Append(this.IsItem.ToString());
			builder.Append(", PrefabUintIdentifier = ");
			builder.Append(this.PrefabUintIdentifier.ToString());
			builder.Append(", EntityId = ");
			builder.Append(this.EntityId.ToString());
			return true;
		}

		// Token: 0x06004DF2 RID: 19954 RVA: 0x002AC1D3 File Offset: 0x002AA3D3
		[CompilerGenerated]
		public static bool operator !=(EntityPositionHeader left, EntityPositionHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06004DF3 RID: 19955 RVA: 0x002AC1DF File Offset: 0x002AA3DF
		[CompilerGenerated]
		public static bool operator ==(EntityPositionHeader left, EntityPositionHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004DF4 RID: 19956 RVA: 0x002AC1E9 File Offset: 0x002AA3E9
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<bool>.Default.GetHashCode(this.<IsItem>k__BackingField) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<PrefabUintIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<EntityId>k__BackingField);
		}

		// Token: 0x06004DF5 RID: 19957 RVA: 0x002AC229 File Offset: 0x002AA429
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is EntityPositionHeader && this.Equals((EntityPositionHeader)obj);
		}

		// Token: 0x06004DF6 RID: 19958 RVA: 0x002AC244 File Offset: 0x002AA444
		[CompilerGenerated]
		public bool Equals(EntityPositionHeader other)
		{
			return EqualityComparer<bool>.Default.Equals(this.<IsItem>k__BackingField, other.<IsItem>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<PrefabUintIdentifier>k__BackingField, other.<PrefabUintIdentifier>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<EntityId>k__BackingField, other.<EntityId>k__BackingField);
		}

		// Token: 0x06004DF7 RID: 19959 RVA: 0x002AC299 File Offset: 0x002AA499
		[CompilerGenerated]
		public void Deconstruct(out bool IsItem, out uint PrefabUintIdentifier, out ushort EntityId)
		{
			IsItem = this.IsItem;
			PrefabUintIdentifier = this.PrefabUintIdentifier;
			EntityId = this.EntityId;
		}
	}
}
