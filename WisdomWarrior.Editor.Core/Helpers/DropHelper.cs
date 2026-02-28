namespace WisdomWarrior.Editor.Core.Helpers;

public static class DropHelper
{
    public static bool CanAccept<T>(this object? payload)
    {
        if (payload is T) return true;
        if (payload is IEnumerable<T> typedList) return typedList.Any();
        if (payload is IEnumerable<object> objList) return objList.Any() && objList.All(x => x is T);

        return false;
    }

    public static bool CanAccept<T>(this object? payload, T target) where T : class
    {
        if (payload is T singleItem)
        {
            return singleItem != target;
        }

        if (payload is IEnumerable<object> list)
        {
            return list.Any() && list.All(item => item is T tItem && tItem != target);
        }

        return false;
    }
}