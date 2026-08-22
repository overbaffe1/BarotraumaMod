using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x0200043A RID: 1082
	internal abstract class SpriteDeformation
	{
		// Token: 0x17001267 RID: 4711
		// (get) Token: 0x06004846 RID: 18502 RVA: 0x002798E1 File Offset: 0x00277AE1
		// (set) Token: 0x06004847 RID: 18503 RVA: 0x002798E9 File Offset: 0x00277AE9
		public virtual float Phase { get; set; }

		// Token: 0x17001268 RID: 4712
		// (get) Token: 0x06004848 RID: 18504 RVA: 0x002798F2 File Offset: 0x00277AF2
		// (set) Token: 0x06004849 RID: 18505 RVA: 0x002798FA File Offset: 0x00277AFA
		protected Vector2[,] Deformation { get; set; }

		// Token: 0x17001269 RID: 4713
		// (get) Token: 0x0600484A RID: 18506 RVA: 0x00279903 File Offset: 0x00277B03
		// (set) Token: 0x0600484B RID: 18507 RVA: 0x0027990B File Offset: 0x00277B0B
		public SpriteDeformationParams Params { get; set; }

		// Token: 0x1700126A RID: 4714
		// (get) Token: 0x0600484C RID: 18508 RVA: 0x00279914 File Offset: 0x00277B14
		public static IEnumerable<string> DeformationTypes
		{
			get
			{
				return SpriteDeformation.deformationTypes;
			}
		}

		// Token: 0x1700126B RID: 4715
		// (get) Token: 0x0600484D RID: 18509 RVA: 0x0027991B File Offset: 0x00277B1B
		// (set) Token: 0x0600484E RID: 18510 RVA: 0x00279928 File Offset: 0x00277B28
		public Point Resolution
		{
			get
			{
				return this.Params.Resolution;
			}
			set
			{
				this.SetResolution(value);
			}
		}

		// Token: 0x1700126C RID: 4716
		// (get) Token: 0x0600484F RID: 18511 RVA: 0x00279931 File Offset: 0x00277B31
		public string TypeName
		{
			get
			{
				return this.Params.Type;
			}
		}

		// Token: 0x1700126D RID: 4717
		// (get) Token: 0x06004850 RID: 18512 RVA: 0x0027993E File Offset: 0x00277B3E
		public int Sync
		{
			get
			{
				return this.Params.Sync;
			}
		}

		// Token: 0x06004851 RID: 18513 RVA: 0x0027994B File Offset: 0x00277B4B
		public static SpriteDeformation Load(string deformationType, string parentDebugName)
		{
			return SpriteDeformation.Load(null, deformationType, parentDebugName);
		}

		// Token: 0x06004852 RID: 18514 RVA: 0x00279955 File Offset: 0x00277B55
		public static SpriteDeformation Load(XElement element, string parentDebugName)
		{
			return SpriteDeformation.Load(element, null, parentDebugName);
		}

		// Token: 0x06004853 RID: 18515 RVA: 0x00279960 File Offset: 0x00277B60
		private static SpriteDeformation Load(XElement element, string deformationType, string parentDebugName)
		{
			string typeName = deformationType;
			if (element != null)
			{
				typeName = (element.GetAttributeString("typename", null) ?? element.GetAttributeString("type", ""));
			}
			Point resolution = element.GetAttributePoint("Resolution", new Point(0, 0));
			if (resolution.X < 2 || resolution.Y < 2)
			{
				DebugConsole.AddWarning("Potential error in sprite deformation (" + parentDebugName + "): resolution must be at least 2x2.", null);
			}
			SpriteDeformation newDeformation = null;
			string a = typeName.ToLowerInvariant();
			if (!(a == "inflate"))
			{
				if (!(a == "custom"))
				{
					if (!(a == "noise"))
					{
						if (!(a == "jointbend") && !(a == "bendjoint"))
						{
							if (a == "reacttotriggerers")
							{
								return new PositionalDeformation(element);
							}
							PositionalDeformation.ReactionType reactionType;
							if (Enum.TryParse<PositionalDeformation.ReactionType>(typeName, out reactionType))
							{
								newDeformation = new PositionalDeformation(element)
								{
									Type = reactionType
								};
							}
							else
							{
								DebugConsole.ThrowError(string.Concat(new string[]
								{
									"Could not load sprite deformation animation in ",
									parentDebugName,
									" - \"",
									typeName,
									"\" is not a valid deformation type."
								}), null, null, false, false);
							}
						}
						else
						{
							newDeformation = new JointBendDeformation(element);
						}
					}
					else
					{
						newDeformation = new NoiseDeformation(element);
					}
				}
				else
				{
					newDeformation = new CustomDeformation(element);
				}
			}
			else
			{
				newDeformation = new Inflate(element);
			}
			if (newDeformation != null)
			{
				newDeformation.Params.Type = typeName;
			}
			return newDeformation;
		}

		// Token: 0x06004854 RID: 18516 RVA: 0x00279AB7 File Offset: 0x00277CB7
		protected SpriteDeformation(XElement element, SpriteDeformationParams deformationParams)
		{
			this.Params = deformationParams;
			SerializableProperty.DeserializeProperties(deformationParams, element);
			this.Deformation = new Vector2[deformationParams.Resolution.X, deformationParams.Resolution.Y];
		}

		// Token: 0x06004855 RID: 18517 RVA: 0x00279AEF File Offset: 0x00277CEF
		public void SetResolution(Point resolution)
		{
			this.Params.Resolution = resolution;
			this.Deformation = new Vector2[this.Params.Resolution.X, this.Params.Resolution.Y];
		}

		// Token: 0x06004856 RID: 18518
		protected abstract void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY);

		// Token: 0x06004857 RID: 18519
		public abstract void Update(float deltaTime);

		// Token: 0x06004858 RID: 18520 RVA: 0x00279B28 File Offset: 0x00277D28
		public static Vector2[,] GetDeformation(IEnumerable<SpriteDeformation> animations, Vector2 scale, bool flippedHorizontally, bool inverseY = false)
		{
			foreach (SpriteDeformation animation in animations)
			{
				if (animation.Params.Resolution.X != animation.Deformation.GetLength(0) || animation.Params.Resolution.Y != animation.Deformation.GetLength(1))
				{
					animation.Deformation = new Vector2[animation.Params.Resolution.X, animation.Params.Resolution.Y];
				}
			}
			Point resolution = animations.First<SpriteDeformation>().Resolution;
			if (animations.Any((SpriteDeformation a) => a.Resolution != resolution))
			{
				DebugConsole.ThrowError("All animations must have the same resolution! Using the lowest resolution.", null, null, false, false);
				resolution = (from anim in animations
				orderby anim.Resolution.X + anim.Resolution.Y
				select anim).First<SpriteDeformation>().Resolution;
				animations.ForEach(delegate(SpriteDeformation a)
				{
					a.Resolution = resolution;
				});
			}
			Vector2[,] deformation = new Vector2[resolution.X, resolution.Y];
			foreach (SpriteDeformation animation2 in animations)
			{
				SpriteDeformation.yValues.Clear();
				for (int y = 0; y < resolution.Y; y++)
				{
					SpriteDeformation.yValues.Add(y);
				}
				if (inverseY && animation2 is CustomDeformation)
				{
					SpriteDeformation.yValues.Reverse();
				}
				Vector2[,] animDeformation;
				float multiplier;
				animation2.GetDeformation(out animDeformation, out multiplier, flippedHorizontally, inverseY);
				for (int x = 0; x < resolution.X; x++)
				{
					for (int y2 = 0; y2 < resolution.Y; y2++)
					{
						switch (animation2.Params.BlendMode)
						{
						case SpriteDeformation.DeformationBlendMode.Add:
							deformation[x, SpriteDeformation.yValues[y2]] += animDeformation[x, y2] * scale * multiplier;
							break;
						case SpriteDeformation.DeformationBlendMode.Multiply:
							deformation[x, SpriteDeformation.yValues[y2]] *= animDeformation[x, y2] * multiplier;
							break;
						case SpriteDeformation.DeformationBlendMode.Override:
							deformation[x, SpriteDeformation.yValues[y2]] = animDeformation[x, y2] * scale * multiplier;
							break;
						}
					}
				}
			}
			return deformation;
		}

		// Token: 0x06004859 RID: 18521 RVA: 0x00279E1C File Offset: 0x0027801C
		public virtual void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this.Params, element, false, false);
		}

		// Token: 0x04002577 RID: 9591
		private static readonly string[] deformationTypes = new string[]
		{
			"Inflate",
			"Custom",
			"Noise",
			"BendJoint",
			"ReactToTriggerers"
		};

		// Token: 0x04002578 RID: 9592
		private static readonly List<int> yValues = new List<int>();

		// Token: 0x02001143 RID: 4419
		public enum DeformationBlendMode
		{
			// Token: 0x04005B5D RID: 23389
			Add,
			// Token: 0x04005B5E RID: 23390
			Multiply,
			// Token: 0x04005B5F RID: 23391
			Override
		}
	}
}
