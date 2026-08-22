using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Steam
{
	// Token: 0x0200042D RID: 1069
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class ImmutableWorkshopMenu : WorkshopMenu
	{
		// Token: 0x060047AB RID: 18347 RVA: 0x00273868 File Offset: 0x00271A68
		public ImmutableWorkshopMenu(GUIFrame parent) : base(parent)
		{
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			WorkshopMenu.Label(mainLayout, TextManager.Get("enabledcore"), GUIStyle.SubHeadingFont, 1f);
			GUIButton coreBox = new GUIButton(WorkshopMenu.NewItemRectT(mainLayout, 1f), ContentPackageManager.EnabledPackages.Core.Name, Alignment.CenterLeft, "GUITextBoxNoIcon", null)
			{
				CanBeFocused = false,
				CanBeSelected = false
			};
			coreBox.TextBlock.Padding = new Vector4(10f, 0f, 10f, 0f);
			WorkshopMenu.Label(mainLayout, TextManager.Get("enabledregular"), GUIStyle.SubHeadingFont, 1f);
			GUIListBox guilistBox = new GUIListBox(WorkshopMenu.NewItemRectT(mainLayout, 11f), false, null, "", true, false);
			guilistBox.OnSelected = ((GUIComponent component, object o) => false);
			guilistBox.HoverCursor = CursorState.Default;
			this.regularList = guilistBox;
			foreach (RegularPackage p in ContentPackageManager.EnabledPackages.Regular)
			{
				GUITextBlock regularBox = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.07f), this.regularList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), p.Name, null, null, Alignment.Left, false, "", null)
				{
					CanBeFocused = false,
					UserData = p
				};
				if (p.FatalLoadErrors.Any<ContentPackage.LoadError>())
				{
					WorkshopMenu.CreateModErrorInfo(p, regularBox, regularBox);
					regularBox.CanBeFocused = true;
				}
			}
			RectTransform searchRectT = WorkshopMenu.NewItemRectT(mainLayout, 1f);
			searchRectT.RelativeSize = new ValueTuple<float, float>(1f, searchRectT.RelativeSize.Y);
			this.filterBox = base.CreateSearchBox(searchRectT);
			WorkshopMenu.Label(mainLayout, TextManager.Get("CannotChangeMods"), GUIStyle.Font, 1f);
		}

		// Token: 0x060047AC RID: 18348 RVA: 0x00273AF0 File Offset: 0x00271CF0
		protected override void UpdateModListItemVisibility()
		{
			string str = this.filterBox.Text;
			this.regularList.Content.Children.ForEach(delegate(GUIComponent c)
			{
				ContentPackage p = c.UserData as ContentPackage;
				c.Visible = (p == null || this.ModNameMatches(p, str));
			});
		}

		// Token: 0x04002534 RID: 9524
		private readonly GUIListBox regularList;

		// Token: 0x04002535 RID: 9525
		private readonly GUITextBox filterBox;
	}
}
