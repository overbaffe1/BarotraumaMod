using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000CF RID: 207
	internal class Job
	{
		// Token: 0x170006BF RID: 1727
		// (get) Token: 0x0600171D RID: 5917 RVA: 0x000C19D8 File Offset: 0x000BFBD8
		public LocalizedString Name
		{
			get
			{
				return this.prefab.Name;
			}
		}

		// Token: 0x170006C0 RID: 1728
		// (get) Token: 0x0600171E RID: 5918 RVA: 0x000C19E5 File Offset: 0x000BFBE5
		public LocalizedString Description
		{
			get
			{
				return this.prefab.Description;
			}
		}

		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x0600171F RID: 5919 RVA: 0x000C19F2 File Offset: 0x000BFBF2
		public JobPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x06001720 RID: 5920 RVA: 0x000C19FA File Offset: 0x000BFBFA
		// (set) Token: 0x06001721 RID: 5921 RVA: 0x000C1A02 File Offset: 0x000BFC02
		public Skill PrimarySkill { get; private set; }

		// Token: 0x06001722 RID: 5922 RVA: 0x000C1A0B File Offset: 0x000BFC0B
		public Job(JobPrefab jobPrefab, bool isPvP) : this(jobPrefab, isPvP, Rand.RandSync.Unsynced, 0, Array.Empty<Skill>())
		{
		}

		// Token: 0x06001723 RID: 5923 RVA: 0x000C1A1C File Offset: 0x000BFC1C
		public Job(JobPrefab jobPrefab, bool isPvP, Rand.RandSync randSync, int variant, params Skill[] s)
		{
			this.prefab = jobPrefab;
			this.Variant = variant;
			this.skills = new Dictionary<Identifier, Skill>();
			foreach (Skill skill in s)
			{
				this.skills.Add(skill.Identifier, skill);
			}
			foreach (SkillPrefab skillPrefab in this.prefab.Skills)
			{
				Skill skill2;
				if (this.skills.ContainsKey(skillPrefab.Identifier))
				{
					skill2 = this.skills[skillPrefab.Identifier];
					this.skills[skillPrefab.Identifier] = new Skill(skill2.Identifier, skill2.Level);
				}
				else
				{
					skill2 = new Skill(skillPrefab, isPvP, randSync);
					this.skills.Add(skillPrefab.Identifier, skill2);
				}
				if (skillPrefab.IsPrimarySkill)
				{
					this.PrimarySkill = skill2;
				}
			}
		}

		// Token: 0x06001724 RID: 5924 RVA: 0x000C1B3C File Offset: 0x000BFD3C
		public Job(ContentXElement element)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			JobPrefab p;
			if (!JobPrefab.Prefabs.ContainsKey(identifier))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the job ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral(". Giving the character a random job.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				p = JobPrefab.Random(Rand.RandSync.Unsynced, null);
			}
			else
			{
				p = JobPrefab.Prefabs[identifier];
			}
			this.prefab = p;
			this.skills = new Dictionary<Identifier, Skill>();
			foreach (ContentXElement subElement in element.Elements())
			{
				Identifier identifier2 = subElement.NameAsIdentifier();
				if (!(identifier2 != "skill"))
				{
					Identifier skillIdentifier = subElement.GetAttributeIdentifier("identifier", "");
					if (!skillIdentifier.IsEmpty)
					{
						Skill skill = new Skill(skillIdentifier, subElement.GetAttributeFloat("level", 0f));
						this.skills.Add(skillIdentifier, skill);
						Identifier? identifier3 = new Identifier?(skillIdentifier);
						SkillPrefab primarySkill = this.prefab.PrimarySkill;
						Identifier? identifier4;
						Identifier? identifier5;
						if (primarySkill == null)
						{
							identifier4 = null;
							identifier5 = identifier4;
						}
						else
						{
							identifier5 = new Identifier?(primarySkill.Identifier);
						}
						identifier4 = identifier5;
						if (identifier3 == identifier4)
						{
							this.PrimarySkill = skill;
						}
					}
				}
			}
		}

		// Token: 0x06001725 RID: 5925 RVA: 0x000C1CB8 File Offset: 0x000BFEB8
		public static Job Random(bool isPvP, Rand.RandSync randSync)
		{
			JobPrefab prefab = JobPrefab.Random(randSync, null);
			int variant = Rand.Range(0, prefab.Variants, randSync);
			return new Job(prefab, isPvP, randSync, variant, Array.Empty<Skill>());
		}

		// Token: 0x06001726 RID: 5926 RVA: 0x000C1CE9 File Offset: 0x000BFEE9
		public IEnumerable<Skill> GetSkills()
		{
			return this.skills.Values;
		}

		// Token: 0x06001727 RID: 5927 RVA: 0x000C1CF8 File Offset: 0x000BFEF8
		public float GetSkillLevel(Identifier skillIdentifier)
		{
			if (skillIdentifier.IsEmpty)
			{
				return 0f;
			}
			Skill skill;
			this.skills.TryGetValue(skillIdentifier, out skill);
			if (skill == null)
			{
				return 0f;
			}
			return skill.Level;
		}

		// Token: 0x06001728 RID: 5928 RVA: 0x000C1D34 File Offset: 0x000BFF34
		public Skill GetSkill(Identifier skillIdentifier)
		{
			if (skillIdentifier.IsEmpty)
			{
				return null;
			}
			Skill skill;
			this.skills.TryGetValue(skillIdentifier, out skill);
			return skill;
		}

		// Token: 0x06001729 RID: 5929 RVA: 0x000C1D5C File Offset: 0x000BFF5C
		public void OverrideSkills(Dictionary<Identifier, float> newSkills)
		{
			this.skills.Clear();
			foreach (KeyValuePair<Identifier, float> newSkillInfo in newSkills)
			{
				Skill newSkill = new Skill(newSkillInfo.Key, newSkillInfo.Value);
				if (this.PrimarySkill != null && newSkill.Identifier == this.PrimarySkill.Identifier)
				{
					this.PrimarySkill = newSkill;
				}
				this.skills.Add(newSkillInfo.Key, newSkill);
			}
		}

		// Token: 0x0600172A RID: 5930 RVA: 0x000C1DFC File Offset: 0x000BFFFC
		public void IncreaseSkillLevel(Identifier skillIdentifier, float increase, bool increasePastMax)
		{
			Skill skill;
			if (this.skills.TryGetValue(skillIdentifier, out skill))
			{
				skill.IncreaseSkill(increase, increasePastMax);
				return;
			}
			this.skills.Add(skillIdentifier, new Skill(skillIdentifier, increase));
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x000C1E35 File Offset: 0x000C0035
		public bool HasJobItem(Func<JobPrefab.JobItem, bool> predicate)
		{
			return this.prefab.HasJobItem(this.Variant, predicate);
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x000C1E4C File Offset: 0x000C004C
		public void GiveJobItems(Character character, bool isPvPMode, WayPoint spawnPoint = null)
		{
			ImmutableArray<JobPrefab.JobItem> spawnItems;
			if (!this.prefab.JobItems.TryGetValue(this.Variant, out spawnItems))
			{
				return;
			}
			foreach (JobPrefab.JobItem jobItem in spawnItems)
			{
				if (jobItem.ParentItem == null)
				{
					for (int i = 0; i < jobItem.Amount; i++)
					{
						this.InitializeJobItem(character, isPvPMode, jobItem, spawnItems, spawnPoint, null);
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.TraitorsEnabled && character.IsSecurity)
			{
				ItemPrefab traitorGuidelineItem = ItemPrefab.Prefabs.Find((ItemPrefab ip) => ip.Tags.Contains(Tags.TraitorGuidelinesForSecurity));
				Entity.Spawner.AddItemToSpawnQueue(traitorGuidelineItem, character.Inventory, null, null, null, true, false, InvSlotType.None);
			}
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x000C1F2C File Offset: 0x000C012C
		private void InitializeJobItem(Character character, bool isPvPMode, JobPrefab.JobItem jobItem, IEnumerable<JobPrefab.JobItem> allJobItems, WayPoint spawnPoint = null, Item parentItem = null)
		{
			Identifier itemIdentifier = jobItem.GetItemIdentifier(character.TeamID, isPvPMode);
			if (itemIdentifier.IsEmpty)
			{
				return;
			}
			ItemPrefab itemPrefab = (MapEntityPrefab.FindByIdentifier(itemIdentifier) ?? MapEntityPrefab.FindByName(itemIdentifier.Value)) as ItemPrefab;
			if (itemPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to spawn \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" with the item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(itemIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Matching item prefab not found.");
				DebugConsole.ThrowErrorLocalized(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			Item item = new Item(itemPrefab, character.Position, null, 0, true);
			if (GameMain.Server != null && Entity.Spawner != null)
			{
				if (GameMain.Server.EntityEventManager.UniqueEvents.Any((ServerEntityEvent ev) => ev.Entity == item))
				{
					string errorMsg = "Error while spawning job items. Item " + item.Name + " created network events before the spawn event had been created.";
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("Job.InitializeJobItem:EventsBeforeSpawning", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					GameMain.Server.EntityEventManager.UniqueEvents.RemoveAll((ServerEntityEvent ev) => ev.Entity == item);
					GameMain.Server.EntityEventManager.Events.RemoveAll((ServerEntityEvent ev) => ev.Entity == item);
				}
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
			}
			if (jobItem.Equip)
			{
				List<InvSlotType> list;
				if (item.GetComponents<Pickable>().Count<Pickable>() <= 1)
				{
					list = new List<InvSlotType>(item.AllowedSlots);
				}
				else
				{
					Wearable component = item.GetComponent<Wearable>();
					list = new List<InvSlotType>(((component != null) ? component.AllowedSlots : null) ?? item.GetComponent<Pickable>().AllowedSlots);
				}
				List<InvSlotType> allowedSlots = list;
				allowedSlots.Remove(InvSlotType.Any);
				item.UnequipAutomatically = false;
				character.Inventory.TryPutItem(item, null, allowedSlots, true, false, true);
			}
			else
			{
				character.Inventory.TryPutItem(item, null, item.AllowedSlots, true, false, true);
			}
			Wearable wearable = item.GetComponent<Wearable>();
			if (wearable != null)
			{
				if (this.Variant > 0 && this.Variant <= wearable.Variants)
				{
					wearable.Variant = this.Variant;
				}
				else
				{
					wearable.Variant = wearable.Variant;
					if (wearable.Variants > 0 && this.Variant == 0)
					{
						this.Variant = wearable.Variant;
					}
				}
			}
			IdCard idCardComponent = item.GetComponent<IdCard>();
			if (idCardComponent != null)
			{
				idCardComponent.Initialize(spawnPoint, character);
			}
			foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
			{
				wifiComponent.TeamID = character.TeamID;
			}
			if (parentItem != null)
			{
				parentItem.Combine(item, null);
			}
			foreach (JobPrefab.JobItem childItem in allJobItems)
			{
				if (childItem.ParentItem == jobItem)
				{
					for (int i = 0; i < childItem.Amount; i++)
					{
						this.InitializeJobItem(character, isPvPMode, childItem, allJobItems, spawnPoint, item);
					}
				}
			}
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x000C22A0 File Offset: 0x000C04A0
		public XElement Save(XElement parentElement)
		{
			XElement jobElement = new XElement("job");
			jobElement.Add(new XAttribute("name", this.Name));
			jobElement.Add(new XAttribute("identifier", this.prefab.Identifier));
			foreach (KeyValuePair<Identifier, Skill> skill in this.skills)
			{
				jobElement.Add(new XElement("skill", new object[]
				{
					new XAttribute("identifier", skill.Value.Identifier),
					new XAttribute("level", skill.Value.Level)
				}));
			}
			parentElement.Add(jobElement);
			return jobElement;
		}

		// Token: 0x04000B27 RID: 2855
		private readonly JobPrefab prefab;

		// Token: 0x04000B28 RID: 2856
		private readonly Dictionary<Identifier, Skill> skills;

		// Token: 0x04000B29 RID: 2857
		public int Variant;
	}
}
