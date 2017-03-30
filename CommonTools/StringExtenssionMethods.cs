using System;
using System.Collections.Generic;

namespace CommonTools
{
    public static class StringExtenssionMethods
    {
        public static List<string> SplitBySpaces(this string str)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            int j = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                {
                    if (j != 0)
                    {
                        result.Add(str.Substring(i - j, j));
                        j = 0;
                    }
                }
                else
                {
                    j++;
                }
            }
            if (j != 0)
            {
                result.Add(str.Substring(str.Length - j, j));
            }

            return result;
        }
    }
}
