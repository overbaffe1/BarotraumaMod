using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200021C RID: 540
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class AfflictionsFile : ContentFile
	{
		// Token: 0x06003686 RID: 13958 RVA: 0x00212CD8 File Offset: 0x00210ED8
		public AfflictionsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003687 RID: 13959 RVA: 0x00212CE4 File Offset: 0x00210EE4
		private void ParseElement(ContentXElement element, bool overriding)
		{
			Identifier elementName = element.NameAsIdentifier();
			if (element.IsOverride())
			{
				element.Elements().ForEach(delegate(ContentXElement s)
				{
					this.ParseElement(s, true);
				});
				return;
			}
			if (elementName == "Afflictions")
			{
				element.Elements().ForEach(delegate(ContentXElement s)
				{
					this.ParseElement(s, overriding);
				});
				return;
			}
			if (elementName == "cprsettings")
			{
				CPRSettings cprSettings = new CPRSettings(element, this);
				CPRSettings.Prefabs.Add(cprSettings, overriding);
				return;
			}
			if (elementName == "damageoverlay")
			{
				CharacterHealth.DamageOverlayPrefab damageOverlay = new CharacterHealth.DamageOverlayPrefab(element, this);
				CharacterHealth.DamageOverlayPrefab.Prefabs.Add(damageOverlay, overriding);
				return;
			}
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("No identifier defined for the affliction '");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(elementName);
				defaultInterpolatedStringHandler.AppendLiteral("' in file '");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, (element != null) ? element.ContentPackage : null, false, false);
				return;
			}
			AfflictionPrefab existingAffliction;
			if (AfflictionPrefab.Prefabs.TryGet(identifier, out existingAffliction))
			{
				if (!overriding)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(171, 4);
					defaultInterpolatedStringHandler2.AppendLiteral("Duplicate affliction: '");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("' defined in ");
					defaultInterpolatedStringHandler2.AppendFormatted(element.ContentPackage.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" is already defined in the previously loaded content package ");
					defaultInterpolatedStringHandler2.AppendFormatted(existingAffliction.ContentPackage.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					defaultInterpolatedStringHandler2.AppendLiteral(" You may need to adjust the mod load order to make sure ");
					defaultInterpolatedStringHandler2.AppendFormatted(element.ContentPackage.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" is loaded first.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, (element != null) ? element.ContentPackage : null, false, false);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(81, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Overriding an affliction or a buff with the identifier '");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler3.AppendLiteral("' using the version in '");
				defaultInterpolatedStringHandler3.AppendFormatted(element.ContentPackage.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.MediumPurple), false);
			}
			Type type = AfflictionsFile.afflictionTypes.FirstOrDefault(delegate(Type t)
			{
				if (!(t.Name == elementName))
				{
					string name = t.Name;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("Affliction");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(elementName);
					Identifier identifier2 = defaultInterpolatedStringHandler4.ToStringAndClear().ToIdentifier();
					return name == identifier2;
				}
				return true;
			}) ?? typeof(Affliction);
			AfflictionPrefab prefab = this.CreatePrefab(element, type);
			AfflictionPrefab.Prefabs.Add(prefab, overriding);
		}

		// Token: 0x06003688 RID: 13960 RVA: 0x00212FA8 File Offset: 0x002111A8
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (((doc != null) ? doc.Root : null) == null)
			{
				return;
			}
			this.ParseElement(doc.Root.FromPackage(this.ContentPackage), false);
		}

		// Token: 0x06003689 RID: 13961 RVA: 0x00212FE8 File Offset: 0x002111E8
		private AfflictionPrefab CreatePrefab(ContentXElement element, Type type)
		{
			if (type == typeof(AfflictionHusk))
			{
				return new AfflictionPrefabHusk(element, this, type);
			}
			return new AfflictionPrefab(element, this, type);
		}

		// Token: 0x0600368A RID: 13962 RVA: 0x0021300D File Offset: 0x0021120D
		public override void UnloadFile()
		{
			CharacterHealth.DamageOverlayPrefab.Prefabs.RemoveByFile(this, null);
			CPRSettings.Prefabs.RemoveByFile(this, null);
			AfflictionPrefab.Prefabs.RemoveByFile(this);
		}

		// Token: 0x0600368B RID: 13963 RVA: 0x00213032 File Offset: 0x00211232
		public override void Sort()
		{
			CharacterHealth.DamageOverlayPrefab.Prefabs.Sort();
			CPRSettings.Prefabs.Sort();
			AfflictionPrefab.Prefabs.SortAll();
		}

		// Token: 0x04001C09 RID: 7177
		private static readonly ImmutableHashSet<Type> afflictionTypes = ReflectionUtils.GetDerivedNonAbstract<Affliction>().ToImmutableHashSet<Type>();
	}
}
