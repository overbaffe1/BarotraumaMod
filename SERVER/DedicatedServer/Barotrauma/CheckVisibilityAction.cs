using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000195 RID: 405
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckVisibilityAction : BinaryOptionAction
	{
		// Token: 0x170008BF RID: 2239
		// (get) Token: 0x06001E95 RID: 7829 RVA: 0x000D6781 File Offset: 0x000D4981
		// (set) Token: 0x06001E96 RID: 7830 RVA: 0x000D6789 File Offset: 0x000D4989
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to do the visibility check from.", "", false)]
		public Identifier EntityTag { get; set; }

		// Token: 0x170008C0 RID: 2240
		// (get) Token: 0x06001E97 RID: 7831 RVA: 0x000D6792 File Offset: 0x000D4992
		// (set) Token: 0x06001E98 RID: 7832 RVA: 0x000D679A File Offset: 0x000D499A
		[Serialize("", IsPropertySaveable.Yes, "Entities that also have this tag are excluded.", "", false)]
		public Identifier ExcludedEntityTag { get; set; }

		// Token: 0x170008C1 RID: 2241
		// (get) Token: 0x06001E99 RID: 7833 RVA: 0x000D67A3 File Offset: 0x000D49A3
		// (set) Token: 0x06001E9A RID: 7834 RVA: 0x000D67AB File Offset: 0x000D49AB
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to do the visibility check to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008C2 RID: 2242
		// (get) Token: 0x06001E9B RID: 7835 RVA: 0x000D67B4 File Offset: 0x000D49B4
		// (set) Token: 0x06001E9C RID: 7836 RVA: 0x000D67BC File Offset: 0x000D49BC
		[Serialize(false, IsPropertySaveable.Yes, "Does the entity need to be facing the target? Only valid if the entity is a character.", "", false)]
		public bool CheckFacing { get; set; }

		// Token: 0x170008C3 RID: 2243
		// (get) Token: 0x06001E9D RID: 7837 RVA: 0x000D67C5 File Offset: 0x000D49C5
		// (set) Token: 0x06001E9E RID: 7838 RVA: 0x000D67CD File Offset: 0x000D49CD
		[Serialize(1000f, IsPropertySaveable.Yes, "Maximum distance between the targets.", "", false)]
		public float MaxDistance { get; set; }

		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x06001E9F RID: 7839 RVA: 0x000D67D6 File Offset: 0x000D49D6
		// (set) Token: 0x06001EA0 RID: 7840 RVA: 0x000D67DE File Offset: 0x000D49DE
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the entity who saw the target when the check succeeds.", "", false)]
		public Identifier ApplyTagToEntity { get; set; }

		// Token: 0x170008C5 RID: 2245
		// (get) Token: 0x06001EA1 RID: 7841 RVA: 0x000D67E7 File Offset: 0x000D49E7
		// (set) Token: 0x06001EA2 RID: 7842 RVA: 0x000D67EF File Offset: 0x000D49EF
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the entity that was seen when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x170008C6 RID: 2246
		// (get) Token: 0x06001EA3 RID: 7843 RVA: 0x000D67F8 File Offset: 0x000D49F8
		// (set) Token: 0x06001EA4 RID: 7844 RVA: 0x000D6800 File Offset: 0x000D4A00
		[Serialize(true, IsPropertySaveable.Yes, "If both the seeing entity and the target are the same, does it count as success?", "", false)]
		public bool AllowSameEntity { get; set; }

		// Token: 0x06001EA5 RID: 7845 RVA: 0x000D6809 File Offset: 0x000D4A09
		public CheckVisibilityAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001EA6 RID: 7846 RVA: 0x000D6814 File Offset: 0x000D4A14
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

		// Token: 0x06001EA7 RID: 7847 RVA: 0x000D6990 File Offset: 0x000D4B90
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
