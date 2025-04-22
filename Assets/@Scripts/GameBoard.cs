using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public static class MouseData
{
    //public static Inventory MouseOverInventory; // 마우스가 올라간 인벤토리
    public static GameObject BlockHoveredOver;   // 마우스 커서가 위치한 슬롯
    public static GameObject DragImage;         // 드래그 중인 아이템 이미지
}

public class GameBoard : MonoBehaviour
{
    public GameObject Block;

    List<Block> blocks = new List<Block>();
    Dictionary<GameObject, Block> blockDictionary = new Dictionary<GameObject, Block>();

    Vector2 _start = new Vector2(-455f, 455f);
    Vector2 _size = new Vector2(125f, 125f);
    Vector2 _space = new Vector2(5f, 5f);
    int _numOfColumn = 8;
    //int _numOfBlock = 64;
    Block[] _blocks = new Block[64];

    void Start()
    {
        CreateRandomBlockBoard();
    }

    void Update()
    {

    }

    Vector2 CalculatePosition(int idx)
    {
        float posX = _start.x + (_size.x + _space.x) * (idx % _numOfColumn);
        float posY = _start.y - (_size.y + _space.y) * (idx / _numOfColumn);
        return new Vector2(posX, posY);
    }

    void CreateRandomBlockBoard()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            GameObject block = Instantiate(Block, transform);
            block.GetComponent<RectTransform>().localPosition = CalculatePosition(i);
            block.AddComponent<EventTrigger>();
            // block에 event 추가
            // blcok component마다 블록 이미지 5가지 중 하나 랜덤 부여

            _blocks[i]=block.GetComponent<Block>();
            blockDictionary.Add(block, _blocks[i]);
            block.name = $"Block{i}";
        }
    }
    // 드래그 시작 시, 해당 블록 이미지 제거 및 마우스 따라가는 이미지 생성
    // 이미지의 이동 범위는 상 or 하 or 좌 or 우 방향으로 한 블록까지만
    // 드래그 종료 시, 종료 지점 블록과 이미지 교환(혹은 해당 열이나 행 블록 밀어내기)
    // 이미지 교환 발생 시, 게임보드 전체 순환하며 사라질 블록 있는지 체크

    void AddEvent(GameObject go, EventTriggerType type,UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null)
            return;
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry { eventID = type };
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    void OnStartDrag(GameObject go)
    {

    }

    void OnDrag(GameObject go)
    {

    }

    void OnEndDrag(GameObject go)
    {

    }

    public void SwapBlocks(Block blockA, Block blockB)
    {
        if (blockA == blockB)
            return;
    }
}
