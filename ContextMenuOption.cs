using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000083 RID: 131
	[NullableContext(1)]
	[Nullable(0)]
	internal struct ContextMenuOption
	{
		// Token: 0x0600127F RID: 4735 RVA: 0x000B5960 File Offset: 0x000B3B60
		public ContextMenuOption(string label, bool isEnabled, Action onSelected)
		{
			this = new ContextMenuOption(TextManager.Get(label).Fallback(label, true), isEnabled, onSelected);
		}

		// Token: 0x06001280 RID: 4736 RVA: 0x000B597C File Offset: 0x000B3B7C
		public ContextMenuOption(Identifier labelTag, bool isEnabled, Action onSelected)
		{
			this = new ContextMenuOption(TextManager.Get(labelTag), isEnabled, onSelected);
		}

		// Token: 0x06001281 RID: 4737 RVA: 0x000B598C File Offset: 0x000B3B8C
		public ContextMenuOption(LocalizedString label, bool isEnabled, Action onSelected)
		{
			this.Label = label;
			this.OnSelected = onSelected;
			this.IsEnabled = isEnabled;
			this.SubOptions = null;
			this.Tooltip = string.Empty;
		}

		// Token: 0x06001282 RID: 4738 RVA: 0x000B59BA File Offset: 0x000B3BBA
		public ContextMenuOption(string label, bool isEnabled, params ContextMenuOption[] options)
		{
			this = new ContextMenuOption(label, isEnabled, delegate()
			{
			});
			this.SubOptions = options;
		}

		// Token: 0x04000941 RID: 2369
		public LocalizedString Label;

		// Token: 0x04000942 RID: 2370
		public Action OnSelected;

		// Token: 0x04000943 RID: 2371
		[Nullable(2)]
		public ContextMenuOption[] SubOptions;

		// Token: 0x04000944 RID: 2372
		public bool IsEnabled;

		// Token: 0x04000945 RID: 2373
		public LocalizedString Tooltip;
	}
}
