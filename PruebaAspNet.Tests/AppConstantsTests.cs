using FluentAssertions;
using PruebaAspNet.Constants;

namespace PruebaAspNet.Tests;

public class AppConstantsTests
{
    [Fact]
    public void MaxFailedAttempts_ShouldBePositive()
    {
        AppConstants.MaxFailedAttempts.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LockoutMinutes_ShouldBePositive()
    {
        AppConstants.LockoutMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SessionTimeoutMinutes_ShouldBePositive()
    {
        AppConstants.SessionTimeoutMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BcryptWorkFactor_ShouldBeReasonable()
    {
        AppConstants.BcryptWorkFactor.Should().BeInRange(10, 14);
    }

    [Fact]
    public void RateLimitPermitLimit_ShouldBePositive()
    {
        AppConstants.RateLimitPermitLimit.Should().BeGreaterThan(0);
    }
}
