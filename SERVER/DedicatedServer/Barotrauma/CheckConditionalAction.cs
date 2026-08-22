using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000186 RID: 390
	internal class CheckConditionalAction : BinaryOptionAction
	{
		// Token: 0x17000888 RID: 2184
		// (get) Token: 0x06001DE2 RID: 7650 RVA: 0x000D36A0 File Offset: 0x000D18A0
		// (set) Token: 0x06001DE3 RID: 7651 RVA: 0x000D36A8 File Offset: 0x000D18A8
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000889 RID: 2185
		// (get) Token: 0x06001DE4 RID: 7652 RVA: 0x000D36B1 File Offset: 0x000D18B1
		// (set) Token: 0x06001DE5 RID: 7653 RVA: 0x000D36B9 File Offset: 0x000D18B9
		[Serialize(PropertyConditional.LogicalOperatorType.Or, IsPropertySaveable.Yes, "Do all of the conditions need to be met, or is it enough if at least one is? Only valid if there are multiple conditionals.", "", false)]
		public PropertyConditional.LogicalOperatorType LogicalOperator { get; set; }

		// Token: 0x1700088A RID: 2186
		// (get) Token: 0x06001DE6 RID: 7654 RVA: 0x000D36C2 File Offset: 0x000D18C2
		private ImmutableArray<PropertyConditional> Conditionals { get; }

		// Token: 0x1700088B RID: 2187
		// (get) Token: 0x06001DE7 RID: 7655 RVA: 0x000D36CA File Offset: 0x000D18CA
		// (set) Token: 0x06001DE8 RID: 7656 RVA: 0x000D36D2 File Offset: 0x000D18D2
		[Serialize("", IsPropertySaveable.Yes, "A tag to apply to the hull the target is currently in when the check succeeds, as well as all the hulls linked to it.", "", false)]
		public Identifier ApplyTagToLinkedHulls { get; set; }

		// Token: 0x1700088C RID: 2188
		// (get) Token: 0x06001DE9 RID: 7657 RVA: 0x000D36DB File Offset: 0x000D18DB
		// (set) Token: 0x06001DEA RID: 7658 RVA: 0x000D36E3 File Offset: 0x000D18E3
		[Serialize("", IsPropertySaveable.Yes, "A tag to apply to the hull the target is currently in when the check succeeds.", "", false)]
		public Identifier ApplyTagToHull { get; set; }

		// Token: 0x1700088D RID: 2189
		// (get) Token: 0x06001DEB RID: 7659 RVA: 0x000D36EC File Offset: 0x000D18EC
		// (set) Token: 0x06001DEC RID: 7660 RVA: 0x000D36F4 File Offset: 0x000D18F4
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the target (or all targets if there's multiple) when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x1700088E RID: 2190
		// (get) Token: 0x06001DED RID: 7661 RVA: 0x000D36FD File Offset: 0x000D18FD
		// (set) Token: 0x06001DEE RID: 7662 RVA: 0x000D3705 File Offset: 0x000D1905
		[Serialize(true, IsPropertySaveable.Yes, "Should the check fail if no targets matching the specified tag are found?", "", false)]
		public bool FailIfTargetNotFound { get; set; }

		// Token: 0x06001DEF RID: 7663 RVA: 0x000D3710 File Offset: 0x000D1910
		public CheckConditionalAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.TargetTag.IsEmpty)
			{
				string msg = "CheckConditionalAction error: " + base.GetEventDebugName() + " uses a CheckConditionalAction with no target tag! This will cause the check to automatically succeed.";
				ContentPackage contentPackage = parentEvent.Prefab.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
			}
			IEnumerable<ContentXElement> conditionalElements = element.GetChildElements("Conditional");
			if (conditionalElements.None(null))
			{
				Predicate<XAttribute> predicate;
				if ((predicate = CheckConditionalAction.<>O.<0>__IsConditionalAttribute) == null)
				{
					predicate = (CheckConditionalAction.<>O.<0>__IsConditionalAttribute = new Predicate<XAttribute>(CheckConditionalAction.<.ctor>g__IsConditionalAttribute|27_0));
				}
				this.Conditionals = PropertyConditional.FromXElement(element, predicate).ToImmutableArray<PropertyConditional>();
			}
			else
			{
				List<PropertyConditional> conditionalList = new List<PropertyConditional>();
				foreach (ContentXElement subElement in conditionalElements)
				{
					conditionalList.AddRange(PropertyConditional.FromXElement(subElement, null));
				}
				this.Conditionals = conditionalList.ToImmutableArray<PropertyConditional>();
			}
			if (this.Conditionals.None(null))
			{
				string msg2 = "CheckConditionalAction error: " + base.GetEventDebugName() + " uses a CheckConditionalAction with no valid PropertyConditional! This will cause the check to automatically succeed.";
				ContentPackage contentPackage = parentEvent.Prefab.ContentPackage;
				DebugConsole.LogError(msg2, null, contentPackage);
			}
		}

		// Token: 0x06001DF0 RID: 7664 RVA: 0x000D3848 File Offset: 0x000D1A48
		protected override bool? DetermineSuccess()
		{
			IEnumerable<ISerializableEntity> targets = null;
			if (!this.TargetTag.IsEmpty)
			{
				targets = this.ParentEvent.GetTargets(this.TargetTag).OfType<ISerializableEntity>();
			}
			if (targets.None(null))
			{
				return new bool?(!this.FailIfTargetNotFound);
			}
			if (this.Conditionals.None(null))
			{
				foreach (ISerializableEntity target in targets)
				{
					this.<DetermineSuccess>g__ApplyTagsToTarget|28_0(target);
				}
				return new bool?(true);
			}
			bool success = false;
			foreach (ISerializableEntity target2 in targets)
			{
				if (this.ConditionalsMatch(target2))
				{
					this.<DetermineSuccess>g__ApplyTagsToTarget|28_0(target2);
					success = true;
				}
			}
			return new bool?(success);
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x000D3940 File Offset: 0x000D1B40
		private bool ConditionalsMatch(ISerializableEntity target)
		{
			if (this.LogicalOperator == PropertyConditional.LogicalOperatorType.And)
			{
				return this.Conditionals.All((PropertyConditional c) => CheckConditionalAction.ConditionalMatches(target, c));
			}
			return this.Conditionals.Any((PropertyConditional c) => CheckConditionalAction.ConditionalMatches(target, c));
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x000D3994 File Offset: 0x000D1B94
		private static bool ConditionalMatches(ISerializableEntity target, PropertyConditional conditional)
		{
			Item item = target as Item;
			if (item != null)
			{
				return (conditional.TargetItemComponent.IsNullOrEmpty() || !item.Components.None((ItemComponent ic) => ic.Name == conditional.TargetItemComponent)) && item.ConditionalMatches(conditional);
			}
			return conditional.Matches(target);
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x000D3A00 File Offset: 0x000D1C00
		[CompilerGenerated]
		internal static bool <.ctor>g__IsConditionalAttribute|27_0(XAttribute attribute)
		{
			Identifier nameAsIdentifier = attribute.NameAsIdentifier();
			return nameAsIdentifier != "TargetTag" && nameAsIdentifier != "LogicalOperator" && nameAsIdentifier != "ApplyTagToLinkedHulls" && nameAsIdentifier != "ApplyTagToHull";
		}

		// Token: 0x06001DF4 RID: 7668 RVA: 0x000D3A4C File Offset: 0x000D1C4C
		[CompilerGenerated]
		private void <DetermineSuccess>g__ApplyTagsToTarget|28_0(ISerializableEntity target)
		{
			if (!this.ApplyTagToTarget.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyTagToTarget, target as Entity);
			}
			base.ApplyTagsToHulls(target as Entity, this.ApplyTagToHull, this.ApplyTagToLinkedHulls);
		}

		// Token: 0x02000902 RID: 2306
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040031CD RID: 12749
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Predicate<XAttribute> <0>__IsConditionalAttribute;
		}
	}
}
