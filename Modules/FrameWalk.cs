using System.Collections;
using System;
using UnityEngine;
using IdolShowdown.Managers;

namespace IdolShowdownTrainingPlusPlus.Modules;

internal class FrameWalk : IDisposable
{
    public FrameWalk()
    {
        Plugin.OnUpdate += OnUpdate;
    }
    private void OnUpdate() {
        if(Input.GetKeyDown(Plugin.ToggleFrameWalk))
            Enabled ^=true;
        if(Enabled && Input.GetKeyDown(Plugin.StepFrame))
            Step();
    }
    bool _enabled;
    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            if (_enabled == value || !Plugin.IsTraining) return;
            GlobalManager.Instance.GameManager.OverridePauseValue(value);
            _enabled = value;
        }
    }
    public void Step()
    {
        if(Plugin.IsTraining)
            Plugin.Instance.StartCoroutine(_Step());
    }
    IEnumerator _Step()
    {
        yield return new WaitForEndOfFrame();
        GlobalManager.Instance.GameManager.OverridePauseValue(false);
        yield return new WaitForEndOfFrame();
        GlobalManager.Instance.GameManager.OverridePauseValue(true);
    }
    public void Dispose()
    {
        Plugin.OnUpdate -= OnUpdate;
    }
}