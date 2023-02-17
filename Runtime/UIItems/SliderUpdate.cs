
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    public class SliderUpdate : UdonSharpBehaviour
    {
        [SerializeField] Slider _slider;
        [SerializeField] Text _sliderText;
        [SerializeField] PlayerMover playerMover;

        private void Start()
        {
            _slider.value = 8.0f;
        }
        public void SliderValueChanged()
        {
            _sliderText.text = "Speed: ";
            _sliderText.text += _slider.value.ToString("#.#");
            playerMover.speedMagnitude = _slider.value;
        }
    }
}
