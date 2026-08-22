using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003B2 RID: 946
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class PipeEndpoint : Endpoint
	{
		// Token: 0x17000F3D RID: 3901
		// (get) Token: 0x0600377A RID: 14202 RVA: 0x00175CBE File Offset: 0x00173EBE
		public override string StringRepresentation
		{
			get
			{
				return "PIPE";
			}
		}

		// Token: 0x17000F3E RID: 3902
		// (get) Token: 0x0600377B RID: 14203 RVA: 0x00175CC5 File Offset: 0x00173EC5
		public override LocalizedString ServerTypeString
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x0600377C RID: 14204 RVA: 0x00175CCC File Offset: 0x00173ECC
		public PipeEndpoint() : base(new PipeAddress())
		{
		}

		// Token: 0x0600377D RID: 14205 RVA: 0x00175CD9 File Offset: 0x00173ED9
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			return obj is PipeEndpoint;
		}

		// Token: 0x0600377E RID: 14206 RVA: 0x00175CE4 File Offset: 0x00173EE4
		public override int GetHashCode()
		{
			return 1;
		}

		// Token: 0x0600377F RID: 14207 RVA: 0x00175CE7 File Offset: 0x00173EE7
		public static bool operator ==(PipeEndpoint a, PipeEndpoint b)
		{
			return true;
		}

		// Token: 0x06003780 RID: 14208 RVA: 0x00175CEA File Offset: 0x00173EEA
		public static bool operator !=(PipeEndpoint a, PipeEndpoint b)
		{
			return !(a == b);
		}
	}
}
