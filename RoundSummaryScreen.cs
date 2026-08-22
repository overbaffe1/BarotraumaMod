using System;
using System.Runtime.ExceptionServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000119 RID: 281
	internal class RoundSummaryScreen : Screen
	{
		// Token: 0x06002684 RID: 9860 RVA: 0x00199084 File Offset: 0x00197284
		public static RoundSummaryScreen Select(Sprite backgroundSprite, RoundSummary roundSummary)
		{
			RoundSummaryScreen summaryScreen = new RoundSummaryScreen
			{
				roundSummary = roundSummary,
				backgroundSprite = backgroundSprite,
				prevGuiElementParent = roundSummary.Frame.RectTransform.Parent,
				loadText = TextManager.Get("campaignstartingpleasewait")
			};
			roundSummary.Frame.RectTransform.Parent = summaryScreen.Frame.RectTransform;
			summaryScreen.Select();
			summaryScreen.AddToGUIUpdateList();
			return summaryScreen;
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x001990F3 File Offset: 0x001972F3
		public override void Deselect()
		{
			this.roundSummary.Frame.RectTransform.Parent = this.prevGuiElementParent;
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x00199110 File Offset: 0x00197310
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			if (this.backgroundSprite != null)
			{
				float scale = Math.Max((float)GameMain.GraphicsWidth / this.backgroundSprite.size.X, (float)GameMain.GraphicsHeight / this.backgroundSprite.size.Y);
				this.backgroundSprite.Draw(spriteBatch, new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight) / 2f, Color.White, this.backgroundSprite.size / 2f, 0f, scale, SpriteEffects.None, null);
			}
			GUI.Draw(this.Cam, spriteBatch);
			LocalizedString loadingText = this.loadText + new string('.', (int)Timing.TotalTime % 3 + 1);
			Vector2 textSize = GUIStyle.LargeFont.MeasureString(this.loadText, false);
			Vector2 pos = new Vector2((float)(GameMain.GraphicsWidth / 2), (float)GameMain.GraphicsHeight * 0.95f) - textSize / 2f;
			LocalizedString text = loadingText;
			Color white = Color.White;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUI.DrawString(spriteBatch, pos, text, white, null, 0, largeFont, ForceUpperCase.Inherit);
			spriteBatch.End();
		}

		// Token: 0x06002687 RID: 9863 RVA: 0x0019925C File Offset: 0x0019745C
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			if (this.LoadException != null)
			{
				Exception temp = this.LoadException;
				this.LoadException = null;
				ExceptionDispatchInfo.Capture(temp).Throw();
			}
		}

		// Token: 0x04001369 RID: 4969
		private Sprite backgroundSprite;

		// Token: 0x0400136A RID: 4970
		private RoundSummary roundSummary;

		// Token: 0x0400136B RID: 4971
		private LocalizedString loadText;

		// Token: 0x0400136C RID: 4972
		private RectTransform prevGuiElementParent;

		// Token: 0x0400136D RID: 4973
		public Exception LoadException;
	}
}
