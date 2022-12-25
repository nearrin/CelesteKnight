namespace Celeste
{
    public class Dash : Module
    {
        public static Dash instance;
        public bool dashingUp;
        public bool dashingDown;
        public bool dashingLeft;
        public bool dashingRight;
        private Vector2 upDashMomentum = new Vector2(0, 4);
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
                On.HeroController.CancelDash += HeroController_CancelDash;
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
            var self_audioCtrl = ReflectionHelper.GetField<HeroController, HeroAudioController>(self, "audioCtrl");
            var self_autoSource = ReflectionHelper.GetField<HeroController, AudioSource>(self, "audioSource");
            if (!self.cState.onGround && !self.inAcid)
            {
                ReflectionHelper.SetField(self, "airDashed", true);
            }
            ReflectionHelper.CallMethod(self, "ResetAttacksDash");
            ReflectionHelper.CallMethod(self, "CancelBounce");
            self_audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
            self_audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
            self_audioCtrl.PlaySound(HeroSounds.DASH);
            ReflectionHelper.CallMethod(self, "ResetLook");
            self.cState.recoiling = false;
            if (self.cState.wallSliding)
            {
                self.FlipSprite();
            }
            else if (Input.instance.rightPressed())
            {
                self.FaceRight();
            }
            else if (Input.instance.leftPressed())
            {
                self.FaceLeft();
            }
            self.cState.dashing = true;
            ReflectionHelper.SetField(self, "dashQueueSteps", 0);
            if (Input.instance.upPressed() && !Input.instance.leftPressed() && !Input.instance.rightPressed() || Input.instance.downPressed())
            {
                self.dashingDown = true;
            }
            else
            {
                self.dashingDown = false;
            }
            dashingUp = Input.instance.upPressed();
            dashingDown = Input.instance.downPressed();
            dashingLeft = Input.instance.leftPressed();
            dashingRight = Input.instance.rightPressed();
            if (!dashingUp && !dashingDown && !dashingLeft && !dashingRight)
            {
                dashingLeft = !self.cState.facingRight;
                dashingRight = self.cState.facingRight;
            }
            ReflectionHelper.SetField(self, "dashCooldownTimer", 0.2f);
            if (Celeste.instance.settings_.shadeCloak && self.playerData.GetBool("hasShadowDash") && ReflectionHelper.GetField<HeroController, float>(self, "shadowDashTimer") <= 0f)
            {
                self.cState.shadowDashing = true;
                ReflectionHelper.SetField(self, "shadowDashTimer", self.SHADOW_DASH_COOLDOWN);
                if (self.playerData.GetBool("equippedCharm_16"))
                {
                    self_autoSource.PlayOneShot(self.sharpShadowClip, 1f);
                    self.sharpShadowPrefab.SetActive(true);
                }
                else
                {
                    self_autoSource.PlayOneShot(self.shadowDashClip, 1f);
                }
                GameObject dashEffect;
                if (self.dashingDown)
                {
                    dashEffect = self.shadowdashDownBurstPrefab.Spawn(self.transform, new Vector3(0, 3.5f, 0.00101f));
                    dashEffect.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                }
                else
                {
                    dashEffect = self.shadowdashBurstPrefab.Spawn(self.transform, new Vector3(5.21f, -0.58f, +0.00101f));
                }
                dashEffect.transform.localScale = new Vector3(1.9196f, 1, 1.9196f);
                ReflectionHelper.SetField(self, "dashEffect", dashEffect);
                self.shadowRechargePrefab.SetActive(true);
                FSMUtility.LocateFSM(self.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
#pragma warning disable 612, 618
                self.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
                self.shadowRingPrefab.Spawn(self.transform.position);
                VibrationManager.PlayVibrationClipOneShot(self.shadowDashVibration, null, false, "");
            }
            else
            {
                if (self.dashingDown)
                {
                    self.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    self.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                }
                else
                {
                    self.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
                    self.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                }
                self.dashBurst.transform.localScale = new Vector3(-1.5085f, 1f, 1.5085f);
                self.dashBurst.SendEvent("PLAY");
#pragma warning disable 612, 618
                self.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
                VibrationManager.PlayVibrationClipOneShot(self.dashVibration, null, false, "");
            }
            if (self.cState.onGround && !self.cState.shadowDashing)
            {
                var self_dashEffect = self.backDashPrefab.Spawn(self.transform.position);
                self_dashEffect.transform.localScale = new Vector3(self.transform.localScale.x * -1f, self.transform.localScale.y, self.transform.localScale.z);
                ReflectionHelper.SetField(self, "dashEffect", self_dashEffect);
            }
        }
        private Vector2 ModHooks_DashVectorHook(Vector2 arg)
        {
            var h = HeroController.instance;
            if (dashingDown && (dashingLeft || dashingRight) && h.cState.onGround)
            {
                h.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                ReflectionHelper.GetField<HeroController, GameObject>(h, "dashEffect").transform.localScale = new Vector3(1.9196f, 0, 1.9196f);
            }
            var v = h.DASH_SPEED;
            var vX = (h.cState.facingRight ? 1 : -1) * v;
            var c = ReflectionHelper.GetField<HeroController, Collider2D>(h, "col2d");
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
                var x = h.cState.facingRight ? c.bounds.max.x : c.bounds.min.x;
                var t = new Vector2(x, c.bounds.min.y + 0.75f);
                var b = new Vector2(x, c.bounds.min.y);
                var d = h.cState.facingRight ? Vector2.right : Vector2.left;
                var rT = Physics2D.Raycast(t, d, 0.05f, 256);
                var rB = Physics2D.Raycast(b, d, 0.05f, 256);
                if (rT.collider == null && rB.collider != null)
                {
                    for (int i = 0; i < 32; ++i)
                    {
                        var m = new Vector2(x, (t.y + b.y) / 2);
                        var rM = Physics2D.Raycast(m, d, 0.05f, 256);
                        if (rM.collider == null)
                        {
                            t = m;
                        }
                        else
                        {
                            b = m;
                        }
                    }
                    var p = h.transform.position;
                    h.transform.position = new Vector3(p.x, p.y + t.y - c.bounds.min.y, p.z);
                }
                return new Vector2(vX, 0);
            }
        }
        public void RotateSprite(float a)
        {
            var h = HeroController.instance;
            var c = h.GetComponent<BoxCollider2D>().bounds.center;
            h.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, a));
            var newC = h.GetComponent<BoxCollider2D>().bounds.center;
            var p = h.transform.localPosition;
            var newP = p + c - newC;
            h.transform.localPosition = newP;
        }
        private void HeroController_CancelDash(On.HeroController.orig_CancelDash orig, HeroController self)
        {
            RotateSprite(0);
            orig(self);
            if (dashingUp && !dashingLeft && !dashingRight)
            {
                Momentum.instance.momentum = upDashMomentum;
            }
        }
        private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
        {
            RotateSprite(0);
            orig(self);
            if (dashingUp && !dashingLeft && !dashingRight)
            {
                Momentum.instance.momentum = upDashMomentum;
            }
        }
        private bool HeroController_CanJump(On.HeroController.orig_CanJump orig, HeroController self)
        {
            if (self.hero_state == ActorStates.no_input || self.hero_state == ActorStates.hard_landing || self.hero_state == ActorStates.dash_landing || self.cState.wallSliding || self.cState.backDashing || self.cState.jumping || self.cState.bouncing || self.cState.shroomBouncing)
            {
                return false;
            }
            if (self.cState.dashing && !dashingLeft && !dashingRight)
            {
                return false;
            }
            if (self.cState.onGround)
            {
                return true;
            }
            if (ReflectionHelper.GetField<HeroController, int>(self, "ledgeBufferSteps") > 0 && !self.cState.dead && !self.cState.hazardDeath && !self.controlReqlinquished && ReflectionHelper.GetField<HeroController, int>(self, "headBumpSteps") <= 0 && !self.CheckNearRoof())
            {
                ReflectionHelper.SetField(self, "ledgeBufferSteps", 0);
                return true;
            }
            if (self.cState.dashing)
            {
                var c = ReflectionHelper.GetField<HeroController, Collider2D>(self, "col2d");
                var l = new Vector2(c.bounds.min.x, c.bounds.min.y);
                var r = new Vector2(c.bounds.max.x, c.bounds.min.y);
                var rL = Physics2D.Raycast(l, Vector2.down, 0.25f, 256);
                var rR = Physics2D.Raycast(r, Vector2.down, 0.25f, 256);
                if (rL.collider != null || rR.collider != null)
                {
                    return true;
                }
            }
            return false;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            if (self.cState.dashing)
            {
                self.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                ReflectionHelper.GetField<HeroController, GameObject>(self, "dashEffect").transform.localScale = new Vector3(1.9196f, 0, 1.9196f);
                ReflectionHelper.CallMethod(self, "FinishedDashing");
                var d = (self.cState.facingRight ? 1 : -1);
                if (Input.instance.leftPressed())
                {
                    d = -1;
                }
                else if (Input.instance.rightPressed())
                {
                    d = 1;
                }
                var m = dashingDown ? hyperMomentum : superMomentum;
                Momentum.instance.momentum += new Vector2(d * m.x, m.y);
            }
            orig(self);
        }
    }
}