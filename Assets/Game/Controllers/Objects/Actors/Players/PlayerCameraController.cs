using Mirror;
using Reoria.Framework.Controllers.Objects;
using Reoria.Framework.Unity.Objects;
using System;
using UnityEngine;

namespace Reoria.Game.Controllers.Objects.Actors.Players
{
    /// <summary>
    /// <see cref="PlayerCameraController"/> script, creates and maintains a camera object on each player.
    /// </summary>
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
        /// The <see cref="NetworkIdentity"/> component instance reference.
        /// </summary>
        [SerializeField]
        private NetworkIdentity identity;

        /// <summary>
        /// The <see cref="CameraController"/> script instance the camera is using.
        /// </summary>
        private CameraController controller;
        /// <summary>
        /// The <see cref="CameraController"/> script instance the camera is using.
        /// </summary>
        public CameraController Controller { get { return controller; } }

        /// <summary>
        /// The target that the camera is currently tracking.
        /// </summary>
        [SerializeField]
        private GameObject target;
        /// <summary>
        /// The target that the camera is currently tracking.
        /// </summary>
        public GameObject Target { get { return target; } }

        /// <summary>
        /// The target position that the camera is tracking. <see cref="target"/> must be set to null to use a raw vector.
        /// </summary>
        [SerializeField]
        private Vector3 targetPosition = new Vector3(0, 0, Constants.Camera.CAMERA_Z_POSITION);
        /// <summary>
        /// The target position that the camera is tracking. <see cref="target"/> must be set to null to use a raw vector.
        /// </summary>
        public Vector3 TargetPosition { get { return targetPosition; } }

        /// <summary>
        /// Called when the script is created.
        /// </summary>
        void Start()
        {
            // Check to see if the player being spawned is the local client or a Unity editor.
            if (isLocalPlayer || isServer || Application.isEditor)
            {
                // Create the camera game object.
                cameraMount = new GameObject("Camera", new Type[] { typeof(NetworkIdentity), typeof(Camera), typeof(CameraController) });
                cameraMount.transform.parent = gameObject.transform;
                cameraMount.transform.position = new Vector3(0, 0, Constants.Camera.CAMERA_Z_POSITION);

                // Create references to the camera components.
                camera = cameraMount.GetComponent<Camera>();
                controller = cameraMount.GetComponent<CameraController>();
                identity = cameraMount.GetComponent<NetworkIdentity>();

                // Setup components for use.
                camera.enabled = false;
                identity.serverOnly = false;
                identity.AssignClientAuthority(gameObject.GetComponent<NetworkIdentity>().connectionToClient);

                // Check to see if this is the local player.
                if (isLocalPlayer)
                {
                    // If in the editor we want to know who we are.
                    gameObject.name += " (Local Player)";

                    // Mount the main camera on this one.
                    MainCamera.Mount(cameraMount);
                }

                // Now setup the camera on the server so it can track and control it if needed.
                NetworkServer.Spawn(cameraMount, gameObject);
                CmdSetTarget(gameObject);
            }
        }

        /// <summary>
        /// Called when hte script is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Check to see if the camera mount has been destroyed.
            if (cameraMount != null)
            {
                // Remove script and component references.
                camera = null;
                controller = null;

                // Check to see if this is the local player.
                if (isLocalPlayer)
                {
                    // Unmount the main camera from this one.
                    MainCamera.Mount(null);
                }

                // Tell the server to delete the camera from all other clients.
                NetworkServer.Destroy(cameraMount);
            }
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
                targetPosition = (target != null) ? new Vector3(target.transform.position.x, target.transform.position.y, Constants.Camera.CAMERA_Z_POSITION) : targetPosition;

                // Adjust the camera's position.
                camera.transform.position = targetPosition;
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
            // Check to see if we have an attached camera component.
            if (camera != null)
            {
                // Update the target the camera is tracking.
                this.target = target;

                // Update the target position, and if we have no targets move the camera back to 0,0.
                targetPosition = (target != null) ? new Vector3(target.transform.position.x, target.transform.position.y, Constants.Camera.CAMERA_Z_POSITION) : new Vector3(0, 0, Constants.Camera.CAMERA_Z_POSITION);

                // Update the target position on the clients.
                RpcUpdateCameraTarget(target, targetPosition);
            }
        }

        /// <summary>
        /// Changes the target position the <see cref="Camera"/> is tracking.
        /// </summary>
        /// <param name="targetPosition">The new target position to track.</param>
        /// <remarks>This will cause the <see cref="Camera"/> to stop tracking whatever <see cref="GameObject"/> it is currently tracking.</remarks>
        [Command]
        public void CmdSetTargetPosition(Vector3 targetPosition)
        {
            // Check to see if we have an attached camera component.
            if (camera != null)
            {
                // We are no longer tracking a specific target.
                target = null;

                // Update the target position manually.
                this.targetPosition = new Vector3(targetPosition.x, targetPosition.y, Constants.Camera.CAMERA_Z_POSITION);

                // Update the target position on the clients.
                RpcUpdateCameraTarget(target, targetPosition);
            }
        }

        /// <summary>
        /// Updates the camera target position values on all clients.
        /// </summary>
        /// <param name="target">The new target to track.</param>
        /// <param name="targetPosition">The new target position to track.</param>
        [ClientRpc]
        private void RpcUpdateCameraTarget(GameObject target, Vector3 targetPosition)
        {
            // Update the properties on the client.
            this.target = target;
            this.targetPosition = targetPosition;
        }
    }
}