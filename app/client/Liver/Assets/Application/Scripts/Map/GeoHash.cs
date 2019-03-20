using System;
using System.Collections.Generic;
using System.Linq;

namespace NGeoHash
{
public static class GeoHash
{
    /**
    *
    * Permission is hereby granted, free of charge, to any person
    * obtaining a copy of this software and associated documentation
    * files (the "Software"), to deal in the Software without
    * restriction, including without limitation the rights to use, copy,
    * modify, merge, publish, distribute, sublicense, and/or sell copies
    * of the Software, and to permit persons to whom the Software is
    * furnished to do so, subject to the following conditions:
    *
    * The above copyright notice and this permission notice shall be
    * included in all copies or substantial portions of the Software.
    *
    * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
    * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
    * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
    * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    * SOFTWARE.
    *
    */
    private const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";

    private static readonly Dictionary<char, int> Base32CodesDict =
        Base32Codes.ToDictionary(chr => chr, chr => Base32Codes.IndexOf(chr));

    /**
     * Encode
     *
     * Create a Geohash out of a latitude and longitude that is
     * `numberOfChars` long.
     *
     * @param {double} latitude
     * @param {double} longitude
     * @param {int} numberOfChars
     * @returns {string}
     */
    public static string Encode(double latitude, double longitude, int numberOfChars = 9)
    {
        var chars = new List<char>();
        var bits = 0;
        var bitsTotal = 0;
        var hashValue = 0;
        var maxLat = 90D;
        var minLat = -90D;
        var maxLon = 180D;
        var minLon = -180D;

        while (chars.Count < numberOfChars)
        {
            double mid;

            if (bitsTotal % 2 == 0)
            {
                mid = (maxLon + minLon) / 2;

                if (longitude > mid)
                {
                    hashValue = (hashValue << 1) + 1;
                    minLon = mid;
                }
                else
                {
                    hashValue = (hashValue << 1) + 0;
                    maxLon = mid;
                }
            }
            else
            {
                mid = (maxLat + minLat) / 2;

                if (latitude > mid)
                {
                    hashValue = (hashValue << 1) + 1;
                    minLat = mid;
                }
                else
                {
                    hashValue = (hashValue << 1) + 0;
                    maxLat = mid;
                }
            }

            bits++;
            bitsTotal++;

            if (bits != 5)
            {
                continue;
            }

            var code = Base32Codes[hashValue];
            chars.Add(code);
            bits = 0;
            hashValue = 0;
        }

        return string.Join("", chars.ToArray());
    }
}
}
