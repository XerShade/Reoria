using Reoria.Behaviours;
using UnityEngine;

namespace Reoria.Extensions.Reoria.Behaviours
{
    /// <summary>
    /// Defines extensions for <see cref="GameObject"/> instances to access their <see cref="StaticBehaviour"/> component.
    /// </summary>
    public static class StaticBehaviourExtensions
    {
        /// <summary>
        /// Determines if the <see cref="GameObject"/> is static and contains a <see cref="StaticBehaviour"/> component.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> instance to run this function on.</param>
        /// <returns>True if a <see cref="StaticBehaviour"/> component is found, false if not.</returns>
        public static bool IsStatic(this GameObject gameObject) => gameObject.GetComponent<StaticBehaviour>() != null;
        /// <summary>
        /// Returns a static <see cref="GameObject"/>'s <see cref="StaticBehaviour"/> component.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> instance to run this function on.</param>
        /// <returns>The <see cref="StaticBehaviour"/> component if it is found, <see cref="default"/> if not.</returns>
        public static StaticBehaviour GetStaticBehaviour(this GameObject gameObject) => gameObject.IsStatic() ? gameObject.GetComponent<StaticBehaviour>() : default;
    }
}
