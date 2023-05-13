using System;
using UnityEngine;
namespace IdolShowdownTrainingPlusPlus.Modules;
internal class TrainingGUI : IDisposable
{
    public TrainingGUI()
    {
        Plugin.Logging.LogInfo("Loading GUI");
        Plugin.drawGUI += OnGUI;
        Plugin.OnUpdate += OnUpdate;
    }
    public void OnUpdate()
    {
        if (UnityEngine.Input.GetKeyDown(Plugin.ToggleMenu))
            ShouldDraw ^= true; // toggle


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

    int windowid = PluginInfo.PLUGIN_GUID.GetHashCode();
    Rect windowRect = new(10, 10, 300, 800);
    static bool ShouldDraw = false;
    public void OnGUI()
    {
        if (ShouldDraw)
            GUI.Window(windowid, windowRect, DrawWindow, "Training++");
    }
    static bool RenderDropdown = false;
    static bool FrameWalkDropdown = false;
    static bool KeybindsDropdown = false;
    static FrameWalk frameWalk;
    static bool ChangingFrameToggle;
    static bool ChangingFrameStep;
    static bool ChangingMenuToggle;

    void DrawWindow(int id)
    {
        Plugin.UseFlatTexture = GUILayout.Toggle(Plugin.UseFlatTexture, "Flat Box Texture");
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
        if (KeybindsDropdown)
        {
            if (GUILayout.Button(ChangingMenuToggle ? "Press any key" : "Change Menu Toggle Key: "+ Plugin.ToggleMenu))
            {
                ChangingMenuToggle = true;
            }
            if (GUILayout.Button(ChangingFrameToggle ? "Press any key" : "Change Frame Walk Toggle Key: "+Plugin.ToggleFrameWalk))
            {
                ChangingFrameToggle = true;
            }
            if (GUILayout.Button(ChangingFrameStep ? "Press any key" : "Change Frame Step Key: "+Plugin.StepFrame))
            {
                ChangingFrameStep = true;
            }
            if (ChangingMenuToggle && Input.anyKey)
            {
                Plugin.ToggleMenu = FetchKey();
                ChangingMenuToggle = false;
            }
            if (ChangingFrameToggle && Input.anyKey)
            {
                Plugin.ToggleFrameWalk = FetchKey();
                ChangingFrameToggle = false;
            }
            if (ChangingFrameStep && Input.anyKey)
            {
                Plugin.StepFrame = FetchKey();
                ChangingFrameStep = false;
            }
        }

        GUI.DragWindow();
    }
    public void Dispose()
    {
        Plugin.drawGUI -= OnGUI;
        Plugin.OnUpdate -= OnUpdate;
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