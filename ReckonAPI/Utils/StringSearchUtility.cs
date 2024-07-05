using System.Collections.Generic;

namespace ReckonAPI.Utils
{
    public static class StringSearchUtility
    {
        public static List<int> FindAllOccurrences(string text, string subtext)
        {
            List<int> positions = new List<int>();
            int subtextLength = subtext.Length;
            int textLength = text.Length;

            for (int i = 0; i <= textLength - subtextLength; i++)
            {
                int j;
                for (j = 0; j < subtextLength; j++)
                {
                    if (char.ToLower(text[i + j]) != char.ToLower(subtext[j]))
                    {
                        break;
                    }
                }
                if (j == subtextLength)
                {
                    positions.Add(i + 1); // 1-based index
                }
            }

            return positions;
        }
    }
}
