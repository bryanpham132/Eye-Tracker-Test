using UnityEngine;
using ViveSR.anipal.Eye; // Importing the namespace for the Vive Eye Tracker SDK.
using System.Text.RegularExpressions; // For using regular expressions.
using System.Collections; // For using coroutines.

public class EyeGazeRaycast : MonoBehaviour // Defining a new class EyeGazeRaycast, inheriting from MonoBehaviour.
{
    public static Vector3? CurrentGazeHitPoint { get; private set; } = null; // A nullable static property to store the current gaze hit point.
    public float rayLength = 100f; // Public variable to set the length of the ray.
    public Color rayColor = Color.red; // Public variable to set the color of the ray.

    private LineRenderer lineRenderer; // Private variable to hold a reference to the LineRenderer component.

    private void Awake() // Awake is called when the script instance is being loaded.
    {
        lineRenderer = GetComponent<LineRenderer>(); // Getting the LineRenderer component attached to this GameObject.

        if (lineRenderer == null) // Checking if the LineRenderer component is not found.
        {
            Debug.LogError("No LineRenderer component found on this game object. Please add one."); // Logging an error message.
            return; // Exiting the method.
        }

        lineRenderer.startColor = rayColor; // Setting the start color of the line.
        lineRenderer.endColor = rayColor; // Setting the end color of the line.
        lineRenderer.positionCount = 2; // Setting the number of positions in the LineRenderer to 2 (start and end points).
    }

    private void Update() // Update is called once per frame.
    {
        PerformEyeGazeRaycast(); // Calling the method to perform the eye gaze raycast.
    }

    private void PerformEyeGazeRaycast() // Method to perform the eye gaze raycast.
    {
        Ray gazeRay; // Declaring a Ray variable to store the gaze direction.

        // Obtaining the combined gaze direction for both eyes and storing it in gazeRay.
        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out gazeRay))
        {
            gazeRay.origin = transform.position; // Setting the origin of the gazeRay to the current GameObject's position.

            RaycastHit hitInfo; // Declaring a RaycastHit variable to store information about what the ray hits.

            lineRenderer.positionCount = 2; // Ensuring the LineRenderer has two positions.

            // Performing a raycast in the direction of gazeRay.
            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                CurrentGazeHitPoint = hitInfo.point; // Updating the current gaze hit point with the point of collision.
                GameObject hitObject = hitInfo.collider.gameObject; // Getting the GameObject that was hit.

                // Checking if the hit object's name matches a specific pattern.
                if (Regex.IsMatch(hitObject.name, @"Cube \(\d+\)"))
                {
                    hitObject.SetActive(false); // Disabling the hit GameObject.
                    StartCoroutine(ActivateAfterDelay(hitObject, 2.0f)); // Starting a coroutine to reactivate the object after a delay.
                }

                // Logging the name and position of the hit object.
                Debug.Log("Gazed at: " + hitObject.name + " at position: " + hitInfo.point);

                // Setting the start and end positions of the LineRenderer to visualize the ray.
                lineRenderer.SetPosition(0, gazeRay.origin);
                lineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                CurrentGazeHitPoint = null; // Resetting the current gaze hit point if nothing was hit.
                // Drawing the ray for the specified length if no collision is detected.
                lineRenderer.SetPosition(0, gazeRay.origin);
                lineRenderer.SetPosition(1, gazeRay.origin + gazeRay.direction * rayLength);
            }
        }
        else
        {
            // Hiding the LineRenderer if no valid gaze data is obtained.
            lineRenderer.positionCount = 0;
        }
    }

    private IEnumerator ActivateAfterDelay(GameObject obj, float delay) // Coroutine to activate a GameObject after a specified delay.
    {
        yield return new WaitForSeconds(delay); // Waiting for the specified amount of time.
        obj.SetActive(true); // Reactivating the GameObject.
    }
}