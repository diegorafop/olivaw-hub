using Microsoft.AspNetCore.Http;
using Moq;
using Owh.OlivawCore.Api.Extensions.Https;
using System.Text;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class AdditionalHttpContextExtensionsTest
{
    [Fact]
    public void DeserializeXml_should_return_null_for_a_nil_root()
    {
        var context = new DefaultHttpContext();
        using var body = new MemoryStream(Encoding.UTF8.GetBytes(
            "<XmlPayload xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:nil=\"true\" />"));
        context.Request.Body = body;
        Assert.Null(context.DeserializeXml<HttpContextInternalExtensionsTest.XmlPayload>());
    }

    [Fact]
    public void GetToken_should_tolerate_incomplete_custom_contexts()
    {
        var context = new Mock<HttpContext>();
        Assert.Equal(string.Empty, context.Object.GetToken());
        context.SetupGet(item => item.Request).Returns(Mock.Of<HttpRequest>());
        Assert.Equal(string.Empty, context.Object.GetToken());
    }

    [Theory]
    [InlineData("e30.e30.invalid")]
    [InlineData("e30.eyJleHAiOiJpbnZhbGlkIn0.invalid")]
    public void JwtTokenIsValid_should_reject_tokens_without_valid_expiration(string token)
    {
        Assert.False(token.JwtTokenIsValid());
    }
}