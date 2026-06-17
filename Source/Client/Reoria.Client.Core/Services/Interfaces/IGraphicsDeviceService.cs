using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Reoria.Client.Core.Services.Interfaces;

public interface IGraphicsDeviceService
{
    GraphicsDevice? GraphicsDevice { get; }
    GraphicsDeviceManager? GraphicsDeviceManager { get; }
}