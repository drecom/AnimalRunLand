using UnityEngine;

namespace Util.Extension
{
public static class ExtendedColor
{
    public static bool FastEqual(ref Color lhs, ref Color rhs)
    {
        return (lhs.r == rhs.r) && (lhs.g == rhs.g) && (lhs.b == rhs.b) && (lhs.a == rhs.a);
    }
}

}
