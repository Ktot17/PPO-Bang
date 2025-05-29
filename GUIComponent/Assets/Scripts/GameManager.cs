using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using BLComponent;
using BLComponent.OutputPort;
using TMPro;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bang
{
    public static class GameState
    {
        public static Transform Deck;
        public static Transform DiscardPile;
        public static IReadOnlyDictionary<Guid, Card> Cards;
        public static IReadOnlyList<Player> Players;
        public static Guid CurrentPlayer;
        public static GameObject Aim;
        public static Transform CardSelection;
        public static GameObject YesOrNo;

        public static void PlayerChoosing(bool isPlayer)
        {
            foreach (var card in Cards.Values)
                card.PlayerChoosing();
            if (isPlayer)
                return;
            foreach (var player in Players)
            {
                if (player.IsPointerInside)
                    player.OnPointerExit();
                player.Choosing();
            }
        }
    }
    
    public sealed class GameView : IGameView
        {
            private static Guid _playerId = Guid.Empty;
            private static Guid _cardId = Guid.Empty;
            
            private class PlayerSelection : MonoBehaviour
            {
                private List<Player> _players;
                
                public void PlayersInit(List<Player> players)
                {
                    _players = players;
                }
                
                private void Update()
                {
                    if (!Input.GetMouseButtonDown(0))
                        return;

                    foreach (var player in _players.Where(player => player.IsPointerInside))
                    {
                        _playerId = player.Id;
                    }
                }
            }

            private class CardSelection : MonoBehaviour
            {
                private List<Card> _selectCards;

                public void CardsInit(List<Card> selectCards)
                {
                    _selectCards = selectCards;
                }

                private void Update()
                {
                    if (!Input.GetMouseButtonDown(0))
                        return;

                    foreach (var card in _selectCards.Where(card => card.IsPointerIn))
                    {
                        _cardId = card.BLCard.Id;
                    }
                }
            }
#nullable enable
            public async Task<Guid> GetPlayerIdAsync(IReadOnlyList<BLComponent.Player> players, Guid currentPlayerId)
            {
                GameState.PlayerChoosing(true);
                GameState.Aim.SetActive(true);
                _playerId = Guid.Empty; // Сбрасываем перед ожиданием
                var livePlayers = players.Select(player => GameState.Players.
                    First(p => p.Id == player.Id)).ToList();
                var selectionObj = new GameObject("PlayerSelection");
                var selection = selectionObj.AddComponent<PlayerSelection>();
                selection.PlayersInit(livePlayers);
                var tcs = new TaskCompletionSource<Guid>();

                selection.StartCoroutine(WaitForPlayerSelection(tcs, currentPlayerId));

                try
                {
                    return await tcs.Task;
                }
                finally
                {
                    // Уничтожаем временные объекты
                    if (Application.isPlaying)
                        Object.Destroy(selectionObj);
                    else
                        Object.DestroyImmediate(selectionObj);
                    _playerId = Guid.Empty;
                    GameState.Aim.SetActive(false);
                    GameState.PlayerChoosing(true);
                }
            }
            
            private static IEnumerator WaitForPlayerSelection(TaskCompletionSource<Guid> tcs, Guid currentPlayerId)
            {
                // Ждем, пока игрок не выберет ID
                while (_playerId == Guid.Empty || _playerId == currentPlayerId)
                {
                    yield return null;
                }
    
                tcs.SetResult(_playerId);
            }

            public async Task<Guid> GetCardIdAsync(IReadOnlyList<BLComponent.Card?> cards, int unknownCardsCount) =>
                await GetCardIdAsync(cards, unknownCardsCount, null);

            public async Task<Guid> GetCardIdAsync(IReadOnlyList<BLComponent.Card?> cards, int unknownCardsCount, Guid? playerId)
            {
                GameState.PlayerChoosing(false);
                var sCards = cards.Where(card => card is not null).Select(card => GameState.Cards[card!.Id]).ToList();
                var parents = new List<Transform>();
                var rotations = new List<Quaternion>();
                foreach (var card in sCards)
                {
                    rotations.Add(card.transform.rotation);
                    card.transform.rotation = Quaternion.identity;
                    parents.Add(card.transform.parent);
                    card.transform.SetParent(GameState.CardSelection);
                    if (playerId is not null && card.IsFlipped)
                        card.FlipCard();
                }
                
                var selectionObj = new GameObject("CardSelection");
                var selection = selectionObj.AddComponent<CardSelection>();
                selection.CardsInit(sCards);
                var tcs = new TaskCompletionSource<Guid>();

                selection.StartCoroutine(WaitForCardSelection(tcs));

                try
                {
                    return await tcs.Task;
                }
                finally
                {
                    if (Application.isPlaying)
                        Object.Destroy(selectionObj);
                    else
                        Object.DestroyImmediate(selectionObj);
                    for (var i = 0; i < sCards.Count; i++)
                    {
                        sCards[i].transform.rotation = rotations[i];
                        sCards[i].transform.SetParent(parents[i]);
                    }
                    _cardId = Guid.Empty;
                    GameState.PlayerChoosing(false);
                }
            }
            
            private static IEnumerator WaitForCardSelection(TaskCompletionSource<Guid> tcs)
            {
                while (_cardId == Guid.Empty)
                {
                    yield return null;
                }
    
                tcs.SetResult(_cardId);
            }
            public async Task<bool> YesOrNoAsync(Guid playerId, CardName name) => true;
            public void ShowCardResult(Guid curPlayerId, CardName name) {}
            public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId) {}
            public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork) {}
            public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, Guid target) {}
            public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, BLComponent.Card card) {}
            public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId, BLComponent.Card card) {}
            public void CardAddedInHand(Guid cardId, Guid playerId)
            {
                var card = GameState.Cards[cardId];
                card.transform.SetParent(GameState.Players.First(p => p.Id == playerId).hand, false);
                if ((GameState.CurrentPlayer == playerId && card.IsFlipped) ||
                    (GameState.CurrentPlayer != playerId && !card.IsFlipped))
                    card.FlipCard();
            }

            public void CardAddedOnBoard(Guid cardId, Guid playerId)
            {
                GameState.Cards[cardId].transform.SetParent(GameState.Players.
                    First(p => p.Id == playerId).board, false);
            }

            public void WeaponAdded(Guid cardId, Guid playerId)
            {
                GameState.Cards[cardId].transform.SetParent(GameState.Players.
                    First(p => p.Id == playerId).board, false);
            }
            public void CardDiscarded(Guid cardId)
            {
                var card = GameState.Cards[cardId];
                if (card.IsFlipped)
                    card.FlipCard();
                card.transform.SetParent(GameState.DiscardPile, false);
                card.transform.position = GameState.DiscardPile.position;
                card.Discard();
            }

            public void CardReturnedToDeck(Guid cardId)
            {
                var card = GameState.Cards[cardId];
                if (!card.IsFlipped)
                    card.FlipCard();
                card.transform.SetParent(GameState.Deck, false);
                card.transform.position = GameState.Deck.position;
                if (card.IsDiscarded)
                    card.Discard();
            }
#nullable disable
        }
    
    public class GameManager : MonoBehaviour
    {
        private IGameManager _gameManager;
        private readonly Dictionary<Guid, Card> _cards = new();
        [SerializeField]
        private Card cardPrefab;
        [SerializeField]
        private Transform cardContainer;
        [SerializeField]
        private Transform deck;
        [SerializeField]
        private Transform discardPile;
        [SerializeField]
        private List<Player> players;
        [SerializeField]
        private List<GameObject> playersBacks;
        [SerializeField]
        private GameObject aim;
        [SerializeField]
        private Transform cardSelection;
        [SerializeField]
        private GameObject yesOrNo;
        [SerializeField]
        private TextMeshProUGUI discardButtonText;
        [SerializeField] 
        private GameObject gameCanvas;
        [SerializeField]
        private GameObject pauseCanvas;
        [SerializeField]
        private GameObject gameOverCanvas;
        [SerializeField]
        private GameObject errorCanvas;
        [SerializeField]
        private TextMeshProUGUI errorText;
        
        public bool IsDiscarding { get; private set; }
        
        private string _savedDiscardButtonText = "Разыгрывать карты";
        private bool _isGameOver;
        private bool _isFatalError;
        
        public void OpenError(string errorString)
        {
            errorText.text = errorString;
            gameCanvas.SetActive(false);
            errorCanvas.SetActive(true);
        }

        public void CloseError()
        {
            if (!_isFatalError)
            {
                errorCanvas.SetActive(false);
                gameCanvas.SetActive(true);
            }
            else
                Application.Quit();
        }

        private void InitLoad()
        {
            foreach (var player in _gameManager.Players)
            {
                foreach (var card in player.CardsInHand)
                {
                    GameState.Cards[card.Id].transform.SetParent(GameState.Players.
                        First(p => p.Id == player.Id).hand, false);
                    if (player.Id == CurrentPlayerId)
                        GameState.Cards[card.Id].FlipCard();
                }

                foreach (var card in player.CardsOnBoard)
                {
                    GameState.Cards[card.Id].transform.SetParent(GameState.Players.
                        First(p => p.Id == player.Id).board, false);
                    GameState.Cards[card.Id].FlipCard();
                }

                if (player.Weapon is null)
                    continue;
                
                GameState.Cards[player.Weapon.Id].transform
                    .SetParent(GameState.Players.First(p => p.Id == player.Id).board, false);
                GameState.Cards[player.Weapon.Id].FlipCard();
            }

            if (_gameManager.TopDiscardedCard is null) 
                return;
            
            GameState.Cards[_gameManager.TopDiscardedCard.Id].transform
                .SetParent(discardPile, false);
            GameState.Cards[_gameManager.TopDiscardedCard.Id].transform.position = discardPile.position;
            GameState.Cards[_gameManager.TopDiscardedCard.Id].FlipCard();
        }

        private void Awake()
        {
            GameState.Deck = deck;
            GameState.DiscardPile = discardPile;
            GameState.Players = players;
            GameState.Aim = aim;
            GameState.CardSelection = cardSelection;
            GameState.YesOrNo = yesOrNo;
            _gameManager = DataCarrier.GameManager;
            var isLoad = DataCarrier.Players.Count == 0;
            if (!isLoad)
            {
                try
                {
                    _gameManager.GameInit(DataCarrier.Players);
                }
                catch (Exception)
                {
                    OpenError("Неправильная строка подключения к базе данных.");
                    _isFatalError = true;
                    return;
                }
            }
            GameState.CurrentPlayer = _gameManager.CurPlayer.Id;
            var curPlayerIndex = _gameManager.Players.ToList().FindIndex(p => p.Id == _gameManager.CurPlayer.Id);
            players[0].InitId(_gameManager.Players[curPlayerIndex].Id);
            players[1].InitId(_gameManager.Players[(curPlayerIndex + 1) % _gameManager.Players.Count].Id);
            players[^1].InitId(_gameManager.Players[(curPlayerIndex + _gameManager.Players.Count - 1) % 
                                                    _gameManager.Players.Count].Id);
            
            var j = 2;
            for (var i = (curPlayerIndex + 2) % _gameManager.Players.Count;
                 i != (curPlayerIndex + _gameManager.Players.Count - 1) % _gameManager.Players.Count;
                 i = (i + 1) % _gameManager.Players.Count)
            {
                if (_gameManager.Players.Count is 4 or 5)
                {
                    players[j + 1].InitId(_gameManager.Players[i].Id);
                    playersBacks[j].SetActive(true);
                }
                else
                {
                    players[j].InitId(_gameManager.Players[i].Id);
                    playersBacks[j - 1].SetActive(true);
                }
                ++j;
            }

            foreach (var bCard in _gameManager.GetAllCards)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.SetCard(bCard, this);
                _cards.Add(card.BLCard.Id, card);
            }

            GameState.Cards = _cards;
            
            if (!isLoad)
                _gameManager.GameStart();
            else
                InitLoad();
            
            OpenError($"Сейчас ходит игрок {_gameManager.CurPlayer.Name}.");
        }

        private void CheckGameOver(CardRc rc)
        {
            switch (rc)
            {
                case CardRc.OutlawWin:
                    _isGameOver = true;
                    gameCanvas.SetActive(false);
                    gameOverCanvas.SetActive(true);
                    gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Бандиты выиграли!";
                    break;
                case CardRc.SheriffWin:
                    _isGameOver = true;
                    gameCanvas.SetActive(false);
                    gameOverCanvas.SetActive(true);
                    gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Шериф и его помощники выиграли!";
                    break;
                case CardRc.RenegadeWin:
                    _isGameOver = true;
                    gameCanvas.SetActive(false);
                    gameOverCanvas.SetActive(true);
                    gameOverCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Ренегат выиграл!";
                    break;
            }
        }

        public async Task<CardRc> PlayCard(Guid cardId)
        {
            var rc = await _gameManager.PlayCard(cardId);

            CheckGameOver(rc);

            switch (rc)
            {
                case CardRc.CantPlay:
                    OpenError("Нельзя разыграть эту карту.");
                    break;
                case CardRc.TooFar:
                    OpenError("Цель слишком далеко.");
                    break;
            }
            
            return rc;
        }

        public void DiscardCard(Guid cardId)
        {
            _gameManager.DiscardCard(cardId);
        }

        public async void EndTurn()
        {
            var rc = await _gameManager.EndTurn();
            
            CheckGameOver(rc);

            if (rc is CardRc.CantEndTurn)
                OpenError("Чтобы закончить ход разыграйте или сбросьте ещё " +
                          $"{_gameManager.CurPlayer.CardsInHand.Count - _gameManager.CurPlayer.Health} карт.");
            
            if (IsDiscarding)
                DiscardButtonClicked();
        }

        private void Update()
        {
            if (_isFatalError)
                return;
            
            if (Input.GetKeyDown(KeyCode.Escape) && !_isGameOver)
            {
                Pause();
                return;
            }
            
            if (pauseCanvas.activeSelf)
                return;
            
            foreach (var card in GameState.Cards.Values)
            {
                if (!card.IsEntered && card.IsScaleBad)
                    card.FixScale();
            }
            
            foreach (var player in _gameManager.Players)
            {
                foreach (var cardId in player.CardsInHand.Select(c => c.Id))
                {
                    var card = GameState.Cards[cardId];
                    if ((card.IsFlipped && player.Id == _gameManager.CurPlayer.Id) ||
                        (!card.IsFlipped && player.Id != _gameManager.CurPlayer.Id))
                        card.FlipCard();
                }
            }
            
            if (_gameManager.CurPlayer.Id == GameState.CurrentPlayer)
                return;
            
            OpenError($"Ход передаётся игроку {_gameManager.CurPlayer.Name}.");
            
            GameState.CurrentPlayer = _gameManager.CurPlayer.Id;
            
            var i = _gameManager.Players.ToList().FindIndex(p => p.Id == CurrentPlayerId);

            foreach (var player in GameState.Players)
            {
                if (player.Id == Guid.Empty)
                {
                    continue;
                }
                var cardIds = _gameManager.Players[i].CardsInHand.Select(c => c.Id);
                foreach (var cardId in cardIds)
                {
                    GameState.Cards[cardId].transform.SetParent(player.hand, false);
                }
                
                var cardBIds = _gameManager.Players[i].CardsOnBoard.Select(c => c.Id);
                foreach (var cardBId in cardBIds)
                    GameState.Cards[cardBId].transform.SetParent(player.board, false);

                var weapon = _gameManager.Players[i].Weapon;
                if (weapon is not null)
                    GameState.Cards[weapon!.Id].transform.SetParent(player.board, false);

                player.InitId(_gameManager.Players[i].Id);
                i = (i + 1) % _gameManager.Players.Count;
            }
        }

        public void DiscardButtonClicked()
        {
            (discardButtonText.text, _savedDiscardButtonText) = (_savedDiscardButtonText, discardButtonText.text);
            IsDiscarding = !IsDiscarding;
        }

        public int CardsInDeckAmount => _gameManager.CardsInDeck.Count;
        public IReadOnlyList<BLComponent.Player> Players => _gameManager.Players;
        public Guid CurrentPlayerId => _gameManager.CurPlayer.Id;

        public void Pause()
        {
            gameCanvas.SetActive(!gameCanvas.activeSelf);
            pauseCanvas.SetActive(!pauseCanvas.activeSelf);
        }

        public void Save()
        {
            _gameManager.SaveState();
        }

        public void ReturnToMenuWithSave()
        {
            _gameManager.SaveState();
            ReturnToMenu();
        }

        public void ReturnToMenu()
        {
            DataCarrier.Players.Clear();
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            _gameManager.SaveState();
            Application.Quit();
        }
    }
}
