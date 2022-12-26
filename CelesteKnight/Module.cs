namespace CelesteKnight
{
    public class Module
    {
        protected static void Log(string message)
        {
            CelesteKnight.instance.Log(DateTime.Now.ToLongTimeString() + " : " + message);
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