using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public static class MouseData
{
    public static bool IsDragging;              // 현재 드래그 진행 중인지 여부
    public static GameObject StartBlock;        // 드래그 시작한 위치의 블록 
}

public class GameBoard : MonoBehaviour
{
    public GameObject Block;
    public Sprite[] BlockImages = new Sprite[5];

    //public Button TestButton;
    //public Button RemoveButton;
    //public Button MakeButton;

    Dictionary<GameObject, Block> _blockDictionary = new Dictionary<GameObject, Block>();

    Vector2 _start = new Vector2(-455f, 455f);
    Vector2 _size = new Vector2(125f, 125f);
    Vector2 _space = new Vector2(5f, 5f);
    const int _numOfColumn = 8;
    //int _numOfBlock = 64;
    Block[,] _blocks = new Block[_numOfColumn, _numOfColumn];

    void Start()
    {
        CreateRandomBlocks();
        Invoke("MakeBlocks", Time.deltaTime);
        MouseData.IsDragging = false;

        // Test용 버튼들
        //TestButton.onClick.AddListener(OnTestButtonClick);
        //RemoveButton.onClick.AddListener(OnRemoveButtonClick);
        //MakeButton.onClick.AddListener(OnMakeButtonClick);
    }

    Vector2 CalculatePosition(int idx)
    {
        float posX = _start.x + (_size.x + _space.x) * (idx % _numOfColumn);
        float posY = _start.y - (_size.y + _space.y) * (idx / _numOfColumn);
        return new Vector2(posX, posY);
    }

    void CreateRandomBlocks()
    {
        int x, y;
        for (int i = 0; i < _blocks.Length; i++)
        {
            x = i / _numOfColumn;
            y = i % _numOfColumn;
            GameObject block = Instantiate(Block, transform);
            block.GetComponent<RectTransform>().localPosition = CalculatePosition(i);
            block.AddComponent<EventTrigger>();
            // block에 event 추가
            AddEvent(block, EventTriggerType.BeginDrag, delegate { OnStartDrag(block); });
            //AddEvent(block, EventTriggerType.Drag, delegate{ OnDrag(block); });
            AddEvent(block, EventTriggerType.EndDrag, delegate { OnEndDrag(block); });
            AddEvent(block, EventTriggerType.PointerEnter, delegate { StartCoroutine(OnEnterBlock(block)); });

            // blcok component마다 블록 이미지 5가지 중 하나 랜덤 부여
            block.GetComponent<Image>().sprite = BlockImages[UnityEngine.Random.Range(0, BlockImages.Length)];

            _blocks[x, y] = block.GetComponent<Block>();
            _blockDictionary.Add(block, _blocks[x, y]);
            block.name = $"Block{i}";
        }
    }

    #region Event
    // 드래그 시작 시, 해당 블록 이미지 제거 및 마우스 따라가는 이미지 생성
    // 이미지의 이동 범위는 상 or 하 or 좌 or 우 방향으로 한 블록까지만
    // 드래그 종료 시, 종료 지점 블록과 이미지 교환(혹은 해당 열이나 행 블록 밀어내기)
    // 이미지 교환 발생 시, 게임보드 전체 순환하며 사라질 블록 있는지 체크
    // 캔디크러쉬사가: 드래그 시작 후, 상하좌우 중 하나의 타일 영역으로 드래그하는 순간 교환 발생
    // 애니팡: 드래그 시작된 방향으로 바로 교환 발생

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
        MouseData.StartBlock = go;
        MouseData.IsDragging = true;
        Vector2 goSize = go.GetComponent<RectTransform>().sizeDelta;
        Debug.Log($"Block Size: {goSize.x}, {goSize.y}");
    }

    void OnEndDrag(GameObject go)
    {
        MouseData.IsDragging = false;
    }

    IEnumerator OnEnterBlock(GameObject go)
    {
        if (MouseData.IsDragging)
        {
            yield return StartCoroutine(CoSwapBlocks(_blockDictionary[go], _blockDictionary[MouseData.StartBlock]));
            {
                // 만약 3매치가 없으면(==삭제할게 없으면), 두 블록 다시 제자리로
                if (!DestroyBlocks())
                {
                    yield return StartCoroutine(CoSwapBlocks(_blockDictionary[go], _blockDictionary[MouseData.StartBlock]));
                }
                else
                {
                    MakeBlocks();
                }
            }
        }
        yield return null;
    }
    #endregion

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
        CheckSameBlock(matchBlocks, idx, _blocks[idx / _numOfColumn, idx % _numOfColumn].GetComponent<Image>().sprite, isChecked);

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
        Sprite blockImage = _blocks[idx / _numOfColumn, idx % _numOfColumn].GetComponent<Image>().sprite;
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

    #region Block Move & Generate & Destroy Method

    IEnumerator CoSwapBlocks(Block blockA, Block blockB)
    {
        if (blockA == blockB)
            yield return null;
        if (blockA.transform.localPosition.x != blockB.transform.localPosition.x
            && blockA.transform.localPosition.y != blockB.transform.localPosition.y)
            yield return null;
        // 교환 일어나면, 더 이상 드래그해도 교환 없도록 마우스정보 초기화
        // MouseData.StartBlock = null;
        MouseData.IsDragging = false;
        // 두 블록의 위치가 바뀌는 애니메이션을 어떻게?
        // 두 블록의 이미지 위치를 lerp함수로 바꾼다.
        // 이때, 움직임은 코루틴으로 표현
        yield return StartCoroutine(CoSwapBlockImages(blockA, blockB));
    }

    IEnumerator CoSwapBlockImages(Block blockA, Block blockB)
    {
        Sprite blockImage1 = blockA.BlockImage.sprite;
        Sprite blockImage2 = blockB.BlockImage.sprite;

        // 각 블록 위치에 이미지 오브젝트 생성
        GameObject block1 = new GameObject("image1");
        block1.transform.SetParent(transform);
        RectTransform rect1 = block1.AddComponent<RectTransform>();
        rect1.sizeDelta = _size;
        rect1.localScale = Vector3.one;
        rect1.localEulerAngles = Vector3.zero;
        rect1.localPosition = blockA.transform.localPosition;
        Image image1 = block1.AddComponent<Image>();
        image1.sprite = blockImage1;

        GameObject block2 = new GameObject("image2");
        block2.transform.SetParent(transform);
        RectTransform rect2 = block2.AddComponent<RectTransform>();
        rect2.sizeDelta = _size;
        rect2.localScale = Vector3.one;
        rect2.localPosition = blockB.transform.localPosition;
        Image image2 = block2.AddComponent<Image>();
        image2.sprite = blockImage2;

        // 각 블록의 이미지 제거
        //blockA.ClearBlock();
        //blockB.ClearBlock();
        blockA.gameObject.SetActive(false);
        blockB.gameObject.SetActive(false);

        // 서로의 위치로 이미지가 이동할 때까지 yield return null
        Vector3 posA = block1.transform.localPosition;
        Vector3 posB = block2.transform.localPosition;
        while (Vector3.Distance(block1.transform.localPosition, posB) > 5f)
        {
            block1.transform.localPosition = Vector3.Lerp(block1.transform.localPosition, posB, Time.deltaTime * 5);
            block2.transform.localPosition = Vector3.Lerp(block2.transform.localPosition, posA, Time.deltaTime * 5);
            yield return null;
        }

        Destroy(block1);
        Destroy(block2);
        // 끝나면 두 블록의 이미지 교체
        Sprite tmp = blockImage1;
        blockA.UpdateBlockImage(blockImage2);
        blockB.UpdateBlockImage(tmp);
        blockA.gameObject.SetActive(true);
        blockB.gameObject.SetActive(true);
    }
    // 3-match된 블록들 제거
    // 블록 제거 시, 해당 블록 위에 있던 블록들 아래로 내려오도록?
    // 혹은 사라진 자리에서 생성되도록?

    IEnumerator CoMakeBlocks()
    {
        do
        {
            yield return StartCoroutine(CoCheckAndSupplyBlocksToColumn());
            yield return new WaitForSeconds(0.5f);
        } while (DestroyBlocks());
    }

    void MakeBlocks()
    {
        StartCoroutine(CoMakeBlocks());
    }

    void DestroyMatchBlocks(List<int> indices)
    {
        foreach (int idx in indices)
        {
            _blocks[idx / _numOfColumn, idx % _numOfColumn].ClearBlock();
            GameManager.Instance.Score++;
        }
    }

    bool DestroyBlocks()
    {
        List<List<int>> matches = CheckMatches();
        // 삭제할 게 없으면 false
        if (matches.Count == 0)
            return false;
        foreach (List<int> matchBlocks in matches)
        {
            DestroyMatchBlocks(matchBlocks);
        }
        return true;
    }

    IEnumerator CoCheckAndSupplyBlocksToColumn()
    {
        int[] countEmpty = new int[_numOfColumn];
        bool isEmptyBlockExist = false;
        // 새로 생성될 블록을 포함하여 밑으로 이동될 블록들
        List<List<GameObject>> movingBlocks = new List<List<GameObject>>();
        // 각 column에서 어느 자리의 블록이 비었는지 확인
        // 새로 생성해야할 블록 개수 == 빈 블록 개수
        // 해당 열에서 특정 블록을 밑으로 내려주는 함수 필요
        for (int j = 0; j < _numOfColumn; j++)
        {
            movingBlocks.Add(new List<GameObject>());
            isEmptyBlockExist = false;
            for (int i = _numOfColumn - 1; i >= 0; i--)
            {
                // 해당 열에 빈 블록 존재할때
                if (_blocks[i, j].IsEmpty)
                {
                    if (!isEmptyBlockExist)
                    {
                        isEmptyBlockExist = true;
                    }
                    countEmpty[j]++;
                }
                else
                {
                    // 만약 밑에 빈 블록 존재했으면, 이 블록은 밑으로 움직여야 할 블록이므로
                    if (isEmptyBlockExist)
                    {
                        GameObject block = new GameObject($"{movingBlocks[j].Count}_th block of {j}_th column");
                        block.transform.SetParent(transform);
                        RectTransform rect = block.AddComponent<RectTransform>();
                        rect.sizeDelta = _size;
                        rect.localScale = Vector3.one;
                        rect.localEulerAngles = Vector3.zero;
                        rect.localPosition = _blocks[i, j].gameObject.transform.localPosition;
                        Image image1 = block.AddComponent<Image>();
                        image1.sprite = _blocks[i, j].BlockImage.sprite;
                        movingBlocks[j].Add(block);
                        _blocks[i, j].ClearBlock();
                    }
                }
            }
        }

        Vector3 columnTopBlockPosition;
        // 빈 블록 채울 새 블록 생성해주기
        for (int j = 0; j < _numOfColumn; j++)
        {
            columnTopBlockPosition = _blocks[0, j].transform.localPosition;
            for (int i = 0; i < countEmpty[j]; i++)
            {
                GameObject block = new GameObject($"{movingBlocks[j].Count}_th block of {j}_th column");
                block.transform.SetParent(transform);
                RectTransform rect = block.AddComponent<RectTransform>();
                rect.sizeDelta = _size;
                rect.localScale = Vector3.one;
                rect.localEulerAngles = Vector3.zero;
                rect.localPosition = new Vector3(columnTopBlockPosition.x, columnTopBlockPosition.y + (i + 1) * (_size.y + _space.y), 0);
                Image image1 = block.AddComponent<Image>();
                image1.sprite = BlockImages[UnityEngine.Random.Range(0, BlockImages.Length)];
                movingBlocks[j].Add(block);
            }
        }
        int sumOfCountEmpty = 0;
        for(int i=0;i<countEmpty.Length;i++)
        {
            sumOfCountEmpty += countEmpty[i];
        }
        // 빈 블록 없을 경우 패스
        if (sumOfCountEmpty == 0)
        {
            yield return null;
        }
        else
        {
            // 블록들 밑으로 내려주기
            yield return StartCoroutine(CoMoveBlocks(movingBlocks));
        }            
    }

    IEnumerator CoMoveBlocks(List<List<GameObject>> movingBlocks)
    {
        // 가장 많은 블록이 새로 생성된 열을 찾아서 그 블록들이 다 제자리로 갈 때까지 옮김
        int maxColumn = 0;
        for (int i = 1; i < _numOfColumn; i++)
        {
            maxColumn = (movingBlocks[maxColumn].Count < movingBlocks[i].Count) ? i : maxColumn;
        }
        while (Vector3.Distance(movingBlocks[maxColumn][movingBlocks[maxColumn].Count - 1].transform.localPosition, _blocks[0, maxColumn].gameObject.transform.localPosition) > 5f)
        {
            for (int j = 0; j < _numOfColumn; j++)
            {
                for (int i = 0; i < movingBlocks[j].Count; i++)
                {
                    // movingBlocks[j][i] 오브젝트를
                    // _blocks[movingBlocks[j].count-1-i,j]의 localPosition으로 이동
                    movingBlocks[j][i].transform.localPosition =
                        Vector3.Lerp(movingBlocks[j][i].transform.localPosition,
                        _blocks[movingBlocks[j].Count - 1 - i, j].transform.localPosition,
                        Time.deltaTime * 5f);
                }
            }
            yield return null;
        }

        for (int j = 0; j < _numOfColumn; j++)
        {
            for (int i = 0; i < movingBlocks[j].Count; i++)
            {
                _blocks[movingBlocks[j].Count - 1 - i, j].UpdateBlockImage(movingBlocks[j][i].GetComponent<Image>().sprite);
                Destroy(movingBlocks[j][i]);
            }
        }

        movingBlocks.Clear();
    }

    #endregion


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
        DestroyBlocks();
    }

    public void OnMakeButtonClick()
    {
        MakeBlocks();
    }
    #endregion
}
