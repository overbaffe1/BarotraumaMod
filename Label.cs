using System;

namespace Barotrauma
{
	// Token: 0x02000294 RID: 660
	internal class Label : EventAction
	{
		// Token: 0x17000F40 RID: 3904
		// (get) Token: 0x060039F8 RID: 14840 RVA: 0x0021D810 File Offset: 0x0021BA10
		// (set) Token: 0x060039F9 RID: 14841 RVA: 0x0021D818 File Offset: 0x0021BA18
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Name { get; set; }

		// Token: 0x060039FA RID: 14842 RVA: 0x0021D821 File Offset: 0x0021BA21
		public Label(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060039FB RID: 14843 RVA: 0x0021D82B File Offset: 0x0021BA2B
		public override bool IsFinished(ref string goTo)
		{
			return true;
		}

		// Token: 0x060039FC RID: 14844 RVA: 0x0021D82E File Offset: 0x0021BA2E
		public override bool SetGoToTarget(string goTo)
		{
			return goTo.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x060039FD RID: 14845 RVA: 0x0021D83D File Offset: 0x0021BA3D
		public override string ToDebugString()
		{
			return "[-] Label \"" + this.Name + "\"";
		}

		// Token: 0x060039FE RID: 14846 RVA: 0x0021D854 File Offset: 0x0021BA54
		public override void Reset()
		{
		}
	}
}
