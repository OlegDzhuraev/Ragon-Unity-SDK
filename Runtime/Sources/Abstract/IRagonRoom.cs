using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ragon.Common;
using UnityEngine;

namespace Ragon.Client
{
  public interface IRagonRoom
  {
    public string Id { get; }
    
    public int MinPlayers { get; }
    public int MaxPlayers { get; }
    
    public RagonPlayer Owner { get; }
    public RagonPlayer LocalPlayer { get; }
    public ReadOnlyCollection<RagonPlayer> Players { get; }
    public IReadOnlyDictionary<uint, RagonPlayer> Connections { get; }
    public IReadOnlyDictionary<string, RagonPlayer> PlayersMap { get; }
    
    public void LoadScene(string map);
    public void SceneLoaded();
    public void CreateEntity(ushort type, IRagonSerializable spawnPayload, RagonAuthority state = RagonAuthority.OWNER_ONLY, RagonAuthority events = RagonAuthority.ALL);
    public void CreateStaticEntity(ushort type, ushort staticId, IRagonSerializable spawnPayload, RagonAuthority state = RagonAuthority.OWNER_ONLY, RagonAuthority events = RagonAuthority.ALL);
    public void DestroyEntity(int entityId, IRagonSerializable destroyPayload);
    public void ReplicateEntityEvent(ushort evntCode, int entityId, RagonTarget target = RagonTarget.ALL, RagonReplicationMode mode = RagonReplicationMode.SERVER_ONLY);
    public void ReplicateEntityEvent(ushort evntCode, int entityId, IRagonSerializable payload, RagonTarget target = RagonTarget.ALL, RagonReplicationMode mode = RagonReplicationMode.SERVER_ONLY);
    public void ReplicateEvent(ushort evntCode, IRagonSerializable payload, RagonTarget target = RagonTarget.ALL, RagonReplicationMode replicationMode = RagonReplicationMode.SERVER_ONLY);
    public void ReplicateEvent(ushort evntCode, RagonTarget target = RagonTarget.ALL, RagonReplicationMode replicationMode = RagonReplicationMode.SERVER_ONLY);
    public void ReplicateEntityState(int entityId, IRagonSerializable payload);
  }
}