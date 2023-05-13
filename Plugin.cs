using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace IdolShowdownTrainingPlusPlus;

[BepInPlugin("org.apotheosis.trainingplusplus", "Training++", "1.0.0")]
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
    public static Plugin Instance;
    private void Awake()
    {
        Instance = this;
        //Find All Modules
        IEnumerable<Type> LoadableTypes = typeof(Plugin).Assembly.GetTypes().Where(x => !x.IsAbstract && typeof(IDisposable).IsAssignableFrom(x));

        foreach (var t in LoadableTypes)
            Disposables.Add(t, (IDisposable)Activator.CreateInstance(t));


    }
    private void OnDisable() {
        enabled =true;
    }
    internal static BepInEx.Logging.ManualLogSource Logging => BepInEx.Logging.Logger.CreateLogSource("Training++");
    internal static bool IsTraining;
    public static bool ShouldDrawGizmos => IsTraining;
    internal static bool UseFlatTexture = true;
    internal static Dictionary<Type, IDisposable> Disposables = new();
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
}