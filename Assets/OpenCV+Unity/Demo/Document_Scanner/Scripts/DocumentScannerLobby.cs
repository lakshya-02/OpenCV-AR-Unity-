namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using UnityEngine.UI;
	using System.Collections;
	using UnityEngine.SceneManagement;

	public class DocumentScannerLobby : MonoBehaviour {

		private bool sceneLoaded = false;
		private bool isLoading = false;

		// Use this for initialization
		void Awake () {
			StartCoroutine(LoadSceneAsync());
		}

		void OnDestroy() {
			Scene scene = SceneManager.GetSceneByName("DocumentScannerScene");
			if (scene.isLoaded) {
				SceneManager.UnloadSceneAsync("DocumentScannerScene");
			}
		}

		public void OnButton(string name) {
			if (sceneLoaded && !isLoading) {
				NavigateTo(name);
			} else if (isLoading) {
				Debug.LogWarning("DocumentScannerScene is still loading, please wait...");
			} else {
				Debug.LogError("DocumentScannerScene failed to load!");
			}
		}

		private IEnumerator LoadSceneAsync() {
			isLoading = true;
			
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("DocumentScannerScene", LoadSceneMode.Additive);
			
			// Wait until the scene is fully loaded
			while (!asyncLoad.isDone) {
				yield return null;
			}
			
			// Additional wait to ensure all GameObjects are initialized
			yield return new WaitForEndOfFrame();
			
			sceneLoaded = true;
			isLoading = false;
			Debug.Log("DocumentScannerScene loaded successfully");
		}

		private void NavigateTo(string name) {
			Scene scene = SceneManager.GetSceneByName("DocumentScannerScene");
			if (scene.isLoaded) {
				DocumentScannerScript script = Object.FindObjectOfType<DocumentScannerScript>();
				
				if (script != null) {
					script.Process(name);
					Debug.Log($"DocumentScannerScript processing: {name}");
				} else {
					Debug.LogError("DocumentScannerScript not found in the loaded scene!");
				}
			} else {
				Debug.LogError("DocumentScannerScene is not loaded!");
			}
		}
	}
}