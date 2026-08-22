using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000191 RID: 401
	internal class CheckSelectedAction : BinaryOptionAction
	{
		// Token: 0x170008B8 RID: 2232
		// (get) Token: 0x06001E79 RID: 7801 RVA: 0x000D5E9E File Offset: 0x000D409E
		// (set) Token: 0x06001E7A RID: 7802 RVA: 0x000D5EA6 File Offset: 0x000D40A6
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier CharacterTag { get; set; }

		// Token: 0x170008B9 RID: 2233
		// (get) Token: 0x06001E7B RID: 7803 RVA: 0x000D5EAF File Offset: 0x000D40AF
		// (set) Token: 0x06001E7C RID: 7804 RVA: 0x000D5EB7 File Offset: 0x000D40B7
		[Serialize("", IsPropertySaveable.Yes, "If specified, only items that have been given this tag using TagAction are considered valid.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008BA RID: 2234
		// (get) Token: 0x06001E7D RID: 7805 RVA: 0x000D5EC0 File Offset: 0x000D40C0
		// (set) Token: 0x06001E7E RID: 7806 RVA: 0x000D5EC8 File Offset: 0x000D40C8
		[Serialize(CheckSelectedAction.SelectedItemType.Any, IsPropertySaveable.Yes, "How does the item need to be selected? Primary item (i.e. any device you're interacting with), secondary item (such as ladders or chairs which allow interacting with a primary item at the same time), or either?", "", false)]
		public CheckSelectedAction.SelectedItemType ItemType { get; set; }

		// Token: 0x06001E7F RID: 7807 RVA: 0x000D5ED1 File Offset: 0x000D40D1
		public CheckSelectedAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x000D5EDC File Offset: 0x000D40DC
		protected override bool? DetermineSuccess()
		{
			CheckSelectedAction.<>c__DisplayClass14_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.character = null;
			if (!this.CharacterTag.IsEmpty)
			{
				foreach (Entity t in this.ParentEvent.GetTargets(this.CharacterTag))
				{
					Character c = t as Character;
					if (c != null)
					{
						CS$<>8__locals1.character = c;
						break;
					}
				}
			}
			if (CS$<>8__locals1.character == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(110, 4);
				defaultInterpolatedStringHandler.AppendFormatted("CheckSelectedAction");
				defaultInterpolatedStringHandler.AppendLiteral(" error: ");
				defaultInterpolatedStringHandler.AppendFormatted(this.GetEventName());
				defaultInterpolatedStringHandler.AppendLiteral(" uses a ");
				defaultInterpolatedStringHandler.AppendFormatted("CheckSelectedAction");
				defaultInterpolatedStringHandler.AppendLiteral(" but no valid character was found for tag \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.CharacterTag);
				defaultInterpolatedStringHandler.AppendLiteral("\"! This will cause the check to automatically fail.");
				this.<DetermineSuccess>g__Error|14_0(defaultInterpolatedStringHandler.ToStringAndClear(), ref CS$<>8__locals1);
				return new bool?(false);
			}
			if (this.TargetTag.IsEmpty)
			{
				bool value;
				switch (this.ItemType)
				{
				case CheckSelectedAction.SelectedItemType.Primary:
					value = (CS$<>8__locals1.character.SelectedItem == null);
					break;
				case CheckSelectedAction.SelectedItemType.Secondary:
					value = (CS$<>8__locals1.character.SelectedSecondaryItem == null);
					break;
				case CheckSelectedAction.SelectedItemType.Any:
					value = !CS$<>8__locals1.character.HasSelectedAnyItem;
					break;
				default:
					value = false;
					break;
				}
				return new bool?(value);
			}
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			if (targets.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(109, 4);
				defaultInterpolatedStringHandler2.AppendFormatted("CheckSelectedAction");
				defaultInterpolatedStringHandler2.AppendLiteral(" error: ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.GetEventName());
				defaultInterpolatedStringHandler2.AppendLiteral(" uses a ");
				defaultInterpolatedStringHandler2.AppendFormatted("CheckSelectedAction");
				defaultInterpolatedStringHandler2.AppendLiteral(" but no valid targets were found for tag \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TargetTag);
				defaultInterpolatedStringHandler2.AppendLiteral("\"! This will cause the check to automatically fail.");
				this.<DetermineSuccess>g__Error|14_0(defaultInterpolatedStringHandler2.ToStringAndClear(), ref CS$<>8__locals1);
				return new bool?(false);
			}
			foreach (Entity target in targets)
			{
				Character targetCharacter = target as Character;
				if (targetCharacter != null)
				{
					if (this.ItemType == CheckSelectedAction.SelectedItemType.Any && CS$<>8__locals1.character.SelectedCharacter == targetCharacter)
					{
						return new bool?(true);
					}
				}
				else
				{
					Item targetItem = target as Item;
					if (targetItem != null && this.<DetermineSuccess>g__IsSelected|14_1(targetItem, ref CS$<>8__locals1))
					{
						return new bool?(true);
					}
				}
			}
			return new bool?(false);
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x000D619C File Offset: 0x000D439C
		private string GetEventName()
		{
			ScriptedEvent parentEvent = this.ParentEvent;
			Identifier? identifier2;
			if (parentEvent == null)
			{
				identifier2 = null;
			}
			else
			{
				EventPrefab prefab = parentEvent.Prefab;
				identifier2 = ((prefab != null) ? new Identifier?(prefab.Identifier) : null);
			}
			Identifier? identifier3 = identifier2;
			if (identifier3 != null)
			{
				Identifier identifier = identifier3.GetValueOrDefault();
				if (!identifier.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("the event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}
			return "an unknown event";
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x000D6230 File Offset: 0x000D4430
		[CompilerGenerated]
		private bool <DetermineSuccess>g__IsSelected|14_1(Item item, ref CheckSelectedAction.<>c__DisplayClass14_0 A_2)
		{
			bool result;
			switch (this.ItemType)
			{
			case CheckSelectedAction.SelectedItemType.Primary:
				result = (A_2.character.SelectedItem == item);
				break;
			case CheckSelectedAction.SelectedItemType.Secondary:
				result = (A_2.character.SelectedSecondaryItem == item);
				break;
			case CheckSelectedAction.SelectedItemType.Any:
				result = A_2.character.IsAnySelectedItem(item);
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x000D628C File Offset: 0x000D448C
		[CompilerGenerated]
		private void <DetermineSuccess>g__Error|14_0(string errorMsg, ref CheckSelectedAction.<>c__DisplayClass14_0 A_2)
		{
			ContentPackage contentPackage = this.ParentEvent.Prefab.ContentPackage;
			DebugConsole.LogError(errorMsg, null, contentPackage);
		}

		// Token: 0x0200090D RID: 2317
		public enum SelectedItemType
		{
			// Token: 0x040031E2 RID: 12770
			Primary,
			// Token: 0x040031E3 RID: 12771
			Secondary,
			// Token: 0x040031E4 RID: 12772
			Any
		}
	}
}
