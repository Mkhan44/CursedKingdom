using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Octokit;
using PurrNet.Logging;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace PurrNet.Editor
{
    public struct PurrNetEntry
    {
        public string name;
        public string url;
        public string query;
        public string fragment;

        public override string ToString()
        {
            return $"{name} -> {url}{query}{fragment}";
        }
    }

    public class PurrNetBranchManager : EditorWindow
    {
        [MenuItem("Tools/PurrNet/Version Manager")]
        public static void ShowWindow()
        {
            GetWindow<PurrNetBranchManager>("PurrNet Version Manager");
        }

        static readonly Uri repositoryUrl = new Uri("https://github.com/BlenMiner/PurrNet");

        private bool _usingBranches;
        private GitHubClient _client;
        private PurrNetEntry? _purrnetEntry;

        private readonly List<Branch> _branches = new List<Branch>();
        private readonly List<Release> _releases = new List<Release>();
        private readonly List<RepositoryContributor> _contributors = new List<RepositoryContributor>();

        private Texture2D _logo;

        private string[] _optionsActual = Array.Empty<string>();
        private string[] _options = Array.Empty<string>();

        static bool TryGetPurrnetEntry(out PurrNetEntry entry)
        {
            const string PATH = "Packages/manifest.json";
            var manifestString = File.ReadAllText(PATH);

            var manifest = (JObject)JObject.Parse(manifestString)["dependencies"];
            if (manifest != null && manifest.TryGetValue("purrnet", out var purrnet))
            {
                var value = purrnet.Value<string>();
                var url = new Uri(value);
                entry = new PurrNetEntry
                {
                    name = "purrnet",
                    url = $"{url.Scheme}://{url.Host}/{url.AbsolutePath}",
                    query = url.Query,
                    fragment = url.Fragment[1..]
                };
                return true;
            }

            entry = default;
            return false;
        }

        static string GetToken()
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Process);

            if (!string.IsNullOrEmpty(token))
                return token;

            token = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.User);

            if (!string.IsNullOrEmpty(token))
                return token;

            token = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Machine);

            if (!string.IsNullOrEmpty(token))
                return token;

            return null;
        }

        private void OnEnable()
        {
            _isRefreshingBranches = false;
            _isRefreshingReleases = false;
            _isRefreshingContributors = false;

            _logo = Resources.Load<Texture2D>("purrlogo");
            if (TryGetPurrnetEntry(out var entry))
                _purrnetEntry = entry;
            else _purrnetEntry = null;

            _client ??= new GitHubClient(new ProductHeaderValue("purrnet"), repositoryUrl);
            var token = GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                var credentials = new Credentials(token);
                _client.Credentials = credentials;
            }

            RefreshBranches();
            RefreshReleases();
            RefreshContributors();
        }

        private bool _isRefreshingBranches;

        private async void RefreshBranches()
        {
            try
            {
                if (_isRefreshingBranches)
                    return;

                _isRefreshingBranches = true;

                var branches = await _client.Repository.Branch.GetAll("BlenMiner", "PurrNet");
                _branches.Clear();

                foreach (var branch in branches)
                    _branches.Add(branch);

                _isRefreshingBranches = false;
            }
            catch (Exception e)
            {
                _isRefreshingBranches = false;
                PurrLogger.LogError(e.Message);
            }
            finally
            {
                RefreshOptions();
                Repaint();
            }
        }

        private bool _isRefreshingReleases;

        private async void RefreshReleases()
        {
            try
            {
                if (_isRefreshingReleases)
                    return;

                _isRefreshingReleases = true;

                var releases = await _client.Repository.Release.GetAll("BlenMiner", "PurrNet", new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 100
                });

                _releases.Clear();

                foreach (var release in releases)
                    _releases.Add(release);
            }
            catch (Exception e)
            {
                PurrLogger.LogError(e.Message);
            }
            finally
            {
                _isRefreshingReleases = false;
                RefreshOptions();
                Repaint();
            }
        }

        private bool _isRefreshingContributors;

        private async void RefreshContributors()
        {
            try
            {
                if (_isRefreshingContributors)
                    return;

                _isRefreshingContributors = true;

                var contributors = await _client.Repository.GetAllContributors(
                    "BlenMiner", "PurrNet");

                _contributors.Clear();

                foreach (var contributor in contributors)
                {
                    bool isBot = contributor.Login is "dependabot[bot]" or "semantic-release-bot";
                    if (!isBot) _contributors.Add(contributor);
                }
            }
            catch (Exception e)
            {
                PurrLogger.LogError(e.Message);
            }
            finally
            {
                _isRefreshingContributors = false;
                Repaint();
            }
        }

        private void RefreshOptions()
        {
            var options = new List<string>();
            var optionsReal = new List<string>();

            foreach (var branch in _branches)
            {
                options.Add($"Branch/{branch.Name}");
                optionsReal.Add(branch.Name);
            }

            foreach (var release in _releases)
            {
                options.Add(release.Prerelease ? $"PreRelease/{release.Name}" : $"Release/{release.Name}");

                optionsReal.Add(release.Name);
            }

            _options = options.ToArray();
            _optionsActual = optionsReal.ToArray();
        }

        private Vector2 _scrollPosition;

        private int GetCurrentIndex(string version)
        {
            for (int i = 0; i < _optionsActual.Length; i++)
            {
                if (_optionsActual[i] == version)
                    return i;
            }

            return -1;
        }

        private string _targetVersion;

        private void OnGUI()
        {
            bool anyRefreshing = _isRefreshingBranches || _isRefreshingReleases || _isRefreshingContributors;

            GUI.DrawTexture(new Rect(10, 10, 64, 64), _logo);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10 + 64 + 10);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            if (_purrnetEntry == null)
            {
                EditorGUILayout.LabelField("PurrNet not found in manifest.json", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Add this git package from url:", EditorStyles.wordWrappedLabel);
                EditorGUILayout.TextField("https://github.com/BlenMiner/PurrNet.git?path=/Assets/PurrNet#release");
                EditorGUILayout.HelpBox("Make sure you only have one installation of PurrNet in your project.\n" +
                                        "Since you are seeing this message, you already have it installed somewhere.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"PurrNet Version - {_purrnetEntry?.query}#{_purrnetEntry?.fragment}",
                    EditorStyles.boldLabel);

                string currentFragment = _targetVersion ?? _purrnetEntry?.fragment ?? "release";
                int actual = GetCurrentIndex(currentFragment);
                if (actual == -1)
                {
                    actual = GetCurrentIndex("release");
                }

                int newActual = EditorGUILayout.Popup(actual, _options);

                if (newActual != actual)
                    _targetVersion = _optionsActual[newActual];

                DrawButtons(anyRefreshing);
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("PurrNet Contributors", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false,
                GUILayout.MaxWidth(Screen.width - (10 + 64 + 10 + 20)));
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            foreach (var c in _contributors)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                var texture = UrlTexture.TryGetTexture(c.AvatarUrl);

                if (texture != null)
                {
                    GUILayout.Label(string.Empty, GUILayout.Width(64), GUILayout.Height(64));
                    var rect = GUILayoutUtility.GetLastRect();
                    var outlineRect = new Rect(rect);
                    outlineRect.x -= 2;
                    outlineRect.y -= 2;
                    outlineRect.width += 4;
                    outlineRect.height += 4;

                    // draw outline
                    GUI.DrawTexture(outlineRect, Texture2D.whiteTexture, ScaleMode.ScaleToFit, false,
                        0f, Color.white, 0f, 32f);

                    GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, false,
                        0f, Color.white, 0f, 32f);

                    // Make the texture clickable
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) // Invisible button over the texture
                        Application.OpenURL(c.HtmlUrl);

                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                }

                if (EditorGUILayout.LinkButton(c.Login))
                    Application.OpenURL(c.HtmlUrl);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(20);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10 + 64 + 10);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons(bool anyRefreshing)
        {
            EditorGUILayout.BeginHorizontal();

            if (anyRefreshing)
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = false;
                GUILayout.Button("Refreshing...");
                GUI.enabled = wasEnabled;
            }
            else
            {
                bool hasNewPendingChanges = _targetVersion != _purrnetEntry?.fragment;
                if (hasNewPendingChanges)
                    GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Update"))
                {
                    // Update the PurrNet Entry
                    if (_targetVersion != _purrnetEntry?.fragment)
                    {
                        _targetVersion = null;
                    }
                }

                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                RefreshBranches();
                RefreshReleases();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}