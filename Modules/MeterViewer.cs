using System;
using HarmonyLib;
using IdolShowdown;
using IdolShowdown.Managers;
using TMPro;
using UnityEngine;
using System.Reflection;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class MeterViewer : IDisposable
{
    GUIStyle style;
    GUIStyle Style{
        get{
            if(style == null){
                style = new(GUI.skin.label);
                style.font =  GlobalManager.Instance.FilesManager.GetCurrentDefaultFont().sourceFontFile;
                style.fontSize = Mathf.CeilToInt(Screen.height*0.02083f); // 30 for 1440p monitor
                style.normal.textColor = Color.blue;
            }
            return style;
        }
    }
    public MeterViewer(){
        Plugin.Logging.LogInfo("Loading Meter Viewer");
        Plugin.drawGUI += OnGUI;
    }
    private void OnGUI() {
        if(!Plugin.IsTraining || !GlobalManager.Instance || GlobalManager.Instance.GameStateManager.Player1ObjectReference == null || GlobalManager.Instance.MatchRunner.CurrentMatch == null || GlobalManager.Instance.GameManager.IsPaused) return;
        //Player 1
        GUI.Label(new Rect(Screen.width*.0977f,Screen.height*.9167f,200,50),$"{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.currentSuperMeter}/{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.superMeterMax}",Style);
        GUI.Label(new Rect(Screen.width*.0977f,Screen.height*.8785f,200,50),$"{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.SCMeter}/{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superChat.MaxSCMeter}",Style);
        GUI.Label(new Rect(Screen.width*.0859f,Screen.height*.1806f,200,50),$"{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.BurstMeter}/{GlobalManager.Instance.GameStateManager.Player1ObjectReference.superMeter.maxBurstMeter}",Style);
        //Player 2
        GUI.Label(new Rect(Screen.width*.8555f,Screen.height*.9167f,200,50),$"{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superMeter.currentSuperMeter}/{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superMeter.superMeterMax}",Style);
        GUI.Label(new Rect(Screen.width*.8633f,Screen.height*.8775f,200,50),$"{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superChat.SCMeter}/{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superChat.MaxSCMeter}",Style);
        GUI.Label(new Rect(Screen.width*.8671f,Screen.height*.1806f,200,50),$"{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superMeter.BurstMeter}/{GlobalManager.Instance.GameStateManager.Player2ObjectReference.superMeter.maxBurstMeter}",Style);
    }
    public void Dispose()
    {
        Plugin.Logging.LogInfo("Unloading Meter Viewer");
        Plugin.drawGUI -= OnGUI;
        style = null;
    }
}