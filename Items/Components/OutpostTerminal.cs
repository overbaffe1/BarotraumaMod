using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C4 RID: 1476
	internal class OutpostTerminal : ItemComponent
	{
		// Token: 0x06005CB0 RID: 23728 RVA: 0x002FD378 File Offset: 0x002FB578
		public override bool Select(Character character)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) == null || !Level.IsLoadedFriendlyOutpost)
			{
				return false;
			}
			if (this.selectionUI == null)
			{
				this.selectionUI = new SubmarineSelection(true, null, GUI.Canvas);
			}
			base.GuiFrame = this.selectionUI.GuiFrame;
			this.selectionUI.RefreshSubmarineDisplay(true, true);
			this.IsActive = true;
			return base.Select(character);
		}

		// Token: 0x06005CB1 RID: 23729 RVA: 0x002FD3E7 File Offset: 0x002FB5E7
		public override void Update(float deltaTime, Camera cam)
		{
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedItem : null) != this.item)
			{
				this.IsActive = false;
				return;
			}
			base.Update(deltaTime, cam);
			SubmarineSelection submarineSelection = this.selectionUI;
			if (submarineSelection == null)
			{
				return;
			}
			submarineSelection.Update();
		}

		// Token: 0x06005CB2 RID: 23730 RVA: 0x002FD422 File Offset: 0x002FB622
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.selectionUI != null)
			{
				this.selectionUI.GuiFrame.RectTransform.Parent = null;
				this.selectionUI = null;
			}
		}

		// Token: 0x06005CB3 RID: 23731 RVA: 0x002FD44F File Offset: 0x002FB64F
		public OutpostTerminal(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x04002F6E RID: 12142
		private SubmarineSelection selectionUI;
	}
}
