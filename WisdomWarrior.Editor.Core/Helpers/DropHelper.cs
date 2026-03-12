namespace WisdomWarrior.Editor.Core.Helpers;

public static class DropHelper
{
    public static IEnumerable<T> GetPayloadItems<T>(this object? payload)
    {
        if (payload == null)
        {
            yield break;
        }

        if (payload is T single)
        {
            yield return single;
            yield break;
        }

        if (payload is not System.Collections.IEnumerable list || payload is string)
        {
            yield break;
        }

        foreach (var item in list)
        {
            if (item is T typedItem)
            {
                yield return typedItem;
            }
        }
    }

    public static bool CanAccept<T>(this object? payload)
    {
        if (payload is System.Collections.IEnumerable list && payload is not string)
        {
            var hasAny = false;

            foreach (var item in list)
            {
                hasAny = true;

                if (item is not T)
                {
                    return false;
                }
            }

            return hasAny;
        }

        if (payload is not T)
        {
            return false;
        }

        return true;
    }

    public static bool CanAccept<T>(this object? payload, T target) where T : class
    {
        if (payload is System.Collections.IEnumerable list && payload is not string)
        {
            var hasAny = false;

            foreach (var item in list)
            {
                if (item is not T typedItem)
                {
                    return false;
                }

                hasAny = true;

                if (typedItem == target)
                {
                    return false;
                }
            }

            return hasAny;
        }

        if (payload is not T singleItem)
        {
            return false;
        }

        return singleItem != target;
    }
}
