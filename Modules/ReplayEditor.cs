// I could properly edit the demo files directly, but thats above my skill level; so, I am writing my own system and player that can persist updates
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class ReplayEditor : IDisposable
{
    public static ulong GetInputMask(string line)
    {
        ulong num = 0uL;
        line = line.ToLower();
        foreach (char cc in line)
        {
            char c = cc;
            if(MirrorInput){
                switch(c){
                    case '1':
                        c = '3';
                        break;
                    case '3':
                        c = '1';
                        break;
                    case '4':
                        c = '6';
                        break;
                    case '6':
                        c = '4';
                        break;
                    case '7':
                        c = '9';
                        break;
                    case '9':
                        c = '7';
                        break;
                }
            }
            switch (c)
            {
                case '1':
                    num ^= 36;
                    break;
                case '2':
                    num ^= 32;
                    break;
                case '3':
                    num ^= 40;
                    break;
                case '4':
                    num ^= 4;
                    break;
                case '6':
                    num ^= 8;
                    break;
                case '7':
                    num ^= 20;
                    break;
                case '8':
                    num ^= 16;
                    break;
                case '9':
                    num ^= 24;
                    break;
                case 'l':
                    num ^= 64;
                    break;
                case 'm':
                    num ^= 128;
                    break;
                case 'h':
                    num ^= 256;
                    break;
                case 's':
                    num ^= 512;
                    break;
            }
        }
        // These are the macro keys, so we dont need them
        /*if ((num & 192) == 192 && (num & 256) == 0) // if LM
            num |= 2048;
        if ((num & 4) == 4) // if 4
            num |= 1024; // TODO: Make work on both sides
        if ((num & 320) == 320 && (num & 128) == 0) // if LH
            num |= 4096;
        if ((num & 448) == 448) // if LMH
            num |= 8192;*/

        // I wonder what the first two bits are for?

        // Got the data from:
        //IdolShowdown.ISInput.GameplayInput.ReadInput(bool includeFacingLeft = true)
        /*if (PlayerInputTranslated[0].Pressed) // left
        {
            num |= 4;
        }
        if (PlayerInputTranslated[1].Pressed) // right
        {
            num |= 8;
        }
        if (PlayerInputTranslated[2].Pressed) // up
        {
            num |= 16;
        }
        if (PlayerInputTranslated[3].Pressed) // down
        {
            num |= 32;
        }
        if (PlayerInputTranslated[4].Pressed) // light
        {
            num |= 64;
        }
        if (PlayerInputTranslated[5].Pressed) // medium
        {
            num |= 128;
        }
        if (PlayerInputTranslated[6].Pressed) // heavy
        {
            num |= 256;
        }
        if (PlayerInputTranslated[7].Pressed) // special
        {
            num |= 512;
        }
        if (PlayerInputTranslated[8].Pressed) // guard
        {
            num |= 1024;
        }
        if (PlayerInputTranslated[9].Pressed) // grab
        {
            num |= 2048;
        }
        if (PlayerInputTranslated[10].Pressed) // collab
        {
            num |= 4096;
        }
        if (PlayerInputTranslated[11].Pressed) // burst
        {
            num |= 8192;
        }
        if (PlayerInputTranslated[12].Pressed) // item
        {
            num |= 16382;
        }*/
        return num;
    }

    public static IEnumerable<ulong> GetInputs(string data)
    {
        try
        {
            List<ulong> ret = new();
            var lines = data.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] split = lines[i].Split(',');
                if(split.Length <2){
                    if(string.IsNullOrEmpty(lines[i]))
                        continue;
                    Log($"Error on line {i+1}");
                    return null;
                }
                ulong input = 0;
                try
                {
                    input = GetInputMask(split[0]);
                }
                catch
                {
                    Log($"Error on line {i+1}");
                    return null;
                }
                if(int.TryParse(split[1].Trim(),out int frames))
                    for (int j = 0; j < frames; j++)
                        ret.Add(input);
                
            }
            return ret;
        }
        catch(Exception e)
        {
            Plugin.Logging.LogError(e);
            return null;
        }
    }
    public static void StartPlayback(IEnumerable<ulong> inputs)
    {
        if (inputs == null) return;
        Inputs = inputs.GetEnumerator();
        Enabled = true;
    }
    static IEnumerator<ulong> Inputs;
    internal static bool Loop;
    internal static ulong CurrentInput // Reading Moves to next input
    {
        get
        {
            var val = Inputs.Current;
            Log(val.ToString());
            if (!Inputs.MoveNext()){
                if(Loop){
                    Inputs.Reset();
                }
                else{
                Enabled = false;
                Inputs.Dispose(); // Clean up after use
                }
            }
            return val;
        }
    }
    internal static bool Enabled;
    static bool _Editing = false;
    internal static bool PlayRight;
    internal static bool MirrorInput;
    internal static bool Editing{
        get{
            return _Editing;
        }
        set{
            if(_Editing != value)
                windowRect = new(350, 50, Screen.width-450, Screen.height-100);

            _Editing = value;
        }
    }

    public ReplayEditor()
    {
        Plugin.Logging.LogInfo("Loading Replay Editor");
        Plugin.drawGUI += OnGUI;
        Directory.CreateDirectory(PATH);
        LoadFiles();
    }
    int windowid = (PluginInfo.PLUGIN_GUID + nameof(ReplayEditor)).GetHashCode();
    static Rect windowRect = new(350, 50, Screen.width-450, Screen.height-100);
    private void OnGUI()
    {
        if (Editing)
            windowRect = GUI.Window(windowid, windowRect, DrawWindow, "Replay Editor");
    }
    string data = "";
    int dataLines = 1;
    string FileName;
    void LoadFiles()
    {
        files.Clear();
        DirectoryInfo dir = new(PATH);
        files.AddRange(dir.GetFiles().Select(x=>x.Name));
    }
    void LoadFile(string name)
    {
        FileName = name;
        data = File.ReadAllText(Path.Combine(PATH, name));
        dataLines = data.Split('\n').Length;
    }
    static string PATH = Path.Combine(Application.streamingAssetsPath, "../..", "Replays");
    Vector2 TextEditorScroll = Vector2.zero;
    Vector2 FileBrowserScroll = Vector2.zero;
    static Vector2 ConsoleScroll = Vector2.zero;
    static string Console;
    static int ConsoleLines = 1;
    List<string> files = new();
    static void Log(string text){
            Console += text+"\n";
            ++ConsoleLines;
            ConsoleScroll[1] = 1f;// Scroll to bottom
    }
    void DrawWindow(int idx)
    {
        GUILayout.BeginHorizontal();
        FileName = GUILayout.TextField(FileName);
        if (GUILayout.Button("Save") && !String.IsNullOrEmpty(FileName))
        {
            File.WriteAllText(Path.Combine(PATH,FileName),data);
            if(!files.Contains(FileName))
                files.Add(FileName);
            Log("Saved to "+ FileName);
        }
        if (GUILayout.Button("Refresh Files"))
        {
            LoadFiles();
            Log("Refreshed Files");
        }
        if (GUILayout.Button("Open Folder"))
        {
            System.Diagnostics.Process.Start(PATH);
        }

        if (GUILayout.Button("Run Left"))
        {
            PlayRight = false;
            StartPlayback(GetInputs(data));
        }

        if (GUILayout.Button("Run Right"))
        {
            PlayRight = true;
            StartPlayback(GetInputs(data));
        }
        GUILayout.EndHorizontal();
        GUI.Box(new Rect(10, 60, windowRect.width/10f, windowRect.height - 420), "File Browser");
        FileBrowserScroll = GUI.BeginScrollView(new Rect(10, 60, windowRect.width/10f, windowRect.height - 420), FileBrowserScroll, new Rect(0, 0, windowRect.width/10f-20, files.Count * 25), false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            
            for(int i = 0; i< files.Count;i++){
                if(GUI.Button(new Rect(5,25+25*i,windowRect.width/10f-10,20),files[i]))
                    LoadFile(files[i]);
            }
        GUI.EndScrollView();
        GUI.Box(new Rect(10, windowRect.height - 350, windowRect.width - 20, 340), "Console");
        ConsoleScroll = GUI.BeginScrollView(new Rect(10, windowRect.height - 350, windowRect.width - 20, 340), ConsoleScroll, new Rect(0, 0, windowRect.width - 40, (ConsoleLines+1) * 15), false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            
            GUI.Label(new Rect(5, 25, windowRect.width - 30, ConsoleLines * 15),Console);
        GUI.EndScrollView();
        GUI.Box(new Rect(windowRect.width/10f+20, 60, windowRect.width*0.9f-30, windowRect.height - 420), "Script Editor");
        TextEditorScroll = GUI.BeginScrollView(new Rect(windowRect.width/10f+20, 60, windowRect.width*0.9f-30, windowRect.height - 420), TextEditorScroll, new Rect(0, 0, windowRect.width*0.9f-40, (dataLines+1) * 15), false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            Loop = GUI.Toggle(new Rect(windowRect.width*0.9f-140,5,110,20),Loop,"Loop");
            MirrorInput = GUI.Toggle(new Rect(windowRect.width*0.9f-250,5,110,20),MirrorInput,"Mirror");
            GUI.changed = false;
            data = GUI.TextArea(new Rect(5, 25, windowRect.width*0.9f-40, (dataLines+1) * 15),data);
            if(GUI.changed){
                dataLines = data.Split('\n').Length;
            }
        GUI.EndScrollView();

        GUI.DragWindow();

    }
    public void Dispose()
    {
        Enabled = false;
        files = null;
        Console = null;
        data = string.Empty;
        if(Inputs != null)
            Inputs.Dispose();
    }
}