public static class Define
{
    public enum DataStatus : int
    {
        Start,
        Swap,
        Destroy,
        Generate,
        Hide,
    }

    #region Path
    public const string BlockPath = "Prefab/Block";
    public const string BlockImagePath = "Fruits";
    #endregion

    #region Scene
    public const string MainScene = "Main";
    public const string GameScene = "Game";
    #endregion
}
