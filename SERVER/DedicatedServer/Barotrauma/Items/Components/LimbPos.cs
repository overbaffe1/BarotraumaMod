using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C8 RID: 1224
	internal class LimbPos : ISerializableEntity
	{
		// Token: 0x170012C3 RID: 4803
		// (get) Token: 0x060045FD RID: 17917 RVA: 0x001C007C File Offset: 0x001BE27C
		// (set) Token: 0x060045FE RID: 17918 RVA: 0x001C0084 File Offset: 0x001BE284
		[Editable]
		public LimbType LimbType { get; set; }

		// Token: 0x170012C4 RID: 4804
		// (get) Token: 0x060045FF RID: 17919 RVA: 0x001C008D File Offset: 0x001BE28D
		// (set) Token: 0x06004600 RID: 17920 RVA: 0x001C0095 File Offset: 0x001BE295
		[Editable]
		public Vector2 Position { get; set; }

		// Token: 0x170012C5 RID: 4805
		// (get) Token: 0x06004601 RID: 17921 RVA: 0x001C00A0 File Offset: 0x001BE2A0
		public string Name
		{
			get
			{
				return this.LimbType.ToString();
			}
		}

		// Token: 0x170012C6 RID: 4806
		// (get) Token: 0x06004602 RID: 17922 RVA: 0x001C00C1 File Offset: 0x001BE2C1
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return null;
			}
		}

		// Token: 0x06004603 RID: 17923 RVA: 0x001C00C4 File Offset: 0x001BE2C4
		public LimbPos(LimbType limbType, Vector2 position, bool allowUsingLimb)
		{
			this.LimbType = limbType;
			this.Position = position;
			this.AllowUsingLimb = allowUsingLimb;
		}

		// Token: 0x040021A4 RID: 8612
		public bool AllowUsingLimb;
	}
}
