using SukiUI.Toasts;

namespace WisdomWarrior.Editor.Core;

public static class EditorUI
{
    public static ISukiToastManager ToastManager { get; } = new SukiToastManager();
}