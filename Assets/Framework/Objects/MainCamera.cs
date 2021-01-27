using UnityEngine;

namespace Reoria.Framework.Objects
{
    /// <summary>
    /// Main camera controller script, controls the main camera object.
    /// </summary>
    public class MainCamera : MonoBehaviour
    {
        /// <summary>
        /// Defines the <see cref="MainCamera"/> script instance.
        /// </summary>
        private static MainCamera instance;
        /// <summary>
        /// Defines the <see cref="MainCamera"/> script instance.
        /// </summary>
        public static MainCamera Instance { get { return instance; } }

        /// <summary>
        /// Defines the main <see cref="UnityEngine.Camera"/> instance.
        /// </summary>
        [SerializeField]
        private new Camera camera;
        /// <summary>
        /// Defines the main <see cref="UnityEngine.Camera"/> instance.
        /// </summary>
        public Camera Camera => camera;

        /// <summary>
        /// Defines the <see cref="GameObject"/> the <see cref="Camera"/> is currently tracking.
        /// </summary>
        public GameObject Target;

        /// <summary>
        /// Defines the target game window resolution.
        /// </summary>
        [SerializeField]
        private Vector2 targetResolution = new Vector2(Constants.Camera.TARGET_RESOLUTION_WIDTH, Constants.Camera.TARGET_RESOLUTION_HEIGHT);
        /// <summary>
        /// Defines the target game window resolution.
        /// </summary>
        public Vector2 TargetResolution { get { return targetResolution; } }
        /// <summary>
        /// Defines the target game window aspect ratio.
        /// </summary>
        [SerializeField]
        private float targetAspectRatio = Constants.Camera.TARGET_RESOLUTION_WIDTH / Constants.Camera.TARGET_RESOLUTION_HEIGHT;
        /// <summary>
        /// Defines the target game window aspect ratio.
        /// </summary>
        public float TargetAspectRatio { get { return targetAspectRatio; } }

        /// <summary>
        /// Defines the resolution ratio of the game window.
        /// </summary>
        [SerializeField]
        private Vector2 windowResolution;
        /// <summary>
        /// Defines the resolution ratio of the game window.
        /// </summary>
        public Vector2 WindowResolution { get { return windowResolution; } }
        /// <summary>
        /// Defines the aspect ratio of the game window.
        /// </summary>
        [SerializeField]
        private float windowAspectRatio;
        /// <summary>
        /// Defines the aspect ratio of the game window.
        /// </summary>
        public float WindowAspectRatio { get { return windowAspectRatio; } }

        /// <summary>
        /// Defines the aspect ratio the game view should be scaled to.
        /// </summary>
        [SerializeField]
        private float scaleAspectRatio;
        /// <summary>
        /// Defines the aspect ratio the game view should be scaled to.
        /// </summary>
        public float ScaleAspectRatio { get { return scaleAspectRatio; } }

        /// <summary>
        /// Called when the object is first created.
        /// </summary>
        void Awake()
        {
            // Check to see if we already have an instance.
            if (Instance != null)
            {
                // Write out to the log so they user knows something else tried to make a main camera.
                Debug.LogWarning($"{gameObject.name} is attempting to create a main camera instance while one is already created and running on {Instance.gameObject.name}. " +
                    $"The old camera will be destroyed and the script will be stopped so the new object and script can function.");

                // We do, destroy the old one.
                Destroy(Instance.gameObject);
                instance = null;
            }

            // Set the script instance reference to this instance.
            instance = this;

            // Cache the main camera instance, Camera.main is slow.
            camera = Camera.main;
        }

        /// <summary>
        /// Called when the object is updated.
        /// </summary>
        void Update()
        {
            // Check to see if the client has been resized since we last calculated the view.
            if (Screen.width != windowResolution.x || Screen.height != windowResolution.y)
            {
                // Calculate the window resolution and aspect ratio.
                windowResolution = new Vector2(Screen.width, Screen.height);
                windowAspectRatio = windowResolution.x / windowResolution.y;

                // Calculate the scaling aspect ratio.
                scaleAspectRatio = windowAspectRatio / targetAspectRatio;

                // Calculate the orthographic size.
                camera.orthographicSize = windowResolution.x / (windowAspectRatio * 2 * Constants.PIXELS_PER_UNIT);

                // Check to see if we need to scale the window vertically.
                if (scaleAspectRatio < 1.0f)
                {
                    // Get the camera's view rect.
                    Rect rect = camera.rect;

                    // Calculate the new view rect using the values we just calculated earlier.
                    rect.width = 1.0f;
                    rect.height = scaleAspectRatio;
                    rect.x = 0;
                    rect.y = (1.0f - scaleAspectRatio) / 2.0f;

                    // Assign the new view rect to the camera.
                    camera.rect = rect;
                }
                else
                {
                    // Okay, we're doing this horizontally, calculate that please.
                    float scalewidth = 1.0f / scaleAspectRatio;

                    // Get the camera's view rect.
                    Rect rect = camera.rect;

                    // Calculate the new view rect using the values we just calculated earlier.
                    rect.width = scalewidth;
                    rect.height = 1.0f;
                    rect.x = (1.0f - scalewidth) / 2.0f;
                    rect.y = 0;

                    // Assign the new view rect to the camera.
                    camera.rect = rect;
                }
            }

            // Calculate the position we want to move the camera to.
            Vector3 targetPosition = (Target != null) ? new Vector3(Target.transform.position.x, Target.transform.position.y, -10) : (Vector3.back * 10);

            // Adjust the camera's position.
            camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, Time.deltaTime);

            // Check to see if we are within the lerp deadzone vertically.
            if ((camera.transform.position.y - targetPosition.y < 0f) && (camera.transform.position.y - targetPosition.y > Constants.LERP_SNAP * -0.1f))
                camera.transform.position = new Vector3(camera.transform.position.x, targetPosition.y, camera.transform.position.z);
            if ((camera.transform.position.y - targetPosition.y > 0f) && (camera.transform.position.y - targetPosition.y < Constants.LERP_SNAP * 0.1f))
                camera.transform.position = new Vector3(camera.transform.position.x, targetPosition.y, camera.transform.position.z);

            // Check to see if we are within the lerp deadzone horizontally.
            if ((camera.transform.position.x - targetPosition.x > 0f) && (camera.transform.position.x - targetPosition.x < Constants.LERP_SNAP * 0.1f))
                camera.transform.position = new Vector3(targetPosition.x, camera.transform.position.y, camera.transform.position.z);
            if ((camera.transform.position.x - targetPosition.x < 0f) && (camera.transform.position.x - targetPosition.x > Constants.LERP_SNAP * -0.1f))
                camera.transform.position = new Vector3(targetPosition.x, camera.transform.position.y, camera.transform.position.z);
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Remove the script instance reference.
            instance = null;
        }
    }
}