using Owh.OlivawCore.Api.Extensions.Serializations;
using Owh.OlivawCore.Requests;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class AuthenticationContractTest
{
    [Fact]
    public void Authentication_response_should_preserve_all_fields_during_json_roundtrip()
    {
        var source = new AuthenticateUserCommandResponse
        {
            IsActive = true,
            UserProfileId = Guid.NewGuid(),
            Token = "test-only",
            TokenRefresh = "test-only-refresh"
        };

        var response = source
            .Serialize()
            .Deserialize<AuthenticateUserCommandResponse>();

        Assert.True(response.IsActive);
        Assert.Equal(source.UserProfileId, response.UserProfileId);
        Assert.Equal(source.Token, response.Token);
        Assert.Equal(source.TokenRefresh, response.TokenRefresh);
    }
}