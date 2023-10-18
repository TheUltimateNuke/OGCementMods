using UnityEngine;
using CementTools;
using UnityEngine.SceneManagement;
using GB.Gamemodes;
using System.Reflection;
using GB.UI;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using CementTools.Modules.SceneModule;
using GB.Platform.Lobby;
using GB.UI.Beasts;
using DG.Tweening;

namespace BetterMapSelection
{

    public class GameData
    {
        public GameMode gameMode;
        public List<string> mapNames = new List<string>();
        public int mapCount;

        public bool votedForMapCount;
        public bool votedForGameMode;
    }

    public class BetterMapSelectionMod : CementMod
    {

        private const float ANIMATION_SPEED = 3.5f;
        private static BetterMapSelectionMod _singleton;
        public static BetterMapSelectionMod Singleton
        {
            get
            {
                return _singleton;
            }
        }

        private GameObject _mapBitPrefab;

        private GameObject _mapGridPrefab;
        private GameObject _mapGrid;
        private bool _addedBaseMaps = false;
        private List<MapUIBit> _mapBits = new List<MapUIBit>();
        private GameObject _mapSelectionUIPrefab;
        private GameObject _activeMapUI;
        private VotingSystem _currentVotingSystem;
        private GameData _currentGameData;
        private Transform _activeSelectedValues;
        private bool _menuLoadedBefore;
        private bool _playersReady = false;
        private LocalBeastSetupTracker _tracker;
        private MenuHandlerGamemodes _menuHandler;
        private GameObject _localBeastMenu;

        private void Awake()
        {
            _singleton = this;
            SceneManager.sceneLoaded += OnSceneChanged;
        }

        private void Start()
        {
            modFile.ChangedValues += delegate ()
            {
                if (modFile.GetBool("UseCustomMenu"))
                {
                    DisableUI(GetCanvas().transform);
                }
                else
                {
                    EnableUI();
                }
            };
        }

        private void SetupValuesFromAssetBundles()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(modDirectoryPath, "bettermapselection"));
            _mapSelectionUIPrefab = assetBundle.LoadAsset<GameObject>("BetterMapSelectionUI");
            _mapBitPrefab = assetBundle.LoadAsset<GameObject>("MapBit");
            MapImages.images = assetBundle.LoadAllAssets<Sprite>();
            BMSResources.actorGraphic = assetBundle.LoadAsset<GameObject>("ActorGraphic");
            BMSResources.defaultImage = assetBundle.LoadAsset<Sprite>("Default");
            assetBundle.Unload(false);

            _mapGrid = _mapSelectionUIPrefab.transform.Find("MapGrid").gameObject;
            DontDestroyOnLoad(_mapSelectionUIPrefab);
        }

        private GameModeSetupConfiguration GetGameModeSetupConfiguration()
        {
            FieldInfo trackerInfo = typeof(MenuHandlerGamemodes).GetField("tracker", BindingFlags.NonPublic | BindingFlags.Instance);
            return trackerInfo.GetValue(FindObjectOfType<MenuHandlerGamemodes>()) as GameModeSetupConfiguration;

        }

        private void _AddMap(string mapName, Sprite mapImage)
        {
            if (mapImage == null)
            {
                mapImage = BMSResources.defaultImage;
            }
            MapUIBit mapBit = Instantiate(_mapBitPrefab).AddComponent<MapUIBit>();
            mapBit.transform.parent = _mapGrid.transform;
            mapBit.UpdateMap(mapName, mapImage);

            _mapBits.Add(mapBit);
        }

        public static void AddMap(string mapName, Sprite mapImage)
        {
            _singleton._AddMap(mapName, mapImage);
        }

        private void DisableMenuHandler()
        {
            IEnumerator _()
            {
                yield return new WaitUntil(() => FindObjectOfType<MenuHandlerGamemodes>() != null);
                _menuHandler = FindObjectOfType<MenuHandlerGamemodes>();
                _menuHandler.enabled = false;
            }

            StartCoroutine(_());
        }

        private void AddBaseMaps(Transform canvasParent)
        {
            IEnumerator _AddBaseMaps()
            {
                Cement.Log("Waiting for Menu Handler Gamemodes");
                yield return new WaitUntil(() => FindObjectOfType<MenuHandlerGamemodes>() != null);

                GameModeSetupConfiguration gmsc = GetGameModeSetupConfiguration();
                foreach (ModeMapStatus map in gmsc.Maps.AvailableMaps)
                {
                    if (map.MapName == "Alley")
                    {
                        continue;
                    }
                    Cement.Log($"ADDING BASE MAP {map.MapName}");
                    _AddMap(map.MapName, MapImages.BaseMapNameToSprite(map.MapName));
                }

                _addedBaseMaps = true;
                Cement.Log("Added base maps!");
            }

            StartCoroutine(_AddBaseMaps());
        }

        private GameObject GetCanvas()
        {
            return GameObject.Find("Beast Menu").transform.Find("Canvas").gameObject;
        }

        private void SpawnCanvas(Transform parent)
        {
            Cement.Log("Spawning in canvas!");
            _activeMapUI = Instantiate(_mapSelectionUIPrefab, parent);
            _activeMapUI.transform.eulerAngles = new Vector3(0, -90, 0);
            _activeMapUI.transform.localPosition = new Vector3(0, 220, 0);
            _activeMapUI.transform.localScale = Vector3.one * 2.4f;

            _activeSelectedValues = _activeMapUI.transform.Find("SelectedStuff");
        }

        string[] _childrenToDisable = new string[]
        {
            "Wins/HoriSort", "Maps", "StartGame", "Ganemodes/HoriSort"
        };
        private void DisableUI(Transform canvas)
        {
            Transform parent = canvas.Find("Local Beast Select Menu/UI/GameModeSelection");
            foreach (string child in _childrenToDisable)
            {
                parent.Find(child).gameObject.SetActive(false);
            }
        }

        private void EnableUI()
        {
            Transform parent = _localBeastMenu.transform.Find("UI/GameModeSelection");
            foreach (string child in _childrenToDisable)
            {
                parent.Find(child).gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            EnableUI();
        }

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name == "Menu")
                OnMenuLoad();
        }

        private void OnMenuLoad()
        {
            _playersReady = false;

            if (!_menuLoadedBefore)
            {
                SetupValuesFromAssetBundles();
                _tracker = FindObjectOfType<LocalBeastSetupTracker>();
                _menuLoadedBefore = true;
            }

            _currentGameData = new GameData();

            GameObject canvas = GetCanvas();
            Cement.Log("Trying to disable ui...");
            DisableUI(canvas.transform);
            Cement.Log("Adding base maps if haven't already...");
            if (!_addedBaseMaps)
            {
                AddBaseMaps(canvas.transform);
            }
            SpawnCanvas(canvas.transform);

            DisableMenuHandler();
        }

        private void OnSceneChanged(Scene scene, LoadSceneMode _)
        {
            if (!enabled)
            {
                return;
            }

            if (scene.name == "Menu")
                OnMenuLoad();
        }

        private void CreateSelectedBit(int result)
        {
            MapUIBit mapBit = Instantiate(_mapBitPrefab).AddComponent<MapUIBit>();
            mapBit.transform.SetParent(_activeSelectedValues.transform, false);

            string mapName = _currentVotingSystem.mapBits[result].GetName();
            Sprite mapImage = _currentVotingSystem.mapBits[result].GetImage();

            mapBit.UpdateMap(mapName, mapImage);

            mapBit.transform.localScale = Vector3.zero;
            mapBit.transform.DOScale(Vector3.one, 1f / ANIMATION_SPEED);
        }

        private Tweener AnimateCurrentGridOff()
        {
            Transform currentGrid = _currentVotingSystem.mapBits[0].transform.parent;
            return currentGrid.DOScale(Vector3.zero, 1f / ANIMATION_SPEED);
        }

        private void AnimateGridOn(GameObject grid)
        {
            grid.SetActive(true);
            grid.transform.localScale = Vector3.zero;
            grid.transform.DOScale(Vector3.one, 1f / ANIMATION_SPEED).OnComplete(delegate ()
            {
                CreateVotingSystem(grid);
            });
        }

        private void OnVotingEnded(int result)
        {
            AnimateCurrentGridOff().OnComplete(() => HandleNextVotingSystem(result));
            CreateSelectedBit(result);
        }

        private void HandleNextVotingSystem(int result)
        {
            if (!_currentGameData.votedForGameMode)
            {
                _currentGameData.votedForGameMode = true;
                _currentGameData.gameMode = (GameMode)(_currentVotingSystem.mapBits[result] as GameModeUIBit).gameModeEnumInt;

                GameObject activeMapCountGrid = _activeMapUI.transform.Find("MapCountGrid").gameObject;
                AnimateGridOn(activeMapCountGrid);
            }
            else if (!_currentGameData.votedForMapCount)
            {
                _currentGameData.votedForMapCount = true;
                _currentGameData.mapCount = (_currentVotingSystem.mapBits[result] as MapCountUIBit).mapCount;

                GameObject activeMapGrid = _activeMapUI.transform.Find("MapGrid").gameObject;
                AnimateGridOn(activeMapGrid);
            }
            else if (_currentGameData.mapNames.Count < _currentGameData.mapCount - 1)
            {
                _currentGameData.mapNames.Add(_currentVotingSystem.mapBits[result].GetName());

                GameObject activeMapGrid = _activeMapUI.transform.Find("MapGrid").gameObject;
                AnimateGridOn(activeMapGrid);
            }
            else
            {
                _menuHandler.enabled = true;
                CustomSceneManager.StartCustomGame(new CustomRotationConfig(
                    _currentGameData.mapNames.ToArray(),
                    _currentGameData.mapNames.Count,
                    _currentGameData.gameMode,
                    false,
                    5 * 3600
                ));
            }
        }

        private void CreateVotingSystem(GameObject grid)
        {
            grid.gameObject.SetActive(true);
            foreach (Transform bit in grid.transform)
            {
                if (bit.GetComponent<MapUIBit>() == null)
                {
                    bit.gameObject.AddComponent<MapUIBit>();
                }
            }
            MapUIBit[] bits = grid.GetComponentsInChildren<MapUIBit>();
            _currentVotingSystem = new VotingSystem(10f, bits, Mathf.Min(6, bits.Length), _activeMapUI.transform);
            _currentVotingSystem.VotingEnded += OnVotingEnded;
        }

        private void StartVoting()
        {
            GameObject activeGameModeGrid = _activeMapUI.transform.Find("GameModeGrid").gameObject;
            CreateVotingSystem(activeGameModeGrid);
        }

        private bool IsLocalLobby()
        {
            return _localBeastMenu.activeSelf;
        }

        // returns true if succeeded
        // returns false if failed
        private bool TrySettingLocalBeastMenu()
        {
            GameObject canvas = GetCanvas();
            if (canvas == null)
            {
                return false;
            }
            _localBeastMenu = canvas.transform.Find("Local Beast Select Menu").gameObject;
            if (_localBeastMenu == null)
            {
                return false;
            }

            return true;
        }

        private void Update()
        {
            if (!modFile.GetBool("UseCustomMenu"))
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != "Menu")
            {
                return;
            }

            // will return if failed
            if (_localBeastMenu == null && !TrySettingLocalBeastMenu())
            {
                return;
            }
        
            if (_tracker == null)
            {
                _tracker = FindObjectOfType<LocalBeastSetupTracker>();
                if (_tracker == null)
                {
                    return;
                }
            }

            if (IsLocalLobby())
            {
                if (_playersReady)
                {
                    _tracker.ForceAllAToB(BeastUtils.PlayerState.Designing, BeastUtils.PlayerState.Ready);
                }
                else
                {
                    _playersReady = _tracker.AllActiveBeastsReady();
                    if (_playersReady)
                    {
                        StartVoting();
                    }
                }
            }

            if (_currentVotingSystem != null)
            {
                _currentVotingSystem.Tick(Time.deltaTime);
            }
        }
    }
}