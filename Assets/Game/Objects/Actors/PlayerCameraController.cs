using Mirror;
using Reoria.Framework;
using Reoria.Framework.Objects;
using UnityEngine;

namespace Reoria.Game.Objects.Actors
{
    public class PlayerCameraController : NetworkBehaviour
    {
        /// <summary>
        /// The <see cref="Camera"/> <see cref="GameObject"/>
        /// </summary>
        [SerializeField]
        private GameObject cameraMount;
        /// <summary>
        /// The <see cref="Camera"/> component instance reference.
        /// </summary>
        [SerializeField]
        private new Camera camera;

        /// <summary>
        /// The target that the camera is currently tracking.
        /// </summary>
        [SerializeField]
        private GameObject target;

        /// <summary>
        /// The target position that the camera is tracking. <see cref="target"/> must be set to null to use a raw vector.
        /// </summary>
        [SerializeField]
        private Vector3 targetPosition;

        /// <summary>
        /// Called when the script is created.
        /// </summary>
        void Start()
        {
            // Create the camera game object.
            cameraMount = new GameObject("Camera", new System.Type[] { typeof(NetworkIdentity), typeof(Camera), typeof(CameraController) });
            cameraMount.transform.parent = gameObject.transform;
            cameraMount.transform.position = new Vector3(0, 0, -10);

            // Check to see if the player being spawned is the local client.
            if (isLocalPlayer)
            {
                // Assign components only the local client needs.
                cameraMount.AddComponent<AudioListener>();

                // Assign local variables to the camera.
                cameraMount.tag = "MainCamera";
            }

            // Create a reference to the camera component.
            camera = cameraMount.GetComponent<Camera>();

            // Now setup the camera on the server so it can track and control it if needed.
            NetworkServer.Spawn(cameraMount, gameObject);
            CmdSetTarget(gameObject);
        }

        /// <summary>
        /// Called once per frame when the script is updated.
        /// </summary>
        void Update()
        {
            // Check to see if we have an attached camera component.
            if(camera != null)
            {
                // Check to see if we are tracking a target, and if so update the position stored on the script.
                targetPosition = (target != null) ? new Vector3(target.transform.position.x, target.transform.position.y, Constants.Camera.CAMERA_Z_POSITION) : targetPosition;

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

        /// <summary>
        /// Changes the target the <see cref="Camera"/> is tracking.
        /// </summary>
        /// <param name="target">The new target to track.</param>
        /// <remarks>This will cause the <see cref="Camera"/> to stop tracking whatever <see cref="Vector3"/> it is currently tracking.</remarks>
        [Command]
        public void CmdSetTarget(GameObject target)
        {
            // Update the target the camera is tracking.
            this.target = target;

            // Update the target position, and if we have no targets move the camera back to 0,0.
            targetPosition = (target != null) ? new Vector3(target.transform.position.x, target.transform.position.y, Constants.Camera.CAMERA_Z_POSITION) : Vector3.forward * Constants.Camera.CAMERA_Z_POSITION;
        }

        /// <summary>
        /// Changes the target position the <see cref="Camera"/> is tracking.
        /// </summary>
        /// <param name="targetPosition">The new target position to track.</param>
        /// <remarks>This will cause the <see cref="Camera"/> to stop tracking whatever <see cref="GameObject"/> it is currently tracking.</remarks>
        [Command]
        public void CmdSetTargetPosition(Vector3 targetPosition)
        {
            // We are no longer tracking a specific target.
            target = null;

            // Update the target position manually.
            this.targetPosition = new Vector3(targetPosition.x, targetPosition.y, Constants.Camera.CAMERA_Z_POSITION);
        }
    }
}
