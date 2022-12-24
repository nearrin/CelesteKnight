namespace Celeste
{
    public class Dash : Module
    {
        public static Dash instance;
        public bool dashingUp;
        public bool dashingDown;
        public bool dashingLeft;
        public bool dashingRight;
        private Vector2 superMomentum = new Vector2(16, 0);
        private Vector2 hyperMomentum = new Vector2(32, -2);
        public Dash()
        {
            instance = this;
        }
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.HeroDash += HeroController_HeroDash;
                ModHooks.DashVectorHook += ModHooks_DashVectorHook;
                On.HeroController.FinishedDashing += HeroController_FinishedDashing;
                On.HeroController.CanJump += HeroController_CanJump;
                On.HeroController.HeroJump += HeroController_HeroJump;
            }
            else
            {
                On.HeroController.HeroDash -= HeroController_HeroDash;
                ModHooks.DashVectorHook -= ModHooks_DashVectorHook;
                On.HeroController.CanJump -= HeroController_CanJump;
                On.HeroController.HeroJump -= HeroController_HeroJump;
            }
        }
        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            var this_ = self;
            var this__audioCtrl = ReflectionHelper.GetField<HeroController, HeroAudioController>(this_, "audioCtrl");
            var this__inputHandler = ReflectionHelper.GetField<HeroController, InputHandler>(this_, "inputHandler");
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
            if (Input.instance.downPressed())
            {
                this_.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                this_.dashingDown = true;
            }
            else
            {
                this_.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
                this_.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                this_.dashingDown = false;
            }
            this_.dashBurst.transform.localScale = new Vector3(-1.5085f, 1f, 1.5085f);
            dashingUp = Input.instance.upPressed();
            dashingDown = Input.instance.downPressed();
            dashingLeft = Input.instance.leftPressed();
            dashingRight = Input.instance.rightPressed();
            ReflectionHelper.SetField(this_, "dashCooldownTimer", 0.2f);
            this_.dashBurst.SendEvent("PLAY");
#pragma warning disable 612, 618
            this_.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
            VibrationManager.PlayVibrationClipOneShot(this_.dashVibration, null, false, "");
            if (this_.cState.onGround)
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
                if (dashingLeft || dashingRight)
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
        public void RotateSprite(float a)
        {
            var h = HeroController.instance;
            var c = h.gameObject.GetComponent<BoxCollider2D>().bounds.center;
            h.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, a));
            var newC = h.gameObject.GetComponent<BoxCollider2D>().bounds.center;
            var p = h.transform.localPosition;
            var newP = p + c - newC;
            h.transform.localPosition = newP;
        }
        private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
        {
            RotateSprite(0);
            orig(self);
        }
        private bool HeroController_CanJump(On.HeroController.orig_CanJump orig, HeroController self)
        {
            var this_ = self;
            if (this_.hero_state == ActorStates.no_input || this_.hero_state == ActorStates.hard_landing || this_.hero_state == ActorStates.dash_landing || this_.cState.wallSliding || this_.cState.backDashing || this_.cState.jumping || this_.cState.bouncing || this_.cState.shroomBouncing)
            {
                return false;
            }
            if (this_.cState.dashing && !dashingLeft && !dashingRight)
            {
                return false;
            }
            if (this_.cState.onGround)
            {
                return true;
            }
            if (ReflectionHelper.GetField<HeroController, int>(this_, "ledgeBufferSteps") > 0 && !this_.cState.dead && !this_.cState.hazardDeath && !this_.controlReqlinquished && ReflectionHelper.GetField<HeroController, int>(this_, "headBumpSteps") <= 0 && !this_.CheckNearRoof())
            {
                ReflectionHelper.SetField(this_, "ledgeBufferSteps", 0);
                return true;
            }
            return false;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            if (self.cState.dashing)
            {
                ReflectionHelper.CallMethod(self, "FinishedDashing");
                self.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                var m = dashingDown ? hyperMomentum : superMomentum;
                Momentum.instance.momentum += new Vector2((self.cState.facingRight ? 1 : -1) * m.x, m.y);
            }
            orig(self);
        }
    }
}