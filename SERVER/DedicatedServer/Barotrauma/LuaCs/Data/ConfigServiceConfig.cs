using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047E RID: 1150
	public class ConfigServiceConfig : IConfigServiceConfig, IService, IDisposable, IEquatable<ConfigServiceConfig>
	{
		// Token: 0x17001046 RID: 4166
		// (get) Token: 0x06003DBD RID: 15805 RVA: 0x0018E886 File Offset: 0x0018CA86
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ConfigServiceConfig);
			}
		}

		// Token: 0x17001047 RID: 4167
		// (get) Token: 0x06003DBE RID: 15806 RVA: 0x0018E892 File Offset: 0x0018CA92
		public string LocalConfigPathPartial
		{
			get
			{
				return "/Config/" + this.FileNamePattern + ".xml";
			}
		}

		// Token: 0x17001048 RID: 4168
		// (get) Token: 0x06003DBF RID: 15807 RVA: 0x0018E8A9 File Offset: 0x0018CAA9
		public string FileNamePattern
		{
			get
			{
				return "<ConfigName>";
			}
		}

		// Token: 0x06003DC0 RID: 15808 RVA: 0x0018E8B0 File Offset: 0x0018CAB0
		public void Dispose()
		{
		}

		// Token: 0x17001049 RID: 4169
		// (get) Token: 0x06003DC1 RID: 15809 RVA: 0x0018E8B2 File Offset: 0x0018CAB2
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003DC2 RID: 15810 RVA: 0x0018E8B8 File Offset: 0x0018CAB8
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ConfigServiceConfig");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003DC3 RID: 15811 RVA: 0x0018E904 File Offset: 0x0018CB04
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("LocalConfigPathPartial = ");
			builder.Append(this.LocalConfigPathPartial);
			builder.Append(", FileNamePattern = ");
			builder.Append(this.FileNamePattern);
			builder.Append(", IsDisposed = ");
			builder.Append(this.IsDisposed.ToString());
			return true;
		}

		// Token: 0x06003DC4 RID: 15812 RVA: 0x0018E970 File Offset: 0x0018CB70
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigServiceConfig left, ConfigServiceConfig right)
		{
			return !(left == right);
		}

		// Token: 0x06003DC5 RID: 15813 RVA: 0x0018E97C File Offset: 0x0018CB7C
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigServiceConfig left, ConfigServiceConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003DC6 RID: 15814 RVA: 0x0018E990 File Offset: 0x0018CB90
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract);
		}

		// Token: 0x06003DC7 RID: 15815 RVA: 0x0018E9A2 File Offset: 0x0018CBA2
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigServiceConfig);
		}

		// Token: 0x06003DC8 RID: 15816 RVA: 0x0018E9B0 File Offset: 0x0018CBB0
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigServiceConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract);
		}

		// Token: 0x06003DCA RID: 15818 RVA: 0x0018E9D6 File Offset: 0x0018CBD6
		[CompilerGenerated]
		protected ConfigServiceConfig([Nullable(1)] ConfigServiceConfig original)
		{
		}

		// Token: 0x06003DCB RID: 15819 RVA: 0x0018E9DE File Offset: 0x0018CBDE
		public ConfigServiceConfig()
		{
		}
	}
}
