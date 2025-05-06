using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    protected Dictionary<GameObject, Block> _blockDictionary = new Dictionary<GameObject, Block>();
    protected Block[,] _blocks = new Block[_numOfColumn, _numOfColumn];

    protected Vector2 _start;
    protected Vector2 _boardSize;
    protected Vector2 _blockSize;
    protected Vector2 _size;
    protected Vector2 _space;
    protected const int _numOfColumn = 7;

    int _score;

    public Text ScoreText;
    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }

    protected virtual void Start()
    {
        _boardSize = GetComponent<RectTransform>().sizeDelta;
        _blockSize = _boardSize / _numOfColumn;
        _space = _blockSize / 10f;
        _size = _blockSize - _space;
        _start = new Vector2(-_boardSize.x / 2 + _blockSize.x / 2, _boardSize.x / 2 - _blockSize.x / 2);
    }

    protected Vector2 CalculatePosition(int idx)
    {
        float posX = _start.x + (_size.x + _space.x) * (idx % _numOfColumn);
        float posY = _start.y - (_size.y + _space.y) * (idx / _numOfColumn);
        return new Vector2(posX, posY);
    }

    // 공통: 블록 이미지 교환하기
    protected IEnumerator CoSwapBlockImages(Block blockA, Block blockB)
    {
        Sprite blockImage1 = blockA.BlockImage.sprite;
        Sprite blockImage2 = blockB.BlockImage.sprite;

        // 각 블록 위치에 이미지 오브젝트 생성
        GameObject block1 = new GameObject("image1");
        block1.transform.SetParent(transform);
        RectTransform rect1 = block1.AddComponent<RectTransform>();
        rect1.sizeDelta = _size - _space;
        rect1.localScale = Vector3.one;
        rect1.localEulerAngles = Vector3.zero;
        rect1.localPosition = blockA.transform.localPosition;
        Image image1 = block1.AddComponent<Image>();
        image1.sprite = blockImage1;

        GameObject block2 = new GameObject("image2");
        block2.transform.SetParent(transform);
        RectTransform rect2 = block2.AddComponent<RectTransform>();
        rect2.sizeDelta = _size - _space;
        rect2.localScale = Vector3.one;
        rect2.localPosition = blockB.transform.localPosition;
        Image image2 = block2.AddComponent<Image>();
        image2.sprite = blockImage2;

        // 각 블록의 이미지 제거
        blockA.TurnOffBlock();
        blockB.TurnOffBlock();

        // 서로의 위치로 이미지가 이동할 때까지 yield return null
        Vector3 posA = block1.transform.localPosition;
        Vector3 posB = block2.transform.localPosition;
        while (Vector3.Distance(block1.transform.localPosition, posB) > 5f)
        {
            blockA.TurnOffBlock();
            blockB.TurnOffBlock();
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
        blockA.TurnOnBlock();
        blockB.TurnOnBlock();
    }

    // 공통: 매칭된 블록 이미지 삭제하기
    protected virtual void DestroyMatchBlocks(List<int> indices)
    {
        foreach (int idx in indices)
        {
            if (!_blocks[idx / _numOfColumn, idx % _numOfColumn].IsEmpty)
            {
                _blocks[idx / _numOfColumn, idx % _numOfColumn].TurnOffBlock();
                _blocks[idx / _numOfColumn, idx % _numOfColumn].BlockCrash();
                _score++;
            }
        }
        ScoreText.text = $"SCORE: {_score}";
    }

    protected IEnumerator CoMoveBlocks(List<List<GameObject>> movingBlocks)
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

    // 공통: 보드판 초기화
    protected void ClearBoard()
    {
        foreach(KeyValuePair<GameObject,Block> keyValue in _blockDictionary)
        {
            Destroy(keyValue.Key);
        }
        for (int i = 0; i < _numOfColumn; i++)
        {
            for (int j = 0; j < _numOfColumn; j++)
            {
                Destroy(_blocks[i, j]);
            }
        }
    }
}
