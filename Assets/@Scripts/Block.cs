using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(EventTrigger))]
public class Block : MonoBehaviour
{
    public Image BlockImage;
    public VisualEffect BlockCrashEffect;

    public bool IsEmpty
    {
        get { return BlockImage.color.a <= 0.1f; }
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

    public void BlockCrash()
    {
        BlockCrashEffect.transform.position = transform.position;
        BlockCrashEffect.Play();
        //BlockCrashEffect.SendEvent("OnPlay");
    }

    public void SetBlockImagePadding(Vector2 padding)
    {
        RectTransform rect = BlockImage.GetComponent<RectTransform>();
        rect.sizeDelta = -2 * padding;
    }
}
