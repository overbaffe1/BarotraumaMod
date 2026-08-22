using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020002D3 RID: 723
	internal class TutorialPrefab : Prefab
	{
		// Token: 0x06003CF6 RID: 15606 RVA: 0x0022C9AC File Offset: 0x0022ABAC
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

		// Token: 0x06003CF7 RID: 15607 RVA: 0x0022CB7C File Offset: 0x0022AD7C
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

		// Token: 0x06003CF8 RID: 15608 RVA: 0x0022CCA0 File Offset: 0x0022AEA0
		public override void Dispose()
		{
		}

		// Token: 0x04001F90 RID: 8080
		public static readonly PrefabCollection<TutorialPrefab> Prefabs = new PrefabCollection<TutorialPrefab>(new Action(MainMenuScreen.UpdateInstanceTutorialButtons));

		// Token: 0x04001F91 RID: 8081
		public readonly int Order;

		// Token: 0x04001F92 RID: 8082
		public readonly bool DisableBotConversations;

		// Token: 0x04001F93 RID: 8083
		public readonly bool AllowCharacterSwitch;

		// Token: 0x04001F94 RID: 8084
		public readonly ContentPath SubmarinePath = ContentPath.FromRaw("Content/Tutorials/Dugong_Tutorial.sub");

		// Token: 0x04001F95 RID: 8085
		public readonly ContentPath OutpostPath = ContentPath.FromRaw("Content/Tutorials/TutorialOutpost.sub");

		// Token: 0x04001F96 RID: 8086
		public readonly string LevelSeed;

		// Token: 0x04001F97 RID: 8087
		public readonly string LevelParams;

		// Token: 0x04001F98 RID: 8088
		private readonly ContentXElement tutorialCharacterElement;

		// Token: 0x04001F99 RID: 8089
		public readonly ImmutableArray<Identifier> StartingItemTags;

		// Token: 0x04001F9A RID: 8090
		public readonly Identifier EventIdentifier;

		// Token: 0x04001F9B RID: 8091
		public readonly Sprite Banner;

		// Token: 0x04001F9C RID: 8092
		public readonly TutorialPrefab.EndMessageInfo EndMessage;

		// Token: 0x02000F7E RID: 3966
		public enum EndType
		{
			// Token: 0x040055CB RID: 21963
			None,
			// Token: 0x040055CC RID: 21964
			Continue,
			// Token: 0x040055CD RID: 21965
			Restart
		}

		// Token: 0x02000F7F RID: 3967
		public readonly struct EndMessageInfo : IEquatable<TutorialPrefab.EndMessageInfo>
		{
			// Token: 0x0600893E RID: 35134 RVA: 0x003A7AAA File Offset: 0x003A5CAA
			public EndMessageInfo(TutorialPrefab.EndType EndType, Identifier NextTutorialIdentifier)
			{
				this.EndType = EndType;
				this.NextTutorialIdentifier = NextTutorialIdentifier;
			}

			// Token: 0x17001C26 RID: 7206
			// (get) Token: 0x0600893F RID: 35135 RVA: 0x003A7ABA File Offset: 0x003A5CBA
			// (set) Token: 0x06008940 RID: 35136 RVA: 0x003A7AC2 File Offset: 0x003A5CC2
			public TutorialPrefab.EndType EndType { get; set; }

			// Token: 0x17001C27 RID: 7207
			// (get) Token: 0x06008941 RID: 35137 RVA: 0x003A7ACB File Offset: 0x003A5CCB
			// (set) Token: 0x06008942 RID: 35138 RVA: 0x003A7AD3 File Offset: 0x003A5CD3
			public Identifier NextTutorialIdentifier { get; set; }

			// Token: 0x06008943 RID: 35139 RVA: 0x003A7ADC File Offset: 0x003A5CDC
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

			// Token: 0x06008944 RID: 35140 RVA: 0x003A7B28 File Offset: 0x003A5D28
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("EndType = ");
				builder.Append(this.EndType.ToString());
				builder.Append(", NextTutorialIdentifier = ");
				builder.Append(this.NextTutorialIdentifier.ToString());
				return true;
			}

			// Token: 0x06008945 RID: 35141 RVA: 0x003A7B84 File Offset: 0x003A5D84
			[CompilerGenerated]
			public static bool operator !=(TutorialPrefab.EndMessageInfo left, TutorialPrefab.EndMessageInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06008946 RID: 35142 RVA: 0x003A7B90 File Offset: 0x003A5D90
			[CompilerGenerated]
			public static bool operator ==(TutorialPrefab.EndMessageInfo left, TutorialPrefab.EndMessageInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008947 RID: 35143 RVA: 0x003A7B9A File Offset: 0x003A5D9A
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<TutorialPrefab.EndType>.Default.GetHashCode(this.<EndType>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<NextTutorialIdentifier>k__BackingField);
			}

			// Token: 0x06008948 RID: 35144 RVA: 0x003A7BC3 File Offset: 0x003A5DC3
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is TutorialPrefab.EndMessageInfo && this.Equals((TutorialPrefab.EndMessageInfo)obj);
			}

			// Token: 0x06008949 RID: 35145 RVA: 0x003A7BDB File Offset: 0x003A5DDB
			[CompilerGenerated]
			public bool Equals(TutorialPrefab.EndMessageInfo other)
			{
				return EqualityComparer<TutorialPrefab.EndType>.Default.Equals(this.<EndType>k__BackingField, other.<EndType>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<NextTutorialIdentifier>k__BackingField, other.<NextTutorialIdentifier>k__BackingField);
			}

			// Token: 0x0600894A RID: 35146 RVA: 0x003A7C0D File Offset: 0x003A5E0D
			[CompilerGenerated]
			public void Deconstruct(out TutorialPrefab.EndType EndType, out Identifier NextTutorialIdentifier)
			{
				EndType = this.EndType;
				NextTutorialIdentifier = this.NextTutorialIdentifier;
			}
		}
	}
}
