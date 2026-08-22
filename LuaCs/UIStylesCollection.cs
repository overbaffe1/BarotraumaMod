using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.LuaCs.Data;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E7 RID: 1255
	public class UIStylesCollection : HashlessFile, IUIStylesCollection, IService, IDisposable
	{
		// Token: 0x060051EB RID: 20971 RVA: 0x002C0274 File Offset: 0x002BE474
		public UIStylesCollection(ContentPath path, IStorageService storageService) : base(path.ContentPackage, path)
		{
			Guard.IsNotNull<ContentPath>(path, "path");
			Guard.IsNotNull<ContentPackage>(path.ContentPackage, "ContentPackage");
			this._storageService = storageService;
			this._fakeFile = new UIStyleFile(path.ContentPackage, path);
		}

		// Token: 0x170014D4 RID: 5332
		// (get) Token: 0x060051EC RID: 20972 RVA: 0x002C0304 File Offset: 0x002BE504
		public new ContentPath Path
		{
			get
			{
				return this.Path;
			}
		}

		// Token: 0x060051ED RID: 20973 RVA: 0x002C030C File Offset: 0x002BE50C
		public Result<GUIFont> GetFont(string name)
		{
			Result<GUIFont> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				GUIFont asset;
				if (this._fonts.TryGetValue(name, out asset))
				{
					result = asset;
				}
				else
				{
					result = Result.Fail("GetFont: Failed to find the font with the name '" + name + "'");
				}
			}
			return result;
		}

		// Token: 0x060051EE RID: 20974 RVA: 0x002C03A0 File Offset: 0x002BE5A0
		public Result<GUISprite> GetSprite(string name)
		{
			Result<GUISprite> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				GUISprite asset;
				if (this._sprites.TryGetValue(name, out asset))
				{
					result = asset;
				}
				else
				{
					result = Result.Fail("GetSprite: Failed to find the sprite with the name '" + name + "'");
				}
			}
			return result;
		}

		// Token: 0x060051EF RID: 20975 RVA: 0x002C0434 File Offset: 0x002BE634
		public Result<GUISpriteSheet> GetSpriteSheet(string name)
		{
			Result<GUISpriteSheet> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				GUISpriteSheet asset;
				if (this._spriteSheets.TryGetValue(name, out asset))
				{
					result = asset;
				}
				else
				{
					result = Result.Fail("GetSpriteSheet: Failed to find the spritesheet with the name '" + name + "'");
				}
			}
			return result;
		}

		// Token: 0x060051F0 RID: 20976 RVA: 0x002C04C8 File Offset: 0x002BE6C8
		public Result<GUICursor> GetCursor(string name)
		{
			Result<GUICursor> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				GUICursor asset;
				if (this._cursors.TryGetValue(name, out asset))
				{
					result = asset;
				}
				else
				{
					result = Result.Fail("GetCursor: Failed to find the cursor with the name '" + name + "'");
				}
			}
			return result;
		}

		// Token: 0x060051F1 RID: 20977 RVA: 0x002C055C File Offset: 0x002BE75C
		public Result<GUIColor> GetColor(string name)
		{
			Result<GUIColor> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				GUIColor asset;
				if (this._colors.TryGetValue(name, out asset))
				{
					result = asset;
				}
				else
				{
					result = Result.Fail("GetColor: Failed to find the color with the name '" + name + "'");
				}
			}
			return result;
		}

		// Token: 0x060051F2 RID: 20978 RVA: 0x002C05F0 File Offset: 0x002BE7F0
		public override void LoadFile()
		{
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<XDocument> result = this._storageService.LoadPackageXml(this.Path);
				if (result == null || !result.IsSuccess)
				{
					DebugConsole.LogError("Failed to load xml from " + this.Path.FullPath + ".", null, null);
					ThrowHelper.ThrowArgumentException("Failed to load xml from " + this.Path.FullPath + ".");
				}
				else
				{
					XElement root2 = result.Value.Root;
					ContentXElement root = (root2 != null) ? root2.FromPackage(this.Path.ContentPackage) : null;
					if (root != null)
					{
						ContentXElement styleElement = (root.Name.LocalName.ToLowerInvariant() == "style") ? root : root.GetChildElement("style");
						if (styleElement != null)
						{
							IEnumerable<ContentXElement> childElements = styleElement.GetChildElements("Font");
							if (childElements != null)
							{
								UIStylesCollection.<LoadFile>g__AddToList|16_0<GUIFont, GUIFontPrefab>(this._fonts, childElements, this._fakeFile);
							}
							childElements = styleElement.GetChildElements("Sprite");
							if (childElements != null)
							{
								UIStylesCollection.<LoadFile>g__AddToList|16_0<GUISprite, GUISpritePrefab>(this._sprites, childElements, this._fakeFile);
							}
							childElements = styleElement.GetChildElements("Spritesheet");
							if (childElements != null)
							{
								UIStylesCollection.<LoadFile>g__AddToList|16_0<GUISpriteSheet, GUISpriteSheetPrefab>(this._spriteSheets, childElements, this._fakeFile);
							}
							childElements = styleElement.GetChildElements("Cursor");
							if (childElements != null)
							{
								UIStylesCollection.<LoadFile>g__AddToList|16_0<GUICursor, GUICursorPrefab>(this._cursors, childElements, this._fakeFile);
							}
							childElements = styleElement.GetChildElements("Color");
							if (childElements != null)
							{
								UIStylesCollection.<LoadFile>g__AddToList|16_0<GUIColor, GUIColorPrefab>(this._colors, childElements, this._fakeFile);
							}
						}
					}
				}
			}
		}

		// Token: 0x060051F3 RID: 20979 RVA: 0x002C07D8 File Offset: 0x002BE9D8
		public override void UnloadFile()
		{
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fonts.Values.ForEach(delegate(GUIFont p)
				{
					p.Prefabs.RemoveByFile(this._fakeFile, null);
				});
				this._sprites.Values.ForEach(delegate(GUISprite p)
				{
					p.Prefabs.RemoveByFile(this._fakeFile, null);
				});
				this._spriteSheets.Values.ForEach(delegate(GUISpriteSheet p)
				{
					p.Prefabs.RemoveByFile(this._fakeFile, null);
				});
				this._cursors.Values.ForEach(delegate(GUICursor p)
				{
					p.Prefabs.RemoveByFile(this._fakeFile, null);
				});
				this._colors.Values.ForEach(delegate(GUIColor p)
				{
					p.Prefabs.RemoveByFile(this._fakeFile, null);
				});
			}
		}

		// Token: 0x060051F4 RID: 20980 RVA: 0x002C08C0 File Offset: 0x002BEAC0
		public override void Sort()
		{
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fonts.Values.ForEach(delegate(GUIFont p)
				{
					p.Prefabs.Sort();
				});
				this._sprites.Values.ForEach(delegate(GUISprite p)
				{
					p.Prefabs.Sort();
				});
				this._spriteSheets.Values.ForEach(delegate(GUISpriteSheet p)
				{
					p.Prefabs.Sort();
				});
				this._cursors.Values.ForEach(delegate(GUICursor p)
				{
					p.Prefabs.Sort();
				});
				this._colors.Values.ForEach(delegate(GUIColor p)
				{
					p.Prefabs.Sort();
				});
			}
		}

		// Token: 0x060051F5 RID: 20981 RVA: 0x002C0A08 File Offset: 0x002BEC08
		public void Dispose()
		{
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this._fonts.Values.ForEach(delegate(GUIFont p)
					{
						p.Prefabs.RemoveByFile(this._fakeFile, null);
					});
					this._sprites.Values.ForEach(delegate(GUISprite p)
					{
						p.Prefabs.RemoveByFile(this._fakeFile, null);
					});
					this._spriteSheets.Values.ForEach(delegate(GUISpriteSheet p)
					{
						p.Prefabs.RemoveByFile(this._fakeFile, null);
					});
					this._cursors.Values.ForEach(delegate(GUICursor p)
					{
						p.Prefabs.RemoveByFile(this._fakeFile, null);
					});
					this._colors.Values.ForEach(delegate(GUIColor p)
					{
						p.Prefabs.RemoveByFile(this._fakeFile, null);
					});
					this._fonts.Clear();
					this._sprites.Clear();
					this._spriteSheets.Clear();
					this._cursors.Clear();
					this._colors.Clear();
				}
			}
		}

		// Token: 0x170014D5 RID: 5333
		// (get) Token: 0x060051F6 RID: 20982 RVA: 0x002C0B34 File Offset: 0x002BED34
		// (set) Token: 0x060051F7 RID: 20983 RVA: 0x002C0B41 File Offset: 0x002BED41
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x060051F8 RID: 20984 RVA: 0x002C0B50 File Offset: 0x002BED50
		[CompilerGenerated]
		internal static void <LoadFile>g__AddToList|16_0<T1, T2>(ConcurrentDictionary<string, T1> dict, IEnumerable<ContentXElement> elem, UIStyleFile file) where T1 : GUISelector<T2> where T2 : GUIPrefab
		{
			foreach (ContentXElement prefabElement in elem)
			{
				string name = prefabElement.GetAttributeString("name", string.Empty);
				if (name != string.Empty)
				{
					T2 prefab = (T2)((object)Activator.CreateInstance(typeof(T2), new object[]
					{
						prefabElement,
						file
					}));
					if (!dict.ContainsKey(name))
					{
						dict[name] = (T1)((object)Activator.CreateInstance(typeof(T1), new object[]
						{
							name
						}));
					}
					dict[name].Prefabs.Add(prefab, false);
				}
			}
		}

		// Token: 0x04002B60 RID: 11104
		private readonly ConcurrentDictionary<string, GUIFont> _fonts = new ConcurrentDictionary<string, GUIFont>();

		// Token: 0x04002B61 RID: 11105
		private readonly ConcurrentDictionary<string, GUISprite> _sprites = new ConcurrentDictionary<string, GUISprite>();

		// Token: 0x04002B62 RID: 11106
		private readonly ConcurrentDictionary<string, GUISpriteSheet> _spriteSheets = new ConcurrentDictionary<string, GUISpriteSheet>();

		// Token: 0x04002B63 RID: 11107
		private readonly ConcurrentDictionary<string, GUICursor> _cursors = new ConcurrentDictionary<string, GUICursor>();

		// Token: 0x04002B64 RID: 11108
		private readonly ConcurrentDictionary<string, GUIColor> _colors = new ConcurrentDictionary<string, GUIColor>();

		// Token: 0x04002B65 RID: 11109
		private UIStyleFile _fakeFile;

		// Token: 0x04002B66 RID: 11110
		private IStorageService _storageService;

		// Token: 0x04002B67 RID: 11111
		private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();

		// Token: 0x04002B68 RID: 11112
		private int _isDisposed;

		// Token: 0x020012A8 RID: 4776
		public class Factory : IUIStylesCollection.IFactory, IService, IDisposable
		{
			// Token: 0x060094CD RID: 38093 RVA: 0x003D1484 File Offset: 0x003CF684
			public IEnumerable<IUIStylesCollection> CreateInstance(IStylesResourceInfo info, IStorageService storageService)
			{
				Guard.IsNotNull<IStylesResourceInfo>(info, "info");
				Guard.IsNotNull<ContentPackage>(info.OwnerPackage, "OwnerPackage");
				if (info.FilePaths.IsDefaultOrEmpty)
				{
					return ImmutableArray<IUIStylesCollection>.Empty;
				}
				ImmutableArray<IUIStylesCollection>.Builder builder = ImmutableArray.CreateBuilder<IUIStylesCollection>();
				foreach (ContentPath contentPath in info.FilePaths)
				{
					builder.Add(new UIStylesCollection(contentPath, storageService));
				}
				return builder.ToImmutable();
			}

			// Token: 0x060094CE RID: 38094 RVA: 0x003D1507 File Offset: 0x003CF707
			public void Dispose()
			{
			}

			// Token: 0x17001CF5 RID: 7413
			// (get) Token: 0x060094CF RID: 38095 RVA: 0x003D1509 File Offset: 0x003CF709
			public bool IsDisposed
			{
				get
				{
					return false;
				}
			}
		}
	}
}
