global using UnityEngine;
global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;
global using Modding;
global using Satchel;
namespace Celeste
{
    public class Settings
    {
        public bool on = true;
    }
    public class Celeste : Mod, IMenuMod, IGlobalSettings<Settings>
    {
        public bool ToggleButtonInsideMenu => true;
        public Settings settings_ = new Settings();
        private List<Module> modules = new List<Module>();
        public Celeste() : base("Celeste")
        {
            modules.Add(new Dash(this));
        }
        public override string GetVersion() => "1.0.0.0";
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            foreach (var module in modules)
            {
                module.Initialize(preloadedObjects);
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
                    Saver = i => settings_.on = i == 0,
                    Loader = () => settings_.on ? 0 : 1
                }
            );
            return menus;
        }
        public void OnLoadGlobal(Settings settings) => settings_ = settings;
        public Settings OnSaveGlobal() => settings_;
    }
}