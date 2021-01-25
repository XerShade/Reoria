using System.Collections.Generic;
using Reoria.Framework.Extensions;
using UnityEngine;

namespace Reoria.Framework.Unity
{
    /// <summary>
    /// Changes a <see cref="UnityEngine.GameObject"/> to be static and remain loaded between scene changes.
    /// </summary>
    public class StaticBehaviour : MonoBehaviour
    {
        /// <summary>
        /// A collection of all <see cref="UnityEngine.GameObject"/> instances currently loaded using this script.
        /// </summary>
        private static readonly Dictionary<string, GameObject> staticObjects = new Dictionary<string, GameObject>();

        /// <summary>
        /// The <see cref="UnityEngine.GameObject"/> instace the script is currently managing.
        /// </summary>
        [SerializeField]
        private GameObject reference = default;

        /// <summary>
        /// Called when the object is first created.
        /// </summary>
        void Awake()
        {
            // Check to see if the object has already been loaded.
            if (!staticObjects.ContainsKey(gameObject.name))
            {
                // Add the object to the collection and store the reference.
                reference = staticObjects.Add(gameObject.name, gameObject, false);

                // Set the object to DontDestroyOnLoad so we can keep it between scenes.
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Woah there, this object is already loaded. This violates the whole static concept.
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the object is updated.
        /// </summary>
        private void Update()
        {
            // Check to see if the object has been loaded.
            if (!staticObjects.ContainsKey(gameObject.name) || !staticObjects[gameObject.name].Equals(reference))
            {
                // We somehow lost the object reference, destroy it.
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Check to see if the object has been loaded.
            if (staticObjects.ContainsKey(gameObject.name))
            {
                // Remove the object from the collection.
                staticObjects.Remove(gameObject.name);

                // Remove the stored reference to the object.
                reference = null;
            }
        }
    }

    /// <summary>
    /// Defines extensions for <see cref="UnityEngine.GameObject"/> instances to access their <see cref="StaticBehaviour"/> component.
    /// </summary>
    public static class StaticBehaviourExtensions
    {
        /// <summary>
        /// Determines if the <see cref="UnityEngine.GameObject"/> is static and contains a <see cref="StaticBehaviour"/> component.
        /// </summary>
        /// <param name="gameObject">The <see cref="UnityEngine.GameObject"/> instance to run this function on.</param>
        /// <returns>True if a <see cref="StaticBehaviour"/> component is found, false if not.</returns>
        public static bool IsStatic(this GameObject gameObject) => gameObject.GetComponent<StaticBehaviour>() != null;
        /// <summary>
        /// Returns a static <see cref="UnityEngine.GameObject"/>'s <see cref="StaticBehaviour"/> component.
        /// </summary>
        /// <param name="gameObject">The <see cref="UnityEngine.GameObject"/> instance to run this function on.</param>
        /// <returns>The <see cref="StaticBehaviour"/> component if it is found, <see cref="default"/> if not.</returns>
        public static StaticBehaviour GetStaticStaticBehaviour(this GameObject gameObject) => gameObject.IsStatic() ? gameObject.GetComponent<StaticBehaviour>() : default;
    }
}
