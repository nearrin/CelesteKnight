namespace Celeste
{
    public class Input : Module
    {
        private bool upPressedLastFrame = false;
        private bool downPressedLastFrame = false;
        private bool upPressedMoreRecently = false;
        private bool leftPressedLastFrame = false;
        private bool rightPressedLastFrame = false;
        private bool leftPressedMoreRecently = false;
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.Update += HeroController_Update;
                On.HeroController.FilterInput += HeroController_FilterInput1;
            }
            else
            {
                On.HeroController.Update -= HeroController_Update;
                On.HeroController.FilterInput -= HeroController_FilterInput1;
            }
        }
        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            HeroActions inputActions = ReflectionHelper.GetField<HeroController, InputHandler>(HeroController.instance, "inputHandler").inputActions;
            if (inputActions.up.IsPressed && !upPressedLastFrame)
            {
                upPressedMoreRecently = true;
            }
            upPressedLastFrame = inputActions.up.IsPressed;
            if (inputActions.down.IsPressed && !downPressedLastFrame)
            {
                upPressedMoreRecently = false;
            }
            downPressedLastFrame = inputActions.down.IsPressed;
            if (inputActions.left.IsPressed && !leftPressedLastFrame)
            {
                leftPressedMoreRecently = true;
            }
            leftPressedLastFrame = inputActions.left.IsPressed;
            if (inputActions.right.IsPressed && !rightPressedLastFrame)
            {
                leftPressedMoreRecently = false;
            }
            rightPressedLastFrame = inputActions.right.IsPressed;
            orig(self);
        }
        private void HeroController_FilterInput1(On.HeroController.orig_FilterInput orig, HeroController self)
        {
            var this_ = self;
            HeroActions inputActions = ReflectionHelper.GetField<HeroController, InputHandler>(HeroController.instance, "inputHandler").inputActions;
            if (this_.move_input > 0.3f)
            {
                this_.move_input = 1f;
            }
            else if (this_.move_input < -0.3f)
            {
                this_.move_input = -1f;
            }
            else if (inputActions.left.IsPressed)
            {
                var newM = 0f;
                if (leftPressedMoreRecently)
                {
                    newM = -1f;
                }
                else
                {
                    newM = 1f;
                }
                Log("Overriding move_input from " + this_.move_input.ToString() + " to " + newM.ToString() + ".");
                this_.move_input = newM;
            }
            else
            {
                this_.move_input = 0f;

            }
            if (this_.vertical_input > 0.5f)
            {
                this_.vertical_input = 1f;
                return;
            }
            if (this_.vertical_input < -0.5f)
            {
                this_.vertical_input = -1f;
                return;
            }
            if (inputActions.up.IsPressed)
            {
                var newV = 0f;
                if (upPressedMoreRecently)
                {
                    newV = 1f;
                }
                else
                {
                    newV = -1f;
                }
                Log("Overriding vertical_input from " + this_.vertical_input.ToString() + " to " + newV.ToString() + ".");
                this_.vertical_input = newV;
            }
            else
            {
                this_.vertical_input = 0f;
            }
        }
    }
}