using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000166 RID: 358
	internal interface ISteerable
	{
		// Token: 0x17000ACB RID: 2763
		// (get) Token: 0x06002AF8 RID: 11000
		// (set) Token: 0x06002AF9 RID: 11001
		Vector2 Steering { get; set; }

		// Token: 0x17000ACC RID: 2764
		// (get) Token: 0x06002AFA RID: 11002
		Vector2 Velocity { get; }

		// Token: 0x17000ACD RID: 2765
		// (get) Token: 0x06002AFB RID: 11003
		Vector2 SimPosition { get; }

		// Token: 0x17000ACE RID: 2766
		// (get) Token: 0x06002AFC RID: 11004
		Vector2 WorldPosition { get; }
	}
}
