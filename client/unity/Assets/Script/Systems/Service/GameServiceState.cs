namespace Service
{
    public enum GameServiceState
    {
        Disconnected,
        HttpAuth,
        HttpAuthSuccess,
        HttpAuthFailed,
        GameLogin,
        GameLoginSuccess,
        GameLoginFailed,
        GameKicked,
    }
}