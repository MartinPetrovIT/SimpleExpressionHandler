using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHandler
{
    public class Constants
    {
        public class RegexConstants
        {
            public const string regexPatternForSum = "[-/*]";
            public const string regexPatternForDevide = "[-+*]";
            public const string regexPatternForMultiply = "[-/+]";
            public const string regexPatternForMinus = "[+/*]";

            public const string regexPatternFullOperations = "[-*+/()]";
            public const string regexPatternFullOperationsWithoutBrackets = "[-*+/]";
        }
        public class Symbols 
        {
            public const string plus = "+";
            public const string minus = "-";
            public const string division = "/";
            public const string multiply= "*";
            public const string leftBracket = "(";
            public const string rightBracket = ")";
            public const string noOpperations = "noOpperations";

            public const string minusMinus = "--";
            public const string plusMinus = "+-";
            public const string multiplyMinus = "*-";
            public const string divisionMinus = "/-";

        }
    }
}
