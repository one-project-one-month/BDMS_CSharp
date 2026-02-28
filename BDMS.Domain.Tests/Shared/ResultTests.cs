using BDMS.Shared;

namespace BDMS.Domain.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Success_Should_SetSuccessFlags_AndData()
    {
        var result = Result<string>.Success("ok", "done");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal("ok", result.Data);
        Assert.Equal("done", result.Message);
        Assert.Equal(EnumRespType.Success, result.GetEnumRespType());
    }

    [Fact]
    public void NotFound_Should_SetNotFoundState()
    {
        var result = Result<object>.NotFound("not found");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.True(result.IsNotFound());
        Assert.Equal(EnumRespType.NotFound, result.GetEnumRespType());
    }

    [Fact]
    public void ValidationError_Should_SetValidationErrorState()
    {
        var result = Result<int>.ValidationError("invalid", 10);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.True(result.IsValidationError());
        Assert.Equal(10, result.Data);
        Assert.Equal(EnumRespType.ValidationError, result.GetEnumRespType());
    }
}
