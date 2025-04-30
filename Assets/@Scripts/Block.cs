using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class Block : MonoBehaviour
{
    public Image BlockImage;

    public bool IsEmpty
    {
        get { return BlockImage.color.a <= 0.1f; }
    }

    void Start()
    {
        //BlockImage = GetComponentsInChildren<Image>()[1];
    }

    public void TurnOffBlock()
    {
        Color color = BlockImage.color;
        color.a = 0f;
        BlockImage.color = color;
    }

    public void TurnOnBlock()
    {
        Color color = BlockImage.color;
        color.a = 1f;
        BlockImage.color = color;
    }

    public void UpdateBlockImage(Sprite sprite)
    {
        BlockImage.sprite = sprite;
        TurnOnBlock();
    }

}
