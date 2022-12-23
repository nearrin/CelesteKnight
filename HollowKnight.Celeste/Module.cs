namespace Celeste
{
    public class Module
    {
        public static Module instance;
        public Module()
        {
            instance = this;
        }
        protected void Log(string message)
        {
            Celeste.instance.Log(message);
        }
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
        }
        public virtual void SetActive(bool active)
        {
        }
    }
}