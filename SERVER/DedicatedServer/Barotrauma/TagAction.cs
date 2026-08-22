using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001B5 RID: 437
	internal class TagAction : EventAction
	{
		// Token: 0x1700094F RID: 2383
		// (get) Token: 0x06002071 RID: 8305 RVA: 0x000DC4CC File Offset: 0x000DA6CC
		// (set) Token: 0x06002072 RID: 8306 RVA: 0x000DC4D4 File Offset: 0x000DA6D4
		[Serialize("", IsPropertySaveable.Yes, "What criteria to use to select the entities to target. Valid values are players, player, traitor, nontraitor, nontraitorplayer, bot, crew, humanprefabidentifier:[id], jobidentifier:[id], structureidentifier:[id], structurespecialtag:[tag], itemidentifier:[id], itemtag:[tag], hull, hullname:[name], submarine:[type], eventtag:[tag], speciesname:[id].", "", false)]
		public string Criteria { get; set; }

		// Token: 0x17000950 RID: 2384
		// (get) Token: 0x06002073 RID: 8307 RVA: 0x000DC4DD File Offset: 0x000DA6DD
		// (set) Token: 0x06002074 RID: 8308 RVA: 0x000DC4E5 File Offset: 0x000DA6E5
		[Serialize("", IsPropertySaveable.Yes, "The tag to apply to the target.", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x17000951 RID: 2385
		// (get) Token: 0x06002075 RID: 8309 RVA: 0x000DC4EE File Offset: 0x000DA6EE
		// (set) Token: 0x06002076 RID: 8310 RVA: 0x000DC4F6 File Offset: 0x000DA6F6
		[Serialize(TagAction.SubType.Any, IsPropertySaveable.Yes, "The type of submarine the target needs to be in.", "", false)]
		public TagAction.SubType SubmarineType { get; set; }

		// Token: 0x17000952 RID: 2386
		// (get) Token: 0x06002077 RID: 8311 RVA: 0x000DC4FF File Offset: 0x000DA6FF
		// (set) Token: 0x06002078 RID: 8312 RVA: 0x000DC507 File Offset: 0x000DA707
		[Serialize(TagAction.CharacterTeam.Any, IsPropertySaveable.Yes, "The team the target needs to be on.", "", false)]
		public TagAction.CharacterTeam Team { get; set; }

		// Token: 0x17000953 RID: 2387
		// (get) Token: 0x06002079 RID: 8313 RVA: 0x000DC510 File Offset: 0x000DA710
		// (set) Token: 0x0600207A RID: 8314 RVA: 0x000DC518 File Offset: 0x000DA718
		[Serialize("", IsPropertySaveable.Yes, "If set, the target must be in an outpost module that has this tag.", "", false)]
		public Identifier RequiredModuleTag { get; set; }

		// Token: 0x17000954 RID: 2388
		// (get) Token: 0x0600207B RID: 8315 RVA: 0x000DC521 File Offset: 0x000DA721
		// (set) Token: 0x0600207C RID: 8316 RVA: 0x000DC529 File Offset: 0x000DA729
		[Serialize(true, IsPropertySaveable.Yes, "Should incapacitated (e.g. dead, paralyzed, unconscious) characters be ignored, i.e. not considered valid targets?", "", false)]
		public bool IgnoreIncapacitatedCharacters { get; set; }

		// Token: 0x17000955 RID: 2389
		// (get) Token: 0x0600207D RID: 8317 RVA: 0x000DC532 File Offset: 0x000DA732
		// (set) Token: 0x0600207E RID: 8318 RVA: 0x000DC53A File Offset: 0x000DA73A
		[Serialize(false, IsPropertySaveable.Yes, "Can items that have been set to be hidden in-game be tagged?", "", false)]
		public bool AllowHiddenItems { get; set; }

		// Token: 0x17000956 RID: 2390
		// (get) Token: 0x0600207F RID: 8319 RVA: 0x000DC543 File Offset: 0x000DA743
		// (set) Token: 0x06002080 RID: 8320 RVA: 0x000DC54B File Offset: 0x000DA74B
		[Serialize(false, IsPropertySaveable.Yes, "If there are multiple matching targets, should all of them be tagged or one chosen randomly?", "", false)]
		public bool ChooseRandom { get; set; }

		// Token: 0x17000957 RID: 2391
		// (get) Token: 0x06002081 RID: 8321 RVA: 0x000DC554 File Offset: 0x000DA754
		// (set) Token: 0x06002082 RID: 8322 RVA: 0x000DC55C File Offset: 0x000DA75C
		[Serialize("", IsPropertySaveable.Yes, "If choosing a random target, targets with this tag can optionally be excluded.", "", false)]
		public Identifier ChooseRandomExcludingTag { get; set; }

		// Token: 0x17000958 RID: 2392
		// (get) Token: 0x06002083 RID: 8323 RVA: 0x000DC565 File Offset: 0x000DA765
		// (set) Token: 0x06002084 RID: 8324 RVA: 0x000DC56D File Offset: 0x000DA76D
		[Serialize(false, IsPropertySaveable.Yes, "Should the event continue if the TagAction can't find any valid targets?", "", false)]
		public bool ContinueIfNoTargetsFound { get; set; }

		// Token: 0x17000959 RID: 2393
		// (get) Token: 0x06002085 RID: 8325 RVA: 0x000DC576 File Offset: 0x000DA776
		// (set) Token: 0x06002086 RID: 8326 RVA: 0x000DC57E File Offset: 0x000DA77E
		[Serialize(0f, IsPropertySaveable.Yes, "If larger than 0, the specified percentage of the matching targets are tagged. Between 0-100.", "", false)]
		public float ChoosePercentage { get; set; }

		// Token: 0x06002087 RID: 8327 RVA: 0x000DC588 File Offset: 0x000DA788
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

		// Token: 0x06002088 RID: 8328 RVA: 0x000DC804 File Offset: 0x000DAA04
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06002089 RID: 8329 RVA: 0x000DC80C File Offset: 0x000DAA0C
		public override void Reset()
		{
			this.taggingDone = false;
			this.cantFindTargets = false;
			this.isFinished = false;
		}

		// Token: 0x0600208A RID: 8330 RVA: 0x000DC824 File Offset: 0x000DAA24
		private void TagBySpeciesName(Identifier speciesName)
		{
			this.AddTarget(this.Tag, Character.CharacterList.Where(delegate(Character c)
			{
				Identifier speciesName2 = c.SpeciesName;
				return speciesName2 == speciesName && this.CharacterTeamMatches(c);
			}));
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x000DC867 File Offset: 0x000DAA67
		private void TagByEventTag(Identifier eventTag)
		{
			this.AddTarget(this.Tag, from t in this.ParentEvent.GetTargets(eventTag)
			where this.MatchesRequirements(t)
			select t);
		}

		// Token: 0x0600208C RID: 8332 RVA: 0x000DC892 File Offset: 0x000DAA92
		private void TagPlayers()
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsPlayer && (!c.IsIncapacitated || !this.IgnoreIncapacitatedCharacters) && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x0600208D RID: 8333 RVA: 0x000DC8AD File Offset: 0x000DAAAD
		private void TagTraitors()
		{
			this.AddTargetPredicate(Tags.Traitor, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && (c.IsPlayer || c.IsBot) && c.IsTraitor && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x0600208E RID: 8334 RVA: 0x000DC8C7 File Offset: 0x000DAAC7
		private void TagNonTraitors()
		{
			this.AddTargetPredicate(Tags.NonTraitor, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && (c.IsPlayer || c.IsBot) && !c.IsTraitor && c.IsOnPlayerTeam && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x0600208F RID: 8335 RVA: 0x000DC8E1 File Offset: 0x000DAAE1
		private void TagNonTraitorPlayers()
		{
			this.AddTargetPredicate(Tags.NonTraitorPlayer, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsPlayer && !c.IsTraitor && c.IsOnPlayerTeam && !c.IsIncapacitated && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06002090 RID: 8336 RVA: 0x000DC8FC File Offset: 0x000DAAFC
		private void TagBots(bool playerCrewOnly)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsBot && (!c.IsIncapacitated || !this.IgnoreIncapacitatedCharacters) && (!playerCrewOnly || c.TeamID == CharacterTeamType.Team1) && this.CharacterTeamMatches(c);
			});
		}

		// Token: 0x06002091 RID: 8337 RVA: 0x000DC936 File Offset: 0x000DAB36
		private void TagCrew()
		{
			this.TagPlayers();
			this.TagBots(true);
		}

		// Token: 0x06002092 RID: 8338 RVA: 0x000DC948 File Offset: 0x000DAB48
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

		// Token: 0x06002093 RID: 8339 RVA: 0x000DC98C File Offset: 0x000DAB8C
		private void TagHumansByTag(Identifier tag)
		{
			this.AddTarget(this.Tag, from c in Character.CharacterList
			where c.HumanPrefab != null && c.HumanPrefab.GetTags().Contains(tag) && this.CharacterTeamMatches(c)
			select c);
		}

		// Token: 0x06002094 RID: 8340 RVA: 0x000DC9D0 File Offset: 0x000DABD0
		private void TagHumansByJobIdentifier(Identifier jobIdentifier)
		{
			this.AddTarget(this.Tag, from c in Character.CharacterList
			where c.HasJob(jobIdentifier) && this.CharacterTeamMatches(c)
			select c);
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x000DCA14 File Offset: 0x000DAC14
		private void TagStructuresByIdentifier(Identifier identifier)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Structure, delegate(Entity e)
			{
				Structure s = e as Structure;
				return s != null && this.MatchesRequirements(s) && s.Prefab.Identifier == identifier;
			});
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x000DCA50 File Offset: 0x000DAC50
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

		// Token: 0x06002097 RID: 8343 RVA: 0x000DCA8C File Offset: 0x000DAC8C
		private void TagItemsByIdentifier(Identifier identifier)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Item, delegate(Entity e)
			{
				Item it = e as Item;
				return it != null && it.Prefab.Identifier == identifier && this.IsValidItem(it);
			});
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x000DCAC8 File Offset: 0x000DACC8
		private void TagItemsByTag(Identifier tag)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Item, delegate(Entity e)
			{
				Item it = e as Item;
				return it != null && it.HasTag(tag) && this.IsValidItem(it);
			});
		}

		// Token: 0x06002099 RID: 8345 RVA: 0x000DCB02 File Offset: 0x000DAD02
		private void TagHulls()
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Hull, delegate(Entity e)
			{
				Hull h = e as Hull;
				return h != null && this.MatchesRequirements(h);
			});
		}

		// Token: 0x0600209A RID: 8346 RVA: 0x000DCB20 File Offset: 0x000DAD20
		private void TagHullsByName(Identifier name)
		{
			this.AddTargetPredicate(this.Tag, ScriptedEvent.TargetPredicate.EntityType.Hull, delegate(Entity e)
			{
				Hull h = e as Hull;
				return h != null && this.MatchesRequirements(h) && h.RoomName.Contains(name.Value, StringComparison.OrdinalIgnoreCase);
			});
		}

		// Token: 0x0600209B RID: 8347 RVA: 0x000DCB5C File Offset: 0x000DAD5C
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

		// Token: 0x0600209C RID: 8348 RVA: 0x000DCB98 File Offset: 0x000DAD98
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

		// Token: 0x0600209D RID: 8349 RVA: 0x000DCC0C File Offset: 0x000DAE0C
		private bool MatchesRequirements(Entity e)
		{
			return this.ModuleTagMatches(e) && this.SubmarineTypeMatches((e as Submarine) ?? e.Submarine);
		}

		// Token: 0x0600209E RID: 8350 RVA: 0x000DCC30 File Offset: 0x000DAE30
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

		// Token: 0x0600209F RID: 8351 RVA: 0x000DCD44 File Offset: 0x000DAF44
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

		// Token: 0x060020A0 RID: 8352 RVA: 0x000DCDA9 File Offset: 0x000DAFA9
		private bool SubmarineTypeMatches(Submarine sub)
		{
			return TagAction.SubmarineTypeMatches(sub, this.SubmarineType);
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x000DCDB8 File Offset: 0x000DAFB8
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

		// Token: 0x060020A2 RID: 8354 RVA: 0x000DCE7C File Offset: 0x000DB07C
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

		// Token: 0x060020A3 RID: 8355 RVA: 0x000DCF04 File Offset: 0x000DB104
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

		// Token: 0x060020A4 RID: 8356 RVA: 0x000DCF8C File Offset: 0x000DB18C
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

		// Token: 0x060020A5 RID: 8357 RVA: 0x000DD014 File Offset: 0x000DB214
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

		// Token: 0x060020A6 RID: 8358 RVA: 0x000DD070 File Offset: 0x000DB270
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

		// Token: 0x060020A7 RID: 8359 RVA: 0x000DD238 File Offset: 0x000DB438
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

		// Token: 0x04000F6B RID: 3947
		private bool isFinished;

		// Token: 0x04000F6C RID: 3948
		private bool cantFindTargets;

		// Token: 0x04000F6D RID: 3949
		private bool mustRecheckTargets;

		// Token: 0x04000F6E RID: 3950
		private bool taggingDone;

		// Token: 0x04000F6F RID: 3951
		private List<Entity> tempEntities;

		// Token: 0x04000F70 RID: 3952
		private readonly ImmutableDictionary<Identifier, Action<Identifier>> Taggers;

		// Token: 0x02000929 RID: 2345
		public enum SubType
		{
			// Token: 0x04003245 RID: 12869
			Any,
			// Token: 0x04003246 RID: 12870
			Player,
			// Token: 0x04003247 RID: 12871
			Outpost,
			// Token: 0x04003248 RID: 12872
			Wreck = 4,
			// Token: 0x04003249 RID: 12873
			BeaconStation = 8,
			// Token: 0x0400324A RID: 12874
			Enemy = 16,
			// Token: 0x0400324B RID: 12875
			Ruin = 32
		}

		// Token: 0x0200092A RID: 2346
		public enum CharacterTeam
		{
			// Token: 0x0400324D RID: 12877
			Any,
			// Token: 0x0400324E RID: 12878
			None,
			// Token: 0x0400324F RID: 12879
			Team1,
			// Token: 0x04003250 RID: 12880
			Team2 = 4,
			// Token: 0x04003251 RID: 12881
			FriendlyNPC = 8
		}
	}
}
