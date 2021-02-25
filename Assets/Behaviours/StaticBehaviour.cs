using Reoria.Extensions.System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

namespace Reoria.Behaviours
{
    /// <summary>
    /// Changes a <see cref="GameObject"/> to be static and remain loaded between scene changes.
    /// </summary>
    public class StaticBehaviour : MonoBehaviour
    {
        /// <summary>
        /// A collection of all <see cref="GameObject"/> instances currently loaded using this script.
        /// </summary>
        private static readonly Dictionary<string, GameObject> staticObjects = new Dictionary<string, GameObject>();

        /// <summary>
        /// The <see cref="GameObject"/> instace the script is currently managing.
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
        void Update()
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
}
