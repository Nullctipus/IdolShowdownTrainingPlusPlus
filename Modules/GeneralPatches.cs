using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

using IdolShowdown;
using IdolShowdown.Managers;
using IdolShowdown.UI.CommandList;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class GeneralPatches : IDisposable, IHarmony
{
    public void Dispose()
    {
    }
    public GeneralPatches(){

        KeybindHelper.RegisterKeybind("Toggle boxes",0,(down)=>{
            DrawingBoxes ^= down;
        });
    }
    public void Patch(Harmony harmony){
        System.Type thisType = GetType();
        //public void SetGameMode(GameMode mode)
        Plugin.TryPatch(typeof(GameManager).GetMethod("SetGameMode"), null,
            new HarmonyMethod(thisType.GetMethod((nameof(SetGameModePostfix)),BindingFlags.NonPublic|BindingFlags.Static)));

        Plugin.TryPatch(typeof(BoxDrawer).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
            new HarmonyMethod(thisType.GetMethod((nameof(OnBoxDrawerAwake)),BindingFlags.NonPublic|BindingFlags.Static)));
    }


    #region GeneralPatches
    private static void SetGameModePostfix(GameMode mode)
    {
        Plugin.GetModule<FrameWalk>().Enabled = false;
        Plugin.IsTraining = mode == GameMode.training;
    }
    static Sprite flat = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one/2);
    internal static bool drawingBoxes = true;
    internal static bool DrawingBoxes{
        get{
            return drawingBoxes;
        }
        set{
            if(drawingBoxes == value) return;

            drawingBoxes = value;
            boxes.RemoveAll(x=>x==null);
            foreach(var v in boxes){
                if(!v.enabled && v.TryGetComponent<BoxDrawer>(out BoxDrawer bd))
                    OnBoxDrawerAwake(bd);
                v.enabled = drawingBoxes && Plugin.ShouldDrawGizmos;
            }
        }
    }
    internal static List<SpriteRenderer> boxes = new();
    internal static bool OnBoxDrawerAwake(BoxDrawer __instance)
    {
        boxes.Add(__instance.GetComponent<SpriteRenderer>());
        if (!drawingBoxes || !Plugin.ShouldDrawGizmos) return true;
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
    
    private static bool OnReadInputOnline(IdolShowdown.Match.IdolMatch __instance,ulong[] ___lastInputs){
        if(!Plugin.IsTraining || !ReplayEditor.Enabled) return true;
        ___lastInputs[ReplayEditor.PlayRight ? 1 : 0] = ReplayEditor.CurrentInput;
		___lastInputs[ReplayEditor.PlayRight ? 0 : 1] = __instance.charPlayerInput[ReplayEditor.PlayRight ? __instance.player1CharacterIndex : __instance.player2CharacterIndex].ReadInput();
		__instance.charPlayerInput[__instance.player1CharacterIndex].ParseInput(___lastInputs[0]);
		if (GlobalManager.Instance.GameManager.GetGameMode() != GameMode.tutorial)
		{
			__instance.charPlayerInput[__instance.player2CharacterIndex].ParseInput(___lastInputs[1]);
		}
        return false;
    }
    #endregion
}