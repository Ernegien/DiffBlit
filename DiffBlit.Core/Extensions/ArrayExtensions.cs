using System;
using System.Collections;

namespace DiffBlit.Core.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Fills the specified byte array with random data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Returns a reference of itself.</returns>
        public static byte[] FillRandom(this byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)Rng.Next(byte.MaxValue);
            }
            return data;
        }

        /// <summary>
        /// Checks if the underlying data is equal.
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsEqual(this byte[] sourceData, byte[] data)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(sourceData, data);
        }

        /// <summary>
        /// Converts the data to a base64 string representation.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }
}
