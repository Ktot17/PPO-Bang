using System.Globalization;
using TMPro;
using UnityEngine;

namespace Bang
{
    public class Deck : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private GameObject deckInfo;
        [SerializeField] 
        private GameObject cardsInDeckAmountText;

        private string _cardsInDeckAmountFormat;

        private void Awake()
        {
            _cardsInDeckAmountFormat = cardsInDeckAmountText.GetComponent<TextMeshProUGUI>().text;
        }

        public void OnPointerEnter()
        {
            cardsInDeckAmountText.GetComponent<TextMeshProUGUI>().text = string.Format(CultureInfo.InvariantCulture,
                _cardsInDeckAmountFormat, gameManager.CardsInDeckAmount);
            deckInfo.SetActive(true);
        }

        public void OnPointerExit()
        {
            deckInfo.SetActive(false);
        }
    }
}
