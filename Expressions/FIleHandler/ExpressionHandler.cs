using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FileHandler.Constants;
namespace FileHandler
{
    public class ExpressionHandler
    {
       
        private static Func<decimal, decimal, decimal> plusFunction = (x, y) => x + y;
        private static Func<decimal, decimal, decimal> multiply = (x, y) => x * y;


        public static string FinalExpressionHandler(string expressionLine)
        {
            FormatChecker(expressionLine);
            bool start = true;

            expressionLine = String.Join("", expressionLine.Split());

            while (start)
            {
                expressionLine = ExpressionHandling(expressionLine);
                start = Regex.Split(expressionLine, RegexConstants.regexPatternFullOperations).Where(x => x.Length >= 1).Count() > 1;
            }
            return expressionLine;
        }

        private static void FormatChecker(string expressionLine)
        {

            if (Regex.IsMatch(expressionLine, "[^-+*/()0-9\\s]"))
            {
                throw new Exception("Message format must to be valid only nums -+*/()");
            }
            else if (Regex.IsMatch(expressionLine, "[0-9][(]|[)][0-9]") || Regex.IsMatch(expressionLine, "[(][+]|[+][)]"))
            {
                throw new Exception("Invalid opperation before or after bracket");
            }
            else if (Regex.IsMatch(expressionLine,
                @"[+]{2}|[-]{2}|[*]{2}|[/]{2}|[+][-]
                  |[+][*]|[+][/]|[-][+]|[-][*]|[-][/]|
                  [*][+]|[*][-]|[*][/]|[/][+]|[/][-]|[/][*]"))
            {
                throw new Exception("Cant use two oppertaors merged like '--'  must have bracket like '9-(-9)'");
            }
        
        }
        private static string ExpressionHandling(string expressionLine)
        {
            if (expressionLine.Contains(Symbols.leftBracket))
            {
                var startBracket = expressionLine.LastIndexOf(Symbols.leftBracket);
                var subString = expressionLine.Substring(startBracket);
                var endBracket = subString.IndexOf(Symbols.rightBracket) + startBracket;
                var insideExp = expressionLine.Substring(startBracket + 1, endBracket - startBracket - 1);
                var container = expressionLine.Substring(startBracket, endBracket - startBracket + 1);
                insideExp = DoMath(insideExp);

                expressionLine = expressionLine.Replace(container, insideExp);
            }
            else
            {
                expressionLine = DoMath(expressionLine);
            }

            return expressionLine;
        }

        private static string DoMath(string insideExp)
        {
            bool stop = false;
            while (!stop)
            {
                var opp = CheckForNextOpperation(insideExp);
                if (opp == Symbols.noOpperations)
                {
                    stop = true;
                }
                else if (opp == Symbols.multiply)
                {
                    insideExp = MultiplyOpperation(
                        insideExp,
                        Symbols.multiply , 
                        RegexConstants.regexPatternForMultiply, 
                        multiply, 1.0M);
                    stop = CheckForOpperations(insideExp);
                }
                else if (opp == Symbols.division)
                {
                    insideExp = DivideOpperation(
                        insideExp,
                        Symbols.division, 
                        RegexConstants.regexPatternForDevide);
                    stop = CheckForOpperations(insideExp);

                }
                else if (opp == Symbols.plus)
                {
                    insideExp = PlusOpperation(
                        insideExp,
                        Symbols.plus,
                        RegexConstants.regexPatternForSum,
                        plusFunction,
                        0M);
                    stop = CheckForOpperations(insideExp);
                }
                else if (opp == Symbols.minus)
                {
                    insideExp = MinusOpperation(
                        insideExp,
                        Symbols.minus,
                        RegexConstants.regexPatternForMinus);
                    stop = CheckForOpperationsWithoutMinus(insideExp);
                }


            }



            return insideExp;
        }

        private static string MultiplyOpperation(
            string expressionLine,
            string opperation, string regexPattern, 
            Func<decimal, decimal, decimal> opp, 
            decimal acumulator)
        {
            

            bool isNegative = false;
            if (expressionLine.Contains(Symbols.multiplyMinus))
            {
                isNegative = CheckForNegativeResult(ref expressionLine, Symbols.multiply, Symbols.multiplyMinus);
            }

            string arrExp = Regex.Split(expressionLine, regexPattern).Where(x => x.Contains(opperation)).First();
            List<decimal> arrResult = new List<decimal>();


            var splited = arrExp.Split(opperation).Select(decimal.Parse);
            decimal res = splited.Aggregate(acumulator, opp);
            var output = NumberFormater(res, isNegative);

            expressionLine = expressionLine.Replace(arrExp,output);
            CleanExpFromMinusMinus(ref expressionLine);


            return expressionLine;

        }

        private static string PlusOpperation(
            string expressionLine, string opperation,
            string regexPattern, 
            Func<decimal, decimal, decimal> opp,
            decimal acumulator)
        {
          
            bool isNegative = false;
            if (expressionLine.Contains(Symbols.plusMinus))
            {
                isNegative = CheckForNegativeResult(ref expressionLine, Symbols.minus, Symbols.plusMinus);
                return expressionLine;
            }

            string arrExp = Regex.Split(expressionLine, regexPattern).Where(x => x.Contains(opperation)).First();
            int index = expressionLine.IndexOf(arrExp);
            if (index != 0)
            {
                if (expressionLine[index - 1] == char.Parse(Symbols.minus))
                {
                    arrExp = Symbols.minus + arrExp;
                }
            }
            List<decimal> arrResult = new List<decimal>();
            CleanExpFromMinusMinus(ref arrExp);

            var splited = arrExp.Split(opperation).Select(decimal.Parse);
            decimal res = splited.Aggregate(acumulator, opp);

            expressionLine = expressionLine.Replace(arrExp, res.ToString());


            return expressionLine;

        }

        private static string MinusOpperation(
            string expressionLine, string opperation,
            string regexPattern)
        {

           
            string arrExp = Regex.Split(expressionLine, regexPattern).Where(x => x.Contains(opperation)).First();

            var container = arrExp;

            if (CleanExpFromMinusMinus(ref arrExp) != container )
            {
                expressionLine = expressionLine.Replace(container, arrExp);
                return expressionLine;
            }
            decimal res;
            if (arrExp[0] == char.Parse(Symbols.minus))
            {
                var splited = arrExp.Split(opperation, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList();
                res = splited.Sum();
                res *= -1M;
            }
            else
            {
                var splited = arrExp.Split(opperation, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList();
                var first = splited[0];
                res = splited.Skip(1).Sum();
                res = first - res;
            }


            expressionLine = expressionLine.Replace(arrExp, res.ToString());



            return expressionLine;

        }

        private static string DivideOpperation(
            string expressionLine,
            string opperation, 
            string regexPattern)
        {

            
            bool isNegative = false;
            if (expressionLine.Contains(Symbols.divisionMinus))
            {
                isNegative = CheckForNegativeResult( ref expressionLine, Symbols.division, Symbols.divisionMinus);
            }

            string arrExp = Regex.Split(expressionLine, regexPattern).Where(x => x.Contains(opperation)).First();

            var splited = arrExp.Split(opperation, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList();
            decimal res = splited[0];

            for (int i = 1; i < splited.Count; i++)
            {
                res /= splited[i];
            }

            var output = NumberFormater(res, isNegative);

            expressionLine = expressionLine.Replace(arrExp, output);

            CleanExpFromMinusMinus(ref expressionLine);


            return expressionLine;

        }

  

        private static string NumberFormater(decimal res, bool isNegative)
        {
            var output = "";
            if (isNegative)
            {
                output = Symbols.minus + res.ToString();
            }
            else
            {
                output = res.ToString();
            }

            return output;
        }

        private static bool CheckForNegativeResult( ref string exp, string replacer, string replaced)
        {           
                var container = exp;
                exp = exp.Replace(replaced, replacer);
                if ((container.Length - exp.Length) % 2 == 0 && (replacer == Symbols.multiply || replacer == Symbols.division))
                {
                    return false;
                }

                return true;     
        }
        private static string CleanExpFromMinusMinus(ref string exp)
        {
            if (exp.Contains(Symbols.minusMinus))
            {
                if (exp.IndexOf(Symbols.minusMinus) != 0)
                {
                    exp = exp.Replace(Symbols.minusMinus, Symbols.plus);
                }
                else
                {
                    exp = exp.Replace(Symbols.minusMinus, "");
                }
            }
            return exp;

        }

        
        private static string CheckForNextOpperation(string exp)
        {
            if (exp.Contains(Symbols.multiply) || exp.Contains(Symbols.division))
            {
               
        
                if (exp.IndexOf(Symbols.multiply) < exp.IndexOf(Symbols.division))
                {
                    if (exp.IndexOf(Symbols.multiply) == -1)
                    {
                        return Symbols.division;
                    }
                    return Symbols.multiply;
                }
                else
                {
                    if (exp.IndexOf(Symbols.division) ==-1)
                    {
                        return Symbols.multiply;
                    }
                    return Symbols.division;
                }
            }
            else
            {
                //Because of -nums
                if (exp[0]==char.Parse(Symbols.minus))
                {
                    exp = exp.Remove(0, 1);
                }
                if (!exp.Contains(Symbols.plus) && !exp.Contains(Symbols.minus))
                {
                    return Symbols.noOpperations;
                }
                if (exp.IndexOf(Symbols.plus) < exp.IndexOf(Symbols.minus))
                {
                    if (exp.IndexOf(Symbols.plus) == -1)
                    {
                        return Symbols.minus;
                    }
                    return Symbols.plus;
                }
                else
                {
                    if (exp.IndexOf(Symbols.minus) == -1)
                    {
                        return Symbols.plus;
                    }
                    return Symbols.minus;
                }
            }
        } 
        private static bool CheckForOpperations(string exp)
        {
            if (Regex.IsMatch(exp, RegexConstants.regexPatternFullOperationsWithoutBrackets))
            {
               
                return false;
            }
            return true;
        } 
        private static bool CheckForOpperationsWithoutMinus(string exp)
        {
            if (Regex.IsMatch(exp, RegexConstants.regexPatternForMinus))
            {

                return false;
            }
            return true;
        } 
    }
}
