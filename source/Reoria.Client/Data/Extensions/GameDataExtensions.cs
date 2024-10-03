using Reoria.Game.Data;
using Reoria.Game.Data.Interfaces;

namespace Reoria.Client.Data.Extensions;

public static class GameDataExtensions
{
    private const int defaultPlayerId = -1;
    private static int localPlayerId = defaultPlayerId;

    public static int GetLocalPlayerId(this IGameData gameData)
    {
        if (gameData.Players.Count == 0)
        {
            localPlayerId = defaultPlayerId;
            return localPlayerId;
        }

        Player? player = (from p in gameData.Players
                          where p.Id.Equals(localPlayerId)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            localPlayerId = defaultPlayerId;
            return localPlayerId;
        }

        return localPlayerId;
    }

    public static void SetLocalPlayerId(this IGameData gameData, int newLocalPlayerId)
    {
        if (gameData.Players.Count == 0)
        {
            localPlayerId = defaultPlayerId;
            return;
        }

        Player? player = (from p in gameData.Players
                          where p.Id.Equals(localPlayerId)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            localPlayerId = defaultPlayerId;
            return;
        }

        localPlayerId = newLocalPlayerId;
    }
}
