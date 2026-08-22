using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000284 RID: 644
	internal class CheckSelectedAction : BinaryOptionAction
	{
		// Token: 0x17000F0C RID: 3852
		// (get) Token: 0x06003943 RID: 14659 RVA: 0x0021B39A File Offset: 0x0021959A
		// (set) Token: 0x06003944 RID: 14660 RVA: 0x0021B3A2 File Offset: 0x002195A2
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier CharacterTag { get; set; }

		// Token: 0x17000F0D RID: 3853
		// (get) Token: 0x06003945 RID: 14661 RVA: 0x0021B3AB File Offset: 0x002195AB
		// (set) Token: 0x06003946 RID: 14662 RVA: 0x0021B3B3 File Offset: 0x002195B3
		[Serialize("", IsPropertySaveable.Yes, "If specified, only items that have been given this tag using TagAction are considered valid.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F0E RID: 3854
		// (get) Token: 0x06003947 RID: 14663 RVA: 0x0021B3BC File Offset: 0x002195BC
		// (set) Token: 0x06003948 RID: 14664 RVA: 0x0021B3C4 File Offset: 0x002195C4
		[Serialize(CheckSelectedAction.SelectedItemType.Any, IsPropertySaveable.Yes, "How does the item need to be selected? Primary item (i.e. any device you're interacting with), secondary item (such as ladders or chairs which allow interacting with a primary item at the same time), or either?", "", false)]
		public CheckSelectedAction.SelectedItemType ItemType { get; set; }

		// Token: 0x06003949 RID: 14665 RVA: 0x0021B3CD File Offset: 0x002195CD
		public CheckSelectedAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600394A RID: 14666 RVA: 0x0021B3D8 File Offset: 0x002195D8
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

		// Token: 0x0600394B RID: 14667 RVA: 0x0021B698 File Offset: 0x00219898
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

		// Token: 0x0600394C RID: 14668 RVA: 0x0021B72C File Offset: 0x0021992C
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

		// Token: 0x0600394D RID: 14669 RVA: 0x0021B788 File Offset: 0x00219988
		[CompilerGenerated]
		private void <DetermineSuccess>g__Error|14_0(string errorMsg, ref CheckSelectedAction.<>c__DisplayClass14_0 A_2)
		{
			ContentPackage contentPackage = this.ParentEvent.Prefab.ContentPackage;
			DebugConsole.LogError(errorMsg, null, contentPackage);
		}

		// Token: 0x02000F1E RID: 3870
		public enum SelectedItemType
		{
			// Token: 0x040054AF RID: 21679
			Primary,
			// Token: 0x040054B0 RID: 21680
			Secondary,
			// Token: 0x040054B1 RID: 21681
			Any
		}
	}
}
