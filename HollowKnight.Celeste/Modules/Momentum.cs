namespace Celeste
{
    public class Momentum : Module
    {
        public static Momentum instance;
        public Vector2 momentum = new Vector2(0, 0);
        public float resistance = 7.0f / 50;
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
            var n = momentum.magnitude;
            if (n > resistance)
            {
                var rb2d = ReflectionHelper.GetField<HeroController, Rigidbody2D>(self, "rb2d");
                rb2d.velocity += momentum;
                n -= resistance;
                momentum = n * momentum.normalized;
                if (self.cState.onGround && rb2d.velocity.y <= 0)
                {
                    momentum = Vector2.zero;
                }
            }
            if (self.cState.dashing)
            {
                var v = ReflectionHelper.GetField<HeroController, Rigidbody2D>(self, "rb2d").velocity;
                if (self.cState.onGround && v.x != 0 & v.y < 0)
                {
                    v = new Vector2(v.x, 0);
                }
                var a = self.dashingDown ? Mathf.Atan2(v.x, -v.y) / Mathf.PI * 180 : 0;
                Dash.instance.RotateSprite(a);
            }
        }

    }
}