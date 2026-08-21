/* parser for numerical expressions.
   2014, 2015, 2024, 2026 Steffen Anders

   usage:

   var P = NumericParser();
   if (P.execute("4*3"))
     alert(P.Result.toString());
   else
     alert(P.ErrorMessage);
*/
var NumericParser = function()
{
	// ----------------------------- properties
	
	// the result of the calculation
	this.Result = 0.00;
	
	// Error message
	this.ErrorMessage = "";
	
	// Reference circle of the trigonometric functions
	// 2π would be the natural standard.
	// 360 degrees for classical mathematics.
	// 400 grad use for German architects.
	this.ArcBase = 400.00;
	
	// EnableLogicalOperators allows the use of logical operators.
	// These always return either a 0 or a 1.
	// The operands are evaluated as false if their value is 0, and as true otherwise.
	this.EnableLogicalOperators = false;

	// ----------------------------- members
	// Do not use these outside this class.
    this.m_Formula = "";
    this.m_Cursor = 0;
    this.m_Lenght = 0;

	// ----------------------------- public functions

	this.getResult = function()
	{
		return 0.001 * Math.round(this.Result * 1000);
	}
	
	this.formatValue = function(value, precis)
	{
		if (value == undefined || value == null) value = this.getResult();
		if (!precis) precis = 3;
		var r = Math.round(value * Math.pow(10, precis)).toString();
		r = r[0] == '-' ? "-" + r.substring(1).padStart(precis + 1, "0") : r.padStart(precis + 1, "0");
		var k = r.length - precis;  // Position of the delimiter
		return r.substring(0, k) + "," + r.substring(k);
	}

    /// Part of a line of text that can be evaluated as part of a formula
	this.getValueRelevantShare = function(lineText)
	{
		if ((lineText == null) || (lineText == "") || (lineText[0] == '#') || (lineText[0] == '*'))
		{
			return "";
		}

		var result = "";
		var colonEnabled = true;
		var inComment = false;
		for (var i = 0; i < lineText.length; )
		{
			var c = lineText[i++];
			if (c > ' ')
			{
				if (c == '"')
				{ 
					inComment = !inComment;
				}
				else if (!inComment)
				{
					if ((c == ':') && (colonEnabled))
					{
						result = "";
						colonEnabled = false;
					}
					else
					{
						result += c;
					}
				}
			}
		}
		return result;
	};

	/// stores the value of the formula in the Result and ErrorMessage properties
	this.execute = function(Formula)
	{
		this.Result = 0;
		this.ErrorMessage = "";
		this.m_Formula = Formula.toLowerCase();
		this.m_Cursor = 0;
		this.m_Lenght = this.m_Formula.length;

		//this.log("execute");
		try
		{
			// determining the first left-hand term
			var sign = this.readSign();
			var term = this.readTerm();
			if (term == "")	return false;
			var leftValue = this.resolve(term, sign);
			
			// combine this value with remaining
			var opCode = this.readOpCode();
			this.Result = opCode == "" ? leftValue : this.combineWithRemaining(leftValue, opCode);
			return this.ErrorMessage == "";
		}
		catch(e)
		{
            if (this.ErrorMessage == "")
				this.ErrorMessage = e.message;
			return false;
		}
	};

	// -------------- resolve
	
	/// returns the value of the term
	this.resolve = function(term, sign)
	{
		//this.log("resolve(" + term + "," + sign.toString() + ")");
		result = 0.00;
		if ((term[0] >= '0') && (term[0] <= '9'))
		{
			result = this.resolveConst(term);
		}
		else if ((term[0] >= 'a') && (term[0] <= 'z'))
		{
			result = this.resolveNamedElement(term);
		}
		else if ((term[0] == '(') && (term[term.length - 1] == ')'))
		{
			var len = term.length;
			var subTerm = term.substring(1, len - 1);
			//this.log("resolve-2[" + term + " == " + len.toString() + " == " + subTerm + "]");
			result = this.resolveExpression(subTerm);
		}
		return sign ? -result : result;
	};
		
	/// Resolve an expression
	this.resolveExpression = function(expr)
	{
		//this.log("resolveExpression(" + expr + ")");
		var P2 = new NumericParser();
		if (P2.execute(expr))
			return P2.Result;
		if (this.ErrorMessage == "")
			this.ErrorMessage = P2.ErrorMessage;
		return 0.00;
	};

    /// Resolve a term that begins with a name
	this.resolveNamedElement = function(term)
	{
		//this.log("resolveNamedElement(" + term + ")");
		var paramPos = term.indexOf('(');
		if (paramPos >= 0)
		{
			var n = term.substring(0, paramPos);
			var p = term.substring(paramPos);
			return this.resolveFunction(n, this.resolveExpression(p.substring(1, p.length - 1)));
		}
		return this.resolveAlias(term);
	};
	
    /// Determine the value of a function
	this.resolveFunction = function(name, dArg)
	{
		try
		{
			switch (name.toLowerCase())
			{
				// REB
				case "sin": return this.fn_sin(dArg);
				case "cos": return this.fn_cos(dArg);
				case "tan": return this.fn_tan(dArg);
				case "asin": return this.fn_asin(dArg);
				case "acos": return this.fn_acos(dArg);
				case "atan": return this.fn_atan(dArg);
				// Extra 
				case "abs": return this.fn_fabs(dArg);
				case "sqrt": return this.fn_sqrt(dArg);
				case "sqr": return this.fn_sqrt(dArg); // komp. zu altem Parser
				case "qdrt": return this.fn_qdrt(dArg); // komp. zu altem Parser
				case "exp": return this.fn_exp(dArg);
				case "log": return this.fn_log(dArg);
				case "log10": return this.fn_log10(dArg);
			}
		}
		catch(err)
		{
			if (this.ErrorMessage == "")
				this.ErrorMessage = err.message;
		}
		return 0.00;
	};

	/// Determine a named constant
	this.resolveAlias = function(name)
	{
		switch (name)
		{
			case "pi": return Math.PI;
			case "e": return Math.E;
		}
		return 0;
	}

	/// Evaluate a constant consisting of digits and separators.
    /// Here is a more robust version of parseFloat. 
	this.resolveConst = function(bsParam)
	{
		if (bsParam == "") return 0.0;

		var sb = "";
		var bDigit = false;
		var nKomma = 0;
		var bKommaAfterPunkt = false;
		var nPunkt = 0;
		var bPunktAfterKomma = false;
		var i;
		for (i = 0; i < bsParam.length;)
		{
			var c = bsParam[i++];
			if ((c >= '0') && (c <= '9'))
			{
				sb += c;
				bDigit = true;
			}
			else switch (c)
			{
				case '-':
					if (!bDigit)
					{
						sb += c;
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
					sb += c;
					nKomma++;
					bKommaAfterPunkt = nPunkt > 0;
					break;
				case '.':
					sb += c;
					nPunkt++;
					bPunktAfterKomma = nKomma > 0;
					break;
			}
		}

		if (nKomma > 0)
		{
			if (bKommaAfterPunkt)
				sb = sb.split(".").join("")
			sb = sb.split(",").join(".");
		}
		if (sb == "")
		{
			return 0;
		}

		try
		{
			var result = parseFloat(sb);
			return isNaN(result) ? 0.00 : result;
		}
		catch(err)
		{
			return 0.00;
		}
	};

	// ----------------- Execute operators

    /// Combine the left and right values using the specified operation.
	this.combine = function(leftValue, opCode, rightValue)
	{
		//this.log("combine " + leftValue.toString() + opCode + rightValue.toString());
		try
		{
			switch (opCode)
			{
                // default
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
					return Math.pow(leftValue, rightValue);

				// boolean
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
		catch (err)
		{
            if (this.ErrorMessage == "")
				this.ErrorMessage = err.message;
		}
		return 0.00;
	};

    /// Link the left-hand value with the as-yet unanalyzed remainder.
	this.combineWithRemaining = function(leftValue, opCode)
	{
		//this.log("combineWithRemaining-1 " + leftValue.toString() + opCode);
		
		var sign = this.readSign();
		var term = this.readTerm();
		if (term == "")
		{
			return leftValue;
		}

		rightValue = this.resolve(term, sign);
		nextOpCode = this.readOpCode();
		if (nextOpCode == "")
		{
			return this.combine(leftValue, opCode, rightValue);
		}

		if (this.compareOpCode(nextOpCode, opCode) > 0)
		{
			rightValue = this.combineWithRemaining(rightValue, nextOpCode);
			return this.combine(leftValue, opCode, rightValue);
		}

		leftValue = this.combine(leftValue, opCode, rightValue);
		return this.combineWithRemaining(leftValue, nextOpCode);
	};
	
	/// Compare the weight of the two operators.
	this.compareOpCode = function(o1, o2)
	{
		return this.getWeightOfOpCode(o1) - this.getWeightOfOpCode(o2);
	};

    /// Determines the weight of an operator for the precedence rules.
	this.getWeightOfOpCode = function(o1)
	{
		switch (o1[0])
		{
			// numeric
			case '+':
			case '-':
				return 10;
			case '*':
			case '/':
				return 11;
			case '^':
				return 12;

			// boolean
			case '&':
			case '|':
				return 1;

			// compare
			default:
				return 2;
		}
	};

	// ----------------- read source

	/// determines the next term
	this.readTerm = function()
	{
		//this.log("readTerm");
		var pos = this.m_Cursor;
		var c = this.peekChar();

		if (c != '')
		{
			if (c == '(')
				return this.readSubTerm();

			if ((c >= '0') && (c <= '9'))
				return this.readConst();

			if ((c >= 'a') && (c <= 'z'))
			{
				var name = this.readName();
				if (this.peekChar() != '(')
					return name;

				var param = this.readSubTerm();
				if (param != "")
					return name + param;
			}

			if (this.ErrorMessage == "")
				this.ErrorMessage = "term expected\r\n" + this.m_Formula.substring(pos);
		}
		return "";
	};

	/// Reads a term enclosed in parentheses
	this.readSubTerm = function()
	{
		//this.log("readSubTerm");
		var result = this.getChar();
		var Plane = 1;
		while (Plane > 0)
		{
			c = this.getChar();
			switch (c)
			{
				case '':
					this.ErrorMessage = ") expected";
					return null;
				case '(':
					Plane++;
					break;
				case ')':
					Plane--;
					break;
			}
			result += c;
		}
		//this.log("readSubTerm(" + result + ")");
		return result;
	};

	/// Reads a constant
	this.readConst = function()
	{
		//this.log("readConst");
		var result = "";
		var c = this.peekChar()
		while ((c != '') && (((c >= '0') && (c <= '9')) || (c == '.') || (c == ',')))
		{
			result += this.getChar();
			c = this.peekChar();
		}
		return result;
	};

	/// Reads the name of a constant or function.
	this.readName = function()
	{
		//this.log("readName");
		var result = "";
		for (var c = this.peekChar(); (c != '') && (((c >= 'a') && (c <= 'z')) || ((c >= '0') && (c <= '9'))); c = this.peekChar())
		{
			result += this.getChar();
		}
		return result;
	};

    /// determines the next operand
	this.readOpCode = function()
	{
		//this.log("readOpCode");
		var c = this.getChar();
		if (c != '')
		{
			if ("+-*/^".indexOf(c) >= 0)
			{
				if ((c == '*') && (this.peekChar() == '*'))
				{
					this.getChar();
					return "^";
				}
				return c;
			}
			if (this.EnableLogicalOperators && ("=><&|".indexOf(c) >= 0))
			{
				if (c == '>')
				{
					switch (this.peekChar())
					{
						case '=':
							return c + this.getChar();
					}
					return ">";
				}
				if (c == '<')
				{
					switch (this.peekChar())
					{
						case '>':
						case '=':
							return c + this.getChar();
					}
					return "<";
				}
				return c;
			}
		}
		return "";
	}

    /// Determines whether the next term has a negative sign.
    this.readSign = function()
	{
		//this.log("readSign");
		var bSign = false;
		var c = this.peekChar();
		while ((c != '') && (c == '-' || c == '+'))
		{
			if (this.getChar() == '-') bSign = !bSign;
			c = this.peekChar();
		}
		return bSign;
	};

	/// Reads the next character and moves the cursor.
	this.getChar = function()
	{
		return this.m_Cursor < this.m_Lenght ? this.m_Formula[this.m_Cursor++].toString() : '';
	};

	/// Determines the next character to be read without moving the cursor.
	this.peekChar = function()
	{
		return this.m_Cursor < this.m_Lenght ? this.m_Formula[this.m_Cursor].toString() : '';
	};

	// ----------------- math functions

	this.fn_sin = function(dArg)
	{
		return Math.sin(this.ArcFunctionParameter(dArg));
	};

	this.fn_cos = function(dArg)
	{
		return Math.cos(this.ArcFunctionParameter(dArg));
	};

	this.fn_tan = function(dArg)
	{
		return Math.tan(this.ArcFunctionParameter(dArg));
	};

	this.fn_asin = function(dArg)
	{
		return this.ArcFunctionArgument(Math.asin(dArg));
	};

	this.fn_acos = function(dArg)
	{
		return this.ArcFunctionArgument(Math.acos(dArg));
	};

	this.fn_atan = function(dArg)
	{
		return this.ArcFunctionArgument(Math.atan(dArg));
	};

	// extra
	this.fn_fabs = function(dArg)
	{
		return Math.abs(dArg);
	};

	this.fn_sqrt = function(dArg)
	{
		return Math.sqrt(dArg);
	};

	this.fn_qdrt = function(dArg)
	{
		return dArg * dArg;
	};

	this.fn_exp = function(dArg)
	{
		return Math.exp(dArg);
	};
	
	this.fn_log = function(dArg)
	{
		return Math.log(dArg);
	};

	this.fn_log10 = function(dArg)
	{
		return Math.log10(dArg);
	};
	
    this._2pi = function()
    {
        return 2 * Math.PI;
    };

    this.ArcFunctionParameter = function(dArg)
	{
		return this.ArcBase != 0 ? (dArg * this._2pi()) / this.ArcBase : dArg;
	};

    this.ArcFunctionArgument = function(dArg)
	{
		return this.ArcBase != 0 ? (dArg / this._2pi()) * this.ArcBase : dArg;
	};
	
	this.log = function(header)
	{
		console.log(header + ": " + this.m_Formula.substring(this.m_Cursor));
	};
	
	return this;
}; // NumericParser
