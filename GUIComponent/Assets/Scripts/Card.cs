using BLComponent;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bang
{
    public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public BLComponent.Card BLCard { get; private set; }
        [SerializeField]
        private TextMeshProUGUI cardName;
        [SerializeField]
        private TextMeshProUGUI cardRank;
        [SerializeField] 
        private Image cardSuit;
        [SerializeField]
        private Image cardColor;
        [SerializeField]
        private GameObject cardCover;
        [SerializeField]
        private RectTransform cardContainer;
        [SerializeField]
        private float scaleCoefficient = 2f;
        [SerializeField]
        private float offset = 100f;
        [SerializeField]
        private CanvasGroup canvasGroup;
        private GameManager _gameManager;
        private bool _isDragging;
        private GameObject _startParent;
        private Vector2 _startPosition;
        private bool _isOverDropZone;
        private bool _isChoosing;
        private Vector3 _scale;
        public bool IsFlipped => cardCover.activeSelf;
        public bool IsDiscarded { get; private set; }
        public bool IsPointerIn { get; private set; }
        public bool IsEntered { get; private set; }
        public bool IsZoomed { get; private set; }
        public bool IsScaleBad => cardContainer.localScale != _scale;
        public string CardName => cardName.text;

        public void SetCard(BLComponent.Card card, GameManager manager)
        {
            BLCard = card;
            _gameManager = manager;
        }

        public void FlipCard()
        {
            cardCover.SetActive(!cardCover.activeSelf);
        }

        public void OverDropZone()
        {
            _isOverDropZone = !_isOverDropZone;
        }

        public void PlayerChoosing()
        {
            _isChoosing = !_isChoosing;
        }

        private void Start()
        {
            _scale = new Vector3(.5f, .5f, .5f);
            cardName.text = DataCarrier.CardNames[BLCard.Name];
            cardRank.text = DataCarrier.CardRanks[BLCard.Rank];
            cardSuit.sprite = DataCarrier.CardSuits[BLCard.Suit];
            cardColor.sprite = DataCarrier.CardWraps[BLCard.Type];
        }

        protected void Update()
        {
            if (_isDragging && !cardCover.activeSelf)
                transform.position = Input.mousePosition;
        }

        public void BeginDrag()
        {
            canvasGroup.blocksRaycasts = false;
            if (IsDiscarded || _isChoosing || IsFlipped)
                return;
            _isDragging = true;
            _startParent = transform.parent.gameObject;
            _startPosition = transform.position;
            if (IsEntered)
                _startPosition.y -= offset;
        }

        public async void EndDrag()
        {
            canvasGroup.blocksRaycasts = true;
            if (IsDiscarded || _isChoosing || IsFlipped)
                return;
            _isDragging = false;
            if (_isOverDropZone)
            {
                OverDropZone();
                if (_gameManager.IsDiscarding)
                {
                    _gameManager.DiscardCard(BLCard.Id);
                    return;
                }
                
                var rc = await _gameManager.PlayCard(BLCard.Id);
                if (rc is CardRc.Ok)
                {
                    if (BLCard.Type is CardType.Instant)
                        Discard();
                    return;
                }
            }

            transform.position = _startPosition;
            transform.SetParent(_startParent.transform, false);
        }

        public void Discard()
        {
            IsDiscarded = !IsDiscarded;
        }

        private void Zoom()
        {
            IsZoomed = true;
            cardContainer.localScale *= scaleCoefficient;
        }
        
        private void UnZoom()
        {
            IsZoomed = false;
            cardContainer.localScale /= scaleCoefficient;
        }

        public void FixScale()
        {
            cardContainer.localScale = _scale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsPointerIn = true;
            if (cardCover.activeSelf || eventData.pointerDrag != null || _isChoosing)
                return;
            IsEntered = true;
            cardContainer.position = new Vector3(cardContainer.position.x,
                cardContainer.position.y + offset, cardContainer.position.z);
            Zoom();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerIn = false;
            if (cardCover.activeSelf || !IsEntered || _isChoosing)
                return;
            IsEntered = false;
            cardContainer.position = new Vector3(cardContainer.position.x,
                cardContainer.position.y - offset, cardContainer.position.z);
            UnZoom();
        }
    }
}
