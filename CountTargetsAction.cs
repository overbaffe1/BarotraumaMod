using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200028B RID: 651
	[NullableContext(1)]
	[Nullable(0)]
	internal class CountTargetsAction : BinaryOptionAction
	{
		// Token: 0x17000F24 RID: 3876
		// (get) Token: 0x0600398E RID: 14734 RVA: 0x0021C3F5 File Offset: 0x0021A5F5
		// (set) Token: 0x0600398F RID: 14735 RVA: 0x0021C3FD File Offset: 0x0021A5FD
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entities to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F25 RID: 3877
		// (get) Token: 0x06003990 RID: 14736 RVA: 0x0021C406 File Offset: 0x0021A606
		// (set) Token: 0x06003991 RID: 14737 RVA: 0x0021C40E File Offset: 0x0021A60E
		[Serialize("", IsPropertySaveable.Yes, "Optional second tag. Can be used if the target must have two different tags.", "", false)]
		public Identifier SecondRequiredTargetTag { get; set; }

		// Token: 0x17000F26 RID: 3878
		// (get) Token: 0x06003992 RID: 14738 RVA: 0x0021C417 File Offset: 0x0021A617
		// (set) Token: 0x06003993 RID: 14739 RVA: 0x0021C41F File Offset: 0x0021A61F
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of a hull the target must be inside.", "", false)]
		public Identifier HullTag { get; set; }

		// Token: 0x17000F27 RID: 3879
		// (get) Token: 0x06003994 RID: 14740 RVA: 0x0021C428 File Offset: 0x0021A628
		// (set) Token: 0x06003995 RID: 14741 RVA: 0x0021C430 File Offset: 0x0021A630
		[Serialize(-1, IsPropertySaveable.Yes, "Minimum number of matching entities for the check to succeed. If omitted or negative, there is no minimum amount.", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x17000F28 RID: 3880
		// (get) Token: 0x06003996 RID: 14742 RVA: 0x0021C439 File Offset: 0x0021A639
		// (set) Token: 0x06003997 RID: 14743 RVA: 0x0021C441 File Offset: 0x0021A641
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of matching entities for the check to succeed. If omitted or negative, there is no maximum amount.", "", false)]
		public int MaxAmount { get; set; }

		// Token: 0x17000F29 RID: 3881
		// (get) Token: 0x06003998 RID: 14744 RVA: 0x0021C44A File Offset: 0x0021A64A
		// (set) Token: 0x06003999 RID: 14745 RVA: 0x0021C452 File Offset: 0x0021A652
		[Serialize("", IsPropertySaveable.Yes, "Tag of some other entities to compare the number of targets to. E.g. you could compare the number of entities tagged as \"discoveredhull\" to entities tagged as \"anyhull\". The minimum/maximum amount of entities there must be relative to the other entities is configured using MinPercentageRelativeToTarget and MaxPercentageRelativeToTarget.", "", false)]
		public Identifier CompareToTarget { get; set; }

		// Token: 0x17000F2A RID: 3882
		// (get) Token: 0x0600399A RID: 14746 RVA: 0x0021C45B File Offset: 0x0021A65B
		// (set) Token: 0x0600399B RID: 14747 RVA: 0x0021C463 File Offset: 0x0021A663
		[Serialize(-1f, IsPropertySaveable.Yes, "Minimum amount of targets, as a percentage of the number of entities tagged with CompareToTarget. E.g. you could compare the number of entities tagged as \"discoveredhull\" to entities tagged as \"anyhull\" to require 50% of hulls to be discovered.", "", false)]
		public float MinPercentageRelativeToTarget { get; set; }

		// Token: 0x17000F2B RID: 3883
		// (get) Token: 0x0600399C RID: 14748 RVA: 0x0021C46C File Offset: 0x0021A66C
		// (set) Token: 0x0600399D RID: 14749 RVA: 0x0021C474 File Offset: 0x0021A674
		[Serialize(-1f, IsPropertySaveable.Yes, "Maximum amount of targets, as a percentage of the number of entities tagged with CompareToTarget. E.g. you could compare the number of entities tagged as \"floodedhull\" to entities tagged as \"anyhull\" to require less than 50% of hulls to be flooded.", "", false)]
		public float MaxPercentageRelativeToTarget { get; set; }

		// Token: 0x0600399E RID: 14750 RVA: 0x0021C480 File Offset: 0x0021A680
		public CountTargetsAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			List<PropertyConditional> conditionalList = new List<PropertyConditional>();
			foreach (ContentXElement subElement in element.GetChildElements("conditional"))
			{
				conditionalList.AddRange(PropertyConditional.FromXElement(subElement, null));
			}
			this.conditionals = conditionalList;
			if (this.CompareToTarget.IsEmpty)
			{
				int amount = element.GetAttributeInt("amount", -1);
				if (amount > -1)
				{
					this.MinAmount = (this.MaxAmount = amount);
				}
				if (this.MinAmount > this.MaxAmount && this.MaxAmount > -1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.MinAmount);
					defaultInterpolatedStringHandler.AppendLiteral(" is larger than ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.MaxAmount);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted("CountTargetsAction");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					return;
				}
			}
			else if (this.MinPercentageRelativeToTarget < 0f && this.MaxPercentageRelativeToTarget < 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(72, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\". Comparing to another target, but neither ");
				defaultInterpolatedStringHandler2.AppendFormatted("MinPercentageRelativeToTarget");
				defaultInterpolatedStringHandler2.AppendLiteral(" or ");
				defaultInterpolatedStringHandler2.AppendFormatted("MaxPercentageRelativeToTarget");
				defaultInterpolatedStringHandler2.AppendLiteral(" is set.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x0600399F RID: 14751 RVA: 0x0021C678 File Offset: 0x0021A878
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Entity> potentialTargets = this.ParentEvent.GetTargets(this.TargetTag);
			if (!this.SecondRequiredTargetTag.IsEmpty)
			{
				potentialTargets = from t in potentialTargets
				where this.ParentEvent.GetTargets(this.SecondRequiredTargetTag).Contains(t)
				select t;
			}
			if (!this.HullTag.IsEmpty)
			{
				IEnumerable<Hull> hulls = this.ParentEvent.GetTargets(this.HullTag).OfType<Hull>();
				potentialTargets = potentialTargets.Where(delegate(Entity t)
				{
					Item it = t as Item;
					if (it == null || !hulls.Contains(it.CurrentHull))
					{
						Character c = t as Character;
						return c != null && hulls.Contains(c.CurrentHull);
					}
					return true;
				});
			}
			if (this.conditionals.Any<PropertyConditional>())
			{
				potentialTargets = from t in potentialTargets
				where this.conditionals.Any((PropertyConditional c) => c.Matches(t as ISerializableEntity))
				select t;
			}
			int targetCount = potentialTargets.Count<Entity>();
			if (this.CompareToTarget.IsEmpty)
			{
				if (this.MinAmount > -1 && targetCount < this.MinAmount)
				{
					return new bool?(false);
				}
				if (this.MaxAmount > -1 && targetCount > this.MaxAmount)
				{
					return new bool?(false);
				}
			}
			else
			{
				int compareToTargetCount = this.ParentEvent.GetTargets(this.CompareToTarget).Count<Entity>();
				if (compareToTargetCount == 0)
				{
					return new bool?(false);
				}
				float percentage = MathUtils.Percentage((float)targetCount, (float)compareToTargetCount);
				if (this.MinPercentageRelativeToTarget > -1f && percentage < this.MinPercentageRelativeToTarget)
				{
					return new bool?(false);
				}
				if (this.MaxPercentageRelativeToTarget > -1f && percentage > this.MaxPercentageRelativeToTarget)
				{
					return new bool?(false);
				}
			}
			return new bool?(true);
		}

		// Token: 0x060039A0 RID: 14752 RVA: 0x0021C7DC File Offset: 0x0021A9DC
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CountTargetsAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DBB RID: 7611
		private readonly IReadOnlyList<PropertyConditional> conditionals;
	}
}
