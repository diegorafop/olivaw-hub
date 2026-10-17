namespace Mps.OlivawCore.Settings;

public interface ISettingProviderAuthentication : ISetting
{
    public string Login { get; }
    public string Senha { get; }
    public int CacheSlidingExpiration { get; }
    public string CacheKey { get; }
}