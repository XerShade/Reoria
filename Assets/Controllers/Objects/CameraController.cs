using UnityEngine;

namespace Reoria.Controllers.Objects
{
    /// <summary>
    /// <see cref="CameraController"/> script, controls basic camera functions for most cameras.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// Defines the <see cref="UnityEngine.Camera"/> instance this script is attached too.
        /// </summary>
        [SerializeField]
        private new Camera camera;

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

        // Start is called before the first frame update
        void Awake()
        {
            // Get the camera object for reference later.
            camera = gameObject.GetComponent<Camera>();

            // Setup some common properties for the camera that the script needs set.
            camera.orthographic = true;
        }

        // Update is called once per frame
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
        }
    }
}
