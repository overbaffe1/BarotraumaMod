using System;

namespace Barotrauma
{
	// Token: 0x020001BA RID: 442
	internal class TutorialIconAction : EventAction
	{
		// Token: 0x1700096E RID: 2414
		// (get) Token: 0x060020F4 RID: 8436 RVA: 0x000DE509 File Offset: 0x000DC709
		// (set) Token: 0x060020F5 RID: 8437 RVA: 0x000DE511 File Offset: 0x000DC711
		[Serialize(TutorialIconAction.ActionType.Add, IsPropertySaveable.Yes, "What to do with the icon. Add = add an icon, Remove = remove the icon that has the specific target and style, RemoveTarget = remove all icons assigned to the specific target, RemoveIcon = remove all icons with the specific style, Remove = remove all icons.", "", false)]
		public TutorialIconAction.ActionType Type { get; set; }

		// Token: 0x1700096F RID: 2415
		// (get) Token: 0x060020F6 RID: 8438 RVA: 0x000DE51A File Offset: 0x000DC71A
		// (set) Token: 0x060020F7 RID: 8439 RVA: 0x000DE522 File Offset: 0x000DC722
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target to assign the icon to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000970 RID: 2416
		// (get) Token: 0x060020F8 RID: 8440 RVA: 0x000DE52B File Offset: 0x000DC72B
		// (set) Token: 0x060020F9 RID: 8441 RVA: 0x000DE533 File Offset: 0x000DC733
		[Serialize("", IsPropertySaveable.Yes, "Style of the icon.", "", false)]
		public Identifier IconStyle { get; set; }

		// Token: 0x060020FA RID: 8442 RVA: 0x000DE53C File Offset: 0x000DC73C
		public TutorialIconAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060020FB RID: 8443 RVA: 0x000DE546 File Offset: 0x000DC746
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x060020FC RID: 8444 RVA: 0x000DE558 File Offset: 0x000DC758
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x000DE560 File Offset: 0x000DC760
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000F90 RID: 3984
		private bool isFinished;

		// Token: 0x0200093F RID: 2367
		public enum ActionType
		{
			// Token: 0x0400327E RID: 12926
			Add,
			// Token: 0x0400327F RID: 12927
			Remove,
			// Token: 0x04003280 RID: 12928
			RemoveTarget,
			// Token: 0x04003281 RID: 12929
			RemoveIcon,
			// Token: 0x04003282 RID: 12930
			Clear
		}
	}
}
