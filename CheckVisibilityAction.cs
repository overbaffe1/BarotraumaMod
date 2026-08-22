using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000288 RID: 648
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckVisibilityAction : BinaryOptionAction
	{
		// Token: 0x17000F13 RID: 3859
		// (get) Token: 0x0600395F RID: 14687 RVA: 0x0021BC35 File Offset: 0x00219E35
		// (set) Token: 0x06003960 RID: 14688 RVA: 0x0021BC3D File Offset: 0x00219E3D
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to do the visibility check from.", "", false)]
		public Identifier EntityTag { get; set; }

		// Token: 0x17000F14 RID: 3860
		// (get) Token: 0x06003961 RID: 14689 RVA: 0x0021BC46 File Offset: 0x00219E46
		// (set) Token: 0x06003962 RID: 14690 RVA: 0x0021BC4E File Offset: 0x00219E4E
		[Serialize("", IsPropertySaveable.Yes, "Entities that also have this tag are excluded.", "", false)]
		public Identifier ExcludedEntityTag { get; set; }

		// Token: 0x17000F15 RID: 3861
		// (get) Token: 0x06003963 RID: 14691 RVA: 0x0021BC57 File Offset: 0x00219E57
		// (set) Token: 0x06003964 RID: 14692 RVA: 0x0021BC5F File Offset: 0x00219E5F
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to do the visibility check to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F16 RID: 3862
		// (get) Token: 0x06003965 RID: 14693 RVA: 0x0021BC68 File Offset: 0x00219E68
		// (set) Token: 0x06003966 RID: 14694 RVA: 0x0021BC70 File Offset: 0x00219E70
		[Serialize(false, IsPropertySaveable.Yes, "Does the entity need to be facing the target? Only valid if the entity is a character.", "", false)]
		public bool CheckFacing { get; set; }

		// Token: 0x17000F17 RID: 3863
		// (get) Token: 0x06003967 RID: 14695 RVA: 0x0021BC79 File Offset: 0x00219E79
		// (set) Token: 0x06003968 RID: 14696 RVA: 0x0021BC81 File Offset: 0x00219E81
		[Serialize(1000f, IsPropertySaveable.Yes, "Maximum distance between the targets.", "", false)]
		public float MaxDistance { get; set; }

		// Token: 0x17000F18 RID: 3864
		// (get) Token: 0x06003969 RID: 14697 RVA: 0x0021BC8A File Offset: 0x00219E8A
		// (set) Token: 0x0600396A RID: 14698 RVA: 0x0021BC92 File Offset: 0x00219E92
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the entity who saw the target when the check succeeds.", "", false)]
		public Identifier ApplyTagToEntity { get; set; }

		// Token: 0x17000F19 RID: 3865
		// (get) Token: 0x0600396B RID: 14699 RVA: 0x0021BC9B File Offset: 0x00219E9B
		// (set) Token: 0x0600396C RID: 14700 RVA: 0x0021BCA3 File Offset: 0x00219EA3
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the entity that was seen when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x17000F1A RID: 3866
		// (get) Token: 0x0600396D RID: 14701 RVA: 0x0021BCAC File Offset: 0x00219EAC
		// (set) Token: 0x0600396E RID: 14702 RVA: 0x0021BCB4 File Offset: 0x00219EB4
		[Serialize(true, IsPropertySaveable.Yes, "If both the seeing entity and the target are the same, does it count as success?", "", false)]
		public bool AllowSameEntity { get; set; }

		// Token: 0x0600396F RID: 14703 RVA: 0x0021BCBD File Offset: 0x00219EBD
		public CheckVisibilityAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003970 RID: 14704 RVA: 0x0021BCC8 File Offset: 0x00219EC8
		protected override bool? DetermineSuccess()
		{
			foreach (Entity entity in this.ParentEvent.GetTargets(this.EntityTag))
			{
				if (this.ExcludedEntityTag.IsEmpty || !this.ParentEvent.GetTargets(this.ExcludedEntityTag).Contains(entity))
				{
					foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
					{
						if ((this.AllowSameEntity || entity != target) && Vector2.DistanceSquared(target.WorldPosition, entity.WorldPosition) <= this.MaxDistance * this.MaxDistance && ISpatialEntity.IsTargetVisible(target, entity, true, this.CheckFacing))
						{
							if (!this.ApplyTagToEntity.IsEmpty)
							{
								this.ParentEvent.AddTarget(this.ApplyTagToEntity, entity);
							}
							if (!this.ApplyTagToTarget.IsEmpty)
							{
								this.ParentEvent.AddTarget(this.ApplyTagToTarget, target);
							}
							return new bool?(true);
						}
					}
				}
			}
			return new bool?(false);
		}

		// Token: 0x06003971 RID: 14705 RVA: 0x0021BE44 File Offset: 0x0021A044
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckVisibilityAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTags: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.EntityTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
