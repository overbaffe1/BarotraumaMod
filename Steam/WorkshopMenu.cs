using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma.Steam
{
	// Token: 0x0200042C RID: 1068
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class WorkshopMenu
	{
		// Token: 0x0600479A RID: 18330 RVA: 0x00272F98 File Offset: 0x00271198
		protected static void CreateBBCodeElement(Item workshopItem, GUIListBox container)
		{
			WorkshopMenu.<>c__DisplayClass2_0 CS$<>8__locals1 = new WorkshopMenu.<>c__DisplayClass2_0();
			CS$<>8__locals1.container = container;
			CS$<>8__locals1.cachedContainerSize = Point.Zero;
			CS$<>8__locals1.bbWords = new List<WorkshopMenu.BBWord>();
			CS$<>8__locals1.tagStack = new Stack<WorkshopMenu.BBWord.TagType>();
			CS$<>8__locals1.bbCode = "";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
			defaultInterpolatedStringHandler.AppendLiteral("GetWorkshopItemLongDescriptionFor");
			defaultInterpolatedStringHandler.AppendFormatted<ulong>(workshopItem.Id.Value);
			TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), SteamManager.Workshop.GetItemAsap(workshopItem.Id.Value, true), delegate(Task t)
			{
				Option<Item> workshopItemWithDescription;
				if (!t.TryGetResult(out workshopItemWithDescription))
				{
					return;
				}
				Item item;
				CS$<>8__locals1.bbCode = (workshopItemWithDescription.TryUnwrap(out item) ? (item.Description ?? "") : "");
				base.<CreateBBCodeElement>g__forceReset|0();
			});
			new GUICustomComponent(new RectTransform(Vector2.One, CS$<>8__locals1.container.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(CS$<>8__locals1.<CreateBBCodeElement>g__draw|2), null);
		}

		// Token: 0x0600479B RID: 18331 RVA: 0x0027307C File Offset: 0x0027127C
		protected static RectTransform NewItemRectT(GUILayoutGroup parent, float heightScale = 1f)
		{
			return new RectTransform(new ValueTuple<float, float>(1f, 0.06f * heightScale), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
		}

		// Token: 0x0600479C RID: 18332 RVA: 0x002730C8 File Offset: 0x002712C8
		protected static void Spacer(GUILayoutGroup parent, float height = 0.03f)
		{
			new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, height), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null);
		}

		// Token: 0x0600479D RID: 18333 RVA: 0x00273120 File Offset: 0x00271320
		protected static GUITextBlock Label(GUILayoutGroup parent, LocalizedString str, GUIFont font, float heightScale = 1f)
		{
			return new GUITextBlock(WorkshopMenu.NewItemRectT(parent, heightScale), str, null, font, Alignment.Left, false, "", null);
		}

		// Token: 0x0600479E RID: 18334 RVA: 0x0027315C File Offset: 0x0027135C
		protected static GUITextBox ScrollableTextBox(GUILayoutGroup parent, float heightScale, string text)
		{
			GUIListBox containingListBox = new GUIListBox(WorkshopMenu.NewItemRectT(parent, heightScale), false, null, "", true, false);
			GUITextBox textBox2 = new GUITextBox(new RectTransform(Vector2.One, containingListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.TopLeft, true, "GUITextBoxNoBorder", null, false, true);
			textBox2.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				string wrappedText = textBox.TextBlock.WrappedText.Value;
				int measuredHeight = (int)textBox.Font.MeasureString(wrappedText, false).Y;
				textBox.RectTransform.NonScaledSize = new ValueTuple<int, int>(containingListBox.Content.Rect.Width, Math.Max(measuredHeight, containingListBox.Content.Rect.Height));
				containingListBox.UpdateScrollBarSize();
				return true;
			};
			GUITextBox guitextBox = textBox2;
			guitextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
			{
				string str = textBox.Text;
				int cursorPos = textBox.CaretIndex;
				textBox.Text = str.Substring(0, cursorPos) + "\n" + str.Substring(cursorPos);
				textBox.CaretIndex = cursorPos + 1;
				return true;
			}));
			textBox2.Text = text2;
			return textBox2;
		}

		// Token: 0x0600479F RID: 18335 RVA: 0x00273242 File Offset: 0x00271442
		protected static GUIDropDown DropdownEnum<[Nullable(0)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, T currentValue, Action<T> setter) where T : Enum
		{
			return WorkshopMenu.Dropdown<T>(parent, textFunc, (T[])Enum.GetValues(typeof(T)), currentValue, setter, 1f);
		}

		// Token: 0x060047A0 RID: 18336 RVA: 0x00273268 File Offset: 0x00271468
		protected static GUIDropDown Dropdown<[Nullable(2)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, IReadOnlyList<T> values, T currentValue, Action<T> setter, float heightScale = 1f)
		{
			GUIDropDown dropdown = new GUIDropDown(WorkshopMenu.NewItemRectT(parent, heightScale), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			WorkshopMenu.SwapDropdownValues<T>(dropdown, textFunc, values, currentValue, setter);
			return dropdown;
		}

		// Token: 0x060047A1 RID: 18337 RVA: 0x002732A0 File Offset: 0x002714A0
		protected static void SwapDropdownValues<[Nullable(2)] T>(GUIDropDown dropdown, Func<T, LocalizedString> textFunc, IReadOnlyList<T> values, T currentValue, Action<T> setter)
		{
			if (dropdown.ListBox.Content.Children.Any((GUIComponent c) => !(c.UserData is T)))
			{
				throw new Exception("SwapValues must preserve the type of the dropdown's userdata");
			}
			dropdown.OnSelected = null;
			dropdown.ClearChildren();
			values.ForEach(delegate(T v)
			{
				dropdown.AddItem(textFunc(v), v, null, null, null);
			});
			dropdown.Select(values.IndexOf(currentValue));
			dropdown.OnSelected = delegate(GUIComponent dd, object obj)
			{
				setter((T)((object)obj));
				return true;
			};
		}

		// Token: 0x060047A2 RID: 18338 RVA: 0x00273361 File Offset: 0x00271561
		protected static int Round(float v)
		{
			return (int)MathF.Round(v);
		}

		// Token: 0x060047A3 RID: 18339 RVA: 0x0027336C File Offset: 0x0027156C
		protected static string Percentage(float v)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<int>(WorkshopMenu.Round(v * 100f));
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060047A4 RID: 18340 RVA: 0x0027339C File Offset: 0x0027159C
		protected GUIComponent CreateActionCarrier(GUIComponent parent, Identifier id, Action action)
		{
			return new GUIFrame(new RectTransform(Vector2.Zero, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				UserData = new WorkshopMenu.ActionCarrier(id, action)
			};
		}

		// Token: 0x060047A5 RID: 18341 RVA: 0x002733F8 File Offset: 0x002715F8
		protected GUITextBox CreateSearchBox(RectTransform searchRectT)
		{
			WorkshopMenu.<>c__DisplayClass14_0 CS$<>8__locals1 = new WorkshopMenu.<>c__DisplayClass14_0();
			CS$<>8__locals1.<>4__this = this;
			GUIFrame searchHolder = new GUIFrame(searchRectT, null, null);
			WorkshopMenu.<>c__DisplayClass14_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(Vector2.One, searchHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text = "";
			Microsoft.Xna.Framework.Color? textColor = null;
			GUIFont font = null;
			Alignment textAlignment = Alignment.Left;
			bool wrap = false;
			string style = "";
			Microsoft.Xna.Framework.Color? color = null;
			CS$<>8__locals2.searchBox = new GUITextBox(rectT, text, textColor, font, textAlignment, wrap, style, color, true, true);
			WorkshopMenu.<>c__DisplayClass14_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectTransform = new RectTransform(Vector2.One, searchHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.Anchor = Anchor.TopLeft;
			color = new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.DarkGray * 0.6f);
			CS$<>8__locals3.searchTitle = new GUITextBlock(rectTransform, TextManager.Get("Search") + TextManager.Get("ellipsis"), color, null, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, searchHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				CS$<>8__locals1.searchTitle.RectTransform.NonScaledSize = CS$<>8__locals1.searchBox.Frame.RectTransform.NonScaledSize;
			});
			CS$<>8__locals1.searchBox.OnSelected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = false;
			};
			CS$<>8__locals1.searchBox.OnDeselected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = CS$<>8__locals1.searchBox.Text.IsNullOrWhiteSpace();
			};
			CS$<>8__locals1.searchBox.OnTextChanged += delegate(GUITextBox sender, string str)
			{
				CS$<>8__locals1.<>4__this.UpdateModListItemVisibility();
				return true;
			};
			return CS$<>8__locals1.searchBox;
		}

		// Token: 0x060047A6 RID: 18342 RVA: 0x0027359C File Offset: 0x0027179C
		protected static void CreateModErrorInfo(ContentPackage mod, GUIComponent uiElement, GUITextBlock nameText)
		{
			uiElement.ToolTip = "";
			if (mod.FatalLoadErrors.Any<ContentPackage.LoadError>())
			{
				nameText.TextColor = GUIStyle.Red;
				uiElement.ToolTip = TextManager.GetWithVariable("ContentPackageHasFatalErrors", "[packagename]", mod.Name, FormatCapitals.No) + '\n' + string.Join<string>('\n', from e in mod.FatalLoadErrors.Take(5)
				select e.Message);
				if (mod.FatalLoadErrors.Length > 5)
				{
					uiElement.ToolTip += '\n' + TextManager.GetWithVariable("workshopitemdownloadprompttruncated", "[number]", (mod.FatalLoadErrors.Count<ContentPackage.LoadError>() - 5).ToString(), FormatCapitals.No);
				}
			}
			if (mod.EnableError.IsSome())
			{
				nameText.TextColor = GUIStyle.Red;
				if (!uiElement.ToolTip.IsNullOrWhiteSpace())
				{
					RichString toolTip = uiElement.ToolTip;
					uiElement.ToolTip = ((toolTip != null) ? toolTip.ToString() : null) + "\n";
				}
				uiElement.ToolTip += TextManager.GetWithVariable("ContentPackageEnableError", "[packagename]", mod.Name, FormatCapitals.No);
			}
			if (mod.MissingDependencies.Any<PublishedFileId>())
			{
				nameText.TextColor = GUIStyle.Orange;
				if (!uiElement.ToolTip.IsNullOrWhiteSpace())
				{
					RichString toolTip2 = uiElement.ToolTip;
					uiElement.ToolTip = ((toolTip2 != null) ? toolTip2.ToString() : null) + "\n";
				}
				LocalizedString left = uiElement.ToolTip;
				string tag = "workshop.dependencynotfound";
				ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
				array[0] = new ValueTuple<string, string>("[name]", mod.Name);
				array[1] = new ValueTuple<string, string>("[required]", string.Join(", ", from c in mod.MissingDependencies
				select c.ToString()));
				uiElement.ToolTip = left + (TextManager.GetWithVariables(tag, array) + '\n' + TextManager.Get("workshop.dependencynotfound.moreinfo"));
			}
		}

		// Token: 0x060047A7 RID: 18343 RVA: 0x00273838 File Offset: 0x00271A38
		public WorkshopMenu(GUIFrame parent)
		{
		}

		// Token: 0x060047A8 RID: 18344
		protected abstract void UpdateModListItemVisibility();

		// Token: 0x060047A9 RID: 18345 RVA: 0x00273840 File Offset: 0x00271A40
		protected bool ModNameMatches(ContentPackage p, string query)
		{
			return p.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04002533 RID: 9523
		protected static readonly Regex bbTagRegex = new Regex("\\[(.+?)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Token: 0x020010FF RID: 4351
		[Nullable(0)]
		protected readonly struct BBWord
		{
			// Token: 0x06008E88 RID: 36488 RVA: 0x003B50AC File Offset: 0x003B32AC
			public BBWord(string text, WorkshopMenu.BBWord.TagType tagTypes)
			{
				this.Text = text;
				this.TagTypes = tagTypes;
				this.Font = (tagTypes.HasFlag(WorkshopMenu.BBWord.TagType.Header) ? GUIStyle.LargeFont : (tagTypes.HasFlag(WorkshopMenu.BBWord.TagType.Bold) ? GUIStyle.SubHeadingFont : GUIStyle.Font));
				this.Size = this.Font.MeasureString(this.Text, false);
			}

			// Token: 0x04005A40 RID: 23104
			public readonly string Text;

			// Token: 0x04005A41 RID: 23105
			public readonly Vector2 Size;

			// Token: 0x04005A42 RID: 23106
			public readonly WorkshopMenu.BBWord.TagType TagTypes;

			// Token: 0x04005A43 RID: 23107
			public readonly GUIFont Font;

			// Token: 0x020015AD RID: 5549
			[NullableContext(0)]
			[Flags]
			public enum TagType
			{
				// Token: 0x04006977 RID: 26999
				None = 0,
				// Token: 0x04006978 RID: 27000
				Bold = 1,
				// Token: 0x04006979 RID: 27001
				Italic = 2,
				// Token: 0x0400697A RID: 27002
				Header = 4,
				// Token: 0x0400697B RID: 27003
				List = 8,
				// Token: 0x0400697C RID: 27004
				NewLine = 16
			}
		}

		// Token: 0x02001100 RID: 4352
		[Nullable(0)]
		protected struct ActionCarrier
		{
			// Token: 0x06008E89 RID: 36489 RVA: 0x003B5123 File Offset: 0x003B3323
			public ActionCarrier(Identifier id, Action action)
			{
				this.Id = id;
				this.Action = action;
			}

			// Token: 0x04005A44 RID: 23108
			public readonly Identifier Id;

			// Token: 0x04005A45 RID: 23109
			public readonly Action Action;
		}
	}
}
