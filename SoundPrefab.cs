using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Sounds;

namespace Barotrauma
{
	// Token: 0x02000131 RID: 305
	internal class SoundPrefab : Prefab
	{
		// Token: 0x17000A64 RID: 2660
		// (get) Token: 0x06002883 RID: 10371 RVA: 0x001C360C File Offset: 0x001C180C
		public static IReadOnlyList<SoundPrefab> FlowSounds
		{
			get
			{
				return SoundPrefab.flowSounds;
			}
		}

		// Token: 0x17000A65 RID: 2661
		// (get) Token: 0x06002884 RID: 10372 RVA: 0x001C3613 File Offset: 0x001C1813
		public static IReadOnlyList<SoundPrefab> SplashSounds
		{
			get
			{
				return SoundPrefab.splashSounds;
			}
		}

		// Token: 0x06002885 RID: 10373 RVA: 0x001C361C File Offset: 0x001C181C
		static SoundPrefab()
		{
			IEnumerable<Type> types = ReflectionUtils.GetDerivedNonAbstract<SoundPrefab>();
			SoundPrefab.TagToDerivedPrefab = types.SelectMany((Type t) => from n in t.GetCustomAttribute<TagNames>().Names
			select new ValueTuple<Identifier, Type>(n, t)).ToImmutableDictionary<Identifier, Type>();
			SoundPrefab.derivedPrefabCollections = (from t in types
			select new ValueTuple<Type, SoundPrefab.PrefabCollectionHandler>(t, new SoundPrefab.PrefabCollectionHandler(t))).ToImmutableDictionary<Type, SoundPrefab.PrefabCollectionHandler>();
			IEnumerable<FieldInfo> prefabSelectorFields = from f in typeof(SoundPrefab).GetFields(BindingFlags.Static | BindingFlags.Public)
			where f.FieldType == typeof(PrefabSelector<SoundPrefab>)
			select f;
			SoundPrefab.prefabSelectors = (from f in prefabSelectorFields
			select new ValueTuple<Identifier, PrefabSelector<SoundPrefab>>(f.Name.ToIdentifier(), (PrefabSelector<SoundPrefab>)f.GetValue(null))).ToImmutableDictionary<Identifier, PrefabSelector<SoundPrefab>>();
			IEnumerable<FieldInfo> prefabsOfTagName = from f in typeof(SoundPrefab).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
			where f.FieldType == typeof(List<SoundPrefab>)
			select f;
			SoundPrefab.prefabsWithTag = (from f in prefabsOfTagName
			select new ValueTuple<Identifier, List<SoundPrefab>>(f.Name.Substring(0, f.Name.Length - 6).ToIdentifier(), (List<SoundPrefab>)f.GetValue(null))).ToImmutableDictionary<Identifier, List<SoundPrefab>>();
			SoundPrefab.Prefabs = new PrefabCollection<SoundPrefab>(delegate(SoundPrefab p, bool isOverride)
			{
				if (SoundPrefab.derivedPrefabCollections.ContainsKey(p.GetType()))
				{
					SoundPrefab.derivedPrefabCollections[p.GetType()].Add(p, isOverride);
				}
				if (SoundPrefab.prefabSelectors.ContainsKey(p.ElementName))
				{
					SoundPrefab.prefabSelectors[p.ElementName].Add(p, isOverride);
				}
				SoundPrefab.UpdateSoundsWithTag();
			}, delegate(SoundPrefab p)
			{
				if (SoundPrefab.derivedPrefabCollections.ContainsKey(p.GetType()))
				{
					SoundPrefab.derivedPrefabCollections[p.GetType()].Remove(p);
				}
				if (SoundPrefab.prefabSelectors.ContainsKey(p.ElementName))
				{
					SoundPrefab.prefabSelectors[p.ElementName].RemoveIfContains(p);
				}
				SoundPrefab.UpdateSoundsWithTag();
				SoundPlayer.DisposeDisabledMusic();
			}, delegate()
			{
				SoundPrefab.derivedPrefabCollections.Values.ForEach(delegate(SoundPrefab.PrefabCollectionHandler h)
				{
					h.SortAll();
				});
				SoundPrefab.prefabSelectors.Values.ForEach(delegate(PrefabSelector<SoundPrefab> h)
				{
					h.Sort();
				});
			}, delegate(ContentFile file)
			{
				SoundPrefab.derivedPrefabCollections.Values.ForEach(delegate(SoundPrefab.PrefabCollectionHandler h)
				{
					h.AddOverrideFile(file);
				});
			}, delegate(ContentFile file)
			{
				SoundPrefab.derivedPrefabCollections.Values.ForEach(delegate(SoundPrefab.PrefabCollectionHandler h)
				{
					h.RemoveOverrideFile(file);
				});
			});
		}

		// Token: 0x06002886 RID: 10374 RVA: 0x001C3794 File Offset: 0x001C1994
		private static void UpdateSoundsWithTag()
		{
			using (IEnumerator<Identifier> enumerator = SoundPrefab.prefabsWithTag.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier tag = enumerator.Current;
					List<SoundPrefab> list = SoundPrefab.prefabsWithTag[tag];
					list.Clear();
					list.AddRange(from p in SoundPrefab.Prefabs
					where p.ElementName == tag
					select p);
					list.Sort(delegate(SoundPrefab p1, SoundPrefab p2)
					{
						if (p1.ContentFile.ContentPackage.Index < p2.ContentFile.ContentPackage.Index)
						{
							return -1;
						}
						if (p1.ContentFile.ContentPackage.Index > p2.ContentFile.ContentPackage.Index)
						{
							return 1;
						}
						if (p2.Element.ComesAfter(p1.Element))
						{
							return -1;
						}
						if (p1.Element.ComesAfter(p2.Element))
						{
							return 1;
						}
						return 0;
					});
				}
			}
		}

		// Token: 0x06002887 RID: 10375 RVA: 0x001C3844 File Offset: 0x001C1A44
		protected override Identifier DetermineIdentifier(XElement element)
		{
			Identifier id = base.DetermineIdentifier(element);
			if (id.IsEmpty)
			{
				if (id.IsEmpty)
				{
					id = Path.GetFileNameWithoutExtension(element.GetAttributeStringUnrestricted("path", "")).ToIdentifier();
				}
				if (id.IsEmpty)
				{
					id = Path.GetFileNameWithoutExtension(element.GetAttributeStringUnrestricted("file", "")).ToIdentifier();
				}
				if (!id.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted<XName>(element.Name);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(id);
					id = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
					string damageSoundType = element.GetAttributeString("damagesoundtype", "");
					if (!damageSoundType.IsNullOrEmpty())
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(id);
						defaultInterpolatedStringHandler2.AppendLiteral("_");
						defaultInterpolatedStringHandler2.AppendFormatted(damageSoundType);
						id = defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier();
					}
					string musicType = element.GetAttributeString("type", "");
					if (!musicType.IsNullOrEmpty())
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(id);
						defaultInterpolatedStringHandler3.AppendLiteral("_");
						defaultInterpolatedStringHandler3.AppendFormatted(musicType);
						id = defaultInterpolatedStringHandler3.ToStringAndClear().ToIdentifier();
					}
				}
			}
			return id;
		}

		// Token: 0x17000A66 RID: 2662
		// (get) Token: 0x06002888 RID: 10376 RVA: 0x001C3987 File Offset: 0x001C1B87
		// (set) Token: 0x06002889 RID: 10377 RVA: 0x001C398F File Offset: 0x001C1B8F
		public Sound Sound { get; private set; }

		// Token: 0x0600288A RID: 10378 RVA: 0x001C3998 File Offset: 0x001C1B98
		public SoundPrefab(ContentXElement element, SoundsFile file, bool stream = false) : base(file, element)
		{
			this.SoundPath = (element.GetAttributeContentPath("file") ?? ContentPath.Empty);
			this.Element = element;
			this.ElementName = element.NameAsIdentifier();
			this.Sound = GameMain.SoundManager.LoadSound(element, stream, null);
			this.Volume = element.GetAttributeFloat("Volume", 1f);
		}

		// Token: 0x0600288B RID: 10379 RVA: 0x001C3A03 File Offset: 0x001C1C03
		public bool IsPlaying()
		{
			return this.Sound.IsPlaying();
		}

		// Token: 0x0600288C RID: 10380 RVA: 0x001C3A10 File Offset: 0x001C1C10
		public override void Dispose()
		{
			Sound sound = this.Sound;
			if (sound != null)
			{
				sound.Dispose();
			}
			this.Sound = null;
		}

		// Token: 0x0400149D RID: 5277
		public static readonly PrefabSelector<SoundPrefab> WaterAmbienceIn = new PrefabSelector<SoundPrefab>();

		// Token: 0x0400149E RID: 5278
		public static readonly PrefabSelector<SoundPrefab> WaterAmbienceOut = new PrefabSelector<SoundPrefab>();

		// Token: 0x0400149F RID: 5279
		public static readonly PrefabSelector<SoundPrefab> WaterAmbienceMoving = new PrefabSelector<SoundPrefab>();

		// Token: 0x040014A0 RID: 5280
		public static readonly PrefabSelector<SoundPrefab> StartupSound = new PrefabSelector<SoundPrefab>();

		// Token: 0x040014A1 RID: 5281
		private static readonly List<SoundPrefab> flowSounds = new List<SoundPrefab>();

		// Token: 0x040014A2 RID: 5282
		private static readonly List<SoundPrefab> splashSounds = new List<SoundPrefab>();

		// Token: 0x040014A3 RID: 5283
		public static readonly ImmutableDictionary<Identifier, Type> TagToDerivedPrefab;

		// Token: 0x040014A4 RID: 5284
		private static readonly ImmutableDictionary<Type, SoundPrefab.PrefabCollectionHandler> derivedPrefabCollections;

		// Token: 0x040014A5 RID: 5285
		private static readonly ImmutableDictionary<Identifier, PrefabSelector<SoundPrefab>> prefabSelectors;

		// Token: 0x040014A6 RID: 5286
		private static readonly ImmutableDictionary<Identifier, List<SoundPrefab>> prefabsWithTag;

		// Token: 0x040014A7 RID: 5287
		public static readonly PrefabCollection<SoundPrefab> Prefabs;

		// Token: 0x040014A8 RID: 5288
		public readonly ContentPath SoundPath;

		// Token: 0x040014A9 RID: 5289
		public readonly ContentXElement Element;

		// Token: 0x040014AA RID: 5290
		public readonly Identifier ElementName;

		// Token: 0x040014AB RID: 5291
		public readonly float Volume;

		// Token: 0x02000D77 RID: 3447
		private class PrefabCollectionHandler
		{
			// Token: 0x06008127 RID: 33063 RVA: 0x00397740 File Offset: 0x00395940
			public void Add(SoundPrefab p, bool isOverride)
			{
				this.AddMethod.Invoke(this.Collection, new object[]
				{
					p,
					isOverride
				});
			}

			// Token: 0x06008128 RID: 33064 RVA: 0x00397767 File Offset: 0x00395967
			public void Remove(SoundPrefab p)
			{
				this.RemoveMethod.Invoke(this.Collection, new object[]
				{
					p
				});
			}

			// Token: 0x06008129 RID: 33065 RVA: 0x00397785 File Offset: 0x00395985
			public void AddOverrideFile(ContentFile file)
			{
				this.AddOverrideFileMethod.Invoke(this.Collection, new object[]
				{
					file
				});
			}

			// Token: 0x0600812A RID: 33066 RVA: 0x003977A3 File Offset: 0x003959A3
			public void RemoveOverrideFile(ContentFile file)
			{
				this.RemoveOverrideFileMethod.Invoke(this.Collection, new object[]
				{
					file
				});
			}

			// Token: 0x0600812B RID: 33067 RVA: 0x003977C1 File Offset: 0x003959C1
			public void SortAll()
			{
				this.SortAllMethod.Invoke(this.Collection, null);
			}

			// Token: 0x0600812C RID: 33068 RVA: 0x003977D8 File Offset: 0x003959D8
			public PrefabCollectionHandler(Type type)
			{
				FieldInfo collectionField = type.GetField(type.Name + "Prefabs", BindingFlags.Static | BindingFlags.Public);
				if (collectionField == null)
				{
					throw new InvalidOperationException("Couldn't determine PrefabCollection for " + type.Name);
				}
				object value = collectionField.GetValue(null);
				if (value == null)
				{
					throw new InvalidOperationException("PrefabCollection for " + type.Name + " was null");
				}
				this.Collection = value;
				this.AddMethod = this.Collection.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
				this.RemoveMethod = this.Collection.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public);
				this.AddOverrideFileMethod = this.Collection.GetType().GetMethod("AddOverrideFile", BindingFlags.Instance | BindingFlags.Public);
				this.RemoveOverrideFileMethod = this.Collection.GetType().GetMethod("RemoveOverrideFile", BindingFlags.Instance | BindingFlags.Public);
				this.SortAllMethod = this.Collection.GetType().GetMethod("SortAll", BindingFlags.Instance | BindingFlags.Public);
			}

			// Token: 0x04004F6D RID: 20333
			public readonly object Collection;

			// Token: 0x04004F6E RID: 20334
			public readonly MethodInfo AddMethod;

			// Token: 0x04004F6F RID: 20335
			public readonly MethodInfo RemoveMethod;

			// Token: 0x04004F70 RID: 20336
			public readonly MethodInfo SortAllMethod;

			// Token: 0x04004F71 RID: 20337
			public readonly MethodInfo AddOverrideFileMethod;

			// Token: 0x04004F72 RID: 20338
			public readonly MethodInfo RemoveOverrideFileMethod;
		}
	}
}
