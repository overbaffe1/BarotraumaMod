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
	// Token: 0x020001C1 RID: 449
	internal class EventPrefab : Prefab
	{
		// Token: 0x17000989 RID: 2441
		// (get) Token: 0x06002154 RID: 8532 RVA: 0x000DF28F File Offset: 0x000DD48F
		public ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x000DF298 File Offset: 0x000DD498
		public static EventPrefab Create(ContentXElement element, RandomEventsFile file, Identifier fallbackIdentifier = default(Identifier))
		{
			Identifier identifier = element.NameAsIdentifier();
			if (identifier == "TraitorEvent")
			{
				return new TraitorEventPrefab(element, file, fallbackIdentifier);
			}
			return new EventPrefab(element, file, fallbackIdentifier);
		}

		// Token: 0x06002156 RID: 8534 RVA: 0x000DF2CC File Offset: 0x000DD4CC
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

		// Token: 0x06002157 RID: 8535 RVA: 0x000DF528 File Offset: 0x000DD728
		public bool TryCreateInstance<T>(int seed, out T instance) where T : Event
		{
			instance = (this.CreateInstance(seed) as T);
			return instance != null;
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x000DF554 File Offset: 0x000DD754
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

		// Token: 0x06002159 RID: 8537 RVA: 0x000DF5F4 File Offset: 0x000DD7F4
		public override void Dispose()
		{
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x000DF5F8 File Offset: 0x000DD7F8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("EventPrefab");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x000DF648 File Offset: 0x000DD848
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

		// Token: 0x0600215C RID: 8540 RVA: 0x000DF724 File Offset: 0x000DD924
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

		// Token: 0x04000FBA RID: 4026
		public static readonly PrefabCollection<EventPrefab> Prefabs = new PrefabCollection<EventPrefab>();

		// Token: 0x04000FBB RID: 4027
		public readonly ContentXElement ConfigElement;

		// Token: 0x04000FBC RID: 4028
		public readonly Type EventType;

		// Token: 0x04000FBD RID: 4029
		private readonly ImmutableHashSet<Identifier> tags;

		// Token: 0x04000FBE RID: 4030
		public readonly float Probability;

		// Token: 0x04000FBF RID: 4031
		public readonly bool TriggerEventCooldown;

		// Token: 0x04000FC0 RID: 4032
		public readonly float Commonness;

		// Token: 0x04000FC1 RID: 4033
		public readonly Identifier BiomeIdentifier;

		// Token: 0x04000FC2 RID: 4034
		public readonly Identifier RequiredLayer;

		// Token: 0x04000FC3 RID: 4035
		public readonly Identifier RequiredSpawnPointTag;

		// Token: 0x04000FC4 RID: 4036
		public readonly Identifier Faction;

		// Token: 0x04000FC5 RID: 4037
		public readonly LocalizedString Name;

		// Token: 0x04000FC6 RID: 4038
		public readonly bool UnlockPathEvent;

		// Token: 0x04000FC7 RID: 4039
		public readonly string UnlockPathTooltip;

		// Token: 0x04000FC8 RID: 4040
		public readonly int UnlockPathReputation;
	}
}
