using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000207 RID: 519
	[NetworkSerialize(121)]
	internal readonly struct CircuitBoxAddComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxAddComponentEvent>
	{
		// Token: 0x06003574 RID: 13684 RVA: 0x00210677 File Offset: 0x0020E877
		public CircuitBoxAddComponentEvent(uint PrefabIdentifier, Vector2 Position)
		{
			this.PrefabIdentifier = PrefabIdentifier;
			this.Position = Position;
		}

		// Token: 0x17000E49 RID: 3657
		// (get) Token: 0x06003575 RID: 13685 RVA: 0x00210687 File Offset: 0x0020E887
		// (set) Token: 0x06003576 RID: 13686 RVA: 0x0021068F File Offset: 0x0020E88F
		public uint PrefabIdentifier { get; set; }

		// Token: 0x17000E4A RID: 3658
		// (get) Token: 0x06003577 RID: 13687 RVA: 0x00210698 File Offset: 0x0020E898
		// (set) Token: 0x06003578 RID: 13688 RVA: 0x002106A0 File Offset: 0x0020E8A0
		public Vector2 Position { get; set; }

		// Token: 0x06003579 RID: 13689 RVA: 0x002106AC File Offset: 0x0020E8AC
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxAddComponentEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600357A RID: 13690 RVA: 0x002106F8 File Offset: 0x0020E8F8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("PrefabIdentifier = ");
			builder.Append(this.PrefabIdentifier.ToString());
			builder.Append(", Position = ");
			builder.Append(this.Position.ToString());
			return true;
		}

		// Token: 0x0600357B RID: 13691 RVA: 0x00210754 File Offset: 0x0020E954
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxAddComponentEvent left, CircuitBoxAddComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600357C RID: 13692 RVA: 0x00210760 File Offset: 0x0020E960
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxAddComponentEvent left, CircuitBoxAddComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600357D RID: 13693 RVA: 0x0021076A File Offset: 0x0020E96A
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<uint>.Default.GetHashCode(this.<PrefabIdentifier>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField);
		}

		// Token: 0x0600357E RID: 13694 RVA: 0x00210793 File Offset: 0x0020E993
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxAddComponentEvent && this.Equals((CircuitBoxAddComponentEvent)obj);
		}

		// Token: 0x0600357F RID: 13695 RVA: 0x002107AB File Offset: 0x0020E9AB
		[CompilerGenerated]
		public bool Equals(CircuitBoxAddComponentEvent other)
		{
			return EqualityComparer<uint>.Default.Equals(this.<PrefabIdentifier>k__BackingField, other.<PrefabIdentifier>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField);
		}

		// Token: 0x06003580 RID: 13696 RVA: 0x002107DD File Offset: 0x0020E9DD
		[CompilerGenerated]
		public void Deconstruct(out uint PrefabIdentifier, out Vector2 Position)
		{
			PrefabIdentifier = this.PrefabIdentifier;
			Position = this.Position;
		}
	}
}
