namespace Owh.OlivawCore.Settings;

public interface ISettingProviderAuthentication : ISetting
{
    public string Login { get; }
    public string Password { get; }
    public int CacheSlidingExpiration { get; }
    public string CacheKey { get; }
}