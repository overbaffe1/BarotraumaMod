using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059A RID: 1434
	public class ConfigServiceConfig : IConfigServiceConfig, IService, IDisposable, IEquatable<ConfigServiceConfig>
	{
		// Token: 0x170015AC RID: 5548
		// (get) Token: 0x06005726 RID: 22310 RVA: 0x002D38AB File Offset: 0x002D1AAB
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

		// Token: 0x170015AD RID: 5549
		// (get) Token: 0x06005727 RID: 22311 RVA: 0x002D38B7 File Offset: 0x002D1AB7
		public string LocalConfigPathPartial
		{
			get
			{
				return "/Config/" + this.FileNamePattern + ".xml";
			}
		}

		// Token: 0x170015AE RID: 5550
		// (get) Token: 0x06005728 RID: 22312 RVA: 0x002D38CE File Offset: 0x002D1ACE
		public string FileNamePattern
		{
			get
			{
				return "<ConfigName>";
			}
		}

		// Token: 0x06005729 RID: 22313 RVA: 0x002D38D5 File Offset: 0x002D1AD5
		public void Dispose()
		{
		}

		// Token: 0x170015AF RID: 5551
		// (get) Token: 0x0600572A RID: 22314 RVA: 0x002D38D7 File Offset: 0x002D1AD7
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600572B RID: 22315 RVA: 0x002D38DC File Offset: 0x002D1ADC
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

		// Token: 0x0600572C RID: 22316 RVA: 0x002D3928 File Offset: 0x002D1B28
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

		// Token: 0x0600572D RID: 22317 RVA: 0x002D3994 File Offset: 0x002D1B94
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ConfigServiceConfig left, ConfigServiceConfig right)
		{
			return !(left == right);
		}

		// Token: 0x0600572E RID: 22318 RVA: 0x002D39A0 File Offset: 0x002D1BA0
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ConfigServiceConfig left, ConfigServiceConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600572F RID: 22319 RVA: 0x002D39B4 File Offset: 0x002D1BB4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract);
		}

		// Token: 0x06005730 RID: 22320 RVA: 0x002D39C6 File Offset: 0x002D1BC6
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ConfigServiceConfig);
		}

		// Token: 0x06005731 RID: 22321 RVA: 0x002D39D4 File Offset: 0x002D1BD4
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ConfigServiceConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract);
		}

		// Token: 0x06005733 RID: 22323 RVA: 0x002D39FA File Offset: 0x002D1BFA
		[CompilerGenerated]
		protected ConfigServiceConfig([Nullable(1)] ConfigServiceConfig original)
		{
		}

		// Token: 0x06005734 RID: 22324 RVA: 0x002D3A02 File Offset: 0x002D1C02
		public ConfigServiceConfig()
		{
		}
	}
}
