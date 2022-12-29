namespace CelesteKnight
{
    public class Input : Module
    {
        private static bool upPressedLastFrame = false;
        private static bool downPressedLastFrame = false;
        private static bool upPressedMoreRecently = false;
        private static bool leftPressedLastFrame = false;
        private static bool rightPressedLastFrame = false;
        private static bool leftPressedMoreRecently = false;
        public override void SetActive(bool active)
        {
            if (active)
            {
                On.HeroController.FilterInput += HeroController_FilterInput1;
            }
            else
            {
                On.HeroController.FilterInput -= HeroController_FilterInput1;
            }
        }
        private static void RefreshInput()
        {
            var inputActions = InputHandler.Instance.inputActions;
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
        }
        private void HeroController_FilterInput1(On.HeroController.orig_FilterInput orig, HeroController self)
        {
            var this_ = self;
            var inputActions = InputHandler.Instance.inputActions;
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
                this_.vertical_input = newV;
            }
            else
            {
                this_.vertical_input = 0f;
            }
        }
        public static bool UpPressed()
        {
            RefreshInput();
            var inputActions = InputHandler.Instance.inputActions;
            if (!inputActions.up.IsPressed || !inputActions.down.IsPressed)
            {
                return inputActions.up.IsPressed;
            }
            else
            {
                return upPressedMoreRecently;
            }
        }
        public static bool DownPressed()
        {
            RefreshInput();
            var inputActions = InputHandler.Instance.inputActions;
            if (!inputActions.up.IsPressed || !inputActions.down.IsPressed)
            {
                return inputActions.down.IsPressed;
            }
            else
            {
                return !upPressedMoreRecently;
            }
        }
        public static bool LeftPressed()
        {
            RefreshInput();
            var inputActions = InputHandler.Instance.inputActions;
            if (!inputActions.left.IsPressed || !inputActions.right.IsPressed)
            {
                return inputActions.left.IsPressed;
            }
            else
            {
                return leftPressedMoreRecently;
            }
        }
        public static bool RightPressed()
        {
            RefreshInput();
            var inputActions = InputHandler.Instance.inputActions;
            if (!inputActions.left.IsPressed || !inputActions.right.IsPressed)
            {
                return inputActions.right.IsPressed;
            }
            else
            {
                return !leftPressedMoreRecently;
            }
        }
        public static void Update()
        {
            RefreshInput();
        }
    }
}