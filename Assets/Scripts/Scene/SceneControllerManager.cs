using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    public SceneName startingSceneName;

    private bool isFading;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image fadeImage = null;

    private IEnumerator Fade (float finalAlpha)
    {
        // Set the fading flag to true so the FadeAndSwitchScenes coroutine won't be called again
        isFading = true;

        // Make sure the CanvasGroup blocks raycasts into the scene so no more input can be accepted
        faderCanvasGroup.blocksRaycasts = true;

        // Calculate how fast the CanvasGroup should fade based on it's current alpha, it's final alpha and how long it has to change
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        // While the CavasGroup hasn't reached the final alpha yet...
        while(!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            // Move the alpha towards it's target alpha
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            // Wait for a frame, then continue
            yield return null;
        }

        // Set the flag to false since the fade has finished
        isFading = false;

        // Stop the CanvasGroup from blocking raycasts
        faderCanvasGroup.blocksRaycasts = false;
    }

    // This is the co-routine where the 'building blocks' of the script are put together
    
    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        // Call before scene unload fade out event
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        // Disable player input
        PlayerMovement.Instance.DisablePlayerInput();

        // Start fading to black and wait for it to finish before continuing
        yield return StartCoroutine(Fade(1f));

        // Set player position
        Player.Instance.gameObject.transform.position = spawnPosition;

        // Call before scene unload event
        EventHandler.CallBeforeSceneUnloadEvent();

        // Unload the current active scene
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        // Start loading the given scene and wait for it to finish
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        // Call after scene load event
        EventHandler.CallAfterSceneLoadEvent();

        // Re-Enable player input
        PlayerMovement.Instance.EnablePlayerInput();

        // Starting fading back in and wait for it to finish before exiting the function
        yield return StartCoroutine(Fade(0f));

        // Call after scene load fade in event
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        // Allow the given scene to load over several frames and add it to the already loaded scenes (Persistent)
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Find the scene that was most recently loaded (the one at the last index of the loaded scenes)
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        // Set the newly loaded scene as the active scene (marks it to be unloaded next)
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Start()
    {
        // Set the initial alpha to start off with black scene
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        // Start the first scene loading and wait for it to finish
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        // If this event has any subscribers, call it
        EventHandler.CallAfterSceneLoadEvent();

        // Once the scene is finished loading, start fading in
        StartCoroutine(Fade(0f));
    }

    // Main external point of contact and influence from the rest of the project.
    // This will be called when the player wants to switch scenes

    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        // If there isn't a fade already happening, then switch scenes
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }
}
