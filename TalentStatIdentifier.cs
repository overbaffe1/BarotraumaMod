using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002E5 RID: 741
	[NetworkSerialize(9)]
	internal readonly struct TalentStatIdentifier : INetSerializableStruct, IEquatable<TalentStatIdentifier>
	{
		// Token: 0x06003D76 RID: 15734 RVA: 0x0022F0FC File Offset: 0x0022D2FC
		public TalentStatIdentifier(ItemTalentStats Stat, Identifier TalentIdentifier, Option<uint> UniqueCharacterId, bool Save)
		{
			this.Stat = Stat;
			this.TalentIdentifier = TalentIdentifier;
			this.UniqueCharacterId = UniqueCharacterId;
			this.Save = Save;
		}

		// Token: 0x17001043 RID: 4163
		// (get) Token: 0x06003D77 RID: 15735 RVA: 0x0022F11B File Offset: 0x0022D31B
		// (set) Token: 0x06003D78 RID: 15736 RVA: 0x0022F123 File Offset: 0x0022D323
		public ItemTalentStats Stat { get; set; }

		// Token: 0x17001044 RID: 4164
		// (get) Token: 0x06003D79 RID: 15737 RVA: 0x0022F12C File Offset: 0x0022D32C
		// (set) Token: 0x06003D7A RID: 15738 RVA: 0x0022F134 File Offset: 0x0022D334
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x17001045 RID: 4165
		// (get) Token: 0x06003D7B RID: 15739 RVA: 0x0022F13D File Offset: 0x0022D33D
		// (set) Token: 0x06003D7C RID: 15740 RVA: 0x0022F145 File Offset: 0x0022D345
		public Option<uint> UniqueCharacterId { get; set; }

		// Token: 0x17001046 RID: 4166
		// (get) Token: 0x06003D7D RID: 15741 RVA: 0x0022F14E File Offset: 0x0022D34E
		// (set) Token: 0x06003D7E RID: 15742 RVA: 0x0022F156 File Offset: 0x0022D356
		public bool Save { get; set; }

		// Token: 0x06003D7F RID: 15743 RVA: 0x0022F15F File Offset: 0x0022D35F
		public static TalentStatIdentifier CreateStackable(ItemTalentStats stat, Identifier talentIdentifier, uint characterId)
		{
			return new TalentStatIdentifier(stat, talentIdentifier, Option<uint>.Some(characterId), false);
		}

		// Token: 0x06003D80 RID: 15744 RVA: 0x0022F170 File Offset: 0x0022D370
		public static TalentStatIdentifier CreateUnstackable(ItemTalentStats stat, Identifier talentIdentifier, bool Save)
		{
			Option.UnspecifiedNone none = Option.None;
			return new TalentStatIdentifier(stat, talentIdentifier, none, Save);
		}

		// Token: 0x06003D81 RID: 15745 RVA: 0x0022F194 File Offset: 0x0022D394
		[NullableContext(1)]
		public XElement Serialize()
		{
			return new XElement("Stat", new object[]
			{
				new XAttribute("type", this.Stat),
				new XAttribute("talent", this.TalentIdentifier)
			});
		}

		// Token: 0x06003D82 RID: 15746 RVA: 0x0022F1F0 File Offset: 0x0022D3F0
		public static Option<TalentStatIdentifier> TryLoadFromXML([Nullable(1)] XElement element)
		{
			ItemTalentStats stat = element.GetAttributeEnum("type", ItemTalentStats.None);
			Identifier talentIdentifier = element.GetAttributeIdentifier("talent", Identifier.Empty);
			if (stat == ItemTalentStats.None || talentIdentifier == Identifier.Empty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to load talent stat identifier from XML ");
				defaultInterpolatedStringHandler.AppendFormatted<XElement>(element);
				string error = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.ThrowError(error, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("ItemStatManager.TryLoadFromXML:Invalid", GameAnalyticsManager.ErrorSeverity.Error, error);
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			return Option.Some<TalentStatIdentifier>(TalentStatIdentifier.CreateUnstackable(stat, talentIdentifier, true));
		}

		// Token: 0x06003D83 RID: 15747 RVA: 0x0022F284 File Offset: 0x0022D484
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TalentStatIdentifier");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D84 RID: 15748 RVA: 0x0022F2D0 File Offset: 0x0022D4D0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Stat = ");
			builder.Append(this.Stat.ToString());
			builder.Append(", TalentIdentifier = ");
			builder.Append(this.TalentIdentifier.ToString());
			builder.Append(", UniqueCharacterId = ");
			builder.Append(this.UniqueCharacterId.ToString());
			builder.Append(", Save = ");
			builder.Append(this.Save.ToString());
			return true;
		}

		// Token: 0x06003D85 RID: 15749 RVA: 0x0022F37A File Offset: 0x0022D57A
		[CompilerGenerated]
		public static bool operator !=(TalentStatIdentifier left, TalentStatIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x06003D86 RID: 15750 RVA: 0x0022F386 File Offset: 0x0022D586
		[CompilerGenerated]
		public static bool operator ==(TalentStatIdentifier left, TalentStatIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003D87 RID: 15751 RVA: 0x0022F390 File Offset: 0x0022D590
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ItemTalentStats>.Default.GetHashCode(this.<Stat>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TalentIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<Option<uint>>.Default.GetHashCode(this.<UniqueCharacterId>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Save>k__BackingField);
		}

		// Token: 0x06003D88 RID: 15752 RVA: 0x0022F3F2 File Offset: 0x0022D5F2
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentStatIdentifier && this.Equals((TalentStatIdentifier)obj);
		}

		// Token: 0x06003D89 RID: 15753 RVA: 0x0022F40C File Offset: 0x0022D60C
		[CompilerGenerated]
		public bool Equals(TalentStatIdentifier other)
		{
			return EqualityComparer<ItemTalentStats>.Default.Equals(this.<Stat>k__BackingField, other.<Stat>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TalentIdentifier>k__BackingField, other.<TalentIdentifier>k__BackingField) && EqualityComparer<Option<uint>>.Default.Equals(this.<UniqueCharacterId>k__BackingField, other.<UniqueCharacterId>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Save>k__BackingField, other.<Save>k__BackingField);
		}

		// Token: 0x06003D8A RID: 15754 RVA: 0x0022F479 File Offset: 0x0022D679
		[CompilerGenerated]
		public void Deconstruct(out ItemTalentStats Stat, out Identifier TalentIdentifier, out Option<uint> UniqueCharacterId, out bool Save)
		{
			Stat = this.Stat;
			TalentIdentifier = this.TalentIdentifier;
			UniqueCharacterId = this.UniqueCharacterId;
			Save = this.Save;
		}
	}
}
