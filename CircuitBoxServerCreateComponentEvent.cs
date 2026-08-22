using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000208 RID: 520
	[NetworkSerialize(124)]
	internal readonly struct CircuitBoxServerCreateComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxServerCreateComponentEvent>
	{
		// Token: 0x06003581 RID: 13697 RVA: 0x002107F3 File Offset: 0x0020E9F3
		public CircuitBoxServerCreateComponentEvent(ushort BackingItemId, uint UsedResource, ushort ComponentId, Vector2 Position)
		{
			this.BackingItemId = BackingItemId;
			this.UsedResource = UsedResource;
			this.ComponentId = ComponentId;
			this.Position = Position;
		}

		// Token: 0x17000E4B RID: 3659
		// (get) Token: 0x06003582 RID: 13698 RVA: 0x00210812 File Offset: 0x0020EA12
		// (set) Token: 0x06003583 RID: 13699 RVA: 0x0021081A File Offset: 0x0020EA1A
		public ushort BackingItemId { get; set; }

		// Token: 0x17000E4C RID: 3660
		// (get) Token: 0x06003584 RID: 13700 RVA: 0x00210823 File Offset: 0x0020EA23
		// (set) Token: 0x06003585 RID: 13701 RVA: 0x0021082B File Offset: 0x0020EA2B
		public uint UsedResource { get; set; }

		// Token: 0x17000E4D RID: 3661
		// (get) Token: 0x06003586 RID: 13702 RVA: 0x00210834 File Offset: 0x0020EA34
		// (set) Token: 0x06003587 RID: 13703 RVA: 0x0021083C File Offset: 0x0020EA3C
		public ushort ComponentId { get; set; }

		// Token: 0x17000E4E RID: 3662
		// (get) Token: 0x06003588 RID: 13704 RVA: 0x00210845 File Offset: 0x0020EA45
		// (set) Token: 0x06003589 RID: 13705 RVA: 0x0021084D File Offset: 0x0020EA4D
		public Vector2 Position { get; set; }

		// Token: 0x0600358A RID: 13706 RVA: 0x00210858 File Offset: 0x0020EA58
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

		// Token: 0x0600358B RID: 13707 RVA: 0x002108A4 File Offset: 0x0020EAA4
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

		// Token: 0x0600358C RID: 13708 RVA: 0x0021094E File Offset: 0x0020EB4E
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxServerCreateComponentEvent left, CircuitBoxServerCreateComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600358D RID: 13709 RVA: 0x0021095A File Offset: 0x0020EB5A
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxServerCreateComponentEvent left, CircuitBoxServerCreateComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600358E RID: 13710 RVA: 0x00210964 File Offset: 0x0020EB64
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ushort>.Default.GetHashCode(this.<BackingItemId>k__BackingField) * -1521134295 + EqualityComparer<uint>.Default.GetHashCode(this.<UsedResource>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ComponentId>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField);
		}

		// Token: 0x0600358F RID: 13711 RVA: 0x002109C6 File Offset: 0x0020EBC6
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxServerCreateComponentEvent && this.Equals((CircuitBoxServerCreateComponentEvent)obj);
		}

		// Token: 0x06003590 RID: 13712 RVA: 0x002109E0 File Offset: 0x0020EBE0
		[CompilerGenerated]
		public bool Equals(CircuitBoxServerCreateComponentEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<BackingItemId>k__BackingField, other.<BackingItemId>k__BackingField) && EqualityComparer<uint>.Default.Equals(this.<UsedResource>k__BackingField, other.<UsedResource>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ComponentId>k__BackingField, other.<ComponentId>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField);
		}

		// Token: 0x06003591 RID: 13713 RVA: 0x00210A4D File Offset: 0x0020EC4D
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
