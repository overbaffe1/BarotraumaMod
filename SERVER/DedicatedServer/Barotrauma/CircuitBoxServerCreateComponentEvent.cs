using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010F RID: 271
	[NetworkSerialize(124)]
	internal readonly struct CircuitBoxServerCreateComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxServerCreateComponentEvent>
	{
		// Token: 0x06001A92 RID: 6802 RVA: 0x000CA61F File Offset: 0x000C881F
		public CircuitBoxServerCreateComponentEvent(ushort BackingItemId, uint UsedResource, ushort ComponentId, Vector2 Position)
		{
			this.BackingItemId = BackingItemId;
			this.UsedResource = UsedResource;
			this.ComponentId = ComponentId;
			this.Position = Position;
		}

		// Token: 0x170007E7 RID: 2023
		// (get) Token: 0x06001A93 RID: 6803 RVA: 0x000CA63E File Offset: 0x000C883E
		// (set) Token: 0x06001A94 RID: 6804 RVA: 0x000CA646 File Offset: 0x000C8846
		public ushort BackingItemId { get; set; }

		// Token: 0x170007E8 RID: 2024
		// (get) Token: 0x06001A95 RID: 6805 RVA: 0x000CA64F File Offset: 0x000C884F
		// (set) Token: 0x06001A96 RID: 6806 RVA: 0x000CA657 File Offset: 0x000C8857
		public uint UsedResource { get; set; }

		// Token: 0x170007E9 RID: 2025
		// (get) Token: 0x06001A97 RID: 6807 RVA: 0x000CA660 File Offset: 0x000C8860
		// (set) Token: 0x06001A98 RID: 6808 RVA: 0x000CA668 File Offset: 0x000C8868
		public ushort ComponentId { get; set; }

		// Token: 0x170007EA RID: 2026
		// (get) Token: 0x06001A99 RID: 6809 RVA: 0x000CA671 File Offset: 0x000C8871
		// (set) Token: 0x06001A9A RID: 6810 RVA: 0x000CA679 File Offset: 0x000C8879
		public Vector2 Position { get; set; }

		// Token: 0x06001A9B RID: 6811 RVA: 0x000CA684 File Offset: 0x000C8884
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxServerCreateComponentEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001A9C RID: 6812 RVA: 0x000CA6D0 File Offset: 0x000C88D0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("BackingItemId = ");
			builder.Append(this.BackingItemId.ToString());
			builder.Append(", UsedResource = ");
			builder.Append(this.UsedResource.ToString());
			builder.Append(", ComponentId = ");
			builder.Append(this.ComponentId.ToString());
			builder.Append(", Position = ");
			builder.Append(this.Position.ToString());
			return true;
		}

		// Token: 0x06001A9D RID: 6813 RVA: 0x000CA77A File Offset: 0x000C897A
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerCreateComponentEvent left, CircuitBoxServerCreateComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x000CA786 File Offset: 0x000C8986
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerCreateComponentEvent left, CircuitBoxServerCreateComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A9F RID: 6815 RVA: 0x000CA790 File Offset: 0x000C8990
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ushort>.Default.GetHashCode(this.<BackingItemId>k__BackingField) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<UsedResource>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ComponentId>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField);
		}

		// Token: 0x06001AA0 RID: 6816 RVA: 0x000CA7F2 File Offset: 0x000C89F2
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerCreateComponentEvent && this.Equals((CircuitBoxServerCreateComponentEvent)obj);
		}

		// Token: 0x06001AA1 RID: 6817 RVA: 0x000CA80C File Offset: 0x000C8A0C
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerCreateComponentEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<BackingItemId>k__BackingField, other.<BackingItemId>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<UsedResource>k__BackingField, other.<UsedResource>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ComponentId>k__BackingField, other.<ComponentId>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField);
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x000CA879 File Offset: 0x000C8A79
		[CompilerGenerated]
		public void Deconstruct(out ushort BackingItemId, out uint UsedResource, out ushort ComponentId, out Vector2 Position)
		{
			BackingItemId = this.BackingItemId;
			UsedResource = this.UsedResource;
			ComponentId = this.ComponentId;
			Position = this.Position;
		}
	}
}
