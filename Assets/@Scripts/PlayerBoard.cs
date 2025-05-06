using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public static class MouseData
{
    public static bool IsDragging;              // 현재 드래그 진행 중인지 여부
    public static GameObject StartBlock;        // 드래그 시작한 위치의 블록 
}

public class PlayerBoard : Board
{
    public Client Client;

    public Button CheckButton;
    public Button ChangeButton;
    public Text CheckResultText;

    public static Action OnGameStart;
    public static Action OnGameFinish;
    public static Action OnRivalConnectionError;


    //public Button TestButton;
    //public Button RemoveButton;
    //public Button MakeButton;

    bool _isBlockMoving = false;
    bool _isChecking = false;
    float _timeCount;
    const float _checkCoolTime = 3f;

    #region Life Cycle
    private void Update()
    {
        if (_isChecking)
        {
            _timeCount += Time.deltaTime;
            if (_timeCount >= _checkCoolTime)
            {
                _timeCount = 0;
                _isChecking = false;
                if (CheckResultText != null)
                {
                    OnCheckButtonClick();
                }
            }
        }
        else
        {
            _timeCount = 0;
        }
    }

    protected override void Start()
    {
        base.Start();
        MouseData.IsDragging = false;
        OnGameStart += GameStart;
        OnGameFinish += FinishGame;
        OnRivalConnectionError += RivalConnectionError;
        if (SceneManager.GetActiveScene().name == Define.MatchGameScene)
        {
            Client.SendMessageToServer("MATCH");
            CheckButton = null;
            ChangeButton = null;
            CheckResultText = null;
        }
        else if (SceneManager.GetActiveScene().name == Define.SoloGameScene)
        {
            Client = null;
            CheckButton.onClick.AddListener(OnCheckButtonClick);
            ChangeButton.onClick.AddListener(OnChangeButtonClick);
            GameStart();
        }

        // Test용 버튼들
        //TestButton.onClick.AddListener(OnTestButtonClick);
        //RemoveButton.onClick.AddListener(OnRemoveButtonClick);
        //MakeButton.onClick.AddListener(OnMakeButtonClick);
    }

    private void OnDestroy()
    {
        OnGameStart -= GameStart;
        OnGameFinish -= FinishGame;
        OnRivalConnectionError -= RivalConnectionError;
    }
    #endregion

    #region General Methods
    void GameStart()
    {
        CreateRandomBlocks();
        Invoke("MakeBlocks", Time.deltaTime);
        Score = 0;
        ScoreText.text = $"SCORE: {Score}";
        GameManager.Instance.GameStatus.PlayerScore = Score;
    }

    void CreateRandomBlocks()
    {
        // IOCP 서버 전달용 string(임시)
        int[,] blockStatus = new int[_numOfColumn, _numOfColumn];
        string clientData = $"{(int)Define.DataStatus.Start}\n";
        int x, y;
        for (int i = 0; i < _blocks.Length; i++)
        {
            x = i / _numOfColumn;
            y = i % _numOfColumn;
            GameObject block = Instantiate(GameManager.Instance.BlockPrefab, transform);
            RectTransform rect = block.GetComponent<RectTransform>();
            rect.localPosition = CalculatePosition(i);
            rect.sizeDelta = _blockSize;
            block.AddComponent<EventTrigger>();
            // block에 event 추가
            AddEvent(block, EventTriggerType.BeginDrag, delegate { OnStartDrag(block); });
            //AddEvent(block, EventTriggerType.Drag, delegate{ OnDrag(block); });
            AddEvent(block, EventTriggerType.EndDrag, delegate { OnEndDrag(block); });
            AddEvent(block, EventTriggerType.PointerEnter, delegate { StartCoroutine(OnEnterBlock(block)); });

            // block component마다 블록 이미지 5가지 중 하나 랜덤 부여
            int rnd = UnityEngine.Random.Range(0, GameManager.Instance.BlockImages.Length);
            block.GetComponent<Block>().UpdateBlockImage(GameManager.Instance.BlockImages[rnd]);

            _blocks[x, y] = block.GetComponent<Block>();
            _blocks[x, y].SetBlockImagePadding(_space);
            _blockDictionary.Add(block, _blocks[x, y]);
            block.name = $"Block{i}";

            blockStatus[x, y] = rnd;
            clientData += rnd.ToString();
            if (y == _numOfColumn - 1)
                clientData += "\n";
            else clientData += " ";
        }
        Client?.SendMessageToServer(clientData);

    }

    IEnumerator ChangeAllBlockImages()
    {
        for (int i = 0; i < _numOfColumn; i++)
        {
            for (int j = 0; j < _numOfColumn; j++)
            {
                _blocks[i, j].BlockCrash();
                _blocks[i, j].UpdateBlockImage(GameManager.Instance.BlockImages[UnityEngine.Random.Range(0, GameManager.Instance.BlockImages.Length)]);
            }
        }
        yield return new WaitForSeconds(0.5f);
        MakeBlocks();
    }

    public void FinishGame()
    {
        ClearBoard();
    }

    public void RivalConnectionError()
    {
        ClearBoard();
        GameManager.Instance.GameStatus.OnRivalConnectionError();
        ResultPanel.OnResultPanelOn?.Invoke();
    }
    #endregion

    #region Event
    // 이미지의 이동 범위는 상 or 하 or 좌 or 우 방향으로 한 블록까지만
    // 드래그 종료 시, 종료 지점 블록과 이미지 교환(혹은 해당 열이나 행 블록 밀어내기)
    // 이미지 교환 발생 시, 게임보드 전체 순환하며 사라질 블록 있는지 체크 -> 교환 발생한 블록만 체크
    // 캔디크러쉬사가: 드래그 시작 후, 상하좌우 중 하나의 타일 영역으로 드래그하는 순간 교환 발생 v
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
        if (_isBlockMoving)
            return;
        MouseData.StartBlock = go;
        MouseData.IsDragging = true;
        Vector2 goSize = go.GetComponent<RectTransform>().sizeDelta;
    }

    void OnEndDrag(GameObject go)
    {
        MouseData.IsDragging = false;
    }

    IEnumerator OnEnterBlock(GameObject go)
    {
        if (MouseData.IsDragging)
        {
            Block startBlock = _blockDictionary[MouseData.StartBlock];
            Block enterBlock = _blockDictionary[go];
            Vector2 localPosDifference = enterBlock.transform.localPosition - startBlock.transform.localPosition;
            // 두 블록이 대각선 위치이면 이동 x
            if (Mathf.Abs(localPosDifference.x) > _size.x - _space.x && Mathf.Abs(localPosDifference.y) > _size.y - _space.y)
            {
                Debug.Log("이동 불가");
                MouseData.IsDragging = false;
            }
            else
            {
                _isBlockMoving = true;
                _isChecking = false;
                yield return StartCoroutine(CoSwapBlocks(enterBlock, startBlock));
                {
                    // 만약 3매치가 없으면(==삭제할게 없으면), 두 블록 다시 제자리로
                    // 교환한 두 블록에 대해서만 검사
                    if (!DestroySwapBlocks(startBlock, enterBlock))
                    {
                        yield return StartCoroutine(CoSwapBlocks(enterBlock, startBlock));
                        _isBlockMoving = false;
                    }
                    else
                    {
                        MakeBlocks();
                    }
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
        List<List<int>> matches = new List<List<int>>();
        List<List<int>> tmp;
        for (int i = 0; i < _numOfColumn; i++)
        {
            tmp = CheckingRowOrColumn(i, true);
            if (tmp.Count > 0)
            {
                for (int j = 0; j < tmp.Count; j++)
                {
                    matches.Add(tmp[j]);
                }
            }

            tmp = CheckingRowOrColumn(i, false);
            if (tmp.Count > 0)
            {
                for (int j = 0; j < tmp.Count; j++)
                {
                    matches.Add(tmp[j]);
                }
            }
        }
        return matches;
    }

    // idx번째 row 혹은 column 조사
    List<List<int>> CheckingRowOrColumn(int idx, bool flag)
    {
        List<List<int>> blocksOfLine = new List<List<int>>();
        List<int> blocks = new List<int>();
        Sprite currentSprite;
        // flag == true이면 idx번째 column 조사
        if (flag)
        {
            currentSprite = _blocks[0, idx].BlockImage.sprite;
            blocks.Add(idx);
            for (int i = 1; i < _numOfColumn; i++)
            {
                // 다음 블록의 이미지가 다른 거면
                if (_blocks[i, idx].BlockImage.sprite != currentSprite)
                {
                    // 다음 블록의 이미지를 기준 이미지로 바꾸고
                    currentSprite = _blocks[i, idx].BlockImage.sprite;
                    if (blocks.Count >= 3)
                    {
                        List<int> tmp = new List<int>(blocks);
                        blocksOfLine.Add(tmp);
                    }
                    blocks.Clear();
                    blocks.Add(i * _numOfColumn + idx);
                }
                // 같은 이미지면 조사 중인 리스트에 넣고 진행
                else
                {
                    blocks.Add(i * _numOfColumn + idx);
                }
            }
            if (blocks.Count >= 3)
            {
                List<int> tmp = new List<int>(blocks);
                blocksOfLine.Add(tmp);
            }
            //return blocksOfLine.Count > 0 ? blocksOfLine : null;
            return blocksOfLine;
        }
        // flag == false이면 idx번째 row 조사
        else
        {
            currentSprite = _blocks[idx, 0].BlockImage.sprite;
            blocks.Add(idx * _numOfColumn);
            for (int i = 1; i < _numOfColumn; i++)
            {
                // 다음 블록의 이미지가 다른 거면
                if (_blocks[idx, i].BlockImage.sprite != currentSprite)
                {
                    // 다음 블록의 이미지를 기준 이미지로 바꾸고
                    currentSprite = _blocks[idx, i].BlockImage.sprite;
                    if (blocks.Count >= 3)
                    {
                        List<int> tmp = new List<int>(blocks);
                        blocksOfLine.Add(tmp);
                    }
                    blocks.Clear();
                    blocks.Add(idx * _numOfColumn + i);
                }
                // 같은 이미지면 조사 중인 리스트에 넣고 진행
                else
                {
                    blocks.Add(idx * _numOfColumn + i);
                }
            }
            if (blocks.Count >= 3)
            {
                List<int> tmp = new List<int>(blocks);
                blocksOfLine.Add(tmp);
            }
            return blocksOfLine;
        }
    }

    // 블록 움직였을 때 3매치가 만들어지는지 여부 검사
    // 블록 움직여서 3매치가 하나라도 만들어지면 true
    // column%2==0 && row%2==0인 블록에 대해서만 실시
    // 해당 블록
    bool Is3MatchPossible()
    {
        // 행 단위 검사
        for (int i = 0; i < _numOfColumn; i += 2)
        {
            for (int j = 0; j < _numOfColumn; j++)
            {
                // 아랫블록과 교환했을 때 확인
                if (i < _numOfColumn - 1 && Is3MatchPossibleOnBlocks(i * _numOfColumn + j, (i + 1) * _numOfColumn + j))
                {
                    return true;
                }
                // 윗블록과 교환했을 때 확인
                if (i > 0 && Is3MatchPossibleOnBlocks(i * _numOfColumn + j, (i - 1) * _numOfColumn + j))
                {
                    return true;
                }
            }
        }
        // 열 단위 검사
        for (int i = 0; i < _numOfColumn; i += 2)
        {
            for (int j = 0; j < _numOfColumn; j++)
            {
                // 오른쪽 블록과 교환했을 때 확인
                if (i < _numOfColumn - 1 && Is3MatchPossibleOnBlocks(j * _numOfColumn + i, j * _numOfColumn + i + 1))
                {
                    return true;
                }
                // 왼쪽 블록과 교환했을 때 확인
                if (i > 0 && Is3MatchPossibleOnBlocks(j * _numOfColumn + i, j * _numOfColumn + i - 1))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // first번 블록과 second번 블록을 swap했을 때 3match가 만들어지는지 검사
    bool Is3MatchPossibleOnBlocks(int first, int second)
    {
        bool flag = false;
        int x1 = first / _numOfColumn, y1 = first % _numOfColumn;
        int x2 = second / _numOfColumn, y2 = second % _numOfColumn;
        Sprite firstImage = _blocks[x1, y1].BlockImage.sprite;
        Sprite secondImage = _blocks[x2, y2].BlockImage.sprite;
        _blocks[x1, y1].BlockImage.sprite = secondImage;
        _blocks[x2, y2].BlockImage.sprite = firstImage;

        if (CheckingRowOrColumn(x1, false).Count > 0 || CheckingRowOrColumn(x2, false).Count > 0
            || CheckingRowOrColumn(y1, true).Count > 0 || CheckingRowOrColumn(y2, true).Count > 0)
        {
            flag = true;
        }
        _blocks[x1, y1].BlockImage.sprite = firstImage;
        _blocks[x2, y2].BlockImage.sprite = secondImage;
        return flag;
    }
    #endregion

    #region Block Move & Generate & Destroy Method

    protected IEnumerator CoSwapBlocks(Block blockA, Block blockB)
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
        string clientData = $"{(int)Define.DataStatus.Swap}\n{blockA.name.Substring(5)} {blockB.name.Substring(5)}";
        Client?.SendMessageToServer(clientData);
        yield return StartCoroutine(CoSwapBlockImages(blockA, blockB));
    }

    IEnumerator CoMakeBlocks()
    {
        do
        {
            yield return StartCoroutine(CoCheckAndSupplyBlocksToColumn());
            yield return new WaitForSeconds(0.5f);
        } while (DestroyBlocks());
        _isBlockMoving = false;
        _isChecking = true;
    }

    void MakeBlocks()
    {
        _isBlockMoving = true;
        StartCoroutine(CoMakeBlocks());
    }

    // 3-match된 블록들 제거
    // 블록 제거 시, 해당 블록 위에 있던 블록들 아래로 내려오고 빈칸은 새로 생성된 블록으로 채우기
    bool DestroyBlocks()
    {
        List<List<int>> matches = CheckMatches();
        // 삭제할 게 없으면 false
        if (matches.Count == 0)
            return false;
        foreach (List<int> matchBlocks in matches)
        {
            string clientData = $"{(int)Define.DataStatus.Destroy}\n";
            for (int i = 0; i < matchBlocks.Count; i++)
            {
                if (i != 0) clientData += ' ';
                clientData += matchBlocks[i].ToString();
            }
            Client?.SendMessageToServer(clientData);
            DestroyMatchBlocks(matchBlocks);
        }
        return true;
    }

    bool DestroySwapBlocks(Block block1, Block block2)
    {
        List<List<int>> matches = new List<List<int>>();
        List<List<int>> tmp;
        int num1 = int.Parse(block1.name.Substring(5));
        int num2 = int.Parse(block2.name.Substring(5));
        tmp = CheckingRowOrColumn(num1 % _numOfColumn, true);
        if (tmp.Count > 0)
        {
            for (int j = 0; j < tmp.Count; j++)
            {
                matches.Add(tmp[j]);
            }
        }

        tmp = CheckingRowOrColumn(num1 / _numOfColumn, false);
        if (tmp.Count > 0)
        {
            for (int j = 0; j < tmp.Count; j++)
            {
                matches.Add(tmp[j]);
                //PrintList(tmp[j]);
            }
        }

        tmp = CheckingRowOrColumn(num2 % _numOfColumn, true);
        if (tmp.Count > 0)
        {
            for (int j = 0; j < tmp.Count; j++)
            {
                matches.Add(tmp[j]);
                //PrintList(tmp[j]);
            }
        }

        tmp = CheckingRowOrColumn(num2 / _numOfColumn, false);
        if (tmp.Count > 0)
        {
            for (int j = 0; j < tmp.Count; j++)
            {
                matches.Add(tmp[j]);
                //PrintList(tmp[j]);
            }
        }

        if (matches.Count == 0)
            return false;
        foreach (List<int> matchBlocks in matches)
        {
            string clientData = $"{(int)Define.DataStatus.Destroy}\n";
            for (int i = 0; i < matchBlocks.Count; i++)
            {
                if (i != 0) clientData += ' ';
                clientData += matchBlocks[i].ToString();
            }
            Client?.SendMessageToServer(clientData);
            DestroyMatchBlocks(matchBlocks);
        }
        return true;
    }

    protected override void DestroyMatchBlocks(List<int> indices)
    {
        base.DestroyMatchBlocks(indices);
        GameManager.Instance.GameStatus.PlayerScore = Score;
    }

    IEnumerator CoCheckAndSupplyBlocksToColumn()
    {
        int[] countEmpty = new int[_numOfColumn];
        bool isEmptyBlockExist = false;
        // 새로 생성될 블록을 포함하여 밑으로 이동될 블록들
        List<List<GameObject>> movingBlocks = new List<List<GameObject>>();
        string hideBlockData = $"{(int)Define.DataStatus.Hide}\n";

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
                        rect.sizeDelta = _size - _space;
                        rect.localScale = Vector3.one;
                        rect.localEulerAngles = Vector3.zero;
                        rect.localPosition = _blocks[i, j].gameObject.transform.localPosition;
                        Image image1 = block.AddComponent<Image>();
                        image1.sprite = _blocks[i, j].BlockImage.sprite;
                        movingBlocks[j].Add(block);
                        _blocks[i, j].TurnOffBlock();
                        hideBlockData += $"{i},{j} ";
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
                rect.sizeDelta = _size - _space;
                rect.localScale = Vector3.one;
                rect.localEulerAngles = Vector3.zero;
                rect.localPosition = new Vector3(columnTopBlockPosition.x, columnTopBlockPosition.y + (i + 1) * (_size.y + _space.y), 0);
                Image image1 = block.AddComponent<Image>();
                image1.sprite = GameManager.Instance.BlockImages[UnityEngine.Random.Range(0, GameManager.Instance.BlockImages.Length)];
                movingBlocks[j].Add(block);
            }
        }
        int sumOfCountEmpty = 0;
        for (int i = 0; i < countEmpty.Length; i++)
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
            string clientData = $"{(int)Define.DataStatus.Generate}\n";
            foreach (List<GameObject> gameObjects in movingBlocks)
            {
                foreach (GameObject go in gameObjects)
                {
                    string name = go.GetComponent<Image>().sprite.name;
                    Vector2 pos = go.GetComponent<RectTransform>().localPosition;
                    // 이미지 번호 - 위치 x좌표 - 위치 y좌표
                    clientData += $"{name[0]} {pos.x} {pos.y}";
                    // 배열의 마지막 요소를 가리키는 인덱스 표기법
                    if (go != gameObjects[^1])
                    {
                        clientData += ',';
                    }
                }
                if (gameObjects != movingBlocks[^1])
                {
                    clientData += '\n';
                }
            }
            Client?.SendMessageToServer(hideBlockData);
            Client?.SendMessageToServer(clientData);
            // 블록들 밑으로 내려주기
            yield return StartCoroutine(CoMoveBlocks(movingBlocks));
        }
    }
    #endregion

    #region Test Code
    public void OnCheckButtonClick()
    {
        Color color = CheckResultText.color;
        color.a = 1f;
        CheckResultText.color = color;
        if (Is3MatchPossible())
        {
            CheckResultText.text = Define.PossibleText;
        }
        else
        {
            CheckResultText.text = Define.ImpossibleText;
        }
        StartCoroutine(CoTextVanish());
    }

    IEnumerator CoTextVanish()
    {
        Color color = CheckResultText.color;
        yield return new WaitForSeconds(2f);
        color.a = 0f;
        CheckResultText.color = color;
    }

    public void OnChangeButtonClick()
    {
        StartCoroutine(CoChangeAllBlocks());
    }

    IEnumerator CoChangeAllBlocks()
    {
        Color color = CheckResultText.color;
        color.a = 1f;
        CheckResultText.color = color;
        CheckResultText.text = Define.NewBoardText;
        StartCoroutine(CoTextVanish());
        yield return new WaitForSeconds(2f);
        StartCoroutine(ChangeAllBlockImages());
    }

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

    public void PrintList(List<int> list)
    {
        string print = "Match: ";
        for (int i = 0; i < list.Count; i++)
        {
            print += list[i].ToString();
            print += " ";
        }
        Debug.Log(print);
    }
    #endregion
}
