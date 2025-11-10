using TMPro;
using UnityEngine;

namespace TheTunnel.Components
{
    public class CountDownText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textGO;

        private void Start()
        {
            // disable on start
            SetActive(false);
        }

        public void SetActive(bool active)
        {
            textGO.gameObject.SetActive(active);
        }

        public void SetText(string text)
        {
            textGO.text = text;
        }
    }
}
