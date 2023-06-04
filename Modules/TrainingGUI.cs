using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace IdolShowdownTrainingPlusPlus.Modules;
internal class TrainingGUI : IDisposable
{
    public TrainingGUI()
    {
        Plugin.Logging.LogInfo("Loading GUI");
        Plugin.drawGUI += OnGUI;
        scMeter = typeof(IdolShowdown.SuperChatHolder).GetField("scMeter",BindingFlags.Instance|BindingFlags.NonPublic);
        KeybindHelper.RegisterKeybind("Toggle Menu",KeyCode.F8,(down)=>{
            ShouldDraw ^= down; // toggle if true
        });
        KeybindHelper.RegisterKeybind("Set Super Meter To Last",0,(down)=>{
            if(!Plugin.IsTraining || !IdolShowdown.Managers.GlobalManager.Instance || IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference == null || IdolShowdown.Managers.GlobalManager.Instance.MatchRunner.CurrentMatch == null || !down) return;
            IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.currentSuperMeter = (Unity.Mathematics.FixedPoint.fp) Single.Parse(currentSuperMeter);
        });
        KeybindHelper.RegisterKeybind("Set Super Chat Meter To Last",0,(down)=>{
            if(!Plugin.IsTraining || !IdolShowdown.Managers.GlobalManager.Instance || IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference == null || IdolShowdown.Managers.GlobalManager.Instance.MatchRunner.CurrentMatch == null || !down) return;
            scMeter.SetValue(IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.SCMeter,int.Parse(SCMeter));
        });
        KeybindHelper.RegisterKeybind("Set Burst Meter To Last",0,(down)=>{
            if(!Plugin.IsTraining || !IdolShowdown.Managers.GlobalManager.Instance || IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference == null || IdolShowdown.Managers.GlobalManager.Instance.MatchRunner.CurrentMatch == null || !down) return;
            IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.BurstMeter = Single.Parse(BurstMeter);
        });
    }
    FieldInfo scMeter;
    IdolShowdown.Structs.Idol[] characters;
    IdolShowdown.Structs.Idol[] Characters
    {
        get
        {
            if (characters == null)
            {
                try
                {
                    characters = IdolShowdown.Managers.GlobalManager.Instance.GameManager.Characters;
                }
                catch { }
            }
            return characters;
        }
    }

    int windowid = (PluginInfo.PLUGIN_GUID+nameof(TrainingGUI)).GetHashCode();
    Rect windowRect = new(10, 10, 350, 800);
    static bool ShouldDraw = false;
    public void OnGUI()
    {
        if (ShouldDraw)
            windowRect = GUI.Window(windowid, windowRect, DrawWindow, "Training++");
    }
    static bool RenderDropdown = false;
    static bool FrameWalkDropdown = false;
    static bool KeybindsDropdown = false;
    static bool ValueDropdown = false;
    static FrameWalk frameWalk;
    static string SelectedKey;
    Vector2 scroll = Vector2.zero;

    string currentSuperMeter = string.Empty;
    string SCMeter = string.Empty;
    string BurstMeter = string.Empty;

    void DrawWindow(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll);
        Plugin.UseFlatTexture = GUILayout.Toggle(Plugin.UseFlatTexture, "Flat Box Texture");
        GeneralPatches.DrawingBoxes = GUILayout.Toggle(GeneralPatches.DrawingBoxes, "Draw Boxes");
        MeterViewer.DrawMeterValues = GUILayout.Toggle(MeterViewer.DrawMeterValues, "Draw Meter Values");
        ReplayEditor.Editing = GUILayout.Toggle(ReplayEditor.Editing , "Open Recording Editor");
        if (GUILayout.Button("Toggle Frame Walk Options"))
            FrameWalkDropdown ^= true;
        if (FrameWalkDropdown)
        {
            if (frameWalk == null) frameWalk = Plugin.GetModule<FrameWalk>();
            frameWalk.Enabled = GUILayout.Toggle(frameWalk.Enabled, "Frame Walk");
            if (frameWalk.Enabled)
            {
                if (GUILayout.Button("Step"))
                    frameWalk.Step();
            }
        }
        if (GUILayout.Button("Toggle Render Options"))
            RenderDropdown ^= true;
        if (RenderDropdown)
        {
            if (GUILayout.Button("Render All Animations With Boxes"))
            {
                Plugin.Instance.StartCoroutine(Plugin.GetModule<RenderAnimations>().RenderAll());
            }
            if (Characters != null)
                foreach (var c in Characters)
                {
                    if (GUILayout.Button("Render " + c.charName))
                    {
                        Plugin.Instance.StartCoroutine(Plugin.GetModule<RenderAnimations>().OnlyRenderCharacter(c));

                    }
                }
        }
        if(Plugin.IsTraining && IdolShowdown.Managers.GlobalManager.Instance && IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference != null && IdolShowdown.Managers.GlobalManager.Instance.MatchRunner.CurrentMatch != null ){
        if (GUILayout.Button("Toggle Value Options"))
            ValueDropdown ^= true;
        if (ValueDropdown){
            if(GUILayout.Button("Refresh")){
                currentSuperMeter = IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.currentSuperMeter.ToString();
                SCMeter = IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.SCMeter.ToString();
                BurstMeter =  IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.BurstMeter.ToString();
            }
            GUILayout.Label("Super Meter");
            GUILayout.BeginHorizontal();
            currentSuperMeter = GUILayout.TextField(currentSuperMeter);// IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.currentSuperMeter
            if(GUILayout.Button("set"))
                IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.currentSuperMeter = (Unity.Mathematics.FixedPoint.fp) Single.Parse(currentSuperMeter);
            GUILayout.EndHorizontal();
            GUILayout.Label("Super Chat Meter");
            GUILayout.BeginHorizontal();
            SCMeter = GUILayout.TextField(SCMeter);//IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.SCMeter
            if(GUILayout.Button("set"))
                scMeter.SetValue(IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.SCMeter,int.Parse(SCMeter));
            GUILayout.EndHorizontal();
            GUILayout.Label("Burst Meter");
            GUILayout.BeginHorizontal();
            BurstMeter = GUILayout.TextField(BurstMeter);//IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.BurstMeter
            if(GUILayout.Button("set"))
                IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.BurstMeter = Single.Parse(BurstMeter);
            GUILayout.EndHorizontal();
        }}
        if (GUILayout.Button("Toggle Keybind Options"))
            KeybindsDropdown ^= true;
        if (KeybindsDropdown)
        {
            foreach(var k in KeybindHelper.Keys){
                if (GUILayout.Button(SelectedKey == k.Key ? "Press any key" : $"Change {k.Key} Key: "+ k.Value))
                {
                    SelectedKey = k.Key;
                }
            }
            if(!string.IsNullOrEmpty(SelectedKey) && Input.anyKey){
                KeybindHelper.SetBind(SelectedKey,FetchKey());
                SelectedKey = string.Empty;
            }
        }
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
    public void Dispose()
    {
        Plugin.drawGUI -= OnGUI;
    }
    KeyCode FetchKey()
    {
        var e = System.Enum.GetNames(typeof(KeyCode)).Length;
        for (int i = 0; i < e; i++)
        {
            if (Input.GetKey((KeyCode)i))
            {
                return (KeyCode)i;
            }
        }

        return KeyCode.None;
    }
}