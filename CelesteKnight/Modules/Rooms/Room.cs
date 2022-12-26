namespace CelesteKnight
{
    public class Room : Module
    {
        public override void SetActive(bool active)
        {
            if (active)
            {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            }
        }
        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            Log("Entering scene: " + arg1.name);
        }
    }
}