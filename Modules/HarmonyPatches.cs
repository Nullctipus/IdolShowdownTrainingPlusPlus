using System;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

using IdolShowdown;
using IdolShowdown.Managers;
using IdolShowdown.UI.CommandList;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class HarmonyPatches : IDisposable
{
    internal static Harmony harmony;
    #region Helpers
    bool TryPatch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
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
    HarmonyMethod GetPatch(string method)
    {
        return new HarmonyMethod(typeof(HarmonyPatches).GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic));
    }
    #endregion
    public void Dispose()
    {
        Plugin.Logging.LogInfo("Unloading Harmony");
        harmony.UnpatchSelf();
    }
    public HarmonyPatches()
    {
        Plugin.Logging.LogInfo("Loading Harmony Patches");
        harmony = new("org.apotheosis.trainingplusplus");

        //public void SetGameMode(GameMode mode)
        TryPatch(typeof(GameManager).GetMethod("SetGameMode"), null, GetPatch(nameof(SetGameModePostfix)));

        //public void InitializeGUI(MoveList listOfMoves)
        TryPatch(typeof(CommandListGUI).GetMethod("InitializeGUI"), null, GetPatch(nameof(OnCommandListInitialized)));

        //public void UpdateSpriteAnimator()
        TryPatch(typeof(CommandListGUI).GetMethod("UpdateSpriteAnimator"), null, GetPatch(nameof(OnCommandListUpdateSpriteAnimator)));

        TryPatch(typeof(BoxDrawer).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance), GetPatch(nameof(OnBoxDrawerAwake)));
        
        Plugin.Logging.LogInfo("Loaded Harmony Patches");
    }


    #region GeneralPatches
    private static void SetGameModePostfix(GameMode mode)
    {
        Plugin.GetModule<FrameWalk>().Enabled = false;
        Plugin.IsTraining = mode == GameMode.training;
    }
    private static void OnCommandListInitialized(CommandListGUI __instance, MoveList listOfMoves) { Plugin.GetModule<CommandListAdditions>().Initialize(__instance); }
    private static void OnCommandListUpdateSpriteAnimator() { Plugin.GetModule<CommandListAdditions>().OnUpdateAnimator(); }
    static Sprite flat = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one/2);
    internal static bool OnBoxDrawerAwake(BoxDrawer __instance)
    {
        if (!Plugin.ShouldDrawGizmos) return true;
        try{
        GameObject g = __instance.gameObject;
        GameObject.Destroy(__instance);
        var sr = g.GetComponent<SpriteRenderer>();
        if(Plugin.UseFlatTexture)
            sr.sprite = flat;
        var bc = g.GetComponentInParent<BoxCollider2D>();
        sr.size = bc != null ? bc.size : Vector2.zero;
        sr.enabled = true;
        }
        catch(Exception e){
            Plugin.Logging.LogError(e);
        }
        return false;
    }
    #endregion
}