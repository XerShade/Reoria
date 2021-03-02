using Reoria.EventArgs;
using System;

namespace Reoria.Models
{
    /// <summary>
    /// Defines common model properties and functions.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Raised when the <see cref="IModel"/>'s unique <see cref="System.Guid"/> changes.
        /// </summary>
        event EventHandler<ModelGuidChangedEventArgs> OnGuidChanged;

        /// <summary>
        /// The <see cref="IModel"/>'s unique <see cref="System.Guid"/>.
        /// </summary>
        /// <remarks>Do not chage this unless you know what you are doing, your game can and most likely will, break.</remarks>
        Guid Guid { get; set; }
    }
}
