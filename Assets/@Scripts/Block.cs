using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class Block : MonoBehaviour
{
    //public Sprite blockImage;
    public Image BlockImage;

    public bool IsEmpty
    {
        get { return BlockImage.sprite == null; }
    }

    void Start()
    {
        BlockImage = GetComponent<Image>();
    }

    public void ClearBlock()
    {
        BlockImage.sprite = null;
    }

    public void UpdateBlockImage(Sprite sprite)
    {
        BlockImage.sprite = sprite;
    }

}
