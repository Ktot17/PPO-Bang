using System;
using System.Collections.Generic;
using System.Linq;
using BLComponent.OutputPort;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bang
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject playCanvas;
        [SerializeField] private GameObject loadCanvas;
        [SerializeField] private GameObject errorCanvas;
        [SerializeField] private TMP_InputField addPlayerField;
        [SerializeField] private Transform players;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform savesContent;
        [SerializeField] private GameObject savePrefab;
        [SerializeField] private Transform placeHolder;
        [SerializeField] private TextMeshProUGUI errorText;
        
        private IGameManager _gameManager;
        private readonly List<GameObject> _players = new();
        private GameObject _lastActiveCanvas;

        protected void Awake()
        {
            _lastActiveCanvas = menuCanvas;
            _gameManager = DataCarrier.GameManager;
            for (var i = 0; i < BLComponent.GameManager.MaxPlayersCount; ++i)
                _players.Add(Instantiate(playerPrefab, placeHolder));
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void PlayGame()
        {
            menuCanvas.SetActive(false);
            playCanvas.SetActive(true);
            _lastActiveCanvas = playCanvas;
        }

        public void LoadGame()
        {
            var saves = _gameManager.GetAllSaves;
            for (var i = 0; i < savesContent.childCount; ++i)
                Destroy(savesContent.GetChild(i).gameObject);
            foreach (var save in saves)
            {
                var saveGo = Instantiate(savePrefab, savesContent, false);
                saveGo.GetComponentInChildren<TextMeshProUGUI>().text =
                    DateTimeOffset.FromUnixTimeSeconds(save.Value).ToLocalTime().ToString();
                saveGo.GetComponentInChildren<Button>().onClick.AddListener(() => LoadSave(save.Key));
            }
            menuCanvas.SetActive(false);
            loadCanvas.SetActive(true);
            _lastActiveCanvas = loadCanvas;
        }

        public void LoadSave(int id)
        {
            _gameManager.LoadState(id);
            DataCarrier.Players.Clear();
            SceneManager.LoadScene("GameScene");
        }

        public void BackToMenu()
        {
            playCanvas.SetActive(false);
            loadCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            _lastActiveCanvas = menuCanvas;
        }

        public void OpenError(string errorString)
        {
            errorText.text = errorString;
            _lastActiveCanvas.SetActive(false);
            errorCanvas.SetActive(true);
        }

        public void CloseError()
        {
            errorCanvas.SetActive(false);
            _lastActiveCanvas.SetActive(true);
        }

        public void AddPlayer()
        {
            if (DataCarrier.Players.Count >= BLComponent.GameManager.MaxPlayersCount)
            {
                OpenError($"Больше {BLComponent.GameManager.MaxPlayersCount} игроков добавить нельзя.");
                return;
            }
            
            if (DataCarrier.Players.Contains(addPlayerField.text))
            {
                OpenError($"Игрок с именем {addPlayerField.text} уже есть.");
                return;
            }

            if (addPlayerField.text == "")
            {
                OpenError("Нужно ввести имя игрока, чтобы добавить его в список.");
                return;
            }
            
            DataCarrier.Players.Add(addPlayerField.text);
            var player = _players.First(p => 
                p.GetComponentInChildren<TextMeshProUGUI>().text == "");
            player.transform.SetParent(players);
            var playerName = player.GetComponentInChildren<TextMeshProUGUI>();
            playerName.text = addPlayerField.text;
            player.GetComponentInChildren<Button>().onClick.AddListener(() => DeletePlayer(playerName.text));
            addPlayerField.text = "";
        }

        public void DeletePlayer(string playerName)
        {
            DataCarrier.Players.Remove(playerName);
            var player = _players.First(p => 
                p.GetComponentInChildren<TextMeshProUGUI>().text == playerName);
            player.transform.SetParent(placeHolder);
            player.GetComponentInChildren<TextMeshProUGUI>().text = "";
            player.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        }

        public void StartGame()
        {
            if (DataCarrier.Players.Count < BLComponent.GameManager.MinPlayersCount)
            {
                OpenError($"Для начала игры необходимо минимум {BLComponent.GameManager.MinPlayersCount} игрока.");
                return;
            }

            SceneManager.LoadScene("GameScene");
        }
    }
}
