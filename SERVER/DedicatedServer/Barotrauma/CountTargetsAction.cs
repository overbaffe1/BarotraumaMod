using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000198 RID: 408
	[NullableContext(1)]
	[Nullable(0)]
	internal class CountTargetsAction : BinaryOptionAction
	{
		// Token: 0x170008D0 RID: 2256
		// (get) Token: 0x06001EC4 RID: 7876 RVA: 0x000D6F41 File Offset: 0x000D5141
		// (set) Token: 0x06001EC5 RID: 7877 RVA: 0x000D6F49 File Offset: 0x000D5149
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entities to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008D1 RID: 2257
		// (get) Token: 0x06001EC6 RID: 7878 RVA: 0x000D6F52 File Offset: 0x000D5152
		// (set) Token: 0x06001EC7 RID: 7879 RVA: 0x000D6F5A File Offset: 0x000D515A
		[Serialize("", IsPropertySaveable.Yes, "Optional second tag. Can be used if the target must have two different tags.", "", false)]
		public Identifier SecondRequiredTargetTag { get; set; }

		// Token: 0x170008D2 RID: 2258
		// (get) Token: 0x06001EC8 RID: 7880 RVA: 0x000D6F63 File Offset: 0x000D5163
		// (set) Token: 0x06001EC9 RID: 7881 RVA: 0x000D6F6B File Offset: 0x000D516B
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of a hull the target must be inside.", "", false)]
		public Identifier HullTag { get; set; }

		// Token: 0x170008D3 RID: 2259
		// (get) Token: 0x06001ECA RID: 7882 RVA: 0x000D6F74 File Offset: 0x000D5174
		// (set) Token: 0x06001ECB RID: 7883 RVA: 0x000D6F7C File Offset: 0x000D517C
		[Serialize(-1, IsPropertySaveable.Yes, "Minimum number of matching entities for the check to succeed. If omitted or negative, there is no minimum amount.", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x170008D4 RID: 2260
		// (get) Token: 0x06001ECC RID: 7884 RVA: 0x000D6F85 File Offset: 0x000D5185
		// (set) Token: 0x06001ECD RID: 7885 RVA: 0x000D6F8D File Offset: 0x000D518D
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of matching entities for the check to succeed. If omitted or negative, there is no maximum amount.", "", false)]
		public int MaxAmount { get; set; }

		// Token: 0x170008D5 RID: 2261
		// (get) Token: 0x06001ECE RID: 7886 RVA: 0x000D6F96 File Offset: 0x000D5196
		// (set) Token: 0x06001ECF RID: 7887 RVA: 0x000D6F9E File Offset: 0x000D519E
		[Serialize("", IsPropertySaveable.Yes, "Tag of some other entities to compare the number of targets to. E.g. you could compare the number of entities tagged as \"discoveredhull\" to entities tagged as \"anyhull\". The minimum/maximum amount of entities there must be relative to the other entities is configured using MinPercentageRelativeToTarget and MaxPercentageRelativeToTarget.", "", false)]
		public Identifier CompareToTarget { get; set; }

		// Token: 0x170008D6 RID: 2262
		// (get) Token: 0x06001ED0 RID: 7888 RVA: 0x000D6FA7 File Offset: 0x000D51A7
		// (set) Token: 0x06001ED1 RID: 7889 RVA: 0x000D6FAF File Offset: 0x000D51AF
		[Serialize(-1f, IsPropertySaveable.Yes, "Minimum amount of targets, as a percentage of the number of entities tagged with CompareToTarget. E.g. you could compare the number of entities tagged as \"discoveredhull\" to entities tagged as \"anyhull\" to require 50% of hulls to be discovered.", "", false)]
		public float MinPercentageRelativeToTarget { get; set; }

		// Token: 0x170008D7 RID: 2263
		// (get) Token: 0x06001ED2 RID: 7890 RVA: 0x000D6FB8 File Offset: 0x000D51B8
		// (set) Token: 0x06001ED3 RID: 7891 RVA: 0x000D6FC0 File Offset: 0x000D51C0
		[Serialize(-1f, IsPropertySaveable.Yes, "Maximum amount of targets, as a percentage of the number of entities tagged with CompareToTarget. E.g. you could compare the number of entities tagged as \"floodedhull\" to entities tagged as \"anyhull\" to require less than 50% of hulls to be flooded.", "", false)]
		public float MaxPercentageRelativeToTarget { get; set; }

		// Token: 0x06001ED4 RID: 7892 RVA: 0x000D6FCC File Offset: 0x000D51CC
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

		// Token: 0x06001ED5 RID: 7893 RVA: 0x000D71C4 File Offset: 0x000D53C4
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

		// Token: 0x06001ED6 RID: 7894 RVA: 0x000D7328 File Offset: 0x000D5528
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

		// Token: 0x04000EC4 RID: 3780
		private readonly IReadOnlyList<PropertyConditional> conditionals;
	}
}
