using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000213 RID: 531
	[NetworkSerialize(157)]
	internal readonly struct CircuitBoxRenameLabelEvent : INetSerializableStruct, IEquatable<CircuitBoxRenameLabelEvent>
	{
		// Token: 0x06003626 RID: 13862 RVA: 0x00211E17 File Offset: 0x00210017
		public CircuitBoxRenameLabelEvent(ushort LabelId, Color Color, NetLimitedString NewHeader, NetLimitedString NewBody)
		{
			this.LabelId = LabelId;
			this.Color = Color;
			this.NewHeader = NewHeader;
			this.NewBody = NewBody;
		}

		// Token: 0x17000E6C RID: 3692
		// (get) Token: 0x06003627 RID: 13863 RVA: 0x00211E36 File Offset: 0x00210036
		// (set) Token: 0x06003628 RID: 13864 RVA: 0x00211E3E File Offset: 0x0021003E
		public ushort LabelId { get; set; }

		// Token: 0x17000E6D RID: 3693
		// (get) Token: 0x06003629 RID: 13865 RVA: 0x00211E47 File Offset: 0x00210047
		// (set) Token: 0x0600362A RID: 13866 RVA: 0x00211E4F File Offset: 0x0021004F
		public Color Color { get; set; }

		// Token: 0x17000E6E RID: 3694
		// (get) Token: 0x0600362B RID: 13867 RVA: 0x00211E58 File Offset: 0x00210058
		// (set) Token: 0x0600362C RID: 13868 RVA: 0x00211E60 File Offset: 0x00210060
		public NetLimitedString NewHeader { get; set; }

		// Token: 0x17000E6F RID: 3695
		// (get) Token: 0x0600362D RID: 13869 RVA: 0x00211E69 File Offset: 0x00210069
		// (set) Token: 0x0600362E RID: 13870 RVA: 0x00211E71 File Offset: 0x00210071
		public NetLimitedString NewBody { get; set; }

		// Token: 0x0600362F RID: 13871 RVA: 0x00211E7C File Offset: 0x0021007C
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRenameLabelEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003630 RID: 13872 RVA: 0x00211EC8 File Offset: 0x002100C8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("LabelId = ");
			builder.Append(this.LabelId.ToString());
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", NewHeader = ");
			builder.Append(this.NewHeader.ToString());
			builder.Append(", NewBody = ");
			builder.Append(this.NewBody.ToString());
			return true;
		}

		// Token: 0x06003631 RID: 13873 RVA: 0x00211F72 File Offset: 0x00210172
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRenameLabelEvent left, CircuitBoxRenameLabelEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003632 RID: 13874 RVA: 0x00211F7E File Offset: 0x0021017E
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRenameLabelEvent left, CircuitBoxRenameLabelEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003633 RID: 13875 RVA: 0x00211F88 File Offset: 0x00210188
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ushort>.Default.GetHashCode(this.<LabelId>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<NewHeader>k__BackingField)) * -1521134295 + EqualityComparer<NetLimitedString>.Default.GetHashCode(this.<NewBody>k__BackingField);
		}

		// Token: 0x06003634 RID: 13876 RVA: 0x00211FEA File Offset: 0x002101EA
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRenameLabelEvent && this.Equals((CircuitBoxRenameLabelEvent)obj);
		}

		// Token: 0x06003635 RID: 13877 RVA: 0x00212004 File Offset: 0x00210204
		[CompilerGenerated]
		public bool Equals(CircuitBoxRenameLabelEvent other)
		{
			return EqualityComparer<ushort>.Default.Equals(this.<LabelId>k__BackingField, other.<LabelId>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<NewHeader>k__BackingField, other.<NewHeader>k__BackingField) && EqualityComparer<NetLimitedString>.Default.Equals(this.<NewBody>k__BackingField, other.<NewBody>k__BackingField);
		}

		// Token: 0x06003636 RID: 13878 RVA: 0x00212071 File Offset: 0x00210271
		[CompilerGenerated]
		public void Deconstruct(out ushort LabelId, out Color Color, out NetLimitedString NewHeader, out NetLimitedString NewBody)
		{
			LabelId = this.LabelId;
			Color = this.Color;
			NewHeader = this.NewHeader;
			NewBody = this.NewBody;
		}
	}
}
