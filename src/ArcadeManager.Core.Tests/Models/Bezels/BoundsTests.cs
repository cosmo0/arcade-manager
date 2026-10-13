using ArcadeManager.Core.Models.Bezels;

namespace ArcadeManager.Core.Tests.Models.Bezels;

public class BoundsTests
{
    [Fact]
    public void ApplyMargin_applies_margins_if_enough_space()
    {
        // arrange
        var sut = new Bounds { X = 0, Y = 0, Width = 10, Height = 10 };

        // act
        var result = sut.ApplyMargin(10);

        // assert
        result.X.Should().Be(-10);
        result.Y.Should().Be(-10);
        result.Width.Should().Be(30);
        result.Height.Should().Be(30);
    }

    [Fact]
    public void Center_is_computed()
    {
        // arrange
        var sut = new Bounds { X = 0, Y = 0, Width = 10, Height = 10 };

        // act
        var result = sut.Center;

        // assert
        result.X.Should().Be(5);
        result.Y.Should().Be(5);
    }

    [Theory]
    [InlineData(10, 5, false)]
    [InlineData(5, 10, true)]
    public void Orientation_is_computed(int width, int height, bool isVertical)
    {
        // arrange
        var sut = new Bounds { Width = width, Height = height };

        // act
        var result = sut.Orientation;

        // assert
        result.Should().Be(isVertical ? Orientation.Vertical : Orientation.Horizontal);
    }

    [Fact]
    public void ToShortString_returns()
    {
        // arrange
        var sut = new Bounds { X = 0, Y = 0, Width = 10, Height = 10 };

        // act
        var result = sut.ToShortString();

        // assert
        result.Should().Be("{ X: 0, Y: 0, Width: 10, Height: 10 }");
    }
}