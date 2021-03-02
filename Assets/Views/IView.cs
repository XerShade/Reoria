using Reoria.Models;
using System;

namespace Reoria.Views
{
    /// <summary>
    /// Defines common view properties and functions.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// The <see cref="IModel"/>'s unique <see cref="System.Guid"/>.
        /// </summary>
        /// <remarks>Do not chage this unless you know what you are doing, your game can and most likely will, break.</remarks>
        Guid Guid { get; set; }
    }
}
