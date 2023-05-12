using System.Collections;
using System;
using UnityEngine;
using IdolShowdown;
using IdolShowdown.Managers;
using IdolShowdown.Structs;
using System.IO;
namespace IdolShowdownTrainingPlusPlus.Modules;
internal class RenderAnimations : IDisposable
{
    Camera renderCamera;
    RenderTexture rt;
    string BasePath = Path.Combine(Application.streamingAssetsPath, "../..", "IdolRenders");
    CanvasRenderer[] canvases;
    RenderTexture oldrt;
    Texture2D tex;
    const int RENDER_LAYER = 9;
    private void Setup()
    {
        canvases = GameObject.FindObjectsOfType<CanvasRenderer>();
        foreach (var c in canvases)
            c.gameObject.SetActive(false);

        Plugin.IsTraining = true;
        if (renderCamera == null)
        {
            renderCamera = new GameObject("Training++ RenderCamera").AddComponent<Camera>();
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 2;
            renderCamera.transform.position = new Vector3(1024, 1024, -5);
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            renderCamera.cullingMask = 1 << RENDER_LAYER;
            rt = new RenderTexture(2048, 2048, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            renderCamera.targetTexture = rt;
            rt.filterMode = FilterMode.Point;
        }
        oldrt = RenderTexture.active;
        RenderTexture.active = rt;
        Directory.CreateDirectory(BasePath);
        tex = new Texture2D(rt.width, rt.height);
    }
    private void Cleanup()
    {

        GameObject.Destroy(tex);
        foreach (var c in canvases)
            c.gameObject.SetActive(true);

        RenderTexture.active = oldrt;
        //reset
        Plugin.IsTraining = GlobalManager.Instance.GameManager.IsGameMode(GameMode.training);
    }
    private void RenderAnimation(Animator animator, string CharacterName, string AnimationName)
    {
        try
        {
            Plugin.Logging.LogInfo(AnimationName);
            Directory.CreateDirectory(Path.Combine(BasePath, CharacterName, AnimationName));
            animator.Play(AnimationName, 0);
            animator.EvaluateController(.001f);

            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            float frametime = 1 / clip.frameRate;
            int i = 1;

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f)
            {
                renderCamera.Render();
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                File.WriteAllBytes(Path.Combine(BasePath, CharacterName, AnimationName, "frame" + i + ".png"), ImageConversion.EncodeToPNG(tex));
                animator.Update(frametime);
                /*
                Check for Projectile
                Set ScaleableTime.DeltaTime
                Call Projectile.UpdateMe(new(),new())
                Call Projectile.FinalizeVelocity()
                */
                i++;
            }
        }
        catch (Exception e)
        {
            Plugin.Logging.LogError(e);
        }
    }
    private IEnumerator RenderCharacter(Idol Character)
    {
        Plugin.Logging.LogInfo(Character.charName);
        Directory.CreateDirectory(Path.Combine(BasePath, Character.charName));
        // Create character
        GameObject instanced = GameObject.Instantiate(Character.originalPrefab, Vector3.zero, Quaternion.identity);

        yield return null; // Let The character Initialize
        RenderTexture.active = rt; // Make sure we still render to the right place

        instanced.transform.position = new Vector3(1024, 1024, 0);

        // Set layer so it gets rendered
        instanced.layer = RENDER_LAYER;
        foreach (var t in instanced.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = RENDER_LAYER;

        // Get list of moves

        Animator animator = instanced.GetComponent<Animator>();
        MoveQueue moveQueue = instanced.GetComponent<MoveQueue>();
        moveQueue.StartMe(); // Populate
        animator.enabled = false;


        foreach (var move in moveQueue.CurrentMoveList.ListedInputMoves)
        {
            RenderAnimation(animator, Character.charName, move.AnimationName);
        }
        GameObject.Destroy(instanced);
    }
    public IEnumerator RenderAll()
    {
        yield return new WaitForEndOfFrame();
        Setup();
        Idol[] characters = GlobalManager.Instance.GameManager.Characters;

        foreach (Idol character in characters)
        {
            yield return RenderCharacter(character);
        }
        Cleanup();

    }
    public IEnumerator OnlyRenderCharacter(Idol Character)
    {
        yield return new WaitForEndOfFrame();
        Setup();
        yield return RenderCharacter(Character);
        Cleanup();
    }
    public void Dispose()
    {
        if (renderCamera) GameObject.Destroy(renderCamera.gameObject);
    }
}