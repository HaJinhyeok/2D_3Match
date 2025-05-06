using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RivalBoard : Board
{
    public static bool s_isMoving = false;

    // Start(게임 시작) 들어왔을 때
    public void StartGame(string data)
    {
        int x, y;
        for (int i = 0; i < _blocks.Length; i++)
        {
            x = i / _numOfColumn;
            y = i % _numOfColumn;
            GameObject block = Instantiate(GameManager.Instance.BlockPrefab, transform);
            RectTransform rect = block.GetComponent<RectTransform>();
            rect.localPosition = CalculatePosition(i);
            rect.sizeDelta = _blockSize;

            _blocks[x, y] = block.GetComponent<Block>();
            _blocks[x, y].SetBlockImagePadding(_space);
            _blockDictionary.Add(block, _blocks[x, y]);
            block.name = $"RivalBlock{i}";
        }
        int j = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != ' ' && data[i] != '\n')
            {
                _blocks[j / _numOfColumn, j % _numOfColumn].UpdateBlockImage(GameManager.Instance.BlockImages[data[i] - 48]);
                j++;
            }
        }
        Score = 0;
        ScoreText.text = $"SCORE: {Score}";
    }

    // Swap 들어왔을 때
    public IEnumerator SwapBlock(string data)
    {
        string[] pos = data.Split(' ');
        int x = int.Parse(pos[0]);
        int y = int.Parse(pos[1]);
        s_isMoving = true;
        yield return StartCoroutine(CoSwapBlockImages(_blocks[x / _numOfColumn, x % _numOfColumn], _blocks[y / _numOfColumn, y % _numOfColumn]));
        s_isMoving = false;
    }

    // Destroy 들어왔을 때
    public void DestroyBlock(string data)
    {
        List<int> matchBlocks = new List<int>();
        string[] indices = data.Split(' ');
        foreach (string str in indices)
        {
            matchBlocks.Add(int.Parse(str));
        }
        DestroyMatchBlocks(matchBlocks);
    }

    protected override void DestroyMatchBlocks(List<int> indices)
    {
        base.DestroyMatchBlocks(indices);
        GameManager.Instance.GameStatus.RivalScore = Score;
    }

    // Generate 들어왔을 때
    public void GenerateBlock(string data)
    {
        StartCoroutine(CoMoveBlocks(MakeMovingBlocksList(data)));
    }

    // Hide 들어왔을 때
    public void HideBlock(string data)
    {
        string[] blockPos = data.Split(' ');
        foreach (string pos in blockPos)
        {
            if (string.IsNullOrEmpty(pos))
                continue;
            else
            {
                string[] coord = pos.Split(',');
                _blocks[int.Parse(coord[0]), int.Parse(coord[1])].TurnOffBlock();
            }
        }
    }

    // Finish 들어왔을 때?
    public void FinishGame()
    {
        ClearBoard();
    }

    // 받아온 데이터를 이용해 움직일 블록 이미지 정보 가져오기
    List<List<GameObject>> MakeMovingBlocksList(string data)
    {
        List<List<GameObject>> movingBlocks = new List<List<GameObject>>();
        string[] blockInfoLine = data.Split('\n');
        // block numbering
        int i = 0;
        // column index numbering
        int idx = 0;
        foreach (string str in blockInfoLine)
        {
            movingBlocks.Add(new List<GameObject>());
            if (string.IsNullOrEmpty(str))
            {
                idx++;
                continue;
            }
            string[] blockInfos = str.Split(',');
            foreach (string blockInfo in blockInfos)
            {
                string[] info = blockInfo.Split(' ');
                GameObject go = new GameObject("RivalMovingBlock" + i.ToString());
                i++;
                go.transform.SetParent(transform);
                RectTransform rect = go.AddComponent<RectTransform>();
                rect.sizeDelta = _size - _space;
                rect.localScale = Vector3.one;
                rect.localEulerAngles = Vector3.zero;
                rect.localPosition = new Vector3(float.Parse(info[1]), float.Parse(info[2]), 0);
                Image image = go.AddComponent<Image>();
                image.sprite = GameManager.Instance.BlockImages[int.Parse(info[0])];
                movingBlocks[idx].Add(go);
            }
            idx++;
        }
        return movingBlocks;
    }
}
