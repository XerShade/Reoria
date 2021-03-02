using Mirror;
using Reoria.EventArgs;
using Reoria.Models;
using System;
using UnityEngine;

namespace Reoria.Behaviours
{
    /// <summary>
    /// Defines common model functions and properties for use in Unity.
    /// </summary>
    /// <typeparam name="ControllerType">The <see cref="ControllerBehaviour{ModelType}"/> type that this model talks to and receives instructions from.</typeparam>
    public abstract class ModelBehaviour<ControllerType> : NetworkBehaviour, IModel where ControllerType : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="ControllerBehaviour{ModelType}"/> type that this model talks to and receives instructions from.
        /// </summary>
        [SerializeField]
        private ControllerType controller;
        /// <summary>
        /// The <see cref="ControllerBehaviour{ModelType}"/> type that this model talks to and receives instructions from.
        /// </summary>
        public ControllerType Controller { get { return controller; } }

        /// <summary>
        /// The <see cref="IModel"/>'s unique <see cref="System.Guid"/>.
        /// </summary>
        /// <remarks>Do not chage this unless you know what you are doing, your game can and most likely will, break.</remarks>
        [SerializeField]
        private Guid guid;
        /// <summary>
        /// The <see cref="IModel"/>'s unique <see cref="System.Guid"/>.
        /// </summary>
        /// <remarks>Do not chage this unless you know what you are doing, your game can and most likely will, break.</remarks>
        public Guid Guid { get { return guid; } set { var eventArgs = new ModelGuidChangedEventArgs(value); OnGuidChanged(this, eventArgs); guid = eventArgs.Guid; } }

        /// <summary>
        /// Raised when the <see cref="IModel"/>'s unique <see cref="System.Guid"/> changes.
        /// </summary>
        public event EventHandler<ModelGuidChangedEventArgs> OnGuidChanged = (sender, e) => { };

        /// <summary>
        /// Called when the script is started by unity.
        /// </summary>
        void Start()
        {
            // Assign the Guid and find the controller.
            guid = Guid.NewGuid();
            controller = gameObject.GetComponent<ControllerType>();

            // Assign event handlers.
            OnGuidChanged += ModelBehaviour_OnGuidChanged;

            // Check to see if the object has the required controller.
            if (controller == null && isServer)
            {
                // The controller was not found, please create one now.
                controller = gameObject.AddComponent<ControllerType>();

                // Tell the clients that they need to create a controller object.
                RpcModelCreateController();
            }
        }

        /// <summary>
        /// Raised when the <see cref="IModel"/>'s unique <see cref="System.Guid"/> changes.
        /// </summary>
        private void ModelBehaviour_OnGuidChanged(object sender, ModelGuidChangedEventArgs e)
        {
            // Check to see if we are the server or not.
            if(isServer)
            {
                // Nothing much to do here, just let all the clients know that we changed it.
                RpcOnGuidChanged(e.Guid);
            }
        }

        /// <summary>
        /// Called when the server changes an <see cref="IModel"/>'s unique <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="guid">The new <see cref="System.Guid"/> that the <see cref="IModel"/>'s unique <see cref="System.Guid"/> is being changed to.</param>
        [ClientRpc]
        private void RpcOnGuidChanged(Guid guid)
        {
            // Update the guid on the object that we've received from the server.
            this.guid = guid;
        }

        /// <summary>
        /// Called when the server creates a new controller object on the model.
        /// </summary>
        [ClientRpc]
        private void RpcModelCreateController()
        {
            // The server has requested we make a controller object.
            controller = gameObject.AddComponent<ControllerType>();
        }
    }
}
