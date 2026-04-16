using PluginContracts;

namespace PluginLibrary;

public class MathOperations : IReflectivePlugin
{
    public int Offset { get; }
    public double Multiplier { get; }

    public MathOperations(int offset, double multiplier)
    {
        Offset = offset;
        Multiplier = multiplier;
    }

    public int Add(int a, int b)
    {
        return (int)Math.Round((a + b) * Multiplier + Offset);
    }

    public double Divide(double a, double b)
    {
        if (Math.Abs(b) < double.Epsilon)
        {
            throw new ArgumentException("Деление на ноль недопустимо.");
        }

        return (a / b) * Multiplier + Offset;
    }

    public bool IsEven(int number)
    {
        var normalized = (int)Math.Round(number * Multiplier + Offset);
        return normalized % 2 == 0;
    }
}

public enum SampleColor
{
    Red,
    Green,
    Blue
}

public class StringTools : IReflectivePlugin
{
    public string Prefix { get; }

    public StringTools(string prefix)
    {
        Prefix = prefix ?? string.Empty;
    }

    public string Concat(string a, string b)
    {
        return $"{Prefix}{a ?? string.Empty}{b ?? string.Empty}";
    }

    public string Repeat(string text, int count)
    {
        if (count < 0)
        {
            throw new ArgumentException("count не может быть отрицательным.");
        }

        return $"{Prefix}{string.Concat(Enumerable.Repeat(text ?? string.Empty, count))}";
    }

    public int Length(string text) => (text ?? string.Empty).Length;
}

public class ColorTools : IReflectivePlugin
{
    private readonly SampleColor _baseColor;

    public ColorTools(SampleColor baseColor)
    {
        _baseColor = baseColor;
    }

    public string DescribeColor(SampleColor color)
    {
        var baseName = ColorToString(_baseColor);
        var colorName = ColorToString(color);
        return $"base={baseName}, color={colorName}";
    }

    private static string ColorToString(SampleColor color) => color switch
    {
        SampleColor.Red => "Красный",
        SampleColor.Green => "Зелёный",
        SampleColor.Blue => "Синий",
        _ => "Неизвестный"
    };
}
