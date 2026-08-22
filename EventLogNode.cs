using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200010F RID: 271
	internal class EventLogNode : EventTextDisplayNode
	{
		// Token: 0x060024E2 RID: 9442 RVA: 0x00174DE9 File Offset: 0x00172FE9
		[NullableContext(1)]
		public EventLogNode(Type type, string name) : base(type, name)
		{
		}

		// Token: 0x170009EA RID: 2538
		// (get) Token: 0x060024E3 RID: 9443 RVA: 0x00174DF3 File Offset: 0x00172FF3
		protected override bool ShowOptions
		{
			get
			{
				return false;
			}
		}
	}
}
