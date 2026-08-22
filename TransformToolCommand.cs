using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200014A RID: 330
	internal class TransformToolCommand : Command
	{
		// Token: 0x060029E3 RID: 10723 RVA: 0x001D0264 File Offset: 0x001CE464
		public TransformToolCommand(Dictionary<MapEntity, SubEditorScreen.TransformData> data, Vector2 pivot)
		{
			this.originalData = data2;
			this.Pivot = pivot;
			this.wirePivot = this.Pivot - Submarine.MainSub.HiddenSubPosition;
			this.MinScale = 0.01f / Math.Max(data2.Values.Min((SubEditorScreen.TransformData data) => data.Scale), 0.01f);
			this.MaxScale = 10f / Math.Min(data2.Values.Max((SubEditorScreen.TransformData data) => data.Scale), 10f);
		}

		// Token: 0x060029E4 RID: 10724 RVA: 0x001D0320 File Offset: 0x001CE520
		public override void Execute()
		{
			this.UpdateTransforms(this.RotationRad.GetValueOrDefault(), this.ScaleMult.GetValueOrDefault(1f));
		}

		// Token: 0x060029E5 RID: 10725 RVA: 0x001D0343 File Offset: 0x001CE543
		public override void UnExecute()
		{
			this.UpdateTransforms(0f, 1f);
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x001D0355 File Offset: 0x001CE555
		public override void Cleanup()
		{
			this.originalData.Clear();
		}

		// Token: 0x060029E7 RID: 10727 RVA: 0x001D0364 File Offset: 0x001CE564
		private void UpdateTransforms(float rotationRad, float scaleMult)
		{
			Action<KeyValuePair<Turret, Vector2>> <>9__5;
			Action<KeyValuePair<ItemLabel, float>> <>9__0;
			Action<KeyValuePair<LightComponent, float>> <>9__1;
			Action<KeyValuePair<Wire, ValueTuple<List<Vector2>, float>>> <>9__2;
			Action<KeyValuePair<Wire, ValueTuple<List<Vector2>, float>>> <>9__3;
			foreach (KeyValuePair<MapEntity, SubEditorScreen.TransformData> keyValuePair in this.originalData)
			{
				MapEntity mapEntity;
				SubEditorScreen.TransformData transformData;
				keyValuePair.Deconstruct(out mapEntity, out transformData);
				MapEntity receiver = mapEntity;
				SubEditorScreen.TransformData data = transformData;
				bool flag = this.RotationRad != null;
				bool flag2 = flag;
				if (flag2)
				{
					Item item2 = receiver as Item;
					if (item2 != null)
					{
						ItemPrefab prefab = item2.Prefab;
						if (prefab == null)
						{
							goto IL_AE;
						}
						if (!prefab.AllowRotatingInEditor)
						{
							goto IL_AE;
						}
						goto IL_A9;
					}
					else
					{
						Structure structure3 = receiver as Structure;
						if (structure3 == null)
						{
							goto IL_AE;
						}
						StructurePrefab prefab2 = structure3.Prefab;
						if (prefab2 == null)
						{
							goto IL_AE;
						}
						bool allowRotatingInEditor = prefab2.AllowRotatingInEditor;
						if (allowRotatingInEditor)
						{
							goto IL_A9;
						}
						goto IL_AE;
					}
					IL_B1:
					bool flag3;
					flag2 = flag3;
					goto IL_B5;
					IL_AE:
					flag3 = false;
					goto IL_B1;
					IL_A9:
					flag3 = true;
					goto IL_B1;
				}
				IL_B5:
				if (flag2)
				{
					int rotationDir = (receiver is Structure && (receiver.FlippedX ^ receiver.FlippedY)) ? -1 : 1;
					float newRotation = MathHelper.ToDegrees(data.RotationRad + rotationRad * (float)rotationDir);
					Item item = receiver as Item;
					if (item == null)
					{
						Structure structure = receiver as Structure;
						if (structure != null)
						{
							structure.Rotation = newRotation;
						}
					}
					else
					{
						item.Rotation = newRotation;
					}
					Dictionary<Turret, Vector2> turretLimits = data.TurretLimits;
					if (turretLimits != null)
					{
						Action<KeyValuePair<Turret, Vector2>> action;
						if ((action = <>9__5) == null)
						{
							action = (<>9__5 = delegate(KeyValuePair<Turret, Vector2> pair)
							{
								pair.Key.RotationLimits = pair.Value + new Vector2(MathHelper.ToDegrees(rotationRad));
							});
						}
						turretLimits.ForEach(action);
					}
				}
				if (this.ScaleMult != null)
				{
					receiver.Scale = data.Scale * scaleMult;
					if (receiver.ResizeVertical || receiver.ResizeHorizontal)
					{
						if (receiver.ResizeVertical)
						{
							receiver.RectHeight = (int)((float)data.Rect.Height * scaleMult);
						}
						if (receiver.ResizeHorizontal)
						{
							receiver.RectWidth = (int)((float)data.Rect.Width * scaleMult);
						}
						Structure structure2 = receiver as Structure;
						if (structure2 != null && data.TexOffset != null)
						{
							structure2.TextureOffset = data.TexOffset.Value * scaleMult;
						}
					}
					Dictionary<ItemLabel, float> textScales = data.TextScales;
					if (textScales != null)
					{
						Action<KeyValuePair<ItemLabel, float>> action2;
						if ((action2 = <>9__0) == null)
						{
							action2 = (<>9__0 = delegate(KeyValuePair<ItemLabel, float> pair)
							{
								pair.Key.TextScale = pair.Value * scaleMult;
							});
						}
						textScales.ForEach(action2);
					}
					Dictionary<LightComponent, float> lightRanges = data.LightRanges;
					if (lightRanges != null)
					{
						Action<KeyValuePair<LightComponent, float>> action3;
						if ((action3 = <>9__1) == null)
						{
							action3 = (<>9__1 = delegate(KeyValuePair<LightComponent, float> pair)
							{
								pair.Key.Range = pair.Value * scaleMult;
							});
						}
						lightRanges.ForEach(action3);
					}
					Dictionary<Wire, ValueTuple<List<Vector2>, float>> wires = data.Wires;
					if (wires != null)
					{
						Action<KeyValuePair<Wire, ValueTuple<List<Vector2>, float>>> action4;
						if ((action4 = <>9__2) == null)
						{
							action4 = (<>9__2 = delegate([TupleElementNames(new string[]
							{
								"Nodes",
								"Width"
							})] KeyValuePair<Wire, ValueTuple<List<Vector2>, float>> pair)
							{
								pair.Key.Width = pair.Value.Item2 * scaleMult;
							});
						}
						wires.ForEach(action4);
					}
				}
				Vector2 newEntityPos = MathUtils.RotatePoint((data.Pos - this.Pivot) * scaleMult, -rotationRad) + this.Pivot;
				receiver.Move(newEntityPos - receiver.DrawPosition, true);
				Dictionary<Wire, ValueTuple<List<Vector2>, float>> wires2 = data.Wires;
				if (wires2 != null)
				{
					Action<KeyValuePair<Wire, ValueTuple<List<Vector2>, float>>> action5;
					if ((action5 = <>9__3) == null)
					{
						action5 = (<>9__3 = delegate([TupleElementNames(new string[]
						{
							"Nodes",
							"Width"
						})] KeyValuePair<Wire, ValueTuple<List<Vector2>, float>> pair)
						{
							pair.Key.SetNodes(pair.Value.Item1.Select(new Func<Vector2, Vector2>(base.<UpdateTransforms>g__TransformWireNode|4)));
						});
					}
					wires2.ForEach(action5);
				}
			}
		}

		// Token: 0x060029E8 RID: 10728 RVA: 0x001D06D0 File Offset: 0x001CE8D0
		public override LocalizedString GetDescription()
		{
			if (this.originalData.Count <= 1)
			{
				return TextManager.GetWithVariable("Undo.ChangedTransform", "[item]", this.originalData.First<KeyValuePair<MapEntity, SubEditorScreen.TransformData>>().Key.Name, FormatCapitals.No);
			}
			return TextManager.GetWithVariable("Undo.ChangedTransformMultiple", "[amount]", this.originalData.Count.ToString(), FormatCapitals.No);
		}

		// Token: 0x040015DC RID: 5596
		private readonly Dictionary<MapEntity, SubEditorScreen.TransformData> originalData;

		// Token: 0x040015DD RID: 5597
		public float? ScaleMult;

		// Token: 0x040015DE RID: 5598
		public float? RotationRad;

		// Token: 0x040015DF RID: 5599
		public readonly Vector2 Pivot;

		// Token: 0x040015E0 RID: 5600
		private readonly Vector2 wirePivot;

		// Token: 0x040015E1 RID: 5601
		public float MinScale;

		// Token: 0x040015E2 RID: 5602
		public float MaxScale;
	}
}
