using Mirror;
using Reoria.Models;
using Reoria.Views;
using System;
using UnityEngine;

namespace Reoria.Behaviours
{
    /// <summary>
    /// Defines common view functions and properties for use in Unity.
    /// </summary>
    /// <typeparam name="ControllerType">The <see cref="ControllerBehaviour{ModelType}"/> type that this view talks to and receives instructions from.</typeparam>
    public abstract class ViewBehaviour<ControllerType> : NetworkBehaviour, IView where ControllerType : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="ControllerBehaviour{ModelType}"/> type that this view talks to and receives instructions from.
        /// </summary>
        private ControllerType controller;
        /// <summary>
        /// The <see cref="ControllerBehaviour{ModelType}"/> type that this view talks to and receives instructions from.
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
        public Guid Guid { get { return guid; } set { guid = value; } }

        /// <summary>
        /// Called when the script is started by unity.
        /// </summary>
        void Start()
        {
            // Find the controller.
            controller = gameObject.GetComponent<ControllerType>();

            // Check to see if the object has the required controller.
            if (controller == null && isServer)
            {
                // The controller was not found, please create one now.
                controller = gameObject.AddComponent<ControllerType>();

                // Tell the clients that they need to create a controller object.
                RpcViewCreateController();
            }
        }

        /// <summary>
        /// Called when the server creates a new controller object on the view.
        /// </summary>
        [ClientRpc]
        private void RpcViewCreateController()
        {
            // The server has requested we make a controller object.
            controller = gameObject.AddComponent<ControllerType>();
        }
    }
}