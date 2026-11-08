using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using FluentValidation.TestHelper;
using Xunit;
using Owh.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UploadEventPhotosValidatorTest
{
    [Fact]
    public void Validate_should_require_id_and_files()
    {
        var result = new UploadEventPhotosValidator().TestValidate(new UploadEventPhotosCommand());
        result.ShouldHaveValidationErrorFor(item => item.Id);
        result.ShouldHaveValidationErrorFor(item => item.Files);
    }
    [Fact]
    public void Validate_should_reject_large_files_and_batches()
    {
        var request = new UploadEventPhotosCommand { Id = Guid.NewGuid(), Files = Enumerable.Range(0, 21)
            .Select(index => (IFormFile)new FormFile(Stream.Null, 0, 11 * 1024 * 1024, "files", "photo.jpg")).ToList() };
        Assert.False(new UploadEventPhotosValidator().Validate(request).IsValid);
    }
    [Fact]
    public void Validate_should_accept_valid_batch()
    {
        var request = new UploadEventPhotosCommand { Id = Guid.NewGuid(), Files = [new FormFile(Stream.Null, 0, 100, "files", "photo.jpg")] };
        new UploadEventPhotosValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
