using System;
using BrilliantMessaging.GuardClauses;
using BrilliantMessaging.GuardClauses.ExceptionFactory;
using BrilliantMessaging.GuardClauses.Exceptions;
using FluentAssertions;
using Xunit;

namespace BrilliantMessaging.Core.Tests.GuardClauses;

public sealed class GuardClausesPublicApiTests
{
    [Fact]
    public void GeneratedSurface_IsPublicFromConsumerAssembly()
    {
        typeof(Check).IsPublic.Should().BeTrue();
        typeof(Throw).IsPublic.Should().BeTrue();
        typeof(WhiteSpaceStringException).IsPublic.Should().BeTrue();
    }

    [Fact]
    public void CallerArgumentExpression_CapturesOriginalExpression()
    {
        string? value = null;

        var act = () => value.MustNotBeNull();

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Fact]
    public void SelectedExceptionFactoryOverload_ThrowsDomainException()
    {
        string? value = null;

        var act = () => value.MustNotBeNull(
            static () => new InvalidOperationException("domain failure")
        );

        act.Should().Throw<InvalidOperationException>().WithMessage("domain failure");
    }

    [Fact]
    public void RepresentativeAssertions_UseGeneratedExceptionTaxonomy()
    {
        string? nullText = null;
        var nullValue = () => nullText.MustNotBeNull();
        var whiteSpace = () => " ".MustNotBeNullOrWhiteSpace();
        var numeric = () => 0.MustBePositive();
        var enumeration = () => ((DayOfWeek) 99).MustBeValidEnumValue();
        var type = () => new object().MustBeOfType<string>();
        var uri = () => "http://[::1".MustBeUri();

        nullValue.Should().Throw<ArgumentNullException>();
        whiteSpace.Should().Throw<WhiteSpaceStringException>();
        numeric.Should().Throw<ArgumentOutOfRangeException>();
        enumeration.Should().Throw<EnumValueNotDefinedException>();
        type.Should().Throw<TypeCastException>();
        uri.Should().Throw<InvalidUriException>();
    }

    [Fact]
    public void StateAndDisposalAssertions_ThrowFrameworkExceptions()
    {
        var invalidState = () => Check.InvalidOperation(true, "invalid state");
        var disposed = () => Check.ObjectDisposed(true, "SampleResource");

        invalidState.Should().Throw<InvalidOperationException>().WithMessage("invalid state");
        disposed.Should().Throw<ObjectDisposedException>()
           .Where(exception => exception.ObjectName == "SampleResource");
    }
}
