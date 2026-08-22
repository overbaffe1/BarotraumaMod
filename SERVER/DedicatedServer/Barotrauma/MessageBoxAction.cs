using System;

namespace Barotrauma
{
	// Token: 0x020001A4 RID: 420
	internal class MessageBoxAction : EventAction
	{
		// Token: 0x170008F5 RID: 2293
		// (get) Token: 0x06001F4E RID: 8014 RVA: 0x000D85B6 File Offset: 0x000D67B6
		// (set) Token: 0x06001F4F RID: 8015 RVA: 0x000D85BE File Offset: 0x000D67BE
		[Serialize(MessageBoxAction.ActionType.Create, IsPropertySaveable.Yes, "What do you want to do with the message box (Create, ConnectObjective, Close, Clear)?", "", false)]
		public MessageBoxAction.ActionType Type { get; set; }

		// Token: 0x170008F6 RID: 2294
		// (get) Token: 0x06001F50 RID: 8016 RVA: 0x000D85C7 File Offset: 0x000D67C7
		// (set) Token: 0x06001F51 RID: 8017 RVA: 0x000D85CF File Offset: 0x000D67CF
		[Serialize("", IsPropertySaveable.Yes, "Optional identifier of the tutorial \"segment\" that can be referenced by other event actions.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x170008F7 RID: 2295
		// (get) Token: 0x06001F52 RID: 8018 RVA: 0x000D85D8 File Offset: 0x000D67D8
		// (set) Token: 0x06001F53 RID: 8019 RVA: 0x000D85E0 File Offset: 0x000D67E0
		[Serialize("", IsPropertySaveable.Yes, "An arbitrary tag given to the message box. Only required if you're intending to close or clear the box with another MessageBoxAction later.", "", false)]
		public string Tag { get; set; }

		// Token: 0x170008F8 RID: 2296
		// (get) Token: 0x06001F54 RID: 8020 RVA: 0x000D85E9 File Offset: 0x000D67E9
		// (set) Token: 0x06001F55 RID: 8021 RVA: 0x000D85F1 File Offset: 0x000D67F1
		[Serialize("", IsPropertySaveable.Yes, "Text displayed in the header of the message box. Can be either the text as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Header { get; set; }

		// Token: 0x170008F9 RID: 2297
		// (get) Token: 0x06001F56 RID: 8022 RVA: 0x000D85FA File Offset: 0x000D67FA
		// (set) Token: 0x06001F57 RID: 8023 RVA: 0x000D8602 File Offset: 0x000D6802
		[Serialize("", IsPropertySaveable.Yes, "Text displayed in the body of the message box. Can be either the text as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Text { get; set; }

		// Token: 0x170008FA RID: 2298
		// (get) Token: 0x06001F58 RID: 8024 RVA: 0x000D860B File Offset: 0x000D680B
		// (set) Token: 0x06001F59 RID: 8025 RVA: 0x000D8613 File Offset: 0x000D6813
		[Serialize("", IsPropertySaveable.Yes, "Style of the icon displayed in the corner of the message box (optional). The style must be defined in a UIStyle file.", "", false)]
		public string IconStyle { get; set; }

		// Token: 0x170008FB RID: 2299
		// (get) Token: 0x06001F5A RID: 8026 RVA: 0x000D861C File Offset: 0x000D681C
		// (set) Token: 0x06001F5B RID: 8027 RVA: 0x000D8624 File Offset: 0x000D6824
		[Serialize(false, IsPropertySaveable.Yes, "Should the button that closes the box be hidden? If it is hidden, you must close the box manually using another MessageBoxAction.", "", false)]
		public bool HideCloseButton { get; set; }

		// Token: 0x170008FC RID: 2300
		// (get) Token: 0x06001F5C RID: 8028 RVA: 0x000D862D File Offset: 0x000D682D
		// (set) Token: 0x06001F5D RID: 8029 RVA: 0x000D8635 File Offset: 0x000D6835
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to show the message box to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008FD RID: 2301
		// (get) Token: 0x06001F5E RID: 8030 RVA: 0x000D863E File Offset: 0x000D683E
		// (set) Token: 0x06001F5F RID: 8031 RVA: 0x000D8646 File Offset: 0x000D6846
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed on some input (e.g. Select, Use, CrewOrders).", "", false)]
		public string CloseOnInput { get; set; }

		// Token: 0x170008FE RID: 2302
		// (get) Token: 0x06001F60 RID: 8032 RVA: 0x000D864F File Offset: 0x000D684F
		// (set) Token: 0x06001F61 RID: 8033 RVA: 0x000D8657 File Offset: 0x000D6857
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user selects an item that has this tag.", "", false)]
		public Identifier CloseOnSelectTag { get; set; }

		// Token: 0x170008FF RID: 2303
		// (get) Token: 0x06001F62 RID: 8034 RVA: 0x000D8660 File Offset: 0x000D6860
		// (set) Token: 0x06001F63 RID: 8035 RVA: 0x000D8668 File Offset: 0x000D6868
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user picks up an item that has this tag.", "", false)]
		public Identifier CloseOnPickUpTag { get; set; }

		// Token: 0x17000900 RID: 2304
		// (get) Token: 0x06001F64 RID: 8036 RVA: 0x000D8671 File Offset: 0x000D6871
		// (set) Token: 0x06001F65 RID: 8037 RVA: 0x000D8679 File Offset: 0x000D6879
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user equips an item that has this tag.", "", false)]
		public Identifier CloseOnEquipTag { get; set; }

		// Token: 0x17000901 RID: 2305
		// (get) Token: 0x06001F66 RID: 8038 RVA: 0x000D8682 File Offset: 0x000D6882
		// (set) Token: 0x06001F67 RID: 8039 RVA: 0x000D868A File Offset: 0x000D688A
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user exits a room with this name.", "", false)]
		public Identifier CloseOnExitRoomName { get; set; }

		// Token: 0x17000902 RID: 2306
		// (get) Token: 0x06001F68 RID: 8040 RVA: 0x000D8693 File Offset: 0x000D6893
		// (set) Token: 0x06001F69 RID: 8041 RVA: 0x000D869B File Offset: 0x000D689B
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user is in a room with this name.", "", false)]
		public Identifier CloseOnInRoomName { get; set; }

		// Token: 0x17000903 RID: 2307
		// (get) Token: 0x06001F6A RID: 8042 RVA: 0x000D86A4 File Offset: 0x000D68A4
		// (set) Token: 0x06001F6B RID: 8043 RVA: 0x000D86AC File Offset: 0x000D68AC
		[Serialize("", IsPropertySaveable.Yes, "Optional tag that will be used to get the text for the objective that is displayed on the screen.", "", false)]
		public Identifier ObjectiveTag { get; set; }

		// Token: 0x17000904 RID: 2308
		// (get) Token: 0x06001F6C RID: 8044 RVA: 0x000D86B5 File Offset: 0x000D68B5
		// (set) Token: 0x06001F6D RID: 8045 RVA: 0x000D86BD File Offset: 0x000D68BD
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ObjectiveCanBeCompleted { get; set; }

		// Token: 0x17000905 RID: 2309
		// (get) Token: 0x06001F6E RID: 8046 RVA: 0x000D86C6 File Offset: 0x000D68C6
		// (set) Token: 0x06001F6F RID: 8047 RVA: 0x000D86CE File Offset: 0x000D68CE
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier ParentObjectiveId { get; set; }

		// Token: 0x06001F70 RID: 8048 RVA: 0x000D86D8 File Offset: 0x000D68D8
		public MessageBoxAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Identifier.IsEmpty)
			{
				this.Identifier = element.GetAttributeIdentifier("id", Identifier.Empty);
			}
		}

		// Token: 0x06001F71 RID: 8049 RVA: 0x000D8713 File Offset: 0x000D6913
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x000D8725 File Offset: 0x000D6925
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06001F73 RID: 8051 RVA: 0x000D872D File Offset: 0x000D692D
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x000D8736 File Offset: 0x000D6936
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " MessageBoxAction";
		}

		// Token: 0x04000EFD RID: 3837
		private bool isFinished;

		// Token: 0x02000916 RID: 2326
		public enum ActionType
		{
			// Token: 0x040031F6 RID: 12790
			Create,
			// Token: 0x040031F7 RID: 12791
			ConnectObjective,
			// Token: 0x040031F8 RID: 12792
			Close,
			// Token: 0x040031F9 RID: 12793
			Clear
		}
	}
}
