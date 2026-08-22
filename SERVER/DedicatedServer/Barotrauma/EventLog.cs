using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200001E RID: 30
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventLog
	{
		// Token: 0x0600040D RID: 1037 RVA: 0x000212DC File Offset: 0x0001F4DC
		public bool TryAddEntry(Identifier eventPrefabId, Identifier entryId, string text, IEnumerable<Client> targetClients)
		{
			if (this.TryAddEntryInternal(eventPrefabId, entryId, text))
			{
				foreach (Client targetClient in targetClients)
				{
					EventManager.ServerWriteEventLog(targetClient, new EventManager.NetEventLogEntry(eventPrefabId, entryId, text));
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x0002133C File Offset: 0x0001F53C
		private bool TryAddEntryInternal(Identifier eventPrefabId, Identifier entryId, string text)
		{
			EventLog.Event ev;
			if (!this.events.TryGetValue(eventPrefabId, out ev))
			{
				ev = new EventLog.Event(eventPrefabId);
				this.events.Add(eventPrefabId, ev);
			}
			EventLog.Entry entry = ev.Entries.FirstOrDefault((EventLog.Entry e) => e.Identifier == entryId);
			if (entry == null)
			{
				ev.Entries.Add(new EventLog.Entry(entryId, text));
				return true;
			}
			if (entry.Text != text)
			{
				entry.Text = text;
				return true;
			}
			return false;
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x000213C6 File Offset: 0x0001F5C6
		public void Clear()
		{
			this.events.Clear();
		}

		// Token: 0x040001DC RID: 476
		private readonly Dictionary<Identifier, EventLog.Event> events = new Dictionary<Identifier, EventLog.Event>();

		// Token: 0x020005B4 RID: 1460
		[NullableContext(0)]
		public class Event
		{
			// Token: 0x06004BCA RID: 19402 RVA: 0x001DC053 File Offset: 0x001DA253
			public Event(Identifier eventPrefabId)
			{
				this.EventIdentifier = eventPrefabId;
			}

			// Token: 0x0400274B RID: 10059
			public readonly Identifier EventIdentifier;

			// Token: 0x0400274C RID: 10060
			[Nullable(1)]
			public readonly List<EventLog.Entry> Entries = new List<EventLog.Entry>();
		}

		// Token: 0x020005B5 RID: 1461
		[Nullable(0)]
		public class Entry
		{
			// Token: 0x06004BCB RID: 19403 RVA: 0x001DC06D File Offset: 0x001DA26D
			public Entry(Identifier identifier, string text)
			{
				this.Identifier = identifier;
				this.Text = text;
			}

			// Token: 0x0400274D RID: 10061
			public readonly Identifier Identifier;

			// Token: 0x0400274E RID: 10062
			public string Text;
		}
	}
}
