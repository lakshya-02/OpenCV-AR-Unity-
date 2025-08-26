using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LobbyScript: MonoBehaviour {
    private string currentScene = null;
    public GameObject mainMenu;
    public GameObject backMenu;
    
    private bool isLoading = false; // Prevent multiple simultaneous operations

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public void OnFaceDetectorButton() {
        if (!isLoading) NavigateTo("FaceDetectorScene");
    }

    public void OnFaceRecognizerButton()
    {
        if (!isLoading) NavigateTo("FaceRecognizer");
    }

    public void OnGrayscaleButton() {
        if (!isLoading) NavigateTo("GrayscaleScene");
    }
    
    public void OnContoursByShapeScenerButton() {
        if (!isLoading) NavigateTo("ContoursByShapeScene");
    }

    public void OnLiveSketchButton() {
        if (!isLoading) NavigateTo("LiveSketchScene");
    }

    public void OnMarkerDetectorButton() {
        if (!isLoading) NavigateTo("MarkerDetectorScene");
    }

    public void OnDocumenScannerButton()
    {
        if (!isLoading) NavigateTo("DocumentScannerLobby");
    }

    public void OnAlphabetOCRButton()
    {
        if (!isLoading) NavigateTo("AlphabetOCR");
    }

    public void OnTrackingButton()
    {
        if (!isLoading) NavigateTo("TrackingScene");
    }

    public void OnBackButton() {
        if (!isLoading) {
            StartCoroutine(UnloadSceneAsync());
        }
    }

    private IEnumerator UnloadSceneAsync() {
        if (currentScene != null) {
            isLoading = true;
            
            // Check if scene exists before unloading
            Scene scene = SceneManager.GetSceneByName(currentScene);
            if (scene.isLoaded) {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentScene);
                
                // Wait for unload to complete
                while (!unloadOperation.isDone) {
                    yield return null;
                }
            }
            
            currentScene = null;
            isLoading = false;
        }
        
        mainMenu.SetActive(true);
        backMenu.SetActive(false);
    }

    private void NavigateTo(string sceneName) {
        StartCoroutine(NavigateToAsync(sceneName));
    }
    
    private IEnumerator NavigateToAsync(string sceneName) {
        isLoading = true;
        
        // First unload current scene if exists
        if (currentScene != null) {
            Scene scene = SceneManager.GetSceneByName(currentScene);
            if (scene.isLoaded) {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentScene);
                
                // Wait for unload to complete
                while (!unloadOperation.isDone) {
                    yield return null;
                }
            }
        }
        
        // Update UI
        mainMenu.SetActive(false);
        backMenu.SetActive(true);
        
        // Load new scene
        currentScene = sceneName;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        // Wait for load to complete
        while (!loadOperation.isDone) {
            yield return null;
        }
        
        isLoading = false;
        Debug.Log($"Scene {sceneName} loaded successfully");
    }
}
