using System;

namespace Barotrauma
{
	// Token: 0x020001A2 RID: 418
	internal class Label : EventAction
	{
		// Token: 0x170008F0 RID: 2288
		// (get) Token: 0x06001F3A RID: 7994 RVA: 0x000D83CD File Offset: 0x000D65CD
		// (set) Token: 0x06001F3B RID: 7995 RVA: 0x000D83D5 File Offset: 0x000D65D5
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Name { get; set; }

		// Token: 0x06001F3C RID: 7996 RVA: 0x000D83DE File Offset: 0x000D65DE
		public Label(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x000D83E8 File Offset: 0x000D65E8
		public override bool IsFinished(ref string goTo)
		{
			return true;
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x000D83EB File Offset: 0x000D65EB
		public override bool SetGoToTarget(string goTo)
		{
			return goTo.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x000D83FA File Offset: 0x000D65FA
		public override string ToDebugString()
		{
			return "[-] Label \"" + this.Name + "\"";
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x000D8411 File Offset: 0x000D6611
		public override void Reset()
		{
		}
	}
}
