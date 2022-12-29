namespace CelesteKnight
{
    public class Update : Module
    {
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.JumpReleased += HeroController_JumpReleased;
                On.HeroController.LookForInput += HeroController_LookForInput;
                On.HeroController.Update += HeroController_Update;
                On.HeroController.FixedUpdate += HeroController_FixedUpdate;
            }
            else
            {
                On.HeroController.JumpReleased -= HeroController_JumpReleased;
                On.HeroController.LookForInput -= HeroController_LookForInput;
                On.HeroController.Update -= HeroController_Update;
                On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
            }
        }
        private void HeroController_JumpReleased(On.HeroController.orig_JumpReleased orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.rb2d.velocity.y > 0f && h.jumped_steps >= h.JUMP_STEPS_MIN && !h.inAcid && !h.cState.shroomBouncing)
            {
                if (h.jumpReleaseQueueingEnabled)
                {
                    if (h.jumpReleaseQueuing && h.jumpReleaseQueueSteps <= 0)
                    {
                        h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, 0f);
                        h.CancelJump();
                    }
                }
                else
                {
                    h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, 0f);
                    h.CancelJump();
                }
            }
            h.jumpQueuing = false;
            h.doubleJumpQueuing = false;
            if (h.cState.swimming)
            {
                h.cState.swimming = false;
            }
        }
        private void HeroController_LookForInput(On.HeroController.orig_LookForInput orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.acceptingInput && !h.gm.isPaused && h.isGameplayScene)
            {
                h.move_input = h.inputHandler.inputActions.moveVector.Vector.x;
                h.vertical_input = h.inputHandler.inputActions.moveVector.Vector.y;
                h.FilterInput();
                if (h.playerData.GetBool("hasWalljump") && h.CanWallSlide() && !h.cState.attacking)
                {
                    if (h.touchingWallL && h.inputHandler.inputActions.left.IsPressed && !h.cState.wallSliding)
                    {
                        h.airDashed = false;
                        h.doubleJumped = false;
                        h.wallSlideVibrationPlayer.Play();
                        h.cState.wallSliding = true;
                        h.cState.willHardLand = false;
#pragma warning disable 612,618
                        h.wallslideDustPrefab.enableEmission = true;
#pragma warning restore 612,618
                        h.wallSlidingL = true;
                        h.wallSlidingR = false;
                        h.FaceLeft();
                        h.CancelFallEffects();
                    }
                    if (h.touchingWallR && h.inputHandler.inputActions.right.IsPressed && !h.cState.wallSliding)
                    {
                        h.airDashed = false;
                        h.doubleJumped = false;
                        h.wallSlideVibrationPlayer.Play();
                        h.cState.wallSliding = true;
                        h.cState.willHardLand = false;
#pragma warning disable 612,618
                        h.wallslideDustPrefab.enableEmission = true;
#pragma warning restore 612,618
                        h.wallSlidingL = false;
                        h.wallSlidingR = true;
                        h.FaceRight();
                        h.CancelFallEffects();
                    }
                }
                if (h.cState.wallSliding && h.inputHandler.inputActions.down.WasPressed)
                {
                    h.CancelWallsliding();
                    h.FlipSprite();
                }
                if (h.wallLocked && h.wallJumpedL && h.inputHandler.inputActions.right.IsPressed && h.wallLockSteps >= h.WJLOCK_STEPS_SHORT)
                {
                    h.wallLocked = false;
                }
                if (h.wallLocked && h.wallJumpedR && h.inputHandler.inputActions.left.IsPressed && h.wallLockSteps >= h.WJLOCK_STEPS_SHORT)
                {
                    h.wallLocked = false;
                }
                if (h.inputHandler.inputActions.jump.WasReleased && h.jumpReleaseQueueingEnabled)
                {
                    h.jumpReleaseQueueSteps = h.JUMP_RELEASE_QUEUE_STEPS;
                    h.jumpReleaseQueuing = true;
                }
                if (!h.inputHandler.inputActions.jump.IsPressed || !Dash.lastActionJump)
                {
                    h.JumpReleased();
                }
                if (!h.inputHandler.inputActions.dash.IsPressed)
                {
                    if (h.cState.preventDash && !h.cState.dashCooldown)
                    {
                        h.cState.preventDash = false;
                    }
                    h.dashQueuing = false;
                }
                if (!h.inputHandler.inputActions.attack.IsPressed)
                {
                    h.attackQueuing = false;
                }
            }
        }
        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            MethodInfo m = typeof(ModHooks).GetMethod("OnHeroUpdate", BindingFlags.NonPublic | BindingFlags.Static);
            m.Invoke(null, new object[] { });
            Input.Update();
            var h = self.Reflect();
            if (Time.frameCount % 10 == 0)
            {
                h.Update10();
            }
            h.current_velocity = h.rb2d.velocity;
            h.FallCheck();
            h.FailSafeChecks();
            if (h.hero_state == ActorStates.running && !h.cState.dashing && !h.cState.backDashing && !h.controlReqlinquished)
            {
                if (h.cState.inWalkZone)
                {
                    h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
                    h.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_WALK);
                }
                else
                {
                    h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
                    h.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_RUN);
                }
                if (h.runMsgSent && h.rb2d.velocity.x > -0.1f && h.rb2d.velocity.x < 0.1f)
                {
                    h.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
                    h.runEffect.transform.SetParent(null, true);
                    h.runMsgSent = false;
                }
                if (!h.runMsgSent && (h.rb2d.velocity.x < -0.1f || h.rb2d.velocity.x > 0.1f))
                {
                    h.runEffect = h.runEffectPrefab.Spawn();
                    h.runEffect.transform.SetParent(h.gameObject.transform, false);
                    h.runMsgSent = true;
                }
            }
            else
            {
                h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
                h.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
                if (h.runMsgSent)
                {
                    h.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
                    h.runEffect.transform.SetParent(null, true);
                    h.runMsgSent = false;
                }
            }
            if (h.hero_state == ActorStates.dash_landing)
            {
                h.dashLandingTimer += Time.deltaTime;
                if (h.dashLandingTimer > h.DOWN_DASH_TIME)
                {
                    h.BackOnGround();
                }
            }
            if (h.hero_state == ActorStates.hard_landing)
            {
                h.hardLandingTimer += Time.deltaTime;
                if (h.hardLandingTimer > h.HARD_LANDING_TIME)
                {
                    h.SetState(ActorStates.grounded);
                    h.BackOnGround();
                }
            }
            else if (h.hero_state == ActorStates.no_input)
            {
                if (h.cState.recoiling)
                {
                    if ((!h.playerData.GetBool("equippedCharm_4") && h.recoilTimer < h.RECOIL_DURATION) || (h.playerData.GetBool("equippedCharm_4") && h.recoilTimer < h.RECOIL_DURATION_STAL))
                    {
                        h.recoilTimer += Time.deltaTime;
                    }
                    else
                    {
                        h.CancelDamageRecoil();
                        if ((h.prev_hero_state == ActorStates.idle || h.prev_hero_state == ActorStates.running) && !h.CheckTouchingGround())
                        {
                            h.cState.onGround = false;
                            h.SetState(ActorStates.airborne);
                        }
                        else
                        {
                            h.SetState(ActorStates.previous);
                        }
                        h.fsm_thornCounter.SendEvent("THORN COUNTER");
                    }
                }
            }
            else if (h.hero_state != ActorStates.no_input)
            {
                h.LookForInput();
                if (h.cState.recoiling)
                {
                    h.cState.recoiling = false;
                    h.AffectedByGravity(true);
                }
                if (h.cState.attacking && !h.cState.dashing)
                {
                    h.attack_time += Time.deltaTime;
                    if (h.attack_time >= h.attackDuration)
                    {
                        h.ResetAttacks();
                        h.animCtrl.StopAttack();
                    }
                }
                if (h.cState.bouncing)
                {
                    if (h.bounceTimer < h.BOUNCE_TIME)
                    {
                        h.bounceTimer += Time.deltaTime;
                    }
                    else
                    {
                        h.CancelBounce();
                        h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, 0f);
                    }
                }
                if (h.cState.shroomBouncing && h.current_velocity.y <= 0f)
                {
                    h.cState.shroomBouncing = false;
                }
                if (h.hero_state == ActorStates.idle)
                {
                    if (!h.controlReqlinquished && !h.gm.isPaused)
                    {
                        if (h.inputHandler.inputActions.up.IsPressed || h.inputHandler.inputActions.rs_up.IsPressed)
                        {
                            h.cState.lookingDown = false;
                            h.cState.lookingDownAnim = false;
                            if (h.lookDelayTimer >= h.LOOK_DELAY || (h.inputHandler.inputActions.rs_up.IsPressed && !h.cState.jumping && !h.cState.dashing))
                            {
                                h.cState.lookingUp = true;
                            }
                            else
                            {
                                h.lookDelayTimer += Time.deltaTime;
                            }
                            if (h.lookDelayTimer >= h.LOOK_ANIM_DELAY || h.inputHandler.inputActions.rs_up.IsPressed)
                            {
                                h.cState.lookingUpAnim = true;
                            }
                            else
                            {
                                h.cState.lookingUpAnim = false;
                            }
                        }
                        else if (h.inputHandler.inputActions.down.IsPressed || h.inputHandler.inputActions.rs_down.IsPressed)
                        {
                            h.cState.lookingUp = false;
                            h.cState.lookingUpAnim = false;
                            if (h.lookDelayTimer >= h.LOOK_DELAY || (h.inputHandler.inputActions.rs_down.IsPressed && !h.cState.jumping && !h.cState.dashing))
                            {
                                h.cState.lookingDown = true;
                            }
                            else
                            {
                                h.lookDelayTimer += Time.deltaTime;
                            }
                            if (h.lookDelayTimer >= h.LOOK_ANIM_DELAY || h.inputHandler.inputActions.rs_down.IsPressed)
                            {
                                h.cState.lookingDownAnim = true;
                            }
                            else
                            {
                                h.cState.lookingDownAnim = false;
                            }
                        }
                        else
                        {
                            h.ResetLook();
                        }
                    }
                    h.runPuffTimer = 0f;
                }
            }
            h.LookForQueueInput();
            if (h.drainMP)
            {
                h.drainMP_timer += Time.deltaTime;
                h.drainMP_seconds += Time.deltaTime;
                while (h.drainMP_timer >= h.drainMP_time)
                {
                    h.MP_drained += 1f;
                    h.drainMP_timer -= h.drainMP_time;
                    h.TakeMP(1);
                    h.gm.soulOrb_fsm.SendEvent("MP DRAIN");
                    if (h.MP_drained == h.focusMP_amount)
                    {
                        h.MP_drained -= h.drainMP_time;
                        h.proxyFSM.SendEvent("HeroCtrl-FocusCompleted");
                    }
                }
            }
            if (h.cState.wallSliding)
            {
                if (h.airDashed)
                {
                    h.airDashed = false;
                }
                if (h.doubleJumped)
                {
                    h.doubleJumped = false;
                }
                if (h.cState.onGround)
                {
                    h.FlipSprite();
                    h.CancelWallsliding();
                }
                if (!h.cState.touchingWall)
                {
                    h.FlipSprite();
                    h.CancelWallsliding();
                }
                if (!h.CanWallSlide())
                {
                    h.CancelWallsliding();
                }
                if (!h.playedMantisClawClip)
                {
                    h.audioSource.PlayOneShot(h.mantisClawClip, 1f);
                    h.playedMantisClawClip = true;
                }
                if (!h.playingWallslideClip)
                {
                    if (h.wallslideClipTimer <= h.WALLSLIDE_CLIP_DELAY)
                    {
                        h.wallslideClipTimer += Time.deltaTime;
                    }
                    else
                    {
                        h.wallslideClipTimer = 0f;
                        h.audioCtrl.PlaySound(HeroSounds.WALLSLIDE);
                        h.playingWallslideClip = true;
                    }
                }
            }
            else if (h.playedMantisClawClip)
            {
                h.playedMantisClawClip = false;
            }
            if (!h.cState.wallSliding && h.playingWallslideClip)
            {
                h.audioCtrl.StopSound(HeroSounds.WALLSLIDE);
                h.playingWallslideClip = false;
            }
            if (!h.cState.wallSliding && h.wallslideClipTimer > 0f)
            {
                h.wallslideClipTimer = 0f;
            }
            if (h.wallSlashing && !h.cState.wallSliding)
            {
                h.CancelAttack();
            }
            if (h.attack_cooldown > 0f)
            {
                h.attack_cooldown -= Time.deltaTime;
            }
            if (h.dashCooldownTimer > 0f)
            {
                h.dashCooldownTimer -= Time.deltaTime;
            }
            if (h.shadowDashTimer > 0f)
            {
                h.shadowDashTimer -= Time.deltaTime;
                if (h.shadowDashTimer <= 0f)
                {
                    h.spriteFlash.FlashShadowRecharge();
                }
            }
            h.preventCastByDialogueEndTimer -= Time.deltaTime;
            if (!h.gm.isPaused)
            {
                if (h.inputHandler.inputActions.attack.IsPressed && h.CanNailCharge())
                {
                    h.cState.nailCharging = true;
                    h.nailChargeTimer += Time.deltaTime;
                }
                else if (h.cState.nailCharging || h.nailChargeTimer != 0f)
                {
                    h.artChargeEffect.SetActive(false);
                    h.cState.nailCharging = false;
                    h.audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
                }
                if (h.cState.nailCharging && h.nailChargeTimer > 0.5f && !h.artChargeEffect.activeSelf && h.nailChargeTimer < h.nailChargeTime)
                {
                    h.artChargeEffect.SetActive(true);
                    h.audioCtrl.PlaySound(HeroSounds.NAIL_ART_CHARGE);
                }
                if (h.artChargeEffect.activeSelf && (!h.cState.nailCharging || h.nailChargeTimer > h.nailChargeTime))
                {
                    h.artChargeEffect.SetActive(false);
                    h.audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
                }
                if (!h.artChargedEffect.activeSelf && h.nailChargeTimer >= h.nailChargeTime)
                {
                    h.artChargedEffect.SetActive(true);
                    h.artChargedFlash.SetActive(true);
                    h.artChargedEffectAnim.PlayFromFrame(0);
                    GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
                    h.audioSource.PlayOneShot(h.nailArtChargeComplete, 1f);
                    h.audioCtrl.PlaySound(HeroSounds.NAIL_ART_READY);
                    h.cState.nailCharging = true;
                }
                if (h.artChargedEffect.activeSelf && (h.nailChargeTimer < h.nailChargeTime || !h.cState.nailCharging))
                {
                    h.artChargedEffect.SetActive(false);
                    h.audioCtrl.StopSound(HeroSounds.NAIL_ART_READY);
                }
            }
            if (h.gm.isPaused && !h.inputHandler.inputActions.attack.IsPressed)
            {
                h.cState.nailCharging = false;
                h.nailChargeTimer = 0f;
            }
            if (h.cState.swimming && !h.CanSwim())
            {
                h.cState.swimming = false;
            }
            if (h.parryInvulnTimer > 0f)
            {
                h.parryInvulnTimer -= Time.deltaTime;
            }
            Afterimage.Update();
        }
        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            var h = self.Reflect();
            if (h.cState.recoilingLeft || h.cState.recoilingRight)
            {
                if ((float)h.recoilSteps <= h.RECOIL_HOR_STEPS)
                {
                    h.recoilSteps++;
                }
                else
                {
                    h.CancelRecoilHorizontal();
                }
            }
            if (h.cState.dead)
            {
                h.rb2d.velocity = new Vector2(0f, 0f);
            }
            if ((h.hero_state == ActorStates.hard_landing && !h.cState.onConveyor) || h.hero_state == ActorStates.dash_landing)
            {
                h.ResetMotion();
            }
            else if (h.hero_state == ActorStates.no_input)
            {
                if (h.cState.transitioning)
                {
                    if (h.transitionState == HeroTransitionState.EXITING_SCENE)
                    {
                        h.AffectedByGravity(false);
                        if (!h.stopWalkingOut)
                        {
                            h.rb2d.velocity = new Vector2(h.transition_vel.x, h.transition_vel.y + h.rb2d.velocity.y);
                        }
                    }
                    else if (h.transitionState == HeroTransitionState.ENTERING_SCENE)
                    {
                        h.rb2d.velocity = h.transition_vel;
                    }
                    else if (h.transitionState == HeroTransitionState.DROPPING_DOWN)
                    {
                        h.rb2d.velocity = new Vector2(h.transition_vel.x, h.rb2d.velocity.y);
                    }
                }
                else if (h.cState.recoiling)
                {
                    h.AffectedByGravity(false);
                    h.rb2d.velocity = h.recoilVector;
                }
            }
            else if (h.hero_state != ActorStates.no_input)
            {
                if (h.hero_state == ActorStates.running)
                {
                    if (h.move_input > 0f)
                    {
                        if (h.CheckForBump(CollisionSide.right))
                        {
                            h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, h.BUMP_VELOCITY);
                        }
                    }
                    else if (h.move_input < 0f && h.CheckForBump(CollisionSide.left))
                    {
                        h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, h.BUMP_VELOCITY);
                    }
                }
                if (!h.cState.backDashing && !h.cState.dashing)
                {
                    h.Move(h.move_input);
                    if ((!h.cState.attacking || h.attack_time >= h.ATTACK_RECOVERY_TIME) && !h.cState.wallSliding && !h.wallLocked)
                    {
                        if (h.move_input > 0f && !h.cState.facingRight)
                        {
                            h.FlipSprite();
                            h.CancelAttack();
                        }
                        else if (h.move_input < 0f && h.cState.facingRight)
                        {
                            h.FlipSprite();
                            h.CancelAttack();
                        }
                    }
                    if (h.cState.recoilingLeft)
                    {
                        float num;
                        if (h.recoilLarge)
                        {
                            num = h.RECOIL_HOR_VELOCITY_LONG;
                        }
                        else
                        {
                            num = h.RECOIL_HOR_VELOCITY;
                        }
                        if (h.rb2d.velocity.x > -num)
                        {
                            h.rb2d.velocity = new Vector2(-num, h.rb2d.velocity.y);
                        }
                        else
                        {
                            h.rb2d.velocity = new Vector2(h.rb2d.velocity.x - num, h.rb2d.velocity.y);
                        }
                    }
                    if (h.cState.recoilingRight)
                    {
                        float num2;
                        if (h.recoilLarge)
                        {
                            num2 = h.RECOIL_HOR_VELOCITY_LONG;
                        }
                        else
                        {
                            num2 = h.RECOIL_HOR_VELOCITY;
                        }
                        if (h.rb2d.velocity.x < num2)
                        {
                            h.rb2d.velocity = new Vector2(num2, h.rb2d.velocity.y);
                        }
                        else
                        {
                            h.rb2d.velocity = new Vector2(h.rb2d.velocity.x + num2, h.rb2d.velocity.y);
                        }
                    }
                }
                if ((h.cState.lookingUp || h.cState.lookingDown) && Mathf.Abs(h.move_input) > 0.6f)
                {
                    h.ResetLook();
                }
                if (h.cState.jumping)
                {
                    h.Jump();
                }
                if (h.cState.doubleJumping)
                {
                    h.DoubleJump();
                }
                if (h.cState.dashing)
                {
                    h.Dash();
                }
                if (h.cState.casting)
                {
                    if (h.cState.castRecoiling)
                    {
                        if (h.cState.facingRight)
                        {
                            h.rb2d.velocity = new Vector2(-h.CAST_RECOIL_VELOCITY, 0f);
                        }
                        else
                        {
                            h.rb2d.velocity = new Vector2(h.CAST_RECOIL_VELOCITY, 0f);
                        }
                    }
                    else
                    {
                        h.rb2d.velocity = Vector2.zero;
                    }
                }
                if (h.cState.bouncing)
                {
                    h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, h.BOUNCE_VELOCITY);
                }
                bool shroomBouncing = h.cState.shroomBouncing;
                if (h.wallLocked)
                {
                    if (h.wallJumpedR)
                    {
                        h.rb2d.velocity = new Vector2(h.currentWalljumpSpeed, h.rb2d.velocity.y);
                    }
                    else if (h.wallJumpedL)
                    {
                        h.rb2d.velocity = new Vector2(-h.currentWalljumpSpeed, h.rb2d.velocity.y);
                    }
                    h.wallLockSteps++;
                    if (h.wallLockSteps > h.WJLOCK_STEPS_LONG)
                    {
                        h.wallLocked = false;
                    }
                    h.currentWalljumpSpeed -= h.walljumpSpeedDecel;
                }
                if (h.cState.wallSliding)
                {
                    if (h.wallSlidingL && h.inputHandler.inputActions.right.IsPressed)
                    {
                        h.wallUnstickSteps++;
                    }
                    else if (h.wallSlidingR && h.inputHandler.inputActions.left.IsPressed)
                    {
                        h.wallUnstickSteps++;
                    }
                    else
                    {
                        h.wallUnstickSteps = 0;
                    }
                    if (h.wallUnstickSteps >= h.WALL_STICKY_STEPS)
                    {
                        h.CancelWallsliding();
                    }
                    if (h.wallSlidingL)
                    {
                        if (!h.CheckStillTouchingWall(CollisionSide.left, false))
                        {
                            h.FlipSprite();
                            h.CancelWallsliding();
                        }
                    }
                    else if (h.wallSlidingR && !h.CheckStillTouchingWall(CollisionSide.right, false))
                    {
                        h.FlipSprite();
                        h.CancelWallsliding();
                    }
                }
            }
            if (h.rb2d.velocity.y < -h.MAX_FALL_VELOCITY && !h.inAcid && !h.controlReqlinquished && !h.cState.shadowDashing && !h.cState.spellQuake)
            {
                h.rb2d.velocity = new Vector2(h.rb2d.velocity.x, -h.MAX_FALL_VELOCITY);
            }
            if (h.jumpQueuing)
            {
                h.jumpQueueSteps++;
            }
            if (h.doubleJumpQueuing)
            {
                h.doubleJumpQueueSteps++;
            }
            if (h.dashQueuing)
            {
                h.dashQueueSteps++;
            }
            if (h.attackQueuing)
            {
                h.attackQueueSteps++;
            }
            if (h.cState.wallSliding && !h.cState.onConveyorV)
            {
                if (h.rb2d.velocity.y > h.WALLSLIDE_SPEED)
                {
                    h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.rb2d.velocity.y - h.WALLSLIDE_DECEL);
                    if (h.rb2d.velocity.y < h.WALLSLIDE_SPEED)
                    {
                        h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.WALLSLIDE_SPEED);
                    }
                }
                if (h.rb2d.velocity.y < h.WALLSLIDE_SPEED)
                {
                    h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.rb2d.velocity.y + h.WALLSLIDE_DECEL);
                    if (h.rb2d.velocity.y < h.WALLSLIDE_SPEED)
                    {
                        h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.WALLSLIDE_SPEED);
                    }
                }
            }
            if (h.nailArt_cyclone)
            {
                if (h.inputHandler.inputActions.right.IsPressed && !h.inputHandler.inputActions.left.IsPressed)
                {
                    h.rb2d.velocity = new Vector3(h.CYCLONE_HORIZONTAL_SPEED, h.rb2d.velocity.y);
                }
                else if (h.inputHandler.inputActions.left.IsPressed && !h.inputHandler.inputActions.right.IsPressed)
                {
                    h.rb2d.velocity = new Vector3(-h.CYCLONE_HORIZONTAL_SPEED, h.rb2d.velocity.y);
                }
                else
                {
                    h.rb2d.velocity = new Vector3(0f, h.rb2d.velocity.y);
                }
            }
            if (h.cState.swimming)
            {
                h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.rb2d.velocity.y + h.SWIM_ACCEL);
                if (h.rb2d.velocity.y > h.SWIM_MAX_SPEED)
                {
                    h.rb2d.velocity = new Vector3(h.rb2d.velocity.x, h.SWIM_MAX_SPEED);
                }
            }
            if (h.cState.superDashOnWall && !h.cState.onConveyorV)
            {
                h.rb2d.velocity = new Vector3(0f, 0f);
            }
            if (h.cState.onConveyor && ((h.cState.onGround && !h.cState.superDashing) || h.hero_state == ActorStates.hard_landing))
            {
                if (h.cState.freezeCharge || h.hero_state == ActorStates.hard_landing || h.controlReqlinquished)
                {
                    h.rb2d.velocity = new Vector3(0f, 0f);
                }
                h.rb2d.velocity = new Vector2(h.rb2d.velocity.x + h.conveyorSpeed, h.rb2d.velocity.y);
            }
            if (h.cState.inConveyorZone)
            {
                if (h.cState.freezeCharge || h.hero_state == ActorStates.hard_landing)
                {
                    h.rb2d.velocity = new Vector3(0f, 0f);
                }
                h.rb2d.velocity = new Vector2(h.rb2d.velocity.x + h.conveyorSpeed, h.rb2d.velocity.y);
                h.superDash.SendEvent("SLOPE CANCEL");
            }
            if (h.cState.slidingLeft && h.rb2d.velocity.x > -5f)
            {
                h.rb2d.velocity = new Vector2(-5f, h.rb2d.velocity.y);
            }
            if (h.landingBufferSteps > 0)
            {
                h.landingBufferSteps--;
            }
            if (h.ledgeBufferSteps > 0)
            {
                h.ledgeBufferSteps--;
            }
            if (h.headBumpSteps > 0)
            {
                h.headBumpSteps--;
            }
            if (h.jumpReleaseQueueSteps > 0)
            {
                h.jumpReleaseQueueSteps--;
            }
            h.positionHistory[1] = h.positionHistory[0];
            h.positionHistory[0] = h.transform.position;
            h.cState.wasOnGround = h.cState.onGround;
            Momentum.FixedUpdate();
            Dash.FixedUpdate();
        }
    }
}