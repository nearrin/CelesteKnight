namespace Celeste
{
    public class Momentum : Module
    {
        public static Vector2 momentum = new Vector2(0, 0);
        private static float resistance = 0.4f;
        private static float effectDistance = 4;
        private static float effectTime;
        public static float effectMaxTime;
        private static Vector3 effectInitialScale = new Vector3(0.1f, 0.04f, 1);
        private static Vector3 effectFinalScale = new Vector3(0.5f, 0.2f, 1);
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
            if (effectMaxTime > 0)
            {
                if (h.rb2d.velocity.magnitude >= 0.01f && effectTime >= effectDistance / h.rb2d.velocity.magnitude)
                {
                    effectTime = 0;
                    var a = Mathf.Atan2(h.rb2d.velocity.y, h.rb2d.velocity.x) / Mathf.PI * 180 - 90;
                    var r = h.shadowRingPrefab.Spawn(h.gameObject.GetComponent<BoxCollider2D>().bounds.center, Quaternion.Euler(new Vector3(0, 0, a)));
                    var f = r.LocateMyFSM("Play Effect");
                    f.GetAction<SetScale>("Init", 3).x = effectInitialScale.x;
                    f.GetAction<SetScale>("Init", 3).y = effectInitialScale.y;
                    f.GetAction<iTweenScaleTo>("Grow", 0).vectorScale = effectFinalScale;
                    f.GetAction<ScaleTo>("Grow", 1).target = effectFinalScale;
                }
                effectTime += Time.deltaTime;
                effectMaxTime -= Time.deltaTime;
            }
        }
    }
}