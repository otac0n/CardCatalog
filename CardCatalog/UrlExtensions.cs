namespace CardCatalog
{
    using System.Collections.Generic;
    using System.Text;

    public static class UrlExtensions
    {
        /// <summary>
        /// Used by the URLFriendly function, for removing accents from various Unicode characters, including the Turkish İ.
        /// </summary>
        private static readonly Dictionary<char, string> replacements = new Dictionary<char, string>()
        {
            { 'Æ', "ae" }, { 'æ', "ae" },
            { 'Ĳ', "ij" }, { 'ĳ', "ij" },
            { 'Œ', "oe" }, { 'œ', "oe" },
            { 'ß', "ss" },
            { 'À', "a" }, { 'Á', "a" }, { 'Â', "a" }, { 'Ã', "a" }, { 'Ä', "a" }, { 'Å', "a" }, { 'à', "a" }, { 'á', "a" }, { 'â', "a" }, { 'ã', "a" }, { 'ä', "a" }, { 'å', "a" }, { 'Ā', "a" }, { 'ā', "a" }, { 'Ă', "a" }, { 'ă', "a" }, { 'Ą', "a" }, { 'ą', "a" },
            { 'Ç', "c" }, { 'ç', "c" }, { 'Ć', "c" }, { 'ć', "c" }, { 'Ĉ', "c" }, { 'ĉ', "c" }, { 'Ċ', "c" }, { 'ċ', "c" }, { 'Č', "c" }, { 'č', "c" },
            { 'Ð', "d" }, { 'ð', "d" }, { 'Ď', "d" }, { 'ď', "d" }, { 'Đ', "d" }, { 'đ', "d" },
            { 'È', "e" }, { 'É', "e" }, { 'Ê', "e" }, { 'Ë', "e" }, { 'è', "e" }, { 'é', "e" }, { 'ê', "e" }, { 'ë', "e" }, { 'Ē', "e" }, { 'ē', "e" }, { 'Ĕ', "e" }, { 'ĕ', "e" }, { 'Ė', "e" }, { 'ė', "e" }, { 'Ę', "e" }, { 'ę', "e" }, { 'Ě', "e" }, { 'ě', "e" },
            { 'Ĝ', "g" }, { 'ĝ', "g" }, { 'Ğ', "g" }, { 'ğ', "g" }, { 'Ġ', "g" }, { 'ġ', "g" }, { 'Ģ', "g" }, { 'ģ', "g" },
            { 'Ĥ', "h" }, { 'ĥ', "h" }, { 'Ħ', "h" }, { 'ħ', "h" },
            { 'Ì', "i" }, { 'Í', "i" }, { 'Î', "i" }, { 'Ï', "i" }, { 'ì', "i" }, { 'í', "i" }, { 'î', "i" }, { 'ï', "i" }, { 'Ĩ', "i" }, { 'ĩ', "i" }, { 'Ī', "i" }, { 'ī', "i" }, { 'Ĭ', "i" }, { 'ĭ', "i" }, { 'Į', "i" }, { 'į', "i" }, { 'İ', "i" }, { 'ı', "i" },
            { 'Ĵ', "j" }, { 'ĵ', "j" },
            { 'Ķ', "k" }, { 'ķ', "k" }, { 'ĸ', "k" },
            { 'Ĺ', "l" }, { 'ĺ', "l" }, { 'Ļ', "l" }, { 'ļ', "l" }, { 'Ľ', "l" }, { 'ľ', "l" }, { 'Ŀ', "l" }, { 'ŀ', "l" }, { 'Ł', "l" }, { 'ł', "l" },
            { 'Ñ', "n" }, { 'ñ', "n" }, { 'Ń', "n" }, { 'ń', "n" }, { 'Ņ', "n" }, { 'ņ', "n" }, { 'Ň', "n" }, { 'ň', "n" }, { 'ŉ', "n" }, { 'Ŋ', "n" }, { 'ŋ', "n" },
            { 'Ò', "o" }, { 'Ó', "o" }, { 'Ô', "o" }, { 'Õ', "o" }, { 'Ö', "o" }, { 'Ø', "o" }, { 'ò', "o" }, { 'ó', "o" }, { 'ô', "o" }, { 'õ', "o" }, { 'ö', "o" }, { 'ø', "o" }, { 'Ō', "o" }, { 'ō', "o" }, { 'Ŏ', "o" }, { 'ŏ', "o" }, { 'Ő', "o" }, { 'ő', "o" },
            { 'Ŕ', "r" }, { 'ŕ', "r" }, { 'Ŗ', "r" }, { 'ŗ', "r" }, { 'Ř', "r" }, { 'ř', "r" },
            { 'Ś', "s" }, { 'ś', "s" }, { 'Ŝ', "s" }, { 'ŝ', "s" }, { 'Ş', "s" }, { 'ş', "s" }, { 'Š', "s" }, { 'š', "s" },
            { 'Ţ', "t" }, { 'ţ', "t" }, { 'Ť', "t" }, { 'ť', "t" }, { 'Ŧ', "t" }, { 'ŧ', "t" },
            { 'Ù', "u" }, { 'Ú', "u" }, { 'Û', "u" }, { 'Ü', "u" }, { 'ù', "u" }, { 'ú', "u" }, { 'û', "u" }, { 'ü', "u" }, { 'Ũ', "u" }, { 'ũ', "u" }, { 'Ū', "u" }, { 'ū', "u" }, { 'Ŭ', "u" }, { 'ŭ', "u" }, { 'Ů', "u" }, { 'ů', "u" }, { 'Ű', "u" }, { 'ű', "u" }, { 'Ų', "u" }, { 'ų', "u" },
            { 'Ŵ', "w" }, { 'ŵ', "w" },
            { 'Ý', "y" }, { 'ý', "y" }, { 'ÿ', "y" }, { 'Ŷ', "y" }, { 'ŷ', "y" }, { 'Ÿ', "y" },
            { 'Ź', "z" }, { 'ź', "z" }, { 'Ż', "z" }, { 'ż', "z" }, { 'Ž', "z" }, { 'ž', "z" },
        };

        /// <summary>
        /// Formats a string as a URL slug.
        /// </summary>
        public static string Slugify(this string title)
        {
            int maxlen, len;

            // Check if the string is empty and cache its length.
            if (title == null || (len = maxlen = title.Length) == 0)
            {
                return string.Empty;
            }

            // Cap the length at 80 characters.
            if (len > 80)
            {
                len = 80;
            }

            var i = 0;
            char c;

            // Loop through all white space to find our starting position.
            // The variables "i" and "c" are kept in an outer scope, so that the following loop can
            // use them without re-initializing.
            while (true)
            {
                c = title[i];

                if (!char.IsWhiteSpace(c))
                {
                    break;
                }

                i++;

                if (i >= maxlen)
                {
                    return string.Empty;
                }
            }

            // Based on our new starting position, update our length.
            len += i;

            if (len > maxlen)
            {
                len = maxlen;
            }

            // Initialize our StringBuilder with the new, more accurate length.
            var sb = new StringBuilder(len - i);
            var appendDash = false;
            var first = true;
            string replacement;

            while (true)
            {
                // If the character's lowercase representation is alpha-numeric.
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    // By appending the dash here (instead of when it was read) we can avoid trimming the end.
                    // This basically ensures that the dash must be followed by a non-dash.
                    if (appendDash)
                    {
                        sb.Append('-');
                        appendDash = false;
                    }

                    // This is a bit of a trick.  It just so happens that all of the lower case characters and all
                    // of the numbers 0-9 have bit #6 set.  By setting this bit, we convert from upper- to lower-case.
                    c = (char)(c | 32);

                    // Append the lower-case character.
                    sb.Append(c);
                }
                else if (c == ' ' || (c >= ',' && c <= '/') || c == '\\' || c == '_')
                {
                    // If or character is one that should be converted to a dash, set the appendDash flag.
                    // We exclude the first character in the string.
                    if (!first)
                    {
                        appendDash = true;
                    }
                }
                else if (c >= 128 && replacements.TryGetValue(c, out replacement))
                {
                    // If we have a special replacement. (For removing slashes, and etc from Unicode characters.
                    // Appending the dash is duplicated from the above function. It is repeated here, rather than
                    // rolled in to the above function, due to the rarity of these cases.
                    if (appendDash)
                    {
                        sb.Append('-');
                        appendDash = false;
                    }

                    sb.Append(replacement);
                }

                // Get the next character.
                i++;

                first = false;

                if (i >= len)
                {
                    break;
                }

                c = title[i];
            }

            return sb.ToString();
        }
    }
}