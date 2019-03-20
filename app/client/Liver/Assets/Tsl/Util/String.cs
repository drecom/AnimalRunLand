using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
class String
{
    public static string[] Lines(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            // throw new InvalidProgramException("Lines() parameter must not empty");
            return new string[] { "(null)" };
        }

        return s.Split(new char[] { '\n' });
    }

    public static int LineCount(string s)
    {
        return Lines(s).Length;
    }

    public static int GetMaxLineLength(string s)
    {
        return Lines(s).Max(x => x.Length);
    }
}
}
