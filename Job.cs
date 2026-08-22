using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001CD RID: 461
	internal class Job
	{
		// Token: 0x17000D44 RID: 3396
		// (get) Token: 0x0600325A RID: 12890 RVA: 0x00208EE8 File Offset: 0x002070E8
		public LocalizedString Name
		{
			get
			{
				return this.prefab.Name;
			}
		}

		// Token: 0x17000D45 RID: 3397
		// (get) Token: 0x0600325B RID: 12891 RVA: 0x00208EF5 File Offset: 0x002070F5
		public LocalizedString Description
		{
			get
			{
				return this.prefab.Description;
			}
		}

		// Token: 0x17000D46 RID: 3398
		// (get) Token: 0x0600325C RID: 12892 RVA: 0x00208F02 File Offset: 0x00207102
		public JobPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x17000D47 RID: 3399
		// (get) Token: 0x0600325D RID: 12893 RVA: 0x00208F0A File Offset: 0x0020710A
		// (set) Token: 0x0600325E RID: 12894 RVA: 0x00208F12 File Offset: 0x00207112
		public Skill PrimarySkill { get; private set; }

		// Token: 0x0600325F RID: 12895 RVA: 0x00208F1B File Offset: 0x0020711B
		public Job(JobPrefab jobPrefab, bool isPvP) : this(jobPrefab, isPvP, Rand.RandSync.Unsynced, 0, Array.Empty<Skill>())
		{
		}

		// Token: 0x06003260 RID: 12896 RVA: 0x00208F2C File Offset: 0x0020712C
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

		// Token: 0x06003261 RID: 12897 RVA: 0x0020904C File Offset: 0x0020724C
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

		// Token: 0x06003262 RID: 12898 RVA: 0x002091C8 File Offset: 0x002073C8
		public static Job Random(bool isPvP, Rand.RandSync randSync)
		{
			JobPrefab prefab = JobPrefab.Random(randSync, null);
			int variant = Rand.Range(0, prefab.Variants, randSync);
			return new Job(prefab, isPvP, randSync, variant, Array.Empty<Skill>());
		}

		// Token: 0x06003263 RID: 12899 RVA: 0x002091F9 File Offset: 0x002073F9
		public IEnumerable<Skill> GetSkills()
		{
			return this.skills.Values;
		}

		// Token: 0x06003264 RID: 12900 RVA: 0x00209208 File Offset: 0x00207408
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

		// Token: 0x06003265 RID: 12901 RVA: 0x00209244 File Offset: 0x00207444
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

		// Token: 0x06003266 RID: 12902 RVA: 0x0020926C File Offset: 0x0020746C
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

		// Token: 0x06003267 RID: 12903 RVA: 0x0020930C File Offset: 0x0020750C
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

		// Token: 0x06003268 RID: 12904 RVA: 0x00209345 File Offset: 0x00207545
		public bool HasJobItem(Func<JobPrefab.JobItem, bool> predicate)
		{
			return this.prefab.HasJobItem(this.Variant, predicate);
		}

		// Token: 0x06003269 RID: 12905 RVA: 0x0020935C File Offset: 0x0020755C
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

		// Token: 0x0600326A RID: 12906 RVA: 0x0020943C File Offset: 0x0020763C
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

		// Token: 0x0600326B RID: 12907 RVA: 0x002096A0 File Offset: 0x002078A0
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

		// Token: 0x04001A6B RID: 6763
		private readonly JobPrefab prefab;

		// Token: 0x04001A6C RID: 6764
		private readonly Dictionary<Identifier, Skill> skills;

		// Token: 0x04001A6D RID: 6765
		public int Variant;
	}
}
