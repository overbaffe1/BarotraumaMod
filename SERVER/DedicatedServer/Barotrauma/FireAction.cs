using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200019B RID: 411
	internal class FireAction : EventAction
	{
		// Token: 0x170008DB RID: 2267
		// (get) Token: 0x06001EEF RID: 7919 RVA: 0x000D7939 File Offset: 0x000D5B39
		// (set) Token: 0x06001EF0 RID: 7920 RVA: 0x000D7941 File Offset: 0x000D5B41
		[Serialize(10f, IsPropertySaveable.Yes, "Size of the fire (width in pixels).", "", false)]
		public float Size { get; set; }

		// Token: 0x170008DC RID: 2268
		// (get) Token: 0x06001EF1 RID: 7921 RVA: 0x000D794A File Offset: 0x000D5B4A
		// (set) Token: 0x06001EF2 RID: 7922 RVA: 0x000D7952 File Offset: 0x000D5B52
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to start the fire at.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001EF3 RID: 7923 RVA: 0x000D795B File Offset: 0x000D5B5B
		public FireAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001EF4 RID: 7924 RVA: 0x000D7965 File Offset: 0x000D5B65
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x000D796D File Offset: 0x000D5B6D
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x000D7978 File Offset: 0x000D5B78
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

		// Token: 0x06001EF7 RID: 7927 RVA: 0x000D7A08 File Offset: 0x000D5C08
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

		// Token: 0x04000ECC RID: 3788
		private bool isFinished;
	}
}
