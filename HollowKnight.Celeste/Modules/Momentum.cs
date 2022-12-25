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
                On.HeroController.FixedUpdate += HeroController_FixedUpdate;
            }
            else
            {
                On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
            }
        }
        private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
        {
            orig(self);
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
    }
}