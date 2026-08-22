using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x0200038F RID: 911
	[NetworkSerialize(119)]
	internal readonly struct EntityPositionHeader : INetSerializableStruct, IEquatable<EntityPositionHeader>
	{
		// Token: 0x0600362C RID: 13868 RVA: 0x001736B5 File Offset: 0x001718B5
		public EntityPositionHeader(bool IsItem, uint PrefabUintIdentifier, ushort EntityId)
		{
			this.IsItem = IsItem;
			this.PrefabUintIdentifier = PrefabUintIdentifier;
			this.EntityId = EntityId;
		}

		// Token: 0x17000F08 RID: 3848
		// (get) Token: 0x0600362D RID: 13869 RVA: 0x001736CC File Offset: 0x001718CC
		// (set) Token: 0x0600362E RID: 13870 RVA: 0x001736D4 File Offset: 0x001718D4
		public bool IsItem { get; set; }

		// Token: 0x17000F09 RID: 3849
		// (get) Token: 0x0600362F RID: 13871 RVA: 0x001736DD File Offset: 0x001718DD
		// (set) Token: 0x06003630 RID: 13872 RVA: 0x001736E5 File Offset: 0x001718E5
		public uint PrefabUintIdentifier { get; set; }

		// Token: 0x17000F0A RID: 3850
		// (get) Token: 0x06003631 RID: 13873 RVA: 0x001736EE File Offset: 0x001718EE
		// (set) Token: 0x06003632 RID: 13874 RVA: 0x001736F6 File Offset: 0x001718F6
		public ushort EntityId { get; set; }

		// Token: 0x06003633 RID: 13875 RVA: 0x00173700 File Offset: 0x00171900
		public static EntityPositionHeader FromEntity(Entity entity)
		{
			bool isItem = entity is Item;
			MapEntity me = entity as MapEntity;
			return new EntityPositionHeader(isItem, (me != null) ? me.Prefab.UintIdentifier : 0U, entity.ID);
		}

		// Token: 0x06003634 RID: 13876 RVA: 0x0017373C File Offset: 0x0017193C
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

		// Token: 0x06003635 RID: 13877 RVA: 0x00173788 File Offset: 0x00171988
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

		// Token: 0x06003636 RID: 13878 RVA: 0x0017380B File Offset: 0x00171A0B
		[CompilerGenerated]
		public static bool operator !=(EntityPositionHeader left, EntityPositionHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06003637 RID: 13879 RVA: 0x00173817 File Offset: 0x00171A17
		[CompilerGenerated]
		public static bool operator ==(EntityPositionHeader left, EntityPositionHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003638 RID: 13880 RVA: 0x00173821 File Offset: 0x00171A21
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<bool>.Default.GetHashCode(this.<IsItem>k__BackingField) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<PrefabUintIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<EntityId>k__BackingField);
		}

		// Token: 0x06003639 RID: 13881 RVA: 0x00173861 File Offset: 0x00171A61
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is EntityPositionHeader && this.Equals((EntityPositionHeader)obj);
		}

		// Token: 0x0600363A RID: 13882 RVA: 0x0017387C File Offset: 0x00171A7C
		[CompilerGenerated]
		public bool Equals(EntityPositionHeader other)
		{
			return EqualityComparer<bool>.Default.Equals(this.<IsItem>k__BackingField, other.<IsItem>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<PrefabUintIdentifier>k__BackingField, other.<PrefabUintIdentifier>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<EntityId>k__BackingField, other.<EntityId>k__BackingField);
		}

		// Token: 0x0600363B RID: 13883 RVA: 0x001738D1 File Offset: 0x00171AD1
		[CompilerGenerated]
		public void Deconstruct(out bool IsItem, out uint PrefabUintIdentifier, out ushort EntityId)
		{
			IsItem = this.IsItem;
			PrefabUintIdentifier = this.PrefabUintIdentifier;
			EntityId = this.EntityId;
		}
	}
}
