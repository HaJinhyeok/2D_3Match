using System;

public static class Define
{
    #region
    public const int PORT = 9000;
    public const string address = "127.0.0.1";
    #endregion

    #region Constants
    public const float TimeLimit = 60f;
    #endregion

    #region Path
    public const string BlockPath = "Prefab/Block";
    public const string BlockImagePath = "Fruits";
    #endregion

    #region Scene
    public const string MainScene = "Main";
    public const string SoloGameScene = "SoloGame";
    public const string MatchGameScene = "MatchGame";
    #endregion

    #region Status Text
    public const string PossibleText = "Possible";
    public const string ImpossibleText = "Impossible";
    public const string NewBoardText = "3매치 불가\n보드 교체";

    public const string WinText = "승리!";
    public const string LoseText = "패배...";
    public const string DrawText = "무승부";
    public const string FinishText = "게임 종료";

    public const string RivalConnectionError = "RIVAL_CONNECTION_ERROR";
    public const string WaitingText = "상대 입장 대기중...";
    public const string WaitingRivalFinishText = "상대 플레이어의 게임이 끝나길 기다리는 중";
    public const string RivalConnectionFailText = "상대방의 연결이 끊어졌습니다.";
    public const string ServerConnectionFailText = "서버와의 연결이 끊어졌습니다.";
    #endregion
}
