using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002B3 RID: 691
	internal class EventPrefab : Prefab
	{
		// Token: 0x17000FC8 RID: 4040
		// (get) Token: 0x06003BF2 RID: 15346 RVA: 0x00225123 File Offset: 0x00223323
		public ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x06003BF3 RID: 15347 RVA: 0x0022512C File Offset: 0x0022332C
		public static EventPrefab Create(ContentXElement element, RandomEventsFile file, Identifier fallbackIdentifier = default(Identifier))
		{
			Identifier identifier = element.NameAsIdentifier();
			if (identifier == "TraitorEvent")
			{
				return new TraitorEventPrefab(element, file, fallbackIdentifier);
			}
			return new EventPrefab(element, file, fallbackIdentifier);
		}

		// Token: 0x06003BF4 RID: 15348 RVA: 0x00225160 File Offset: 0x00223360
		public EventPrefab(ContentXElement element, RandomEventsFile file, Identifier fallbackIdentifier = default(Identifier)) : base(file, element.GetAttributeIdentifier("identifier", fallbackIdentifier))
		{
			this.ConfigElement = element;
			try
			{
				string str = "Barotrauma.";
				XName name = this.ConfigElement.Name;
				this.EventType = Type.GetType(str + ((name != null) ? name.ToString() : null), true, true);
				if (this.EventType == null)
				{
					string str2 = "Could not find an event class of the type \"";
					XName name2 = this.ConfigElement.Name;
					DebugConsole.ThrowError(str2 + ((name2 != null) ? name2.ToString() : null) + "\".", null, element.ContentPackage, false, false);
				}
			}
			catch
			{
				string str3 = "Could not find an event class of the type \"";
				XName name3 = this.ConfigElement.Name;
				DebugConsole.ThrowError(str3 + ((name3 != null) ? name3.ToString() : null) + "\".", null, element.ContentPackage, false, false);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("eventname.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(this.Identifier.ToString(), true);
			this.tags = this.ConfigElement.GetAttributeIdentifierImmutableHashSet("tags", ImmutableHashSet<Identifier>.Empty, true);
			this.BiomeIdentifier = this.ConfigElement.GetAttributeIdentifier("biome", Identifier.Empty);
			this.Faction = this.ConfigElement.GetAttributeIdentifier("faction", Identifier.Empty);
			this.Commonness = element.GetAttributeFloat("commonness", 1f);
			this.Probability = Math.Clamp(element.GetAttributeFloat(1f, new string[]
			{
				"probability",
				"spawnprobability"
			}), 0f, 1f);
			this.TriggerEventCooldown = element.GetAttributeBool("triggereventcooldown", this.EventType != typeof(ScriptedEvent));
			this.RequiredLayer = element.GetAttributeIdentifier("RequiredLayer", Identifier.Empty);
			this.RequiredSpawnPointTag = element.GetAttributeIdentifier("RequiredSpawnPointTag", Identifier.Empty);
			this.UnlockPathEvent = element.GetAttributeBool("unlockpathevent", false);
			this.UnlockPathTooltip = element.GetAttributeString("unlockpathtooltip", "lockedpathtooltip");
			this.UnlockPathReputation = element.GetAttributeInt("unlockpathreputation", 0);
		}

		// Token: 0x06003BF5 RID: 15349 RVA: 0x002253BC File Offset: 0x002235BC
		public bool TryCreateInstance<T>(int seed, out T instance) where T : Event
		{
			instance = (this.CreateInstance(seed) as T);
			return instance != null;
		}

		// Token: 0x06003BF6 RID: 15350 RVA: 0x002253E8 File Offset: 0x002235E8
		public Event CreateInstance(int seed)
		{
			ConstructorInfo constructor = this.EventType.GetConstructor(new Type[]
			{
				base.GetType(),
				typeof(int)
			});
			Event instance = null;
			try
			{
				instance = (constructor.Invoke(new object[]
				{
					this,
					seed
				}) as Event);
			}
			catch (Exception ex)
			{
				DebugConsole.ThrowError((ex.InnerException != null) ? ex.InnerException.ToString() : ex.ToString(), null, null, false, false);
			}
			if (instance != null && !instance.LevelMeetsRequirements())
			{
				return null;
			}
			return instance;
		}

		// Token: 0x06003BF7 RID: 15351 RVA: 0x00225488 File Offset: 0x00223688
		public override void Dispose()
		{
		}

		// Token: 0x06003BF8 RID: 15352 RVA: 0x0022548C File Offset: 0x0022368C
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("EventPrefab");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003BF9 RID: 15353 RVA: 0x002254DC File Offset: 0x002236DC
		public static EventPrefab GetUnlockPathEvent(Identifier biomeIdentifier, Faction faction)
		{
			IEnumerable<EventPrefab> unlockPathEvents = from p in EventPrefab.Prefabs
			orderby p.Identifier
			select p into e
			where e.UnlockPathEvent
			select e;
			if (faction != null && unlockPathEvents.Any((EventPrefab e) => e.Faction == faction.Prefab.Identifier))
			{
				unlockPathEvents = from e in unlockPathEvents
				where e.Faction == faction.Prefab.Identifier
				select e;
			}
			EventPrefab result;
			if ((result = unlockPathEvents.FirstOrDefault((EventPrefab ep) => ep.BiomeIdentifier == biomeIdentifier)) == null)
			{
				result = unlockPathEvents.FirstOrDefault((EventPrefab ep) => ep.BiomeIdentifier == Identifier.Empty);
			}
			return result;
		}

		// Token: 0x06003BFA RID: 15354 RVA: 0x002255B8 File Offset: 0x002237B8
		public static EventPrefab FindEventPrefab(Identifier identifier, Identifier tag, ContentPackage source)
		{
			EventPrefab eventPrefab = null;
			if (!identifier.IsEmpty)
			{
				eventPrefab = EventSet.GetEventPrefab(identifier);
				if (eventPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to find an event prefab with the identifier ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, source, false, false);
				}
			}
			else if (!tag.IsEmpty)
			{
				eventPrefab = (from e in EventSet.GetAllEventPrefabs()
				where e.Tags.Contains(tag)
				select e).GetRandomUnsynced<EventPrefab>();
				if (eventPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(45, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to find an event prefab with the tag ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(tag);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, source, false, false);
				}
			}
			else
			{
				DebugConsole.ThrowError("Failed to find an event prefab: neither an identifier or tag were defined.", null, source, false, false);
			}
			return eventPrefab;
		}

		// Token: 0x04001E9E RID: 7838
		public static readonly PrefabCollection<EventPrefab> Prefabs = new PrefabCollection<EventPrefab>();

		// Token: 0x04001E9F RID: 7839
		public readonly ContentXElement ConfigElement;

		// Token: 0x04001EA0 RID: 7840
		public readonly Type EventType;

		// Token: 0x04001EA1 RID: 7841
		private readonly ImmutableHashSet<Identifier> tags;

		// Token: 0x04001EA2 RID: 7842
		public readonly float Probability;

		// Token: 0x04001EA3 RID: 7843
		public readonly bool TriggerEventCooldown;

		// Token: 0x04001EA4 RID: 7844
		public readonly float Commonness;

		// Token: 0x04001EA5 RID: 7845
		public readonly Identifier BiomeIdentifier;

		// Token: 0x04001EA6 RID: 7846
		public readonly Identifier RequiredLayer;

		// Token: 0x04001EA7 RID: 7847
		public readonly Identifier RequiredSpawnPointTag;

		// Token: 0x04001EA8 RID: 7848
		public readonly Identifier Faction;

		// Token: 0x04001EA9 RID: 7849
		public readonly LocalizedString Name;

		// Token: 0x04001EAA RID: 7850
		public readonly bool UnlockPathEvent;

		// Token: 0x04001EAB RID: 7851
		public readonly string UnlockPathTooltip;

		// Token: 0x04001EAC RID: 7852
		public readonly int UnlockPathReputation;
	}
}
