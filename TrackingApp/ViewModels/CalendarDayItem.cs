using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace TrackingApp.ViewModels;

/// <summary>
/// Represents a single cell in the inline calendar grid.
/// Properties are computed once at build time; the calendar is rebuilt on any change.
/// </summary>
public class CalendarDayItem
{
    public DateTime? Date          { get; init; }
    public string    Label         { get; init; } = "";
    public bool      IsCurrentMonth { get; init; }
    public bool      IsSelected    { get; init; }

    public bool IsEmpty   => !Date.HasValue;
    public bool IsToday   => Date?.Date == DateTime.Today;
    public bool IsFuture  => Date.HasValue && Date.Value.Date > DateTime.Today;

    // ── Visual properties bound directly in XAML ──────────────────────────────

    public Color DayBackground => IsSelected
        ? Color.FromArgb("#2a3d66")
        : Colors.Transparent;

    public Color DayTextColor => IsSelected
        ? Colors.White
        : IsToday
            ? Color.FromArgb("#1565C0")
            : !IsCurrentMonth
                ? Color.FromArgb("#BBBBBB")
                : Color.FromArgb("#212121");

    public Color DayStroke => IsToday && !IsSelected
        ? Color.FromArgb("#2a3d66")
        : Colors.Transparent;

    public double DayOpacity => (IsEmpty || IsFuture) ? 0.25 : 1.0;

    public FontAttributes DayFontAttributes =>
        (IsToday || IsSelected) ? FontAttributes.Bold : FontAttributes.None;
}
