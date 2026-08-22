using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200004C RID: 76
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventLog
	{
		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000AE6 RID: 2790 RVA: 0x00062CFE File Offset: 0x00060EFE
		// (set) Token: 0x06000AE7 RID: 2791 RVA: 0x00062D06 File Offset: 0x00060F06
		public bool UnreadEntries { get; private set; }

		// Token: 0x06000AE8 RID: 2792 RVA: 0x00062D0F File Offset: 0x00060F0F
		public void AddEntry(Identifier eventPrefabId, Identifier entryId, string text)
		{
			this.TryAddEntryInternal(eventPrefabId, entryId, text);
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.EnableEventLogNotificationIcon(true);
			}
			this.UnreadEntries = true;
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x00062D34 File Offset: 0x00060F34
		public void CreateEventLogUI(GUIComponent parent, TraitorManager.TraitorResults? traitorResults = null)
		{
			this.UnreadEntries = false;
			int spacing = GUI.IntScale(5f);
			foreach (EventLog.Event ev in this.events.Values)
			{
				LocalizedString nameString = string.Empty;
				int difficultyIconCount = 0;
				EventPrefab eventPrefab;
				EventPrefab.Prefabs.TryGet(ev.EventIdentifier, out eventPrefab);
				if (eventPrefab != null)
				{
					nameString = RichString.Rich(eventPrefab.Name, null);
					TraitorEventPrefab traitorEventPrefab = eventPrefab as TraitorEventPrefab;
					if (traitorEventPrefab != null)
					{
						difficultyIconCount = traitorEventPrefab.DangerLevel;
					}
				}
				List<LocalizedString> textContent = new List<LocalizedString>();
				textContent.AddRange(from e in ev.Entries
				select e.Text);
				GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("TraitorMissionIcon");
				Sprite icon = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
				GUIImage missionIcon;
				RoundSummary.CreateMissionEntry(parent, nameString, textContent, difficultyIconCount, icon, GUIStyle.Red, null, out missionIcon);
				if (traitorResults != null && traitorResults.Value.TraitorEventIdentifier == ev.EventIdentifier)
				{
					RoundSummary.UpdateMissionStateIcon(traitorResults.Value.ObjectiveSuccessful, missionIcon, 0.5f);
				}
			}
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x00062EA0 File Offset: 0x000610A0
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

		// Token: 0x06000AEB RID: 2795 RVA: 0x00062F2A File Offset: 0x0006112A
		public void Clear()
		{
			this.events.Clear();
		}

		// Token: 0x0400058C RID: 1420
		private readonly Dictionary<Identifier, EventLog.Event> events = new Dictionary<Identifier, EventLog.Event>();

		// Token: 0x020007A4 RID: 1956
		[NullableContext(0)]
		public class Event
		{
			// Token: 0x06006B25 RID: 27429 RVA: 0x0035C8E7 File Offset: 0x0035AAE7
			public Event(Identifier eventPrefabId)
			{
				this.EventIdentifier = eventPrefabId;
			}

			// Token: 0x04003B4F RID: 15183
			public readonly Identifier EventIdentifier;

			// Token: 0x04003B50 RID: 15184
			[Nullable(1)]
			public readonly List<EventLog.Entry> Entries = new List<EventLog.Entry>();
		}

		// Token: 0x020007A5 RID: 1957
		[Nullable(0)]
		public class Entry
		{
			// Token: 0x06006B26 RID: 27430 RVA: 0x0035C901 File Offset: 0x0035AB01
			public Entry(Identifier identifier, string text)
			{
				this.Identifier = identifier;
				this.Text = text;
			}

			// Token: 0x04003B51 RID: 15185
			public readonly Identifier Identifier;

			// Token: 0x04003B52 RID: 15186
			public string Text;
		}
	}
}
