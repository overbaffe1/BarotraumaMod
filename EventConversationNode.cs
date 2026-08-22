using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200010E RID: 270
	internal class EventConversationNode : EventTextDisplayNode
	{
		// Token: 0x060024E0 RID: 9440 RVA: 0x00174DDC File Offset: 0x00172FDC
		[NullableContext(1)]
		public EventConversationNode(Type type, string name) : base(type, name)
		{
		}

		// Token: 0x170009E9 RID: 2537
		// (get) Token: 0x060024E1 RID: 9441 RVA: 0x00174DE6 File Offset: 0x00172FE6
		protected override bool ShowOptions
		{
			get
			{
				return true;
			}
		}
	}
}
