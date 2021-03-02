using Mirror;
using Reoria.Controllers;
using Reoria.Models;
using Reoria.Views;
using UnityEngine;

namespace Reoria.Behaviours
{
    /// <summary>
    /// Defines common view functions and properties for use in Unity.
    /// </summary>
    /// <typeparam name="ModelType">The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.</typeparam>
    public abstract class ControllerBehaviour<ModelType> : NetworkBehaviour, IController where ModelType : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        private ModelType model;
        /// <summary>
        /// The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        public ModelType Model { get { return model; } }

        /// <summary>
        /// Called when the script is started by unity.
        /// </summary>
        void Start()
        {
            // Find the controller.
            model = gameObject.GetComponent<ModelType>();

            // Check to see if the object has the required model.
            if (model == null && isServer)
            {
                // The controller was not found, please create one now.
                model = gameObject.AddComponent<ModelType>();

                // Tell the clients that they need to create a controller object.
                RpcControllerCreateModel();
            }
        }

        /// <summary>
        /// Called when the server creates a new model object on the controller.
        /// </summary>
        [ClientRpc]
        private void RpcControllerCreateModel()
        {
            // The server has requested we make a model object.
            model = gameObject.AddComponent<ModelType>();
        }
    }

    /// <summary>
    /// Defines common view functions and properties for use in Unity.
    /// </summary>
    /// <typeparam name="ModelType">The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.</typeparam>
    /// <typeparam name="ViewType">The <see cref="ViewBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.</typeparam>
    public abstract class ControllerBehaviour<ModelType, ViewType> : NetworkBehaviour where ModelType : MonoBehaviour where ViewType : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        private ModelType model;
        /// <summary>
        /// The <see cref="ModelBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        public ModelType Model { get { return model; } }

        /// <summary>
        /// The <see cref="ViewBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        private ViewType view;
        /// <summary>
        /// The <see cref="ViewBehaviour{ControllerType}"/> type that this model talks to and sends instructions to.
        /// </summary>
        public ViewType View { get { return view; } }

        /// <summary>
        /// Called when the script is started by unity.
        /// </summary>
        void Start()
        {
            // Find the controller.
            model = gameObject.GetComponent<ModelType>();
            view = gameObject.GetComponent<ViewType>();

            // Assign event handlers to the model.
            (model as IModel).OnGuidChanged += HandleGuidChanged;

            // Check to see if the object has the required model.
            if (model == null && isServer)
            {
                // The controller was not found, please create one now.
                model = gameObject.AddComponent<ModelType>();

                // Tell the clients that they need to create a controller object.
                RpcControllerCreateModel();
            }

            // Check to see if the object has the required view.
            if (view == null && isServer)
            {
                // The controller was not found, please create one now.
                view = gameObject.AddComponent<ViewType>();

                // Tell the clients that they need to create a controller object.
                RpcControllerCreateView();
            }
        }

        /// <summary>
        /// Called when the <see cref="IModel.OnGuidChanged"/> event is raised.
        /// </summary>
        /// <param name="sender">The object invoking the event handler.</param>
        /// <param name="e">The event arguments being passed to the event handler.</param>
        private void HandleGuidChanged(object sender, EventArgs.ModelGuidChangedEventArgs e)
        {
            // Update the view's stored value.
            (view as IView).Guid = e.Guid;
        }

        /// <summary>
        /// Called when the server creates a new model object on the controller.
        /// </summary>
        [ClientRpc]
        private void RpcControllerCreateModel()
        {
            // The server has requested we make a model object.
            model = gameObject.AddComponent<ModelType>();
        }

        /// <summary>
        /// Called when the server creates a new model object on the controller.
        /// </summary>
        [ClientRpc]
        private void RpcControllerCreateView()
        {
            // The server has requested we make a model object.
            view = gameObject.AddComponent<ViewType>();
        }
    }
}