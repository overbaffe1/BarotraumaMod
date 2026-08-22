using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004AF RID: 1199
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class PipeEndpoint : Endpoint
	{
		// Token: 0x17001438 RID: 5176
		// (get) Token: 0x06004F4F RID: 20303 RVA: 0x002AE7FA File Offset: 0x002AC9FA
		public override string StringRepresentation
		{
			get
			{
				return "PIPE";
			}
		}

		// Token: 0x17001439 RID: 5177
		// (get) Token: 0x06004F50 RID: 20304 RVA: 0x002AE801 File Offset: 0x002ACA01
		public override LocalizedString ServerTypeString
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x06004F51 RID: 20305 RVA: 0x002AE808 File Offset: 0x002ACA08
		public PipeEndpoint() : base(new PipeAddress())
		{
		}

		// Token: 0x06004F52 RID: 20306 RVA: 0x002AE815 File Offset: 0x002ACA15
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			return obj is PipeEndpoint;
		}

		// Token: 0x06004F53 RID: 20307 RVA: 0x002AE820 File Offset: 0x002ACA20
		public override int GetHashCode()
		{
			return 1;
		}

		// Token: 0x06004F54 RID: 20308 RVA: 0x002AE823 File Offset: 0x002ACA23
		public static bool operator ==(PipeEndpoint a, PipeEndpoint b)
		{
			return true;
		}

		// Token: 0x06004F55 RID: 20309 RVA: 0x002AE826 File Offset: 0x002ACA26
		public static bool operator !=(PipeEndpoint a, PipeEndpoint b)
		{
			return !(a == b);
		}
	}
}
