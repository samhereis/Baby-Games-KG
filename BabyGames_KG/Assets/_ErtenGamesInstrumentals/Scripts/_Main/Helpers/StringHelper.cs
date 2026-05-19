using System.Text.RegularExpressions;

namespace Helpers
{
    public static class StringHelper
    {
        public const string emailPatternRegEx =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        public const string hasNumbersRegEx = @"[0-9]+";
        public const string hasUpperCharRegEx = @"[A-Z]+";
        public const string hasMinimum8CharsRegEx = @".{8,}";
        public const string isLetterOrDigit = "^[A-Za-z0-9]+$";

        public const string passwordPatternRegEx = @"^[A-Za-z0-9-._]+([A-Za-z0-9]*|[._]?[A-Za-z0-9-._]+)*$";
        public const string nickNamePatternRegEx = @"^[A-Za-z0-9-._]+([A-Za-z0-9]*|[._]?[A-Za-z0-9-._]+)*$";

        public static bool IsEmail(string email)
        {
            if (email != null) return Regex.IsMatch(email, emailPatternRegEx); else return false;
        }

        public static bool IsNickName(string nickName)
        {
            return Regex.IsMatch(nickName, nickNamePatternRegEx);
        }

        public static bool IsPassword(string password)
        {
            return Regex.IsMatch(password, passwordPatternRegEx);
        }
    }
}