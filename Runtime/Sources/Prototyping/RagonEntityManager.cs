using System;
using System.Collections.Generic;
using Ragon.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragon.Client.Prototyping
{
  public struct PrefabRequest
  {
    public ushort Type;
    public bool IsOwned;
  }

  [DefaultExecutionOrder(-10000)]
  public class RagonEntityManager : MonoBehaviour
  {
    [Range(1.0f, 60.0f, order = 0)] public float ReplicationRate = 1.0f;
    [SerializeField] private RagonPrefabRegistry _prefabRegistry;
    
    public static RagonEntityManager Instance { get; private set; }
    
    private Dictionary<int, RagonEntity> _entitiesDict = new Dictionary<int, RagonEntity>();
    private Dictionary<int, RagonEntity> _entitiesStatic = new Dictionary<int, RagonEntity>();
    
    private List<RagonEntity> _entitiesList = new List<RagonEntity>();
    private List<RagonEntity> _entitiesOwned = new List<RagonEntity>();

    private Func<PrefabRequest, GameObject> _prefabCallback;

    private RagonSerializer _serializer = new RagonSerializer();
    private float _replicationTimer = 0.0f;
    private float _replicationRate = 0.0f;

    private void Awake()
    {
      Instance = this;
      
      _prefabRegistry.Cache();
      _replicationTimer = 1000.0f / ReplicationRate;
    }

    public void CollectSceneData()
    {
      var gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
      var objs = new List<RagonEntity>();
      foreach (var go in gameObjects)
      {
        if (go.TryGetComponent<RagonEntity>(out var ragonObject))
        {
          objs.Add(ragonObject);
        }
      }
      
      Debug.Log("Found static entities: " + objs.Count);
      
      ushort staticId = 1;
      foreach (var entity in objs)
      {
        staticId += 1;
        _entitiesStatic.Add(staticId, entity);
        
        if (RagonNetwork.Room.LocalPlayer.IsRoomOwner)
          RagonNetwork.Room.CreateStaticEntity(entity.gameObject, staticId, null);
      }
    }

    public void Cleanup()
    {
      foreach (var ent in _entitiesList)
        ent.Detach(Array.Empty<byte>());

      _entitiesDict.Clear();
      _entitiesList.Clear();
      _entitiesOwned.Clear();
      _entitiesStatic.Clear();
    }

    public void Update()
    {
      
    }

    public void FixedUpdate()
    {
      _replicationTimer += Time.fixedTime;
      if (_replicationTimer > _replicationRate)
      {
        foreach (var ent in _entitiesOwned)
        {
          if (ent.AutoReplication)
            ent.ReplicateState(_serializer);
        }

        _replicationTimer = 0.0f; 
      }
    }

    public void OnEntityStaticCreated(int entityId, ushort staticId, ushort entityType, RagonAuthority state, RagonAuthority evnt, RagonPlayer creator, byte[] payload)
    {
      if (_entitiesStatic.Remove(staticId, out var ragonEntity))
      {
        ragonEntity.RetrieveProperties();
        ragonEntity.Attach(entityType, creator, entityId, payload);

        _entitiesDict.Add(entityId, ragonEntity);
        _entitiesList.Add(ragonEntity);

        if (creator.IsMe)
          _entitiesOwned.Add(ragonEntity);
      }
    }

    public void OnEntityCreated(int entityId, ushort entityType, RagonAuthority state, RagonAuthority evnt, RagonPlayer creator, byte[] payload)
    {
      var prefab = _prefabRegistry.Prefabs[entityType];
      var go = Instantiate(prefab); 

      var component = go.GetComponent<RagonEntity>();
      component.RetrieveProperties();
      component.Attach(entityType, creator, entityId, payload);

      _entitiesDict.Add(entityId, component);
      _entitiesList.Add(component);

      if (creator.IsMe)
        _entitiesOwned.Add(component);
    }

    public void OnEntityDestroyed(int entityId, byte[] payload)
    {
      if (_entitiesDict.Remove(entityId, out var ragonEntity))
      {
        _entitiesList.Remove(ragonEntity);

        if (_entitiesOwned.Contains(ragonEntity))
          _entitiesOwned.Remove(ragonEntity);

        ragonEntity.Detach(payload);
      }
    }

    public void OnEntityEvent(RagonPlayer player, int entityId, ushort evntCode, RagonSerializer payload)
    {
      if (_entitiesDict.TryGetValue(entityId, out var ent))
        ent.ProcessEvent(player, evntCode, payload);
    }

    public void OnEntityState(int entityId, RagonSerializer payload)
    {
      if (_entitiesDict.TryGetValue(entityId, out var ent))
        ent.ProcessState(payload);
    }

    public void OnOwnerShipChanged(RagonPlayer player)
    {
      foreach (var ent in _entitiesList)
        ent.ChangeOwner(player);
    }
  }
}