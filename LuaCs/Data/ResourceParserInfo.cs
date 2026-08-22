using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058B RID: 1419
	public class ResourceParserInfo : IEquatable<ResourceParserInfo>
	{
		// Token: 0x060056D5 RID: 22229 RVA: 0x002D31C7 File Offset: 0x002D13C7
		public ResourceParserInfo([NotNull] ContentPackage Owner, [NotNull] XElement Element, ImmutableArray<Identifier> Required, ImmutableArray<Identifier> Incompatible)
		{
			this.Owner = Owner;
			this.Element = Element;
			this.Required = Required;
			this.Incompatible = Incompatible;
			base..ctor();
		}

		// Token: 0x1700157F RID: 5503
		// (get) Token: 0x060056D6 RID: 22230 RVA: 0x002D31EC File Offset: 0x002D13EC
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

		// Token: 0x17001580 RID: 5504
		// (get) Token: 0x060056D7 RID: 22231 RVA: 0x002D31F8 File Offset: 0x002D13F8
		// (set) Token: 0x060056D8 RID: 22232 RVA: 0x002D3200 File Offset: 0x002D1400
		public ContentPackage Owner { get; set; }

		// Token: 0x17001581 RID: 5505
		// (get) Token: 0x060056D9 RID: 22233 RVA: 0x002D3209 File Offset: 0x002D1409
		// (set) Token: 0x060056DA RID: 22234 RVA: 0x002D3211 File Offset: 0x002D1411
		public XElement Element { get; set; }

		// Token: 0x17001582 RID: 5506
		// (get) Token: 0x060056DB RID: 22235 RVA: 0x002D321A File Offset: 0x002D141A
		// (set) Token: 0x060056DC RID: 22236 RVA: 0x002D3222 File Offset: 0x002D1422
		public ImmutableArray<Identifier> Required { get; set; }

		// Token: 0x17001583 RID: 5507
		// (get) Token: 0x060056DD RID: 22237 RVA: 0x002D322B File Offset: 0x002D142B
		// (set) Token: 0x060056DE RID: 22238 RVA: 0x002D3233 File Offset: 0x002D1433
		public ImmutableArray<Identifier> Incompatible { get; set; }

		// Token: 0x060056DF RID: 22239 RVA: 0x002D323C File Offset: 0x002D143C
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

		// Token: 0x060056E0 RID: 22240 RVA: 0x002D3288 File Offset: 0x002D1488
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

		// Token: 0x060056E1 RID: 22241 RVA: 0x002D331B File Offset: 0x002D151B
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ResourceParserInfo left, ResourceParserInfo right)
		{
			return !(left == right);
		}

		// Token: 0x060056E2 RID: 22242 RVA: 0x002D3327 File Offset: 0x002D1527
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ResourceParserInfo left, ResourceParserInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x060056E3 RID: 22243 RVA: 0x002D333C File Offset: 0x002D153C
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<Owner>k__BackingField)) * -1521134295 + EqualityComparer<XElement>.Default.GetHashCode(this.<Element>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Required>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Incompatible>k__BackingField);
		}

		// Token: 0x060056E4 RID: 22244 RVA: 0x002D33B5 File Offset: 0x002D15B5
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ResourceParserInfo);
		}

		// Token: 0x060056E5 RID: 22245 RVA: 0x002D33C4 File Offset: 0x002D15C4
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ResourceParserInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<ContentPackage>.Default.Equals(this.<Owner>k__BackingField, other.<Owner>k__BackingField) && EqualityComparer<XElement>.Default.Equals(this.<Element>k__BackingField, other.<Element>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Required>k__BackingField, other.<Required>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Incompatible>k__BackingField, other.<Incompatible>k__BackingField));
		}

		// Token: 0x060056E7 RID: 22247 RVA: 0x002D3455 File Offset: 0x002D1655
		[CompilerGenerated]
		protected ResourceParserInfo([Nullable(1)] ResourceParserInfo original)
		{
			this.Owner = original.<Owner>k__BackingField;
			this.Element = original.<Element>k__BackingField;
			this.Required = original.<Required>k__BackingField;
			this.Incompatible = original.<Incompatible>k__BackingField;
		}

		// Token: 0x060056E8 RID: 22248 RVA: 0x002D348D File Offset: 0x002D168D
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
