using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using IdolShowdown;
using IdolShowdown.UI.CommandList;
using TMPro;

using Type=System.Type;
using HarmonyLib;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class CommandListAdditions : IDisposable, IHarmony
{
    static CommandListAdditions instance;
    #region ExposeFields
    public void SetupFieldInfo()
    {
        Type CommandListGUIType = typeof(CommandListGUI);
        _previewAnimator = CommandListGUIType.GetField(nameof(previewAnimator),BindingFlags.Instance|BindingFlags.NonPublic);
        _currentListOfMoves = CommandListGUIType.GetField(nameof(currentListOfMoves),BindingFlags.Instance|BindingFlags.NonPublic);
        _moves = CommandListGUIType.GetField(nameof(moves),BindingFlags.Instance|BindingFlags.NonPublic);

        Type type = typeof(CommandListMove);
        _moveTags = type.GetField("moveTags",BindingFlags.Instance|BindingFlags.NonPublic);
        _currentMove = type.GetField("currentMove",BindingFlags.Instance|BindingFlags.NonPublic);
    }
    //private Animator previewAnimator;
    FieldInfo _previewAnimator;
    Animator previewAnimator{
        get{
            return (Animator)_previewAnimator.GetValue(GUIInstance);
        }
        set{
            _previewAnimator.SetValue(GUIInstance,value);
        }
    }
    // private MoveList currentListOfMoves;
    FieldInfo _currentListOfMoves;
    MoveList currentListOfMoves{
        get{
            return (MoveList)_currentListOfMoves.GetValue(GUIInstance);
        }
        set{
            _currentListOfMoves.SetValue(GUIInstance,value);
        }
    }
    //private Dictionary<string, CommandListMove> moves;
    FieldInfo _moves;
    Dictionary<string, CommandListMove> moves {
        get{
            return (Dictionary<string, CommandListMove>)_moves.GetValue(GUIInstance);
        }
        set{
            _moves.SetValue(GUIInstance,value);
        }
    }
    //private TextMeshProUGUI moveTags;
    FieldInfo _moveTags;
    List<TextMeshProUGUI> tags = new();
    internal TextMeshProUGUI GetTag(CommandListMove instance){
        var tag = (TextMeshProUGUI)_moveTags.GetValue(instance);;
        if(!tags.Contains(tag)) tags.Add(tag);
        return tag;
    }
    //private InputMove currentMove;
    FieldInfo _currentMove;
    internal InputMove currentmove(CommandListMove instance){
        return (InputMove)_currentMove.GetValue(instance);
    }

    
    #endregion
    static CommandListGUI GUIInstance;
    public CommandListAdditions()
    {
        Plugin.Logging.LogInfo("Loading CommandListAdditions");
        SetupFieldInfo();
        instance = this;
    }
    internal void OnUpdateAnimator(){
        if(previewAnimator == null) return;

        if(!frametimes.ContainsKey(previewAnimator)) frametimes.Add(previewAnimator,new());
        //Animator dupeAnimator = GameObject.Instantiate(previewAnimator.gameObject,Vector3.zero,Quaternion.identity).GetComponent<Animator>();
        // Update Text To Match
        string lastStateName = previewAnimator.GetCurrentStateName(0);
        foreach(var v in moves){
            try{
            CommandListMove currentMove = v.Value;
            TextMeshProUGUI t = GetTag(currentMove);
            InputMove inputmove = currentmove(currentMove);
            t.text = t.text.Split('\n')[0];
            t.text +=$"\n<color=#ff0080>{GetFrameTime(inputmove.AnimationName)}\nCancel Priority: {inputmove.CancelPriority()}</color>";
            }
            catch (Exception e){
                Plugin.Logging.LogError(e);
            }
        }
        previewAnimator.Play(lastStateName,0);
        //GameObject.Destroy(dupeAnimator.gameObject);
    }
    Dictionary<Animator,Dictionary<string,string>> frametimes = new();
    internal string GetFrameTime(string name){
        try{
        if(!frametimes[previewAnimator].ContainsKey(name)){
            previewAnimator.Play(name,0,0.5f);
            previewAnimator.EvaluateController(.01f);

            AnimationClip clip = previewAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
            frametimes[previewAnimator].Add(name,$"{Mathf.CeilToInt(clip.length*clip.frameRate)} Frames");
            
        }
        return frametimes[previewAnimator][name];
        }
        catch(Exception e){
            Plugin.Logging.LogError(e);
        }
        return "";
    }
    internal static void Initialize(CommandListGUI commandList)
    {
        Plugin.Logging.LogInfo("Command List Initialized");
        GUIInstance = commandList;
    }
    
    public void Dispose()
    {
        Plugin.Logging.LogInfo("Unloading CommandListAdditions");
        foreach(var t in tags)
            t.text = t.text.Split('\n')[0];
        frametimes = new();
        
    }


    public void Patch(Harmony harmony)
    {
        System.Type thisType = GetType();
        //public void InitializeGUI(MoveList listOfMoves)
        Plugin.TryPatch(typeof(CommandListGUI).GetMethod("InitializeGUI"), null,
            new HarmonyMethod(thisType.GetMethod(nameof(OnCommandListInitialized),BindingFlags.NonPublic|BindingFlags.Static)));

        //public void UpdateSpriteAnimator()
        Plugin.TryPatch(typeof(CommandListGUI).GetMethod("UpdateSpriteAnimator"), null,
            new HarmonyMethod(thisType.GetMethod(nameof(OnCommandListUpdateSpriteAnimator),BindingFlags.NonPublic|BindingFlags.Static)));
    }
    private static void OnCommandListInitialized(CommandListGUI __instance, MoveList listOfMoves) { Initialize(__instance); }
    private static void OnCommandListUpdateSpriteAnimator() { instance.OnUpdateAnimator(); }
}