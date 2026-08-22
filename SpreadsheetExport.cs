using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000156 RID: 342
	[NullableContext(1)]
	[Nullable(0)]
	public class SpreadsheetExport
	{
		// Token: 0x06002A22 RID: 10786 RVA: 0x001D1A68 File Offset: 0x001CFC68
		public static void Export()
		{
			XDocument doc = new XDocument();
			if (doc.Root == null)
			{
				doc.Add(new XElement("Content"));
			}
			XElement root = doc.Root;
			foreach (ItemPrefab prefab in ItemPrefab.Prefabs)
			{
				XName name = "Item";
				object[] array = new object[4];
				array[0] = new XAttribute("identifier", prefab.Identifier);
				array[1] = new XAttribute("name", prefab.Name);
				array[2] = new XAttribute("tags", SpreadsheetExport.FormatArray<Identifier>(prefab.Tags));
				int num = 3;
				XName name2 = "value";
				PriceInfo defaultPrice = prefab.DefaultPrice;
				array[num] = new XAttribute(name2, (defaultPrice != null) ? defaultPrice.Price : 0);
				XElement itemElement = new XElement(name, array);
				itemElement.Add(SpreadsheetExport.ParseRecipe(prefab));
				itemElement.Add(SpreadsheetExport.ParseDecon(prefab));
				itemElement.Add(SpreadsheetExport.ParseMedical(prefab));
				itemElement.Add(SpreadsheetExport.ParseWeapon(prefab));
				root.Add(itemElement);
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = false,
				NewLineOnAttributes = false
			};
			using (XmlWriter writer = XmlWriter.Create("spreadsheetdata.xml", settings))
			{
				doc.SaveSafe(writer);
			}
		}

		// Token: 0x06002A23 RID: 10787 RVA: 0x001D1BF8 File Offset: 0x001CFDF8
		private static XElement ParseRecipe(ItemPrefab prefab)
		{
			FabricationRecipe recipe = prefab.FabricationRecipes.Values.FirstOrDefault<FabricationRecipe>();
			List<ItemPrefab> list;
			if (recipe == null)
			{
				list = null;
			}
			else
			{
				list = recipe.RequiredItems.SelectMany((FabricationRecipe.RequiredItem ri) => ri.ItemPrefabs).Distinct<ItemPrefab>().ToList<ItemPrefab>();
			}
			List<ItemPrefab> ingredients = list ?? new List<ItemPrefab>();
			Skill skill = (recipe != null) ? recipe.RequiredSkills.FirstOrDefault<Skill>() : null;
			XName name = "Recipe";
			object[] array = new object[6];
			array[0] = new XAttribute("amount", (recipe != null) ? recipe.Amount : 0);
			array[1] = new XAttribute("time", (recipe != null) ? recipe.RequiredTime : 0f);
			array[2] = new XAttribute("skillname", ((skill != null) ? skill.Identifier.Value : null) ?? "");
			int num = 3;
			XName name2 = "skillamount";
			float? num2 = (skill != null) ? new float?(skill.Level) : null;
			array[num] = new XAttribute(name2, ((num2 != null) ? new int?((int)num2.GetValueOrDefault()) : null).GetValueOrDefault());
			array[4] = new XAttribute("ingredients", SpreadsheetExport.FormatArray<LocalizedString>(from ip in ingredients
			select ip.Name));
			array[5] = new XAttribute("values", SpreadsheetExport.FormatArray<int>(ingredients.Select(delegate(ItemPrefab ip)
			{
				PriceInfo defaultPrice = ip.DefaultPrice;
				if (defaultPrice == null)
				{
					return 0;
				}
				return defaultPrice.Price;
			})));
			return new XElement(name, array);
		}

		// Token: 0x06002A24 RID: 10788 RVA: 0x001D1DD4 File Offset: 0x001CFFD4
		private static XElement ParseDecon(ItemPrefab prefab)
		{
			List<ItemPrefab> deconOutput = (from item in prefab.DeconstructItems
			select ItemPrefab.Find(null, item.ItemIdentifier) into outputPrefab
			where outputPrefab != null
			select outputPrefab).ToList<ItemPrefab>();
			XName name = "Deconstruct";
			object[] array = new object[3];
			array[0] = new XAttribute("time", prefab.DeconstructTime);
			array[1] = new XAttribute("outputs", SpreadsheetExport.FormatArray<LocalizedString>(from ip in deconOutput
			select ip.Name));
			array[2] = new XAttribute("values", SpreadsheetExport.FormatArray<int>(deconOutput.Select(delegate(ItemPrefab ip)
			{
				PriceInfo defaultPrice = ip.DefaultPrice;
				if (defaultPrice == null)
				{
					return 0;
				}
				return defaultPrice.Price;
			})));
			return new XElement(name, array);
		}

		// Token: 0x06002A25 RID: 10789 RVA: 0x001D1EE0 File Offset: 0x001D00E0
		private static XElement ParseMedical(ItemPrefab prefab)
		{
			ContentXElement itemMeleeWeapon = prefab.ConfigElement.GetChildElement("MeleeWeapon");
			List<ValueTuple<LocalizedString, float, float>> onSuccessAfflictions = new List<ValueTuple<LocalizedString, float, float>>();
			List<ValueTuple<LocalizedString, float, float>> onFailureAfflictions = new List<ValueTuple<LocalizedString, float, float>>();
			int medicalRequiredSkill = 0;
			ContentXElement contentXElement = null;
			if (itemMeleeWeapon != contentXElement)
			{
				List<StatusEffect> statusEffects = new List<StatusEffect>();
				foreach (ContentXElement subElement in itemMeleeWeapon.Elements())
				{
					string name = subElement.Name.ToString();
					Skill skill;
					if (name.Equals("StatusEffect", StringComparison.OrdinalIgnoreCase))
					{
						StatusEffect statusEffect = StatusEffect.Load(subElement, "_spreadsheet");
						if (statusEffect != null && statusEffect.HasTag(Tags.MedicalItem))
						{
							statusEffects.Add(statusEffect);
						}
					}
					else if (SpreadsheetExport.IsRequiredSkill(subElement, out skill) && skill != null)
					{
						medicalRequiredSkill = (int)skill.Level;
					}
				}
				List<StatusEffect> successEffects = (from se in statusEffects
				where se.type == ActionType.OnSuccess
				select se).ToList<StatusEffect>();
				List<StatusEffect> failureEffects = (from se in statusEffects
				where se.type == ActionType.OnFailure
				select se).ToList<StatusEffect>();
				foreach (StatusEffect statusEffect2 in successEffects)
				{
					float duration = statusEffect2.Duration;
					onSuccessAfflictions.AddRange(from ra in statusEffect2.ReduceAffliction
					select new ValueTuple<LocalizedString, float, float>(SpreadsheetExport.GetAfflictionName(ra.Item1), -ra.Item2, duration));
					onSuccessAfflictions.AddRange(from affliction in statusEffect2.Afflictions
					select new ValueTuple<LocalizedString, float, float>(affliction.Prefab.Name, affliction.NonClampedStrength, duration));
				}
				foreach (StatusEffect statusEffect3 in failureEffects)
				{
					float duration = statusEffect3.Duration;
					onFailureAfflictions.AddRange(from ra in statusEffect3.ReduceAffliction
					select new ValueTuple<LocalizedString, float, float>(SpreadsheetExport.GetAfflictionName(ra.Item1), -ra.Item2, duration));
					onFailureAfflictions.AddRange(from affliction in statusEffect3.Afflictions
					select new ValueTuple<LocalizedString, float, float>(affliction.Prefab.Name, affliction.NonClampedStrength, duration));
				}
			}
			XName name2 = "Medical";
			object[] array = new object[7];
			array[0] = new XAttribute("skillamount", medicalRequiredSkill);
			array[1] = new XAttribute("successafflictions", SpreadsheetExport.FormatArray<LocalizedString>(from tpl in onSuccessAfflictions
			select tpl.Item1));
			array[2] = new XAttribute("successamounts", SpreadsheetExport.FormatArray<string>(from tpl in onSuccessAfflictions
			select SpreadsheetExport.FormatFloat(tpl.Item2)));
			array[3] = new XAttribute("successdurations", SpreadsheetExport.FormatArray<string>(from tpl in onSuccessAfflictions
			select SpreadsheetExport.FormatFloat(tpl.Item3)));
			array[4] = new XAttribute("failureafflictions", SpreadsheetExport.FormatArray<LocalizedString>(from tpl in onFailureAfflictions
			select tpl.Item1));
			array[5] = new XAttribute("failureamounts", SpreadsheetExport.FormatArray<string>(from tpl in onFailureAfflictions
			select SpreadsheetExport.FormatFloat(tpl.Item2)));
			array[6] = new XAttribute("failuredurations", SpreadsheetExport.FormatArray<string>(from tpl in onFailureAfflictions
			select SpreadsheetExport.FormatFloat(tpl.Item3)));
			return new XElement(name2, array);
		}

		// Token: 0x06002A26 RID: 10790 RVA: 0x001D22CC File Offset: 0x001D04CC
		private static XElement ParseWeapon(ItemPrefab prefab)
		{
			SpreadsheetExport.<>c__DisplayClass6_0 CS$<>8__locals1;
			CS$<>8__locals1.stun = 0f;
			CS$<>8__locals1.isAoE = false;
			CS$<>8__locals1.structDamage = null;
			int skillRequirement = 0;
			CS$<>8__locals1.damages = new List<ValueTuple<LocalizedString, float>>();
			string[] validNames = new string[]
			{
				"Projectile",
				"MeleeWeapon",
				"RepairTool",
				"ItemComponent",
				"RangedWeapon"
			};
			foreach (ContentXElement icElement in prefab.ConfigElement.Elements())
			{
				string icName = icElement.Name.ToString();
				if (validNames.Any((string name) => icName.Equals(name, StringComparison.OrdinalIgnoreCase)))
				{
					foreach (ContentXElement icChildElement in icElement.Elements())
					{
						string name3 = icChildElement.Name.ToString();
						Skill skill;
						if (SpreadsheetExport.IsRequiredSkill(icChildElement, out skill) && skill != null)
						{
							skillRequirement = (int)skill.Level;
						}
						else if (name3.Equals("Attack", StringComparison.OrdinalIgnoreCase))
						{
							SpreadsheetExport.<ParseWeapon>g__ParseAttack|6_5(new Attack(icChildElement, "_spreadsheet"), ref CS$<>8__locals1);
						}
						else if (name3.Equals("Explosion", StringComparison.OrdinalIgnoreCase))
						{
							SpreadsheetExport.<ParseWeapon>g__ParseExplosion|6_4(new Explosion[]
							{
								new Explosion(icChildElement, "_spreadsheet")
							}, ref CS$<>8__locals1);
						}
						else if (name3.Equals("StatusEffect", StringComparison.OrdinalIgnoreCase))
						{
							SpreadsheetExport.<ParseWeapon>g__ParseStatusEffect|6_3(new StatusEffect[]
							{
								StatusEffect.Load(icChildElement, "_spreadsheet")
							}, ref CS$<>8__locals1);
						}
					}
				}
			}
			XName name2 = "Weapon";
			object[] array = new object[6];
			array[0] = new XAttribute("damagenames", SpreadsheetExport.FormatArray<LocalizedString>(from tpl in CS$<>8__locals1.damages
			select tpl.Item1));
			array[1] = new XAttribute("damageamounts", SpreadsheetExport.FormatArray<string>(from tpl in CS$<>8__locals1.damages
			select SpreadsheetExport.FormatFloat(tpl.Item2)));
			array[2] = new XAttribute("isaoe", CS$<>8__locals1.isAoE);
			array[3] = new XAttribute("structuredamage", CS$<>8__locals1.structDamage.GetValueOrDefault());
			array[4] = new XAttribute("stun", SpreadsheetExport.FormatFloat(CS$<>8__locals1.stun));
			array[5] = new XAttribute("skillrequirement", skillRequirement);
			return new XElement(name2, array);
		}

		// Token: 0x06002A27 RID: 10791 RVA: 0x001D25B8 File Offset: 0x001D07B8
		private static LocalizedString GetAfflictionName(Identifier identifier)
		{
			AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab prefab) => prefab.Identifier == identifier);
			return ((afflictionPrefab != null) ? afflictionPrefab.Name : null) ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(identifier.Value.ToLower());
		}

		// Token: 0x06002A28 RID: 10792 RVA: 0x001D261C File Offset: 0x001D081C
		private static string FormatFloat(float value)
		{
			return value.ToString("0.00", CultureInfo.InvariantCulture);
		}

		// Token: 0x06002A29 RID: 10793 RVA: 0x001D262F File Offset: 0x001D082F
		private static string FormatArray<[Nullable(2)] T>(IEnumerable<T> array)
		{
			return string.Join<T>(',', array);
		}

		// Token: 0x06002A2A RID: 10794 RVA: 0x001D263C File Offset: 0x001D083C
		private static bool IsRequiredSkill(ContentXElement element, [Nullable(2)] out Skill skill)
		{
			string name = element.Name.ToString();
			bool isSkill = name.Equals("RequiredSkill", StringComparison.OrdinalIgnoreCase) || name.Equals("RequiredSkills", StringComparison.OrdinalIgnoreCase);
			if (isSkill)
			{
				Identifier identifier = element.GetAttributeIdentifier("Identifier", Identifier.Empty);
				float level = element.GetAttributeFloat("Level".ToLowerInvariant(), 0f);
				skill = new Skill(identifier, level);
			}
			else
			{
				skill = null;
			}
			return isSkill;
		}

		// Token: 0x06002A2C RID: 10796 RVA: 0x001D26B4 File Offset: 0x001D08B4
		[CompilerGenerated]
		internal static void <ParseWeapon>g__ParseStatusEffect|6_3(IEnumerable<StatusEffect> statusEffects, ref SpreadsheetExport.<>c__DisplayClass6_0 A_1)
		{
			foreach (StatusEffect effect in statusEffects)
			{
				if (!effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					SpreadsheetExport.<ParseWeapon>g__ParseAfflictions|6_6(effect.Afflictions, ref A_1);
					SpreadsheetExport.<ParseWeapon>g__ParseExplosion|6_4(effect.Explosions, ref A_1);
				}
			}
		}

		// Token: 0x06002A2D RID: 10797 RVA: 0x001D2718 File Offset: 0x001D0918
		[CompilerGenerated]
		internal static void <ParseWeapon>g__ParseExplosion|6_4(IEnumerable<Explosion> explosions, ref SpreadsheetExport.<>c__DisplayClass6_0 A_1)
		{
			foreach (Explosion explosion in explosions)
			{
				A_1.isAoE = true;
				SpreadsheetExport.<ParseWeapon>g__ParseAttack|6_5(explosion.Attack, ref A_1);
				SpreadsheetExport.<ParseWeapon>g__ParseStatusEffect|6_3(explosion.Attack.StatusEffects, ref A_1);
			}
		}

		// Token: 0x06002A2E RID: 10798 RVA: 0x001D2780 File Offset: 0x001D0980
		[CompilerGenerated]
		internal static void <ParseWeapon>g__ParseAttack|6_5(Attack attack, ref SpreadsheetExport.<>c__DisplayClass6_0 A_1)
		{
			float value = A_1.structDamage.GetValueOrDefault();
			if (A_1.structDamage == null)
			{
				value = attack.StructureDamage;
				A_1.structDamage = new float?(value);
			}
			SpreadsheetExport.<ParseWeapon>g__ParseAfflictions|6_6(attack.Afflictions.Keys, ref A_1);
			SpreadsheetExport.<ParseWeapon>g__ParseStatusEffect|6_3(attack.StatusEffects, ref A_1);
		}

		// Token: 0x06002A2F RID: 10799 RVA: 0x001D27D8 File Offset: 0x001D09D8
		[CompilerGenerated]
		internal static void <ParseWeapon>g__ParseAfflictions|6_6(IEnumerable<Affliction> afflictions, ref SpreadsheetExport.<>c__DisplayClass6_0 A_1)
		{
			foreach (Affliction affliction in afflictions)
			{
				if (affliction.Prefab == AfflictionPrefab.Stun)
				{
					A_1.stun += affliction.NonClampedStrength;
				}
				else
				{
					A_1.damages.Add(new ValueTuple<LocalizedString, float>(affliction.Prefab.Name, affliction.NonClampedStrength));
				}
			}
		}

		// Token: 0x0400160D RID: 5645
		private const char separator = ',';

		// Token: 0x0400160E RID: 5646
		private const string debugIdentifier = "_spreadsheet";
	}
}
