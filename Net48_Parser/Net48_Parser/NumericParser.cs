using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net48_Parser
{
    public class NumericParser
    {
        #region members and constructor

        public decimal Result => m_Result;
        decimal m_Result = 0;

        public string ErrorMessage => m_ErrorMessage;
        string m_ErrorMessage = null;

        public bool EnableLogicalOperators { set; get; } = false;

        public decimal ArcBase { set; get; } = 400;

        string m_Formula = null;
        int m_Cursor = 0;
        int m_Lenght = 0;

        public NumericParser(bool paramEnableLogicalOperators, decimal paramArcBase)
        {
            EnableLogicalOperators = paramEnableLogicalOperators;
            ArcBase = paramArcBase != 0 ? paramArcBase : (decimal)_2pi();
        }

        #endregion

        #region parse

        public bool execute(string Formula)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string Formula2 = Formula.ToLowerInvariant();

                foreach (char c in Formula2)
                {
                    // potentiell gueltige Zeichen
                    if (((c >= '0') && (c <= '9')) || ((c >= 'a') && (c <= 'z')) || (",.()+-*/^=><&|".IndexOf(c) >= 0))
                    {
                        sb.Append(c);
                        continue;
                    }

                    // Trennzeichen und ungueltige Zeichen
                    if (!("\r\n\t ".IndexOf(c) >= 0))
                    {
                        m_ErrorMessage = "illegal character '" + c + "'\r\n" + Formula.Substring(Formula2.IndexOf(c));
                    }
                }

                m_Formula = sb.ToString();
                m_Cursor = 0;
                m_Lenght = m_Formula.Length;
                return parse();
            }
            catch (Exception e)
            {
                if (String.IsNullOrEmpty(m_ErrorMessage))
                    m_ErrorMessage = e.Message;
                return false;
            }
        }

        bool parse()
        {
            m_Result = 0;
            m_ErrorMessage = null;

            // linker Term
            bool sign = readSign();
            string term = readTerm();
            if (String.IsNullOrEmpty(term))
            {
                return false;
            }

            decimal leftValue = resolve(term, sign);
            string opCode = readOpCode();
            m_Result = String.IsNullOrEmpty(opCode) ? leftValue : combineWithRemaining(leftValue, opCode);
            return String.IsNullOrEmpty(m_ErrorMessage);
        }

        #endregion

        #region resolve

        /// <summary>
        /// gibt den Wert des Terms zurueck
        /// </summary>
        /// <param name="term"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        decimal resolve(string term, bool sign)
        {
            decimal result = 0;
            if ((term[0] >= '0') && (term[0] <= '9'))
            {
                result = resolveConst(term);
            }
            else if ((term[0] >= 'a') && (term[0] <= 'z'))
            {
                result = resolveNamedElement(term);
            }
            else if ((term[0] == '(') && (term[term.Length - 1] == ')'))
            {
                result = resolveExpression(term.Substring(1, term.Length - 2));
            }
            return sign ? 0m - result : result;
        }

        /// <summary>
        /// Einen Ausdruck aufloesen
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        decimal resolveExpression(string expr)
        {
            NumericParser P2 = new NumericParser(EnableLogicalOperators, ArcBase);

            if (P2.execute(expr))
                return P2.Result;
            
            if (String.IsNullOrEmpty(m_ErrorMessage))
                m_ErrorMessage = P2.ErrorMessage;

            return 0;
        }

        /// <summary>
        /// Einen Term aufloesen, der mir mit einen Namen beginnt
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        decimal resolveNamedElement(string term)
        {
            int paramPos = term.IndexOf('(');
            if (paramPos >= 0)
            {
                string n = term.Substring(0, paramPos);
                string p = term.Substring(paramPos);
                return resolveFunction(n, resolveExpression(p.Substring(1, p.Length - 2)));
            }
            return resolveAlias(term);
        }

        /// <summary>
        /// Wert einer Funktion ermitteln
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dArg"></param>
        /// <returns></returns>
        decimal resolveFunction(string name, decimal dArg)
        {
            switch (name.ToLower())
            {
                // REB
                case "sin": return fn_sin(dArg);
                case "cos": return fn_cos(dArg);
                case "tan": return fn_tan(dArg);
                case "asin": return fn_asin(dArg);
                case "acos": return fn_acos(dArg);
                case "atan": return fn_atan(dArg);
                // Extra 
                case "abs": return fn_fabs(dArg);
                case "sqrt": return fn_sqrt(dArg);
                case "sqr": return fn_sqrt(dArg); // komp. zu altem Parser
                case "qdrt": return fn_qdrt(dArg); // komp. zu altem Parser
                case "exp": return fn_exp(dArg);
                case "log": return fn_log(dArg);
                case "log10": return fn_log10(dArg);
                // Optionale Ersetzungen
                //case "opt1(": return lpParser.fn_opt1(dArg);
            }
            return 0;
        }

        /// <summary>
        /// Eine benannte Konstante ermitteln
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        decimal resolveAlias(string name)
        {
            switch (name)
            {
                case "pi":
                    return (decimal)(Math.Asin(1) * 2);
                case "e":
                    return (decimal)(Math.Exp(1));
            }
            return 0;
        }

        /// <summary>
        /// Eine Konstante aus Ziffern und Trennzeichen auswerten
        /// </summary>
        /// <param name="constString"></param>
        /// <returns></returns>
        decimal resolveConst(string constString)
        {
            //return Decimal.Parse(constString);
            return StringToDecimal(constString);
        }

        /// <summary>
        /// Robustere Version von Decimal.Parse 
        /// </summary>
        /// <param name="bsParam"></param>
        /// <returns></returns>
        public static decimal StringToDecimal(string bsParam)
        {
            bsParam = bsParam.Trim();
            if (String.IsNullOrEmpty(bsParam))
            {
                return 0;
            }

            StringBuilder sb = new StringBuilder(30);
            bool bDigit = false;
            int nKomma = 0;
            bool bKommaAfterPunkt = false;
            int nPunkt = 0;
            bool bPunktAfterKomma = false;
            foreach (char c in bsParam)
            {
                if ((c >= '0') && (c <= '9'))
                {
                    sb.Append(c);
                    bDigit = true;
                }
                else switch (c)
                    {
                        case '-':
                            if (!bDigit)
                            {
                                sb.Append(c);
                                bDigit = true;
                            }
                            break;
                        case '+':
                            if (!bDigit)
                            {
                                bDigit = true;
                            }
                            break;
                        case ',':
                            sb.Append(c);
                            nKomma++;
                            bKommaAfterPunkt = nPunkt > 0;
                            break;
                        case '.':
                            sb.Append(c);
                            nPunkt++;
                            bPunktAfterKomma = nKomma > 0;
                            break;
                    }
            }

            string s = sb.ToString();
            if (nKomma > 0)
            {
                if (bKommaAfterPunkt)
                    s = s.Replace(".", "");
                s = s.Replace(",", ".");
            }
            if (String.IsNullOrEmpty(s))
            {
                return 0;
            }

            try { return Decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture); }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Operatoren ausfuehren

        /// <summary>
        /// Linken und rechten Wert mit der angegebene Operation verknuepfen
        /// </summary>
        /// <param name="leftValue"></param>
        /// <param name="opCode"></param>
        /// <param name="rightValue"></param>
        /// <returns></returns>
        decimal combine(decimal leftValue, string opCode, decimal rightValue)
        {
            try
            {
                switch (opCode)
                {
                    // Standard
                    case "+":
                        return leftValue + rightValue;
                    case "-":
                        return leftValue - rightValue;
                    case "*":
                        return leftValue * rightValue;
                    case "/":
                        return leftValue / rightValue;
                    case "**":
                    case "^":
                        return (decimal) Math.Pow((double)leftValue, (double)rightValue);

                    // Logisch
                    case "|":
                        return (leftValue != 0) || (rightValue != 0) ? 1 : 0;
                    case "&":
                        return (leftValue != 0) && (rightValue != 0) ? 1 : 0;
                    case "=":
                        return leftValue == rightValue ? 1 : 0;
                    case "<>":
                        return leftValue != rightValue ? 1 : 0;
                    case ">=":
                        return leftValue >= rightValue ? 1 : 0;
                    case "<=":
                        return leftValue <= rightValue ? 1 : 0;
                    case ">":
                        return leftValue > rightValue ? 1 : 0;
                    case "<":
                        return leftValue < rightValue ? 1 : 0;
                }
            }
            catch (Exception e)
            {
                if (String.IsNullOrEmpty(m_ErrorMessage))
                {
                    m_ErrorMessage = e.Message;
                }
            }
            return 0;
        }

        /// <summary>
        /// Linken Wert mit dem noch nicht analysierten Rest verknuepfen
        /// </summary>
        /// <param name="leftValue"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        decimal combineWithRemaining(decimal leftValue, string opCode)
        {
            bool sign = readSign();
            string term = readTerm();
            if (String.IsNullOrEmpty(term))
            {
                return leftValue;
            }

            decimal rightValue = resolve(term, sign);
            string nextOpCode = readOpCode();
            if (String.IsNullOrEmpty(nextOpCode))
            {
                return combine(leftValue, opCode, rightValue);
            }

            if (compareOpCode(nextOpCode, opCode) > 0)
            {
                rightValue = combineWithRemaining(rightValue, nextOpCode);
                return combine(leftValue, opCode, rightValue);
            }

            leftValue = combine(leftValue, opCode, rightValue);
            return combineWithRemaining(leftValue, nextOpCode);
        }

        /// <summary>
        /// Vergleicht das Gewicht der beiden Op-Codes
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        private int compareOpCode(string o1, string o2)
        {
            return getWeightOfOpCode(o1) - getWeightOfOpCode(o2);
        }

        /// <summary>
        /// Ermittelt das Gewicht eines Op-Codes fuer die Vorrangregel
        /// </summary>
        /// <param name="o1"></param>
        /// <returns></returns>
        private int getWeightOfOpCode(string o1)
        {
            switch (o1[0])
            {
                // Mathematische Operatoren
                case '+':
                case '-':
                    return 10;
                case '*':
                case '/':
                    return 11;
                case '^':
                    return 12;

                // logische Operatoren
                case '&':
                case '|':
                    return 1;

                // Vergleichsoperatoren
                default:
                    return 2;
            }
        }

        #endregion

        #region read source

        /// <summary>
        /// ermittelt den naechsten Term
        /// </summary>
        /// <returns></returns>
        string readTerm()
        {
            int pos = m_Cursor;
            char c = peekChar();

            if (c == '(')
                return readSubTerm();

            if ((c >= '0') && (c <= '9'))
                return readConst();

            if ((c >= 'a') && (c <= 'z'))
            {
                string name = readName();
                if (peekChar() != '(')
                    return name;

                string param = readSubTerm();
                if (param != null)
                    return name + param;
            }

            if ((c != (char)0) && (String.IsNullOrEmpty(m_ErrorMessage)))
                m_ErrorMessage = "term expected\r\n" + m_Formula.Substring(pos);
            return null;
        }

        /// <summary>
        /// Liest eine in Klammern eingeschlossenen Term
        /// </summary>
        /// <returns></returns>
        private string readSubTerm()
        {
            StringBuilder result = new StringBuilder(getChar().ToString());
            int Plane = 1;
            while (Plane > 0)
            {
                char c = getChar();
                switch (c)
                {
                    case (char)0:
                        m_ErrorMessage = ") expected";
                        return null;
                    case '(':
                        Plane++;
                        break;
                    case ')':
                        Plane--;
                        break;
                }
                result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// Liste eine Konstante
        /// </summary>
        /// <returns></returns>
        private string readConst()
        {
            StringBuilder result = new StringBuilder();
            for (char c = peekChar(); ((c >= '0') && (c <= '9')) || (c == '.') || (c == ','); c = peekChar())
            {
                result.Append(getChar());
            }
            return result.ToString();
        }

        /// <summary>
        /// Liest den Namen einer Konstanten oder Funktion
        /// </summary>
        /// <returns></returns>
        private string readName()
        {
            StringBuilder result = new StringBuilder();
            for (char c = peekChar(); ((c >= 'a') && (c <= 'z')) || ((c >= '0') && (c <= '9')); c = peekChar())
            {
                result.Append(getChar());
            }
            return result.ToString();
        }

        /// <summary>
        /// ermittelt den naechsten Operanten
        /// </summary>
        /// <returns></returns>
        string readOpCode()
        {
            char c = getChar();
            if (c != (char)0)
            {
                if ("+-*/^".IndexOf(c) >= 0)
                {
                    if ((c == '*') && (peekChar() == '*'))
                    {
                        getChar();
                        return "^";
                    }
                    return c.ToString();
                }
                if (EnableLogicalOperators && ("=><&|".IndexOf(c) >= 0))
                {
                    if (c == '>')
                    {
                        switch (peekChar())
                        {
                            case '=':
                                return c.ToString() + getChar();
                        }
                        return ">";
                    }
                    if (c == '<')
                    {
                        switch (peekChar())
                        {
                            case '>':
                            case '=':
                                return c.ToString() + getChar();
                        }
                        return "<";
                    }
                    return c.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Ermittelt, ob der naechste Term ein negatives Vorzeichen hat
        /// </summary>
        /// <returns></returns>
        bool readSign()
        {
            bool bSign = false;
            char c = peekChar();
            while (c == '-' || c == '+')
            {
                if (getChar() == '-') bSign = !bSign;
                c = peekChar();
            }
            return bSign;
        }

        /// <summary>
        /// Liest da naechste Zeichen und setzt den Cursor weiter
        /// </summary>
        /// <returns></returns>
        char getChar()
        {
            return m_Cursor < m_Lenght ? m_Formula[m_Cursor++] : (char)0;
        }

        /// <summary>
        /// Ermittelt das als naechstes zu lesende Zeichen ohne den Cursor zu veraendern
        /// </summary>
        /// <returns></returns>
        char peekChar()
        {
            return m_Cursor < m_Lenght ? m_Formula[m_Cursor] : (char)0;
        }

        #endregion

        #region math functions

        public decimal fn_sin(decimal dArg)
        {
            return CheckResult(Math.Sin(ArcFunctionParameter((double)dArg)));
        }

        public decimal fn_cos(decimal dArg)
        {
            return CheckResult(Math.Cos(ArcFunctionParameter((double)dArg)));
        }

        public decimal fn_tan(decimal dArg)
        {
            return CheckResult(Math.Tan(ArcFunctionParameter((double)dArg)));
        }

        public decimal fn_asin(decimal dArg)
        {
            return CheckResult(ArcFunctionArgument(Math.Asin((double)dArg)));
        }

        public decimal fn_acos(decimal dArg)
        {
            return CheckResult(ArcFunctionArgument(Math.Acos((double)dArg)));
        }

        public decimal fn_atan(decimal dArg)
        {
            return CheckResult(ArcFunctionArgument(Math.Atan((double)dArg)));
        }

        // Extra
        public decimal fn_fabs(decimal dArg)
        {
            return CheckResult(Math.Abs((double)dArg));
        }

        public decimal fn_sqrt(decimal dArg)
        {
            return CheckResult(Math.Sqrt((double)dArg));
        }

        public decimal fn_qdrt(decimal dArg)
        {
            return CheckResult((double)(dArg * dArg));
        }

        public decimal fn_exp(decimal dArg)
        {
            return CheckResult(Math.Exp((double)dArg));
        }
        public decimal fn_log(decimal dArg)
        {
            return CheckResult(Math.Log((double)dArg));
        }

        public decimal fn_log10(decimal dArg)
        {
            return CheckResult(Math.Log10((double)dArg));
        }

        double _2pi()
        {
            return 4.0 * Math.Asin(1.0);
        }

        private decimal CheckResult(double dResult)
        {
            try
            {
                return (decimal)dResult;
            }
            catch(Exception e)
            {
                if (String.IsNullOrEmpty(m_ErrorMessage))
                {
                    m_ErrorMessage = e.Message;

                    var stackTrace = new System.Diagnostics.StackTrace();
                    var stackFrames = stackTrace.GetFrames();
                    var frame = stackFrames[1];
                    var methodInfo = frame.GetMethod();
                    var module = methodInfo.Module;
                    if (methodInfo.ReflectedType != null)
                    {
                        m_ErrorMessage += " function = " + methodInfo.Name;
                    }
                }
                return 0;
            }
        }

        double ArcFunctionParameter(double dArg)
        {
            return ArcBase != 0 ? (dArg * _2pi()) / (double)ArcBase : dArg;
        }

        double ArcFunctionArgument(double dArg)
        {
            CheckResult(dArg);
            return ArcBase != 0 ? (dArg / _2pi()) * (double)ArcBase : dArg;
        }

        #endregion
    }
}
