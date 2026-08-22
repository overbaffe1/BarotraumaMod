using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000139 RID: 313
	internal class ConditionalSprite
	{
		// Token: 0x060028B2 RID: 10418 RVA: 0x001C4768 File Offset: 0x001C2968
		public void Remove()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
			DeformableSprite deformableSprite = this.DeformableSprite;
			if (deformableSprite != null)
			{
				deformableSprite.Remove();
			}
			this.DeformableSprite = null;
		}

		// Token: 0x17000A68 RID: 2664
		// (get) Token: 0x060028B3 RID: 10419 RVA: 0x001C479A File Offset: 0x001C299A
		// (set) Token: 0x060028B4 RID: 10420 RVA: 0x001C47A2 File Offset: 0x001C29A2
		public bool IsActive { get; private set; } = true;

		// Token: 0x17000A69 RID: 2665
		// (get) Token: 0x060028B5 RID: 10421 RVA: 0x001C47AB File Offset: 0x001C29AB
		// (set) Token: 0x060028B6 RID: 10422 RVA: 0x001C47B3 File Offset: 0x001C29B3
		public ISerializableEntity Target { get; private set; }

		// Token: 0x17000A6A RID: 2666
		// (get) Token: 0x060028B7 RID: 10423 RVA: 0x001C47BC File Offset: 0x001C29BC
		// (set) Token: 0x060028B8 RID: 10424 RVA: 0x001C47C4 File Offset: 0x001C29C4
		public Sprite Sprite { get; private set; }

		// Token: 0x17000A6B RID: 2667
		// (get) Token: 0x060028B9 RID: 10425 RVA: 0x001C47CD File Offset: 0x001C29CD
		// (set) Token: 0x060028BA RID: 10426 RVA: 0x001C47D5 File Offset: 0x001C29D5
		public DeformableSprite DeformableSprite { get; private set; }

		// Token: 0x17000A6C RID: 2668
		// (get) Token: 0x060028BB RID: 10427 RVA: 0x001C47DE File Offset: 0x001C29DE
		public Sprite ActiveSprite
		{
			get
			{
				return this.Sprite ?? this.DeformableSprite.Sprite;
			}
		}

		// Token: 0x060028BC RID: 10428 RVA: 0x001C47F8 File Offset: 0x001C29F8
		public ConditionalSprite(ContentXElement element, ISerializableEntity target, string file = "", bool lazyLoad = false, float sourceRectScale = 1f)
		{
			this.Target = target;
			this.Exclusive = element.GetAttributeBool("exclusive", this.Exclusive);
			this.LogicalOperator = element.GetAttributeEnum<PropertyConditional.LogicalOperatorType>("comparison", this.LogicalOperator);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "conditional"))
				{
					if (!(a == "sprite"))
					{
						if (a == "deformablesprite")
						{
							this.DeformableSprite = new DeformableSprite(subElement, null, null, file, lazyLoad, false, sourceRectScale);
						}
					}
					else
					{
						this.Sprite = new Sprite(subElement, "", file, lazyLoad, sourceRectScale);
					}
				}
				else
				{
					this.conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
				}
			}
		}

		// Token: 0x060028BD RID: 10429 RVA: 0x001C4928 File Offset: 0x001C2B28
		public void CheckConditionals()
		{
			this.IsActive = (this.Target != null && PropertyConditional.CheckConditionals(this.Target, this.conditionals, this.LogicalOperator));
		}

		// Token: 0x040014D6 RID: 5334
		public readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

		// Token: 0x040014D8 RID: 5336
		public readonly PropertyConditional.LogicalOperatorType LogicalOperator;

		// Token: 0x040014D9 RID: 5337
		public readonly bool Exclusive;
	}
}
