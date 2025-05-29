using System;
using System.Globalization;
using System.Linq;
using BLComponent;
using TMPro;
using UnityEngine;

namespace Bang
{
    public class Player : MonoBehaviour
    {
        public Transform hand;
        public Transform board;
        [SerializeField]
        private GameObject enemyInfo;
        [SerializeField]
        private GameObject playerNameText;
        [SerializeField]
        private GameObject sheriffStar;
        [SerializeField] 
        private TextMeshProUGUI playerRoleText;
        [SerializeField]
        private GameObject healthText;
        [SerializeField]
        private GameObject cardsCountText;
        [SerializeField]
        private Transform cardsOnBoard;
        [SerializeField]
        private GameManager gameManager;

        public Guid Id { get; private set; } = Guid.Empty;
        public string Name { get; private set; } = string.Empty;
        public bool IsPointerInside { get; private set; }
        public bool IsChoosing { get; private set; }
        private string _healthFormat;
        private string _cardsCountFormat;
        
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _playerNameText;

        public void InitId(Guid id)
        {
            Id = id;
            Name = gameManager.Players.First(p => p.Id == Id).Name;
        }
        
        private void Awake()
        {
            _healthFormat = healthText.GetComponent<TextMeshProUGUI>().text;
            _cardsCountFormat = cardsCountText.GetComponent<TextMeshProUGUI>().text;
            
            _playerNameText = playerNameText.GetComponent<TextMeshProUGUI>();
            _healthText = healthText.GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (Id != gameManager.CurrentPlayerId)
                return;
            var player = gameManager.Players.First(p => p.Id == Id);
            _playerNameText.text = player.Name;
            _healthText.text = string.Format(CultureInfo.InvariantCulture,
                _healthFormat, player.Health, player.MaxHealth);
            playerRoleText.text = DataCarrier.PlayerRoles[player.Role];
        }

        public void OnPointerEnter()
        {
            if (Id == Guid.Empty || Id == gameManager.CurrentPlayerId || IsChoosing)
                return;
            IsPointerInside = true;
            var player = gameManager.Players.First(p => p.Id == Id);
            var pName = player.Name;
            if (player.IsDead)
                pName += $"\nРоль: {DataCarrier.PlayerRoles[player.Role]}";
            playerNameText.GetComponent<TextMeshProUGUI>().text = pName;
            healthText.GetComponent<TextMeshProUGUI>().text = string.Format(CultureInfo.InvariantCulture,
                _healthFormat, player.Health, player.MaxHealth);
            cardsCountText.GetComponent<TextMeshProUGUI>().text = string.Format(CultureInfo.InvariantCulture,
                _cardsCountFormat, player.CardsInHand.Count);
            foreach (var card in player.CardsOnBoard)
            {
                GameState.Cards[card.Id].transform.SetParent(cardsOnBoard);
            }

            if (player.Weapon is not null)
            {
                GameState.Cards[player.Weapon.Id].transform.SetParent(cardsOnBoard);
            }
            enemyInfo.SetActive(true);
            sheriffStar.SetActive(player.Role is PlayerRole.Sheriff);
        }
        
        public void OnPointerExit()
        {
            if (Id == Guid.Empty || Id == gameManager.CurrentPlayerId || IsChoosing)
                return;
            var player = gameManager.Players.First(p => p.Id == Id);
            foreach (var card in player.CardsOnBoard)
            {
                GameState.Cards[card.Id].transform.SetParent(board);
            }
            if (player.Weapon is not null)
            {
                GameState.Cards[player.Weapon.Id].transform.SetParent(board);
            }
            IsPointerInside = false;
            enemyInfo.SetActive(false);
        }

        public void Choosing()
        {
            IsChoosing = !IsChoosing;
        }
    }
}
