using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200046E RID: 1134
	public class ResourceParserInfo : IEquatable<ResourceParserInfo>
	{
		// Token: 0x06003D66 RID: 15718 RVA: 0x0018E21B File Offset: 0x0018C41B
		public ResourceParserInfo([NotNull] ContentPackage Owner, [NotNull] XElement Element, ImmutableArray<Identifier> Required, ImmutableArray<Identifier> Incompatible)
		{
			this.Owner = Owner;
			this.Element = Element;
			this.Required = Required;
			this.Incompatible = Incompatible;
			base..ctor();
		}

		// Token: 0x1700101A RID: 4122
		// (get) Token: 0x06003D67 RID: 15719 RVA: 0x0018E240 File Offset: 0x0018C440
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ResourceParserInfo);
			}
		}

		// Token: 0x1700101B RID: 4123
		// (get) Token: 0x06003D68 RID: 15720 RVA: 0x0018E24C File Offset: 0x0018C44C
		// (set) Token: 0x06003D69 RID: 15721 RVA: 0x0018E254 File Offset: 0x0018C454
		public ContentPackage Owner { get; set; }

		// Token: 0x1700101C RID: 4124
		// (get) Token: 0x06003D6A RID: 15722 RVA: 0x0018E25D File Offset: 0x0018C45D
		// (set) Token: 0x06003D6B RID: 15723 RVA: 0x0018E265 File Offset: 0x0018C465
		public XElement Element { get; set; }

		// Token: 0x1700101D RID: 4125
		// (get) Token: 0x06003D6C RID: 15724 RVA: 0x0018E26E File Offset: 0x0018C46E
		// (set) Token: 0x06003D6D RID: 15725 RVA: 0x0018E276 File Offset: 0x0018C476
		public ImmutableArray<Identifier> Required { get; set; }

		// Token: 0x1700101E RID: 4126
		// (get) Token: 0x06003D6E RID: 15726 RVA: 0x0018E27F File Offset: 0x0018C47F
		// (set) Token: 0x06003D6F RID: 15727 RVA: 0x0018E287 File Offset: 0x0018C487
		public ImmutableArray<Identifier> Incompatible { get; set; }

		// Token: 0x06003D70 RID: 15728 RVA: 0x0018E290 File Offset: 0x0018C490
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ResourceParserInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D71 RID: 15729 RVA: 0x0018E2DC File Offset: 0x0018C4DC
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Owner = ");
			builder.Append(this.Owner);
			builder.Append(", Element = ");
			builder.Append(this.Element);
			builder.Append(", Required = ");
			builder.Append(this.Required.ToString());
			builder.Append(", Incompatible = ");
			builder.Append(this.Incompatible.ToString());
			return true;
		}

		// Token: 0x06003D72 RID: 15730 RVA: 0x0018E36F File Offset: 0x0018C56F
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ResourceParserInfo left, ResourceParserInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D73 RID: 15731 RVA: 0x0018E37B File Offset: 0x0018C57B
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ResourceParserInfo left, ResourceParserInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D74 RID: 15732 RVA: 0x0018E390 File Offset: 0x0018C590
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<Owner>k__BackingField)) * -1521134295 + EqualityComparer<XElement>.Default.GetHashCode(this.<Element>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Required>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Incompatible>k__BackingField);
		}

		// Token: 0x06003D75 RID: 15733 RVA: 0x0018E409 File Offset: 0x0018C609
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ResourceParserInfo);
		}

		// Token: 0x06003D76 RID: 15734 RVA: 0x0018E418 File Offset: 0x0018C618
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ResourceParserInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<ContentPackage>.Default.Equals(this.<Owner>k__BackingField, other.<Owner>k__BackingField) && EqualityComparer<XElement>.Default.Equals(this.<Element>k__BackingField, other.<Element>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Required>k__BackingField, other.<Required>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Incompatible>k__BackingField, other.<Incompatible>k__BackingField));
		}

		// Token: 0x06003D78 RID: 15736 RVA: 0x0018E4A9 File Offset: 0x0018C6A9
		[CompilerGenerated]
		protected ResourceParserInfo([Nullable(1)] ResourceParserInfo original)
		{
			this.Owner = original.<Owner>k__BackingField;
			this.Element = original.<Element>k__BackingField;
			this.Required = original.<Required>k__BackingField;
			this.Incompatible = original.<Incompatible>k__BackingField;
		}

		// Token: 0x06003D79 RID: 15737 RVA: 0x0018E4E1 File Offset: 0x0018C6E1
		[CompilerGenerated]
		public void Deconstruct(out ContentPackage Owner, out XElement Element, out ImmutableArray<Identifier> Required, out ImmutableArray<Identifier> Incompatible)
		{
			Owner = this.Owner;
			Element = this.Element;
			Required = this.Required;
			Incompatible = this.Incompatible;
		}
	}
}
