using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompilerr
{
    class Lexer
    {
        private static readonly string OPERATOR_CHARS = "+-*/():=<>begiandort{}"; // сами знаки операторов (одиночные)

        private static readonly Dictionary<string, Token.TypeToken> OPERATORS;
        static Lexer()
        {
            OPERATORS = new Dictionary<string, Token.TypeToken>();
            OPERATORS.Add("+", Token.TypeToken.PLUS);
            OPERATORS.Add("-", Token.TypeToken.MINUS);
            OPERATORS.Add("*", Token.TypeToken.STAR);
            OPERATORS.Add("/", Token.TypeToken.SLASH);
            OPERATORS.Add("(", Token.TypeToken.LBRACKET);
            OPERATORS.Add(")", Token.TypeToken.RBRACKET);
            OPERATORS.Add(":=", Token.TypeToken.EQUAL);
            OPERATORS.Add("<", Token.TypeToken.LESSER);
            OPERATORS.Add(">", Token.TypeToken.GREATER);

            OPERATORS.Add("not", Token.TypeToken.NEGATE);
            
            OPERATORS.Add("==", Token.TypeToken.EQUALEQUAL);
            OPERATORS.Add("<>", Token.TypeToken.NEGATEEQUAL);
            OPERATORS.Add("<=", Token.TypeToken.LESSEREQUAL);
            OPERATORS.Add(">=", Token.TypeToken.GREATEREQUAL);

            OPERATORS.Add("and", Token.TypeToken.ANDAND);
            OPERATORS.Add("or", Token.TypeToken.OROR);
            OPERATORS.Add("begin", Token.TypeToken.BEGIN);
            OPERATORS.Add("end", Token.TypeToken.END);
        }
        private readonly string input; // строка, которая подается на вход лексеру
        private readonly int length;
        private readonly List<Token> tokens;
        private int position; // текущая позиция просматриваемого исходника
        public Lexer(string input)
        {
            this.input = input;
            length = input.Length;

            tokens = new List<Token>();
        }

        public List<Token> Tokenize()
        {
            while (position < length)
            {
                char cur = GetSymbol(0);
                if (Char.IsDigit(cur))
                {
                    TokenizeNumber();
                }
                else if (Char.IsLetter(cur) && !isAnd() && !isNot() && !isOr())
                {
                    TokenizeWord();
                }
                else if (OPERATOR_CHARS.IndexOf(cur) != -1)
                {
                    TokenizeOperator();
                }
                else if (cur == '#')
                {
                    Next();
                    TokenizeHexNumber();
                }
                else if (cur == '"')
                {
                    Next();
                    TokenizeText();
                }
                else
                {
                    Next(); // если встретилось и не число и не оператор (например пробел), пропускаем
                }
                
            }
            return tokens;
        }
        private bool isAnd()
        {
            return (GetSymbol(0) == 'a' && GetSymbol(1) == 'n' && GetSymbol(2) == 'd' && GetSymbol(3) == ' ');
        }
        private bool isNot()
        {
            return (GetSymbol(0) == 'n' && GetSymbol(1) == 'o' && GetSymbol(2) == 't' && GetSymbol(3) == ' ');
        }
        private bool isOr()
        {
            return (GetSymbol(0) == 'o' && GetSymbol(1) == 'r' && GetSymbol(2) == ' ');
        }
        private bool isBegin()
        {
            return (GetSymbol(0) == 'b' && GetSymbol(1) == 'e' && GetSymbol(2) == 'g' && GetSymbol(3) == 'i' && GetSymbol(4) == 'n' && GetSymbol(5) == ' ');
        }
        private bool isEnd()
        {
            return (GetSymbol(0) == 'e' && GetSymbol(1) == 'n' && GetSymbol(2) == 'd' && GetSymbol(3) == ' ');
        }
        private void TokenizeOperator()
        {
            char cur = GetSymbol(0);
            if (cur == '/') //если у нас встретился /
            {
                if (GetSymbol(1) == '/') // а затем еще один /
                {
                    Next(); //учитываем эти два символа
                    Next();
                    TokenizeComment();
                    return;
                }
            }
            if (cur == '{')
            {
                Next();
                TokenizeMultilineComment();
                return;
            }
            StringBuilder buf = new StringBuilder(); // символы в буфер будем читать до тех пор, пока не встретим такой оператор, которого нет в таблице.
            if (isAnd() || isNot() || isEnd())
            {
                buf.Append(cur).Append(GetSymbol(1)).Append(GetSymbol(2));
                AddToken(OPERATORS[buf.ToString()]);
                Next();
                Next();
                Next();
                return;
            }

            if (isBegin())
            {
                buf.Append(cur).Append(GetSymbol(1)).Append(GetSymbol(2)).Append(GetSymbol(3)).Append(GetSymbol(4));
                AddToken(OPERATORS[buf.ToString()]);
                Next();
                Next();
                Next();
                Next();
                Next();
                return;
            }
    
            while (true) //читаем
            {
                // <=?  
                string text = buf.ToString(); //смотрим что у нас в буфере
                if (!OPERATORS.ContainsKey(text + cur) && text != "") // если нет символа text + cur и при этом text не пустой
                {
                    AddToken(OPERATORS[text]); // берем токен из нашего словаря и добавляем в список токенов
                    return; // как только добавили, выходим из метода
                }
                
                buf.Append(cur); // добавляем в буфер текущий символ
                cur = Next(); // переходим к новому символу
            }
        }

        private void TokenizeMultilineComment()
        {
            char cur = GetSymbol(0);
            while (true)
            {
                if (cur == '\0') throw new Exception("Пропущен закрывающий тег");
                if (cur == '}') break;
                cur = Next();
            }
            Next(); // как только вышли из цикла, прочитываем }
        }

        private void TokenizeComment()
        {
            char cur = GetSymbol(0);
            while ("\r\n\0".IndexOf(cur) == -1) // пока этих символов нет
            {
                cur = Next(); //пропускаем комментарии
            }
        }

        private void TokenizeNumber()
        {
            char cur = GetSymbol(0); // текущий символ
            StringBuilder buf = new StringBuilder(); //создаем буфер для хранения цифр числа
            while (true) //если следующий символ цифра
            {
                if (cur == '.')
                {
                    if (buf.ToString().IndexOf(cur) != -1)
                        throw new Exception("Неправильное вещественное число");
                }
                else if (!Char.IsDigit(cur)) // если встретилось не число
                {
                    break;
                }
                buf.Append(cur); //добавляем в буфер
                cur = Next(); //переходим к следующему символу
            }
            AddToken(Token.TypeToken.NUM, buf.ToString()); // добавляем новый токен в список токенов
        }

        private void TokenizeWord()
        {
            char cur = GetSymbol(0); // текущий символ
            StringBuilder buf = new StringBuilder(); //создаем буфер для хранения цифр числа
            while (true) //если следующий символ цифра
            {
                if (!Char.IsLetterOrDigit(cur) && (cur != '_' ) && (cur != '$'))// если встретилось не число
                {
                    break;
                }
                buf.Append(cur); //добавляем в буфер
                cur = Next(); //переходим к следующему символу
            }
            string word = buf.ToString();
            switch (word)
            {
                case "writeln":
                    AddToken(Token.TypeToken.WRITELN);
                    break;
                case "write":
                    AddToken(Token.TypeToken.WRITE);
                    break;
                case "if":
                    AddToken(Token.TypeToken.IF);
                    break;
                case "then":
                    AddToken(Token.TypeToken.THEN);
                    break;
                case "else":
                    AddToken(Token.TypeToken.ELSE);
                    break;
                case "begin":
                    AddToken(Token.TypeToken.BEGIN);
                    break;
                case "end":
                    AddToken(Token.TypeToken.END);
                    break;
                case "while":
                    AddToken(Token.TypeToken.WHILE);
                    break;
                case "for":
                    AddToken(Token.TypeToken.FOR);
                    break;
                case "do":
                    AddToken(Token.TypeToken.DO);
                    break;
                default:
                    AddToken(Token.TypeToken.WORD, word);
                    break;
            }
          
        }

        private void TokenizeText()
        {
            char cur = GetSymbol(0); // текущий символ
            StringBuilder buf = new StringBuilder(); //создаем буфер для хранения цифр числа
            while (true) //если следующий символ цифра
            {
                if (cur == '\\')
                {
                    cur = Next();
                    switch (cur)
                    {
                        case '\"':
                            cur = Next();
                            buf.Append('\"');
                            continue;
                        case 'n':
                            cur = Next();
                            buf.Append('\n');
                            continue;
                        case 't':
                            cur = Next();
                            buf.Append('\t');
                            continue;
                    }
                    buf.Append('\\');
                    continue;
                }
                if (cur == '"')// если встретилось не число
                {
                    break;
                }
                buf.Append(cur); //добавляем в буфер
                cur = Next(); //переходим к следующему символу
            }
            Next();
            AddToken(Token.TypeToken.TEXT, buf.ToString());
        }

        private void TokenizeHexNumber()
        {
            char cur = GetSymbol(0); // текущий символ
            StringBuilder buf = new StringBuilder(); //создаем буфер для хранения цифр числа
            while (char.IsDigit(cur) || isHexNumber(cur)) //если следующий символ цифра
            {
                buf.Append(cur); //добавляем в буфер
                cur = Next(); //переходим к следующему символу
            }
            AddToken(Token.TypeToken.HEX_NUM, buf.ToString()); // добавляем новый токен в список токенов
        }

        private static bool isHexNumber(char cur)
        {
            return ("abcdef".IndexOf(Char.ToLower(cur)) != -1);
        }

        private char Next()
        {
            position++;
            return GetSymbol(0);
        }
        private char GetSymbol(int currentPosition)
        {
            int pos = position + currentPosition; 
            if (pos >= length)
            {
                return '\0';
            }
            return input[pos];
        }
        private void AddToken(Token.TypeToken type)
        {
            AddToken(type, "");
        }
        private void AddToken(Token.TypeToken type, string text)
        {
            tokens.Add(new Token(type, text));
        }
        
    }
}
