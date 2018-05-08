namespace Plugins.Common
{
    using System;
    using System.Globalization;
    using System.Text;

    using Microsoft.Xrm.Sdk;

    /// <summary>
    ///     Common Plugin Utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     The <see cref="Guid" /> compression map.
        /// </summary>
        private const string Map = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        ///     The map radix.
        /// </summary>
        private static readonly int MapRadix = Map.Length;

        /// <summary>
        ///     Compress a <see cref="Guid" /> into a <see cref="string" /> using 0-9, A-Z, and a-z.
        /// </summary>
        /// <param name="source">
        ///     The source <see cref="Guid" />.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public static string CompressGuid(Guid source)
        {
            var start = source.ToString("N");
            var part1 = ulong.Parse(start.Substring(0, 16), NumberStyles.HexNumber);
            var part2 = ulong.Parse(start.Substring(16), NumberStyles.HexNumber);

            var sb = new StringBuilder();

            int remainder;
            while (part1 != 0)
            {
                remainder = (int)(part1 % (ulong)MapRadix);
                sb.Insert(0, Map[remainder]);
                part1 /= (ulong)MapRadix;
            }

            sb.Insert(0, "-");
            while (part2 != 0)
            {
                remainder = (int)(part2 % (ulong)MapRadix);
                sb.Insert(0, Map[remainder]);
                part2 /= (ulong)MapRadix;
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Decompress a <see cref="Guid" /> <see cref="string" />
        /// </summary>
        /// <param name="source">
        ///     The compressed <see cref="Guid" /> <see cref="string" />
        /// </param>
        /// <returns>
        ///     The <see cref="Guid" />.
        /// </returns>
        public static Guid DecompressGuid(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return Guid.Empty;
            }

            var parts = source.Split('-');

            ulong segment2 = 0, segment1 = 0, multiplier = 1;
            for (var i = parts[0].Length - 1; i >= 0; i--)
            {
                segment2 += (ulong)Map.IndexOf(parts[0][i]) * multiplier;
                multiplier *= (ulong)MapRadix;
            }

            multiplier = 1;
            for (var i = parts[1].Length - 1; i >= 0; i--)
            {
                segment1 += (ulong)Map.IndexOf(parts[1][i]) * multiplier;
                multiplier *= (ulong)MapRadix;
            }

            return Guid.ParseExact($"{segment1:X}".PadLeft(16, '0') + $"{segment2:X}".PadLeft(16, '0'), "N");
        }

        /// <summary>
        ///     Convert EntityReference to an Entity
        /// </summary>
        /// <param name="target">
        ///     The target EntityReference.
        /// </param>
        /// <returns>
        ///     The <see cref="Entity" />.
        /// </returns>
        public static Entity ToEntity(this EntityReference target) => 
            new Entity(target.LogicalName)
                {
                    Id = target.Id
                };
    }
}