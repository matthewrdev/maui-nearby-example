#if __ANDROID__


namespace MauiNearby.Services
{
    public enum ConnectionState
    {
        Idle = 1023,
        Ready = 1024,
        Advertising = 1025,
        Discovering = 1026,
        Connected = 1027
    }
}

#endif