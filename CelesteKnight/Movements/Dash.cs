namespace CelesteKnight
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
        private Vector2 hyperMomentum = new Vector2(28, -1.75f);
        private bool wallbouncingLeft;
        private Vector2 wallbounceMomentum = new Vector2(0, 2);
        public static bool lastActionJump;
        public Dash()
        {
            instance = this;
        }
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.CanDash += HeroController_CanDash;
                On.HeroController.HeroDash += HeroController_HeroDash;
                ModHooks.DashVectorHook += ModHooks_DashVectorHook;
                On.HeroController.CancelDash += HeroController_CancelDash;
                On.HeroController.FinishedDashing += HeroController_FinishedDashing;
                On.HeroController.CanJump += HeroController_CanJump;
                On.HeroController.CanWallJump += HeroController_CanWallJump;
                On.HeroController.HeroJump += HeroController_HeroJump;
                On.HeroController.DoWallJump += HeroController_DoWallJump;
                On.HeroController.CanDoubleJump += HeroController_CanDoubleJump;
                On.HeroController.ShouldHardLand += HeroController_ShouldHardLand;
                On.HeroController.RegainControl += HeroController_RegainControl;
            }
            else
            {
                On.HeroController.CanDash -= HeroController_CanDash;
                On.HeroController.HeroDash -= HeroController_HeroDash;
                ModHooks.DashVectorHook -= ModHooks_DashVectorHook;
                On.HeroController.CanJump -= HeroController_CanJump;
                On.HeroController.CanWallJump -= HeroController_CanWallJump;
                On.HeroController.HeroJump -= HeroController_HeroJump;
                On.HeroController.CanDoubleJump -= HeroController_CanDoubleJump;
                On.HeroController.ShouldHardLand -= HeroController_ShouldHardLand;
                On.HeroController.RegainControl -= HeroController_RegainControl;
            }
        }
        private static GameObject GetDashEffectHolder()
        {
            var h = HeroController.instance;
            var d = h.transform.Find("dashEffectHolder");
            return d?.gameObject;
        }
        private GameObject GetAddDashEffectHolder()
        {
            var h = HeroController.instance;
            if (GetDashEffectHolder() == null)
            {
                var d = new GameObject("dashEffectHolder");
                d.transform.parent = h.transform;
                d.transform.localPosition = Vector3.zero;
                d.transform.localScale = Vector3.one;
                var b = d.AddComponent<BoxCollider2D>();
                b.offset = new Vector2(-0.3f, -0.75f);
                b.isTrigger = true;
            }
            return h.transform.Find("dashEffectHolder").gameObject;
        }
        private bool HeroController_CanDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            var h = self.Reflect();
            return h.hero_state != ActorStates.no_input && h.hero_state != ActorStates.hard_landing && h.hero_state != ActorStates.dash_landing && h.dashCooldownTimer <= 0f && !h.cState.dashing && !h.cState.backDashing && (!h.cState.attacking || h.attack_time >= h.ATTACK_RECOVERY_TIME) && !h.cState.preventDash && (h.cState.onGround || !h.airDashed || h.cState.wallSliding) && !h.cState.hazardDeath;
        }
        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            var h = self.Reflect();
            if (!h.cState.onGround && !h.inAcid)
            {
                h.airDashed = true;
            }
            h.ResetAttacksDash();
            h.CancelBounce();
            h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
            h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
            h.audioCtrl.PlaySound(HeroSounds.DASH);
            h.ResetLook();
            h.cState.recoiling = false;
            if (h.cState.wallSliding)
            {
                h.FlipSprite();
            }
            else if (Input.RightPressed())
            {
                h.FaceRight();
            }
            else if (Input.LeftPressed())
            {
                h.FaceLeft();
            }
            h.cState.dashing = true;
            h.dashQueueSteps = 0;
            dashingUp = Input.UpPressed();
            dashingDown = Input.DownPressed();
            dashingLeft = Input.LeftPressed();
            dashingRight = Input.RightPressed();
            if (!dashingUp && !dashingDown && !dashingLeft && !dashingRight)
            {
                dashingLeft = !h.cState.facingRight;
                dashingRight = h.cState.facingRight;
            }
            h.dashingDown = !dashingLeft && !dashingRight;
            h.dashCooldownTimer = 0.2f;
            if (CelesteKnight.instance.settings_.shadeCloak && h.playerData.GetBool("hasShadowDash") && h.shadowDashTimer <= 0f)
            {
                h.cState.shadowDashing = true;
                h.shadowDashTimer = h.SHADOW_DASH_COOLDOWN;
                if (h.playerData.GetBool("equippedCharm_16"))
                {
                    h.audioSource.PlayOneShot(h.sharpShadowClip, 1f);
                    h.sharpShadowPrefab.SetActive(true);
                }
                else
                {
                    h.audioSource.PlayOneShot(h.shadowDashClip, 1f);
                }
                GameObject dashEffect;
                if (h.dashingDown)
                {
                    dashEffect = h.shadowdashDownBurstPrefab.Spawn(GetAddDashEffectHolder().transform, new Vector3(0, 3.5f, 0.00101f));
                    dashEffect.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                }
                else
                {
                    dashEffect = h.shadowdashBurstPrefab.Spawn(GetAddDashEffectHolder().transform, new Vector3(5.21f, -0.58f, +0.00101f));
                }
                dashEffect.transform.localScale = new Vector3(1.9196f, 1, 1.9196f);
                h.dashEffect = dashEffect;
                h.shadowRechargePrefab.SetActive(true);
                FSMUtility.LocateFSM(h.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
                VibrationManager.PlayVibrationClipOneShot(h.shadowDashVibration, null, false, "");
            }
            else
            {
                h.dashBurst.transform.parent = GetAddDashEffectHolder().transform;
                if (h.dashingDown)
                {
                    h.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
                    h.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                }
                else
                {
                    h.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
                    h.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                }
                h.dashBurst.transform.localScale = new Vector3(-1.5085f, 1f, 1.5085f);
                h.dashBurst.SendEvent("PLAY");
                if (h.cState.onGround && (dashingLeft || dashingRight))
                {
                    var dashEffect = h.backDashPrefab.Spawn(h.transform.position);
                    dashEffect.transform.localScale = new Vector3(h.transform.localScale.x * -1f, h.transform.localScale.y, h.transform.localScale.z);
                }
#pragma warning disable 612, 618
                h.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
#pragma warning restore 612, 618
                VibrationManager.PlayVibrationClipOneShot(h.dashVibration, null, false, "");
            }
            h.jumped_steps = h.JUMP_STEPS_MIN;
            lastActionJump = false;
        }
        private Vector2 ModHooks_DashVectorHook(Vector2 arg)
        {
            var h = HeroController.instance.Reflect();
            if (dashingDown && (dashingLeft || dashingRight) && h.cState.onGround)
            {
                h.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                if (h.dashEffect != null)
                {
                    h.dashEffect.transform.localScale = new Vector3(1.9196f, 0, 1.9196f);
                }
            }
            var v = h.DASH_SPEED;
            var vX = (h.cState.facingRight ? 1 : -1) * v;
            var bounds = h.col2d.bounds;
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
                    var y = dashingUp ? bounds.max.y : bounds.min.y;
                    var l = new Vector2(bounds.min.x, y);
                    var r = new Vector2(bounds.max.x, y);
                    var d = dashingUp ? Vector2.up : Vector2.down;
                    var rL = Physics2D.Raycast(l, d, 0.05f, 256);
                    var rR = Physics2D.Raycast(r, d, 0.05f, 256);
                    var check = (RaycastHit2D r1, RaycastHit2D r2) =>
                    {
                        return r1.collider == null && r2.collider != null || r1.collider != null && r2.collider == null;
                    };
                    if (check(rL, rR))
                    {
                        for (int i = 0; i < 32; ++i)
                        {
                            var m = new Vector2((l.x + r.x) / 2, y);
                            var rM = Physics2D.Raycast(m, d, 0.05f, 256);
                            if (check(rM, rL))
                            {
                                r = m;
                            }
                            else
                            {
                                l = m;
                            }
                        }
                        var p = h.transform.position;
                        float delta;
                        if (rL.collider == null)
                        {
                            delta = l.x - bounds.max.x - 0.01f;
                        }
                        else
                        {
                            delta = r.x - bounds.min.x + 0.01f;
                        }
                        if (Mathf.Abs(delta) < 0.25)
                        {
                            h.transform.position = new Vector3(p.x + delta, p.y, p.z);
                        }
                    }
                    return new Vector2(0, vY);
                }
            }
            else
            {
                var x = h.cState.facingRight ? bounds.max.x : bounds.min.x;
                var t = new Vector2(x, bounds.min.y + 0.75f);
                var b = new Vector2(x, bounds.min.y);
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
                    h.transform.position = new Vector3(p.x, p.y + t.y - bounds.min.y + 0.01f, p.z);
                }
                return new Vector2(vX, 0);
            }
        }
        public static void AdjustSprite(float knight, float holder)
        {
            var rotate = (GameObject g, float a) =>
            {
                var c = g.GetComponent<BoxCollider2D>().bounds.center;
                g.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, a));
                var newC = g.GetComponent<BoxCollider2D>().bounds.center;
                var p = g.transform.localPosition;
                var dP = c - newC;
                var h = HeroController.instance;
                if (g.transform.parent == h.transform && h.transform.localScale.x < 0)
                {
                    dP = new Vector3(-dP.x, dP.y, dP.z);
                }
                g.transform.localPosition = p + dP;
            };
            var h = HeroController.instance;
            rotate(h.gameObject, knight);
            var d = GetDashEffectHolder();
            if (d != null)
            {
                rotate(GetDashEffectHolder(), holder);
            }
        }
        private void HeroController_CancelDash(On.HeroController.orig_CancelDash orig, HeroController self)
        {
            orig(self);
            AdjustSprite(0, 0);
            if (dashingUp && !dashingLeft && !dashingRight)
            {
                self.Reflect().rb2d.velocity = Vector2.zero;
                Momentum.momentum = upDashMomentum;
            }
        }
        private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
        {
            orig(self);
            AdjustSprite(0, 0);
            if (dashingUp && !dashingLeft && !dashingRight)
            {
                Momentum.momentum = upDashMomentum;
            }
        }
        private bool HeroController_CanJump(On.HeroController.orig_CanJump orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.hero_state == ActorStates.no_input || h.hero_state == ActorStates.hard_landing || h.hero_state == ActorStates.dash_landing || h.cState.wallSliding || h.cState.backDashing || h.cState.jumping || h.cState.bouncing || h.cState.shroomBouncing)
            {
                return false;
            }
            if (h.cState.dashing && !dashingLeft && !dashingRight)
            {
                return false;
            }
            if (h.cState.onGround)
            {
                return true;
            }
            if (h.ledgeBufferSteps > 0 && !h.cState.dead && !h.cState.hazardDeath && !h.controlReqlinquished && h.headBumpSteps <= 0 && !h.CheckNearRoof())
            {
                h.ledgeBufferSteps = 0;
                return true;
            }
            if (h.cState.dashing)
            {
                var b = h.col2d.bounds;
                var l = new Vector2(b.min.x, b.min.y);
                var r = new Vector2(b.max.x, b.min.y);
                var rL = Physics2D.Raycast(l, Vector2.down, 0.25f, 256);
                var rR = Physics2D.Raycast(r, Vector2.down, 0.25f, 256);
                if (rL.collider != null || rR.collider != null)
                {
                    return true;
                }
            }
            return false;
        }
        private bool HeroController_CanWallJump(On.HeroController.orig_CanWallJump orig, HeroController self)
        {
            var h = self.Reflect();
            if (!h.cState.touchingNonSlider && (h.cState.wallSliding || (h.cState.touchingWall && !h.cState.onGround)))
            {
                return true;
            }
            if (dashingUp && !dashingLeft && !dashingRight)
            {
                var b = h.col2d.bounds;
                for (int i = 0; i < 32; ++i)
                {
                    var y = b.min.y + (b.max.y - b.min.y) * (i / 31f);
                    var l = new Vector2(b.min.x, y);
                    var r = new Vector2(b.max.x, y);
                    var rL = Physics2D.Raycast(l, Vector2.left, 0.35f, 256);
                    var rR = Physics2D.Raycast(r, Vector2.right, 0.35f, 256);
                    if (rL.collider != null)
                    {
                        wallbouncingLeft = true;
                        return true;
                    }
                    if (rR.collider != null)
                    {
                        wallbouncingLeft = false;
                        return true;
                    }
                }
            }
            return false;
        }
        private void HeroController_HeroJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.cState.dashing)
            {
                h.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                if (h.dashEffect != null)
                {
                    h.dashEffect.transform.localScale = new Vector3(1.9196f, 0, 1.9196f);
                }
                h.FinishedDashing();
                var d = (h.cState.facingRight ? 1 : -1);
                if (Input.LeftPressed())
                {
                    d = -1;
                }
                else if (Input.RightPressed())
                {
                    d = 1;
                }
                if (dashingDown)
                {
                    Momentum.momentum += new Vector2(d * hyperMomentum.x, hyperMomentum.y);
                    Momentum.StartEffect();
                }
                else
                {
                    Momentum.momentum += new Vector2(d * superMomentum.x, superMomentum.y);
                    Momentum.StartEffect();
                }
            }
            lastActionJump = true;
            orig(self);
        }
        private void HeroController_DoWallJump(On.HeroController.orig_DoWallJump orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.cState.dashing)
            {
                h.dashBurst.transform.localScale = new Vector3(-1.5085f, 0f, 1.5085f);
                if (h.dashEffect != null)
                {
                    h.dashEffect.transform.localScale = new Vector3(1.9196f, 0, 1.9196f);
                }
                h.FinishedDashing();
                if (h.cState.facingRight != wallbouncingLeft)
                {
                    h.FlipSprite();
                }
                if (wallbouncingLeft)
                {
                    h.wallJumpedR = true;
                    h.wallJumpedL = false;
                }
                else
                {
                    h.wallJumpedR = false;
                    h.wallJumpedL = true;
                }
                Momentum.momentum += wallbounceMomentum;
                Momentum.StartEffect();
            }
            h.wallPuffPrefab.SetActive(true);
            h.audioCtrl.PlaySound(HeroSounds.WALLJUMP);
            VibrationManager.PlayVibrationClipOneShot(h.wallJumpVibration, null, false, "");
            if (h.touchingWallL)
            {
                h.FaceRight();
                h.wallJumpedR = true;
                h.wallJumpedL = false;
            }
            else if (h.touchingWallR)
            {
                h.FaceLeft();
                h.wallJumpedR = false;
                h.wallJumpedL = true;
            }
            h.CancelWallsliding();
            h.cState.touchingWall = false;
            h.touchingWallL = false;
            h.touchingWallR = false;
            h.airDashed = false;
            h.doubleJumped = false;
            h.currentWalljumpSpeed = h.WJ_KICKOFF_SPEED;
            h.walljumpSpeedDecel = (h.WJ_KICKOFF_SPEED - h.RUN_SPEED) / (float)h.WJLOCK_STEPS_LONG;
            h.dashBurst.SendEvent("CANCEL");
            h.cState.jumping = true;
            h.wallLockSteps = 0;
            h.wallLocked = true;
            h.jumpQueueSteps = 0;
            h.jumped_steps = 0;
            lastActionJump = true;
        }
        private bool HeroController_CanDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            if (!CelesteKnight.instance.settings_.doubleJump)
            {
                return false;
            }
            return orig(self);
        }
        private bool HeroController_ShouldHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            return false;
        }
        private void HeroController_RegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
        {
            orig(self);
            lastActionJump = true;
        }
        public static void FixedUpdate()
        {
            var h = HeroController.instance.Reflect();
            if (h.cState.dashing)
            {
                var v = h.rb2d.velocity;
                if (h.cState.onGround && v.x != 0 & v.y < 0)
                {
                    v = new Vector2(v.x, 0);
                }
                var diagonal = (Dash.instance.dashingUp || Dash.instance.dashingDown) && (Dash.instance.dashingLeft || Dash.instance.dashingRight);
                if (diagonal)
                {
                    float a;
                    if (h.cState.facingRight)
                    {
                        a = -Mathf.Atan2(v.y, v.x);
                    }
                    else
                    {
                        a = Mathf.Atan2(v.y, v.x) + Mathf.PI;
                    }
                    AdjustSprite(0, a / Mathf.PI * 180);
                }
                else if (Dash.instance.dashingUp)
                {
                    AdjustSprite(180, 0);
                }
            }
        }
    }
}