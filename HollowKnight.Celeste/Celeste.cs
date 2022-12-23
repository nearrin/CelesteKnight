global using GlobalEnums;
global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;
global using Modding;
global using Satchel;
global using UnityEngine;
namespace Celeste
{
    public class Settings
    {
        public bool on = true;
    }
    public class Celeste : Mod, IMenuMod, IGlobalSettings<Settings>
    {
        public static Celeste instance;
        public bool ToggleButtonInsideMenu => true;
        public Settings settings_ = new Settings();
        private List<Module> modules = new List<Module>();
        public Celeste() : base("Celeste")
        {
            instance = this;
            modules.Add(new Input());
            modules.Add(new Dash());
            modules.Add(new Afterimage());
            modules.Add(new Momentum());
        }
        public override string GetVersion() => "1.0.0.0";
        public override List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> p = new List<(string, string)>();
            foreach (var module in modules)
            {
                foreach(var name in module.GetPreloadNames())
                {
                    p.Add(name);
                }
            }
            return p;
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            foreach (var module in modules)
            {
                module.Initialize(preloadedObjects);
            }
            SetActive(settings_.on);
        }
        private void SetActive(bool active)
        {
            foreach (var module in modules)
            {
                module.SetActive(active);
            }
        }
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? menu)
        {
            List<IMenuMod.MenuEntry> menus = new();
            menus.Add(
                new()
                {
                    Values = new string[]
                    {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                    },
                    Saver = i => {
                        settings_.on = i == 0;
                        SetActive(settings_.on);
                    },
                    Loader = () => settings_.on ? 0 : 1
                }
            ); ;
            return menus;
        }
        public void OnLoadGlobal(Settings settings) => settings_ = settings;
        public Settings OnSaveGlobal() => settings_;
    }
}