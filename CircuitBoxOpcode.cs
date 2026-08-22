using System;

namespace Barotrauma
{
	// Token: 0x02000200 RID: 512
	public enum CircuitBoxOpcode
	{
		// Token: 0x04001BA7 RID: 7079
		Error,
		// Token: 0x04001BA8 RID: 7080
		Cursor,
		// Token: 0x04001BA9 RID: 7081
		AddComponent,
		// Token: 0x04001BAA RID: 7082
		MoveComponent,
		// Token: 0x04001BAB RID: 7083
		AddWire,
		// Token: 0x04001BAC RID: 7084
		RemoveWire,
		// Token: 0x04001BAD RID: 7085
		SelectComponents,
		// Token: 0x04001BAE RID: 7086
		SelectWires,
		// Token: 0x04001BAF RID: 7087
		UpdateSelection,
		// Token: 0x04001BB0 RID: 7088
		DeleteComponent,
		// Token: 0x04001BB1 RID: 7089
		RenameLabel,
		// Token: 0x04001BB2 RID: 7090
		AddLabel,
		// Token: 0x04001BB3 RID: 7091
		RemoveLabel,
		// Token: 0x04001BB4 RID: 7092
		ResizeLabel,
		// Token: 0x04001BB5 RID: 7093
		RenameConnections,
		// Token: 0x04001BB6 RID: 7094
		ServerInitialize
	}
}
