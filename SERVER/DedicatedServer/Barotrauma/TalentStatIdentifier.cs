using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001FC RID: 508
	[NetworkSerialize(9)]
	internal readonly struct TalentStatIdentifier : INetSerializableStruct, IEquatable<TalentStatIdentifier>
	{
		// Token: 0x06002498 RID: 9368 RVA: 0x000F1B93 File Offset: 0x000EFD93
		public TalentStatIdentifier(ItemTalentStats Stat, Identifier TalentIdentifier, Option<uint> UniqueCharacterId, bool Save)
		{
			this.Stat = Stat;
			this.TalentIdentifier = TalentIdentifier;
			this.UniqueCharacterId = UniqueCharacterId;
			this.Save = Save;
		}

		// Token: 0x17000AAB RID: 2731
		// (get) Token: 0x06002499 RID: 9369 RVA: 0x000F1BB2 File Offset: 0x000EFDB2
		// (set) Token: 0x0600249A RID: 9370 RVA: 0x000F1BBA File Offset: 0x000EFDBA
		public ItemTalentStats Stat { get; set; }

		// Token: 0x17000AAC RID: 2732
		// (get) Token: 0x0600249B RID: 9371 RVA: 0x000F1BC3 File Offset: 0x000EFDC3
		// (set) Token: 0x0600249C RID: 9372 RVA: 0x000F1BCB File Offset: 0x000EFDCB
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x17000AAD RID: 2733
		// (get) Token: 0x0600249D RID: 9373 RVA: 0x000F1BD4 File Offset: 0x000EFDD4
		// (set) Token: 0x0600249E RID: 9374 RVA: 0x000F1BDC File Offset: 0x000EFDDC
		public Option<uint> UniqueCharacterId { get; set; }

		// Token: 0x17000AAE RID: 2734
		// (get) Token: 0x0600249F RID: 9375 RVA: 0x000F1BE5 File Offset: 0x000EFDE5
		// (set) Token: 0x060024A0 RID: 9376 RVA: 0x000F1BED File Offset: 0x000EFDED
		public bool Save { get; set; }

		// Token: 0x060024A1 RID: 9377 RVA: 0x000F1BF6 File Offset: 0x000EFDF6
		public static TalentStatIdentifier CreateStackable(ItemTalentStats stat, Identifier talentIdentifier, uint characterId)
		{
			return new TalentStatIdentifier(stat, talentIdentifier, Option<uint>.Some(characterId), false);
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x000F1C08 File Offset: 0x000EFE08
		public static TalentStatIdentifier CreateUnstackable(ItemTalentStats stat, Identifier talentIdentifier, bool Save)
		{
			Option.UnspecifiedNone none = Option.None;
			return new TalentStatIdentifier(stat, talentIdentifier, none, Save);
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x000F1C2C File Offset: 0x000EFE2C
		[NullableContext(1)]
		public XElement Serialize()
		{
			return new XElement("Stat", new object[]
			{
				new XAttribute("type", this.Stat),
				new XAttribute("talent", this.TalentIdentifier)
			});
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x000F1C88 File Offset: 0x000EFE88
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

		// Token: 0x060024A5 RID: 9381 RVA: 0x000F1D1C File Offset: 0x000EFF1C
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

		// Token: 0x060024A6 RID: 9382 RVA: 0x000F1D68 File Offset: 0x000EFF68
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

		// Token: 0x060024A7 RID: 9383 RVA: 0x000F1E12 File Offset: 0x000F0012
		[CompilerGenerated]
		public static bool operator !=(TalentStatIdentifier left, TalentStatIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x000F1E1E File Offset: 0x000F001E
		[CompilerGenerated]
		public static bool operator ==(TalentStatIdentifier left, TalentStatIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x000F1E28 File Offset: 0x000F0028
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<ItemTalentStats>.Default.GetHashCode(this.<Stat>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TalentIdentifier>k__BackingField)) * -1521134295 + EqualityComparer<Option<uint>>.Default.GetHashCode(this.<UniqueCharacterId>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Save>k__BackingField);
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x000F1E8A File Offset: 0x000F008A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentStatIdentifier && this.Equals((TalentStatIdentifier)obj);
		}

		// Token: 0x060024AB RID: 9387 RVA: 0x000F1EA4 File Offset: 0x000F00A4
		[CompilerGenerated]
		public bool Equals(TalentStatIdentifier other)
		{
			return EqualityComparer<ItemTalentStats>.Default.Equals(this.<Stat>k__BackingField, other.<Stat>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TalentIdentifier>k__BackingField, other.<TalentIdentifier>k__BackingField) && EqualityComparer<Option<uint>>.Default.Equals(this.<UniqueCharacterId>k__BackingField, other.<UniqueCharacterId>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Save>k__BackingField, other.<Save>k__BackingField);
		}

		// Token: 0x060024AC RID: 9388 RVA: 0x000F1F11 File Offset: 0x000F0111
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
