using System;

namespace Util
{
public struct TaggedInt32<Tag> : IComparable
{
    private System.Int32 value;

    public static TaggedInt32<Tag> Make(System.Int32 _value)
    {
        TaggedInt32<Tag> x = new TaggedInt32<Tag>();
        x.value = _value;
        return x;
    }

    public System.Int32 Get()
    {
        return this.value;
    }

    public override string ToString()
    {
        return this.value.ToString();
    }

    public int CompareTo(object other)
    {
        var x = (TaggedInt32<Tag>)other;
        return this.value.CompareTo(x.Get());
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (!(obj is TaggedInt32<Tag>))
        {
            return false;
        }

        var p = (TaggedInt32<Tag>)obj;
        return p.Get().Equals(this.Get());
    }

    public override int GetHashCode()
    {
        return this.Get().GetHashCode();
    }

    public static bool operator==(TaggedInt32<Tag> a, TaggedInt32<Tag> b)
    {
        return a.Get() == b.Get();
    }

    public static bool operator!=(TaggedInt32<Tag> a, TaggedInt32<Tag> b)
    {
        return !(a == b);
    }
}
}
