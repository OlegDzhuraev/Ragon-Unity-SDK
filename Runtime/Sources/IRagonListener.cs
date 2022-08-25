using Ragon.Common;

namespace Ragon.Client
{
  public interface IRagonListener
  {
    void OnAuthorized(string playerId, string playerName);
    void OnJoined();
    void OnFailed(string message);
    void OnLeaved();
    void OnConnected();
    void OnDisconnected();
    
    void OnPlayerJoined(RagonPlayer player);
    void OnPlayerLeft(RagonPlayer player);
    void OnOwnerShipChanged(RagonPlayer player);
    
    void OnLevel(string sceneName);
    void OnEvent(RagonPlayer player, ushort evntCode, RagonSerializer payload);
  }
}