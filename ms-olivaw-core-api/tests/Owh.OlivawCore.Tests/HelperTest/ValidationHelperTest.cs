using Owh.OlivawCore.Api.Helpers;
using Xunit;

namespace Owh.OlivawCore.Tests.HelperTest;

public class ValidationHelperTest
{
    [Fact]
    public void ThrowIfNull_should_reject_null_with_the_shared_message()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => ValidationHelper.ThrowIfNull<object>(null!));
        Assert.Equal(MessageHelper.NullFor<object>(), exception.ParamName);
    }

    [Fact]
    public void ThrowIfNull_should_accept_a_value()
    {
        ValidationHelper.ThrowIfNull(new object());
        ValidationHelper.ThrowIfNull(0);
    }
}
