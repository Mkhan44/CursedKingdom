using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PurrNet.Editor
{
    public static class UrlTexture
    {
        static readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        static readonly HashSet<string> _loading = new HashSet<string>();

        public static Texture2D TryGetTexture(string url)
        {
            if (_textures.TryGetValue(url, out var texture))
                return texture;

            if (_loading.Add(url))
                LoadTexture(url);
            return null;
        }

        static async void LoadTexture(string url)
        {
            try
            {
                var request = UnityWebRequestTexture.GetTexture(url);
                await request.SendWebRequest();

                if (request.error != null)
                {
                    Debug.LogError($"Failed to load texture from {url}: {request.error}");
                    return;
                }

                _textures[url] = ((DownloadHandlerTexture)request.downloadHandler).texture;
                _loading.Remove(url);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load texture from {url}: {e}");
            }
        }
    }
}