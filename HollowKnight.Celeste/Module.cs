namespace Celeste
{
    public class Module
    {
        private Celeste Celeste_;
        public Module(Celeste celeste)
        {
            Celeste_ = celeste;
        }
        protected bool On()
        {
            return Celeste_.settings_.on;
        }
        protected void Log(string message)
        {
            Celeste_.Log(message);
        }
        public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
        }
    }
}