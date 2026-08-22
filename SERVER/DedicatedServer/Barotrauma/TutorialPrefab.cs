using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020001E8 RID: 488
	internal class TutorialPrefab : Prefab
	{
		// Token: 0x06002304 RID: 8964 RVA: 0x000E9A3C File Offset: 0x000E7C3C
		public TutorialPrefab(ContentFile file, ContentXElement element) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.Order = element.GetAttributeInt("order", int.MaxValue);
			this.DisableBotConversations = element.GetAttributeBool("disablebotconversations", true);
			this.AllowCharacterSwitch = element.GetAttributeBool("allowcharacterswitch", false);
			this.SubmarinePath = (element.GetAttributeContentPath("submarinepath") ?? this.SubmarinePath);
			this.OutpostPath = (element.GetAttributeContentPath("outpostpath") ?? this.OutpostPath);
			this.LevelSeed = element.GetAttributeString("levelseed", "nLoZLLtza");
			this.LevelParams = element.GetAttributeString("levelparams", "ColdCavernsTutorial");
			this.tutorialCharacterElement = element.GetChildElement("characterinfo");
			ContentXElement contentXElement = null;
			if (this.tutorialCharacterElement != contentXElement)
			{
				this.StartingItemTags = this.tutorialCharacterElement.GetAttributeIdentifierArray("startingitemtags", new Identifier[0], true).ToImmutableArray<Identifier>();
			}
			else
			{
				this.StartingItemTags = ImmutableArray<Identifier>.Empty;
			}
			ContentXElement bannerElement = element.GetChildElement("banner");
			contentXElement = null;
			if (bannerElement != contentXElement)
			{
				this.Banner = new Sprite(bannerElement, "", "", true, 1f);
			}
			ContentXElement childElement = element.GetChildElement("scriptedevent");
			this.EventIdentifier = ((childElement != null) ? childElement.GetAttributeIdentifier("identifier", "") : Identifier.Empty);
			ContentXElement endMessageElement = element.GetChildElement("endmessage");
			if (endMessageElement != null)
			{
				ContentXElement contentXElement2 = endMessageElement;
				string key = "type";
				TutorialPrefab.EndType endType = TutorialPrefab.EndType.None;
				this.EndMessage = new TutorialPrefab.EndMessageInfo(contentXElement2.GetAttributeEnum<TutorialPrefab.EndType>(key, endType), endMessageElement.GetAttributeIdentifier("nexttutorial", Identifier.Empty));
			}
		}

		// Token: 0x06002305 RID: 8965 RVA: 0x000E9C0C File Offset: 0x000E7E0C
		public CharacterInfo GetTutorialCharacterInfo()
		{
			ContentXElement contentXElement = null;
			if (this.tutorialCharacterElement == contentXElement)
			{
				return null;
			}
			Identifier speciesName = this.tutorialCharacterElement.GetAttributeIdentifier("speciesname", CharacterPrefab.HumanSpeciesName);
			Identifier jobPrefabIdentifier = this.tutorialCharacterElement.GetAttributeIdentifier("jobidentifier", "assistant");
			JobPrefab jobPrefab;
			if (!JobPrefab.Prefabs.TryGet(jobPrefabIdentifier, out jobPrefab))
			{
				jobPrefab = JobPrefab.Prefabs.First<JobPrefab>();
			}
			int jobVariant = this.tutorialCharacterElement.GetAttributeInt("variant", 0);
			CharacterInfo characterInfo = new CharacterInfo(speciesName, "", "", jobPrefab, jobVariant, Rand.RandSync.Unsynced, default(Identifier));
			foreach (ContentXElement skillElement in this.tutorialCharacterElement.GetChildElements("skill"))
			{
				Identifier skillIdentifier = skillElement.GetAttributeIdentifier("identifier", "");
				if (!skillIdentifier.IsEmpty)
				{
					float level = skillElement.GetAttributeFloat("level", 0f);
					characterInfo.SetSkillLevel(skillIdentifier, level, false);
				}
			}
			return characterInfo;
		}

		// Token: 0x06002306 RID: 8966 RVA: 0x000E9D30 File Offset: 0x000E7F30
		public override void Dispose()
		{
		}

		// Token: 0x040010EE RID: 4334
		public static readonly PrefabCollection<TutorialPrefab> Prefabs = new PrefabCollection<TutorialPrefab>();

		// Token: 0x040010EF RID: 4335
		public readonly int Order;

		// Token: 0x040010F0 RID: 4336
		public readonly bool DisableBotConversations;

		// Token: 0x040010F1 RID: 4337
		public readonly bool AllowCharacterSwitch;

		// Token: 0x040010F2 RID: 4338
		public readonly ContentPath SubmarinePath = ContentPath.FromRaw("Content/Tutorials/Dugong_Tutorial.sub");

		// Token: 0x040010F3 RID: 4339
		public readonly ContentPath OutpostPath = ContentPath.FromRaw("Content/Tutorials/TutorialOutpost.sub");

		// Token: 0x040010F4 RID: 4340
		public readonly string LevelSeed;

		// Token: 0x040010F5 RID: 4341
		public readonly string LevelParams;

		// Token: 0x040010F6 RID: 4342
		private readonly ContentXElement tutorialCharacterElement;

		// Token: 0x040010F7 RID: 4343
		public readonly ImmutableArray<Identifier> StartingItemTags;

		// Token: 0x040010F8 RID: 4344
		public readonly Identifier EventIdentifier;

		// Token: 0x040010F9 RID: 4345
		public readonly Sprite Banner;

		// Token: 0x040010FA RID: 4346
		public readonly TutorialPrefab.EndMessageInfo EndMessage;

		// Token: 0x0200098F RID: 2447
		public enum EndType
		{
			// Token: 0x040033B7 RID: 13239
			None,
			// Token: 0x040033B8 RID: 13240
			Continue,
			// Token: 0x040033B9 RID: 13241
			Restart
		}

		// Token: 0x02000990 RID: 2448
		public readonly struct EndMessageInfo : IEquatable<TutorialPrefab.EndMessageInfo>
		{
			// Token: 0x06005A06 RID: 23046 RVA: 0x001FB38A File Offset: 0x001F958A
			public EndMessageInfo(TutorialPrefab.EndType EndType, Identifier NextTutorialIdentifier)
			{
				this.EndType = EndType;
				this.NextTutorialIdentifier = NextTutorialIdentifier;
			}

			// Token: 0x1700155E RID: 5470
			// (get) Token: 0x06005A07 RID: 23047 RVA: 0x001FB39A File Offset: 0x001F959A
			// (set) Token: 0x06005A08 RID: 23048 RVA: 0x001FB3A2 File Offset: 0x001F95A2
			public TutorialPrefab.EndType EndType { get; set; }

			// Token: 0x1700155F RID: 5471
			// (get) Token: 0x06005A09 RID: 23049 RVA: 0x001FB3AB File Offset: 0x001F95AB
			// (set) Token: 0x06005A0A RID: 23050 RVA: 0x001FB3B3 File Offset: 0x001F95B3
			public Identifier NextTutorialIdentifier { get; set; }

			// Token: 0x06005A0B RID: 23051 RVA: 0x001FB3BC File Offset: 0x001F95BC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("EndMessageInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06005A0C RID: 23052 RVA: 0x001FB408 File Offset: 0x001F9608
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("EndType = ");
				builder.Append(this.EndType.ToString());
				builder.Append(", NextTutorialIdentifier = ");
				builder.Append(this.NextTutorialIdentifier.ToString());
				return true;
			}

			// Token: 0x06005A0D RID: 23053 RVA: 0x001FB464 File Offset: 0x001F9664
			[CompilerGenerated]
			public static bool operator !=(TutorialPrefab.EndMessageInfo left, TutorialPrefab.EndMessageInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06005A0E RID: 23054 RVA: 0x001FB470 File Offset: 0x001F9670
			[CompilerGenerated]
			public static bool operator ==(TutorialPrefab.EndMessageInfo left, TutorialPrefab.EndMessageInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005A0F RID: 23055 RVA: 0x001FB47A File Offset: 0x001F967A
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<TutorialPrefab.EndType>.Default.GetHashCode(this.<EndType>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<NextTutorialIdentifier>k__BackingField);
			}

			// Token: 0x06005A10 RID: 23056 RVA: 0x001FB4A3 File Offset: 0x001F96A3
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is TutorialPrefab.EndMessageInfo && this.Equals((TutorialPrefab.EndMessageInfo)obj);
			}

			// Token: 0x06005A11 RID: 23057 RVA: 0x001FB4BB File Offset: 0x001F96BB
			[CompilerGenerated]
			public bool Equals(TutorialPrefab.EndMessageInfo other)
			{
				return EqualityComparer<TutorialPrefab.EndType>.Default.Equals(this.<EndType>k__BackingField, other.<EndType>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<NextTutorialIdentifier>k__BackingField, other.<NextTutorialIdentifier>k__BackingField);
			}

			// Token: 0x06005A12 RID: 23058 RVA: 0x001FB4ED File Offset: 0x001F96ED
			[CompilerGenerated]
			public void Deconstruct(out TutorialPrefab.EndType EndType, out Identifier NextTutorialIdentifier)
			{
				EndType = this.EndType;
				NextTutorialIdentifier = this.NextTutorialIdentifier;
			}
		}
	}
}
