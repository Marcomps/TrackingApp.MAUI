using Xunit;
using TrackingApp.Helpers;
using TrackingApp.Models;
using FluentAssertions;

namespace TrackingApp.Tests.ViewModels;

public class ResumenCalculatorTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static FoodEntry Formula(decimal ml) => new()
    {
        TipoAlimentacion = TipoAlimentacion.Formula,
        CantidadMl       = ml,
        Time             = DateTime.Today
    };

    private static FoodEntry Lactancia(int minutos) => new()
    {
        TipoAlimentacion = TipoAlimentacion.Lactancia,
        DuracionMinutos  = minutos,
        Time             = DateTime.Today
    };

    private static FoodEntry Solido(decimal gramos) => new()
    {
        TipoAlimentacion  = TipoAlimentacion.Solido,
        CantidadGramos    = gramos,
        Time              = DateTime.Today
    };

    private static FoodEntry Personalizado(string nombre, decimal ml) => new()
    {
        TipoAlimentacion = TipoAlimentacion.Personalizado,
        FoodType         = nombre,
        CantidadMl       = ml,
        Time             = DateTime.Today
    };

    private static FoodEntry PersonalizadoConUnidad(string nombre, double amount, string unit) => new()
    {
        TipoAlimentacion = TipoAlimentacion.Personalizado,
        FoodType         = nombre,
        Amount           = amount,
        Unit             = unit,
        Time             = DateTime.Today
    };

    // ── Formula ───────────────────────────────────────────────────────────────

    [Fact]
    public void Formula_FiveTakes_SumsPrecisely()
    {
        var entries = new[] { Formula(120), Formula(130), Formula(125), Formula(110), Formula(115) };

        var result = ResumenCalculator.Compute(entries);

        result.Should().HaveCount(1);
        result[0].Total.Should().Be("600 ml");
        result[0].Tomas.Should().Be("5 tomas");
    }

    [Fact]
    public void Formula_DecimalAmounts_NoRounding()
    {
        // 3 takes of 125.5 ml = 376.5 ml  (NOT 376 or 377)
        var entries = new[] { Formula(125.5m), Formula(125.5m), Formula(125.5m) };

        var result = ResumenCalculator.Compute(entries);

        result.Should().HaveCount(1);
        result[0].Total.Should().Be("376.5 ml");
    }

    [Fact]
    public void Formula_IntegerAmounts_NoDecimalPoint()
    {
        // 2 takes of 150 ml = 300 ml (should NOT show "300.00")
        var entries = new[] { Formula(150m), Formula(150m) };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("300 ml");
    }

    [Fact]
    public void Formula_SingleTake_ShowsOneToma()
    {
        var result = ResumenCalculator.Compute(new[] { Formula(200m) });

        result[0].Tomas.Should().Be("1 toma");
    }

    // ── Lactancia ─────────────────────────────────────────────────────────────

    [Fact]
    public void Lactancia_SumsMinutes()
    {
        var entries = new[] { Lactancia(15), Lactancia(20), Lactancia(10) };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("45 min");
    }

    // ── Sólido ────────────────────────────────────────────────────────────────

    [Fact]
    public void Solido_DecimalGrams_NoRounding()
    {
        // 2 takes of 85.5g = 171 g  (not 170 or 172)
        var entries = new[] { Solido(85.5m), Solido(85.5m) };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("171 g");
    }

    [Fact]
    public void Solido_Label_IsGrams()
    {
        var entries = new[] { Solido(50m), Solido(75.3m) };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("125.3 g");
    }

    // ── Multiple types in same day ────────────────────────────────────────────

    [Fact]
    public void MultipleTypes_ReturnsOneRowPerType()
    {
        var entries = new FoodEntry[]
        {
            Formula(120m), Formula(130m),
            Lactancia(20),
            Solido(50m)
        };

        var result = ResumenCalculator.Compute(entries);

        result.Should().HaveCount(3);
        result.Select(r => r.TipoDisplay).Should().Contain(new[]
        {
            "Lactancia materna", "Fórmula / Biberón", "Alimentación sólida"
        });
    }

    // ── Personalizado ─────────────────────────────────────────────────────────

    [Fact]
    public void Personalizado_TwoDifferentFoods_TwoRows()
    {
        var entries = new FoodEntry[]
        {
            Personalizado("Papilla manzana", 80m),
            Personalizado("Papilla manzana", 80m),
            Personalizado("Yogur",            50m),
        };

        var result = ResumenCalculator.Compute(entries);

        result.Should().HaveCount(2);
        result.First(r => r.TipoDisplay == "Papilla manzana").Total.Should().Be("160 ml");
        result.First(r => r.TipoDisplay == "Yogur").Total.Should().Be("50 ml");
    }

    [Fact]
    public void Personalizado_DecimalMl_NoPrecisionLoss()
    {
        // 3 × 33.3 ml = 99.9 ml (NOT 99 or 100)
        var entries = new[]
        {
            Personalizado("Zumo", 33.3m),
            Personalizado("Zumo", 33.3m),
            Personalizado("Zumo", 33.3m),
        };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("99.9 ml");
    }

    [Fact]
    public void Personalizado_WithUnit_SumsCorrectly()
    {
        var entries = new[]
        {
            PersonalizadoConUnidad("Cereal", 2.5, "oz"),
            PersonalizadoConUnidad("Cereal", 2.5, "oz"),
        };

        var result = ResumenCalculator.Compute(entries);

        result[0].Total.Should().Be("5 oz");
    }

    // ── Empty list ────────────────────────────────────────────────────────────

    [Fact]
    public void EmptyEntries_ReturnsEmptyList()
    {
        var result = ResumenCalculator.Compute(Array.Empty<FoodEntry>());

        result.Should().BeEmpty();
    }
}
