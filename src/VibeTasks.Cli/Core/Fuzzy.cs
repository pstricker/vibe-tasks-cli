using System;

namespace VibeTasks.Core
{
    public static class Fuzzy
    {
        // Normalized Levenshtein similarity in [0,100]
        public static int SimilarityScore(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 100;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            var dist = LevenshteinDistance(a, b);
            var maxLen = Math.Max(a.Length, b.Length);
            var score = (int)Math.Round((1.0 - (double)dist / maxLen) * 100.0);
            return Math.Clamp(score, 0, 100);
        }

        private static int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }
            return d[n, m];
        }
    }
}