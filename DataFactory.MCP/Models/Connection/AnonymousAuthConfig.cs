namespace DataFactory.MCP.Models.Connection;

/// <summary>
/// Anonymous authentication configuration
/// </summary>
public class AnonymousAuthConfig : IAuthConfig
{
    public CredentialType Type => CredentialType.Anonymous;
    public SingleSignOnType SingleSignOnType => SingleSignOnType.None;
    public ConnectionEncryption ConnectionEncryption { get; }
    public bool SkipTestConnection { get; }

    public AnonymousAuthConfig(ConnectionEncryption connectionEncryption = ConnectionEncryption.NotEncrypted,
        bool skipTestConnection = false)
    {
        ConnectionEncryption = connectionEncryption;
        SkipTestConnection = skipTestConnection;
    }

    public Credentials GetCredentials()
    {
        return new AnonymousCredentials();
    }

    public static AnonymousAuthConfig Create(bool skipTestConnection = false)
    {
        return new AnonymousAuthConfig(ConnectionEncryption.NotEncrypted, skipTestConnection);
    }
}