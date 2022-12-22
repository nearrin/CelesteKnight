namespace Celeste
{
    public class Dash : Module
    {
        public Dash(Celeste celeste) : base(celeste)
        {
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            global::On.HeroController.HeroDash += HeroController_HeroDash;
            global::On.PlayerData.GetBool += PlayerData_GetBool;
        }
        private void HeroController_HeroDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            var this_ = HeroController.instance;
            HeroActions inputActions = ReflectionHelper.GetField<HeroController, InputHandler>(HeroController.instance, "inputHandler").inputActions;
            if (inputActions.down.IsPressed && !this_.cState.onGround && !inputActions.left.IsPressed && !inputActions.right.IsPressed)
            {
            }
            orig(self);
            var d = ReflectionHelper.GetField<HeroController, float>(HeroController.instance, "dashCooldownTimer");
            var newD = 0.2f;
            // Log("Overriding dashCooldownTimer from " + d.ToString() + " to " + newD.ToString() + ".");
            ReflectionHelper.SetField<HeroController, float>(HeroController.instance, "dashCooldownTimer", newD);
        }
        private bool PlayerData_GetBool(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            var b = orig(self, boolName);
            var newB = false;
            if (boolName == "equippedCharm_31")
            {
                // Log("Overriding equippedCharm_31 from " + b.ToString() + " to " + newB.ToString() + ".");
                b = newB;
            }
            return b;
        }
    }
}