using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000145 RID: 325
	internal class TransformCommand : Command
	{
		// Token: 0x060029C0 RID: 10688 RVA: 0x001CEE78 File Offset: 0x001CD078
		public TransformCommand(List<MapEntity> receivers, List<Rectangle> newData, List<Rectangle> oldData, bool resized)
		{
			this.Receivers = receivers;
			this.NewData = newData;
			this.OldData = oldData;
			this.Resized = resized;
		}

		// Token: 0x060029C1 RID: 10689 RVA: 0x001CEE9D File Offset: 0x001CD09D
		public override void Execute()
		{
			this.SetRects(this.NewData);
		}

		// Token: 0x060029C2 RID: 10690 RVA: 0x001CEEAB File Offset: 0x001CD0AB
		public override void UnExecute()
		{
			this.SetRects(this.OldData);
		}

		// Token: 0x060029C3 RID: 10691 RVA: 0x001CEEB9 File Offset: 0x001CD0B9
		public override void Cleanup()
		{
			this.NewData.Clear();
			this.OldData.Clear();
			this.Receivers.Clear();
		}

		// Token: 0x060029C4 RID: 10692 RVA: 0x001CEEDC File Offset: 0x001CD0DC
		private void SetRects(IReadOnlyList<Rectangle> rects)
		{
			if (this.Receivers.Count != rects.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Receivers.Count did not match Rects.Count (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Receivers.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" vs ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(rects.Count);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			for (int i = 0; i < rects.Count; i++)
			{
				MapEntity entity = this.Receivers[i].GetReplacementOrThis();
				Rectangle Rect = rects[i];
				Vector2 diff = Rect.Location.ToVector2() - entity.Rect.Location.ToVector2();
				entity.Move(diff, true);
				entity.Rect = Rect;
			}
		}

		// Token: 0x060029C5 RID: 10693 RVA: 0x001CEFC4 File Offset: 0x001CD1C4
		public override LocalizedString GetDescription()
		{
			if (this.Resized)
			{
				string tag = "Undo.ResizedItem";
				string varName = "[item]";
				MapEntity mapEntity = this.Receivers.FirstOrDefault<MapEntity>();
				return TextManager.GetWithVariable(tag, varName, (mapEntity != null) ? mapEntity.Name : null, FormatCapitals.No);
			}
			if (this.Receivers.Count <= 1)
			{
				string tag2 = "Undo.MovedItem";
				string varName2 = "[item]";
				MapEntity mapEntity2 = this.Receivers.FirstOrDefault<MapEntity>();
				return TextManager.GetWithVariable(tag2, varName2, (mapEntity2 != null) ? mapEntity2.Name : null, FormatCapitals.No);
			}
			return TextManager.GetWithVariable("Undo.MovedItemsMultiple", "[count]", this.Receivers.Count.ToString(), FormatCapitals.No);
		}

		// Token: 0x040015C4 RID: 5572
		private readonly List<MapEntity> Receivers;

		// Token: 0x040015C5 RID: 5573
		private readonly List<Rectangle> NewData;

		// Token: 0x040015C6 RID: 5574
		private readonly List<Rectangle> OldData;

		// Token: 0x040015C7 RID: 5575
		private readonly bool Resized;
	}
}
