using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class Block : MonoBehaviour
{
    //public Sprite blockImage;
    Image _blockImage;

    public bool IsEmpty
    {
        get { return _blockImage.sprite == null; }
    }

    void Start()
    {
        _blockImage = GetComponent<Image>();
    }

    public void ClearBlock()
    {
        _blockImage.sprite = null;
    }

    public void MakeBlock(Sprite sprite)
    {
        _blockImage.sprite = sprite;
    }

}
