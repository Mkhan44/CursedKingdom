using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet
{
    public class PlayerSpawner : PurrMonoBehaviour
    {
        [SerializeField, HideInInspector] private NetworkIdentity playerPrefab;
        [SerializeField] private GameObject _playerPrefab;

        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        private int _currentSpawnPoint;
        
        private void Awake()
        {
            CleanupSpawnPoints();
        }

        private void CleanupSpawnPoints()
        {
            bool hadNullEntry = false;
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (!spawnPoints[i])
                {
                    hadNullEntry = true;
                    spawnPoints.RemoveAt(i);
                    i--;
                }
            }
            
            if (hadNullEntry)
                PurrLogger.LogWarning($"Some spawn points were invalid and have been cleaned up.", this);
        }

        private void OnValidate()
        {
            if (playerPrefab)
            {
                _playerPrefab = playerPrefab.gameObject;
                playerPrefab = null;
            }
        }

        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
            {
                scenePlayersModule.onPlayerLoadedScene += OnPlayerLoadedScene;

                if (!manager.TryGetModule(out ScenesModule scenes, true))
                    return;
            
                if (!scenes.TryGetSceneID(gameObject.scene, out var sceneID))
                    return;
                
                if (scenePlayersModule.TryGetPlayersInScene(sceneID, out var players))
                {
                    foreach (var player in players)
                        OnPlayerLoadedScene(player, sceneID, true);
                }
            }
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
        }
 
        private void OnDestroy()
        {
            if(NetworkManager.main && NetworkManager.main.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
        }

        private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
        {
            var main = NetworkManager.main;
            
            if (!main || !main.TryGetModule(out ScenesModule scenes, true))
                return;

            var unityScene = gameObject.scene;
            
            if (!scenes.TryGetSceneID(unityScene, out var sceneID))
                return;
            
            if (sceneID != scene)
                return;

            if (!asServer)
                return;

            bool isDestroyOnDisconnectEnabled = main.networkRules.ShouldDespawnOnOwnerDisconnect();
            if (!isDestroyOnDisconnectEnabled && main.TryGetModule(out GlobalOwnershipModule ownership, true) && 
                ownership.PlayerOwnsSomething(player))
                return;
            
            GameObject newPlayer;
            
            CleanupSpawnPoints();
            
            if (spawnPoints.Count > 0)
            {
                var spawnPoint = spawnPoints[_currentSpawnPoint];
                _currentSpawnPoint = (_currentSpawnPoint + 1) % spawnPoints.Count;
                newPlayer = UnityProxy.Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation, unityScene);
            }
            else
            {
                var instance = Instantiate(_playerPrefab);
                SceneManager.MoveGameObjectToScene(instance, unityScene);
                newPlayer = instance;
            }
            
            if (newPlayer.TryGetComponent(out NetworkIdentity identity))
                identity.GiveOwnership(player);
        }
    }
}
