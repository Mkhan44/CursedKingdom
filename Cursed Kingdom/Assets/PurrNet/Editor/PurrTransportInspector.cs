using System;
using PurrNet.Transports;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(PurrTransport), true)]
    public class PurrTransportInspector : UnityEditor.Editor
    {
        private SerializedProperty _masterServer;
        private SerializedProperty _roomName;
        private SerializedProperty _region;
        private SerializedProperty _host;
        
        private bool _lookingForBestRegion;
        static string[] _regions = Array.Empty<string>();
        static string[] _hosts = Array.Empty<string>();
 
        void OnEnable()
        {
            _masterServer = serializedObject.FindProperty("_masterServer");
            _roomName = serializedObject.FindProperty("_roomName");
            _region = serializedObject.FindProperty("_region");
            _host = serializedObject.FindProperty("_host");
            
            if (_regions.Length == 0)
                LoadRegions();
        }

        public static string _bestRegion;
        static bool _loadingRegions;
        
        async void LoadRegions()
        {
            try
            {
                if (_loadingRegions)
                    return;
                
                _loadingRegions = true;
                var servers = await PurrTransportUtils.GetRelayServersAsync(_masterServer.stringValue);
                
                _hosts = new string[servers.servers.Length];
                _regions = new string[servers.servers.Length];

                for (var i = 0; i < servers.servers.Length; i++)
                {
                    _hosts[i] = servers.servers[i].host;
                    _regions[i] = servers.servers[i].region;
                }
                
                _loadingRegions = false;
            }
            catch (Exception e)
            {
                _loadingRegions = false;
                Debug.LogException(e);
            }
        }
        
        static int RegionId(string region, string host)
        {
            for (var i = 0; i < _regions.Length; i++)
            {
                if (_regions[i] == region)
                {
                    if (_hosts[i] != host)
                        return -1;
                    return i;
                }
            }

            return -1;
        }

        private async void FindBestRegion()
        {
            try
            {
                if (_lookingForBestRegion)
                    return;
                
                _lookingForBestRegion = true;
            
                var server = await PurrTransportUtils.GetRelayServerAsync(_masterServer.stringValue);
            
                _region.stringValue = server.region;
                _bestRegion = server.region;
                
                serializedObject.ApplyModifiedProperties();
            
                _lookingForBestRegion = false;
            }
            catch (Exception e)
            {
                _lookingForBestRegion = false;
                Debug.LogException(e);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            
            var transport = (PurrTransport)target;
            
            EditorGUILayout.PropertyField(_masterServer);
            
            var server = _masterServer.stringValue;
            if (Uri.TryCreate(server, UriKind.Absolute, out var url) && url.Host.EndsWith("riten.dev"))
            {
                // draw help box saying this is meant for dev use only
                EditorGUILayout.HelpBox("This server is meant for development use only.\n" +
                                        "Usage in production is strictly prohibited.\n" +
                                        "You need to host your own relay servers for production.", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(_roomName);
            
            bool oldEnabled = GUI.enabled;
            if (_lookingForBestRegion)
                GUI.enabled = false;
            
            EditorGUILayout.BeginHorizontal();
            
            if (_regions.Length == 0)
            {
                bool enabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_region);
                GUI.enabled = enabled;
            }
            else
            {
                int region = RegionId(transport.region, transport.host);
                var newRegion = EditorGUILayout.Popup("Region", region, _regions);
                
                if (newRegion < 0 && _regions.Length > 0)
                    newRegion = 0;

                if (region != newRegion && newRegion >= 0 && newRegion < _regions.Length)
                {
                    _region.stringValue = _regions[newRegion];
                    _host.stringValue = _hosts[newRegion];
                }
            }
            
            if (GUILayout.Button("Find Best Region", GUILayout.ExpandWidth(false)))
                FindBestRegion();

            GUI.enabled = oldEnabled;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.Label(_host.stringValue);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            TransportInspector.DrawTransportStatus(transport);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
