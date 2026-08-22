using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F5 RID: 1525
	internal class LimbPos : ISerializableEntity
	{
		// Token: 0x17001937 RID: 6455
		// (get) Token: 0x060063A3 RID: 25507 RVA: 0x0033E1D0 File Offset: 0x0033C3D0
		// (set) Token: 0x060063A4 RID: 25508 RVA: 0x0033E1D8 File Offset: 0x0033C3D8
		[Editable]
		public LimbType LimbType { get; set; }

		// Token: 0x17001938 RID: 6456
		// (get) Token: 0x060063A5 RID: 25509 RVA: 0x0033E1E1 File Offset: 0x0033C3E1
		// (set) Token: 0x060063A6 RID: 25510 RVA: 0x0033E1E9 File Offset: 0x0033C3E9
		[Editable]
		public Vector2 Position { get; set; }

		// Token: 0x17001939 RID: 6457
		// (get) Token: 0x060063A7 RID: 25511 RVA: 0x0033E1F4 File Offset: 0x0033C3F4
		public string Name
		{
			get
			{
				return this.LimbType.ToString();
			}
		}

		// Token: 0x1700193A RID: 6458
		// (get) Token: 0x060063A8 RID: 25512 RVA: 0x0033E215 File Offset: 0x0033C415
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060063A9 RID: 25513 RVA: 0x0033E218 File Offset: 0x0033C418
		public LimbPos(LimbType limbType, Vector2 position, bool allowUsingLimb)
		{
			this.LimbType = limbType;
			this.Position = position;
			this.AllowUsingLimb = allowUsingLimb;
		}

		// Token: 0x040033AB RID: 13227
		public bool AllowUsingLimb;
	}
}
