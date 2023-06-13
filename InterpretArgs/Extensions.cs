using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpretArgs;

public static class TwoWayEnumeratorHelper
{
    public static ITwoWayEnumerator<T> GetTwoWayEnumerator<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new  ArgumentNullException("source");

        return new TwoWayEnumerator<T>(source.GetEnumerator());
    }

    public static T Parse<T>(this string input, IFormatProvider? formatProvider =null) where T : IParsable<T>
    {
        return T.Parse(input, formatProvider);
    }

}
