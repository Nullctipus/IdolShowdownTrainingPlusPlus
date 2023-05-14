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

        KeybindHelper.RegisterKeybind("Toggle Menu",KeyCode.F8,(down)=>{
            ShouldDraw ^= down; // toggle if true
        });
    }
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
    Rect windowRect = new(10, 10, 300, 800);
    static bool ShouldDraw = false;
    public void OnGUI()
    {
        if (ShouldDraw)
            windowRect = GUI.Window(windowid, windowRect, DrawWindow, "Training++");
    }
    static bool RenderDropdown = false;
    static bool FrameWalkDropdown = false;
    static bool KeybindsDropdown = false;
    static FrameWalk frameWalk;
    static bool ChangingFrameToggle;
    static bool ChangingFrameStep;
    static bool ChangingMenuToggle;
    static Dictionary<string,bool> ChangingKey;
    static string SelectedKey;
    void DrawWindow(int id)
    {
        Plugin.UseFlatTexture = GUILayout.Toggle(Plugin.UseFlatTexture, "Flat Box Texture");
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
        if (GUILayout.Button("Toggle Keybind Options"))
            KeybindsDropdown ^= true;
        if (KeybindsDropdown) //TODO: Should Prolly make this more extensive
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