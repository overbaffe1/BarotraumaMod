using System;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200004A RID: 74
	internal class MessageBoxAction : EventAction
	{
		// Token: 0x06000AA6 RID: 2726 RVA: 0x00062718 File Offset: 0x00060918
		public void CreateMessageBox()
		{
			RichString headerText = TextManager.Get(this.Header);
			RichString text = RichString.Rich(TextManager.ParseInputTypes(TextManager.Get(this.Text).Fallback(this.Text.ToString(), true), true), null);
			LocalizedString[] buttons = Array.Empty<LocalizedString>();
			string tag = this.Tag;
			string iconStyle = this.IconStyle;
			Func<bool> autoCloseCondition = this.GetAutoCloseCondition();
			bool hideCloseButton = this.HideCloseButton;
			new GUIMessageBox(headerText, text, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.Tutorial, tag, null, iconStyle, null, autoCloseCondition, hideCloseButton).FlashOnAutoCloseCondition = true;
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x000627B8 File Offset: 0x000609B8
		private Func<bool> GetAutoCloseCondition()
		{
			Character character = this.ParentEvent.GetTargets(this.TargetTag).FirstOrDefault<Entity>() as Character;
			Func<bool> autoCloseCondition = null;
			InputType closeOnInput;
			if (!string.IsNullOrEmpty(this.CloseOnInput) && Enum.TryParse<InputType>(this.CloseOnInput, true, out closeOnInput))
			{
				autoCloseCondition = (() => PlayerInput.KeyDown(closeOnInput));
			}
			else if (!this.CloseOnSelectTag.IsEmpty)
			{
				autoCloseCondition = delegate()
				{
					Character character = character;
					return ((character != null) ? character.SelectedItem : null) != null && character.SelectedItem.HasTag(this.CloseOnSelectTag);
				};
			}
			else if (!this.CloseOnPickUpTag.IsEmpty)
			{
				autoCloseCondition = delegate()
				{
					Character character = character;
					return ((character != null) ? character.Inventory : null) != null && character.Inventory.FindItemByTag(this.CloseOnPickUpTag, true) != null;
				};
			}
			else if (!this.CloseOnEquipTag.IsEmpty)
			{
				autoCloseCondition = (() => character != null && character.HasEquippedItem(this.CloseOnEquipTag, true, null));
			}
			else if (!this.CloseOnExitRoomName.IsEmpty)
			{
				autoCloseCondition = delegate()
				{
					Character character = character;
					if (((character != null) ? character.CurrentHull : null) != null)
					{
						Identifier identifier = character.CurrentHull.RoomName.ToIdentifier();
						Identifier closeOnExitRoomName = this.CloseOnExitRoomName;
						return identifier != closeOnExitRoomName;
					}
					return true;
				};
			}
			else if (!this.CloseOnInRoomName.IsEmpty)
			{
				autoCloseCondition = delegate()
				{
					Character character = character;
					if (((character != null) ? character.CurrentHull : null) != null)
					{
						Identifier identifier = character.CurrentHull.RoomName.ToIdentifier();
						Identifier closeOnInRoomName = this.CloseOnInRoomName;
						return identifier == closeOnInRoomName;
					}
					return false;
				};
			}
			return autoCloseCondition;
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000AA8 RID: 2728 RVA: 0x000628C2 File Offset: 0x00060AC2
		// (set) Token: 0x06000AA9 RID: 2729 RVA: 0x000628CA File Offset: 0x00060ACA
		[Serialize(MessageBoxAction.ActionType.Create, IsPropertySaveable.Yes, "What do you want to do with the message box (Create, ConnectObjective, Close, Clear)?", "", false)]
		public MessageBoxAction.ActionType Type { get; set; }

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000AAA RID: 2730 RVA: 0x000628D3 File Offset: 0x00060AD3
		// (set) Token: 0x06000AAB RID: 2731 RVA: 0x000628DB File Offset: 0x00060ADB
		[Serialize("", IsPropertySaveable.Yes, "Optional identifier of the tutorial \"segment\" that can be referenced by other event actions.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000AAC RID: 2732 RVA: 0x000628E4 File Offset: 0x00060AE4
		// (set) Token: 0x06000AAD RID: 2733 RVA: 0x000628EC File Offset: 0x00060AEC
		[Serialize("", IsPropertySaveable.Yes, "An arbitrary tag given to the message box. Only required if you're intending to close or clear the box with another MessageBoxAction later.", "", false)]
		public string Tag { get; set; }

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06000AAE RID: 2734 RVA: 0x000628F5 File Offset: 0x00060AF5
		// (set) Token: 0x06000AAF RID: 2735 RVA: 0x000628FD File Offset: 0x00060AFD
		[Serialize("", IsPropertySaveable.Yes, "Text displayed in the header of the message box. Can be either the text as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Header { get; set; }

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06000AB0 RID: 2736 RVA: 0x00062906 File Offset: 0x00060B06
		// (set) Token: 0x06000AB1 RID: 2737 RVA: 0x0006290E File Offset: 0x00060B0E
		[Serialize("", IsPropertySaveable.Yes, "Text displayed in the body of the message box. Can be either the text as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Text { get; set; }

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06000AB2 RID: 2738 RVA: 0x00062917 File Offset: 0x00060B17
		// (set) Token: 0x06000AB3 RID: 2739 RVA: 0x0006291F File Offset: 0x00060B1F
		[Serialize("", IsPropertySaveable.Yes, "Style of the icon displayed in the corner of the message box (optional). The style must be defined in a UIStyle file.", "", false)]
		public string IconStyle { get; set; }

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000AB4 RID: 2740 RVA: 0x00062928 File Offset: 0x00060B28
		// (set) Token: 0x06000AB5 RID: 2741 RVA: 0x00062930 File Offset: 0x00060B30
		[Serialize(false, IsPropertySaveable.Yes, "Should the button that closes the box be hidden? If it is hidden, you must close the box manually using another MessageBoxAction.", "", false)]
		public bool HideCloseButton { get; set; }

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000AB6 RID: 2742 RVA: 0x00062939 File Offset: 0x00060B39
		// (set) Token: 0x06000AB7 RID: 2743 RVA: 0x00062941 File Offset: 0x00060B41
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to show the message box to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000AB8 RID: 2744 RVA: 0x0006294A File Offset: 0x00060B4A
		// (set) Token: 0x06000AB9 RID: 2745 RVA: 0x00062952 File Offset: 0x00060B52
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed on some input (e.g. Select, Use, CrewOrders).", "", false)]
		public string CloseOnInput { get; set; }

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000ABA RID: 2746 RVA: 0x0006295B File Offset: 0x00060B5B
		// (set) Token: 0x06000ABB RID: 2747 RVA: 0x00062963 File Offset: 0x00060B63
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user selects an item that has this tag.", "", false)]
		public Identifier CloseOnSelectTag { get; set; }

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000ABC RID: 2748 RVA: 0x0006296C File Offset: 0x00060B6C
		// (set) Token: 0x06000ABD RID: 2749 RVA: 0x00062974 File Offset: 0x00060B74
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user picks up an item that has this tag.", "", false)]
		public Identifier CloseOnPickUpTag { get; set; }

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000ABE RID: 2750 RVA: 0x0006297D File Offset: 0x00060B7D
		// (set) Token: 0x06000ABF RID: 2751 RVA: 0x00062985 File Offset: 0x00060B85
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user equips an item that has this tag.", "", false)]
		public Identifier CloseOnEquipTag { get; set; }

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000AC0 RID: 2752 RVA: 0x0006298E File Offset: 0x00060B8E
		// (set) Token: 0x06000AC1 RID: 2753 RVA: 0x00062996 File Offset: 0x00060B96
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user exits a room with this name.", "", false)]
		public Identifier CloseOnExitRoomName { get; set; }

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000AC2 RID: 2754 RVA: 0x0006299F File Offset: 0x00060B9F
		// (set) Token: 0x06000AC3 RID: 2755 RVA: 0x000629A7 File Offset: 0x00060BA7
		[Serialize("", IsPropertySaveable.Yes, "The message box is automatically closed when the user is in a room with this name.", "", false)]
		public Identifier CloseOnInRoomName { get; set; }

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000AC4 RID: 2756 RVA: 0x000629B0 File Offset: 0x00060BB0
		// (set) Token: 0x06000AC5 RID: 2757 RVA: 0x000629B8 File Offset: 0x00060BB8
		[Serialize("", IsPropertySaveable.Yes, "Optional tag that will be used to get the text for the objective that is displayed on the screen.", "", false)]
		public Identifier ObjectiveTag { get; set; }

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000AC6 RID: 2758 RVA: 0x000629C1 File Offset: 0x00060BC1
		// (set) Token: 0x06000AC7 RID: 2759 RVA: 0x000629C9 File Offset: 0x00060BC9
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ObjectiveCanBeCompleted { get; set; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000AC8 RID: 2760 RVA: 0x000629D2 File Offset: 0x00060BD2
		// (set) Token: 0x06000AC9 RID: 2761 RVA: 0x000629DA File Offset: 0x00060BDA
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier ParentObjectiveId { get; set; }

		// Token: 0x06000ACA RID: 2762 RVA: 0x000629E4 File Offset: 0x00060BE4
		public MessageBoxAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Identifier.IsEmpty)
			{
				this.Identifier = element.GetAttributeIdentifier("id", Identifier.Empty);
			}
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x00062A1F File Offset: 0x00060C1F
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.UpdateProjSpecific();
			this.isFinished = true;
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x00062A38 File Offset: 0x00060C38
		private void UpdateProjSpecific()
		{
			if (this.Type == MessageBoxAction.ActionType.Create || this.Type == MessageBoxAction.ActionType.ConnectObjective)
			{
				this.CreateMessageBox();
				if (!this.ObjectiveTag.IsEmpty)
				{
					Identifier identifier = this.Identifier;
					Identifier text = this.Text;
					Identifier id = identifier.IfEmpty(text);
					ObjectiveManager.Segment segment = ObjectiveManager.Segment.CreateMessageBoxSegment(id, this.ObjectiveTag, new Action(this.CreateMessageBox));
					segment.CanBeCompleted = this.ObjectiveCanBeCompleted;
					segment.ParentId = this.ParentObjectiveId;
					ObjectiveManager.TriggerSegment(segment, this.Type == MessageBoxAction.ActionType.ConnectObjective);
					return;
				}
			}
			else
			{
				if (this.Type == MessageBoxAction.ActionType.Close)
				{
					GUIMessageBox.Close(this.Tag);
					return;
				}
				if (this.Type == MessageBoxAction.ActionType.Clear)
				{
					GUIMessageBox.CloseAll();
				}
			}
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x00062AE8 File Offset: 0x00060CE8
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x00062AF0 File Offset: 0x00060CF0
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00062AF9 File Offset: 0x00060CF9
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " MessageBoxAction";
		}

		// Token: 0x04000580 RID: 1408
		private bool isFinished;

		// Token: 0x020007A0 RID: 1952
		public enum ActionType
		{
			// Token: 0x04003B33 RID: 15155
			Create,
			// Token: 0x04003B34 RID: 15156
			ConnectObjective,
			// Token: 0x04003B35 RID: 15157
			Close,
			// Token: 0x04003B36 RID: 15158
			Clear
		}
	}
}
