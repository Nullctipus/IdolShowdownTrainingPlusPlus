using System.Reflection;
// I could properly edit the demo files directly, but thats above my skill level; so, I am writing my own system and player that can persist updates
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class ReplayEditor : IDisposable, IHarmony
{
    #region Converter
    public static string FromInputMask(ulong input){
        string ret = "";
        if((input & 4) == 4)
            ret+="4";
        if((input & 32) == 32)
            ret+="2";
        if((input & 8) == 8)
            ret+="6";
        if((input & 16) == 16)
            ret+="8";
        if((input & 64) == 64)
            ret+="l";
        if((input & 128) == 128)
            ret+="m";
        if((input & 256) == 256)
            ret+="h";
        if((input & 512) == 512)
            ret+="s";
        if((input & 2048) == 2048)
            ret+="lm";
        if((input & 4096) == 4096)
            ret+="lh";
        if((input & 8192) == 8192)
            ret+="lmh";
        return ret;
    }
    public static ulong GetInputMask(string line)
    {
        ulong num = 0uL;
        line = line.ToLower();
        foreach (char cc in line)
        {
            char c = cc;
            /*if(MirrorInput){
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
            }*/
            if(MirrorInput)
                num ^= 32768;
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
    #endregion
    
    #region GUI
    static string data = "";
    static bool _Editing = false;
    internal static bool Editing{
        get{
            return _Editing;
        }
        set{
            if(_Editing != value)
                windowRect = new(400, 50, Screen.width-450, Screen.height-100);

            _Editing = value;
        }
    }
    int dataLines = 1;
    string FileName;
    static void LoadFiles()
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
    void UpdateWatcher(){
        if(fileSystemWatcher != null)fileSystemWatcher.Dispose();

        fileSystemWatcher = new(PATH);
        fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
        FileSystemEventHandler onAnything = (object sender, FileSystemEventArgs e)=>{
            if (e.ChangeType == WatcherChangeTypes.Changed && e.FullPath.EndsWith(FileName))
                LoadFile(FileName);
            else
                LoadFiles();
        };
        fileSystemWatcher.Changed+= onAnything;
        fileSystemWatcher.Created+= onAnything;
        fileSystemWatcher.Deleted+= onAnything;
        fileSystemWatcher.Filter = "*";
        fileSystemWatcher.IncludeSubdirectories = false;
        fileSystemWatcher.EnableRaisingEvents =true;

    }

    FileSystemWatcher fileSystemWatcher;
    static string PATH = Path.Combine(Application.streamingAssetsPath, "../..", "Replays");
    Vector2 TextEditorScroll = Vector2.zero;
    Vector2 FileBrowserScroll = Vector2.zero;
    static Vector2 ConsoleScroll = Vector2.zero;
    static string Console;
    static int ConsoleLines = 1;
    static List<string> files = new();
    int windowid = (PluginInfo.PLUGIN_GUID + nameof(ReplayEditor)).GetHashCode();
    static Rect windowRect = new(400, 50, Screen.width-450, Screen.height-100);
    static void Log(string text){
            Console += text+"\n";
            ++ConsoleLines;
    }
    
    private void OnGUI()
    {
        if (Editing)
            windowRect = GUI.Window(windowid, windowRect, DrawWindow, "Replay Editor");
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

        if (GUILayout.Button("Export Demo"))
        {
            ExportDemoToFile("exported");
        }
        if (GUILayout.Button("Run"))
        {
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
            MirrorInput = GUI.Toggle(new Rect(windowRect.width*0.9f-230,5,110,20),MirrorInput,"Mirror");
            PlayAfterBlock = GUI.Toggle(new Rect(windowRect.width*0.9f-340,5,110,20),PlayAfterBlock,"Play After Block");
            PlayRight = GUI.Toggle(new Rect(windowRect.width*0.9f-450,5,110,20),PlayRight,"Play on Right");
            TakeoverDemo = GUI.Toggle(new Rect(windowRect.width*0.9f-560,5,110,20),TakeoverDemo,"Takeover Demo");
            playbackAfterHitstun = GUI.Toggle(new Rect(windowRect.width*0.9f-690,5,130,20),playbackAfterHitstun,"Play After Hitstun");
            GUI.changed = false;
            data = GUI.TextArea(new Rect(5, 25, windowRect.width*0.9f-40, (dataLines+1) * 15),data);
            if(GUI.changed){
                dataLines = data.Split('\n').Length;
            }
        GUI.EndScrollView();

        GUI.DragWindow();

    }
    #endregion
    
    #region Harmony
    public void Patch(HarmonyLib.Harmony harmony){
        //public void ReadInput()
        Plugin.TryPatch(typeof(IdolShowdown.Match.IdolMatch).GetMethod("ReadInput"),
            new HarmonyLib.HarmonyMethod(GetType().GetMethod(nameof(OnReadInput),BindingFlags.Static|BindingFlags.NonPublic)));
        //private void ReduceBlockStun()
        Plugin.TryPatch(typeof(IdolShowdown.FighterAttackable).GetMethod("ReduceBlockStun",BindingFlags.Instance|BindingFlags.NonPublic),
            new HarmonyLib.HarmonyMethod(GetType().GetMethod(nameof(BlockStunOver),BindingFlags.Static|BindingFlags.NonPublic)));
        //PlayDemo(bool flipPlayer2 = false)
        Plugin.TryPatch(typeof(IdolShowdown.DemoRecorder).GetMethod("PlayDemo"),
            new HarmonyLib.HarmonyMethod(GetType().GetMethod(nameof(PlayDemo),BindingFlags.Static|BindingFlags.NonPublic)));
        //public void UpdateMe()
        Plugin.TryPatch(typeof(IdolShowdown.Managers.TrainingManager).GetMethod("UpdateMe"),
            new HarmonyLib.HarmonyMethod(GetType().GetMethod(nameof(TrainingManagerUpdate),BindingFlags.Static|BindingFlags.NonPublic)));
    }
    private static void TrainingManagerUpdate(ref IdolShowdown.FighterAttackable ___player2Attackable, ref bool ___hitstunReceived, ref bool ___allowToReplayInput)
    {
        if (!playbackAfterHitstun || !PlayRight) return;
	    
	    	if (___player2Attackable == null)
	    	{
	    		___player2Attackable = IdolShowdown.Managers.GlobalManager.Instance.GameStateManager.Player2Reference.GetComponent<IdolShowdown.FighterAttackable>();
	    	}
	    	AnimatorStateInfo currentAnimatorStateInfo = ___player2Attackable.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
	    	___hitstunReceived = ___player2Attackable.HitStun > 0m || currentAnimatorStateInfo.IsName("KnockDown") || currentAnimatorStateInfo.IsName("KnockDown_Air");
	    	if (___hitstunReceived)
	    	{
	    			___allowToReplayInput = true;
	    		
	    	}
	    	else if (___allowToReplayInput )
	    	{
                Plugin.Logging.LogInfo("Running");
                //MirrorInput = true;
                StartPlayback(GetInputs(data));
	    		___allowToReplayInput = false;
	    	}
	    
    }
    private static bool PlayDemo(bool flipPlayer2)
    {
        if(!Plugin.IsTraining || !TakeoverDemo) return true;
        MirrorInput = flipPlayer2;
        PlayRight = true;
        StartPlayback(GetInputs(data));

        return false;
    }
    private static void BlockStunOver(IdolShowdown.FighterAttackable __instance, int ___framesOfBlockStunWearingOff)
    {
        if(!Plugin.IsTraining || !PlayAfterBlock || ___framesOfBlockStunWearingOff != 1 || IdolShowdown.Managers.GlobalManager.Instance.MatchRunner.CurrentMatch.frameNumber == 1 || (PlayRight && __instance.name != "player2")) return;
            StartPlayback(GetInputs(data));
    }
    private static bool OnReadInput(IdolShowdown.Match.IdolMatch __instance,ulong[] ___lastInputs){
        if(!Plugin.IsTraining || !ReplayEditor.Enabled) return true;
        ___lastInputs[ReplayEditor.PlayRight ? 1 : 0] = ReplayEditor.CurrentInput;
		___lastInputs[ReplayEditor.PlayRight ? 0 : 1] = __instance.charPlayerInput[ReplayEditor.PlayRight ? __instance.player1CharacterIndex : __instance.player2CharacterIndex].ReadInput();
		__instance.charPlayerInput[__instance.player1CharacterIndex].ParseInput(___lastInputs[0]);
		if (IdolShowdown.Managers.GlobalManager.Instance.GameManager.GetGameMode() != IdolShowdown.Managers.GameMode.tutorial)
		{
			__instance.charPlayerInput[__instance.player2CharacterIndex].ParseInput(___lastInputs[1]);
		}
        return false;
    }
    #endregion
    static FieldInfo demoRecorder;
    internal static ulong CurrentInput // Reading Moves to next input
    {
        get
        {
            var val = Inputs.Current;
            if(IdolShowdown.Managers.GlobalManager.Instance.TrainingManager.WaitForInputToRecordDemo){
                IdolShowdown.Managers.GlobalManager.Instance.TrainingManager.WaitForInputToRecordDemo = false;
                (demoRecorder.GetValue(IdolShowdown.Managers.GlobalManager.Instance.TrainingManager) as IdolShowdown.DemoRecorder).StartRecording();
            }
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
    internal static bool Loop;
    internal static bool PlayRight;
    internal static bool MirrorInput;
    internal static bool PlayAfterBlock;
    internal static bool TakeoverDemo;
    internal static bool playbackAfterHitstun;

    public ReplayEditor()
    {
        Plugin.Logging.LogInfo("Loading Replay Editor");
        Plugin.drawGUI += OnGUI;
        Directory.CreateDirectory(PATH);
        LoadFiles();
        UpdateWatcher();

        KeybindHelper.RegisterKeybind("Replay Left",KeyCode.F5,(down)=>{
            if(down)
            {
                PlayRight = false;
                StartPlayback(GetInputs(data));
            }
        });
        KeybindHelper.RegisterKeybind("Replay Right",KeyCode.F6,(down)=>{
            if(down)
            {
                PlayRight = true;
                StartPlayback(GetInputs(data));
            }
        });
        KeybindHelper.RegisterKeybind("Mirror Replay",KeyCode.F3,(down)=>{
            MirrorInput ^= down;
        });
        KeybindHelper.RegisterKeybind("Replay Loop",0,(down)=>{
            Loop ^=down;
        });

        KeybindHelper.RegisterKeybind("Toggle Replay Window",0,(down)=>{
            Editing ^=down;
        });
        KeybindHelper.RegisterKeybind("Toggle Replay After Block",0,(down)=>{
            PlayAfterBlock ^=down;
        });
        KeybindHelper.RegisterKeybind("Toggle Takeover Demo Block",0,(down)=>{
            TakeoverDemo ^=down;
        });
        KeybindHelper.RegisterKeybind("Toggle Replay after Hitstun",0,(down)=>{
            playbackAfterHitstun ^=down;
        });

        demoRecorder = typeof(IdolShowdown.Managers.TrainingManager).GetField("demoRecorder",BindingFlags.NonPublic|BindingFlags.Instance);
    }
    public static void ExportDemoToFile(string to){
        try{
        IdolShowdown.DemoRecorder recorder = (IdolShowdown.DemoRecorder)demoRecorder.GetValue(IdolShowdown.Managers.GlobalManager.Instance.TrainingManager);
        
        //private frameData[] recordFrame;
        var demoData = (IdolShowdown.DemoRecorder.frameData[]) typeof(IdolShowdown.DemoRecorder).GetField("recordFrame",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(recorder);

        if(demoData == null) return;

        string fileData = "";
        int j = 1;
        for(int i = 1; i< demoData.Length;i++){
            ulong lastinput = demoData[i-1].player2Input;
            ulong input = demoData[i].player2Input;
            if(input == lastinput){
                j++;
            }
            else{
                fileData+=FromInputMask(lastinput)+","+j+"\n";
                j=1;
            }
        }
        File.WriteAllText(Path.Combine(PATH,to), fileData);
        LoadFiles();
        }
        catch(Exception e){
            Plugin.Logging.LogError(e);
        }

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