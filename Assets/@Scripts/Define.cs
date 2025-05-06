public static class Define
{
    public enum DataStatus : int
    {
        Start,
        Swap,
        Destroy,
        Generate,
        Hide,

        Finish,
        RivalConnectionClosed,
    }

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
    public const string NewBoardText = "불판 교체";

    public const string VictoryText = "승리!";
    public const string LoseText = "패배...";
    public const string DrawText = "무승부";

    public const string RivalConnectionError = "RIVAL_CONNECTION_ERROR";
    public const string WaitingText = "상대 입장 대기중...";
    public const string RivalConnectionFailText = "상대방의 연결이 끊어졌습니다.";
    public const string ServerConnectionFailText = "서버와의 연결이 끊어졌습니다.";
    #endregion
}
