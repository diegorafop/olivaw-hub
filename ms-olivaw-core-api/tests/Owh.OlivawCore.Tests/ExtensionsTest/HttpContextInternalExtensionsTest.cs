using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Owh.OlivawCore.Api.Extensions.Https;
using Owh.OlivawCore.Tests.TestSupport;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class HttpContextInternalExtensionsTest
{
    [Fact]
    public void DeserializeXml_should_read_request_body_and_handle_missing_context()
    {
        var context = new DefaultHttpContext();
        using var body = new MemoryStream(Encoding.UTF8.GetBytes("<XmlPayload><Name>OlivawCore</Name></XmlPayload>"));
        context.Request.Body = body;
        Assert.Equal("OlivawCore", context.DeserializeXml<XmlPayload>().Name);
        Assert.Null(((HttpContext)null!).DeserializeXml<XmlPayload>());
    }

    [Fact]
    public void GetToken_and_GetValue_should_read_bearer_header_and_claims()
    {
        var context = new DefaultHttpContext();
        var token = TokenFactory.Create(DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddHours(1), new Claim("name", "OlivawCore"));
        context.Request.Headers.Authorization = "Bearer " + token;
        Assert.Equal("Bearer " + token, context.GetToken());
        Assert.Equal("OlivawCore", context.GetValue("name"));
        Assert.Equal(string.Empty, context.GetValue("missing"));
        Assert.Equal(string.Empty, new DefaultHttpContext().GetValue("name"));
        Assert.Equal(string.Empty, ((HttpContext)null!).GetValue("name"));
        Assert.Equal(string.Empty, ((HttpContext)null!).GetToken());
    }

    [Fact]
    public void GetUserProfileId_should_use_explicit_id_or_configured_claim()
    {
        var claimId = Guid.NewGuid();
        var explicitId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["authentication:oidc:standard-claims:sub"] = "sub"
        }).Build();
        context.Request.Headers.Authorization = TokenFactory.Create(DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow.AddHours(1), new Claim("sub", claimId.ToString()));
        Assert.Equal(explicitId, context.GetUserProfileId(configuration, explicitId));
        Assert.Equal(claimId, context.GetUserProfileId(configuration));
        Assert.Equal(claimId, context.GetUserProfileId(configuration, null));
        Assert.Equal(claimId, context.GetUserProfileId(configuration, Guid.Empty));
        Assert.Equal(Guid.Empty, ((HttpContext)null!).GetUserProfileId(configuration));
        Assert.Equal(Guid.Empty, ((HttpContext)null!).GetUserProfileId(configuration, null));
    }

    public class XmlPayload
    {
        public string Name { get; set; } = "";
    }
}