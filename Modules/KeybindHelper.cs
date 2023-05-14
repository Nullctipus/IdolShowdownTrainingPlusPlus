using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
namespace IdolShowdownTrainingPlusPlus;

public class KeybindHelper : IDisposable
{
    
    public static Dictionary<string,KeyCode> Keys;
    public static Dictionary<string,Action<bool>> Actions = new();

    public KeybindHelper(){
        Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "../..", "TrainingPlusPlus"));
        Plugin.OnUpdate += OnUpdate;
    }
    static public void RegisterKeybind(string name, KeyCode defaultKey, Action<bool> action){
        if(Keys == null) LoadKeys();

        if(!Keys.ContainsKey(name))
            Keys.Add(name,defaultKey);
        if(!Actions.ContainsKey(name))
            Actions.Add(name,action);

        SaveKeys();
    }
    static public void SetBind(string name,KeyCode key){
        if(!Keys.ContainsKey(name))
            Keys.Add(name,key);
        else
            Keys[name] = key;
    }
    static string PATH = Path.Combine(Application.streamingAssetsPath, "../..", "TrainingPlusPlus","Keybinds.json");
    static void LoadKeys(){
        Keys = new();
        try{
            var keys = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(PATH)); // Will just be a number otherwise
            foreach(var k in keys)
                Keys.Add(k.Key,(KeyCode)Enum.Parse(typeof(KeyCode),k.Value));
            
        }
        catch{
            File.WriteAllText(PATH,JsonConvert.SerializeObject(new Dictionary<string,string>(),Formatting.Indented));
        }
    }
    static void SaveKeys(){
        Dictionary<string,string> keys = new();
        foreach(var k in Keys)
            keys.Add(k.Key,k.Value.ToString());
        
        File.WriteAllText(PATH,JsonConvert.SerializeObject(keys,Formatting.Indented));
    }

    private void OnUpdate() {
        if(Keys == null) return;
        foreach(var k in Keys)
            {
                if(k.Value == 0) continue; // if none
                
                if(Input.GetKeyDown(k.Value))
                    Actions[k.Key].Invoke(true);
                if(Input.GetKeyUp(k.Value))
                    Actions[k.Key].Invoke(false);
            }
    }
    public void Dispose()
    {
        
    }
}