using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000504 RID: 1284
	internal class Turret : Powered, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001350 RID: 4944
		// (get) Token: 0x060047E3 RID: 18403 RVA: 0x001C9431 File Offset: 0x001C7631
		public IEnumerable<Item> ActiveProjectiles
		{
			get
			{
				return this.activeProjectiles;
			}
		}

		// Token: 0x17001351 RID: 4945
		// (get) Token: 0x060047E4 RID: 18404 RVA: 0x001C9439 File Offset: 0x001C7639
		// (set) Token: 0x060047E5 RID: 18405 RVA: 0x001C9441 File Offset: 0x001C7641
		public float Rotation { get; private set; }

		// Token: 0x17001352 RID: 4946
		// (get) Token: 0x060047E6 RID: 18406 RVA: 0x001C944A File Offset: 0x001C764A
		// (set) Token: 0x060047E7 RID: 18407 RVA: 0x001C9452 File Offset: 0x001C7652
		[Serialize("0,0", IsPropertySaveable.No, "The position of the barrel relative to the upper left corner of the base sprite (in pixels).", "", false)]
		public Vector2 BarrelPos
		{
			get
			{
				return this.barrelPos;
			}
			set
			{
				this.barrelPos = value;
				this.UpdateTransformedBarrelPos();
			}
		}

		// Token: 0x17001353 RID: 4947
		// (get) Token: 0x060047E8 RID: 18408 RVA: 0x001C9461 File Offset: 0x001C7661
		// (set) Token: 0x060047E9 RID: 18409 RVA: 0x001C9469 File Offset: 0x001C7669
		[Serialize("0,0", IsPropertySaveable.No, "The projectile launching location relative to transformed barrel position (in pixels).", "", false)]
		public Vector2 FiringOffset { get; set; }

		// Token: 0x17001354 RID: 4948
		// (get) Token: 0x060047EA RID: 18410 RVA: 0x001C9472 File Offset: 0x001C7672
		// (set) Token: 0x060047EB RID: 18411 RVA: 0x001C947A File Offset: 0x001C767A
		[Serialize(false, IsPropertySaveable.No, "If enabled, the firing offset will alternate from left to right (i.e. flipping the x-component of the offset each shot.)", "", false)]
		public bool AlternatingFiringOffset { get; set; }

		// Token: 0x17001355 RID: 4949
		// (get) Token: 0x060047EC RID: 18412 RVA: 0x001C9483 File Offset: 0x001C7683
		public Vector2 TransformedBarrelPos
		{
			get
			{
				return this.transformedBarrelPos;
			}
		}

		// Token: 0x17001356 RID: 4950
		// (get) Token: 0x060047ED RID: 18413 RVA: 0x001C948B File Offset: 0x001C768B
		// (set) Token: 0x060047EE RID: 18414 RVA: 0x001C9493 File Offset: 0x001C7693
		[Serialize(0f, IsPropertySaveable.No, "The impulse applied to the physics body of the projectile (the higher the impulse, the faster the projectiles are launched).", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x17001357 RID: 4951
		// (get) Token: 0x060047EF RID: 18415 RVA: 0x001C949C File Offset: 0x001C769C
		// (set) Token: 0x060047F0 RID: 18416 RVA: 0x001C94A4 File Offset: 0x001C76A4
		[Serialize(1f, IsPropertySaveable.No, "Multiplies the damage the turret deals by this amount.", "", false)]
		public float DamageMultiplier { get; set; }

		// Token: 0x17001358 RID: 4952
		// (get) Token: 0x060047F1 RID: 18417 RVA: 0x001C94AD File Offset: 0x001C76AD
		// (set) Token: 0x060047F2 RID: 18418 RVA: 0x001C94B5 File Offset: 0x001C76B5
		[Serialize(1, IsPropertySaveable.No, "How many projectiles the weapon launches when fired once.", "", false)]
		public int ProjectileCount { get; set; }

		// Token: 0x17001359 RID: 4953
		// (get) Token: 0x060047F3 RID: 18419 RVA: 0x001C94BE File Offset: 0x001C76BE
		// (set) Token: 0x060047F4 RID: 18420 RVA: 0x001C94C6 File Offset: 0x001C76C6
		[Serialize(false, IsPropertySaveable.No, "Can the turret be fired without projectiles (causing it just to execute the OnUse effects and the firing animation without actually firing anything).", "", false)]
		public bool LaunchWithoutProjectile { get; set; }

		// Token: 0x1700135A RID: 4954
		// (get) Token: 0x060047F5 RID: 18421 RVA: 0x001C94CF File Offset: 0x001C76CF
		// (set) Token: 0x060047F6 RID: 18422 RVA: 0x001C94D7 File Offset: 0x001C76D7
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x1700135B RID: 4955
		// (get) Token: 0x060047F7 RID: 18423 RVA: 0x001C94E0 File Offset: 0x001C76E0
		// (set) Token: 0x060047F8 RID: 18424 RVA: 0x001C94E8 File Offset: 0x001C76E8
		[Serialize(1f, IsPropertySaveable.No, "How fast the turret can rotate while firing (for charged weapons).", "", false)]
		public float FiringRotationSpeedModifier { get; set; }

		// Token: 0x1700135C RID: 4956
		// (get) Token: 0x060047F9 RID: 18425 RVA: 0x001C94F1 File Offset: 0x001C76F1
		// (set) Token: 0x060047FA RID: 18426 RVA: 0x001C94F9 File Offset: 0x001C76F9
		[Serialize(false, IsPropertySaveable.Yes, "Whether the turret should always charge-up fully to shoot.", "", false)]
		public bool SingleChargedShot { get; set; }

		// Token: 0x1700135D RID: 4957
		// (get) Token: 0x060047FB RID: 18427 RVA: 0x001C9502 File Offset: 0x001C7702
		// (set) Token: 0x060047FC RID: 18428 RVA: 0x001C950F File Offset: 0x001C770F
		[Serialize(0f, IsPropertySaveable.Yes, "The angle of the turret's base in degrees.", "", true)]
		public float BaseRotation
		{
			get
			{
				return this.item.Rotation;
			}
			set
			{
				this.item.Rotation = value;
				this.UpdateTransformedBarrelPos();
			}
		}

		// Token: 0x1700135E RID: 4958
		// (get) Token: 0x060047FD RID: 18429 RVA: 0x001C9523 File Offset: 0x001C7723
		// (set) Token: 0x060047FE RID: 18430 RVA: 0x001C952B File Offset: 0x001C772B
		[Serialize(3500f, IsPropertySaveable.Yes, "How close to a target the turret has to be for an AI character to fire it.", "", false)]
		public float AIRange { get; set; }

		// Token: 0x1700135F RID: 4959
		// (get) Token: 0x060047FF RID: 18431 RVA: 0x001C9534 File Offset: 0x001C7734
		// (set) Token: 0x06004800 RID: 18432 RVA: 0x001C953C File Offset: 0x001C773C
		[Serialize(10f, IsPropertySaveable.No, "How much off the turret can be from the target for the AI to shoot. In degrees.", "", false)]
		public float MaxAngleOffset
		{
			get
			{
				return this._maxAngleOffset;
			}
			private set
			{
				this._maxAngleOffset = MathHelper.Clamp(value, 0f, 180f);
			}
		}

		// Token: 0x17001360 RID: 4960
		// (get) Token: 0x06004801 RID: 18433 RVA: 0x001C9554 File Offset: 0x001C7754
		// (set) Token: 0x06004802 RID: 18434 RVA: 0x001C955C File Offset: 0x001C775C
		[Serialize(1.1f, IsPropertySaveable.No, "How much does the AI prefer currently selected targets over new targets closer to the turret.", "", false)]
		public float AICurrentTargetPriorityMultiplier { get; private set; }

		// Token: 0x17001361 RID: 4961
		// (get) Token: 0x06004803 RID: 18435 RVA: 0x001C9565 File Offset: 0x001C7765
		// (set) Token: 0x06004804 RID: 18436 RVA: 0x001C956D File Offset: 0x001C776D
		[Serialize(-1, IsPropertySaveable.Yes, "The turret won't fire additional projectiles if the number of previously fired, still active projectiles reaches this limit. If set to -1, there is no limit to the number of projectiles.", "", false)]
		public int MaxActiveProjectiles { get; set; }

		// Token: 0x17001362 RID: 4962
		// (get) Token: 0x06004805 RID: 18437 RVA: 0x001C9576 File Offset: 0x001C7776
		// (set) Token: 0x06004806 RID: 18438 RVA: 0x001C957E File Offset: 0x001C777E
		[Serialize(0f, IsPropertySaveable.Yes, "The time required for a charge-type turret to charge up before able to fire.", "", false)]
		public float MaxChargeTime { get; private set; }

		// Token: 0x17001363 RID: 4963
		// (get) Token: 0x06004807 RID: 18439 RVA: 0x001C9587 File Offset: 0x001C7787
		// (set) Token: 0x06004808 RID: 18440 RVA: 0x001C958F File Offset: 0x001C778F
		[Serialize(5f, IsPropertySaveable.No, "The period of time the user has to wait between shots.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float Reload { get; set; }

		// Token: 0x17001364 RID: 4964
		// (get) Token: 0x06004809 RID: 18441 RVA: 0x001C9598 File Offset: 0x001C7798
		// (set) Token: 0x0600480A RID: 18442 RVA: 0x001C95A0 File Offset: 0x001C77A0
		[Serialize(1, IsPropertySaveable.No, "How many projectiles needs to be shot before we add an extra break? Think of the double coilgun.", "", false)]
		[Editable(1, 100)]
		public int ShotsPerBurst { get; set; }

		// Token: 0x17001365 RID: 4965
		// (get) Token: 0x0600480B RID: 18443 RVA: 0x001C95A9 File Offset: 0x001C77A9
		// (set) Token: 0x0600480C RID: 18444 RVA: 0x001C95B1 File Offset: 0x001C77B1
		[Serialize(0f, IsPropertySaveable.No, "An extra delay between the bursts. Added to the reload.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float DelayBetweenBursts { get; set; }

		// Token: 0x17001366 RID: 4966
		// (get) Token: 0x0600480D RID: 18445 RVA: 0x001C95BA File Offset: 0x001C77BA
		// (set) Token: 0x0600480E RID: 18446 RVA: 0x001C95C2 File Offset: 0x001C77C2
		[Serialize(1f, IsPropertySaveable.No, "Modifies the duration of retraction of the barrell after recoil to get back to the original position after shooting. Reload time affects this too.", "", false)]
		[Editable(0.1f, 10f, 1)]
		public float RetractionDurationMultiplier { get; set; }

		// Token: 0x17001367 RID: 4967
		// (get) Token: 0x0600480F RID: 18447 RVA: 0x001C95CB File Offset: 0x001C77CB
		// (set) Token: 0x06004810 RID: 18448 RVA: 0x001C95D3 File Offset: 0x001C77D3
		[Serialize(0.1f, IsPropertySaveable.No, "How quickly the recoil moves the barrel after launching.", "", false)]
		[Editable(0.1f, 10f, 1)]
		public float RecoilTime { get; set; }

		// Token: 0x17001368 RID: 4968
		// (get) Token: 0x06004811 RID: 18449 RVA: 0x001C95DC File Offset: 0x001C77DC
		// (set) Token: 0x06004812 RID: 18450 RVA: 0x001C95E4 File Offset: 0x001C77E4
		[Serialize(0f, IsPropertySaveable.No, "How long the barrell stays in place after the recoil and before retracting back to the original position.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float RetractionDelay { get; set; }

		// Token: 0x17001369 RID: 4969
		// (get) Token: 0x06004813 RID: 18451 RVA: 0x001C95ED File Offset: 0x001C77ED
		// (set) Token: 0x06004814 RID: 18452 RVA: 0x001C960C File Offset: 0x001C780C
		[Editable(VectorComponentLabels = new string[]
		{
			"editable.minvalue",
			"editable.maxvalue"
		})]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "The range at which the barrel can rotate.", "", true)]
		public Vector2 RotationLimits
		{
			get
			{
				return new Vector2(MathHelper.ToDegrees(this.minRotation), MathHelper.ToDegrees(this.maxRotation));
			}
			set
			{
				float newMinRotation = MathHelper.ToRadians(value.X);
				float newMaxRotation = MathHelper.ToRadians(value.Y);
				bool minRotationModified = MathHelper.Distance(newMinRotation, this.minRotation) > 0.02f;
				bool maxRotationModified = MathHelper.Distance(newMaxRotation, this.maxRotation) > 0.02f;
				if (minRotationModified && !maxRotationModified)
				{
					newMinRotation = MathHelper.Clamp(newMinRotation, this.maxRotation - 6.2831855f, this.maxRotation);
				}
				else if (!minRotationModified && maxRotationModified)
				{
					newMaxRotation = MathHelper.Clamp(newMaxRotation, this.minRotation, this.minRotation + 6.2831855f);
				}
				this.maxRotation = newMaxRotation;
				this.minRotation = newMinRotation;
				this.Rotation = (this.minRotation + this.maxRotation) / 2f;
			}
		}

		// Token: 0x1700136A RID: 4970
		// (get) Token: 0x06004815 RID: 18453 RVA: 0x001C96C2 File Offset: 0x001C78C2
		// (set) Token: 0x06004816 RID: 18454 RVA: 0x001C96CA File Offset: 0x001C78CA
		[Serialize(5f, IsPropertySaveable.No, "How much torque is applied to rotate the barrel when the item is used by a character with insufficient skills to operate it. Higher values make the barrel rotate faster.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringStiffnessLowSkill { get; private set; }

		// Token: 0x1700136B RID: 4971
		// (get) Token: 0x06004817 RID: 18455 RVA: 0x001C96D3 File Offset: 0x001C78D3
		// (set) Token: 0x06004818 RID: 18456 RVA: 0x001C96DB File Offset: 0x001C78DB
		[Serialize(2f, IsPropertySaveable.No, "How much torque is applied to rotate the barrel when the item is used by a character with sufficient skills to operate it. Higher values make the barrel rotate faster.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringStiffnessHighSkill { get; private set; }

		// Token: 0x1700136C RID: 4972
		// (get) Token: 0x06004819 RID: 18457 RVA: 0x001C96E4 File Offset: 0x001C78E4
		// (set) Token: 0x0600481A RID: 18458 RVA: 0x001C96EC File Offset: 0x001C78EC
		[Serialize(50f, IsPropertySaveable.No, "How much torque is applied to resist the movement of the barrel when the item is used by a character with insufficient skills to operate it. Higher values make the aiming more \"snappy\", stopping the barrel from swinging around the direction it's being aimed at.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringDampingLowSkill { get; private set; }

		// Token: 0x1700136D RID: 4973
		// (get) Token: 0x0600481B RID: 18459 RVA: 0x001C96F5 File Offset: 0x001C78F5
		// (set) Token: 0x0600481C RID: 18460 RVA: 0x001C96FD File Offset: 0x001C78FD
		[Serialize(10f, IsPropertySaveable.No, "How much torque is applied to resist the movement of the barrel when the item is used by a character with sufficient skills to operate it. Higher values make the aiming more \"snappy\", stopping the barrel from swinging around the direction it's being aimed at.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringDampingHighSkill { get; private set; }

		// Token: 0x1700136E RID: 4974
		// (get) Token: 0x0600481D RID: 18461 RVA: 0x001C9706 File Offset: 0x001C7906
		// (set) Token: 0x0600481E RID: 18462 RVA: 0x001C970E File Offset: 0x001C790E
		[Serialize(1f, IsPropertySaveable.No, "Maximum angular velocity of the barrel when used by a character with insufficient skills to operate it.", "", false)]
		[Editable(0f, 100f, 1, DecimalCount = 2)]
		public float RotationSpeedLowSkill { get; private set; }

		// Token: 0x1700136F RID: 4975
		// (get) Token: 0x0600481F RID: 18463 RVA: 0x001C9717 File Offset: 0x001C7917
		// (set) Token: 0x06004820 RID: 18464 RVA: 0x001C971F File Offset: 0x001C791F
		[Serialize(5f, IsPropertySaveable.No, "Maximum angular velocity of the barrel when used by a character with sufficient skills to operate it.", "", false)]
		[Editable(0f, 100f, 1, DecimalCount = 2)]
		public float RotationSpeedHighSkill { get; private set; }

		// Token: 0x17001370 RID: 4976
		// (get) Token: 0x06004821 RID: 18465 RVA: 0x001C9728 File Offset: 0x001C7928
		// (set) Token: 0x06004822 RID: 18466 RVA: 0x001C9730 File Offset: 0x001C7930
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "Optional screen tint color when the item is being operated (R,G,B,A).", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Color HudTint { get; set; }

		// Token: 0x17001371 RID: 4977
		// (get) Token: 0x06004823 RID: 18467 RVA: 0x001C9739 File Offset: 0x001C7939
		// (set) Token: 0x06004824 RID: 18468 RVA: 0x001C9741 File Offset: 0x001C7941
		[Header("", "sp.turret.AutoOperate.propertyheader")]
		[Serialize(false, IsPropertySaveable.Yes, "Should the turret operate automatically using AI targeting? Comes with some optional random movement that can be adjusted below.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AutoOperate { get; set; }

		// Token: 0x17001372 RID: 4978
		// (get) Token: 0x06004825 RID: 18469 RVA: 0x001C974A File Offset: 0x001C794A
		// (set) Token: 0x06004826 RID: 18470 RVA: 0x001C9752 File Offset: 0x001C7952
		[Serialize(false, IsPropertySaveable.Yes, "Can the Auto Operate functionality be enabled using signals to the turret?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AllowAutoOperateWithWiring { get; set; }

		// Token: 0x17001373 RID: 4979
		// (get) Token: 0x06004827 RID: 18471 RVA: 0x001C975B File Offset: 0x001C795B
		// (set) Token: 0x06004828 RID: 18472 RVA: 0x001C9763 File Offset: 0x001C7963
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How much the turret should adjust the aim off the target randomly instead of tracking the target perfectly? In Degrees.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimAmount { get; set; }

		// Token: 0x17001374 RID: 4980
		// (get) Token: 0x06004829 RID: 18473 RVA: 0x001C976C File Offset: 0x001C796C
		// (set) Token: 0x0600482A RID: 18474 RVA: 0x001C9774 File Offset: 0x001C7974
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How often the turret should adjust the aim randomly instead of tracking the target perfectly? Minimum wait time, in seconds.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimMinTime { get; set; }

		// Token: 0x17001375 RID: 4981
		// (get) Token: 0x0600482B RID: 18475 RVA: 0x001C977D File Offset: 0x001C797D
		// (set) Token: 0x0600482C RID: 18476 RVA: 0x001C9785 File Offset: 0x001C7985
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How often the turret should adjust the aim randomly instead of tracking the target perfectly? Maximum wait time, in seconds.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimMaxTime { get; set; }

		// Token: 0x17001376 RID: 4982
		// (get) Token: 0x0600482D RID: 18477 RVA: 0x001C978E File Offset: 0x001C798E
		// (set) Token: 0x0600482E RID: 18478 RVA: 0x001C9796 File Offset: 0x001C7996
		[Serialize(false, IsPropertySaveable.Yes, "[Auto Operate] Should the turret move randomly while idle?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool RandomMovement { get; set; }

		// Token: 0x17001377 RID: 4983
		// (get) Token: 0x0600482F RID: 18479 RVA: 0x001C979F File Offset: 0x001C799F
		// (set) Token: 0x06004830 RID: 18480 RVA: 0x001C97A7 File Offset: 0x001C79A7
		[Serialize(false, IsPropertySaveable.Yes, "[Auto Operate] Should the turret have a delay while targeting targets or always aim prefectly?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AimDelay { get; set; }

		// Token: 0x17001378 RID: 4984
		// (get) Token: 0x06004831 RID: 18481 RVA: 0x001C97B0 File Offset: 0x001C79B0
		// (set) Token: 0x06004832 RID: 18482 RVA: 0x001C97B8 File Offset: 0x001C79B8
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target characters in general?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetCharacters { get; set; }

		// Token: 0x17001379 RID: 4985
		// (get) Token: 0x06004833 RID: 18483 RVA: 0x001C97C1 File Offset: 0x001C79C1
		// (set) Token: 0x06004834 RID: 18484 RVA: 0x001C97C9 File Offset: 0x001C79C9
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target all monsters?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetMonsters { get; set; }

		// Token: 0x1700137A RID: 4986
		// (get) Token: 0x06004835 RID: 18485 RVA: 0x001C97D2 File Offset: 0x001C79D2
		// (set) Token: 0x06004836 RID: 18486 RVA: 0x001C97DA File Offset: 0x001C79DA
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target all humans (or creatures in the same group, like pets)?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetHumans { get; set; }

		// Token: 0x1700137B RID: 4987
		// (get) Token: 0x06004837 RID: 18487 RVA: 0x001C97E3 File Offset: 0x001C79E3
		// (set) Token: 0x06004838 RID: 18488 RVA: 0x001C97EB File Offset: 0x001C79EB
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target other submarines?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetSubmarines { get; set; }

		// Token: 0x1700137C RID: 4988
		// (get) Token: 0x06004839 RID: 18489 RVA: 0x001C97F4 File Offset: 0x001C79F4
		// (set) Token: 0x0600483A RID: 18490 RVA: 0x001C97FC File Offset: 0x001C79FC
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target items?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetItems { get; set; }

		// Token: 0x1700137D RID: 4989
		// (get) Token: 0x0600483B RID: 18491 RVA: 0x001C9805 File Offset: 0x001C7A05
		// (set) Token: 0x0600483C RID: 18492 RVA: 0x001C980D File Offset: 0x001C7A0D
		[Serialize("", IsPropertySaveable.Yes, "[Auto Operate] Group or SpeciesName that the AI ignores when the turret is operated automatically.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Identifier FriendlyTag { get; private set; }

		// Token: 0x1700137E RID: 4990
		// (get) Token: 0x0600483D RID: 18493 RVA: 0x001C9816 File Offset: 0x001C7A16
		// (set) Token: 0x0600483E RID: 18494 RVA: 0x001C981E File Offset: 0x001C7A1E
		[Serialize("OwnSub", IsPropertySaveable.Yes, "[Auto Operate] Team that the turret considers friendly.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Turret.TeamType FriendlyTeamType { get; private set; }

		// Token: 0x0600483F RID: 18495 RVA: 0x001C9828 File Offset: 0x001C7A28
		public Turret(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "barrelsprite"))
				{
					if (!(a == "railsprite"))
					{
						if (!(a == "barrelspritebroken"))
						{
							if (!(a == "railspritebroken"))
							{
								if (!(a == "chargesprite"))
								{
									if (a == "spinningbarrelsprite")
									{
										int spriteCount = subElement.GetAttributeInt("spriteamount", 1);
										for (int i = 0; i < spriteCount; i++)
										{
											this.spinningBarrelSprites.Add(new Sprite(subElement, "", "", false, 1f));
										}
									}
								}
								else
								{
									List<ValueTuple<Sprite, Vector2>> list = this.chargeSprites;
									Sprite item2 = new Sprite(subElement, "", "", false, 1f);
									ContentXElement contentXElement = subElement;
									string key = "chargetarget";
									Vector2 zero = Vector2.Zero;
									list.Add(new ValueTuple<Sprite, Vector2>(item2, contentXElement.GetAttributeVector2(key, zero)));
								}
							}
							else
							{
								this.railSpriteBroken = new Sprite(subElement, "", "", false, 1f);
							}
						}
						else
						{
							this.barrelSpriteBroken = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.railSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.barrelSprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
			item.IsShootable = true;
			item.RequireAimToUse = false;
			this.isSlowTurret = item.HasTag("slowturret".ToIdentifier());
		}

		// Token: 0x06004840 RID: 18496 RVA: 0x001C9A3C File Offset: 0x001C7C3C
		private void UpdateTransformedBarrelPos()
		{
			this.transformedBarrelPos = MathUtils.RotatePointAroundTarget(this.barrelPos * this.item.Scale, new Vector2((float)(this.item.Rect.Width / 2), (float)(this.item.Rect.Height / 2)), MathHelper.ToRadians(this.item.Rotation), true);
			this.prevBaseRotation = this.item.Rotation;
			this.prevScale = this.item.Scale;
		}

		// Token: 0x06004841 RID: 18497 RVA: 0x001C9AC8 File Offset: 0x001C7CC8
		public override void OnMapLoaded()
		{
			base.OnMapLoaded();
			if (this.loadedRotationLimits != null)
			{
				this.RotationLimits = this.loadedRotationLimits.Value;
			}
			if (this.loadedBaseRotation != null)
			{
				this.BaseRotation = this.loadedBaseRotation.Value;
			}
			if (this.loadedFriendlyTeamType != null)
			{
				this.FriendlyTeamType = this.loadedFriendlyTeamType.Value;
			}
			this.targetRotation = this.Rotation;
			this.UpdateTransformedBarrelPos();
			if (!this.AllowAutoOperateWithWiring)
			{
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					foreach (ConnectionPanel connectionPanel in base.Item.GetComponents<ConnectionPanel>())
					{
						connectionPanel.Connections.RemoveAll(delegate(Connection c)
						{
							string name = c.Name;
							bool flag = name == "toggle_auto_operate" || name == "set_auto_operate";
							return flag && c.Wires.None(null);
						});
					}
				}
			}
		}

		// Token: 0x06004842 RID: 18498 RVA: 0x001C9BCC File Offset: 0x001C7DCC
		private void FindLightComponents()
		{
			if (this.lightComponents != null)
			{
				return;
			}
			foreach (LightComponent lc in this.item.GetComponents<LightComponent>())
			{
				if (((lc != null) ? lc.Parent : null) == this)
				{
					if (this.lightComponents == null)
					{
						this.lightComponents = new List<LightComponent>();
					}
					this.lightComponents.Add(lc);
				}
			}
		}

		// Token: 0x06004843 RID: 18499 RVA: 0x001C9C50 File Offset: 0x001C7E50
		public override void Update(float deltaTime, Camera cam)
		{
			this.cam = cam;
			if (this.reload > 0f)
			{
				this.reload -= deltaTime;
			}
			if (!MathUtils.NearlyEqual(this.item.Rotation, this.prevBaseRotation, 0.0001f) || !MathUtils.NearlyEqual(this.item.Scale, this.prevScale, 0.0001f))
			{
				this.UpdateTransformedBarrelPos();
			}
			Character activeUser = this.user;
			if (activeUser != null && activeUser.Removed)
			{
				this.user = null;
			}
			else
			{
				this.resetUserTimer -= deltaTime;
				if (this.resetUserTimer <= 0f)
				{
					this.user = null;
				}
			}
			activeUser = this.ActiveUser;
			if (activeUser != null && activeUser.Removed)
			{
				this.ActiveUser = null;
			}
			else
			{
				this.resetActiveUserTimer -= deltaTime;
				if (this.resetActiveUserTimer <= 0f)
				{
					this.ActiveUser = null;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float previousChargeTime = this.currentChargeTime;
			if (this.SingleChargedShot && this.reload > 0f)
			{
				this.currentChargeTime = ((this.Reload > 0f) ? Math.Max(0f, this.MaxChargeTime * (this.reload / this.Reload - 0.5f)) : 0f);
			}
			else
			{
				float chargeDeltaTime = this.tryingToCharge ? deltaTime : (-deltaTime);
				if (chargeDeltaTime > 0f && this.user != null)
				{
					chargeDeltaTime *= 1f + this.user.GetStatValue(StatTypes.TurretChargeSpeed, true);
				}
				this.currentChargeTime = Math.Clamp(this.currentChargeTime + chargeDeltaTime, 0f, this.MaxChargeTime);
			}
			this.tryingToCharge = false;
			if (this.currentChargeTime == 0f)
			{
				this.currentChargingState = Turret.ChargingState.Inactive;
			}
			else if (this.currentChargeTime < previousChargeTime)
			{
				this.currentChargingState = Turret.ChargingState.WindingDown;
			}
			else
			{
				this.currentChargingState = Turret.ChargingState.WindingUp;
			}
			if (MathUtils.NearlyEqual(this.minRotation, this.maxRotation, 0.0001f))
			{
				this.UpdateLightComponents();
				return;
			}
			float targetMidDiff = MathHelper.WrapAngle(this.targetRotation - (this.minRotation + this.maxRotation) / 2f);
			float maxDist = (this.maxRotation - this.minRotation) / 2f;
			if (Math.Abs(targetMidDiff) > maxDist)
			{
				this.targetRotation = ((targetMidDiff < 0f) ? this.minRotation : this.maxRotation);
			}
			float degreeOfSuccess = (this.user == null) ? 0.5f : base.DegreeOfSuccess(this.user);
			if (degreeOfSuccess < 0.5f)
			{
				degreeOfSuccess *= degreeOfSuccess;
			}
			float springStiffness = MathHelper.Lerp(this.SpringStiffnessLowSkill, this.SpringStiffnessHighSkill, degreeOfSuccess);
			float springDamping = MathHelper.Lerp(this.SpringDampingLowSkill, this.SpringDampingHighSkill, degreeOfSuccess);
			float rotationSpeed = MathHelper.Lerp(this.RotationSpeedLowSkill, this.RotationSpeedHighSkill, degreeOfSuccess);
			if (this.MaxChargeTime > 0f)
			{
				rotationSpeed *= MathHelper.Lerp(1f, this.FiringRotationSpeedModifier, MathUtils.EaseIn(this.currentChargeTime / this.MaxChargeTime));
			}
			Character character = this.user;
			if (((character != null) ? character.Info : null) != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null || !Level.IsLoadedFriendlyOutpost)
				{
					this.user.Info.ApplySkillGain(Tags.WeaponsSkill, SkillSettings.Current.SkillIncreasePerSecondWhenOperatingTurret * deltaTime, false, 2f, false);
				}
			}
			float rotMidDiff = MathHelper.WrapAngle(this.Rotation - (this.minRotation + this.maxRotation) / 2f);
			float targetRotationDiff = MathHelper.WrapAngle(this.targetRotation - this.Rotation);
			if (this.maxRotation - this.minRotation < 6.2831855f)
			{
				float targetRotationMaxDiff = MathHelper.WrapAngle(this.targetRotation - this.maxRotation);
				float targetRotationMinDiff = MathHelper.WrapAngle(this.targetRotation - this.minRotation);
				if (Math.Abs(targetRotationMaxDiff) < Math.Abs(targetRotationMinDiff) && rotMidDiff < 0f && targetRotationDiff < 0f)
				{
					targetRotationDiff += 6.2831855f;
				}
				else if (Math.Abs(targetRotationMaxDiff) > Math.Abs(targetRotationMinDiff) && rotMidDiff > 0f && targetRotationDiff > 0f)
				{
					targetRotationDiff -= 6.2831855f;
				}
			}
			this.angularVelocity += (targetRotationDiff * springStiffness - this.angularVelocity * springDamping) * deltaTime;
			this.angularVelocity = MathHelper.Clamp(this.angularVelocity, -rotationSpeed, rotationSpeed);
			this.Rotation += this.angularVelocity * deltaTime;
			rotMidDiff = MathHelper.WrapAngle(this.Rotation - (this.minRotation + this.maxRotation) / 2f);
			if (rotMidDiff < -maxDist)
			{
				this.Rotation = this.minRotation;
				this.angularVelocity *= -0.5f;
			}
			else if (rotMidDiff > maxDist)
			{
				this.Rotation = this.maxRotation;
				this.angularVelocity *= -0.5f;
			}
			if (this.aiFindTargetTimer > 0f)
			{
				this.aiFindTargetTimer -= deltaTime;
			}
			this.UpdateLightComponents();
			if (this.AutoOperate && this.ActiveUser == null)
			{
				this.UpdateAutoOperate(deltaTime, false, default(Identifier));
			}
		}

		// Token: 0x06004844 RID: 18500 RVA: 0x001CA170 File Offset: 0x001C8370
		public void UpdateLightComponents()
		{
			if (this.lightComponents != null)
			{
				foreach (LightComponent light in this.lightComponents)
				{
					light.Rotation = this.Rotation - this.item.RotationRad;
				}
			}
		}

		// Token: 0x06004845 RID: 18501 RVA: 0x001CA1DC File Offset: 0x001C83DC
		public override bool Use(float deltaTime, Character character = null)
		{
			if (!this.characterUsable && character != null)
			{
				return false;
			}
			if (this.isUseBeingCalled)
			{
				return false;
			}
			this.isUseBeingCalled = true;
			bool wasSuccessful = this.TryLaunch(deltaTime, character, false);
			this.isUseBeingCalled = false;
			return wasSuccessful;
		}

		// Token: 0x06004846 RID: 18502 RVA: 0x001CA21C File Offset: 0x001C841C
		public float GetPowerRequiredToShoot()
		{
			float powerCost = this.powerConsumption;
			if (this.user != null)
			{
				powerCost /= 1f + this.user.GetStatValue(StatTypes.TurretPowerCostReduction, true);
			}
			return powerCost;
		}

		// Token: 0x06004847 RID: 18503 RVA: 0x001CA250 File Offset: 0x001C8450
		public bool HasPowerToShoot()
		{
			return base.GetAvailableInstantaneousBatteryPower() >= this.GetPowerRequiredToShoot();
		}

		// Token: 0x06004848 RID: 18504 RVA: 0x001CA263 File Offset: 0x001C8463
		private Vector2 GetBarrelDir()
		{
			return new Vector2((float)Math.Cos((double)this.Rotation), -(float)Math.Sin((double)this.Rotation));
		}

		// Token: 0x06004849 RID: 18505 RVA: 0x001CA288 File Offset: 0x001C8488
		private bool TryLaunch(float deltaTime, Character character = null, bool ignorePower = false)
		{
			Turret.<>c__DisplayClass244_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			CS$<>8__locals1.<>4__this = this;
			this.tryingToCharge = true;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			if (this.currentChargeTime < this.MaxChargeTime)
			{
				return false;
			}
			if (this.reload > 0f)
			{
				return false;
			}
			if (this.MaxActiveProjectiles >= 0)
			{
				this.activeProjectiles.RemoveAll((Item it) => it.Removed);
				if (this.activeProjectiles.Count >= this.MaxActiveProjectiles)
				{
					return false;
				}
			}
			if (!ignorePower && !this.HasPowerToShoot())
			{
				return false;
			}
			Projectile launchedProjectile = null;
			float tinkeringStrength = 0f;
			for (int k = 0; k < this.ProjectileCount; k++)
			{
				Turret.<>c__DisplayClass244_1 CS$<>8__locals2;
				CS$<>8__locals2.projectiles = this.GetLoadedProjectiles();
				if (CS$<>8__locals2.projectiles.Any<Projectile>())
				{
					Item container2 = CS$<>8__locals2.projectiles.First<Projectile>().Item.Container;
					ItemContainer projectileContainer = (container2 != null) ? container2.GetComponent<ItemContainer>() : null;
					if (projectileContainer != null && projectileContainer.Item != this.item && projectileContainer != null)
					{
						projectileContainer.Item.Use(CS$<>8__locals1.deltaTime, null, null, null, this.user);
					}
				}
				else
				{
					for (int j = 0; j < this.item.linkedTo.Count; j++)
					{
						MapEntity e = this.item.linkedTo[(j + this.currentLoaderIndex) % this.item.linkedTo.Count];
						Item linkedItem = e as Item;
						if (linkedItem != null && this.item.Prefab.IsLinkAllowed(e.Prefab))
						{
							if (linkedItem.Condition > 0f)
							{
								if (this.<TryLaunch>g__tryUseProjectileContainer|244_1(linkedItem, ref CS$<>8__locals1, ref CS$<>8__locals2))
								{
									break;
								}
							}
						}
					}
					this.<TryLaunch>g__tryUseProjectileContainer|244_1(this.item, ref CS$<>8__locals1, ref CS$<>8__locals2);
				}
				if (CS$<>8__locals2.projectiles.Count == 0 && !this.LaunchWithoutProjectile)
				{
					this.failedLaunchAttempts++;
					return false;
				}
				this.failedLaunchAttempts = 0;
				foreach (MapEntity e2 in this.item.linkedTo)
				{
					Item linkedItem2 = e2 as Item;
					if (linkedItem2 != null && this.item.Prefab.IsLinkAllowed(e2.Prefab))
					{
						Repairable repairable = linkedItem2.GetComponent<Repairable>();
						if (repairable != null && repairable.IsTinkering && linkedItem2.HasTag(Tags.TurretAmmoSource))
						{
							tinkeringStrength = repairable.TinkeringStrength;
						}
					}
				}
				if (!ignorePower)
				{
					IEnumerable<PowerContainer> batteries = from b in base.GetDirectlyConnectedBatteries()
					where !b.OutputDisabled && b.Charge > 0.0001f && b.MaxOutPut > 0.0001f
					select b;
					float neededPower = this.GetPowerRequiredToShoot();
					neededPower /= 1f + tinkeringStrength * 0.2f;
					while (neededPower > 0.0001f && batteries.Any<PowerContainer>())
					{
						float takePower = neededPower / (float)batteries.Count<PowerContainer>();
						takePower = Math.Min(takePower, batteries.Min((PowerContainer b) => Math.Min(b.Charge * 3600f, b.MaxOutPut)));
						foreach (PowerContainer battery in batteries)
						{
							neededPower -= takePower;
							battery.Charge -= takePower / 3600f;
							battery.Item.CreateServerEvent<PowerContainer>(battery);
						}
					}
				}
				launchedProjectile = CS$<>8__locals2.projectiles.FirstOrDefault<Projectile>();
				Item container = (launchedProjectile != null) ? launchedProjectile.Item.Container : null;
				if (container != null)
				{
					Repairable repairable2 = (launchedProjectile != null) ? launchedProjectile.Item.Container.GetComponent<Repairable>() : null;
					if (repairable2 != null)
					{
						repairable2.LastActiveTime = (float)Timing.TotalTime + 1f;
					}
				}
				if (launchedProjectile != null || this.LaunchWithoutProjectile)
				{
					if (((launchedProjectile != null) ? launchedProjectile.Item.GetComponent<Rope>() : null) != null)
					{
						Projectile projectile2 = this.lastProjectile;
						Rope rope = (projectile2 != null) ? projectile2.Item.GetComponent<Rope>() : null;
						if (rope != null && rope.SnapWhenWeaponFiredAgain)
						{
							rope.Snap();
						}
					}
					float tinkeringStrength2;
					if (CS$<>8__locals2.projectiles.Any<Projectile>())
					{
						using (List<Projectile>.Enumerator enumerator3 = CS$<>8__locals2.projectiles.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								Projectile projectile = enumerator3.Current;
								Item item = projectile.Item;
								tinkeringStrength2 = tinkeringStrength;
								this.Launch(item, character, null, tinkeringStrength2);
							}
							goto IL_499;
						}
						goto IL_482;
					}
					goto IL_482;
					IL_499:
					if (this.item.AiTarget != null)
					{
						this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
					}
					if (container != null)
					{
						Turret.ShiftItemsInProjectileContainer(container.GetComponent<ItemContainer>());
					}
					if (this.item.linkedTo.Count > 0)
					{
						this.currentLoaderIndex = (this.currentLoaderIndex + 1) % this.item.linkedTo.Count;
						goto IL_508;
					}
					goto IL_508;
					IL_482:
					Item projectile3 = null;
					tinkeringStrength2 = tinkeringStrength;
					this.Launch(projectile3, character, null, tinkeringStrength2);
					goto IL_499;
				}
				IL_508:;
			}
			this.lastProjectile = launchedProjectile;
			if (character != null && launchedProjectile != null)
			{
				string msg = string.Concat(new string[]
				{
					GameServer.CharacterLogName(character),
					" launched ",
					this.item.Name,
					" (projectile: ",
					launchedProjectile.Item.Name
				});
				IEnumerable<Item> containedItems = launchedProjectile.Item.ContainedItems;
				if (containedItems == null || !containedItems.Any<Item>())
				{
					msg += ")";
				}
				else
				{
					msg = msg + ", contained items: " + string.Join(", ", from i in containedItems
					select i.Name) + ")";
				}
				GameServer.Log(msg, ServerLog.MessageType.ItemInteraction);
			}
			return true;
		}

		// Token: 0x0600484A RID: 18506 RVA: 0x001CA8A4 File Offset: 0x001C8AA4
		private void Launch(Item projectile, Character user = null, float? launchRotation = null, float tinkeringStrength = 0f)
		{
			this.reload = this.Reload;
			if (this.ShotsPerBurst > 1)
			{
				this.shotCounter++;
				if (this.shotCounter >= this.ShotsPerBurst)
				{
					this.reload += this.DelayBetweenBursts;
					this.shotCounter = 0;
				}
			}
			this.reload /= 1f + tinkeringStrength * 0.2f;
			if (user != null)
			{
				this.reload /= 1f + user.GetStatValue(StatTypes.TurretAttackSpeed, true);
			}
			if (projectile != null)
			{
				if (this.AlternatingFiringOffset)
				{
					this.flipFiringOffset = !this.flipFiringOffset;
				}
				this.activeProjectiles.Add(projectile);
				projectile.Drop(null, true, false);
				if (projectile.body != null)
				{
					projectile.body.Dir = 1f;
					projectile.body.ResetDynamics();
					projectile.body.Enabled = true;
				}
				float spread = MathHelper.ToRadians(this.Spread) * Rand.Range(-0.5f, 0.5f, Rand.RandSync.Unsynced);
				Vector2 launchPos = ConvertUnits.ToSimUnits(this.GetRelativeFiringPosition(true));
				Body pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(this.item.WorldPosition), launchPos, null, new Category?(Category.Cat1), true, delegate(Fixture f)
				{
					Submarine sub = f.Body.UserData as Submarine;
					return sub == null || sub != this.item.Submarine;
				}, true);
				if (pickedBody != null)
				{
					launchPos = Submarine.LastPickedPosition;
				}
				projectile.SetTransform(launchPos, -(launchRotation ?? this.Rotation) + spread, true, true, null);
				projectile.UpdateTransform();
				PhysicsBody body = projectile.body;
				projectile.Submarine = ((body != null) ? body.Submarine : null);
				Projectile projectileComponent = projectile.GetComponent<Projectile>();
				if (projectileComponent != null)
				{
					this.TryDetermineProjectileSpeed(projectileComponent);
					projectileComponent.Launcher = this.item;
					Projectile projectile2 = projectileComponent;
					projectileComponent.User = user;
					projectile2.Attacker = user;
					if (projectileComponent.Attack != null)
					{
						projectileComponent.Attack.DamageMultiplier = 1f * this.DamageMultiplier + 0.2f * tinkeringStrength;
					}
					projectileComponent.Use(null, this.LaunchImpulse);
					TriggerComponent trigger = this.item.GetComponent<TriggerComponent>();
					if (trigger != null)
					{
						projectileComponent.IgnoredBodies.Add(trigger.PhysicsBody.FarseerBody);
					}
					Rope component = projectile.GetComponent<Rope>();
					if (component != null)
					{
						component.Attach(this.item, projectile);
					}
					projectileComponent.User = user;
					if (this.item.Submarine != null && projectile.body != null)
					{
						Vector2 velocitySum = this.item.Submarine.PhysicsBody.LinearVelocity + projectile.body.LinearVelocity;
						if (velocitySum.LengthSquared() < 3686.4f)
						{
							projectile.body.LinearVelocity = velocitySum;
						}
					}
				}
				Item container = projectile.Container;
				if (container != null)
				{
					container.RemoveContained(projectile);
				}
			}
			this.item.CreateServerEvent<Turret>(this, new Turret.EventData(projectile, this));
			base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, user, null, 1f);
		}

		// Token: 0x0600484B RID: 18507 RVA: 0x001CAB8C File Offset: 0x001C8D8C
		private void TryDetermineProjectileSpeed(Projectile projectile)
		{
			if (projectile != null && !projectile.Hitscan)
			{
				this.projectileSpeed = ConvertUnits.ToDisplayUnits(MathHelper.Clamp((projectile.LaunchImpulse + this.LaunchImpulse) / projectile.Item.body.Mass, 20f, 64f));
			}
		}

		// Token: 0x0600484C RID: 18508 RVA: 0x001CABDC File Offset: 0x001C8DDC
		private static void ShiftItemsInProjectileContainer(ItemContainer container)
		{
			if (container == null)
			{
				return;
			}
			bool moved;
			do
			{
				moved = false;
				for (int i = 1; i < container.Capacity; i++)
				{
					Item item = container.Inventory.GetItemAt(i);
					if (item != null && container.Inventory.CanBePutInSlot(item, i - 1, false) && container.Inventory.TryPutItem(item, i - 1, false, false, null, true, false, true))
					{
						moved = true;
					}
				}
			}
			while (moved);
		}

		// Token: 0x0600484D RID: 18509 RVA: 0x001CAC3E File Offset: 0x001C8E3E
		private float GetTargetPriorityModifier()
		{
			if (this.currentChargingState != Turret.ChargingState.WindingUp)
			{
				return this.AICurrentTargetPriorityMultiplier;
			}
			return 10f;
		}

		// Token: 0x0600484E RID: 18510 RVA: 0x001CAC58 File Offset: 0x001C8E58
		public void UpdateAutoOperate(float deltaTime, bool ignorePower, Identifier friendlyTag = default(Identifier))
		{
			if (!ignorePower && !this.HasPowerToShoot())
			{
				return;
			}
			this.IsActive = true;
			if (friendlyTag.IsEmpty)
			{
				friendlyTag = this.FriendlyTag;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.updatePending)
			{
				if (this.updateTimer < 0f)
				{
					this.item.CreateServerEvent<Turret>(this);
					this.prevTargetRotation = this.targetRotation;
					this.updateTimer = 0.25f;
				}
				this.updateTimer -= deltaTime;
			}
			if (this.AimDelay && this.waitTimer > 0f)
			{
				this.waitTimer -= deltaTime;
				return;
			}
			Submarine closestSub = null;
			float maxDistance = 10000f;
			float shootDistance = this.AIRange;
			ISpatialEntity target = null;
			float closestDist = shootDistance * shootDistance;
			if (this.TargetCharacters)
			{
				foreach (Character character in Character.CharacterList)
				{
					if (Turret.IsValidTarget(character))
					{
						float priority = this.isSlowTurret ? character.Params.AISlowTurretPriority : character.Params.AITurretPriority;
						if (priority > 0f && this.IsValidTargetForAutoOperate(character, friendlyTag))
						{
							float dist = Vector2.DistanceSquared(character.WorldPosition, this.item.WorldPosition);
							if (dist <= closestDist && this.IsWithinAimingRadius(character.WorldPosition))
							{
								target = character;
								if (this.currentTarget != null && target == this.currentTarget)
								{
									priority *= this.GetTargetPriorityModifier();
								}
								closestDist = dist / priority;
							}
						}
					}
				}
			}
			if (this.TargetItems)
			{
				foreach (Item targetItem in Item.TurretTargetItems)
				{
					if (Turret.IsValidTarget(targetItem))
					{
						float priority2 = this.isSlowTurret ? targetItem.Prefab.AISlowTurretPriority : targetItem.Prefab.AITurretPriority;
						if (priority2 > 0f)
						{
							float dist2 = Vector2.DistanceSquared(this.item.WorldPosition, targetItem.WorldPosition);
							if (dist2 <= closestDist && dist2 <= shootDistance * shootDistance && this.IsTargetItemCloseEnough(targetItem, dist2) && this.IsWithinAimingRadius(targetItem.WorldPosition))
							{
								target = targetItem;
								if (this.currentTarget != null && target == this.currentTarget)
								{
									priority2 *= this.GetTargetPriorityModifier();
								}
								closestDist = dist2 / priority2;
							}
						}
					}
				}
			}
			if (this.TargetSubmarines && (target == null || target.Submarine != null))
			{
				closestDist = maxDistance * maxDistance;
				foreach (Submarine sub in Submarine.Loaded)
				{
					if (sub != base.Item.Submarine && !sub.IsRespawnShuttle && (this.item.Submarine == null || !Character.IsOnFriendlyTeam(this.item.Submarine.TeamID, sub.TeamID)))
					{
						float dist3 = Vector2.DistanceSquared(sub.WorldPosition, this.item.WorldPosition);
						if (dist3 <= closestDist)
						{
							closestSub = sub;
							closestDist = dist3;
						}
					}
				}
				closestDist = shootDistance * shootDistance;
				if (closestSub != null)
				{
					foreach (Hull hull in Hull.HullList)
					{
						if (closestSub.IsEntityFoundOnThisSub(hull, true, false, false))
						{
							float dist4 = Vector2.DistanceSquared(hull.WorldPosition, this.item.WorldPosition);
							if (dist4 <= closestDist)
							{
								target = hull;
								closestDist = dist4;
							}
						}
					}
				}
			}
			if (target == null && this.RandomMovement)
			{
				this.waitTimer = ((Rand.Value(Rand.RandSync.Unsynced) < 0.98f) ? 0f : Rand.Range(5f, 20f, Rand.RandSync.Unsynced));
				this.targetRotation = Rand.Range(this.minRotation, this.maxRotation, Rand.RandSync.Unsynced);
				this.updatePending = true;
				return;
			}
			if (this.AimDelay && this.RandomAimAmount > 0f)
			{
				if (this.randomAimTimer < 0f)
				{
					this.randomAimTimer = Rand.Range(this.RandomAimMinTime, this.RandomAimMaxTime, Rand.RandSync.Unsynced);
					this.waitTimer = Rand.Range(0.25f, 1f, Rand.RandSync.Unsynced);
					float randomAim = MathHelper.ToRadians(this.RandomAimAmount);
					this.targetRotation = MathUtils.WrapAngleTwoPi(this.targetRotation += Rand.Range(-randomAim, randomAim, Rand.RandSync.Unsynced));
					this.updatePending = true;
					return;
				}
				this.randomAimTimer -= deltaTime;
			}
			if (target == null)
			{
				return;
			}
			this.currentTarget = target;
			float angle = -MathUtils.VectorToAngle(target.WorldPosition - this.item.WorldPosition);
			this.targetRotation = MathUtils.WrapAngleTwoPi(angle);
			if (Math.Abs(this.targetRotation - this.prevTargetRotation) > 0.1f)
			{
				this.updatePending = true;
			}
			Hull targetHull = target as Hull;
			if (targetHull != null)
			{
				Vector2 barrelDir = this.GetBarrelDir();
				Vector2 vector;
				if (!MathUtils.GetLineWorldRectangleIntersection(this.item.WorldPosition, this.item.WorldPosition + barrelDir * this.AIRange, targetHull.WorldRect, out vector))
				{
					return;
				}
			}
			else
			{
				if (!this.IsWithinAimingRadius(angle))
				{
					return;
				}
				if (!this.IsPointingTowards(target.WorldPosition))
				{
					return;
				}
			}
			Vector2 start = ConvertUnits.ToSimUnits(this.item.WorldPosition);
			Vector2 end = ConvertUnits.ToSimUnits(target.WorldPosition);
			bool doLineOfSightCheck = this.lastLineOfSightCheck.Item3 < Timing.TotalTimeUnpaused - 0.5;
			if (doLineOfSightCheck)
			{
				this.lastLineOfSightCheck.Item1 = this.CheckLineOfSight(start, end);
				this.lastLineOfSightCheck.Item3 = Timing.TotalTime;
			}
			Body worldTarget = this.lastLineOfSightCheck.Item1;
			bool shoot;
			if (target.Submarine != null)
			{
				if (doLineOfSightCheck)
				{
					start -= target.Submarine.SimPosition;
					end -= target.Submarine.SimPosition;
					this.lastLineOfSightCheck.Item2 = this.CheckLineOfSight(start, end);
				}
				shoot = ((worldTarget == null || this.CanShoot(worldTarget, null, friendlyTag, this.TargetSubmarines, false)) && this.CanShoot(this.lastLineOfSightCheck.Item2, null, friendlyTag, this.TargetSubmarines, false));
			}
			else
			{
				shoot = this.CanShoot(worldTarget, null, friendlyTag, this.TargetSubmarines, false);
			}
			if (shoot)
			{
				this.TryLaunch(deltaTime, null, ignorePower);
			}
		}

		// Token: 0x0600484F RID: 18511 RVA: 0x001CB2F4 File Offset: 0x001C94F4
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			Turret.<>c__DisplayClass257_0 CS$<>8__locals1 = new Turret.<>c__DisplayClass257_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			AITarget selectedAiTarget = CS$<>8__locals1.character.AIController.SelectedAiTarget;
			Character previousTarget = ((selectedAiTarget != null) ? selectedAiTarget.Entity : null) as Character;
			if (previousTarget != null && previousTarget.IsDead)
			{
				if (previousTarget.LastAttacker == null || previousTarget.LastAttacker == CS$<>8__locals1.character)
				{
					Character character2 = CS$<>8__locals1.character;
					string value = TextManager.Get("DialogTurretTargetDead").Value;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("killedtarget");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(previousTarget.ID);
					Identifier identifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
					character2.Speak(value, null, 0f, identifier, 5f);
				}
				CS$<>8__locals1.character.AIController.SelectTarget(null);
			}
			if (!this.HasPowerToShoot())
			{
				float lowestCharge = 0f;
				PowerContainer batteryToLoad = null;
				foreach (PowerContainer battery in base.GetDirectlyConnectedBatteries())
				{
					if (battery.Item.IsInteractable(CS$<>8__locals1.character) && !battery.OutputDisabled)
					{
						if (batteryToLoad == null || battery.Charge < lowestCharge)
						{
							batteryToLoad = battery;
							lowestCharge = battery.Charge;
						}
						if (battery.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(battery.Item, CS$<>8__locals1.character))
						{
							IEnumerable<Repairable> repairables = battery.Item.Repairables;
							Func<Repairable, float> selector;
							if ((selector = CS$<>8__locals1.<>9__0) == null)
							{
								selector = (CS$<>8__locals1.<>9__0 = ((Repairable r) => r.DegreeOfSuccess(CS$<>8__locals1.character)));
							}
							if (repairables.Average(selector) > 0.4f)
							{
								objective.AddSubObjective(new AIObjectiveRepairItem(CS$<>8__locals1.character, battery.Item, objective.objectiveManager, 1f, true), false);
								return false;
							}
							Character character3 = CS$<>8__locals1.character;
							string value2 = TextManager.Get("DialogSupercapacitorIsBroken").Value;
							Identifier identifier = "supercapacitorisbroken".ToIdentifier();
							character3.Speak(value2, null, 0f, identifier, 30f);
						}
					}
				}
				if (batteryToLoad == null)
				{
					return true;
				}
				if (batteryToLoad.RechargeSpeed < batteryToLoad.MaxRechargeSpeed * 0.4f)
				{
					objective.AddSubObjective(new AIObjectiveOperateItem(batteryToLoad, CS$<>8__locals1.character, objective.objectiveManager, Identifier.Empty, false, null, false, null, 1f), false);
					return false;
				}
				if (lowestCharge <= 0f && batteryToLoad.Item.ConditionPercentage > 0f)
				{
					Character character4 = CS$<>8__locals1.character;
					string value3 = TextManager.Get("DialogTurretHasNoPower").Value;
					Identifier identifier = "turrethasnopower".ToIdentifier();
					character4.Speak(value3, null, 0f, identifier, 30f);
				}
			}
			int usableProjectileCount = 0;
			int maxProjectileCount = 0;
			foreach (MapEntity e in this.item.linkedTo)
			{
				if (this.item.IsInteractable(CS$<>8__locals1.character) && this.item.Prefab.IsLinkAllowed(e.Prefab))
				{
					Item projectileContainer = e as Item;
					if (projectileContainer != null)
					{
						ItemContainer container = projectileContainer.GetComponent<ItemContainer>();
						if (container != null)
						{
							maxProjectileCount += container.Capacity;
							IEnumerable<Item> projectiles = from it in projectileContainer.ContainedItems
							where it.Condition > 0f
							select it;
							Item firstProjectile = projectiles.FirstOrDefault<Item>();
							ItemPrefab itemPrefab = (firstProjectile != null) ? firstProjectile.Prefab : null;
							Item item = this.previousAmmo;
							if (itemPrefab != ((item != null) ? item.Prefab : null))
							{
								this.projectileSpeed = float.PositiveInfinity;
							}
							this.previousAmmo = firstProjectile;
							if (projectiles.Any<Item>())
							{
								Projectile projectile2;
								if ((projectile2 = firstProjectile.GetComponent<Projectile>()) == null)
								{
									Item item2 = firstProjectile.ContainedItems.FirstOrDefault<Item>();
									projectile2 = ((item2 != null) ? item2.GetComponent<Projectile>() : null);
								}
								Projectile projectile = projectile2;
								this.TryDetermineProjectileSpeed(projectile);
								usableProjectileCount += projectiles.Count<Item>();
							}
						}
					}
				}
			}
			if (usableProjectileCount == 0)
			{
				Turret.<>c__DisplayClass257_1 CS$<>8__locals2 = new Turret.<>c__DisplayClass257_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.container = null;
				Item containerItem = null;
				foreach (MapEntity e2 in this.item.linkedTo)
				{
					containerItem = (e2 as Item);
					if (containerItem != null && containerItem.IsInteractable(CS$<>8__locals2.CS$<>8__locals1.character))
					{
						HumanAIController aiController = CS$<>8__locals2.CS$<>8__locals1.character.AIController as HumanAIController;
						if (aiController == null || !aiController.IgnoredItems.Contains(containerItem))
						{
							CS$<>8__locals2.container = containerItem.GetComponent<ItemContainer>();
							if (CS$<>8__locals2.container != null)
							{
								break;
							}
						}
					}
				}
				if (CS$<>8__locals2.container == null || !CS$<>8__locals2.container.ContainableItemIdentifiers.Any<Identifier>())
				{
					if (CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam)
					{
						Character character5 = CS$<>8__locals2.CS$<>8__locals1.character;
						string value4 = TextManager.GetWithVariable("DialogCannotLoadTurret", "[itemname]", this.item.Name, FormatCapitals.Yes).Value;
						Identifier identifier = "cannotloadturret".ToIdentifier();
						character5.Speak(value4, null, 0f, identifier, 30f);
					}
					return true;
				}
				if (objective.SubObjectives.None(null))
				{
					AIObjectiveContainItem loadItemsObjective = base.AIContainItems<Turret>(CS$<>8__locals2.container, CS$<>8__locals2.CS$<>8__locals1.character, objective, usableProjectileCount + 1, true, true, false, true);
					loadItemsObjective.ignoredContainerIdentifiers = containerItem.Prefab.Identifier.ToEnumerable<Identifier>().ToImmutableHashSet<Identifier>();
					if (CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam)
					{
						Character character6 = CS$<>8__locals2.CS$<>8__locals1.character;
						string value5 = TextManager.GetWithVariable("DialogLoadTurret", "[itemname]", this.item.Name, FormatCapitals.Yes).Value;
						Identifier identifier = "loadturret".ToIdentifier();
						character6.Speak(value5, null, 0f, identifier, 30f);
					}
					loadItemsObjective.Abandoned += CS$<>8__locals2.<CrewAIOperate>g__CheckRemainingAmmo|2;
					loadItemsObjective.Completed += CS$<>8__locals2.<CrewAIOperate>g__CheckRemainingAmmo|2;
					return false;
				}
				if (objective.SubObjectives.Any<AIObjective>())
				{
					return false;
				}
			}
			CS$<>8__locals1.closestEnemy = null;
			CS$<>8__locals1.targetPos = null;
			float maxDistance = 10000f;
			float shootDistance = this.AIRange * this.item.OffsetOnSelectedMultiplier;
			float closestDistance = maxDistance * maxDistance;
			bool hadCurrentTarget = this.currentTarget != null;
			if (hadCurrentTarget)
			{
				bool isValidTarget = Turret.IsValidTarget(this.currentTarget);
				if (isValidTarget)
				{
					float dist = Vector2.DistanceSquared(this.item.WorldPosition, this.currentTarget.WorldPosition);
					if (dist > closestDistance)
					{
						isValidTarget = false;
					}
					else
					{
						Item targetItem = this.currentTarget as Item;
						if (targetItem != null && !this.IsTargetItemCloseEnough(targetItem, dist))
						{
							isValidTarget = false;
						}
					}
				}
				if (!isValidTarget)
				{
					this.currentTarget = null;
					this.aiFindTargetTimer = 0.2f;
				}
			}
			if (this.aiFindTargetTimer <= 0f)
			{
				foreach (Character enemy in Character.CharacterList)
				{
					if (Turret.IsValidTarget(enemy))
					{
						float priority = this.isSlowTurret ? enemy.Params.AISlowTurretPriority : enemy.Params.AITurretPriority;
						if (priority > 0f && (CS$<>8__locals1.character.Submarine == null || (enemy.Submarine != CS$<>8__locals1.character.Submarine && (enemy.Submarine == null || (enemy.Submarine.TeamID != CS$<>8__locals1.character.Submarine.TeamID && !enemy.Submarine.Info.IsOutpost)))) && (enemy.IsHuman || enemy.CurrentHull == null) && !HumanAIController.IsFriendly(CS$<>8__locals1.character, enemy, false, true) && !enemy.LockHands)
						{
							float dist2 = Vector2.DistanceSquared(enemy.WorldPosition, this.item.WorldPosition);
							if (dist2 <= closestDistance && (dist2 >= shootDistance * shootDistance || this.IsWithinAimingRadius(enemy.WorldPosition)))
							{
								if (this.currentTarget != null && enemy == this.currentTarget)
								{
									priority *= this.GetTargetPriorityModifier();
								}
								CS$<>8__locals1.targetPos = new Vector2?(enemy.WorldPosition);
								CS$<>8__locals1.closestEnemy = enemy;
								closestDistance = dist2 / priority;
								this.currentTarget = CS$<>8__locals1.closestEnemy;
							}
						}
					}
				}
				foreach (Item targetItem2 in Item.TurretTargetItems)
				{
					if (Turret.IsValidTarget(targetItem2))
					{
						float priority2 = this.isSlowTurret ? targetItem2.Prefab.AISlowTurretPriority : targetItem2.Prefab.AITurretPriority;
						if (priority2 > 0f)
						{
							float dist3 = Vector2.DistanceSquared(this.item.WorldPosition, targetItem2.WorldPosition);
							if (dist3 <= closestDistance && dist3 <= shootDistance * shootDistance && this.IsTargetItemCloseEnough(targetItem2, dist3) && this.IsWithinAimingRadius(targetItem2.WorldPosition))
							{
								if (this.currentTarget != null && targetItem2 == this.currentTarget)
								{
									priority2 *= this.GetTargetPriorityModifier();
								}
								CS$<>8__locals1.targetPos = new Vector2?(targetItem2.WorldPosition);
								closestDistance = dist3 / priority2;
								CS$<>8__locals1.closestEnemy = null;
								this.currentTarget = targetItem2;
							}
						}
					}
				}
				this.aiFindTargetTimer = ((this.currentTarget == null) ? 1f : 0.2f);
			}
			else if (this.currentTarget != null)
			{
				CS$<>8__locals1.targetPos = new Vector2?(this.currentTarget.WorldPosition);
			}
			bool iceSpireSpotted = false;
			Vector2 targetVelocity = Vector2.Zero;
			Character targetCharacter = this.currentTarget as Character;
			if (targetCharacter != null)
			{
				bool enemyInAnotherSub = targetCharacter.Submarine != null && targetCharacter.CurrentHull != null && targetCharacter.Submarine != this.item.Submarine;
				bool canSeeTarget = true;
				if (enemyInAnotherSub && (this.lastCanSeeTargetCheck.Item3 < Timing.TotalTime - 0.5 || targetCharacter != this.lastCanSeeTargetCheck.Item1))
				{
					canSeeTarget = targetCharacter.CanSeeTarget(base.Item, null, false, false);
					this.lastCanSeeTargetCheck = new ValueTuple<Character, bool, double>(targetCharacter, canSeeTarget, Timing.TotalTime);
				}
				if (enemyInAnotherSub && !canSeeTarget)
				{
					CS$<>8__locals1.targetPos = new Vector2?(targetCharacter.CurrentHull.WorldPosition);
					if (closestDistance > maxDistance * maxDistance)
					{
						CS$<>8__locals1.<CrewAIOperate>g__ResetTarget|4();
					}
				}
				else
				{
					float closestDistSqr = closestDistance;
					foreach (Limb limb in targetCharacter.AnimController.Limbs)
					{
						if (!limb.IsSevered && !limb.Hidden && this.IsWithinAimingRadius(limb.WorldPosition))
						{
							float distSqr = Vector2.DistanceSquared(limb.WorldPosition, this.item.WorldPosition);
							if (distSqr < closestDistSqr)
							{
								closestDistSqr = distSqr;
								if (limb == targetCharacter.AnimController.MainLimb)
								{
									closestDistSqr *= 0.5f;
								}
								CS$<>8__locals1.targetPos = new Vector2?(limb.WorldPosition);
							}
						}
					}
					if (this.projectileSpeed < float.PositiveInfinity && CS$<>8__locals1.targetPos != null)
					{
						float dist4 = MathF.Sqrt(closestDistSqr);
						float projectileMovementTime = dist4 / this.projectileSpeed;
						targetVelocity = targetCharacter.AnimController.Collider.LinearVelocity;
						Vector2 movementAmount = targetVelocity * projectileMovementTime;
						movementAmount = ConvertUnits.ToDisplayUnits(movementAmount.ClampLength(10f));
						Vector2 futurePosition = CS$<>8__locals1.targetPos.Value + movementAmount;
						CS$<>8__locals1.targetPos = new Vector2?(Vector2.Lerp(CS$<>8__locals1.targetPos.Value, futurePosition, base.DegreeOfSuccess(CS$<>8__locals1.character)));
					}
					if (closestDistSqr > shootDistance * shootDistance)
					{
						this.aiFindTargetTimer = 0.2f;
						CS$<>8__locals1.<CrewAIOperate>g__ResetTarget|4();
					}
				}
			}
			else if (CS$<>8__locals1.targetPos == null && this.item.Submarine != null && Level.Loaded != null)
			{
				shootDistance = this.AIRange * this.item.OffsetOnSelectedMultiplier;
				closestDistance = shootDistance;
				foreach (LevelWall wall in Level.Loaded.ExtraWalls)
				{
					DestructibleLevelWall destructibleWall = wall as DestructibleLevelWall;
					if (destructibleWall != null && !destructibleWall.Destroyed)
					{
						foreach (VoronoiCell cell in wall.Cells)
						{
							if (cell.DoesDamage)
							{
								foreach (GraphEdge edge in cell.Edges)
								{
									Vector2 p = edge.Point1 + cell.Translation;
									Vector2 p2 = edge.Point2 + cell.Translation;
									Vector2 closestPoint = MathUtils.GetClosestPointOnLineSegment(p, p2, this.item.WorldPosition);
									if (!this.IsWithinAimingRadius(closestPoint))
									{
										Vector2 barrelDir = new Vector2((float)Math.Cos((double)this.Rotation), -(float)Math.Sin((double)this.Rotation));
										Vector2 intersection;
										if (!MathUtils.GetLineSegmentIntersection(p, p2, this.item.WorldPosition, this.item.WorldPosition + barrelDir * shootDistance, out intersection))
										{
											continue;
										}
										closestPoint = intersection;
										if (!this.IsWithinAimingRadius(closestPoint))
										{
											continue;
										}
									}
									float dist5 = Vector2.Distance(closestPoint, this.item.WorldPosition);
									closestPoint += (closestPoint - this.item.WorldPosition) / Math.Max(dist5, 1f);
									if (dist5 <= this.AIRange + 1000f)
									{
										float dot = 0f;
										if (!MathUtils.NearlyEqual(this.item.Submarine.Velocity, Vector2.Zero, 0.0001f))
										{
											dot = Vector2.Dot(Vector2.Normalize(this.item.Submarine.Velocity), Vector2.Normalize(closestPoint - this.item.Submarine.WorldPosition));
										}
										float minAngle = 0.5f;
										if (dot >= minAngle || dist5 <= 1000f)
										{
											dist5 -= MathHelper.Lerp(0f, 1000f, MathUtils.InverseLerp(minAngle, 1f, dot));
											if (dist5 <= closestDistance)
											{
												CS$<>8__locals1.targetPos = new Vector2?(closestPoint);
												closestDistance = dist5;
												iceSpireSpotted = true;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (CS$<>8__locals1.targetPos == null)
			{
				return false;
			}
			objective.ForceHighestPriority = true;
			if (CS$<>8__locals1.closestEnemy != null && CS$<>8__locals1.character.AIController.SelectedAiTarget != CS$<>8__locals1.closestEnemy.AiTarget)
			{
				if (CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					if (CS$<>8__locals1.character.AIController.SelectedAiTarget == null && !hadCurrentTarget)
					{
						if (CreatureMetrics.RecentlyEncountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName) || CS$<>8__locals1.closestEnemy.IsHuman)
						{
							Character character7 = CS$<>8__locals1.character;
							string value6 = TextManager.Get("DialogNewTargetSpotted").Value;
							Identifier identifier = "newtargetspotted".ToIdentifier();
							character7.Speak(value6, null, 0f, identifier, 30f);
						}
						else if (CreatureMetrics.Encountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName))
						{
							Character character8 = CS$<>8__locals1.character;
							string value7 = TextManager.GetWithVariable("DialogIdentifiedTargetSpotted", "[speciesname]", CS$<>8__locals1.closestEnemy.DisplayName, FormatCapitals.No).Value;
							Identifier identifier = "identifiedtargetspotted".ToIdentifier();
							character8.Speak(value7, null, 0f, identifier, 30f);
						}
						else
						{
							Character character9 = CS$<>8__locals1.character;
							string value8 = TextManager.Get("DialogUnidentifiedTargetSpotted").Value;
							Identifier identifier = "unidentifiedtargetspotted".ToIdentifier();
							character9.Speak(value8, null, 0f, identifier, 5f);
						}
					}
					else if (!CreatureMetrics.Encountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName))
					{
						Character character10 = CS$<>8__locals1.character;
						string value9 = TextManager.Get("DialogUnidentifiedTargetSpotted").Value;
						Identifier identifier = "unidentifiedtargetspotted".ToIdentifier();
						character10.Speak(value9, null, 0f, identifier, 5f);
					}
					CreatureMetrics.AddEncounter(CS$<>8__locals1.closestEnemy.SpeciesName);
				}
				CS$<>8__locals1.character.AIController.SelectTarget(CS$<>8__locals1.closestEnemy.AiTarget);
			}
			else if (iceSpireSpotted && CS$<>8__locals1.character.IsOnPlayerTeam)
			{
				Character character11 = CS$<>8__locals1.character;
				string value10 = TextManager.Get("DialogIceSpireSpotted").Value;
				Identifier identifier = "icespirespotted".ToIdentifier();
				character11.Speak(value10, null, 0f, identifier, 60f);
			}
			CS$<>8__locals1.character.CursorPosition = CS$<>8__locals1.targetPos.Value;
			if (CS$<>8__locals1.character.Submarine != null)
			{
				CS$<>8__locals1.character.CursorPosition -= CS$<>8__locals1.character.Submarine.Position;
			}
			if (this.IsPointingTowards(CS$<>8__locals1.targetPos.Value))
			{
				Vector2 barrelDir2 = this.GetBarrelDir();
				Vector2 aimStartPos = this.item.WorldPosition;
				Vector2 aimEndPos = this.item.WorldPosition + barrelDir2 * shootDistance;
				bool allowShootingIfNothingInWay = false;
				if (this.currentTarget != null)
				{
					Vector2 targetStartPos = this.currentTarget.WorldPosition;
					Vector2 targetEndPos = this.currentTarget.WorldPosition + targetVelocity * ConvertUnits.ToDisplayUnits(10f);
					allowShootingIfNothingInWay = (targetVelocity.LengthSquared() > 0.001f && MathUtils.LineSegmentsIntersect(aimStartPos, aimEndPos, targetStartPos, targetEndPos) && Math.Abs(Vector2.Dot(Vector2.Normalize(aimEndPos - aimStartPos), Vector2.Normalize(targetEndPos - targetStartPos))) < 0.5f);
				}
				Vector2 start = ConvertUnits.ToSimUnits(aimStartPos);
				Vector2 end = ConvertUnits.ToSimUnits(aimEndPos);
				Body worldTarget = this.CheckLineOfSight(start, end);
				bool canShoot;
				if (CS$<>8__locals1.closestEnemy != null && CS$<>8__locals1.closestEnemy.Submarine != null)
				{
					start -= CS$<>8__locals1.closestEnemy.Submarine.SimPosition;
					end -= CS$<>8__locals1.closestEnemy.Submarine.SimPosition;
					Body transformedTarget = this.CheckLineOfSight(start, end);
					Body targetBody = transformedTarget;
					Character character12 = CS$<>8__locals1.character;
					bool allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
					bool flag;
					if (this.CanShoot(targetBody, character12, default(Identifier), true, allowShootingIfNothingInWay2))
					{
						if (worldTarget != null)
						{
							Body targetBody2 = worldTarget;
							Character character13 = CS$<>8__locals1.character;
							allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
							flag = this.CanShoot(targetBody2, character13, default(Identifier), true, allowShootingIfNothingInWay2);
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = false;
					}
					canShoot = flag;
				}
				else
				{
					Body targetBody3 = worldTarget;
					Character character14 = CS$<>8__locals1.character;
					bool allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
					canShoot = this.CanShoot(targetBody3, character14, default(Identifier), true, allowShootingIfNothingInWay2);
				}
				if (!canShoot)
				{
					return false;
				}
				if (CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					Character character15 = CS$<>8__locals1.character;
					string value11 = TextManager.Get("DialogFireTurret").Value;
					Identifier identifier = "fireturret".ToIdentifier();
					character15.Speak(value11, null, 0f, identifier, 30f);
				}
				CS$<>8__locals1.character.SetInput(InputType.Shoot, true, true);
			}
			return false;
		}

		// Token: 0x06004850 RID: 18512 RVA: 0x001CC744 File Offset: 0x001CA944
		private bool IsPointingTowards(Vector2 targetPos)
		{
			float enemyAngle = MathUtils.VectorToAngle(targetPos - this.item.WorldPosition);
			float turretAngle = -this.Rotation;
			float maxAngleError = MathHelper.ToRadians(this.MaxAngleOffset);
			if (this.MaxChargeTime > 0f && this.currentChargingState == Turret.ChargingState.WindingUp && this.FiringRotationSpeedModifier > 0f)
			{
				maxAngleError *= 2f;
			}
			return Math.Abs(MathUtils.GetShortestAngle(enemyAngle, turretAngle)) <= maxAngleError;
		}

		// Token: 0x06004851 RID: 18513 RVA: 0x001CC7B9 File Offset: 0x001CA9B9
		private bool IsTargetItemCloseEnough(Item target, float sqrDist)
		{
			return float.IsPositiveInfinity(target.Prefab.AITurretTargetingMaxDistance) || sqrDist < MathUtils.Pow2(target.Prefab.AITurretTargetingMaxDistance);
		}

		// Token: 0x06004852 RID: 18514 RVA: 0x001CC7E2 File Offset: 0x001CA9E2
		public override float GetCurrentPowerConsumption(Connection conn = null)
		{
			return 0f;
		}

		// Token: 0x06004853 RID: 18515 RVA: 0x001CC7EC File Offset: 0x001CA9EC
		private static bool IsValidTarget(ISpatialEntity target)
		{
			if (target == null)
			{
				return false;
			}
			Character targetCharacter = target as Character;
			if (targetCharacter != null)
			{
				if (!targetCharacter.Enabled || targetCharacter.Removed || targetCharacter.IsDead || targetCharacter.AITurretPriority <= 0f)
				{
					return false;
				}
			}
			else
			{
				Item targetItem = target as Item;
				if (targetItem != null)
				{
					if (targetItem.Removed || targetItem.Condition <= 0f || !targetItem.Prefab.IsAITurretTarget || targetItem.Prefab.AITurretPriority <= 0f || targetItem.IsHidden)
					{
						return false;
					}
					if (targetItem.Submarine != null)
					{
						return false;
					}
					if (targetItem.ParentInventory != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06004854 RID: 18516 RVA: 0x001CC88C File Offset: 0x001CAA8C
		private CharacterTeamType GetFriendlyTeam()
		{
			CharacterTeamType result;
			switch (this.FriendlyTeamType)
			{
			case Turret.TeamType.OwnSub:
			{
				Submarine submarine = this.item.Submarine;
				result = ((submarine != null) ? submarine.TeamID : CharacterTeamType.None);
				break;
			}
			case Turret.TeamType.Team1:
				result = CharacterTeamType.Team1;
				break;
			case Turret.TeamType.Team2:
				result = CharacterTeamType.Team2;
				break;
			case Turret.TeamType.FriendlyNPC:
				result = CharacterTeamType.FriendlyNPC;
				break;
			case Turret.TeamType.NoneTeam:
				result = CharacterTeamType.None;
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		// Token: 0x06004855 RID: 18517 RVA: 0x001CC8F0 File Offset: 0x001CAAF0
		private bool IsValidTargetForAutoOperate(Character target, Identifier friendlyTag)
		{
			if (!friendlyTag.IsEmpty)
			{
				Identifier identifier = target.SpeciesName;
				if (!identifier.Equals(friendlyTag))
				{
					identifier = target.Group;
					if (!identifier.Equals(friendlyTag))
					{
						goto IL_2D;
					}
				}
				return false;
			}
			IL_2D:
			CharacterTeamType friendlyTeam = this.GetFriendlyTeam();
			if (target.TeamID == friendlyTeam)
			{
				return false;
			}
			bool flag;
			if (!target.IsHuman)
			{
				Identifier identifier = target.Group;
				flag = (identifier == CharacterPrefab.HumanSpeciesName);
			}
			else
			{
				flag = true;
			}
			bool isHuman = flag;
			if (isHuman)
			{
				return !target.IsOnFriendlyTeam(friendlyTeam) && this.TargetHumans;
			}
			return this.TargetMonsters;
		}

		// Token: 0x06004856 RID: 18518 RVA: 0x001CC978 File Offset: 0x001CAB78
		private bool CanShoot(Body targetBody, Character user = null, Identifier friendlyTag = default(Identifier), bool targetSubmarines = true, bool allowShootingIfNothingInWay = false)
		{
			if (targetBody == null)
			{
				return allowShootingIfNothingInWay;
			}
			Character targetCharacter = null;
			Character c = targetBody.UserData as Character;
			if (c != null)
			{
				targetCharacter = c;
			}
			else
			{
				Limb limb = targetBody.UserData as Limb;
				if (limb != null)
				{
					targetCharacter = limb.character;
				}
			}
			if (targetCharacter != null && !targetCharacter.Removed)
			{
				if (user != null)
				{
					if (HumanAIController.IsFriendly(user, targetCharacter, false, true))
					{
						return false;
					}
				}
				else if (!this.IsValidTargetForAutoOperate(targetCharacter, friendlyTag))
				{
					return false;
				}
			}
			else
			{
				ISpatialEntity e = targetBody.UserData as ISpatialEntity;
				if (e != null)
				{
					Structure structure = e as Structure;
					if (structure != null && structure.Indestructible)
					{
						return false;
					}
					if (!targetSubmarines && e is Submarine)
					{
						return false;
					}
					Submarine sub = e.Submarine ?? (e as Submarine);
					if (sub == null)
					{
						return true;
					}
					if (sub == base.Item.Submarine)
					{
						return false;
					}
					if (sub.Info.IsOutpost || sub.Info.IsWreck || sub.Info.IsBeacon || sub.Info.IsRuin)
					{
						return false;
					}
					if (this.item.Submarine == null)
					{
						if (sub.TeamID == this.GetFriendlyTeam())
						{
							return false;
						}
					}
					else if (sub.TeamID == base.Item.Submarine.TeamID)
					{
						return false;
					}
				}
				else
				{
					VoronoiCell voronoiCell = targetBody.UserData as VoronoiCell;
					if (voronoiCell == null || !voronoiCell.IsDestructible)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06004857 RID: 18519 RVA: 0x001CCAD4 File Offset: 0x001CACD4
		private Body CheckLineOfSight(Vector2 start, Vector2 end)
		{
			Category collisionCategories = Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8;
			return Submarine.PickBody(start, end, null, new Category?(collisionCategories), true, delegate(Fixture f)
			{
				Item i = f.UserData as Item;
				return (i == null || i.GetComponent<Turret>() == null) && f.CollidesWith != Category.None && f.Body.UserData != this.item && !(f.UserData is Hull) && !this.item.StaticFixtures.Contains(f);
			}, true);
		}

		// Token: 0x06004858 RID: 18520 RVA: 0x001CCB08 File Offset: 0x001CAD08
		private Vector2 GetRelativeFiringPosition(bool useOffset = true)
		{
			Vector2 transformedFiringOffset = Vector2.Zero;
			if (useOffset)
			{
				Vector2 currOffSet = this.FiringOffset;
				if (this.flipFiringOffset)
				{
					currOffSet.X = -currOffSet.X;
				}
				transformedFiringOffset = MathUtils.RotatePoint(new Vector2(-currOffSet.Y, -currOffSet.X) * this.item.Scale, -this.Rotation);
			}
			return new Vector2((float)this.item.WorldRect.X + this.transformedBarrelPos.X + transformedFiringOffset.X, (float)this.item.WorldRect.Y - this.transformedBarrelPos.Y + transformedFiringOffset.Y);
		}

		// Token: 0x06004859 RID: 18521 RVA: 0x001CCBB8 File Offset: 0x001CADB8
		private bool IsWithinAimingRadius(float angle)
		{
			float midRotation = (this.minRotation + this.maxRotation) / 2f;
			while (midRotation - angle < -3.1415927f)
			{
				angle -= 6.2831855f;
			}
			while (midRotation - angle > 3.1415927f)
			{
				angle += 6.2831855f;
			}
			return angle >= this.minRotation && angle <= this.maxRotation;
		}

		// Token: 0x0600485A RID: 18522 RVA: 0x001CCC1A File Offset: 0x001CAE1A
		public bool IsWithinAimingRadius(Vector2 target)
		{
			return this.IsWithinAimingRadius(-MathUtils.VectorToAngle(target - this.item.WorldPosition));
		}

		// Token: 0x0600485B RID: 18523 RVA: 0x001CCC3C File Offset: 0x001CAE3C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.barrelSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.barrelSprite = null;
			Sprite sprite2 = this.railSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			this.railSprite = null;
			Sprite sprite3 = this.barrelSpriteBroken;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			this.barrelSpriteBroken = null;
			Sprite sprite4 = this.railSpriteBroken;
			if (sprite4 != null)
			{
				sprite4.Remove();
			}
			this.railSpriteBroken = null;
		}

		// Token: 0x0600485C RID: 18524 RVA: 0x001CCCB0 File Offset: 0x001CAEB0
		private List<Projectile> GetLoadedProjectiles()
		{
			List<Projectile> projectiles = new List<Projectile>();
			bool flag;
			Turret.CheckProjectileContainer(this.item, projectiles, out flag);
			for (int i = 0; i < this.item.linkedTo.Count; i++)
			{
				MapEntity e = this.item.linkedTo[(i + this.currentLoaderIndex) % this.item.linkedTo.Count];
				if (this.item.Prefab.IsLinkAllowed(e.Prefab))
				{
					Item projectileContainer = e as Item;
					if (projectileContainer != null)
					{
						bool stopSearching;
						Turret.CheckProjectileContainer(projectileContainer, projectiles, out stopSearching);
						if (projectiles.Any<Projectile>() || stopSearching)
						{
							return projectiles;
						}
					}
				}
			}
			return projectiles;
		}

		// Token: 0x0600485D RID: 18525 RVA: 0x001CCD54 File Offset: 0x001CAF54
		private static void CheckProjectileContainer(Item projectileContainer, List<Projectile> projectiles, out bool stopSearching)
		{
			stopSearching = false;
			if (projectileContainer.Condition <= 0f)
			{
				return;
			}
			IEnumerable<Item> containedItems = projectileContainer.ContainedItems;
			if (containedItems == null)
			{
				return;
			}
			foreach (Item containedItem in containedItems)
			{
				Projectile projectileComponent = containedItem.GetComponent<Projectile>();
				if (projectileComponent != null && projectileComponent.Item.body != null)
				{
					projectiles.Add(projectileComponent);
					break;
				}
				foreach (Item subContainedItem in containedItem.ContainedItems)
				{
					projectileComponent = subContainedItem.GetComponent<Projectile>();
					if (projectileComponent != null && projectileComponent.Item.body != null)
					{
						projectiles.Add(projectileComponent);
					}
				}
				if (containedItem.Condition > 0f || projectiles.Any<Projectile>())
				{
					stopSearching = true;
					break;
				}
			}
		}

		// Token: 0x0600485E RID: 18526 RVA: 0x001CCE4C File Offset: 0x001CB04C
		public override void FlipX(bool relativeToSub)
		{
			this.minRotation = 3.1415927f - this.minRotation;
			this.maxRotation = 3.1415927f - this.maxRotation;
			float temp = this.minRotation;
			this.minRotation = this.maxRotation;
			this.maxRotation = temp;
			this.barrelPos.X = (float)this.item.Rect.Width / this.item.Scale - this.barrelPos.X;
			while (this.minRotation < 0f)
			{
				this.minRotation += 6.2831855f;
				this.maxRotation += 6.2831855f;
			}
			this.targetRotation = (this.Rotation = (this.minRotation + this.maxRotation) / 2f);
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x0600485F RID: 18527 RVA: 0x001CCF2C File Offset: 0x001CB12C
		public override void FlipY(bool relativeToSub)
		{
			this.BaseRotation = MathHelper.ToDegrees(MathUtils.WrapAngleTwoPi(MathHelper.ToRadians(180f - this.BaseRotation)));
			this.minRotation = -this.minRotation;
			this.maxRotation = -this.maxRotation;
			float temp = this.minRotation;
			this.minRotation = this.maxRotation;
			this.maxRotation = temp;
			while (this.minRotation < 0f)
			{
				this.minRotation += 6.2831855f;
				this.maxRotation += 6.2831855f;
			}
			this.targetRotation = (this.Rotation = (this.minRotation + this.maxRotation) / 2f);
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x06004860 RID: 18528 RVA: 0x001CCFF0 File Offset: 0x001CB1F0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			Character sender = signal.sender;
			string name = connection.Name;
			if (!(name == "position_in"))
			{
				if (!(name == "trigger_in"))
				{
					if (!(name == "toggle_light"))
					{
						if (!(name == "set_light"))
						{
							if (!(name == "set_auto_operate"))
							{
								if (!(name == "toggle_auto_operate"))
								{
									return;
								}
								if (!this.AllowAutoOperateWithWiring)
								{
									return;
								}
								if (signal.value != "0")
								{
									this.AutoOperate = !this.AutoOperate;
								}
							}
							else
							{
								if (!this.AllowAutoOperateWithWiring)
								{
									return;
								}
								this.AutoOperate = (signal.value != "0");
								return;
							}
						}
						else if (this.lightComponents != null)
						{
							bool shouldBeOn = signal.value != "0";
							foreach (LightComponent light in this.lightComponents)
							{
								light.IsOn = shouldBeOn;
							}
							this.UpdateLightComponents();
							return;
						}
					}
					else if (this.lightComponents != null && signal.value != "0")
					{
						foreach (LightComponent light2 in this.lightComponents)
						{
							light2.IsOn = !light2.IsOn;
						}
						this.UpdateLightComponents();
						return;
					}
				}
				else
				{
					if (signal.value == "0")
					{
						return;
					}
					this.item.Use(0.016666668f, sender, null, null, null);
					this.user = sender;
					this.ActiveUser = sender;
					this.resetActiveUserTimer = 1f;
					this.resetUserTimer = 10f;
					if (!this.characterUsable && sender != null)
					{
						this.TryLaunch(0.016666668f, sender, false);
						return;
					}
				}
				return;
			}
			float newRotation;
			if (float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newRotation))
			{
				if (!MathUtils.IsValid(newRotation))
				{
					return;
				}
				this.targetRotation = MathHelper.ToRadians(newRotation);
				this.IsActive = true;
			}
			this.user = sender;
			this.ActiveUser = sender;
			this.resetActiveUserTimer = 1f;
			this.resetUserTimer = 10f;
		}

		// Token: 0x06004861 RID: 18529 RVA: 0x001CD25C File Offset: 0x001CB45C
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			string key = "rotationlimits";
			Vector2 rotationLimits = this.RotationLimits;
			this.loadedRotationLimits = new Vector2?(componentElement.GetAttributeVector2(key, rotationLimits));
			this.loadedBaseRotation = new float?(componentElement.GetAttributeFloat("baserotation", componentElement.Parent.GetAttributeFloat("rotation", this.BaseRotation)));
			XAttribute friendlyTeamAttribute = componentElement.GetAttribute("FriendlyTeam");
			if (friendlyTeamAttribute != null)
			{
				Turret.TeamType value;
				switch (XMLExtensions.ParseEnumValue<CharacterTeamType>(friendlyTeamAttribute.Value, CharacterTeamType.None, friendlyTeamAttribute))
				{
				case CharacterTeamType.None:
					value = Turret.TeamType.OwnSub;
					break;
				case CharacterTeamType.Team1:
					value = Turret.TeamType.Team1;
					break;
				case CharacterTeamType.Team2:
					value = Turret.TeamType.Team2;
					break;
				case CharacterTeamType.FriendlyNPC:
					value = Turret.TeamType.FriendlyNPC;
					break;
				default:
					throw new NotImplementedException();
				}
				this.loadedFriendlyTeamType = new Turret.TeamType?(value);
			}
		}

		// Token: 0x06004862 RID: 18530 RVA: 0x001CD318 File Offset: 0x001CB518
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.FindLightComponents();
			this.targetRotation = this.Rotation;
			if (this.loadedBaseRotation == null)
			{
				if (this.item.FlippedX)
				{
					this.FlipX(false);
				}
				if (this.item.FlippedY)
				{
					this.FlipY(false);
				}
			}
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x06004863 RID: 18531 RVA: 0x001CD380 File Offset: 0x001CB580
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Turret.EventData eventData;
			if (base.TryExtractEventData<Turret.EventData>(extraData, out eventData))
			{
				Item projectile = eventData.Projectile;
				msg.WriteUInt16((projectile != null) ? projectile.ID : ushort.MaxValue);
				msg.WriteRangedSingle(MathHelper.Clamp(this.<ServerEventWrite>g__wrapAngle|280_0(this.Rotation), this.minRotation, this.maxRotation), this.minRotation, this.maxRotation, 16);
				return;
			}
			msg.WriteUInt16(0);
			msg.WriteRangedSingle(MathHelper.Clamp(this.<ServerEventWrite>g__wrapAngle|280_0(this.targetRotation), this.minRotation, this.maxRotation), this.minRotation, this.maxRotation, 16);
		}

		// Token: 0x06004864 RID: 18532 RVA: 0x001CD420 File Offset: 0x001CB620
		[CompilerGenerated]
		private bool <TryLaunch>g__tryUseProjectileContainer|244_1(Item containerItem, ref Turret.<>c__DisplayClass244_0 A_2, ref Turret.<>c__DisplayClass244_1 A_3)
		{
			ItemContainer projectileContainer = containerItem.GetComponent<ItemContainer>();
			if (projectileContainer != null)
			{
				containerItem.Use(A_2.deltaTime, null, null, null, this.user);
				A_3.projectiles = this.GetLoadedProjectiles();
				if (A_3.projectiles.Any<Projectile>())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06004867 RID: 18535 RVA: 0x001CD504 File Offset: 0x001CB704
		[CompilerGenerated]
		private float <ServerEventWrite>g__wrapAngle|280_0(float angle)
		{
			float wrappedAngle;
			for (wrappedAngle = angle; wrappedAngle < this.minRotation; wrappedAngle += 6.2831855f)
			{
				if (!MathUtils.IsValid(wrappedAngle))
				{
					break;
				}
			}
			while (wrappedAngle > this.maxRotation && MathUtils.IsValid(wrappedAngle))
			{
				wrappedAngle -= 6.2831855f;
			}
			return wrappedAngle;
		}

		// Token: 0x040022C2 RID: 8898
		private Sprite barrelSprite;

		// Token: 0x040022C3 RID: 8899
		private Sprite railSprite;

		// Token: 0x040022C4 RID: 8900
		private Sprite barrelSpriteBroken;

		// Token: 0x040022C5 RID: 8901
		private Sprite railSpriteBroken;

		// Token: 0x040022C6 RID: 8902
		[TupleElementNames(new string[]
		{
			"sprite",
			"position"
		})]
		private readonly List<ValueTuple<Sprite, Vector2>> chargeSprites = new List<ValueTuple<Sprite, Vector2>>();

		// Token: 0x040022C7 RID: 8903
		private readonly List<Sprite> spinningBarrelSprites = new List<Sprite>();

		// Token: 0x040022C8 RID: 8904
		private const ushort LaunchWithoutProjectileId = 65535;

		// Token: 0x040022C9 RID: 8905
		private Vector2 barrelPos;

		// Token: 0x040022CA RID: 8906
		private Vector2 transformedBarrelPos;

		// Token: 0x040022CB RID: 8907
		private float targetRotation;

		// Token: 0x040022CC RID: 8908
		private float reload;

		// Token: 0x040022CD RID: 8909
		private int shotCounter;

		// Token: 0x040022CE RID: 8910
		private float minRotation;

		// Token: 0x040022CF RID: 8911
		private float maxRotation;

		// Token: 0x040022D0 RID: 8912
		private Camera cam;

		// Token: 0x040022D1 RID: 8913
		private float angularVelocity;

		// Token: 0x040022D2 RID: 8914
		private int failedLaunchAttempts;

		// Token: 0x040022D3 RID: 8915
		private float currentChargeTime;

		// Token: 0x040022D4 RID: 8916
		private bool tryingToCharge;

		// Token: 0x040022D5 RID: 8917
		private const float LineOfSightCheckInterval = 0.5f;

		// Token: 0x040022D6 RID: 8918
		[TupleElementNames(new string[]
		{
			"WorldTarget",
			"TransformedTarget",
			"Time"
		})]
		private ValueTuple<Body, Body, double> lastLineOfSightCheck;

		// Token: 0x040022D7 RID: 8919
		[TupleElementNames(new string[]
		{
			"Target",
			"CanSee",
			"Time"
		})]
		private ValueTuple<Character, bool, double> lastCanSeeTargetCheck;

		// Token: 0x040022D8 RID: 8920
		private Turret.ChargingState currentChargingState;

		// Token: 0x040022D9 RID: 8921
		private readonly List<Item> activeProjectiles = new List<Item>();

		// Token: 0x040022DA RID: 8922
		private Character user;

		// Token: 0x040022DB RID: 8923
		private float resetUserTimer;

		// Token: 0x040022DC RID: 8924
		private float aiFindTargetTimer;

		// Token: 0x040022DD RID: 8925
		private ISpatialEntity currentTarget;

		// Token: 0x040022DE RID: 8926
		private const float CrewAiFindTargetMaxInterval = 1f;

		// Token: 0x040022DF RID: 8927
		private const float CrewAIFindTargetMinInverval = 0.2f;

		// Token: 0x040022E0 RID: 8928
		private const float MinimumProjectileVelocityForAimAhead = 20f;

		// Token: 0x040022E1 RID: 8929
		private const float MaximumAimAhead = 10f;

		// Token: 0x040022E2 RID: 8930
		private float projectileSpeed;

		// Token: 0x040022E3 RID: 8931
		private Item previousAmmo;

		// Token: 0x040022E4 RID: 8932
		private int currentLoaderIndex;

		// Token: 0x040022E5 RID: 8933
		private const float TinkeringPowerCostReduction = 0.2f;

		// Token: 0x040022E6 RID: 8934
		private const float TinkeringDamageIncrease = 0.2f;

		// Token: 0x040022E7 RID: 8935
		private const float TinkeringReloadDecrease = 0.2f;

		// Token: 0x040022E8 RID: 8936
		public Character ActiveUser;

		// Token: 0x040022E9 RID: 8937
		private float resetActiveUserTimer;

		// Token: 0x040022EA RID: 8938
		private List<LightComponent> lightComponents;

		// Token: 0x040022EB RID: 8939
		private Projectile lastProjectile;

		// Token: 0x040022EC RID: 8940
		private readonly bool isSlowTurret;

		// Token: 0x040022EF RID: 8943
		private bool flipFiringOffset;

		// Token: 0x040022F8 RID: 8952
		private float prevScale;

		// Token: 0x040022F9 RID: 8953
		private float prevBaseRotation;

		// Token: 0x040022FB RID: 8955
		private float _maxAngleOffset;

		// Token: 0x0400231A RID: 8986
		private const string SetAutoOperateConnection = "set_auto_operate";

		// Token: 0x0400231B RID: 8987
		private const string ToggleAutoOperateConnection = "toggle_auto_operate";

		// Token: 0x0400231C RID: 8988
		private bool isUseBeingCalled;

		// Token: 0x0400231D RID: 8989
		private float waitTimer;

		// Token: 0x0400231E RID: 8990
		private float randomAimTimer;

		// Token: 0x0400231F RID: 8991
		private float prevTargetRotation;

		// Token: 0x04002320 RID: 8992
		private float updateTimer;

		// Token: 0x04002321 RID: 8993
		private bool updatePending;

		// Token: 0x04002322 RID: 8994
		private Vector2? loadedRotationLimits;

		// Token: 0x04002323 RID: 8995
		private float? loadedBaseRotation;

		// Token: 0x04002324 RID: 8996
		private Turret.TeamType? loadedFriendlyTeamType;

		// Token: 0x02000E3A RID: 3642
		private enum ChargingState
		{
			// Token: 0x04004234 RID: 16948
			Inactive,
			// Token: 0x04004235 RID: 16949
			WindingUp,
			// Token: 0x04004236 RID: 16950
			WindingDown
		}

		// Token: 0x02000E3B RID: 3643
		public enum TeamType
		{
			// Token: 0x04004238 RID: 16952
			OwnSub,
			// Token: 0x04004239 RID: 16953
			Team1,
			// Token: 0x0400423A RID: 16954
			Team2,
			// Token: 0x0400423B RID: 16955
			FriendlyNPC,
			// Token: 0x0400423C RID: 16956
			NoneTeam
		}

		// Token: 0x02000E3C RID: 3644
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x060069D7 RID: 27095 RVA: 0x0022546B File Offset: 0x0022366B
			public EventData(Item projectile, Turret turret)
			{
				this.Projectile = projectile;
			}

			// Token: 0x0400423D RID: 16957
			public readonly Item Projectile;
		}
	}
}
