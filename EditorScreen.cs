using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000107 RID: 263
	internal class EditorScreen : Screen
	{
		// Token: 0x170009DA RID: 2522
		// (get) Token: 0x0600248E RID: 9358 RVA: 0x00172151 File Offset: 0x00170351
		public override bool IsEditor
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x00172154 File Offset: 0x00170354
		public sealed override void Deselect()
		{
			this.DeselectEditorSpecific();
			GameMain.LightManager.LightingEnabled = true;
			GameMain.LightManager.LosEnabled = true;
			Hull.EditFire = false;
			Hull.EditWater = false;
			HumanAIController.DisableCrewAI = false;
		}

		// Token: 0x06002490 RID: 9360 RVA: 0x00172184 File Offset: 0x00170384
		protected virtual void DeselectEditorSpecific()
		{
		}

		// Token: 0x06002491 RID: 9361 RVA: 0x00172188 File Offset: 0x00170388
		public unsafe void CreateBackgroundColorPicker()
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("CharacterEditor.EditBackgroundColor"), "", new LocalizedString[]
			{
				TextManager.Get("Reset"),
				TextManager.Get("OK")
			}, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup rgbLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup[] layoutParents = new GUILayoutGroup[3];
			for (int i = 0; i < 3; i++)
			{
				GUILayoutGroup colorContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.33f, 1f), rgbLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true
				};
				RectTransform rectTransform = new RectTransform(new Vector2(0.2f, 1f), colorContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				rectTransform.MinSize = new Point(15, 0);
				RichString text = GUI.ColorComponentLabels[i];
				GUIFont smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectTransform, text, null, smallFont, Alignment.Center, false, "", null);
				layoutParents[i] = colorContainer;
			}
			GUINumberInput rInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), layoutParents[0].RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				IntValue = (int)EditorScreen.BackgroundColor.R
			};
			GUINumberInput gInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), layoutParents[1].RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				IntValue = (int)EditorScreen.BackgroundColor.G
			};
			GUINumberInput bInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), layoutParents[2].RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				IntValue = (int)EditorScreen.BackgroundColor.B
			};
			GUINumberInput rInput3 = rInput;
			GUINumberInput gInput3 = gInput;
			GUINumberInput bInput3 = bInput;
			int? num = new int?(0);
			bInput3.MinValueInt = num;
			rInput3.MinValueInt = (gInput3.MinValueInt = num);
			GUINumberInput rInput2 = rInput;
			GUINumberInput gInput2 = gInput;
			GUINumberInput bInput2 = bInput;
			num = new int?(255);
			bInput2.MaxValueInt = num;
			rInput2.MaxValueInt = (gInput2.MaxValueInt = num);
			rInput.OnValueChanged = (gInput.OnValueChanged = (bInput.OnValueChanged = delegate(GUINumberInput <p0>)
			{
				Color color = new Color(rInput.IntValue, gInput.IntValue, bInput.IntValue);
				EditorScreen.BackgroundColor = color;
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.SubEditorBackground = color;
				GameSettings.SetCurrentConfig(config);
			}));
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				rInput.IntValue = 13;
				gInput.IntValue = 37;
				bInput.IntValue = 69;
				return true;
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton button, object o)
			{
				msgBox.Close();
				GameSettings.SaveCurrentConfig();
				return true;
			};
		}

		// Token: 0x0400123E RID: 4670
		public static Color BackgroundColor = GameSettings.CurrentConfig.SubEditorBackground;
	}
}
