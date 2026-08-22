using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002A8 RID: 680
	internal class TagAction : EventAction
	{
		// Token: 0x17000F96 RID: 3990
		// (get) Token: 0x06003B25 RID: 15141 RVA: 0x00222330 File Offset: 0x00220530
		// (set) Token: 0x06003B26 RID: 15142 RVA: 0x00222338 File Offset: 0x00220538
		[Serialize("", IsPropertySaveable.Yes, "What criteria to use to select the entities to target. Valid values are players, player, traitor, nontraitor, nontraitorplayer, bot, crew, humanprefabidentifier:[id], jobidentifier:[id], structureidentifier:[id], structurespecialtag:[tag], itemidentifier:[id], itemtag:[tag], hull, hullname:[name], submarine:[type], eventtag:[tag], speciesname:[id].", "", false)]
		public string Criteria { get; set; }

		// Token: 0x17000F97 RID: 3991
		// (get) Token: 0x06003B27 RID: 15143 RVA: 0x00222341 File Offset: 0x00220541
		// (set) Token: 0x06003B28 RID: 15144 RVA: 0x00222349 File Offset: 0x00220549
		[Serialize("", IsPropertySaveable.Yes, "The tag to apply to the target.", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x17000F98 RID: 3992
		// (get) Token: 0x06003B29 RID: 15145 RVA: 0x00222352 File Offset: 0x00220552
		// (set) Token: 0x06003B2A RID: 15146 RVA: 0x0022235A File Offset: 0x0022055A
		[Serialize(TagAction.SubType.Any, IsPropertySaveable.Yes, "The type of submarine the target needs to be in.", "", false)]
		public TagAction.SubType SubmarineType { get; set; }

		// Token: 0x17000F99 RID: 3993
		// (get) Token: 0x06003B2B RID: 15147 RVA: 0x00222363 File Offset: 0x00220563
		// (set) Token: 0x06003B2C RID: 15148 RVA: 0x0022236B File Offset: 0x0022056B
		[Serialize(TagAction.CharacterTeam.Any, IsPropertySaveable.Yes, "The team the target needs to be on.", "", false)]
		public TagAction.CharacterTeam Team { get; set; }

		// Token: 0x17000F9A RID: 3994
		// (get) Token: 0x06003B2D RID: 15149 RVA: 0x00222374 File Offset: 0x00220574
		// (set) Token: 0x06003B2E RID: 15150 RVA: 0x0022237C File Offset: 0x0022057C
		[Serialize("", IsPropertySaveable.Yes, "If set, the target must be in an outpost module that has this tag.", "", false)]
		public Identifier RequiredModuleTag { get; set; }

		// Token: 0x17000F9B RID: 3995
		// (get) Token: 0x06003B2F RID: 15151 RVA: 0x00222385 File Offset: 0x00220585
		// (set) Token: 0x06003B30 RID: 15152 RVA: 0x0022238D File Offset: 0x0022058D
		[Serialize(true, IsPropertySaveable.Yes, "Should incapacitated (e.g. dead, paralyzed, unconscious) characters be ignored, i.e. not considered valid targets?", "", false)]
		public bool IgnoreIncapacitatedCharacters { get; set; }

		// Token: 0x17000F9C RID: 3996
		// (get) Token: 0x06003B31 RID: 15153 RVA: 0x00222396 File Offset: 0x00220596
		// (set) Token: 0x06003B32 RID: 15154 RVA: 0x0022239E File Offset: 0x0022059E
		[Serialize(false, IsPropertySaveable.Yes, "Can items that have been set to be hidden in-game be tagged?", "", false)]
		public bool AllowHiddenItems { get; set; }

		// Token: 0x17000F9D RID: 3997
		// (get) Token: 0x06003B33 RID: 15155 RVA: 0x002223A7 File Offset: 0x002205A7
		// (set) Token: 0x06003B34 RID: 15156 RVA: 0x002223AF File Offset: 0x002205AF
		[Serialize(false, IsPropertySaveable.Yes, "If there are multiple matching targets, should all of them be tagged or one chosen randomly?", "", false)]
		public bool ChooseRandom { get; set; }

		// Token: 0x17000F9E RID: 3998
		// (get) Token: 0x06003B35 RID: 15157 RVA: 0x002223B8 File Offset: 0x002205B8
		// (set) Token: 0x06003B36 RID: 15158 RVA: 0x002223C0 File Offset: 0x002205C0
		[Serialize("", IsPropertySaveable.Yes, "If choosing a random target, targets with this tag can optionally be excluded.", "", false)]
		public Identifier ChooseRandomExcludingTag { get; set; }

		// Token: 0x17000F9F RID: 3999
		// (get) Token: 0x06003B37 RID: 15159 RVA: 0x002223C9 File Offset: 0x002205C9
		// (set) Token: 0x06003B38 RID: 15160 RVA: 0x002223D1 File Offset: 0x002205D1
		[Serialize(false, IsPropertySaveable.Yes, "Should the event continue if the TagAction can't find any valid targets?", "", false)]
		public bool ContinueIfNoTargetsFound { get; set; }

		// Token: 0x17000FA0 RID: 4000
		// (get) Token: 0x06003B39 RID: 15161 RVA: 0x002223DA File Offset: 0x002205DA
		// (set) Token: 0x06003B3A RID: 15162 RVA: 0x002223E2 File Offset: 0x002205E2
		[Serialize(0f, IsPropertySaveable.Yes, "If larger than 0, the specified percentage of the matching targets are tagged. Between 0-100.", "", false)]
		public float ChoosePercentage { get; set; }

		// Token: 0x06003B3B RID: 15163 RVA: 0x002223EC File Offset: 0x002205EC
		public TagAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.Taggers = (from t in new ValueTuple<string, Action<Identifier>>[]
			{
				new ValueTuple<string, Action<Identifier>>("players", delegate(Identifier v)
				{
					this.TagPlayers();
				}),
				new ValueTuple<string, Action<Identifier>>("player", delegate(Identifier v)
				{
					this.TagPlayers();
				}),
				new ValueTuple<string, Action<Identifier>>("traitor", delegate(Identifier v)
				{
					this.TagTraitors();
				}),
				new ValueTuple<string, Action<Identifier>>("nontraitor", delegate(Identifier v)
				{
					this.TagNonTraitors();
				}),
				new ValueTuple<string, Action<Identifier>>("nontraitorplayer", delegate(Identifier v)
				{
					this.TagNonTraitorPlayers();
				}),
				new ValueTuple<string, Action<Identifier>>("bot", delegate(Identifier v)
				{
					this.TagBots(false);
				}),
				new ValueTuple<string, Action<Identifier>>("crew", delegate(Identifier v)
				{
					this.TagCrew();
				}),
				new ValueTuple<string, Action<Identifier>>("humanprefabidentifier", new Action<Identifier>(this.TagHumansByIdentifier)),
				new ValueTuple<string, Action<Identifier>>("humanprefabtag", new Action<Identifier>(this.TagHumansByTag)),
				new ValueTuple<string, Action<Identifier>>("jobidentifier", new Action<Identifier>(this.TagHumansByJobIdentifier)),
				new ValueTuple<string, Action<Identifier>>("structureidentifier", new Action<Identifier>(this.TagStructuresByIdentifier)),
				new ValueTuple<string, Action<Identifier>>("structurespecialtag", new Action<Identifier>(this.TagStructuresBySpecialTag)),
				new ValueTuple<string, Action<Identifier>>("itemidentifier", new Action<Identifier>(this.TagItemsByIdentifier)),
				new ValueTuple<string, Action<Identifier>>("itemtag", new Action<Identifier>(this.TagItemsByTag)),
				new ValueTuple<string, Action<Identifier>>("hull", delegate(Identifier v)
				{
					this.TagHulls();
				}),
				new ValueTuple<string, Action<Identifier>>("hullname", new Action<Identifier>(this.TagHullsByName)),
				new ValueTuple<string, Action<Identifier>>("submarine", new Action<Identifier>(this.TagSubmarinesByType)),
				new ValueTuple<string, Action<Identifier>>("eventtag", new Action<Identifier>(this.TagByEventTag)),
				new ValueTuple<string, Action<Identifier>>("speciesname", new Action<Identifier>(this.TagBySpeciesName))
			}
			select new ValueTuple<Identifier, Action<Identifier>>(t.Item1.ToIdentifier(), t.Item2)).ToImmutableDictionary<Identifier, Action<Identifier>>();
		}

		// Token: 0x06003B3C RID: 15164 RVA: 0x00222668 File Offset: 0x00220868
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003B3D RID: 15165 RVA: 0x00222670 File Offset: 0x00220870
		public override void Reset()
		{
			this.taggingDone = false;
			this.cantFindTargets = false;
			this.isFinished = false;
		}

		// Token: 0x06003B3E RID: 15166 RVA: 0x00222688 File Offset: 0x00220888
		private void TagBySpeciesName(Identifier speciesName)
		{
			this.AddTarget(this.Tag, Character.CharacterList.Where(delegate(Character c)
			{
				Identifier speciesName2 = c.SpeciesName;
				return speciesName2 == speciesName && this.CharacterTeamMatches(c);
			}));
		}

		// Token: 0x06003B3F RID: 15167 RVA: 0x002226CB File Offset: 0x002208CB
		private void TagByEventTag(Identifier eventTag)
		{
			this.AddTarget(this.Tag, from t in this.ParentEvent.GetTargets(eventTag)
			where this.MatchesRequirements(t)
			select t);
		}

		// Token: 0x06003B40 RID: 15168 RVA: 0x002226F6 File Offset: 0x002208F6
		private void TagPlayers()
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsPlayer && (!c.IsIncapacitated || !this.IgnoreIncapacitatedCharacters) && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06003B41 RID: 15169 RVA: 0x00222711 File Offset: 0x00220911
		private void TagTraitors()
		{
			this.AddTargetPredicate(Tags.Traitor, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && (c.IsPlayer || c.IsBot) && c.IsTraitor && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06003B42 RID: 15170 RVA: 0x0022272B File Offset: 0x0022092B
		private void TagNonTraitors()
		{
			this.AddTargetPredicate(Tags.NonTraitor, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && (c.IsPlayer || c.IsBot) && !c.IsTraitor && c.IsOnPlayerTeam && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06003B43 RID: 15171 RVA: 0x00222745 File Offset: 0x00220945
		private void TagNonTraitorPlayers()
		{
			this.AddTargetPredicate(Tags.NonTraitorPlayer, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsPlayer && !c.IsTraitor && c.IsOnPlayerTeam && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06003B44 RID: 15172 RVA: 0x00222760 File Offset: 0x00220960
		private void TagBots(bool playerCrewOnly)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsBot && (!c.IsIncapacitated || !this.IgnoreIncapacitatedCharacters) && (!playerCrewOnly || c.TeamID == CharacterTeamType.Team1) && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06003B45 RID: 15173 RVA: 0x0022279A File Offset: 0x0022099A
		private void TagCrew()
		{
			this.AddTarget(this.Tag, GameMain.GameSession.CrewManager.GetCharacters());
		}

		// Token: 0x06003B46 RID: 15174 RVA: 0x002227B8 File Offset: 0x002209B8
		private void TagHumansByIdentifier(Identifier identifier)
		{
			this.AddTarget(this.Tag, Character.CharacterList.Where(delegate(Character c)
			{
				HumanPrefab humanPrefab = c.HumanPrefab;
				Identifier? identifier2;
				Identifier? identifier3;
				if (humanPrefab == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(humanPrefab.Identifier);
				}
				identifier2 = identifier3;
				Identifier? identifier4 = new Identifier?(identifier);
				return identifier2 == identifier4 && this.CharacterTeamMatches(c);
			}));
		}

		// Token: 0x06003B47 RID: 15175 RVA: 0x002227FC File Offset: 0x002209FC
		private void TagHumansByTag(Identifier tag)
		{
			this.AddTarget(this.Tag, from c in Character.CharacterList
			where c.HumanPrefab != null && c.HumanPrefab.GetTags().Contains(tag) && this.CharacterTeamMatches(c)
			select c);
		}

		// Token: 0x06003B48 RID: 15176 RVA: 0x00222840 File Offset: 0x00220A40
		private void TagHumansByJobIdentifier(Identifier jobIdentifier)
		{
			this.AddTarget(this.Tag, from c in Character.CharacterList
			where c.HasJob(jobIdentifier) && this.CharacterTeamMatches(c)
			select c);
		}

		// Token: 0x06003B49 RID: 15177 RVA: 0x00222884 File Offset: 0x00220A84
		private void TagStructuresByIdentifier(Identifier identifier)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Structure, delegate(Entity e)
			{
				Structure s = e as Structure;
				return s != null && this.MatchesRequirements(s) && s.Prefab.Identifier == identifier;
			});
		}

		// Token: 0x06003B4A RID: 15178 RVA: 0x002228C0 File Offset: 0x00220AC0
		private void TagStructuresBySpecialTag(Identifier tag)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Structure, delegate(Entity e)
			{
				Structure s = e as Structure;
				if (s != null && this.MatchesRequirements(s))
				{
					Identifier identifier = s.SpecialTag.ToIdentifier();
					return identifier == tag;
				}
				return false;
			});
		}

		// Token: 0x06003B4B RID: 15179 RVA: 0x002228FC File Offset: 0x00220AFC
		private void TagItemsByIdentifier(Identifier identifier)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Item, delegate(Entity e)
			{
				Item it = e as Item;
				return it != null && it.Prefab.Identifier == identifier && this.IsValidItem(it);
			});
		}

		// Token: 0x06003B4C RID: 15180 RVA: 0x00222938 File Offset: 0x00220B38
		private void TagItemsByTag(Identifier tag)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Item, delegate(Entity e)
			{
				Item it = e as Item;
				return it != null && it.HasTag(tag) && this.IsValidItem(it);
			});
		}

		// Token: 0x06003B4D RID: 15181 RVA: 0x00222972 File Offset: 0x00220B72
		private void TagHulls()
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Hull, delegate(Entity e)
			{
				Hull h = e as Hull;
				return h != null && this.MatchesRequirements(h);
			});
		}

		// Token: 0x06003B4E RID: 15182 RVA: 0x00222990 File Offset: 0x00220B90
		private void TagHullsByName(Identifier name)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Hull, delegate(Entity e)
			{
				Hull h = e as Hull;
				return h != null && this.MatchesRequirements(h) && h.RoomName.Contains(name.Value, StringComparison.OrdinalIgnoreCase);
			});
		}

		// Token: 0x06003B4F RID: 15183 RVA: 0x002229CC File Offset: 0x00220BCC
		private void TagSubmarinesByType(Identifier type)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Submarine, delegate(Entity e)
			{
				Submarine s = e as Submarine;
				if (s == null || !this.MatchesRequirements(s))
				{
					return false;
				}
				if (!type.IsEmpty)
				{
					Identifier? identifier = new Identifier?(type);
					SubmarineInfo info = s.Info;
					Identifier? identifier2;
					Identifier? identifier3;
					if (info == null)
					{
						identifier2 = null;
						identifier3 = identifier2;
					}
					else
					{
						identifier3 = new Identifier?(info.Type.ToIdentifier<SubmarineType>());
					}
					identifier2 = identifier3;
					return identifier == identifier2;
				}
				return true;
			});
		}

		// Token: 0x06003B50 RID: 15184 RVA: 0x00222A08 File Offset: 0x00220C08
		private bool IsValidItem(Item it)
		{
			if (!it.IsLayerHidden && (!it.HiddenInGame || this.AllowHiddenItems) && this.ModuleTagMatches(it))
			{
				Submarine sub;
				if ((sub = it.Submarine) == null)
				{
					Hull currentHull = it.CurrentHull;
					if ((sub = ((currentHull != null) ? currentHull.Submarine : null)) == null)
					{
						Inventory parentInventory = it.ParentInventory;
						if (parentInventory == null)
						{
							sub = null;
						}
						else
						{
							Entity owner = parentInventory.Owner;
							sub = ((owner != null) ? owner.Submarine : null);
						}
					}
				}
				return this.SubmarineTypeMatches(sub);
			}
			return false;
		}

		// Token: 0x06003B51 RID: 15185 RVA: 0x00222A7C File Offset: 0x00220C7C
		private bool MatchesRequirements(Entity e)
		{
			return this.ModuleTagMatches(e) && this.SubmarineTypeMatches((e as Submarine) ?? e.Submarine);
		}

		// Token: 0x06003B52 RID: 15186 RVA: 0x00222AA0 File Offset: 0x00220CA0
		private bool ModuleTagMatches(Entity e)
		{
			if (this.RequiredModuleTag.IsEmpty)
			{
				return true;
			}
			if (((e != null) ? e.Submarine : null) == null)
			{
				return false;
			}
			Character character = e as Character;
			Hull hull;
			if (character != null)
			{
				hull = character.CurrentHull;
			}
			else
			{
				Item item = e as Item;
				if (item != null)
				{
					hull = item.CurrentHull;
				}
				else
				{
					WayPoint wp = e as WayPoint;
					if (wp != null)
					{
						hull = wp.CurrentHull;
					}
					else
					{
						Hull h = e as Hull;
						if (h == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Potential error in event \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\": ");
							defaultInterpolatedStringHandler.AppendFormatted("TagAction");
							defaultInterpolatedStringHandler.AppendLiteral(" cannot check the module tags of an entity of the type ");
							defaultInterpolatedStringHandler.AppendFormatted<Type>(e.GetType());
							defaultInterpolatedStringHandler.AppendLiteral(".");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							return false;
						}
						hull = h;
					}
				}
			}
			return hull != null && hull.OutpostModuleTags.Contains(this.RequiredModuleTag);
		}

		// Token: 0x06003B53 RID: 15187 RVA: 0x00222BB4 File Offset: 0x00220DB4
		private bool CharacterTeamMatches(Character character)
		{
			if (this.Team == TagAction.CharacterTeam.Any)
			{
				return true;
			}
			TagAction.CharacterTeam team = this.Team;
			switch (team)
			{
			case TagAction.CharacterTeam.None:
				return character.TeamID == CharacterTeamType.None;
			case TagAction.CharacterTeam.Team1:
				return character.TeamID == CharacterTeamType.Team1;
			case (TagAction.CharacterTeam)3:
				break;
			case TagAction.CharacterTeam.Team2:
				return character.TeamID == CharacterTeamType.Team2;
			default:
				if (team == TagAction.CharacterTeam.FriendlyNPC)
				{
					return character.TeamID == CharacterTeamType.FriendlyNPC;
				}
				break;
			}
			return false;
		}

		// Token: 0x06003B54 RID: 15188 RVA: 0x00222C19 File Offset: 0x00220E19
		private bool SubmarineTypeMatches(Submarine sub)
		{
			return TagAction.SubmarineTypeMatches(sub, this.SubmarineType);
		}

		// Token: 0x06003B55 RID: 15189 RVA: 0x00222C28 File Offset: 0x00220E28
		public static bool SubmarineTypeMatches(Submarine sub, TagAction.SubType submarineType)
		{
			if (submarineType == TagAction.SubType.Any)
			{
				return true;
			}
			if (sub == null)
			{
				return false;
			}
			switch (sub.Info.Type)
			{
			case Barotrauma.SubmarineType.Player:
				return submarineType.HasFlag(TagAction.SubType.Player) && !sub.IsRespawnShuttle;
			case Barotrauma.SubmarineType.Outpost:
			case Barotrauma.SubmarineType.OutpostModule:
				return submarineType.HasFlag(TagAction.SubType.Outpost);
			case Barotrauma.SubmarineType.Wreck:
				return submarineType.HasFlag(TagAction.SubType.Wreck);
			case Barotrauma.SubmarineType.BeaconStation:
				return submarineType.HasFlag(TagAction.SubType.BeaconStation);
			case Barotrauma.SubmarineType.EnemySubmarine:
				return submarineType.HasFlag(TagAction.SubType.Enemy);
			case Barotrauma.SubmarineType.Ruin:
				return submarineType.HasFlag(TagAction.SubType.Ruin);
			default:
				return false;
			}
		}

		// Token: 0x06003B56 RID: 15190 RVA: 0x00222CEC File Offset: 0x00220EEC
		private void AddTargetPredicate(Identifier tag, ScriptedEvent.TargetPredicate.EntityType entityType, Predicate<Entity> predicate)
		{
			if (this.ChoosePercentage > 0f)
			{
				this.TagPercentage(tag, from e in Entity.GetEntities()
				where predicate(e)
				select e);
				return;
			}
			if (this.ChooseRandom)
			{
				this.TagRandom(tag, from e in Entity.GetEntities()
				where predicate(e)
				select e);
				return;
			}
			this.ParentEvent.AddTargetPredicate(tag, entityType, predicate);
			this.mustRecheckTargets = true;
		}

		// Token: 0x06003B57 RID: 15191 RVA: 0x00222D74 File Offset: 0x00220F74
		private void AddTarget(Identifier tag, IEnumerable<Entity> entities)
		{
			if (entities.None(null))
			{
				this.cantFindTargets = true;
				return;
			}
			if (this.ChoosePercentage > 0f)
			{
				this.TagPercentage(tag, entities);
				return;
			}
			if (this.ChooseRandom)
			{
				this.TagRandom(tag, entities);
				return;
			}
			foreach (Entity entity in entities)
			{
				this.ParentEvent.AddTarget(tag, entity);
			}
		}

		// Token: 0x06003B58 RID: 15192 RVA: 0x00222DFC File Offset: 0x00220FFC
		private void TagPercentage(Identifier tag, IEnumerable<Entity> entities)
		{
			if (entities.None(null))
			{
				this.cantFindTargets = true;
				return;
			}
			int amountToChoose = (int)Math.Ceiling((double)((float)entities.Count<Entity>() * (this.ChoosePercentage / 100f)));
			if (this.tempEntities == null)
			{
				this.tempEntities = new List<Entity>();
			}
			this.tempEntities.Clear();
			for (int i = 0; i < amountToChoose; i++)
			{
				Entity entity = entities.GetRandomUnsynced<Entity>();
				this.tempEntities.Remove(entity);
				this.ParentEvent.AddTarget(tag, entity);
			}
		}

		// Token: 0x06003B59 RID: 15193 RVA: 0x00222E84 File Offset: 0x00221084
		private void TagRandom(Identifier tag, IEnumerable<Entity> entities)
		{
			if (!this.ChooseRandomExcludingTag.IsEmpty)
			{
				IEnumerable<Entity> excludedTargets = this.ParentEvent.GetTargets(this.ChooseRandomExcludingTag);
				entities = entities.Except(excludedTargets);
			}
			if (entities.None(null))
			{
				this.cantFindTargets = true;
				return;
			}
			this.ParentEvent.AddTarget(tag, entities.GetRandomUnsynced<Entity>());
		}

		// Token: 0x06003B5A RID: 15194 RVA: 0x00222EE0 File Offset: 0x002210E0
		public override void Update(float deltaTime)
		{
			if (this.isFinished || this.cantFindTargets)
			{
				return;
			}
			if (!this.taggingDone)
			{
				this.cantFindTargets = false;
				string[] criteriaSplit = this.Criteria.Split(';', StringSplitOptions.None);
				foreach (string entry in criteriaSplit)
				{
					string[] kvp = entry.Split(':', StringSplitOptions.None);
					Identifier key = kvp[0].Trim().ToIdentifier();
					Identifier value = (kvp.Length > 1) ? kvp[1].Trim().ToIdentifier() : Identifier.Empty;
					Action<Identifier> tagger;
					if (this.Taggers.TryGetValue(key, out tagger))
					{
						tagger(value);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in TagAction (event \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\") - unrecognized target criteria \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(key);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						string errorMessage = defaultInterpolatedStringHandler.ToStringAndClear();
						string error = errorMessage;
						Exception e = null;
						EventPrefab prefab = this.ParentEvent.Prefab;
						DebugConsole.ThrowError(error, e, (prefab != null) ? prefab.ContentPackage : null, false, false);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(34, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("TagAction.Update:InvalidCriteria_");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("_");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(key);
						GameAnalyticsManager.AddErrorEventOnce(defaultInterpolatedStringHandler2.ToStringAndClear(), GameAnalyticsManager.ErrorSeverity.Error, errorMessage);
					}
				}
				this.taggingDone = true;
			}
			if (this.ContinueIfNoTargetsFound)
			{
				this.isFinished = true;
				return;
			}
			if (this.mustRecheckTargets)
			{
				this.isFinished = this.ParentEvent.GetTargets(this.Tag).Any<Entity>();
				return;
			}
			this.isFinished = !this.cantFindTargets;
		}

		// Token: 0x06003B5B RID: 15195 RVA: 0x002230A8 File Offset: 0x002212A8
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("TagAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Criteria: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Criteria.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Tag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Tag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Sub: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SubmarineType.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Team: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Team.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E58 RID: 7768
		private bool isFinished;

		// Token: 0x04001E59 RID: 7769
		private bool cantFindTargets;

		// Token: 0x04001E5A RID: 7770
		private bool mustRecheckTargets;

		// Token: 0x04001E5B RID: 7771
		private bool taggingDone;

		// Token: 0x04001E5C RID: 7772
		private List<Entity> tempEntities;

		// Token: 0x04001E5D RID: 7773
		private readonly ImmutableDictionary<Identifier, Action<Identifier>> Taggers;

		// Token: 0x02000F38 RID: 3896
		public enum SubType
		{
			// Token: 0x0400550C RID: 21772
			Any,
			// Token: 0x0400550D RID: 21773
			Player,
			// Token: 0x0400550E RID: 21774
			Outpost,
			// Token: 0x0400550F RID: 21775
			Wreck = 4,
			// Token: 0x04005510 RID: 21776
			BeaconStation = 8,
			// Token: 0x04005511 RID: 21777
			Enemy = 16,
			// Token: 0x04005512 RID: 21778
			Ruin = 32
		}

		// Token: 0x02000F39 RID: 3897
		public enum CharacterTeam
		{
			// Token: 0x04005514 RID: 21780
			Any,
			// Token: 0x04005515 RID: 21781
			None,
			// Token: 0x04005516 RID: 21782
			Team1,
			// Token: 0x04005517 RID: 21783
			Team2 = 4,
			// Token: 0x04005518 RID: 21784
			FriendlyNPC = 8
		}
	}
}
