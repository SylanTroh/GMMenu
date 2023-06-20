
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    public class SliderUpdate : GMMenuPart
    {
        [SerializeField] Slider _slider;
        [SerializeField] Text _sliderText;
        [SerializeField] PlayerMover playerMover;

        private void Start()
        {
            _slider.value = 16.0f;
        }
        public void SliderValueChanged()
        {
            _sliderText.text = "Noclip Speed: ";
            _sliderText.text += _slider.value.ToString("#.#");
            playerMover.speedMagnitude = _slider.value;
        }
    }
}
