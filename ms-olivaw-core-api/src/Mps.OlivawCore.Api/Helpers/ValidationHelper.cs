namespace Mps.OlivawCore.Api.Helpers;

public static class ValidationHelper
{
    public static void ThrowIfNull<T>(T request)
    {
        ArgumentNullException.ThrowIfNull
        (
            request,
            MessageHelper.NullFor<T>()
        );
    }
}