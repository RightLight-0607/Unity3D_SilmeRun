using UnityEngine;
using UnityEngine.UI;

public class CharacterInfo : MonoBehaviour
{
    [SerializeField] Slider curveSlider;
    [SerializeField] Slider speedSlider;
    [SerializeField] Slider jumpPowerSlider;
    [SerializeField] Player[] players = new Player[4];
    [SerializeField] Image characterInfoImage;
    Sprite[] characterImage;
    private void Awake()
    {
        characterImage = GameManager.instance.characterImage;
    }
    private void OnEnable()
    {
        transform.position = Input.mousePosition;
    }
    public void StatSet(int index)
    {
        characterInfoImage.sprite = characterImage[index];
        curveSlider.value = players[index].curve;
        speedSlider.value = players[index].speed;
        jumpPowerSlider.value = players[index].jumpPower;
    }
}
