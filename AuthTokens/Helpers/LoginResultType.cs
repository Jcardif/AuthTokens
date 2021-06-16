namespace AuthTokens.Helpers
{
    public enum LoginResultType
    {
        Success,
        Unauthorized,
        CancelledByUser,
        NoNetworkAvailable,
        UnknownError
    }
}
