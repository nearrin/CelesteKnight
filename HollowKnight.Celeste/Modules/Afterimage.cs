namespace Celeste
{
    public class Afterimage : Module
    {
        public class Animation : MonoBehaviour
        {
            private float time;
            public Generator generator;
            public tk2dSpriteAnimationClip clip;
            void Update()
            {
                time += Time.deltaTime;
                if (time > 0.5)
                {
                    time = 0;
                    gameObject.SetActive(false);
                    generator.pool.inactive.Add(gameObject);
                }
                var newA = gameObject.GetAddComponent<tk2dSpriteAnimator>();
                newA.Play(clip);
                var c = generator.color;
                newA.Sprite.color = new Color(c.r, c.g, c.b, c.a * (1 - time / 0.5f));
            }
        }
        public class Pool
        {
            public GameObject prefab;
            public List<GameObject> inactive = new List<GameObject>();
            public GameObject instantiate(Vector3 positon, Quaternion rotation, Vector3 scale)
            {
                GameObject newG;
                if (inactive.Count != 0)
                {
                    newG = inactive[0];
                    newG.SetActive(true);
                    inactive.RemoveAt(0);
                }
                else
                {
                    newG = UnityEngine.Object.Instantiate(prefab);
                    newG.RemoveComponent<AudioSource>();
                    newG.RemoveComponent<BoxCollider2D>();
                    newG.RemoveComponent<ConstrainPosition>();
                    newG.RemoveComponent<DamageHero>();
                    newG.RemoveComponent<EnemyDeathEffectsUninfected>();
                    newG.RemoveComponent<EnemyDreamnailReaction>();
                    newG.RemoveComponent<EnemyHitEffectsUninfected>();
                    newG.RemoveComponent<ExtraDamageable>();
                    newG.RemoveComponent<HealthManager>();
                    newG.RemoveComponent<MeshRenderer>();
                    newG.RemoveComponent<MeshFilter>();
                    newG.RemoveComponent<PersistentBoolItem>();
                    newG.RemoveComponent<PersonalObjectPool>();
                    newG.RemoveComponent<PlayMakerFixedUpdate>();
                    newG.RemoveComponent<PlayMakerFSM>();
                    newG.RemoveComponent<PlayMakerFSM>();
                    newG.RemoveComponent<Recoil>();
                    newG.RemoveComponent<Rigidbody2D>();
                    newG.RemoveComponent<SpriteFlash>();
                    newG.tag = "Untagged";
                }
                newG.transform.position = new Vector3(positon.x, positon.y, positon.z + 1e-3f);
                newG.transform.rotation = rotation;
                newG.transform.localScale = scale;
                return newG;
            }
        }
        public class Generator : MonoBehaviour
        {
            private float time;
            public Pool pool = new();
            public Color color = new Color(0.28f, 0.72f, 0.84f, 0.75f);
            void Update()
            {
                time += Time.deltaTime;
                if (HeroController.instance.cState.dashing && time > 0.1)
                {
                    time = 0;
                    var originalG = HeroController.instance.gameObject;
                    var newG = pool.instantiate(originalG.transform.position, originalG.transform.rotation, originalG.transform.localScale);
                    DontDestroyOnLoad(newG);
                    var originalA = originalG.GetComponent<tk2dSpriteAnimator>();
                    var newA = newG.GetComponent<tk2dSpriteAnimator>();
                    newA.SetSprite(originalA.Sprite.Collection, originalA.Sprite.spriteId);
                    newA.Library = originalA.Library;
                    var originalC = originalA.CurrentClip;
                    var newC = new tk2dSpriteAnimationClip();
                    newC.CopyFrom(originalC);
                    newC.frames = new tk2dSpriteAnimationFrame[1];
                    newC.frames[0] = originalC.frames[originalA.CurrentFrame];
                    newC.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                    var i = newG.GetAddComponent<Animation>();
                    i.generator = this;
                    i.clip = newC;
                }
            }
        }
        GameObject prefab;
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                 ("GG_Mighty_Zote","Battle Control"),
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
            prefab = battleControl.transform.Find("Zotelings").gameObject.transform.Find("Ordeal Zoteling").gameObject;
        }
        public override void SetActive(bool active)
        {
            if (active)
            {
                ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
            }
            else
            {
                ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
                HeroController.instance?.gameObject.RemoveComponent<Generator>();
            }
        }
        private void ModHooks_HeroUpdateHook()
        {
            HeroController.instance.gameObject.GetAddComponent<Generator>().pool.prefab = prefab;
        }
    }
}