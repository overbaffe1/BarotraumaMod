using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200028E RID: 654
	internal class FireAction : EventAction
	{
		// Token: 0x17000F2F RID: 3887
		// (get) Token: 0x060039B9 RID: 14777 RVA: 0x0021CDED File Offset: 0x0021AFED
		// (set) Token: 0x060039BA RID: 14778 RVA: 0x0021CDF5 File Offset: 0x0021AFF5
		[Serialize(10f, IsPropertySaveable.Yes, "Size of the fire (width in pixels).", "", false)]
		public float Size { get; set; }

		// Token: 0x17000F30 RID: 3888
		// (get) Token: 0x060039BB RID: 14779 RVA: 0x0021CDFE File Offset: 0x0021AFFE
		// (set) Token: 0x060039BC RID: 14780 RVA: 0x0021CE06 File Offset: 0x0021B006
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to start the fire at.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060039BD RID: 14781 RVA: 0x0021CE0F File Offset: 0x0021B00F
		public FireAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060039BE RID: 14782 RVA: 0x0021CE19 File Offset: 0x0021B019
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060039BF RID: 14783 RVA: 0x0021CE21 File Offset: 0x0021B021
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039C0 RID: 14784 RVA: 0x0021CE2C File Offset: 0x0021B02C
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			foreach (Entity target in targets)
			{
				Vector2 pos = target.WorldPosition;
				FireSource newFire = new FireSource(pos, null, null, false);
				newFire.Size = new Vector2(this.Size, this.Size);
			}
			this.isFinished = true;
		}

		// Token: 0x060039C1 RID: 14785 RVA: 0x0021CEBC File Offset: 0x0021B0BC
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("FireAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Size: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Size.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DC3 RID: 7619
		private bool isFinished;
	}
}
