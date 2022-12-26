namespace Celeste
{
    public class Momentum : Module
    {
        public static Vector2 momentum = new Vector2(0, 0);
        public static float resistance = 0.4f;
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.ResetMotion += HeroController_ResetMotion;
            }
            else
            {
                On.HeroController.ResetMotion -= HeroController_ResetMotion;
            }
        }
        private void HeroController_ResetMotion(On.HeroController.orig_ResetMotion orig, HeroController self)
        {
            momentum = Vector2.zero;
            orig(self);
        }
        public static void FixedUpdate()
        {
            var h = HeroController.instance.Reflect();
            var n = momentum.magnitude;
            if (n >= resistance)
            {
                h.rb2d.velocity += momentum;
                n -= resistance;
                momentum = n * momentum.normalized;
            }
            if (h.cState.onGround && h.rb2d.velocity.y <= 0)
            {
                momentum = Vector2.zero;
            }
            var diagonal = (Dash.instance.dashingUp || Dash.instance.dashingDown) && (Dash.instance.dashingLeft || Dash.instance.dashingRight);
            if (h.cState.dashing && (!diagonal || Mathf.Sign(h.rb2d.velocity.x) != (Dash.instance.dashingLeft ? -1 : 1)))
            {
                momentum = Vector2.zero;
            }
        }
    }
}