using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace IdolShowdownTrainingPlusPlus;

[BepInPlugin(Plugin.GUID, Plugin.NAME, "1.0.0")]
public class EntryPoint : BaseUnityPlugin // Gets destroyed after started
{
    private void Awake()
    {
        GameObject gameobject = new GameObject("Training++");
        DontDestroyOnLoad(gameobject);
        gameobject.hideFlags = HideFlags.HideAndDontSave;
        gameobject.AddComponent<Plugin>();
    }
}


public class Plugin : MonoBehaviour
{
    public const string GUID = "org.apotheosis.trainingplusplus";
    public const string NAME = "Training++";
    public static Plugin Instance;
    private void Awake()
    {
        Instance = this;
        //Find All Modules
        // +< is for enumerator types
        IEnumerable<Type> LoadableTypes = typeof(Plugin).Assembly.GetTypes().Where(x => !x.IsAbstract && typeof(IDisposable).IsAssignableFrom(x) && x.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) != null);

        
        foreach (var t in LoadableTypes){
            try{
            Disposables.Add(t, (IDisposable)Activator.CreateInstance(t));
            }
            catch(Exception e){
            Logging.LogError(e);
        }
        }
        


        harmony = new(GUID);


        Plugin.Logging.LogInfo("Loading Harmony Patches");
        foreach(var m in Plugin.Disposables){
            if(m.Value is IHarmony HarmonyModule)
                HarmonyModule.Patch(harmony);

        }
        Plugin.Logging.LogInfo("Loaded Harmony Patches");

    }
    private void OnDisable() {
        enabled =true;
    }
    internal static BepInEx.Logging.ManualLogSource Logging => BepInEx.Logging.Logger.CreateLogSource(NAME);
    internal static bool IsTraining;
    public static bool ShouldDrawGizmos => IsTraining;
    internal static bool UseFlatTexture = true;
    internal static Dictionary<Type, IDisposable> Disposables = new();

    internal static KeyCode ToggleFrameWalk = KeyCode.F9;
    internal static KeyCode ToggleMenu = KeyCode.F8;
    internal static KeyCode StepFrame = KeyCode.F10;


    public static event Action drawGUI;
    public static event Action OnUpdate;
    private void OnGUI()
    {
        drawGUI();
    }
    private void Update()
    {
        OnUpdate();
    }
    internal static T GetModule<T>() where T : IDisposable
    {
        return (T)(object)Disposables[typeof(T)];
    }
    internal static Harmony harmony;
    public static bool TryPatch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
    {
        try
        {
            harmony.Patch(original, prefix, postfix, transpiler, finalizer, ilmanipulator);
        }
        catch (Exception e)
        {
            Plugin.Logging.LogError($"Problem patching {original.Name}:\n{e}");
            return false;
        }
        return true;
    }
}