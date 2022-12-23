namespace Celeste
{
    public class Dash : Module
    {
        private bool dashingUp = false;
        private bool dashingDown = false;
        private bool dashingHorizontal = false;
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.HeroDash += HeroController_HeroDash;
                ModHooks.DashVectorHook += ModHooks_DashVectorHook;
            }
            else
            {
                On.HeroController.HeroDash -= HeroController_HeroDash;
                ModHooks.DashVectorHook -= ModHooks_DashVectorHook;
            }
        }
        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            var this_ = self;
            var this__audioCtrl = ReflectionHelper.GetField<HeroController, HeroAudioController>(this_, "audioCtrl");
            var this__inputHandler = ReflectionHelper.GetField<HeroController, InputHandler>(this_, "inputHandler");
            var this__audioSource = ReflectionHelper.GetField<HeroController, AudioSource>(this_, "audioSource");
            if (!this_.cState.onGround && !this_.inAcid)
            {
                ReflectionHelper.SetField(this_, "airDashed", true);
            }
            ReflectionHelper.CallMethod(this_, "ResetAttacksDash");
            ReflectionHelper.CallMethod(this_, "CancelBounce");
            this__audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
            this__audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
            this__audioCtrl.PlaySound(HeroSounds.DASH);
            ReflectionHelper.CallMethod(this_, "ResetLook");
            this_.cState.recoiling = false;
            if (this_.cState.wallSliding)
            {
                this_.FlipSprite();
            }
            else if (Input.instance.rightPressed())
            {
                this_.FaceRight();
            }
            else if (Input.instance.leftPressed())
            {
                this_.FaceLeft();
            }
            this_.cState.dashing = true;
            ReflectionHelper.SetField(this_, "dashQueueSteps", 0);
            HeroActions inputActions = this__inputHandler.inputActions;
            if (Input.instance.upPressed())
            {
                this_.dashingDown = true;
                dashingUp = true;
                dashingDown = false;
                if (Input.instance.leftPressed() || Input.instance.rightPressed())
                {
                    this_.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    dashingHorizontal = true;
                }
                else
                {
                    this_.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    dashingHorizontal = false;
                }
            }
            else if (Input.instance.downPressed())
            {
                this_.dashingDown = true;
                dashingUp = false;
                dashingDown = true;
                if (Input.instance.leftPressed() || Input.instance.rightPressed())
                {
                    this_.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    dashingHorizontal = true;
                }
                else
                {
                    this_.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    dashingHorizontal = false;
                }
            }
            else
            {
                this_.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
                this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                this_.dashingDown = false;
                dashingUp = false;
                dashingDown = false;
                dashingHorizontal = true;
            }
            ReflectionHelper.SetField(this_, "dashCooldownTimer", 0.2f);
            if (this_.playerData.GetBool("hasShadowDash") && ReflectionHelper.GetField<HeroController, float>(this_, "shadowDashTimer") <= 0f)
            {
                ReflectionHelper.SetField(this_, "shadowDashTimer", this_.SHADOW_DASH_COOLDOWN);
                this_.cState.shadowDashing = true;
                if (this_.playerData.GetBool("equippedCharm_16"))
                {
                    this__audioSource.PlayOneShot(this_.sharpShadowClip, 1f);
                    this_.sharpShadowPrefab.SetActive(true);
                }
                else
                {
                    this__audioSource.PlayOneShot(this_.shadowDashClip, 1f);
                }
            }
            if (this_.cState.shadowDashing)
            {
                if (this_.dashingDown)
                {
                    var this__dashEffect = this_.shadowdashDownBurstPrefab.Spawn(new Vector3(this_.transform.position.x, this_.transform.position.y + 3.5f, this_.transform.position.z + 0.00101f));
                    this__dashEffect.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    ReflectionHelper.SetField(this_, "dashEffect", this__dashEffect);
                }
                else if (this_.transform.localScale.x > 0f)
                {
                    var this__dashEffect = this_.shadowdashBurstPrefab.Spawn(new Vector3(this_.transform.position.x + 5.21f, this_.transform.position.y - 0.58f, this_.transform.position.z + 0.00101f));
                    this__dashEffect.transform.localScale = new Vector3(1.919591f, this__dashEffect.transform.localScale.y, this__dashEffect.transform.localScale.z);
                    ReflectionHelper.SetField(this_, "dashEffect", this__dashEffect);
                }
                else
                {
                    var this__dashEffect = this_.shadowdashBurstPrefab.Spawn(new Vector3(this_.transform.position.x - 5.21f, this_.transform.position.y - 0.58f, this_.transform.position.z + 0.00101f));
                    this__dashEffect.transform.localScale = new Vector3(-1.919591f, this__dashEffect.transform.localScale.y, this__dashEffect.transform.localScale.z);
                    ReflectionHelper.SetField(this_, "dashEffect", this__dashEffect);
                }
                this_.shadowRechargePrefab.SetActive(true);
                FSMUtility.LocateFSM(this_.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
#pragma warning disable 612, 618
                this_.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
                VibrationManager.PlayVibrationClipOneShot(this_.shadowDashVibration, null, false, "");
                this_.shadowRingPrefab.Spawn(this_.transform.position);
            }
            else
            {
                this_.dashBurst.SendEvent("PLAY");
#pragma warning disable 612, 618
                this_.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
                VibrationManager.PlayVibrationClipOneShot(this_.dashVibration, null, false, "");
            }
            if (this_.cState.onGround && !this_.cState.shadowDashing)
            {
                var this__dashEffect = this_.backDashPrefab.Spawn(this_.transform.position);
                this__dashEffect.transform.localScale = new Vector3(this_.transform.localScale.x * -1f, this_.transform.localScale.y, this_.transform.localScale.z);
                ReflectionHelper.SetField(this_, "dashEffect", this__dashEffect);
            }
        }
        private Vector2 ModHooks_DashVectorHook(Vector2 arg)
        {
            var h = HeroController.instance;
            var v = h.DASH_SPEED;
            var vX = (h.cState.facingRight ? 1 : -1) * v;
            if (dashingUp || dashingDown)
            {
                var vY = (dashingUp ? 1 : -1) * v;
                if (dashingHorizontal)
                {
                    var k = Mathf.Sqrt(2) / 2;
                    return new Vector2(k * vX, k * vY);
                }
                else
                {
                    return new Vector2(0, vY);
                }
            }
            else
            {
                var vY = 0f;
                if (!h.cState.facingRight && h.CheckForBump(CollisionSide.left) || h.cState.facingRight && h.CheckForBump(CollisionSide.right))
                {
                    vY = (!h.cState.onGround) ? 5f : 4f;
                }
                return new Vector2(vX, vY);
            }
        }
    }
}