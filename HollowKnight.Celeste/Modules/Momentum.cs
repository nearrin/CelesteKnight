namespace Celeste
{
    public class Momentum : Module
    {
        public static Momentum instance;
        public Vector2 momentum = new Vector2(0, 0);
        public float resistance = 0.4f;
        public Momentum()
        {
            instance = this;
        }
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.ResetMotion += HeroController_ResetMotion;
                On.HeroController.FixedUpdate += HeroController_FixedUpdate;
            }
            else
            {
                On.HeroController.ResetMotion -= HeroController_ResetMotion;
                On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
            }
        }
        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            if (self.Reflect().cState.recoilingLeft || self.Reflect().cState.recoilingRight)
            {
                if ((float)self.Reflect().recoilSteps <= self.Reflect().RECOIL_HOR_STEPS)
                {
                    self.Reflect().recoilSteps++;
                }
                else
                {
                    self.Reflect().CancelRecoilHorizontal();
                }
            }
            if (self.Reflect().cState.dead)
            {
                self.Reflect().rb2d.velocity = new Vector2(0f, 0f);
            }
            if ((self.Reflect().hero_state == ActorStates.hard_landing && !self.Reflect().cState.onConveyor) || self.Reflect().hero_state == ActorStates.dash_landing)
            {
                self.Reflect().ResetMotion();
            }
            else if (self.Reflect().hero_state == ActorStates.no_input)
            {
                if (self.Reflect().cState.transitioning)
                {
                    if (self.Reflect().transitionState == HeroTransitionState.EXITING_SCENE)
                    {
                        self.Reflect().AffectedByGravity(false);
                        if (!self.Reflect().stopWalkingOut)
                        {
                            self.Reflect().rb2d.velocity = new Vector2(self.Reflect().transition_vel.x, self.Reflect().transition_vel.y + self.Reflect().rb2d.velocity.y);
                        }
                    }
                    else if (self.Reflect().transitionState == HeroTransitionState.ENTERING_SCENE)
                    {
                        self.Reflect().rb2d.velocity = self.Reflect().transition_vel;
                    }
                    else if (self.Reflect().transitionState == HeroTransitionState.DROPPING_DOWN)
                    {
                        self.Reflect().rb2d.velocity = new Vector2(self.Reflect().transition_vel.x, self.Reflect().rb2d.velocity.y);
                    }
                }
                else if (self.Reflect().cState.recoiling)
                {
                    self.Reflect().AffectedByGravity(false);
                    self.Reflect().rb2d.velocity = self.Reflect().recoilVector;
                }
            }
            else if (self.Reflect().hero_state != ActorStates.no_input)
            {
                if (self.Reflect().hero_state == ActorStates.running)
                {
                    if (self.Reflect().move_input > 0f)
                    {
                        if (self.Reflect().CheckForBump(CollisionSide.right))
                        {
                            self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x, self.Reflect().BUMP_VELOCITY);
                        }
                    }
                    else if (self.Reflect().move_input < 0f && self.Reflect().CheckForBump(CollisionSide.left))
                    {
                        self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x, self.Reflect().BUMP_VELOCITY);
                    }
                }
                if (!self.Reflect().cState.backDashing && !self.Reflect().cState.dashing)
                {
                    self.Reflect().Move(self.Reflect().move_input);
                    if ((!self.Reflect().cState.attacking || self.Reflect().attack_time >= self.Reflect().ATTACK_RECOVERY_TIME) && !self.Reflect().cState.wallSliding && !self.Reflect().wallLocked)
                    {
                        if (self.Reflect().move_input > 0f && !self.Reflect().cState.facingRight)
                        {
                            self.Reflect().FlipSprite();
                            self.Reflect().CancelAttack();
                        }
                        else if (self.Reflect().move_input < 0f && self.Reflect().cState.facingRight)
                        {
                            self.Reflect().FlipSprite();
                            self.Reflect().CancelAttack();
                        }
                    }
                    if (self.Reflect().cState.recoilingLeft)
                    {
                        float num;
                        if (self.Reflect().recoilLarge)
                        {
                            num = self.Reflect().RECOIL_HOR_VELOCITY_LONG;
                        }
                        else
                        {
                            num = self.Reflect().RECOIL_HOR_VELOCITY;
                        }
                        if (self.Reflect().rb2d.velocity.x > -num)
                        {
                            self.Reflect().rb2d.velocity = new Vector2(-num, self.Reflect().rb2d.velocity.y);
                        }
                        else
                        {
                            self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x - num, self.Reflect().rb2d.velocity.y);
                        }
                    }
                    if (self.Reflect().cState.recoilingRight)
                    {
                        float num2;
                        if (self.Reflect().recoilLarge)
                        {
                            num2 = self.Reflect().RECOIL_HOR_VELOCITY_LONG;
                        }
                        else
                        {
                            num2 = self.Reflect().RECOIL_HOR_VELOCITY;
                        }
                        if (self.Reflect().rb2d.velocity.x < num2)
                        {
                            self.Reflect().rb2d.velocity = new Vector2(num2, self.Reflect().rb2d.velocity.y);
                        }
                        else
                        {
                            self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x + num2, self.Reflect().rb2d.velocity.y);
                        }
                    }
                }
                if ((self.Reflect().cState.lookingUp || self.Reflect().cState.lookingDown) && Mathf.Abs(self.Reflect().move_input) > 0.6f)
                {
                    self.Reflect().ResetLook();
                }
                if (self.Reflect().cState.jumping)
                {
                    self.Reflect().Jump();
                }
                if (self.Reflect().cState.doubleJumping)
                {
                    self.Reflect().DoubleJump();
                }
                if (self.Reflect().cState.dashing)
                {
                    self.Reflect().Dash();
                }
                if (self.Reflect().cState.casting)
                {
                    if (self.Reflect().cState.castRecoiling)
                    {
                        if (self.Reflect().cState.facingRight)
                        {
                            self.Reflect().rb2d.velocity = new Vector2(-self.Reflect().CAST_RECOIL_VELOCITY, 0f);
                        }
                        else
                        {
                            self.Reflect().rb2d.velocity = new Vector2(self.Reflect().CAST_RECOIL_VELOCITY, 0f);
                        }
                    }
                    else
                    {
                        self.Reflect().rb2d.velocity = Vector2.zero;
                    }
                }
                if (self.Reflect().cState.bouncing)
                {
                    self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x, self.Reflect().BOUNCE_VELOCITY);
                }
                bool shroomBouncing = self.Reflect().cState.shroomBouncing;
                if (self.Reflect().wallLocked)
                {
                    if (self.Reflect().wallJumpedR)
                    {
                        self.Reflect().rb2d.velocity = new Vector2(self.Reflect().currentWalljumpSpeed, self.Reflect().rb2d.velocity.y);
                    }
                    else if (self.Reflect().wallJumpedL)
                    {
                        self.Reflect().rb2d.velocity = new Vector2(-self.Reflect().currentWalljumpSpeed, self.Reflect().rb2d.velocity.y);
                    }
                    self.Reflect().wallLockSteps++;
                    if (self.Reflect().wallLockSteps > self.Reflect().WJLOCK_STEPS_LONG)
                    {
                        self.Reflect().wallLocked = false;
                    }
                    self.Reflect().currentWalljumpSpeed -= self.Reflect().walljumpSpeedDecel;
                }
                if (self.Reflect().cState.wallSliding)
                {
                    if (self.Reflect().wallSlidingL && self.Reflect().inputHandler.inputActions.right.IsPressed)
                    {
                        self.Reflect().wallUnstickSteps++;
                    }
                    else if (self.Reflect().wallSlidingR && self.Reflect().inputHandler.inputActions.left.IsPressed)
                    {
                        self.Reflect().wallUnstickSteps++;
                    }
                    else
                    {
                        self.Reflect().wallUnstickSteps = 0;
                    }
                    if (self.Reflect().wallUnstickSteps >= self.Reflect().WALL_STICKY_STEPS)
                    {
                        self.Reflect().CancelWallsliding();
                    }
                    if (self.Reflect().wallSlidingL)
                    {
                        if (!self.Reflect().CheckStillTouchingWall(CollisionSide.left, false))
                        {
                            self.Reflect().FlipSprite();
                            self.Reflect().CancelWallsliding();
                        }
                    }
                    else if (self.Reflect().wallSlidingR && !self.Reflect().CheckStillTouchingWall(CollisionSide.right, false))
                    {
                        self.Reflect().FlipSprite();
                        self.Reflect().CancelWallsliding();
                    }
                }
            }
            if (self.Reflect().rb2d.velocity.y < -self.Reflect().MAX_FALL_VELOCITY && !self.Reflect().inAcid && !self.Reflect().controlReqlinquished && !self.Reflect().cState.shadowDashing && !self.Reflect().cState.spellQuake)
            {
                self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x, -self.Reflect().MAX_FALL_VELOCITY);
            }
            if (self.Reflect().jumpQueuing)
            {
                self.Reflect().jumpQueueSteps++;
            }
            if (self.Reflect().doubleJumpQueuing)
            {
                self.Reflect().doubleJumpQueueSteps++;
            }
            if (self.Reflect().dashQueuing)
            {
                self.Reflect().dashQueueSteps++;
            }
            if (self.Reflect().attackQueuing)
            {
                self.Reflect().attackQueueSteps++;
            }
            if (self.Reflect().cState.wallSliding && !self.Reflect().cState.onConveyorV)
            {
                if (self.Reflect().rb2d.velocity.y > self.Reflect().WALLSLIDE_SPEED)
                {
                    self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().rb2d.velocity.y - self.Reflect().WALLSLIDE_DECEL);
                    if (self.Reflect().rb2d.velocity.y < self.Reflect().WALLSLIDE_SPEED)
                    {
                        self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().WALLSLIDE_SPEED);
                    }
                }
                if (self.Reflect().rb2d.velocity.y < self.Reflect().WALLSLIDE_SPEED)
                {
                    self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().rb2d.velocity.y + self.Reflect().WALLSLIDE_DECEL);
                    if (self.Reflect().rb2d.velocity.y < self.Reflect().WALLSLIDE_SPEED)
                    {
                        self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().WALLSLIDE_SPEED);
                    }
                }
            }
            if (self.Reflect().nailArt_cyclone)
            {
                if (self.Reflect().inputHandler.inputActions.right.IsPressed && !self.Reflect().inputHandler.inputActions.left.IsPressed)
                {
                    self.Reflect().rb2d.velocity = new Vector3(self.Reflect().CYCLONE_HORIZONTAL_SPEED, self.Reflect().rb2d.velocity.y);
                }
                else if (self.Reflect().inputHandler.inputActions.left.IsPressed && !self.Reflect().inputHandler.inputActions.right.IsPressed)
                {
                    self.Reflect().rb2d.velocity = new Vector3(-self.Reflect().CYCLONE_HORIZONTAL_SPEED, self.Reflect().rb2d.velocity.y);
                }
                else
                {
                    self.Reflect().rb2d.velocity = new Vector3(0f, self.Reflect().rb2d.velocity.y);
                }
            }
            if (self.Reflect().cState.swimming)
            {
                self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().rb2d.velocity.y + self.Reflect().SWIM_ACCEL);
                if (self.Reflect().rb2d.velocity.y > self.Reflect().SWIM_MAX_SPEED)
                {
                    self.Reflect().rb2d.velocity = new Vector3(self.Reflect().rb2d.velocity.x, self.Reflect().SWIM_MAX_SPEED);
                }
            }
            if (self.Reflect().cState.superDashOnWall && !self.Reflect().cState.onConveyorV)
            {
                self.Reflect().rb2d.velocity = new Vector3(0f, 0f);
            }
            if (self.Reflect().cState.onConveyor && ((self.Reflect().cState.onGround && !self.Reflect().cState.superDashing) || self.Reflect().hero_state == ActorStates.hard_landing))
            {
                if (self.Reflect().cState.freezeCharge || self.Reflect().hero_state == ActorStates.hard_landing || self.Reflect().controlReqlinquished)
                {
                    self.Reflect().rb2d.velocity = new Vector3(0f, 0f);
                }
                self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x + self.Reflect().conveyorSpeed, self.Reflect().rb2d.velocity.y);
            }
            if (self.Reflect().cState.inConveyorZone)
            {
                if (self.Reflect().cState.freezeCharge || self.Reflect().hero_state == ActorStates.hard_landing)
                {
                    self.Reflect().rb2d.velocity = new Vector3(0f, 0f);
                }
                self.Reflect().rb2d.velocity = new Vector2(self.Reflect().rb2d.velocity.x + self.Reflect().conveyorSpeed, self.Reflect().rb2d.velocity.y);
                self.Reflect().superDash.SendEvent("SLOPE CANCEL");
            }
            if (self.Reflect().cState.slidingLeft && self.Reflect().rb2d.velocity.x > -5f)
            {
                self.Reflect().rb2d.velocity = new Vector2(-5f, self.Reflect().rb2d.velocity.y);
            }
            if (self.Reflect().landingBufferSteps > 0)
            {
                self.Reflect().landingBufferSteps--;
            }
            if (self.Reflect().ledgeBufferSteps > 0)
            {
                self.Reflect().ledgeBufferSteps--;
            }
            if (self.Reflect().headBumpSteps > 0)
            {
                self.Reflect().headBumpSteps--;
            }
            if (self.Reflect().jumpReleaseQueueSteps > 0)
            {
                self.Reflect().jumpReleaseQueueSteps--;
            }
            self.Reflect().positionHistory[1] = self.Reflect().positionHistory[0];
            self.Reflect().positionHistory[0] = self.Reflect().transform.position;
            self.Reflect().cState.wasOnGround = self.Reflect().cState.onGround;
            var rb2d = ReflectionHelper.GetField<HeroController, Rigidbody2D>(self, "rb2d");
            var n = momentum.magnitude;
            if (n >= resistance)
            {
                rb2d.velocity += momentum;
                n -= resistance;
                momentum = n * momentum.normalized;
            }
            if (self.cState.onGround && rb2d.velocity.y <= 0)
            {
                momentum = Vector2.zero;
            }
            var diagonal = (Dash.instance.dashingUp || Dash.instance.dashingDown) && (Dash.instance.dashingLeft || Dash.instance.dashingRight);
            if (self.cState.dashing && (!diagonal || Mathf.Sign(rb2d.velocity.x) != (Dash.instance.dashingLeft ? -1 : 1)))
            {
                momentum = Vector2.zero;
            }
            if (self.cState.dashing)
            {
                var v = rb2d.velocity;
                if (self.cState.onGround && v.x != 0 & v.y < 0)
                {
                    v = new Vector2(v.x, 0);
                }
                if (diagonal)
                {
                    float a;
                    if (self.cState.facingRight)
                    {
                        a = -Mathf.Atan2(v.y, v.x);
                    }
                    else
                    {
                        a = Mathf.Atan2(v.y, v.x) + Mathf.PI;
                    }
                    Dash.instance.AdjustSprite(0, a / Mathf.PI * 180);
                }
                else if (Dash.instance.dashingUp)
                {
                    Dash.instance.AdjustSprite(180, 0);
                }
            }
        }
        private void HeroController_ResetMotion(On.HeroController.orig_ResetMotion orig, HeroController self)
        {
            momentum = Vector2.zero;
            orig(self);
        }
    }
}