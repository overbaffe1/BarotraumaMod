using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059C RID: 1436
	public class LuaScriptServicesConfig : ILuaScriptServicesConfig, IService, IDisposable, IEquatable<LuaScriptServicesConfig>
	{
		// Token: 0x170015B2 RID: 5554
		// (get) Token: 0x06005737 RID: 22327 RVA: 0x002D3A0A File Offset: 0x002D1C0A
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

		// Token: 0x170015B3 RID: 5555
		// (get) Token: 0x06005738 RID: 22328 RVA: 0x002D3A16 File Offset: 0x002D1C16
		public bool SafeLuaIOEnabled
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170015B4 RID: 5556
		// (get) Token: 0x06005739 RID: 22329 RVA: 0x002D3A19 File Offset: 0x002D1C19
		public bool UseCaching
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600573A RID: 22330 RVA: 0x002D3A1C File Offset: 0x002D1C1C
		public void Dispose()
		{
		}

		// Token: 0x170015B5 RID: 5557
		// (get) Token: 0x0600573B RID: 22331 RVA: 0x002D3A1E File Offset: 0x002D1C1E
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600573C RID: 22332 RVA: 0x002D3A24 File Offset: 0x002D1C24
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

		// Token: 0x0600573D RID: 22333 RVA: 0x002D3A70 File Offset: 0x002D1C70
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

		// Token: 0x0600573E RID: 22334 RVA: 0x002D3AF8 File Offset: 0x002D1CF8
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(LuaScriptServicesConfig left, LuaScriptServicesConfig right)
		{
			return !(left == right);
		}

		// Token: 0x0600573F RID: 22335 RVA: 0x002D3B04 File Offset: 0x002D1D04
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(LuaScriptServicesConfig left, LuaScriptServicesConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06005740 RID: 22336 RVA: 0x002D3B18 File Offset: 0x002D1D18
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract);
		}

		// Token: 0x06005741 RID: 22337 RVA: 0x002D3B2A File Offset: 0x002D1D2A
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as LuaScriptServicesConfig);
		}

		// Token: 0x06005742 RID: 22338 RVA: 0x002D3B38 File Offset: 0x002D1D38
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(LuaScriptServicesConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract);
		}

		// Token: 0x06005744 RID: 22340 RVA: 0x002D3B5E File Offset: 0x002D1D5E
		[CompilerGenerated]
		protected LuaScriptServicesConfig([Nullable(1)] LuaScriptServicesConfig original)
		{
		}

		// Token: 0x06005745 RID: 22341 RVA: 0x002D3B66 File Offset: 0x002D1D66
		public LuaScriptServicesConfig()
		{
		}
	}
}
