using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200028F RID: 655
	public class Sprite
	{
		// Token: 0x17000D7A RID: 3450
		// (get) Token: 0x06002DEA RID: 11754 RVA: 0x001300BE File Offset: 0x0012E2BE
		// (set) Token: 0x06002DEB RID: 11755 RVA: 0x001300C6 File Offset: 0x0012E2C6
		public ContentXElement SourceElement { get; private set; }

		// Token: 0x17000D7B RID: 3451
		// (get) Token: 0x06002DEC RID: 11756 RVA: 0x001300CF File Offset: 0x0012E2CF
		// (set) Token: 0x06002DED RID: 11757 RVA: 0x001300D7 File Offset: 0x0012E2D7
		public bool LazyLoad { get; private set; }

		// Token: 0x17000D7C RID: 3452
		// (get) Token: 0x06002DEE RID: 11758 RVA: 0x001300E0 File Offset: 0x0012E2E0
		// (set) Token: 0x06002DEF RID: 11759 RVA: 0x001300E8 File Offset: 0x0012E2E8
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

		// Token: 0x17000D7D RID: 3453
		// (get) Token: 0x06002DF0 RID: 11760 RVA: 0x001300F1 File Offset: 0x0012E2F1
		// (set) Token: 0x06002DF1 RID: 11761 RVA: 0x001300F9 File Offset: 0x0012E2F9
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

		// Token: 0x17000D7E RID: 3454
		// (get) Token: 0x06002DF2 RID: 11762 RVA: 0x00130111 File Offset: 0x0012E311
		// (set) Token: 0x06002DF3 RID: 11763 RVA: 0x0013011C File Offset: 0x0012E31C
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

		// Token: 0x17000D7F RID: 3455
		// (get) Token: 0x06002DF4 RID: 11764 RVA: 0x0013016B File Offset: 0x0012E36B
		// (set) Token: 0x06002DF5 RID: 11765 RVA: 0x00130174 File Offset: 0x0012E374
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

		// Token: 0x17000D80 RID: 3456
		// (get) Token: 0x06002DF6 RID: 11766 RVA: 0x001301C3 File Offset: 0x0012E3C3
		// (set) Token: 0x06002DF7 RID: 11767 RVA: 0x001301CB File Offset: 0x0012E3CB
		public Vector2 RelativeSize { get; private set; }

		// Token: 0x17000D81 RID: 3457
		// (get) Token: 0x06002DF8 RID: 11768 RVA: 0x001301D4 File Offset: 0x0012E3D4
		// (set) Token: 0x06002DF9 RID: 11769 RVA: 0x001301DC File Offset: 0x0012E3DC
		public ContentPath FilePath { get; private set; }

		// Token: 0x17000D82 RID: 3458
		// (get) Token: 0x06002DFA RID: 11770 RVA: 0x001301E5 File Offset: 0x0012E3E5
		public string FullPath
		{
			get
			{
				return this.FilePath.FullPath;
			}
		}

		// Token: 0x17000D83 RID: 3459
		// (get) Token: 0x06002DFB RID: 11771 RVA: 0x001301F2 File Offset: 0x0012E3F2
		// (set) Token: 0x06002DFC RID: 11772 RVA: 0x001301FA File Offset: 0x0012E3FA
		public bool Compress { get; private set; }

		// Token: 0x06002DFD RID: 11773 RVA: 0x00130204 File Offset: 0x0012E404
		public override string ToString()
		{
			ContentPath filePath = this.FilePath;
			string str = (filePath != null) ? filePath.ToString() : null;
			string str2 = ": ";
			Rectangle rectangle = this.sourceRect;
			return str + str2 + rectangle.ToString();
		}

		// Token: 0x17000D84 RID: 3460
		// (get) Token: 0x06002DFE RID: 11774 RVA: 0x00130241 File Offset: 0x0012E441
		// (set) Token: 0x06002DFF RID: 11775 RVA: 0x00130249 File Offset: 0x0012E449
		public Identifier EntityIdentifier { get; set; }

		// Token: 0x17000D85 RID: 3461
		// (get) Token: 0x06002E00 RID: 11776 RVA: 0x00130252 File Offset: 0x0012E452
		// (set) Token: 0x06002E01 RID: 11777 RVA: 0x0013025A File Offset: 0x0012E45A
		public string Name { get; set; }

		// Token: 0x06002E02 RID: 11778 RVA: 0x00130264 File Offset: 0x0012E464
		public Sprite(ContentXElement element, string path = "", string file = "", bool lazyLoad = false, float sourceRectScale = 1f)
		{
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
		}

		// Token: 0x06002E03 RID: 11779 RVA: 0x001304A8 File Offset: 0x0012E6A8
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

		// Token: 0x06002E04 RID: 11780 RVA: 0x00130518 File Offset: 0x0012E718
		public Sprite(string newFile, Vector2 newOrigin)
		{
			Vector2? newOrigin2 = new Vector2?(newOrigin);
			this.Init(newFile, null, newOrigin2, null, 0f);
		}

		// Token: 0x06002E05 RID: 11781 RVA: 0x00130560 File Offset: 0x0012E760
		public Sprite(string newFile, Rectangle? sourceRectangle, Vector2? origin = null, float rotation = 0f)
		{
			this.Init(newFile, sourceRectangle, origin, null, rotation);
		}

		// Token: 0x06002E06 RID: 11782 RVA: 0x00130594 File Offset: 0x0012E794
		private void Init(string newFile, Rectangle? sourceRectangle = null, Vector2? newOrigin = null, Vector2? newOffset = null, float newRotation = 0f)
		{
			this.FilePath = ContentPath.FromRaw(newFile);
			Vector4 sourceVector = Vector4.Zero;
			bool shouldReturn = false;
			if (shouldReturn)
			{
				return;
			}
			if (sourceRectangle != null)
			{
				this.sourceRect = sourceRectangle.Value;
			}
			this.offset = (newOffset ?? Vector2.Zero);
			if (newOrigin != null)
			{
				this.RelativeOrigin = newOrigin.Value;
			}
			this.size = new Vector2((float)this.sourceRect.Width, (float)this.sourceRect.Height);
			this.rotation = newRotation;
		}

		// Token: 0x06002E07 RID: 11783 RVA: 0x00130630 File Offset: 0x0012E830
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

		// Token: 0x06002E08 RID: 11784 RVA: 0x0013068B File Offset: 0x0012E88B
		public void Remove()
		{
		}

		// Token: 0x06002E09 RID: 11785 RVA: 0x00130690 File Offset: 0x0012E890
		~Sprite()
		{
			this.Remove();
		}

		// Token: 0x06002E0A RID: 11786 RVA: 0x001306BC File Offset: 0x0012E8BC
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

		// Token: 0x06002E0B RID: 11787 RVA: 0x00130AB8 File Offset: 0x0012ECB8
		public bool ParseTexturePath(string path = "", string file = "")
		{
			return true;
		}

		// Token: 0x06002E0C RID: 11788 RVA: 0x00130AC8 File Offset: 0x0012ECC8
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

		// Token: 0x04001678 RID: 5752
		private Rectangle sourceRect;

		// Token: 0x04001679 RID: 5753
		protected Vector2 offset;

		// Token: 0x0400167B RID: 5755
		protected Vector2 origin;

		// Token: 0x0400167C RID: 5756
		public Vector2 size = Vector2.One;

		// Token: 0x0400167D RID: 5757
		public float rotation;

		// Token: 0x0400167E RID: 5758
		protected float depth;

		// Token: 0x0400167F RID: 5759
		private Vector2 _relativeOrigin;
	}
}
