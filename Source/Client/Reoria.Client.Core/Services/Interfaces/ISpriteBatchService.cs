using Microsoft.Xna.Framework.Graphics;

namespace Reoria.Client.Core.Services.Interfaces;

public interface ISpriteBatchService
{
    SpriteBatch? SpriteBatch { get; }
}