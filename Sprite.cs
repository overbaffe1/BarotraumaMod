using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200013C RID: 316
	public class Sprite
	{
		// Token: 0x17000A88 RID: 2696
		// (get) Token: 0x06002905 RID: 10501 RVA: 0x001C61FF File Offset: 0x001C43FF
		public Identifier Identifier
		{
			get
			{
				if (this.identifier.IsEmpty)
				{
					this.identifier = Sprite.GetIdentifier(this.SourceElement);
				}
				return this.identifier;
			}
		}

		// Token: 0x17000A89 RID: 2697
		// (get) Token: 0x06002906 RID: 10502 RVA: 0x001C622C File Offset: 0x001C442C
		public static IEnumerable<Sprite> LoadedSprites
		{
			get
			{
				List<Sprite> retVal = null;
				List<WeakReference<Sprite>> obj = Sprite.list;
				lock (obj)
				{
					retVal = (from s in Sprite.list.Select(delegate(WeakReference<Sprite> wRef)
					{
						Sprite spr;
						if (wRef.TryGetTarget(out spr))
						{
							return spr;
						}
						return null;
					})
					where s != null
					select s).ToList<Sprite>();
				}
				return retVal;
			}
		}

		// Token: 0x17000A8A RID: 2698
		// (get) Token: 0x06002907 RID: 10503 RVA: 0x001C62BC File Offset: 0x001C44BC
		// (set) Token: 0x06002908 RID: 10504 RVA: 0x001C62C4 File Offset: 0x001C44C4
		private protected Texture2D texture { protected get; private set; }

		// Token: 0x17000A8B RID: 2699
		// (get) Token: 0x06002909 RID: 10505 RVA: 0x001C62CD File Offset: 0x001C44CD
		public Texture2D Texture
		{
			get
			{
				this.EnsureLazyLoaded(false);
				return this.texture;
			}
		}

		// Token: 0x17000A8C RID: 2700
		// (get) Token: 0x0600290A RID: 10506 RVA: 0x001C62DC File Offset: 0x001C44DC
		public bool Loaded
		{
			get
			{
				return this.texture != null && !this.cannotBeLoaded;
			}
		}

		// Token: 0x0600290B RID: 10507 RVA: 0x001C62F4 File Offset: 0x001C44F4
		public Sprite(Sprite other) : this(other.texture, new Rectangle?(other.sourceRect), new Vector2?(other.offset), other.rotation, other.FilePath.Value)
		{
			this.Compress = other.Compress;
			this.size = other.size;
			this.effects = other.effects;
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x001C6358 File Offset: 0x001C4558
		public Sprite(Texture2D texture, Rectangle? sourceRectangle, Vector2? newOffset, float newRotation = 0f, string path = null)
		{
			this.size = Vector2.One;
			base..ctor();
			this.texture = texture;
			this.sourceRect = (sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height));
			this.offset = (newOffset ?? Vector2.Zero);
			this.size = new Vector2((float)this.sourceRect.Width, (float)this.sourceRect.Height);
			this.origin = Vector2.Zero;
			this.effects = SpriteEffects.None;
			this.rotation = newRotation;
			this.FilePath = ContentPath.FromRaw(path);
			Sprite.AddToList(this);
			if (!string.IsNullOrEmpty(path))
			{
				Identifier fullPath = Path.GetFullPath(path).CleanUpPathCrossPlatform(false, "").ToIdentifier();
				List<WeakReference<Sprite>> obj = Sprite.list;
				lock (obj)
				{
					if (!Sprite.textureRefCounts.TryAdd(fullPath, new Sprite.TextureRefCounter
					{
						RefCount = 1,
						Texture = texture
					}))
					{
						Sprite.textureRefCounts[fullPath].RefCount++;
					}
				}
			}
		}

		// Token: 0x0600290D RID: 10509 RVA: 0x001C64A0 File Offset: 0x001C46A0
		public Task LazyLoadAsync()
		{
			Sprite.<LazyLoadAsync>d__20 <LazyLoadAsync>d__;
			<LazyLoadAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<LazyLoadAsync>d__.<>4__this = this;
			<LazyLoadAsync>d__.<>1__state = -1;
			<LazyLoadAsync>d__.<>t__builder.Start<Sprite.<LazyLoadAsync>d__20>(ref <LazyLoadAsync>d__);
			return <LazyLoadAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x001C64E4 File Offset: 0x001C46E4
		public void EnsureLazyLoaded(bool isAsync = false)
		{
			if (!this.LazyLoad || this.texture != null || this.cannotBeLoaded || this.loadingAsync)
			{
				return;
			}
			this.loadingAsync = isAsync;
			Vector4 sourceVector = Vector4.Zero;
			bool temp2 = false;
			int maxLoadRetries = File.Exists(this.FilePath) ? 3 : 0;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				try
				{
					this.LoadTexture(ref sourceVector, ref temp2);
				}
				catch (IOException)
				{
					if (i == maxLoadRetries || !File.Exists(this.FilePath))
					{
						throw;
					}
					string str = "Loading sprite \"";
					ContentPath filePath = this.FilePath;
					DebugConsole.NewMessage(str + ((filePath != null) ? filePath.ToString() : null) + "\" failed, retrying in 250 ms...", null, false);
					Thread.Sleep(500);
				}
			}
			if (this.sourceRect.Width == 0 && this.sourceRect.Height == 0)
			{
				this.sourceRect = new Rectangle((int)sourceVector.X, (int)sourceVector.Y, (int)sourceVector.Z, (int)sourceVector.W);
				ContentXElement sourceElement = this.SourceElement;
				string key = "size";
				Vector2 vector = Vector2.One;
				this.size = sourceElement.GetAttributeVector2(key, vector);
				this.size.X = this.size.X * (float)this.sourceRect.Width;
				this.size.Y = this.size.Y * (float)this.sourceRect.Height;
				ContentXElement sourceElement2 = this.SourceElement;
				string key2 = "origin";
				vector = new Vector2(0.5f, 0.5f);
				this.RelativeOrigin = sourceElement2.GetAttributeVector2(key2, vector);
			}
			if (this.texture == null)
			{
				this.cannotBeLoaded = true;
			}
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x001C6688 File Offset: 0x001C4888
		public void ReloadTexture()
		{
			Texture2D oldTexture = this.texture;
			if (this.texture == null)
			{
				DebugConsole.ThrowError("Sprite: Failed to reload the texture, texture is null.", null, null, false, false);
				return;
			}
			this.texture.Dispose();
			string value = this.FilePath.Value;
			bool compress = this.Compress;
			bool mipmap = false;
			ContentXElement sourceElement = this.SourceElement;
			this.texture = TextureLoader.FromFile(value, compress, mipmap, (sourceElement != null) ? sourceElement.ContentPackage : null);
			Identifier pathKey = this.FullPath.ToIdentifier();
			if (Sprite.textureRefCounts.ContainsKey(pathKey))
			{
				Sprite.textureRefCounts[pathKey].Texture = this.texture;
			}
			foreach (Sprite sprite in Sprite.LoadedSprites)
			{
				if (sprite.texture == oldTexture)
				{
					sprite.texture = this.texture;
				}
			}
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x001C676C File Offset: 0x001C496C
		public static Texture2D LoadTexture(string file, bool compress = true, ContentPackage contentPackage = null)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				Texture2D t = null;
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					t = new Texture2D(GameMain.GraphicsDeviceManager.GraphicsDevice, 1, 1);
				});
				return t;
			}
			Identifier fullPath = Path.GetFullPath(file).CleanUpPathCrossPlatform(false, "").ToIdentifier();
			List<WeakReference<Sprite>> obj = Sprite.list;
			lock (obj)
			{
				if (Sprite.textureRefCounts.ContainsKey(fullPath))
				{
					Sprite.textureRefCounts[fullPath].RefCount++;
					return Sprite.textureRefCounts[fullPath].Texture;
				}
			}
			if (File.Exists(file))
			{
				ToolBox.IsProperFilenameCase(file);
				Texture2D newTexture = TextureLoader.FromFile(file, compress, false, contentPackage);
				List<WeakReference<Sprite>> obj2 = Sprite.list;
				lock (obj2)
				{
					if (!Sprite.textureRefCounts.TryAdd(fullPath, new Sprite.TextureRefCounter
					{
						RefCount = 1,
						Texture = newTexture
					}))
					{
						CrossThread.RequestExecutionOnMainThread(delegate
						{
							newTexture.Dispose();
						});
						Sprite.textureRefCounts[fullPath].RefCount++;
						return Sprite.textureRefCounts[fullPath].Texture;
					}
				}
				return newTexture;
			}
			DebugConsole.ThrowError("Sprite \"" + file + "\" not found!", null, contentPackage, false, false);
			DebugConsole.Log(Environment.StackTrace.CleanupStackTrace());
			return null;
		}

		// Token: 0x06002911 RID: 10513 RVA: 0x001C6914 File Offset: 0x001C4B14
		public void Draw(ISpriteBatch spriteBatch, Vector2 pos, float rotate = 0f, float scale = 1f, SpriteEffects spriteEffect = SpriteEffects.None)
		{
			this.Draw(spriteBatch, pos, Color.White, rotate, scale, spriteEffect, null);
		}

		// Token: 0x06002912 RID: 10514 RVA: 0x001C693C File Offset: 0x001C4B3C
		public void Draw(ISpriteBatch spriteBatch, Vector2 pos, Color color, float rotate = 0f, float scale = 1f, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			this.Draw(spriteBatch, pos, color, this.origin, rotate, new Vector2(scale, scale), spriteEffect, depth);
		}

		// Token: 0x06002913 RID: 10515 RVA: 0x001C6968 File Offset: 0x001C4B68
		public void Draw(ISpriteBatch spriteBatch, Vector2 pos, Color color, Vector2 origin, float rotate = 0f, float scale = 1f, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			this.Draw(spriteBatch, pos, color, origin, rotate, new Vector2(scale, scale), spriteEffect, depth);
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x001C6990 File Offset: 0x001C4B90
		public virtual void Draw(ISpriteBatch spriteBatch, Vector2 pos, Color color, Vector2 origin, float rotate, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			if (this.Texture == null)
			{
				return;
			}
			spriteBatch.Draw(this.texture, pos + this.offset, new Rectangle?(this.sourceRect), color, this.rotation + rotate, origin, scale, spriteEffect, depth ?? this.depth);
		}

		// Token: 0x06002915 RID: 10517 RVA: 0x001C69F4 File Offset: 0x001C4BF4
		public void DrawSilhouette(SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float rotate, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, float? depth = null)
		{
			if (this.Texture == null)
			{
				return;
			}
			for (int x = -1; x <= 1; x += 2)
			{
				for (int y = -1; y <= 1; y += 2)
				{
					spriteBatch.Draw(this.texture, pos + this.offset + new Vector2((float)x, (float)y), new Rectangle?(this.sourceRect), Color.Black, this.rotation + rotate, origin, scale, spriteEffect, (depth ?? this.depth) + 0.01f);
				}
			}
		}

		// Token: 0x06002916 RID: 10518 RVA: 0x001C6A88 File Offset: 0x001C4C88
		public void DrawTiled(ISpriteBatch spriteBatch, Vector2 position, Vector2 targetSize, float rotation = 0f, Vector2? origin = null, Color? color = null, Vector2? startOffset = null, Vector2? textureScale = null, float? depth = null)
		{
			this.DrawTiled(spriteBatch, position, targetSize, this.effects, rotation, origin, color, startOffset, textureScale, depth);
		}

		// Token: 0x06002917 RID: 10519 RVA: 0x001C6AB0 File Offset: 0x001C4CB0
		public void DrawTiled(ISpriteBatch spriteBatch, Vector2 position, Vector2 targetSize, SpriteEffects spriteEffects, float rotation = 0f, Vector2? origin = null, Color? color = null, Vector2? startOffset = null, Vector2? textureScale = null, float? depth = null)
		{
			Sprite.<>c__DisplayClass31_0 CS$<>8__locals1;
			CS$<>8__locals1.targetSize = targetSize;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteEffects = spriteEffects;
			CS$<>8__locals1.depth = depth;
			if (this.Texture == null)
			{
				return;
			}
			CS$<>8__locals1.flipHorizontal = ((CS$<>8__locals1.spriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally);
			CS$<>8__locals1.flipVertical = ((CS$<>8__locals1.spriteEffects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically);
			CS$<>8__locals1.addedRotation = rotation + this.rotation;
			if (CS$<>8__locals1.flipHorizontal != CS$<>8__locals1.flipVertical)
			{
				CS$<>8__locals1.addedRotation = -CS$<>8__locals1.addedRotation;
			}
			CS$<>8__locals1.advanceX = ((CS$<>8__locals1.addedRotation == 0f) ? Vector2.UnitX : new Vector2((float)Math.Cos((double)CS$<>8__locals1.addedRotation), (float)Math.Sin((double)CS$<>8__locals1.addedRotation)));
			CS$<>8__locals1.advanceY = new Vector2(-CS$<>8__locals1.advanceX.Y, CS$<>8__locals1.advanceX.X);
			Vector2 drawOffset = startOffset ?? Vector2.Zero;
			CS$<>8__locals1.scale = (textureScale ?? Vector2.One);
			CS$<>8__locals1.drawColor = (color ?? Color.White);
			CS$<>8__locals1.transformedOrigin = (origin ?? Vector2.Zero);
			CS$<>8__locals1.transformedOrigin = CS$<>8__locals1.advanceX * CS$<>8__locals1.transformedOrigin.X + CS$<>8__locals1.advanceY * CS$<>8__locals1.transformedOrigin.Y;
			drawOffset.X = drawOffset.X / CS$<>8__locals1.scale.X % (float)this.sourceRect.Width;
			drawOffset.Y = drawOffset.Y / CS$<>8__locals1.scale.Y % (float)this.sourceRect.Height;
			int xTiles = (int)Math.Ceiling((double)((CS$<>8__locals1.targetSize.X + drawOffset.X * CS$<>8__locals1.scale.X) / ((float)this.sourceRect.Width * CS$<>8__locals1.scale.X)));
			int yTiles = (int)Math.Ceiling((double)((CS$<>8__locals1.targetSize.Y + drawOffset.Y * CS$<>8__locals1.scale.Y) / ((float)this.sourceRect.Height * CS$<>8__locals1.scale.Y)));
			Vector2 currDrawPosition = -drawOffset;
			Rectangle texPerspective = this.sourceRect;
			for (int x = 0; x < xTiles; x++)
			{
				texPerspective.X = this.sourceRect.X;
				texPerspective.Width = this.sourceRect.Width;
				texPerspective.Height = this.sourceRect.Height;
				if (currDrawPosition.X < 0f)
				{
					float diff = -currDrawPosition.X;
					currDrawPosition.X += diff;
					texPerspective.Width -= (int)diff;
					texPerspective.X += (int)diff;
				}
				if (x == xTiles - 1)
				{
					int diff2 = (int)((currDrawPosition.X + (float)texPerspective.Width * CS$<>8__locals1.scale.X - CS$<>8__locals1.targetSize.X) / CS$<>8__locals1.scale.X);
					texPerspective.Width -= diff2;
				}
				currDrawPosition.Y = -drawOffset.Y;
				for (int y = 0; y < yTiles; y++)
				{
					texPerspective.Y = this.sourceRect.Y;
					texPerspective.Height = this.sourceRect.Height;
					if (currDrawPosition.Y < 0f)
					{
						float diff3 = -currDrawPosition.Y;
						currDrawPosition.Y += diff3;
						texPerspective.Height -= (int)diff3;
						texPerspective.Y += (int)diff3;
					}
					if (y == yTiles - 1)
					{
						int diff4 = (int)((currDrawPosition.Y + (float)texPerspective.Height * CS$<>8__locals1.scale.Y - CS$<>8__locals1.targetSize.Y) / CS$<>8__locals1.scale.Y);
						texPerspective.Height -= diff4;
					}
					this.<DrawTiled>g__drawSection|31_0(currDrawPosition, texPerspective, ref CS$<>8__locals1);
					currDrawPosition.Y += (float)texPerspective.Height * CS$<>8__locals1.scale.Y;
				}
				currDrawPosition.X += (float)texPerspective.Width * CS$<>8__locals1.scale.X;
			}
		}

		// Token: 0x17000A8D RID: 2701
		// (get) Token: 0x06002918 RID: 10520 RVA: 0x001C6F2F File Offset: 0x001C512F
		// (set) Token: 0x06002919 RID: 10521 RVA: 0x001C6F37 File Offset: 0x001C5137
		public ContentXElement SourceElement { get; private set; }

		// Token: 0x17000A8E RID: 2702
		// (get) Token: 0x0600291A RID: 10522 RVA: 0x001C6F40 File Offset: 0x001C5140
		// (set) Token: 0x0600291B RID: 10523 RVA: 0x001C6F48 File Offset: 0x001C5148
		public bool LazyLoad { get; private set; }

		// Token: 0x17000A8F RID: 2703
		// (get) Token: 0x0600291C RID: 10524 RVA: 0x001C6F51 File Offset: 0x001C5151
		// (set) Token: 0x0600291D RID: 10525 RVA: 0x001C6F59 File Offset: 0x001C5159
		public Rectangle SourceRect
		{
			get
			{
				return this.sourceRect;
			}
			set
			{
				this.sourceRect = value;
			}
		}

		// Token: 0x17000A90 RID: 2704
		// (get) Token: 0x0600291E RID: 10526 RVA: 0x001C6F62 File Offset: 0x001C5162
		// (set) Token: 0x0600291F RID: 10527 RVA: 0x001C6F6A File Offset: 0x001C516A
		public float Depth
		{
			get
			{
				return this.depth;
			}
			set
			{
				this.depth = MathHelper.Clamp(value, 0.001f, 0.999f);
			}
		}

		// Token: 0x17000A91 RID: 2705
		// (get) Token: 0x06002920 RID: 10528 RVA: 0x001C6F82 File Offset: 0x001C5182
		// (set) Token: 0x06002921 RID: 10529 RVA: 0x001C6F8C File Offset: 0x001C518C
		public Vector2 Origin
		{
			get
			{
				return this.origin;
			}
			set
			{
				this.origin = value;
				this._relativeOrigin = new Vector2(this.origin.X / (float)this.sourceRect.Width, this.origin.Y / (float)this.sourceRect.Height);
			}
		}

		// Token: 0x17000A92 RID: 2706
		// (get) Token: 0x06002922 RID: 10530 RVA: 0x001C6FDB File Offset: 0x001C51DB
		// (set) Token: 0x06002923 RID: 10531 RVA: 0x001C6FE4 File Offset: 0x001C51E4
		public Vector2 RelativeOrigin
		{
			get
			{
				return this._relativeOrigin;
			}
			set
			{
				this._relativeOrigin = value;
				this.origin = new Vector2(this._relativeOrigin.X * (float)this.sourceRect.Width, this._relativeOrigin.Y * (float)this.sourceRect.Height);
			}
		}

		// Token: 0x17000A93 RID: 2707
		// (get) Token: 0x06002924 RID: 10532 RVA: 0x001C7033 File Offset: 0x001C5233
		// (set) Token: 0x06002925 RID: 10533 RVA: 0x001C703B File Offset: 0x001C523B
		public Vector2 RelativeSize { get; private set; }

		// Token: 0x17000A94 RID: 2708
		// (get) Token: 0x06002926 RID: 10534 RVA: 0x001C7044 File Offset: 0x001C5244
		// (set) Token: 0x06002927 RID: 10535 RVA: 0x001C704C File Offset: 0x001C524C
		public ContentPath FilePath { get; private set; }

		// Token: 0x17000A95 RID: 2709
		// (get) Token: 0x06002928 RID: 10536 RVA: 0x001C7055 File Offset: 0x001C5255
		public string FullPath
		{
			get
			{
				return this.FilePath.FullPath;
			}
		}

		// Token: 0x17000A96 RID: 2710
		// (get) Token: 0x06002929 RID: 10537 RVA: 0x001C7062 File Offset: 0x001C5262
		// (set) Token: 0x0600292A RID: 10538 RVA: 0x001C706A File Offset: 0x001C526A
		public bool Compress { get; private set; }

		// Token: 0x0600292B RID: 10539 RVA: 0x001C7074 File Offset: 0x001C5274
		public override string ToString()
		{
			ContentPath filePath = this.FilePath;
			string str = (filePath != null) ? filePath.ToString() : null;
			string str2 = ": ";
			Rectangle rectangle = this.sourceRect;
			return str + str2 + rectangle.ToString();
		}

		// Token: 0x17000A97 RID: 2711
		// (get) Token: 0x0600292C RID: 10540 RVA: 0x001C70B1 File Offset: 0x001C52B1
		// (set) Token: 0x0600292D RID: 10541 RVA: 0x001C70B9 File Offset: 0x001C52B9
		public Identifier EntityIdentifier { get; set; }

		// Token: 0x17000A98 RID: 2712
		// (get) Token: 0x0600292E RID: 10542 RVA: 0x001C70C2 File Offset: 0x001C52C2
		// (set) Token: 0x0600292F RID: 10543 RVA: 0x001C70CA File Offset: 0x001C52CA
		public string Name { get; set; }

		// Token: 0x06002930 RID: 10544 RVA: 0x001C70D4 File Offset: 0x001C52D4
		private void LoadTexture(ref Vector4 sourceVector, ref bool shouldReturn)
		{
			string value = this.FilePath.Value;
			bool compress = this.Compress;
			ContentXElement sourceElement = this.SourceElement;
			this.texture = Sprite.LoadTexture(value, compress, (sourceElement != null) ? sourceElement.ContentPackage : null);
			if (this.texture == null)
			{
				shouldReturn = true;
				return;
			}
			if (sourceVector.Z == 0f)
			{
				sourceVector.Z = (float)this.texture.Width;
			}
			if (sourceVector.W == 0f)
			{
				sourceVector.W = (float)this.texture.Height;
			}
		}

		// Token: 0x06002931 RID: 10545 RVA: 0x001C7159 File Offset: 0x001C5359
		private void CalculateSourceRect()
		{
			this.sourceRect = new Rectangle(0, 0, this.texture.Width, this.texture.Height);
		}

		// Token: 0x06002932 RID: 10546 RVA: 0x001C7180 File Offset: 0x001C5380
		private static void AddToList(Sprite sprite)
		{
			List<WeakReference<Sprite>> obj = Sprite.list;
			lock (obj)
			{
				Sprite.list.Add(new WeakReference<Sprite>(sprite));
			}
		}

		// Token: 0x06002933 RID: 10547 RVA: 0x001C71CC File Offset: 0x001C53CC
		public Sprite(ContentXElement element, string path = "", string file = "", bool lazyLoad = false, float sourceRectScale = 1f)
		{
			this.size = Vector2.One;
			base..ctor();
			if (element == null)
			{
				DebugConsole.ThrowError("Sprite: xml element null in " + file + ". Failed to create the sprite!", null, null, false, false);
				return;
			}
			this.LazyLoad = lazyLoad;
			this.SourceElement = element;
			if (!this.ParseTexturePath(path, file))
			{
				return;
			}
			this.Name = this.SourceElement.GetAttributeString("name", null);
			ContentXElement sourceElement = this.SourceElement;
			string key = "sourcerect";
			Vector4 zero = Vector4.Zero;
			Vector4 sourceVector = sourceElement.GetAttributeVector4(key, zero);
			XElement overrideElement = this.GetLocalizationOverrideElement();
			if (overrideElement != null && overrideElement.Attribute("sourcerect") != null)
			{
				sourceVector = overrideElement.GetAttributeVector4("sourcerect", Vector4.Zero);
			}
			if ((overrideElement ?? this.SourceElement).Attribute("sheetindex") != null)
			{
				Point sheetElementSize = (overrideElement ?? this.SourceElement).GetAttributePoint("sheetelementsize", Point.Zero);
				Point sheetIndex = (overrideElement ?? this.SourceElement).GetAttributePoint("sheetindex", Point.Zero);
				sourceVector = new Vector4((float)(sheetIndex.X * sheetElementSize.X), (float)(sheetIndex.Y * sheetElementSize.Y), (float)sheetElementSize.X, (float)sheetElementSize.Y);
			}
			this.Compress = this.SourceElement.GetAttributeBool("compress", true);
			bool shouldReturn = false;
			if (!lazyLoad)
			{
				this.LoadTexture(ref sourceVector, ref shouldReturn);
			}
			if (shouldReturn)
			{
				return;
			}
			this.sourceRect = new Rectangle((int)(sourceVector.X * sourceRectScale), (int)(sourceVector.Y * sourceRectScale), (int)(sourceVector.Z * sourceRectScale), (int)(sourceVector.W * sourceRectScale));
			ContentXElement sourceElement2 = this.SourceElement;
			string key2 = "size";
			Vector2 vector = Vector2.One;
			this.size = sourceElement2.GetAttributeVector2(key2, vector);
			this.RelativeSize = this.size;
			this.size.X = this.size.X * (float)this.sourceRect.Width;
			this.size.Y = this.size.Y * (float)this.sourceRect.Height;
			ContentXElement sourceElement3 = this.SourceElement;
			string key3 = "origin";
			vector = new Vector2(0.5f, 0.5f);
			this.RelativeOrigin = sourceElement3.GetAttributeVector2(key3, vector);
			this.Depth = this.SourceElement.GetAttributeFloat("depth", 0.001f);
			Sprite.AddToList(this);
		}

		// Token: 0x06002934 RID: 10548 RVA: 0x001C7424 File Offset: 0x001C5624
		internal void LoadParams(RagdollParams.SpriteParams spriteParams, bool isFlipped)
		{
			this.SourceElement = spriteParams.Element;
			this.sourceRect = spriteParams.SourceRect;
			this.RelativeOrigin = spriteParams.Origin;
			if (isFlipped)
			{
				this.Origin = new Vector2((float)this.sourceRect.Width - this.origin.X, this.origin.Y);
			}
			this.depth = spriteParams.Depth;
		}

		// Token: 0x06002935 RID: 10549 RVA: 0x001C7494 File Offset: 0x001C5694
		public Sprite(string newFile, Vector2 newOrigin)
		{
			this.size = Vector2.One;
			base..ctor();
			Vector2? newOrigin2 = new Vector2?(newOrigin);
			this.Init(newFile, null, newOrigin2, null, 0f);
			Sprite.AddToList(this);
		}

		// Token: 0x06002936 RID: 10550 RVA: 0x001C74E0 File Offset: 0x001C56E0
		public Sprite(string newFile, Rectangle? sourceRectangle, Vector2? origin = null, float rotation = 0f)
		{
			this.size = Vector2.One;
			base..ctor();
			this.Init(newFile, sourceRectangle, origin, null, rotation);
			Sprite.AddToList(this);
		}

		// Token: 0x06002937 RID: 10551 RVA: 0x001C751C File Offset: 0x001C571C
		private void Init(string newFile, Rectangle? sourceRectangle = null, Vector2? newOrigin = null, Vector2? newOffset = null, float newRotation = 0f)
		{
			this.FilePath = ContentPath.FromRaw(newFile);
			Vector4 sourceVector = Vector4.Zero;
			bool shouldReturn = false;
			this.LoadTexture(ref sourceVector, ref shouldReturn);
			if (shouldReturn)
			{
				return;
			}
			if (sourceRectangle != null)
			{
				this.sourceRect = sourceRectangle.Value;
			}
			else
			{
				this.CalculateSourceRect();
			}
			this.offset = (newOffset ?? Vector2.Zero);
			if (newOrigin != null)
			{
				this.RelativeOrigin = newOrigin.Value;
			}
			this.size = new Vector2((float)this.sourceRect.Width, (float)this.sourceRect.Height);
			this.rotation = newRotation;
		}

		// Token: 0x06002938 RID: 10552 RVA: 0x001C75CC File Offset: 0x001C57CC
		public static Identifier GetIdentifier(XElement sourceElement)
		{
			if (sourceElement == null)
			{
				return "".ToIdentifier();
			}
			XElement parentElement = sourceElement.Parent;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted<XElement>(sourceElement);
			defaultInterpolatedStringHandler.AppendFormatted(((parentElement != null) ? parentElement.ToString() : null) ?? "");
			return defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
		}

		// Token: 0x06002939 RID: 10553 RVA: 0x001C7628 File Offset: 0x001C5828
		private static void RemoveFromList(Sprite sprite)
		{
			List<WeakReference<Sprite>> obj = Sprite.list;
			lock (obj)
			{
				Sprite.list.RemoveAll(delegate(WeakReference<Sprite> wRef)
				{
					Sprite s;
					return !wRef.TryGetTarget(out s) || s == sprite;
				});
			}
		}

		// Token: 0x0600293A RID: 10554 RVA: 0x001C7688 File Offset: 0x001C5888
		public void Remove()
		{
			Sprite.RemoveFromList(this);
			this.DisposeTexture();
		}

		// Token: 0x0600293B RID: 10555 RVA: 0x001C7698 File Offset: 0x001C5898
		~Sprite()
		{
			this.Remove();
		}

		// Token: 0x0600293C RID: 10556 RVA: 0x001C76C4 File Offset: 0x001C58C4
		private void DisposeTexture()
		{
			if (this.texture != null)
			{
				List<WeakReference<Sprite>> obj = Sprite.list;
				lock (obj)
				{
					if (!this.FilePath.IsNullOrEmpty())
					{
						Identifier pathKey = this.FullPath.ToIdentifier();
						if (!pathKey.IsEmpty && Sprite.textureRefCounts.ContainsKey(pathKey))
						{
							Sprite.textureRefCounts[pathKey].RefCount--;
							if (Sprite.textureRefCounts[pathKey].RefCount > 0)
							{
								this.texture = null;
								this.FilePath = ContentPath.Empty;
								return;
							}
							Sprite.textureRefCounts.Remove(pathKey);
						}
					}
				}
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					this.texture.Dispose();
				});
				this.texture = null;
			}
		}

		// Token: 0x0600293D RID: 10557 RVA: 0x001C779C File Offset: 0x001C599C
		public void ReloadXML()
		{
			ContentXElement sourceElement = this.SourceElement;
			ContentXElement contentXElement = null;
			if (sourceElement == contentXElement)
			{
				return;
			}
			string path = this.SourceElement.ParseContentPathFromUri();
			if (string.IsNullOrWhiteSpace(path))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[Sprite] Could not parse the content path from the source element (");
				defaultInterpolatedStringHandler.AppendFormatted<ContentXElement>(this.SourceElement);
				defaultInterpolatedStringHandler.AppendLiteral(") uri: ");
				defaultInterpolatedStringHandler.AppendFormatted(this.SourceElement.BaseUri);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
				return;
			}
			XDocument doc = XMLExtensions.TryLoadXml(path);
			if (doc == null)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(this.Name) && string.IsNullOrWhiteSpace(this.EntityIdentifier.Value))
			{
				return;
			}
			IEnumerable<XElement> spriteElements = doc.Descendants("sprite").Concat(doc.Descendants("Sprite"));
			IEnumerable<XElement> sourceElements = from e in spriteElements
			where e.GetAttributeString("name", null) == this.Name
			select e;
			if (sourceElements.None(null))
			{
				sourceElements = spriteElements.Where(delegate(XElement e)
				{
					XElement parent = e.Parent;
					string str = (parent != null) ? parent.GetAttributeString("identifier", null) : null;
					Identifier entityIdentifier = this.EntityIdentifier;
					return str == entityIdentifier;
				});
				if (sourceElements.None(null))
				{
					sourceElements = spriteElements.Where(delegate(XElement e)
					{
						XElement parent = e.Parent;
						return ((parent != null) ? parent.GetAttributeString("name", null) : null) == this.Name;
					});
				}
			}
			if (sourceElements.Multiple(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(72, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("[Sprite] Multiple matching elements found by name (");
				defaultInterpolatedStringHandler2.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(") or identifier (");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.EntityIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(")!: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentXElement>(this.SourceElement);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Yellow), false);
			}
			else if (sourceElements.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(142, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("[Sprite] Cannot find matching source element by comparing the name attribute (");
				defaultInterpolatedStringHandler3.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler3.AppendLiteral(") or identifier (");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.EntityIdentifier);
				defaultInterpolatedStringHandler3.AppendLiteral(")! Cannot reload the xml for sprite element \"");
				defaultInterpolatedStringHandler3.AppendFormatted(this.SourceElement.ToString());
				defaultInterpolatedStringHandler3.AppendLiteral("\"!");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.Yellow), false);
			}
			else
			{
				this.SourceElement = sourceElements.Single<XElement>().FromPackage(this.SourceElement.ContentPackage);
			}
			sourceElement = this.SourceElement;
			contentXElement = null;
			if (sourceElement != contentXElement)
			{
				ContentXElement sourceElement2 = this.SourceElement;
				string key = "sourcerect";
				Rectangle empty = Rectangle.Empty;
				this.sourceRect = sourceElement2.GetAttributeRect(key, empty);
				XElement overrideElement = this.GetLocalizationOverrideElement();
				if (overrideElement != null && overrideElement.Attribute("sourcerect") != null)
				{
					this.sourceRect = overrideElement.GetAttributeRect("sourcerect", Rectangle.Empty);
				}
				if ((overrideElement ?? this.SourceElement).Attribute("sheetindex") != null)
				{
					Point sheetElementSize = (overrideElement ?? this.SourceElement).GetAttributePoint("sheetelementsize", Point.Zero);
					Point sheetIndex = (overrideElement ?? this.SourceElement).GetAttributePoint("sheetindex", Point.Zero);
					this.sourceRect = new Rectangle(sheetIndex.X * sheetElementSize.X, sheetIndex.Y * sheetElementSize.Y, sheetElementSize.X, sheetElementSize.Y);
				}
				ContentXElement sourceElement3 = this.SourceElement;
				string key2 = "size";
				Vector2 vector = Vector2.One;
				this.size = sourceElement3.GetAttributeVector2(key2, vector);
				this.size.X = this.size.X * (float)this.sourceRect.Width;
				this.size.Y = this.size.Y * (float)this.sourceRect.Height;
				ContentXElement sourceElement4 = this.SourceElement;
				string key3 = "origin";
				vector = new Vector2(0.5f, 0.5f);
				this.RelativeOrigin = sourceElement4.GetAttributeVector2(key3, vector);
				this.Depth = this.SourceElement.GetAttributeFloat("depth", 0.001f);
			}
		}

		// Token: 0x0600293E RID: 10558 RVA: 0x001C7B98 File Offset: 0x001C5D98
		public bool ParseTexturePath(string path = "", string file = "")
		{
			if (file == "")
			{
				file = this.SourceElement.GetAttributeStringUnrestricted("texture", "");
				XElement overrideElement = this.GetLocalizationOverrideElement();
				if (overrideElement != null)
				{
					string overrideFile = overrideElement.GetAttributeStringUnrestricted("texture", "");
					if (!string.IsNullOrEmpty(overrideFile))
					{
						file = overrideFile;
					}
				}
			}
			if (file == "")
			{
				string str = "Sprite ";
				XElement element = this.SourceElement.Element;
				DebugConsole.ThrowError(str + ((element != null) ? element.ToString() : null) + " doesn't have a texture specified!", null, this.SourceElement.ContentPackage, false, false);
				return false;
			}
			if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
			{
				path += "/";
			}
			this.FilePath = ContentPath.FromRaw(this.SourceElement.ContentPackage, (path + file).CleanUpPathCrossPlatform(true, ""));
			return true;
		}

		// Token: 0x0600293F RID: 10559 RVA: 0x001C7C84 File Offset: 0x001C5E84
		private XElement GetLocalizationOverrideElement()
		{
			foreach (ContentXElement subElement in this.SourceElement.Elements())
			{
				if (subElement.Name.ToString().Equals("override", StringComparison.OrdinalIgnoreCase))
				{
					LanguageIdentifier language = subElement.GetAttributeIdentifier("language", "").ToLanguageIdentifier();
					if (GameSettings.CurrentConfig.Language == language)
					{
						return subElement;
					}
				}
			}
			return null;
		}

		// Token: 0x06002941 RID: 10561 RVA: 0x001C7D40 File Offset: 0x001C5F40
		[CompilerGenerated]
		private void <DrawTiled>g__drawSection|31_0(Vector2 slicePos, Rectangle sliceRect, ref Sprite.<>c__DisplayClass31_0 A_3)
		{
			Vector2 transformedPos = slicePos;
			if (A_3.flipHorizontal)
			{
				transformedPos.X = A_3.targetSize.X - transformedPos.X - (float)sliceRect.Width * A_3.scale.X;
			}
			if (A_3.flipVertical)
			{
				transformedPos.Y = A_3.targetSize.Y - transformedPos.Y - (float)sliceRect.Height * A_3.scale.Y;
			}
			transformedPos = A_3.advanceX * transformedPos.X + A_3.advanceY * transformedPos.Y;
			transformedPos += A_3.position - A_3.transformedOrigin;
			A_3.spriteBatch.Draw(this.texture, transformedPos, new Rectangle?(sliceRect), A_3.drawColor, A_3.addedRotation, Vector2.Zero, A_3.scale, A_3.spriteEffects, A_3.depth ?? this.depth);
		}

		// Token: 0x04001507 RID: 5383
		private Identifier identifier;

		// Token: 0x04001508 RID: 5384
		private static readonly List<WeakReference<Sprite>> list = new List<WeakReference<Sprite>>();

		// Token: 0x04001509 RID: 5385
		private static readonly Dictionary<Identifier, Sprite.TextureRefCounter> textureRefCounts = new Dictionary<Identifier, Sprite.TextureRefCounter>();

		// Token: 0x0400150A RID: 5386
		private bool cannotBeLoaded;

		// Token: 0x0400150B RID: 5387
		protected volatile bool loadingAsync;

		// Token: 0x0400150D RID: 5389
		public static readonly Version LastBrokenTiledSpriteGameVersion = new Version(1, 2, 7, 0);

		// Token: 0x0400150F RID: 5391
		private Rectangle sourceRect;

		// Token: 0x04001510 RID: 5392
		protected Vector2 offset;

		// Token: 0x04001512 RID: 5394
		protected Vector2 origin;

		// Token: 0x04001513 RID: 5395
		public Vector2 size;

		// Token: 0x04001514 RID: 5396
		public float rotation;

		// Token: 0x04001515 RID: 5397
		public SpriteEffects effects;

		// Token: 0x04001516 RID: 5398
		protected float depth;

		// Token: 0x04001517 RID: 5399
		private Vector2 _relativeOrigin;

		// Token: 0x02000D82 RID: 3458
		private class TextureRefCounter
		{
			// Token: 0x04004F91 RID: 20369
			public Texture2D Texture;

			// Token: 0x04004F92 RID: 20370
			public int RefCount;
		}
	}
}
