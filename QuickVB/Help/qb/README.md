# Documentation

A clone of QBasic language.

*To follow along with the progress, please reference the [Roadmap](roadmap) and [Differences](differences) documents.*

## Language Reference

The first part *(this document)* describes the elements of the BASIC language and the syntax and grammar that applies to the language. The second part is the Keywords ([Alphabetical](alphabetical)) section containing all of the built in commands and functions available (with examples).

### Character Set

The BASIC character set is composed of alphabetic, numeric, and special characters. These are the only characters that BASIC recognizes. There are many other characters that can be displayed or printed, but they have no special meaning in BASIC.

The BASIC alphabetic characters include all the uppercase and lowercase letters of the American English alphabet. Numeric characters are digits 0 through 9. The following list shows the special characters that are recognized by BASIC.

- ` ` Blank
- `=` Equal sign or assignment symbol
- `+` Plus sign
- `-` Minus sign
- `*` Asterisk or multiplication symbol
- `/` Slash or division symbol
- `^` Exponential symbol
- `(` Left parenthesis
- `)` Right parenthesis
- `%` Percent sign
- `#` Number (or pound) sign
- `$` Dollar sign
- `!` Exclamation point
- `[` Left bracket
- `]` Right bracket
- `,` Comma
- `.` Period or decimal point
- `'` Single quotation mark (apostrophe)
- `;` Semicolon
- `:` Colon
- `&` Ampersand
- `?` Question mark
- `<` Less than
- `>` Greater than
- `\` Backslash or integer division symbol
- `@` At-sign
- `_` Underscore
- `ENTER` Terminates input of a line
- `"` Double quotation mark

### The BASIC Line

BASIC program lines have the following format:

`[nnnn] statement [:statment...][comment]<enter>`

or

`[alpha-num-label:]statement[:statement2...][comment]<enter>`

The `nnnn` (which specifies the line number) must be an integer between 0 and 65529.

The `alpha-num-label` is any combination of letters, digits and periods that start with a letter and is followed (with no intervening spaces) by a colon (`:`).

A `comment` is a non-executing statement or characters that you may put in your programs to help clarify the program's operation and purpose.

As you can see, BASIC program lines can begin with a line number, an alphanumeric label, neither or both, and must end with a carriage return. A program line can contain a maximum of 255 characters. More than one BASIC statement can be placed on a line, but each must be separated from the last by a colon. Program lines are entered into a program by pressing the `<enter>` key. This carriage return is an invisible part of the line format.

Line numbers and labels are pointers used to document the program (make it more easily understood) or to redirect program flow, as with the `GOSUB` statement.

If, for example, you want a specific part of a program to run only when a certain condition is met, you can write the following:

```vb
IF account$ <> "" Then Gosub Design
```

The interpreter searches for a line with the label `Design:` and executes the subroutine beginning with that line. Note that no colon is needed for `Design` in the `GOSUB` statement.

Note: BASIC executes each line you enter sequentially regardless of the line number you assign. You should be aware of this if you are accustomed to using another BASIC that sorts the lines sequentially before execution.

### Label Definitions

Alphanumeric line labels can contain from 1 to 40 letters, digits or periods. They must begin with an alphabetical character. This allows the use of mnemonic labels to make you programs easier to read and maintain.

For example, the following line numbers and alphanumeric labels are valid:

- `100`
- `65000`
- `Alpha:`
- `A16:`
- `screen.sub:`

#### Restrictions

In order to distinguish alphanumeric labels from variables, each alphanumeric label definition must have a colon (`:`) following it. A legal label cannot have a space between the name and the colon. When you refer to a label in a `GOSUB` or `GOTO` or other control statement, do not include the colon as part of the label name. You cannot use any BASIC reserved word as an alphanumeric label.

While the line number 0 is not restricted from use in a program, error-trapping routines use line number 0 to mean that error trapping is to be disabled. Thus,

`ON ERROR GOTO 0`

does not branch to line number 0 if an error occurs. Instead, error trapping is disabled by this statement.

Warning: Line numbers are used only as labels. BASIC does not sort them or remove duplicates.

#### Format

A label, a line number or both a label and a line number can appear on any line. The line number, when present, must always begin in the leftmost column. A label must begin with the first non-blank character following the line number (if present) and end with a colon; a blank cannot exist between the label and the colon.

Alphanumeric labels and line numbers can be intermixed in the same program.

### Constants

Constants are the actual values BASIC uses during program execution. There are two types of constants: string and numeric. A string constant is a sequence of alphanumeric characters enclosed in double quotation marks. String constants may be up to 32,767 characters in length.

Numeric constants are positive or negative numbers. There are five types of numeric constants:

- `Short Integer` Whole numbers between -32,768 and +32,768. Short integer constants do not contain decimal points.
- `Long Integer` Whole numbers between -2,147,483,648 and +2,147,836,647. Long integer constants do not contain decimal points.
- `Fixed-point` Positive or negative real numbers; that is, number constants that contain decimal points.
- `Floating-pint` Positive or negative numbers represented in exponential form (similar to scientific notation). A floating-point constant consists of an optionally signed integer or fixed-point number (the mantissa) followed by the letter E and an optionally signed integer (the exponent). (Double precision floating-point constants are denoted by the letter D instead of E.)
- `Hex constants` Hexadecimal numbers with the prefix &H.
- `Octal constants` Octal numbers with the prefix &O or &.

Fixed-point and floating-point constants can be either single-precision or double-precision numbers. Single-precision numeric constants are stored with 7 digits of precision (plus the exponent) and printed with up to 7 digits of precision. Double-precision numbers are stored with 16 digits of precision and printed with up to 16 digits of precision. A single-precision constant is any numeric constant that has one of the following properties:

- Seven or fewer digits
- Exponential form denoted by E
- A trailing exclamation point (!)

A double-precision constant is any numeric constant that has one of the following properties:

- Eight or more digits
- Exponential form denoted by D
- A trailing declaration character (#)

The following are examples of numeric constants:

**Single Precision**

- 46.8
- 1.9E-6
- 3489.0
- 22.51

**Double Precision**

- 345692811
- -1.09432D-06
- 3489.0#
- 7654321.1234

### Variables

Variables represent values that are used in a program. As with constants, there are two types of variables: numeric and string. A numeric variable can only be assigned a value that is a number. A string variable can only be assigned a character string value. You can assign a value to a variable or it can be assigned as the result of calculations in the program. Before a variable is assigned a value, its value is zero (numeric variables) or null (string variables).

#### Variable Names

A variable name can contain as many as 40 characters. The characters allowed in a variable name are letters, numbers and the decimal point. The first character in a variable name must be a letter. Special type declaration characters are also allowed (see "Declaring Variable Types" in this section).

Variable names are not case-sensitive. That means that variables with the names `ALPHA`, `alpha` and `AlPhA` are the same variable.

If a variable name begins with `FN`, BASIC assumes it to be a call to a user-defined function. (See `DEF FN` in the Keyword section that follows for more information on user-defined functions.)

#### Reserved Words

Reserved words are words that have special meaning in BASIC. They include the names of all BASIC commands, statements, functions and operators. Examples include `GOTO`, `PRINT` and `TAN`. Always separate reserved words from data or other elements of a BASIC statement with spaces. Reserved words cannot be used as variable names. Reserved words can be entered in either uppercase or lowercase. A complete list of reserved words is given in Appendix C, "BASIC Reserved Words."

While a variable name cannot be a reserved word, a reserved word embedded in a variable name is allowed.

#### Declaring Variable Types

Variable names can be declared either as numeric values or as string values. String variable names can be written with a dollar sign ($) as the last character. For example:

`LET A$ = "SALES REPORT"`

The dollar sign is a variable type declaration character; that is, it "declares" that the variable will represent a string.

You can assign a numeric value certain properties by appending a trailing declaration character to its variable name. You declare the value to be a short integer or a long integer with a single-preceision or double-precision value. Computations with double-precision variables are more accurate than single-precision variables. However, double-precision variables take up more memory space than single-precision variables.

The default type for a numeric variable is single-precision.

The trailing declaration characters for numeric variables and the memory requirements (in bytes) for storing each variable type are as follows:

- `%` Short integer (2 bytes)
- `&` Long integer (4 bytes)
- `!` Singe-precision (4 bytes)
- `#` Double-precision (8 bytes)
- `$` String (5 bytes plus the contents of the string)

Instead of using the trailing declaration characters, you can include `DEFINT`, `DEFLNG`, `DEFSTR`, `DEFDBL` and `DEFSNG` statements in a program to relate the starting letter of a variable name to a variable type. Each time you declare a variable name beginning with the specified letter, BASIC assumes the variable type you specified in the `DEF`*type* statement. (The statements are described in the `DEFINT` section later in this chapter.)

##### Array Variables

An array is a group of values of the same type, referenced by a single variable name. The individual values in an array are called elements. Array elements are variables also. They can be used in any BASIC statement or function that uses variables. Declaring the name and type of an array and setting the number of elements in the array is known as *dimensioning* the array.

Each element in an array is referenced by an array variable that is subscripted with an integer or an integer expression. An array variable name has as many subscripts as there are dimensions in the array. For example, `V(10)` would reference a value in a one-dimension array, `T(1,4)` would reference a value in a two-dimension array, and so on. Note that the array variable `T(n)` and the "simple" variable `T` are not the same variable. The maximum number of dimensions for an array is 255. The maximum number of elements per dimension is 32,768.

Individual elements of string arrays need not be the same length.

Array elements, like numeric variables, require a certain amount of memory space, depending on the variable type. The memory requirements for storing arrays are the same as for variables, each element of the array requiring as much as the same type of "simple" variables.

##### Type Conversion

When necessary, BASIC will convert a numeric constant from one type to another. Keep the following rules in mind.

If a numeric constant of one type is assigned to a numeric variable of a different type, the numeric constant is stored as the type declared in the variable name. (If a string variable is assigned to a numeric value or vice versa, a "Type mismatch" error message is generated.)

During expression evaluation, all of the operands in an arithmetic or relational operation are converted to the same degree of precision; that is, the degree of the most precise operand. Also, the result of an arithmetic operation is returned to this degree of precision.

Logical operators convert their operands to integers and return an integer result. The operand must be in the range applicable to the short integer or long integer specified.

When a floating-point value is converted to an integer, the fractional portion is rounded.

### Expressions and Operators

An expression is a combination of constants, variables and other expressions with operators. Expressions are "evaluated" by the interpreter to produce a string or numeric value. Operators perform mathematical or logical operations on values. The operators provided by BASIC can be divided into four categories:

- Arithmetic
- Relational
- Logical
- Functional

#### Hierarchy of Operations

The BASIC operators have an order of precedence; that is, when several operations take place within the same program statement, certain operations are executed before others. If the operations are of the same level, the leftmost one is executed first, the rightmost last. The following is the order in which operations are executed:

- Exponentiation
- Unary Negation
- Multiplication and Floating-point Division
- Integer Division
- Modulo Arithmetic
- Addition and Subtraction
- Relational Operators
- `NOT`
- `AND`
- `OR` and `XOR`
- `EQV`
- `IMP`

#### Arithmetic Operators

The BASIC arithmetic operators are listed in the following in order of operational precedence:

- `^` Exponentiation
- `-` Unary Negation
- `*` Multiplication
- `/` Floating-point Division
- `\` Integer Division
- `MOD` Modulo Arithmetic
- `+`,`-` Addition, Subtraction

To change the order in which the operations are performed, use parentheses. Operations within parenthesis are performed first. Inside parentheses, the usual order operations is maintained.

#### Integer Division

Integer division is denoted by the backslash (`\`) instead of the slash (`/`); the slash indicates floating-point division. The operands of integer division are rounded to integers (or short integers, in the range -32,768 to +32,767 and for long integers, from -2,147,483,648 to +2,147,483,647) before the division is performed, and the quotient is truncated to an integer.

For example:

```vb
X=10/4
Y=25.68\6.99
PRINT X, Y
```

Will result in an output of:

`2        3`

##### Modulo Arithmetic

Modulo arithmetic is denoted by the operator `MOD`. Modulo arithmetic provides the integer remainder of an integer division.

For example:

```vb
PRINT 10.4 MOD 4 ' 10.4\4=2 with a remainder of 2
PRINT 25.68 MOD 6.99 ' 26\7=3 with a remainder of 5
```

Note that BASIC rounds both the divisor and the dividend to integers for the `MOD` operation.

##### Overflow and Division by Zero

If a division by zero is encountered during the evaluation of an expression, the "Division by zero" error message is also displayed, machine infinity (the highest number BASIC can produce) with the sign of the numerator is supplied as the result of the division, and execution continues. If the evaluation is an exponentiation results in zero being raised to a negative power, the "Division by zero" error message is displayed, positive machine infinity is supplied as the result of the exponentiation and execution continues. If overflow occurs, the "Overflow" error message is displayed, plus or minus infinity is supplied as a result and execution continues.

#### Relational Operators

Relational operators are used to compare two values. The result of the comparison is either "true" (-1) or "false" (0). This result can then be used to make a decision regarding program flow (see the `IF...THEN` statement is the Keywords section). The following table lists the relational operators:

- `=` Equality
- `<>` Inequality
- `<` Less than
- `>` Greater than
- `<=` Less than or equal to
- `>=` Greater than or equal to

(The equal sign is also used to assign a value to a variable. See `LET` in the Keywords section.) When arithmetic and relational operators are combined in one expression, the arithmetic operation is always performed first.

#### Logical Operators

Logical operators perform bit manipulation, Boolean operations or tests on multiple relations. Like relational operators, logical operators can be used to make decisions regarding program flow.

A logical operator returns a result from the combination of true-false operands. The result (in bits) is either "true" (-1) or "false" (0). The true-false combinations and the results of a logical operation are known as *truth tables*. There are six logical operators in BASIC. They are: `NOT` (logical complement), `AND` (conjunction), `OR` (disjunction), `XOR` (exclusive or), `IMP` (implication) and `EQV` (equivalence). Each operator returns results as indicated in the following table. A "T" indicates a true value and an "F" indicates a false value. Operators are listed in order of operational precedence.

|Operation|Value|Value|Result  |
|---------|-----|-----|--------|
|`NOT`    |X    |     |NOT X   |
|         |T    |     |F       |
|         |F    |     |T       |
|`AND`    |X    |Y    |X AND Y |
|         |T    |T    |T       |
|         |T    |F    |F       |
|         |F    |T    |F       |
|         |F    |F    |F       |
|`OR`     |X    |Y    |X OR Y  |
|         |T    |T    |T       |
|         |T    |F    |T       |
|         |F    |T    |T       |
|         |F    |F    |F       |
|`XOR`    |X    |Y    |X XOR Y |
|         |T    |T    |F       |
|         |T    |F    |T       |
|         |F    |T    |T       |
|         |F    |F    |F       |
|`IMP`    |X    |Y    |X IMP Y |
|         |T    |T    |T       |
|         |T    |F    |F       |
|         |F    |T    |T       |
|         |F    |F    |T       |
|`EQV`    |X    |Y    |X EQV Y |
|         |T    |T    |T       |
|         |T    |F    |F       |
|         |F    |T    |F       |
|         |F    |F    |T       |

In an expression, logical operations are performed after arithmetic and relational operations. Logical operators convert their operands to signed, two's complement integers in the range applicable to the long integer or short integer specified.

If both operands are supplied as 0 or -1, logical operators return 0 or -1, respectively. The given operation is performed on these integers in bits; that is, each bit of the result is determined by the corresponding bits in the two operands. Thus, it is possible to use logical operators to test bytes for a particular bit pattern. For instance, the `AND` operator can be used to "mask" all but one of the bits of a status bytes. The `OR` operator can be used to "merge" two bytes to create a particular binary value. The following examples illustrate how the logical operators work:

`63 AND 16` = 16; 63 = binary 111111 and 16 = binary 010000, so `63 AND 16` = 16.

`15 AND 14` = 14; 15 = binary 1111 and 14 = binary 1110, so `15 AND 14` = 14 (binary 1110).

`-1 AND 8` = 8; -1 = binary 1111111111111111 and 8 = binary 1000, so `-1 AND 8` = 8.

`4 OR 2` = 6; 4 = binary 100 and 2 = binary 10, so `4 OR 2` = 6 (binary 110).

`-1 OR -2` = -1; -1 = binary 1111111111111111 and -2 = binary 1111111111111110, so `-1 OR -2` = -1. The binary complement of 16 zeros is sixteen ones, which is two's complement representation of -1.

`NOT X = -(X + 1)`; the two's complement of any integer is the bit complement plus one.

#### Functions and Functional Operators

When a function is used in an expression, it calls a predetermined operation that is to be performed on its operands. BASIC has two types of functions: "intrinsic" functions, such as `SQR` (square root) or `SIN` (sine), which reside in the system, and user-defined functions that are written by the programmer.

See the Keywords section for exact description of individual intrinsic functions and `DEF FN`.

### Using Operators with Strings

A string expression consists of string constants, string variables and other string expressions combined by operators. There are three classes of operations with strings: concatenation, relational and functional.

#### Concatenation

Combining two strings together is called concatenation. The plus symbol (+) is the concatenation operator. Here is an example of the use of the operator:

```vb
LET A$ = "File" : LET B$ = "name"
PRINT A$ + B$
PRINT "New " + A$ + B$
END
```

These statements display the following output:

```text
Filename
New Filename
```

This example combines the string variables `A$` and `B$` to produce the value "Filename".

### Relational Operators

Strings can also be compared using the same relational operators that are used with numbers:

`= < > <> <= >=`

Using operators with strings is similar to using them with numbers, except that the operands ar strings rather than numeric values. String comparisons are made by taking one character at a time from each string and comparing the ASCII codes. The ASCII code system assigns a number value to each character produced by the computer. If all the ASCII codes are the same, the strings are equal. If the ASCII codes differ, the lower code number precedes the higher. If during string comparison the end of one string is reached, the shorter string is said to be smaller if they are equal at that point. Leading and trailing blanks are significant.

Here are some examples of true expressions:

```vb
"AA" < "BB"
"FILENAME" = "FILENAME"
"X&" >= "X#"
"CL " <> "CL"
"KG" <= "kg"
"SMYTH" < "SMYTHE"
```

Thus, string comparisons can be used to test string values or to alphabetize strings. All string constants used in comparison expressions must be enclosed in quotation marks.

## Keywords (Commands, Functions)

The following describes each BASIC command or function, including the appropriate syntax for each statement. Many descriptions include a programming example. The syntax conventions are outlined below, followed by a description of each BASIC command and function, listed alphabetically.

### Syntax Conventions

BASIC is a powerful programming language with over 130 statements and functions. These are presented in alphabetical order.

The correct syntax for each statement or function is given after the name. There are two kinds of syntax: one for statements and one for functions. All functions return a value of a particular type and can be used wherever an expression can be used. Unlike functions, statements can appear alone on a BASIC program line.

Following the name and syntax is a summary of what the statement or function does, descriptions of arguments and options and an explanation of how to use the statement or function.

Cross-references to related statements and functions (if any) along with notes and warnings are provided following the example program using the statement or function.

The following syntax notation is used in this section:

- `CAPS` Items in capital letters must be input as shown.
- `italics` Items in italics are to be supplied by the user.
- `[]` Items inside square brackets are optional. The brackets are not part of the statement syntax.
- `...` Items followed by ellipses may be repeated any number of times.
- `{}` Braces indicate that the user has a choice between two or more items. One of these items must be chosen unless the entries are also enclosed in square brackets. The braces are not a part of the statement syntax.
- `|` Vertical bars (pipes) separate the items enclosed in braces discussed above.
- `()` Items in parentheses  are to be supplied by the user.

All punctuation, including commas, parentheses, semicolons, hyphens and equal signs must be included where shown.

## Variable Types and Declarations

For comprehensive information about the BASIC type system, see:

- [Type-System](Type-System) - Complete guide to variable types, precedence, and best practices
- [Type-Characters](Type-Characters) - Type declaration characters (%,&,!,#,$) usage and examples
- [DEFINT](DEFINT) - DEF type statements (DEFINT, DEFLNG, DEFSNG, DEFDBL, DEFSTR)
- [DIM](DIM) - Variable and array declarations with AS clause
- [DEFSTR](DEFSTR), [DEFSNG](DEFSNG), [DEFLNG](DEFLNG), [DEFDBL](DEFDBL) - Individual DEF type documentation

## Additional Reference

- [Alphabetical](alphabetical)
- [Categories](categories)
