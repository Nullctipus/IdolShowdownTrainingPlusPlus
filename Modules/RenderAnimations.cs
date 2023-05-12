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
    public IEnumerator RenderAll()
    {
        yield return new WaitForEndOfFrame();
        CanvasRenderer[] canvases = GameObject.FindObjectsOfType<CanvasRenderer>();
        foreach (var c in canvases)
            c.gameObject.SetActive(false);

        Plugin.IsTraining = true;
        int layer = 9;
        if (renderCamera == null)
        {
            renderCamera = new GameObject("Training++ RenderCamera").AddComponent<Camera>();
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 2;
            renderCamera.transform.position = new Vector3(1024, 1024, -5);
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            renderCamera.cullingMask = 1 << layer;
            rt = new RenderTexture(2048, 2048, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            renderCamera.targetTexture = rt;
            rt.filterMode = FilterMode.Point;
        }
        RenderTexture oldrt = RenderTexture.active;
        RenderTexture.active = rt;
        Directory.CreateDirectory(BasePath);
        Idol[] characters = GlobalManager.Instance.GameManager.Characters;
        Texture2D tex = new Texture2D(rt.width, rt.height);

        foreach (Idol character in characters)
        {
            Plugin.Logging.LogInfo(character.charName);
            Directory.CreateDirectory(Path.Combine(BasePath, character.charName));
            // Create character
            GameObject instanced = GameObject.Instantiate(character.originalPrefab, Vector3.zero, Quaternion.identity);

            yield return null; // Let The character Initialize
            RenderTexture.active = rt; // Make sure we still render to the right place

            instanced.transform.position = new Vector3(1024, 1024, 0);

            // Set layer so it gets rendered
            instanced.layer = layer;
            foreach (var t in instanced.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;

            // Get list of moves

            Animator animator = instanced.GetComponent<Animator>();
            MoveQueue moveQueue = instanced.GetComponent<MoveQueue>();
            moveQueue.StartMe(); // Populate
            animator.enabled = false;


            foreach (var move in moveQueue.CurrentMoveList.ListedInputMoves)
            {

                Plugin.Logging.LogInfo(move.AnimationName);
                Directory.CreateDirectory(Path.Combine(BasePath, character.charName, move.AnimationName));
                animator.Play(move.AnimationName, 0);
                animator.EvaluateController(.001f);

                AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                float frametime = 1 / clip.frameRate;
                int i = 1;

                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f)
                {
                    renderCamera.Render();
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    File.WriteAllBytes(Path.Combine(BasePath, character.charName, move.AnimationName, "frame" + i + ".png"), ImageConversion.EncodeToPNG(tex));
                    animator.Update(frametime);
                    i++;
                }

            }
            GameObject.Destroy(instanced);
        }
        GameObject.Destroy(tex);
        foreach (var c in canvases)
            c.gameObject.SetActive(true);

        RenderTexture.active = oldrt;
        //reset
        Plugin.IsTraining = GlobalManager.Instance.GameManager.IsGameMode(GameMode.training);
        yield return null;

    }
    public void Dispose()
    {
        if (renderCamera) GameObject.Destroy(renderCamera.gameObject);
    }
}