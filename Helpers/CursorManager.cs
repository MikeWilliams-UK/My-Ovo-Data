using System.Windows;
using System.Windows.Input;

namespace OvoData.Helpers;

public static class CursorManager
{
    private static Cursor _originalOverride;

    public static void SetWaitCursorExcept(params UIElement[] exceptions)
    {
        _originalOverride = Mouse.OverrideCursor;
        Mouse.OverrideCursor = Cursors.Wait;

        foreach (var control in exceptions)
        {
            control.MouseEnter += SuppressOverride;
            control.MouseLeave += RestoreOverride;
        }
    }

    public static void ClearWaitCursor(params UIElement[] exceptions)
    {
        Mouse.OverrideCursor = null;

        foreach (var control in exceptions)
        {
            control.MouseEnter -= SuppressOverride;
            control.MouseLeave -= RestoreOverride;
        }
    }

    private static void SuppressOverride(object sender, MouseEventArgs e)
    {
        Mouse.OverrideCursor = null;
    }

    private static void RestoreOverride(object sender, MouseEventArgs e)
    {
        Mouse.OverrideCursor = _originalOverride ?? Cursors.Wait;
    }
}