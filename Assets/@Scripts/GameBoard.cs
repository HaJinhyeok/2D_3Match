using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MouseData
{
    //public static Inventory MouseOverInventory; // 마우스가 올라간 인벤토리
    public static GameObject BlockHoveredOver;   // 마우스 커서가 위치한 슬롯
    public static GameObject DragImage;         // 드래그 중인 아이템 이미지
}

public class GameBoard : MonoBehaviour
{
    public GameObject Block;
    public Sprite[] BlockImages = new Sprite[5];

    public Button TestButton;
    public Button RemoveButton;
    public Button MakeButton;

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

        // Test용 버튼들
        TestButton.onClick.AddListener(OnTestButtonClick);
        RemoveButton.onClick.AddListener(OnRemoveButtonClick);
        MakeButton.onClick.AddListener(OnMakeButtonClick);
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
            block.GetComponent<Image>().sprite = BlockImages[Random.Range(0, BlockImages.Length)];

            _blocks[i] = block.GetComponent<Block>();
            blockDictionary.Add(block, _blocks[i]);
            block.name = $"Block{i}";
        }
    }

    #region Event
    // 드래그 시작 시, 해당 블록 이미지 제거 및 마우스 따라가는 이미지 생성
    // 이미지의 이동 범위는 상 or 하 or 좌 or 우 방향으로 한 블록까지만
    // 드래그 종료 시, 종료 지점 블록과 이미지 교환(혹은 해당 열이나 행 블록 밀어내기)
    // 이미지 교환 발생 시, 게임보드 전체 순환하며 사라질 블록 있는지 체크

    void AddEvent(GameObject go, EventTriggerType type, UnityAction<BaseEventData> action)
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
    #endregion

    public void SwapBlocks(Block blockA, Block blockB)
    {
        if (blockA == blockB)
            return;
    }

    #region 3-Match Check Method
    // 3-match가 발생했는지 여부 확인
    // 발생 시, 해당 블록들의 리스트들을 반환
    public List<List<int>> CheckMatches()
    {
        bool[] isChecked = new bool[64];
        List<List<int>> matches = new List<List<int>>();
        List<int> tmp;
        for (int i = 0; i < _blocks.Length; i++)
        {
            tmp = CheckMatchFromBlock(i, isChecked);
            if (tmp != null)
                matches.Add(tmp);
        }
        return matches;
    }

    // idx번째 블록을 기준으로 3-match 발생하는지 검사
    // 발생 시 해당 블록들의 인덱스 정보 담은 리스트 반환
    // 배열 크기가 3 미만이면 null 반환
    List<int> CheckMatchFromBlock(int idx, bool[] isChecked)
    {
        List<int> matchBlocks = new List<int>();
        CheckSameBlock(matchBlocks, idx, _blocks[idx].GetComponent<Image>().sprite, isChecked);

        if (matchBlocks.Count <= 2)
            return null;
        else
            return matchBlocks;
    }

    // idx번째 블록 기준으로 오른쪽 혹은 아래 블록이 같은 종류인지 체크
    // 같은 종류면 blocks 리스트에 담아주고, 아니면 통과
    void CheckSameBlock(List<int> matchBlocks, int idx, Sprite sprite, bool[] isChecked)
    {
        if (isChecked[idx])
            return;
        Sprite blockImage = _blocks[idx].GetComponent<Image>().sprite;
        if (blockImage == null)
            return;
        if (blockImage == sprite)
        {
            int posX = idx / _numOfColumn;
            int posY = idx % _numOfColumn;
            matchBlocks.Add(idx);
            isChecked[idx] = true;
            // 왼쪽 블록
            if (idx % _numOfColumn - 1 >= 0)
                CheckSameBlock(matchBlocks, idx - 1, sprite, isChecked);
            // 오른쪽 블록
            if (idx % _numOfColumn + 1 < _numOfColumn)
                CheckSameBlock(matchBlocks, idx + 1, sprite, isChecked);
            // 아래쪽 블록
            if (idx / _numOfColumn + 1 < _numOfColumn)
                CheckSameBlock(matchBlocks, idx + _numOfColumn, sprite, isChecked);
        }

    }
    #endregion

    // 3-match된 블록들 제거
    // 블록 제거 시, 해당 블록 위에 있던 블록들 아래로 내려오도록?
    // 혹은 사라진 자리에서 생성되도록?
    void DestroyMatchBlocks(List<int> indices)
    {
        foreach (int idx in indices)
        {
            _blocks[idx].ClearBlock();
        }
    }

    void MakeBlocks()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            if (_blocks[i].IsEmpty)
                _blocks[i].MakeBlock(BlockImages[Random.Range(0, BlockImages.Length)]);
        }
    }

    #region Test Code
    public void OnTestButtonClick()
    {
        string test = "match idices\n";
        List<List<int>> matches = CheckMatches();
        foreach (List<int> matchBlocks in matches)
        {
            foreach (int idx in matchBlocks)
            {
                test += $"{idx} ";
            }
            test += "\n";
        }
        Debug.Log(test);
    }

    public void OnRemoveButtonClick()
    {
        List<List<int>> matches = CheckMatches();
        foreach (List<int> matchBlocks in matches)
        {
            DestroyMatchBlocks(matchBlocks);
        }
    }

    public void OnMakeButtonClick()
    {
        MakeBlocks();
    }
    #endregion
}
