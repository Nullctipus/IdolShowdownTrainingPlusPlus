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
        if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
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
    void DrawWindow(int id)
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
        GUI.DragWindow();
    }
    public void Dispose()
    {
        Plugin.drawGUI -= OnGUI;
        Plugin.OnUpdate -= OnUpdate;
    }
}