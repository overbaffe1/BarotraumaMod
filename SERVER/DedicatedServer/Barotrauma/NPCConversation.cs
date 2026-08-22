using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000063 RID: 99
	internal class NPCConversation
	{
		// Token: 0x06000DC7 RID: 3527 RVA: 0x000884BC File Offset: 0x000866BC
		public NPCConversation(XElement element)
		{
			this.Line = element.GetAttributeString("line", "");
			this.speakerIndex = element.GetAttributeInt("speaker", 0);
			this.AllowedJobs = element.GetAttributeIdentifierArray("allowedjobs", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.Flags = element.GetAttributeIdentifierArray("flags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.allowedSpeakerTags = element.GetAttributeIdentifierArray("speakertags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			if (element.Attribute("minintensity") != null)
			{
				this.minIntensity = new float?(element.GetAttributeFloat("minintensity", 0f));
			}
			if (element.Attribute("maxintensity") != null)
			{
				this.maxIntensity = new float?(element.GetAttributeFloat("maxintensity", 1f));
			}
			this.Responses = (from s in element.Elements()
			select new NPCConversation(s)).ToImmutableArray<NPCConversation>();
			this.requireNextLine = element.GetAttributeBool("requirenextline", false);
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x000885EC File Offset: 0x000867EC
		private static List<Identifier> GetCurrentFlags(Character speaker)
		{
			List<Identifier> currentFlags = new List<Identifier>();
			if (Submarine.MainSub != null && Submarine.MainSub.AtCosmeticDamageDepth)
			{
				currentFlags.Add("SubmarineDeep".ToIdentifier());
			}
			if (GameMain.GameSession != null && Level.Loaded != null)
			{
				if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
				{
					if (GameMain.GameSession.RoundDuration < 30f)
					{
						currentFlags.Add("Initial".ToIdentifier());
					}
				}
				else if (Level.Loaded.Type == LevelData.LevelType.Outpost)
				{
					if (GameMain.GameSession.RoundDuration < 120f)
					{
						Character speaker2 = speaker;
						if (((speaker2 != null) ? speaker2.CurrentHull : null) != null)
						{
							Map map = GameMain.GameSession.Map;
							bool flag;
							if (map == null)
							{
								flag = false;
							}
							else
							{
								Location currentLocation = map.CurrentLocation;
								float? num;
								if (currentLocation == null)
								{
									num = null;
								}
								else
								{
									Reputation reputation = currentLocation.Reputation;
									num = ((reputation != null) ? new float?(reputation.Value) : null);
								}
								float? num2 = num;
								float num3 = 0f;
								flag = (num2.GetValueOrDefault() >= num3 & num2 != null);
							}
							if (flag && Character.CharacterList.Any((Character c) => c.TeamID != speaker.TeamID && c.CurrentHull == speaker.CurrentHull))
							{
								currentFlags.Add("EnterOutpost".ToIdentifier());
							}
						}
					}
					if (Level.Loaded.IsEndBiome)
					{
						currentFlags.Add("EndLevel".ToIdentifier());
					}
				}
				if (GameMain.GameSession.EventManager.CurrentIntensity <= 0.2f)
				{
					currentFlags.Add("Casual".ToIdentifier());
				}
				if (GameMain.GameSession.IsCurrentLocationRadiated())
				{
					currentFlags.Add("InRadiation".ToIdentifier());
				}
			}
			if (speaker != null)
			{
				if (speaker.AnimController.InWater)
				{
					currentFlags.Add("Underwater".ToIdentifier());
				}
				currentFlags.Add(((speaker.CurrentHull == null) ? "Outside" : "Inside").ToIdentifier());
				if (Character.Controlled != null && Character.Controlled.CharacterHealth.GetAffliction("psychosis", true) != null)
				{
					currentFlags.Add(((speaker != Character.Controlled) ? "Psychosis" : "PsychosisSelf").ToIdentifier());
				}
				IReadOnlyCollection<Affliction> afflictions = speaker.CharacterHealth.GetAllAfflictions();
				foreach (Affliction affliction in afflictions)
				{
					AfflictionPrefab.Effect currentEffect = affliction.GetActiveEffect();
					if (currentEffect != null && !currentEffect.DialogFlag.IsEmpty && !currentFlags.Contains(currentEffect.DialogFlag))
					{
						currentFlags.Add(currentEffect.DialogFlag);
					}
				}
				if (speaker.TeamID == CharacterTeamType.FriendlyNPC && speaker.Submarine != null && speaker.Submarine.Info.IsOutpost)
				{
					currentFlags.Add("OutpostNPC".ToIdentifier());
					GameSession gameSession = GameMain.GameSession;
					Faction faction2;
					if (gameSession == null)
					{
						faction2 = null;
					}
					else
					{
						Level level = gameSession.Level;
						if (level == null)
						{
							faction2 = null;
						}
						else
						{
							Location startLocation = level.StartLocation;
							faction2 = ((startLocation != null) ? startLocation.Faction : null);
						}
					}
					Faction faction = faction2;
					if (faction != null)
					{
						List<Identifier> list = currentFlags;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler.AppendLiteral("OutpostNPC");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(faction.Prefab.Identifier);
						list.Add(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
					}
				}
				if (speaker.CampaignInteractionType != CampaignMode.InteractionType.None)
				{
					List<Identifier> list2 = currentFlags;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("CampaignNPC.");
					defaultInterpolatedStringHandler2.AppendFormatted<CampaignMode.InteractionType>(speaker.CampaignInteractionType);
					list2.Add(defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier());
				}
				GameSession gameSession2 = GameMain.GameSession;
				CampaignMode campaignMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
				if (campaignMode != null)
				{
					Map map2 = campaignMode.Map;
					Identifier? identifier;
					Identifier? identifier2;
					if (map2 == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						Location currentLocation2 = map2.CurrentLocation;
						if (currentLocation2 == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							LocationType type = currentLocation2.Type;
							if (type == null)
							{
								identifier = null;
								identifier2 = identifier;
							}
							else
							{
								identifier2 = new Identifier?(type.Identifier);
							}
						}
					}
					identifier = identifier2;
					if (identifier == "abandoned")
					{
						if (speaker.TeamID == CharacterTeamType.None)
						{
							currentFlags.Add("Bandit".ToIdentifier());
						}
						else if (speaker.TeamID == CharacterTeamType.FriendlyNPC)
						{
							currentFlags.Add("Hostage".ToIdentifier());
						}
					}
				}
				if (speaker.IsEscorted)
				{
					currentFlags.Add("escort".ToIdentifier());
				}
			}
			return currentFlags;
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x00088A88 File Offset: 0x00086C88
		[return: TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})]
		public static List<ValueTuple<Character, string>> CreateRandom(List<Character> availableSpeakers)
		{
			Dictionary<int, Character> assignedSpeakers = new Dictionary<int, Character>();
			List<ValueTuple<Character, string>> lines = new List<ValueTuple<Character, string>>();
			LanguageIdentifier language = GameSettings.CurrentConfig.Language;
			if (language != TextManager.DefaultLanguage && !NPCConversationCollection.Collections.ContainsKey(language))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find NPC conversations for the language \"");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(language);
				defaultInterpolatedStringHandler.AppendLiteral("\". Using \"");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(TextManager.DefaultLanguage);
				defaultInterpolatedStringHandler.AppendLiteral("\" instead..");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				language = TextManager.DefaultLanguage;
			}
			NPCConversation.CreateConversation(availableSpeakers, assignedSpeakers, null, lines, NPCConversationCollection.Collections[language].SelectMany((NPCConversationCollection cc) => cc.Conversations).ToList<NPCConversation>(), false);
			return lines;
		}

		// Token: 0x06000DCA RID: 3530 RVA: 0x00088B5C File Offset: 0x00086D5C
		[return: TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})]
		public static List<ValueTuple<Character, string>> CreateRandom(List<Character> availableSpeakers, IEnumerable<Identifier> requiredFlags)
		{
			Dictionary<int, Character> assignedSpeakers = new Dictionary<int, Character>();
			List<ValueTuple<Character, string>> lines = new List<ValueTuple<Character, string>>();
			LanguageIdentifier language = GameSettings.CurrentConfig.Language;
			if (language != TextManager.DefaultLanguage && !NPCConversationCollection.Collections.ContainsKey(language))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find NPC conversations for the language \"");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(language);
				defaultInterpolatedStringHandler.AppendLiteral("\". Using \"");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(TextManager.DefaultLanguage);
				defaultInterpolatedStringHandler.AppendLiteral("\" instead..");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				language = TextManager.DefaultLanguage;
			}
			Func<NPCConversation, bool> <>9__1;
			List<NPCConversation> availableConversations = NPCConversationCollection.Collections[language].SelectMany(delegate(NPCConversationCollection cc)
			{
				IEnumerable<NPCConversation> conversations = cc.Conversations;
				Func<NPCConversation, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((NPCConversation c) => requiredFlags.All((Identifier f) => c.Flags.Contains(f))));
				}
				return conversations.Where(predicate);
			}).ToList<NPCConversation>();
			if (availableConversations.Count > 0)
			{
				NPCConversation.CreateConversation(availableSpeakers, assignedSpeakers, null, lines, availableConversations, false);
			}
			return lines;
		}

		// Token: 0x06000DCB RID: 3531 RVA: 0x00088C38 File Offset: 0x00086E38
		private static void CreateConversation(List<Character> availableSpeakers, Dictionary<int, Character> assignedSpeakers, NPCConversation baseConversation, [TupleElementNames(new string[]
		{
			"speaker",
			"line"
		})] IList<ValueTuple<Character, string>> lineList, IList<NPCConversation> availableConversations, bool ignoreFlags = false)
		{
			IList<NPCConversation> list2;
			if (baseConversation != null)
			{
				IList<NPCConversation> list = baseConversation.Responses;
				list2 = list;
			}
			else
			{
				list2 = availableConversations;
			}
			IList<NPCConversation> conversations = list2;
			if (conversations.Count == 0)
			{
				return;
			}
			int conversationIndex = Rand.Int(conversations.Count, Rand.RandSync.Unsynced);
			NPCConversation selectedConversation = conversations[conversationIndex];
			if (string.IsNullOrEmpty(selectedConversation.Line))
			{
				return;
			}
			Character speaker = null;
			if (assignedSpeakers.ContainsKey(selectedConversation.speakerIndex))
			{
				List<Identifier> characterFlags = NPCConversation.GetCurrentFlags(assignedSpeakers[selectedConversation.speakerIndex]);
				if (selectedConversation.Flags.All((Identifier flag) => characterFlags.Contains(flag)))
				{
					speaker = assignedSpeakers[selectedConversation.speakerIndex];
				}
			}
			if (speaker == null)
			{
				List<Character> allowedSpeakers = new List<Character>();
				List<NPCConversation> potentialLines = new List<NPCConversation>(conversations);
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null)
				{
					potentialLines.RemoveAll(delegate(NPCConversation l)
					{
						if (l.minIntensity != null)
						{
							float currentIntensity = GameMain.GameSession.EventManager.CurrentIntensity;
							float? num = l.minIntensity;
							if (currentIntensity < num.GetValueOrDefault() & num != null)
							{
								return true;
							}
						}
						if (l.maxIntensity != null)
						{
							float currentIntensity2 = GameMain.GameSession.EventManager.CurrentIntensity;
							float? num = l.maxIntensity;
							return currentIntensity2 > num.GetValueOrDefault() & num != null;
						}
						return false;
					});
				}
				while (potentialLines.Count > 0)
				{
					selectedConversation = NPCConversation.GetRandomConversation(potentialLines, baseConversation == null);
					if (selectedConversation == null || string.IsNullOrEmpty(selectedConversation.Line))
					{
						return;
					}
					if (assignedSpeakers.ContainsKey(selectedConversation.speakerIndex))
					{
						speaker = assignedSpeakers[selectedConversation.speakerIndex];
						break;
					}
					foreach (Character potentialSpeaker in availableSpeakers)
					{
						if (NPCConversation.CheckSpeakerViability(potentialSpeaker, selectedConversation, assignedSpeakers.Values.ToList<Character>(), ignoreFlags))
						{
							allowedSpeakers.Add(potentialSpeaker);
						}
					}
					if (allowedSpeakers.Count != 0 && !NPCConversation.NextLineFailure(selectedConversation, availableSpeakers, allowedSpeakers, ignoreFlags))
					{
						break;
					}
					allowedSpeakers.Clear();
					potentialLines.Remove(selectedConversation);
				}
				if (allowedSpeakers.Count == 0)
				{
					return;
				}
				speaker = allowedSpeakers[Rand.Int(allowedSpeakers.Count, Rand.RandSync.Unsynced)];
				availableSpeakers.Remove(speaker);
				assignedSpeakers.Add(selectedConversation.speakerIndex, speaker);
			}
			if (baseConversation == null)
			{
				NPCConversation.previousConversations.Insert(0, selectedConversation);
				if (NPCConversation.previousConversations.Count > 20)
				{
					NPCConversation.previousConversations.RemoveAt(20);
				}
			}
			lineList.Add(new ValueTuple<Character, string>(speaker, selectedConversation.Line));
			NPCConversation.CreateConversation(availableSpeakers, assignedSpeakers, selectedConversation, lineList, availableConversations, false);
		}

		// Token: 0x06000DCC RID: 3532 RVA: 0x00088E7C File Offset: 0x0008707C
		private static bool NextLineFailure(NPCConversation selectedConversation, List<Character> availableSpeakers, List<Character> allowedSpeakers, bool ignoreFlags)
		{
			if (selectedConversation.requireNextLine)
			{
				foreach (NPCConversation nextConversation in selectedConversation.Responses)
				{
					foreach (Character potentialNextSpeaker in availableSpeakers)
					{
						if (NPCConversation.CheckSpeakerViability(potentialNextSpeaker, nextConversation, allowedSpeakers, ignoreFlags))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000DCD RID: 3533 RVA: 0x00088F00 File Offset: 0x00087100
		private static bool CheckSpeakerViability(Character potentialSpeaker, NPCConversation selectedConversation, List<Character> checkedSpeakers, bool ignoreFlags)
		{
			CharacterInfo info = potentialSpeaker.Info;
			if ((((info != null) ? info.Job : null) != null && potentialSpeaker.Info.Job.Prefab.OnlyJobSpecificDialog) || selectedConversation.AllowedJobs.Count > 0)
			{
				CharacterInfo info2 = potentialSpeaker.Info;
				JobPrefab jobPrefab;
				if (info2 == null)
				{
					jobPrefab = null;
				}
				else
				{
					Job job = info2.Job;
					jobPrefab = ((job != null) ? job.Prefab : null);
				}
				JobPrefab speakerJobPrefab = jobPrefab;
				if (speakerJobPrefab == null || !selectedConversation.AllowedJobs.Contains(speakerJobPrefab.Identifier))
				{
					return false;
				}
			}
			if (!ignoreFlags)
			{
				List<Identifier> characterFlags = NPCConversation.GetCurrentFlags(potentialSpeaker);
				if (!selectedConversation.Flags.All((Identifier flag) => characterFlags.Contains(flag)))
				{
					return false;
				}
			}
			if (checkedSpeakers.Any((Character s) => !potentialSpeaker.CanHearCharacter(s)))
			{
				return false;
			}
			if (checkedSpeakers.Any((Character s) => !potentialSpeaker.CanSeeTarget(s, null, false, false)))
			{
				return false;
			}
			if (selectedConversation.allowedSpeakerTags.Count > 0)
			{
				CharacterInfo info3 = potentialSpeaker.Info;
				if (((info3 != null) ? info3.PersonalityTrait : null) == null)
				{
					return false;
				}
				if (!selectedConversation.allowedSpeakerTags.Any((Identifier t) => potentialSpeaker.Info.PersonalityTrait.AllowedDialogTags.Any((string t2) => t2 == t)))
				{
					return false;
				}
			}
			else
			{
				CharacterInfo info4 = potentialSpeaker.Info;
				if (((info4 != null) ? info4.PersonalityTrait : null) != null && !potentialSpeaker.Info.PersonalityTrait.AllowedDialogTags.Contains("none"))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000DCE RID: 3534 RVA: 0x0008907C File Offset: 0x0008727C
		private static NPCConversation GetRandomConversation(List<NPCConversation> conversations, bool avoidPreviouslyUsed)
		{
			if (avoidPreviouslyUsed)
			{
				List<float> probabilities = new List<float>();
				foreach (NPCConversation conversation in conversations)
				{
					probabilities.Add(NPCConversation.GetConversationProbability(conversation));
				}
				return ToolBox.SelectWeightedRandom<NPCConversation>(conversations, probabilities, Rand.RandSync.Unsynced);
			}
			if (conversations.Count != 0)
			{
				return conversations[Rand.Int(conversations.Count, Rand.RandSync.Unsynced)];
			}
			return null;
		}

		// Token: 0x06000DCF RID: 3535 RVA: 0x00089100 File Offset: 0x00087300
		private static float GetConversationProbability(NPCConversation conversation)
		{
			float baseProbability = MathF.Pow((float)(conversation.Flags.Count + 1), 2f);
			int index = NPCConversation.previousConversations.IndexOf(conversation);
			if (index < 0)
			{
				return baseProbability * 10f;
			}
			return baseProbability + 1f - 1f / (float)(index + 1);
		}

		// Token: 0x0400067B RID: 1659
		private const int MaxPreviousConversations = 20;

		// Token: 0x0400067C RID: 1660
		public readonly string Line;

		// Token: 0x0400067D RID: 1661
		public readonly ImmutableHashSet<Identifier> AllowedJobs;

		// Token: 0x0400067E RID: 1662
		public readonly ImmutableHashSet<Identifier> Flags;

		// Token: 0x0400067F RID: 1663
		public readonly float? maxIntensity;

		// Token: 0x04000680 RID: 1664
		public readonly float? minIntensity;

		// Token: 0x04000681 RID: 1665
		public readonly ImmutableArray<NPCConversation> Responses;

		// Token: 0x04000682 RID: 1666
		private readonly int speakerIndex;

		// Token: 0x04000683 RID: 1667
		private readonly ImmutableHashSet<Identifier> allowedSpeakerTags;

		// Token: 0x04000684 RID: 1668
		private readonly bool requireNextLine;

		// Token: 0x04000685 RID: 1669
		private static readonly List<NPCConversation> previousConversations = new List<NPCConversation>();
	}
}
