using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000273 RID: 627
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class Prefab
	{
		// Token: 0x06002CC2 RID: 11458 RVA: 0x00127714 File Offset: 0x00125914
		public static void DisallowCallFromConstructor()
		{
			if (!Prefab.potentialCallFromConstructor)
			{
				return;
			}
			StackTrace st = new StackTrace(2, false);
			for (int i = st.FrameCount - 1; i >= 0; i--)
			{
				StackFrame frame = st.GetFrame(i);
				MethodBase methodBase = (frame != null) ? frame.GetMethod() : null;
				if (methodBase != null && methodBase.IsConstructor)
				{
					Type declaringType = methodBase.DeclaringType;
					if (declaringType != null && Prefab.Types.Contains(declaringType))
					{
						throw new Exception("Called disallowed method from within a prefab's constructor!");
					}
				}
			}
			Prefab.potentialCallFromConstructor = false;
		}

		// Token: 0x17000D53 RID: 3411
		// (get) Token: 0x06002CC3 RID: 11459 RVA: 0x0012778B File Offset: 0x0012598B
		[Nullable(2)]
		public ContentPackage ContentPackage
		{
			[NullableContext(2)]
			get
			{
				ContentFile contentFile = this.ContentFile;
				if (contentFile == null)
				{
					return null;
				}
				return contentFile.ContentPackage;
			}
		}

		// Token: 0x17000D54 RID: 3412
		// (get) Token: 0x06002CC4 RID: 11460 RVA: 0x0012779E File Offset: 0x0012599E
		public ContentPath FilePath
		{
			get
			{
				return this.ContentFile.Path;
			}
		}

		// Token: 0x06002CC5 RID: 11461 RVA: 0x001277AC File Offset: 0x001259AC
		public Prefab(ContentFile file, Identifier identifier)
		{
			Prefab.potentialCallFromConstructor = true;
			this.ContentFile = file;
			this.Identifier = identifier;
			if (this.Identifier.IsEmpty)
			{
				throw new ArgumentException("Error creating " + base.GetType().Name + ": Identifier cannot be empty");
			}
		}

		// Token: 0x06002CC6 RID: 11462 RVA: 0x00127800 File Offset: 0x00125A00
		public Prefab(ContentFile file, ContentXElement element)
		{
			Prefab.potentialCallFromConstructor = true;
			this.ContentFile = file;
			this.Identifier = this.DetermineIdentifier(element);
			if (this.Identifier.IsEmpty)
			{
				throw new ArgumentException("Error creating " + base.GetType().Name + ": Identifier cannot be empty");
			}
		}

		// Token: 0x06002CC7 RID: 11463 RVA: 0x0012785F File Offset: 0x00125A5F
		protected virtual Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x06002CC8 RID: 11464
		public abstract void Dispose();

		// Token: 0x0400160E RID: 5646
		public static readonly ImmutableHashSet<Type> Types = ReflectionUtils.GetDerivedNonAbstract<Prefab>().ToImmutableHashSet<Type>();

		// Token: 0x0400160F RID: 5647
		private static bool potentialCallFromConstructor;

		// Token: 0x04001610 RID: 5648
		public readonly Identifier Identifier;

		// Token: 0x04001611 RID: 5649
		public readonly ContentFile ContentFile;
	}
}
