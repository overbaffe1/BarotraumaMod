using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000480 RID: 1152
	public class LuaScriptServicesConfig : ILuaScriptServicesConfig, IService, IDisposable, IEquatable<LuaScriptServicesConfig>
	{
		// Token: 0x1700104C RID: 4172
		// (get) Token: 0x06003DCE RID: 15822 RVA: 0x0018E9E6 File Offset: 0x0018CBE6
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(LuaScriptServicesConfig);
			}
		}

		// Token: 0x1700104D RID: 4173
		// (get) Token: 0x06003DCF RID: 15823 RVA: 0x0018E9F2 File Offset: 0x0018CBF2
		public bool SafeLuaIOEnabled
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700104E RID: 4174
		// (get) Token: 0x06003DD0 RID: 15824 RVA: 0x0018E9F5 File Offset: 0x0018CBF5
		public bool UseCaching
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06003DD1 RID: 15825 RVA: 0x0018E9F8 File Offset: 0x0018CBF8
		public void Dispose()
		{
		}

		// Token: 0x1700104F RID: 4175
		// (get) Token: 0x06003DD2 RID: 15826 RVA: 0x0018E9FA File Offset: 0x0018CBFA
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003DD3 RID: 15827 RVA: 0x0018EA00 File Offset: 0x0018CC00
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("LuaScriptServicesConfig");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003DD4 RID: 15828 RVA: 0x0018EA4C File Offset: 0x0018CC4C
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("SafeLuaIOEnabled = ");
			builder.Append(this.SafeLuaIOEnabled.ToString());
			builder.Append(", UseCaching = ");
			builder.Append(this.UseCaching.ToString());
			builder.Append(", IsDisposed = ");
			builder.Append(this.IsDisposed.ToString());
			return true;
		}

		// Token: 0x06003DD5 RID: 15829 RVA: 0x0018EAD4 File Offset: 0x0018CCD4
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(LuaScriptServicesConfig left, LuaScriptServicesConfig right)
		{
			return !(left == right);
		}

		// Token: 0x06003DD6 RID: 15830 RVA: 0x0018EAE0 File Offset: 0x0018CCE0
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(LuaScriptServicesConfig left, LuaScriptServicesConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003DD7 RID: 15831 RVA: 0x0018EAF4 File Offset: 0x0018CCF4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract);
		}

		// Token: 0x06003DD8 RID: 15832 RVA: 0x0018EB06 File Offset: 0x0018CD06
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as LuaScriptServicesConfig);
		}

		// Token: 0x06003DD9 RID: 15833 RVA: 0x0018EB14 File Offset: 0x0018CD14
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(LuaScriptServicesConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract);
		}

		// Token: 0x06003DDB RID: 15835 RVA: 0x0018EB3A File Offset: 0x0018CD3A
		[CompilerGenerated]
		protected LuaScriptServicesConfig([Nullable(1)] LuaScriptServicesConfig original)
		{
		}

		// Token: 0x06003DDC RID: 15836 RVA: 0x0018EB42 File Offset: 0x0018CD42
		public LuaScriptServicesConfig()
		{
		}
	}
}
