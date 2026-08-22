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
	// Token: 0x0200027A RID: 634
	internal class CheckConditionalAction : BinaryOptionAction
	{
		// Token: 0x17000EDC RID: 3804
		// (get) Token: 0x060038AE RID: 14510 RVA: 0x00218C54 File Offset: 0x00216E54
		// (set) Token: 0x060038AF RID: 14511 RVA: 0x00218C5C File Offset: 0x00216E5C
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000EDD RID: 3805
		// (get) Token: 0x060038B0 RID: 14512 RVA: 0x00218C65 File Offset: 0x00216E65
		// (set) Token: 0x060038B1 RID: 14513 RVA: 0x00218C6D File Offset: 0x00216E6D
		[Serialize(PropertyConditional.LogicalOperatorType.Or, IsPropertySaveable.Yes, "Do all of the conditions need to be met, or is it enough if at least one is? Only valid if there are multiple conditionals.", "", false)]
		public PropertyConditional.LogicalOperatorType LogicalOperator { get; set; }

		// Token: 0x17000EDE RID: 3806
		// (get) Token: 0x060038B2 RID: 14514 RVA: 0x00218C76 File Offset: 0x00216E76
		private ImmutableArray<PropertyConditional> Conditionals { get; }

		// Token: 0x17000EDF RID: 3807
		// (get) Token: 0x060038B3 RID: 14515 RVA: 0x00218C7E File Offset: 0x00216E7E
		// (set) Token: 0x060038B4 RID: 14516 RVA: 0x00218C86 File Offset: 0x00216E86
		[Serialize("", IsPropertySaveable.Yes, "A tag to apply to the hull the target is currently in when the check succeeds, as well as all the hulls linked to it.", "", false)]
		public Identifier ApplyTagToLinkedHulls { get; set; }

		// Token: 0x17000EE0 RID: 3808
		// (get) Token: 0x060038B5 RID: 14517 RVA: 0x00218C8F File Offset: 0x00216E8F
		// (set) Token: 0x060038B6 RID: 14518 RVA: 0x00218C97 File Offset: 0x00216E97
		[Serialize("", IsPropertySaveable.Yes, "A tag to apply to the hull the target is currently in when the check succeeds.", "", false)]
		public Identifier ApplyTagToHull { get; set; }

		// Token: 0x17000EE1 RID: 3809
		// (get) Token: 0x060038B7 RID: 14519 RVA: 0x00218CA0 File Offset: 0x00216EA0
		// (set) Token: 0x060038B8 RID: 14520 RVA: 0x00218CA8 File Offset: 0x00216EA8
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the target (or all targets if there's multiple) when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x17000EE2 RID: 3810
		// (get) Token: 0x060038B9 RID: 14521 RVA: 0x00218CB1 File Offset: 0x00216EB1
		// (set) Token: 0x060038BA RID: 14522 RVA: 0x00218CB9 File Offset: 0x00216EB9
		[Serialize(true, IsPropertySaveable.Yes, "Should the check fail if no targets matching the specified tag are found?", "", false)]
		public bool FailIfTargetNotFound { get; set; }

		// Token: 0x060038BB RID: 14523 RVA: 0x00218CC4 File Offset: 0x00216EC4
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

		// Token: 0x060038BC RID: 14524 RVA: 0x00218DFC File Offset: 0x00216FFC
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

		// Token: 0x060038BD RID: 14525 RVA: 0x00218EF4 File Offset: 0x002170F4
		private bool ConditionalsMatch(ISerializableEntity target)
		{
			if (this.LogicalOperator == PropertyConditional.LogicalOperatorType.And)
			{
				return this.Conditionals.All((PropertyConditional c) => CheckConditionalAction.ConditionalMatches(target, c));
			}
			return this.Conditionals.Any((PropertyConditional c) => CheckConditionalAction.ConditionalMatches(target, c));
		}

		// Token: 0x060038BE RID: 14526 RVA: 0x00218F48 File Offset: 0x00217148
		private static bool ConditionalMatches(ISerializableEntity target, PropertyConditional conditional)
		{
			Item item = target as Item;
			if (item != null)
			{
				return (conditional.TargetItemComponent.IsNullOrEmpty() || !item.Components.None((ItemComponent ic) => ic.Name == conditional.TargetItemComponent)) && item.ConditionalMatches(conditional);
			}
			return conditional.Matches(target);
		}

		// Token: 0x060038BF RID: 14527 RVA: 0x00218FB4 File Offset: 0x002171B4
		[CompilerGenerated]
		internal static bool <.ctor>g__IsConditionalAttribute|27_0(XAttribute attribute)
		{
			Identifier nameAsIdentifier = attribute.NameAsIdentifier();
			return nameAsIdentifier != "TargetTag" && nameAsIdentifier != "LogicalOperator" && nameAsIdentifier != "ApplyTagToLinkedHulls" && nameAsIdentifier != "ApplyTagToHull";
		}

		// Token: 0x060038C0 RID: 14528 RVA: 0x00219000 File Offset: 0x00217200
		[CompilerGenerated]
		private void <DetermineSuccess>g__ApplyTagsToTarget|28_0(ISerializableEntity target)
		{
			if (!this.ApplyTagToTarget.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyTagToTarget, target as Entity);
			}
			base.ApplyTagsToHulls(target as Entity, this.ApplyTagToHull, this.ApplyTagToLinkedHulls);
		}

		// Token: 0x02000F14 RID: 3860
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400549B RID: 21659
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Predicate<XAttribute> <0>__IsConditionalAttribute;
		}
	}
}
