using UnityEngine;
using System;
using Reoria.Framework.Controllers.Objects;

namespace Reoria.Framework.Unity.Objects
{
    /// <summary>
    /// <see cref="MainCamera"/> script, creates and maintains the main camera object.
    /// </summary>
    public class MainCamera : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="MainCamera"/> object instance.
        /// </summary>
        [SerializeField]
        private static GameObject instance;
        /// <summary>
        /// The <see cref="MainCamera"/> object instance.
        /// </summary>
        public static GameObject Instance { get { return instance; } }

        /// <summary>
        /// Creates a new <see cref="MainCamera"/> instance.
        /// </summary>
        public static void Instantiate()
        {
            // Check to see if we have an instance already.
            if(instance == null)
            {
                // We don't, create one. No need for setup, the script will do that when it loads.
                instance = new GameObject("MainCamera", new Type[] { typeof(Camera), typeof(AudioListener), typeof(StaticBehaviour), typeof(MainCamera), typeof(CameraController) });
            }
        }

        /// <summary>
        /// Mounts the <see cref="MainCamera"/> on a <see cref="GameObject"/>.
        /// </summary>
        /// <param name="mountPoint">The <see cref="GameObject"/> to mount the camera on.</param>
        public static void Mount(GameObject mountPoint)
        {
            // Update the mount point.
            instance.GetComponent<MainCamera>().mountPoint = mountPoint;
        }

        /// <summary>
        /// The <see cref="Camera"/> component instance reference.
        /// </summary>
        [SerializeField]
        private new Camera camera;

        /// <summary>
        /// The <see cref="CameraController"/> script instance the camera is using.
        /// </summary>
        [SerializeField]
        private CameraController controller;

        /// <summary>
        /// The <see cref="GameObject"/> the camera is currently mounted on.
        /// </summary>
        [SerializeField]
        private GameObject mountPoint;

        /// <summary>
        /// Called when the script is created.
        /// </summary>
        void Start()
        {
            // Set default properties.
            instance.transform.position = new Vector3(0, 0, Constants.Camera.CAMERA_Z_POSITION);
            instance.tag = "MainCamera";

            // Create references to the camera components.
            camera = instance.GetComponent<Camera>();
            controller = instance.GetComponent<CameraController>();
        }

        /// <summary>
        /// Called once per frame when the script is updated.
        /// </summary>
        void Update()
        {
            // Check to see if we have an attached camera component.
            if (camera != null)
            {
                // Check to see if we are tracking a target, and if so update the position stored on the script.
                var targetPosition = (mountPoint != null) ? new Vector3(mountPoint.transform.position.x, mountPoint.transform.position.y, Constants.Camera.CAMERA_Z_POSITION) :
                    new Vector3(0, 0, Constants.Camera.CAMERA_Z_POSITION);

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
        }
    }
}
