using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlace : MonoBehaviour
{
    public GameObject objectToSpawn;
    public ARRaycastManager raycastManager;
    private GameObject spawnedObject;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // MODIFIED: Check for Spacebar press instead of touch/click
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // We will raycast from the center of the screen
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            Debug.Log("Spacebar pressed. Trying to raycast from screen center.");

            // Raycast from the screen center against detected planes
            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                Debug.Log("Raycast HIT a plane!");

                Pose hitPose = hits[0].pose;

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                    spawnedObject.transform.rotation = hitPose.rotation;
                }
            }
            else
            {
                // Add this for better debugging
                Debug.Log("Raycast from center did NOT hit a plane.");
            }
        }
    }
}
