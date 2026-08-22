using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010E RID: 270
	[NetworkSerialize(121)]
	internal readonly struct CircuitBoxAddComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxAddComponentEvent>
	{
		// Token: 0x06001A85 RID: 6789 RVA: 0x000CA4A3 File Offset: 0x000C86A3
		public CircuitBoxAddComponentEvent(uint PrefabIdentifier, Vector2 Position)
		{
			this.PrefabIdentifier = PrefabIdentifier;
			this.Position = Position;
		}

		// Token: 0x170007E5 RID: 2021
		// (get) Token: 0x06001A86 RID: 6790 RVA: 0x000CA4B3 File Offset: 0x000C86B3
		// (set) Token: 0x06001A87 RID: 6791 RVA: 0x000CA4BB File Offset: 0x000C86BB
		public uint PrefabIdentifier { get; set; }

		// Token: 0x170007E6 RID: 2022
		// (get) Token: 0x06001A88 RID: 6792 RVA: 0x000CA4C4 File Offset: 0x000C86C4
		// (set) Token: 0x06001A89 RID: 6793 RVA: 0x000CA4CC File Offset: 0x000C86CC
		public Vector2 Position { get; set; }

		// Token: 0x06001A8A RID: 6794 RVA: 0x000CA4D8 File Offset: 0x000C86D8
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

		// Token: 0x06001A8B RID: 6795 RVA: 0x000CA524 File Offset: 0x000C8724
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("PrefabIdentifier = ");
			builder.Append(this.PrefabIdentifier.ToString());
			builder.Append(", Position = ");
			builder.Append(this.Position.ToString());
			return true;
		}

		// Token: 0x06001A8C RID: 6796 RVA: 0x000CA580 File Offset: 0x000C8780
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxAddComponentEvent left, CircuitBoxAddComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001A8D RID: 6797 RVA: 0x000CA58C File Offset: 0x000C878C
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxAddComponentEvent left, CircuitBoxAddComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A8E RID: 6798 RVA: 0x000CA596 File Offset: 0x000C8796
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<uint>.Default.GetHashCode(this.<PrefabIdentifier>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Position>k__BackingField);
		}

		// Token: 0x06001A8F RID: 6799 RVA: 0x000CA5BF File Offset: 0x000C87BF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxAddComponentEvent && this.Equals((CircuitBoxAddComponentEvent)obj);
		}

		// Token: 0x06001A90 RID: 6800 RVA: 0x000CA5D7 File Offset: 0x000C87D7
		[CompilerGenerated]
		public bool Equals(CircuitBoxAddComponentEvent other)
		{
			return EqualityComparer<uint>.Default.Equals(this.<PrefabIdentifier>k__BackingField, other.<PrefabIdentifier>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Position>k__BackingField, other.<Position>k__BackingField);
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x000CA609 File Offset: 0x000C8809
		[CompilerGenerated]
		public void Deconstruct(out uint PrefabIdentifier, out Vector2 Position)
		{
			PrefabIdentifier = this.PrefabIdentifier;
			Position = this.Position;
		}
	}
}
