namespace Celeste
{
    public class Dash : Module
    {
        private bool dashingUp = false;
        private bool dashingDown = false;
        private bool dashingLeft = false;
        private bool dashingRight = false;
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.HeroDash += HeroController_HeroDash;
                ModHooks.DashVectorHook += ModHooks_DashVectorHook;
                On.HeroController.FinishedDashing += HeroController_FinishedDashing;
            }
            else
            {
                On.HeroController.HeroDash -= HeroController_HeroDash;
                ModHooks.DashVectorHook -= ModHooks_DashVectorHook;
                On.HeroController.FinishedDashing -= HeroController_FinishedDashing;
            }
        }
        private void RotateSprite(int direction)
        {
            var h = HeroController.instance;
            var r = h.transform.localRotation.eulerAngles;
            var a = 0;
            if (dashingUp)
            {
                if (dashingLeft)
                {
                    a = -135;
                }
                else if (dashingRight)
                {
                    a = 135;
                }
                else
                {
                    a = 180;
                }
            }
            else if (dashingDown)
            {
                if (dashingLeft)
                {
                    a = -45;
                }
                else if (dashingRight)
                {
                    a = 45;
                }
            }
            var newR = new Vector3(r.x, r.y, r.z + direction * a);
            var c = h.gameObject.GetComponent<BoxCollider2D>().bounds.center;
            h.transform.localRotation = Quaternion.Euler(newR);
            var newC = h.gameObject.GetComponent<BoxCollider2D>().bounds.center;
            var p = h.transform.localPosition;
            var newP = p + c - newC;
            h.transform.localPosition = newP;
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
            if (Input.instance.upPressed() || Input.instance.downPressed())
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
            var s = this_.dashBurst.transform.localScale;
            s = new Vector3(s.x, 1, s.z);
            this_.dashBurst.transform.localScale = s;
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
            RotateSprite(1);
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
        private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
        {
            RotateSprite(-1);
            orig(self);
        }
    }
}