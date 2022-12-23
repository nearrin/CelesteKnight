namespace Celeste
{
    public class Module
    {
        protected void Log(string message)
        {
            Celeste.instance.Log(message);
        }
        public virtual List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
            };
        }
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
        }
        public virtual void SetActive(bool active)
        {
        }
    }
}