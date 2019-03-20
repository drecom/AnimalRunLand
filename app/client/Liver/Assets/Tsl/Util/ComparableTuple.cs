using System;

namespace Util
{
public class ComparableTuple<T1, T2> : IComparable
    where T1 : IComparable
    where T2 : IComparable
{
    public T1 Item1 { get; set; }
    public T2 Item2 { get; set; }

    public int CompareTo(object other)
    {
        var x = (ComparableTuple<T1, T2>)other;
        int result1 = Item1.CompareTo(x.Item1);

        if (result1 != 0)
        {
            return result1;
        }

        return Item2.CompareTo(x.Item2);
    }
}

public class ComparableTuple<T1, T2, T3> : IComparable
    where T1 : IComparable
    where T2 : IComparable
    where T3 : IComparable
{
    public T1 Item1 { get; set; }
    public T2 Item2 { get; set; }
    public T3 Item3 { get; set; }

    public int CompareTo(object other)
    {
        var x = (ComparableTuple<T1, T2, T3>)other;
        int result1 = Item1.CompareTo(x.Item1);

        if (result1 != 0)
        {
            return result1;
        }

        int result2 = Item2.CompareTo(x.Item2);

        if (result2 != 0)
        {
            return result2;
        }

        return Item3.CompareTo(x.Item3);
    }
}

public class ComparableTuple
{
    public static ComparableTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
    where T1 : IComparable
        where T2 : IComparable
    {
        return new ComparableTuple<T1, T2> { Item1 = item1, Item2 = item2 };
    }

    public static ComparableTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
    where T1 : IComparable
        where T2 : IComparable
        where T3 : IComparable
    {
        return new ComparableTuple<T1, T2, T3> { Item1 = item1, Item2 = item2, Item3 = item3 };
    }
}
}