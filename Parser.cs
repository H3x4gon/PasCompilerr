using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompilerr
{
    //НИЖЕ ПРЕДСТАВЛЕН ИНТЕРФЕЙС IValue И РЕАЛИЗУЮЩИЕ ЕГО КЛАССЫ
    // IExpression ПОКРЫВАЕТ ТОЛЬКО ТОП double, НО БЫЛО БЫ НЕПЛОХО ДОБАВИТЬ НОВЫЙ ТИП ДАННЫХ string. IExpression ТЕПЕРЬ БУДЕТ ВОЗВРАЩАТЬ НЕ double А IValue
    public interface IValue
    {
        double AsNumber();
        string AsString();
    }

    public class NumberValue : IValue
    {
        private readonly double value;

        public NumberValue(double value)
        {
            this.value = value;
        }

        public NumberValue(bool value)
        {
            this.value = value ? 1 : 0;
        }
        public double AsNumber()
        {
            return value;
        }

        public string AsString()
        {
            return Convert.ToString(value);
        }

        public override string ToString()
        {
            return AsString();
        }
    }

    public class StringValue : IValue
    {
        private readonly string value;

        public StringValue(string value)
        {
            this.value = value;
        }

        public double AsNumber()
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception e)
            {
                return 0;
            }
            
        }

        public string AsString()
        {
            return value;
        }

        public override string ToString()
        {
            return AsString();
        }
    }

    // НИЖЕ ПРЕДСТАВЛЕН ИНТЕРФЕЙС IStatement (ОПЕРАТОР) И РЕАЛИЗУЮЩИЕ ЕГО КЛАССЫ 
 
    public interface IStatement
    {
        void execute();
    }
    
    public class AssignmentStatement : IStatement
    {
        // на вход принимаем имя переменной и выражение
        private readonly string variable;
        private readonly IExpression expression;

        public AssignmentStatement(string variable, IExpression expression)
        {
            this.variable = variable;
            this.expression = expression;
        }

        public void execute()
        {
            IValue result = expression.evaluate();
            Variables.SetValueByKey(variable, result);
        }

        public override string ToString()
        {
            return String.Format("{0} = {1}", variable, expression);
        }
    }

    public class WritelnStatement : IStatement // на входе expression
    {
        private readonly IExpression expression;

        public WritelnStatement(IExpression expression)
        {
            this.expression = expression;
        }

        public void execute()
        {
            Console.WriteLine(expression.evaluate().AsString());
        }

        public override string ToString()
        {
            return "writeln" + expression;
        }
    }

    public class WriteStatement : IStatement // на входе expression
    {
        private readonly IExpression expression;

        public WriteStatement(IExpression expression)
        {
            this.expression = expression;
        }

        public void execute()
        {
            Console.Write(expression.evaluate().AsString());
        }

        public override string ToString()
        {
            return "write" + expression;
        }
    }

    public class IfStatement : IStatement
    {
        //тут у нас 3 поля. Логическое выражение, if, else
        private readonly IExpression expression;
        private readonly IStatement ifStatement, elseStatement;

        public IfStatement(IExpression expression, IStatement ifStatement, IStatement elseStatement)
        {
            this.expression = expression;
            this.ifStatement = ifStatement;
            this.elseStatement = elseStatement;
        }

        public void execute()
        {
            double result = expression.evaluate().AsNumber();//если result = 1 это true
            if (result != 0)
            {
                ifStatement.execute();
            }
            else if (elseStatement != null)
            {
                elseStatement.execute();
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("if ").Append(expression).Append(" then ").Append(ifStatement);
            if (elseStatement != null)
            {
                result.Append("\nelse ").Append(elseStatement);
            }
            return result.ToString();
        }
    }

    public class BlockStatement : IStatement
    {
        private readonly List<IStatement> statements;

        public BlockStatement()
        {
            statements = new List<IStatement>();
        }

        public void add(IStatement statement)
        {
            statements.Add(statement);
        }
        public void execute() // проходим по списку и вызываем у каждого statement метод execute
        {
            foreach (IStatement statement in statements)
            {
                statement.execute();
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (IStatement statement in statements)
            {
                result.Append(statement.ToString()).Append(Environment.NewLine);
            }
            return result.ToString();
        }
    }

    public class WhileStatement : IStatement
    {
        private readonly IExpression boolean;
        private readonly IStatement statement;

        public WhileStatement(IExpression boolean, IStatement statement)
        {
            this.boolean = boolean;
            this.statement = statement;
        }

        public void execute()
        {
            while (boolean.evaluate().AsNumber() != 0)
            {
                try
                {
                    statement.execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    //continue;
                }
            }
        }

        public override string ToString()
        {
            return "while " + boolean + " do " + statement;
        }
    }
    public class RepeatUntilStatement : IStatement
    {
        private readonly IExpression boolean;
        private readonly IStatement statement;

        public RepeatUntilStatement(IExpression boolean, IStatement statement)
        {
            this.boolean = boolean;
            this.statement = statement;
        }

        public void execute()
        {
            do
            {
                try
                {
                    statement.execute();
                }
                catch (BreakStatement)
                {
                    break;
                }
                catch (ContinueStatement)
                {
                    //continue;
                }
            }
            while (boolean.evaluate().AsNumber() != 0);
        }

        public override string ToString()
        {
            return "repeat " + statement + " until " + boolean;
        }
    }
    public class BreakStatement : Exception, IStatement
    {
        public void execute()
        {
            throw this;
        }
        public override string ToString()
        {
            return "break";
        }
    }

    public class ContinueStatement : Exception, IStatement
    {
        public void execute()
        {
            throw this;
        }
        public override string ToString()
        {
            return "continue";
        }
    }

    // НИЖЕ ПРЕДСТАВЛЕН ИНТЕРФЕЙС IExpression (ВЫРАЖЕНИЕ) И РЕАЛИЗУЮЩИЕ ЕГО КЛАССЫ 
    // (StringExpression (ЧИСЛОВОЕ ВЫРАЖЕНИЕ), BinaryExpression (БИНАРНОЕ ВЫРАЖЕНИЕ), UnaryExpression (УНАРНОЕ ВЫРАЖЕНИЕ),
    // Variables (ТАБЛИЦА КОНСТАНТ (хранятся в словаре)), КОНСТАНТНОЕ ВЫРАЖЕНИЕ (VariablesExpression)

    public interface IExpression
    {
        IValue evaluate();
    }
    public class ValueExpression : IExpression
    {
        private readonly IValue value;

        public ValueExpression (double value)
        {
            this.value = new NumberValue(value);
        }
        public ValueExpression(string value)
        {
            this.value = new StringValue(value);
        }
        public IValue evaluate()
        {
            return value;
        }

        public override string ToString()
        {
            return value.AsString();
        }
    }

    public class BinaryExpression: IExpression
    {
        private readonly IExpression expression1, expression2;
        private readonly char operation;

        public BinaryExpression(char operation, IExpression expression1, IExpression expression2)
        {
            this.expression1 = expression1;
            this.expression2 = expression2;
            this.operation = operation;
        }

        public IValue evaluate()
        {
            IValue value1 = expression1.evaluate();
            IValue value2 = expression2.evaluate();

            if (value1 is StringValue)
            {
                string string1 = value1.AsString();
                switch (operation)
                { 
                    case '*':
                        int iterations = Convert.ToInt32(value2.AsNumber());
                        StringBuilder buf = new StringBuilder();
                        for (int i = 0; i < iterations; i++)
                        {
                            buf.Append(string1);
                        }
                        return new StringValue(buf.ToString());
                    case '+':
                    default:
                        return new StringValue(string1 + value2.AsString());
                }
            }

            double number1 = value1.AsNumber();
            double number2 = value2.AsNumber();
            switch (operation)
            {
                case '-':
                    return new NumberValue(number1 - number2);
                case '*':
                    return new NumberValue(number1 * number2);
                case '/':
                    return new NumberValue(number1 / number2);
                case '+':
                default:
                    return new NumberValue(number1 + number2);
            }
        }
        public override string ToString()
        {
            return String.Format("[{0} {1} {2}]", expression1, operation, expression2);
        }
    }

    public class BooleanExpression : IExpression
    {
        public enum Operator // операции представляются в виде перечисления
        {
            PLUS,
            MINUS,
            MULTIPLY,
            DIVIDE,

            EQUALS,
            NOT_EQUALS,

            LESSER,
            LESSEREQUAL,
            GREATER,
            GREATEREQUAL,

            ANDAND,
            OROR
     
        }
        public string GetName(Operator oper)
        {
            switch (oper)
            {
                case Operator.PLUS:
                    return "+";
                case Operator.MINUS:
                    return "-";
                case Operator.MULTIPLY:
                    return "*";
                case Operator.DIVIDE:
                    return "/";
                case Operator.EQUALS:
                    return "==";
                case Operator.NOT_EQUALS:
                    return "<>";
                case Operator.LESSER:
                    return "<";
                case Operator.LESSEREQUAL:
                    return "<=";
                case Operator.GREATER:
                    return ">";
                case Operator.GREATEREQUAL:
                    return ">=";
                case Operator.ANDAND:
                    return "and";
                case Operator.OROR:
                default:
                    return "or";
            }


        }
        private readonly IExpression expression1, expression2;
        private readonly Operator operation;

        public BooleanExpression(Operator operation, IExpression expression1, IExpression expression2)
        {
            this.expression1 = expression1;
            this.expression2 = expression2;
            this.operation = operation;
        }

        public IValue evaluate()
        {
            IValue value1 = expression1.evaluate();
            IValue value2 = expression2.evaluate();

            double number1, number2; //если у нас операция над строками
            if (value1 is StringValue)
            {
                number1 = value1.AsString().CompareTo(value2.AsString()); //num1 присваиваем результат сравнения string1 и string2
                number2 = 0; //num2 присваиваем 0
            }
            else
            {
                number1 = value1.AsNumber();
                number2 = value2.AsNumber();
            }

            bool result;
            switch (operation)
            {
                case Operator.LESSER:
                    result = number1 < number2;
                    break;
                case Operator.LESSEREQUAL:
                    result = number1 <= number2;
                    break;
                case Operator.GREATER:
                    result = number1 > number2;
                    break;
                case Operator.GREATEREQUAL:
                    result = number1 >= number2;
                    break;

                case Operator.ANDAND:
                    result = (number1 != 0) && (number2 != 0);
                    break;
                case Operator.OROR:
                    result = !(number1 == 0 && number2 == 0);
                    break;

                case Operator.EQUALS:   
                default:
                    result = number1 == number2;
                    break;
            }
            return new NumberValue(result);
        }
        public override string ToString()
        {
            return String.Format("[{0} {1} {2}]", expression1, GetName(operation), expression2);
        }
    }

    public class UnaryExpression : IExpression
    {
        private readonly IExpression expression1;
        private readonly char operation;

        public UnaryExpression(char operation, IExpression expression1)
        {
            this.expression1 = expression1;
            this.operation = operation;
        }

        public IValue evaluate()
        {
            switch (operation)
            {
                case '-': return new NumberValue(-expression1.evaluate().AsNumber());
                case '+':
                default:
                    return expression1.evaluate();
            }
        }

        public override string ToString()
        {
            return String.Format("[{0} {1}]", operation, expression1);
        }
    }

    public class Variables //переменные и константы
    {
        private static readonly NumberValue ZERO = new NumberValue(0);
        private static Dictionary<string, IValue> constants;

        static Variables()
        {
            constants = new Dictionary<string, IValue>();
            constants.Add("PI", new NumberValue(Math.PI));
            constants.Add("EXP", new NumberValue(Math.E));
        }

        public static bool isExists(string key)
        {
            return constants.ContainsKey(key);
        }
        public static IValue GetValueByKey(string key)
        {
            if (!isExists(key))
                return ZERO;
            else
                return constants[key];
        }

        public static void SetValueByKey(string key, IValue value)
        {
            if (!constants.ContainsKey(key))
            {
                constants.Add(key, value);
            }
            else
            {
                constants.Remove(key);
                constants.Add(key, value);
            }
        }
    }

    public class VariablesExpression : IExpression
    {
        private readonly string name;//имя константы

        public VariablesExpression(string name)
        {
            this.name = name;
        }

        public IValue evaluate()
        {
            if (!Variables.isExists(name))
                throw new Exception("Такой константы не существует");

            return Variables.GetValueByKey(name); //возвращаем значение константы по ключу
        }
        public override string ToString()
        {
            return String.Format("{0}", Variables.GetValueByKey(name));
        }
    }
    // НИЖЕ СОБСТВЕННО САМ ПАРСЕР
    public class Parser
    {
        private static readonly Token EOF = new Token(Token.TypeToken.EOF, "");

        private readonly List<Token> tokens;
        private readonly int size;
        private int position;
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            size = tokens.Count;
        }

        public IStatement Parse()
        {
            BlockStatement result = new BlockStatement();
            while (!Match(Token.TypeToken.EOF)) //парсим, добавляем все в список и возвращаем список.
            {
                result.add(statement());
            }
            return result;
        }

        //Парсить будем методом рекурсивного спуска. То есть от самого верхнего уровня к самому нижнему. За счет этого создается приоритет операций
        //Здесь по аналогии с методом Parse мы должны добавлять в список Statement, но возвращать мы должны Statement а не List<Statement>
        //Поэтому здесь нам нужен такой оператор, который будет хранить в себе другие операторы
        private IStatement block()                         
        {
            BlockStatement block = new BlockStatement();
            Consume(Token.TypeToken.BEGIN);
            while(!Match(Token.TypeToken.END))
            {
                block.add(statement());
            }
            return block;
        }

        private IStatement statementOrBlock() //таким образом будем парсить либо 1 оператор либо блок
        {
            if (GetToken(0).GetType() == Token.TypeToken.BEGIN)
                return block();
            return statement();
        }
        private IStatement statement() //метод, который парсит один оператор
        {
            if (Match(Token.TypeToken.WRITELN))
            {
                return new WritelnStatement(expression());
            }
            if (Match(Token.TypeToken.WRITE))
            {
                return new WriteStatement(expression());
            }
            if (Match(Token.TypeToken.IF))
            {
                return ifElseStatement();
            }
            if (Match(Token.TypeToken.BREAK))
            {
                return new BreakStatement();
            }
            if (Match(Token.TypeToken.CONTINUE))
            {
                return new ContinueStatement();
            }
            if (Match(Token.TypeToken.WHILE))
            {
                return whileStatement();
            }
            if (Match(Token.TypeToken.REPEAT))
            {
                return repeatUntilStatement();
            }
            return assignmentStatement();
        }

        private IStatement assignmentStatement()
        {
            // WORD EQ
            Token cur = GetToken(0);
            if (cur.GetType() == Token.TypeToken.WORD && GetToken(1).GetType() == Token.TypeToken.EQUAL)
            {
                Consume(Token.TypeToken.WORD);
                string variable = cur.GetText();
                Consume(Token.TypeToken.EQUAL);
                return new AssignmentStatement(variable, expression());
            }
            throw new Exception("Неизвестный оператор");
        }

        private IStatement ifElseStatement()
        {
            IExpression boolean = expression();
            IStatement ifStatement;
            if (Match(Token.TypeToken.THEN))
            {
                ifStatement = statementOrBlock();
            }
            else
            {
                throw new Exception("Пропущено служебное слово then");
            }
            IStatement elseStatement;
            if (Match(Token.TypeToken.ELSE))
            {
                elseStatement = statementOrBlock();
            }
            else
            {
                elseStatement = null;
            }
            return new IfStatement(boolean, ifStatement, elseStatement);
        }

        private IStatement whileStatement()
        {
            IExpression boolean = expression();
            if (!Match(Token.TypeToken.DO))
            {
                throw new Exception("Пропущено ключевое слово do");
            }
            IStatement statement = statementOrBlock();
            return new WhileStatement(boolean, statement);
           
        }

        private IStatement repeatUntilStatement()
        {
            IStatement statement = statementOrBlock();
            Consume(Token.TypeToken.UNTIL);
            IExpression boolean = expression();
            return new RepeatUntilStatement(boolean, statement);

        }
        private IExpression expression()
        {
            return logicalOr();
        }

        private IExpression logicalOr()// Уровень повыше. Логическая операция ИЛИ (or).
        {
            IExpression result = logicalAnd(); // сначала вызываем метод который стоит ниже
            while (true)
            {
                if (Match(Token.TypeToken.OROR)) //Теперь смотрим, если наш текущий токен это OROR, возвращаем Boolean Expr с операцией ИЛИ
                {
                    result = new BooleanExpression(BooleanExpression.Operator.OROR, result, logicalAnd()); // и тут возвращаем logicalAnd
                    continue;
                }
                break;
            }
            return result; //Если мы не встретили токен OROR, возвращаем logicalAnd, идем дальше по рекурсии
        }

        private IExpression logicalAnd() // Уровень повыше. Логические операция И (and). У логического И приоритет больше чем у ИЛИ поэтому мы его пишем ниже
        {
            IExpression result = equality(); // сначала вызываем метод который стоит ниже

            while (true)
            {
                if (Match(Token.TypeToken.ANDAND)) //Теперь смотрим, если наш текущий токен это ANDAND, возвращаем Boolean Expr с операцией И
                {
                    result = new BooleanExpression(BooleanExpression.Operator.ANDAND, result, equality()); // и тут возвращаем boolean()
                    continue;
                }
                break;
            }
            return result; //Если мы не встретили токен OROR, возвращаем logicalAnd, идем дальше по рекурсии
        }

        private IExpression equality() //Уровень повыше. Логические операции (== <>)
        {
            IExpression result = boolean();
            if (Match(Token.TypeToken.EQUALEQUAL))
            {
                return new BooleanExpression(BooleanExpression.Operator.EQUALS, result, boolean());
            }
            if (Match(Token.TypeToken.NEGATEEQUAL))
            {
                return new BooleanExpression(BooleanExpression.Operator.NOT_EQUALS, result, boolean());
            }
            return result;
        }
        private IExpression boolean() //Уровень повыше. Логические операции (< <= > >=). Тут парсим операции с одинаковым приоритетом. У операций and or будет более низкий приоритет
        {
            IExpression result = additive();
            while (true)
            {
                if (Match(Token.TypeToken.LESSER))
                {
                    result = new BooleanExpression(BooleanExpression.Operator.LESSER, result, additive());
                    continue;
                }
                if (Match(Token.TypeToken.LESSEREQUAL))
                {
                    result = new BooleanExpression(BooleanExpression.Operator.LESSEREQUAL, result, additive());
                    continue;
                }
                if (Match(Token.TypeToken.GREATER))
                {
                    result = new BooleanExpression(BooleanExpression.Operator.GREATER, result, additive());
                    continue;
                }
                if (Match(Token.TypeToken.GREATEREQUAL))
                {
                    result = new BooleanExpression(BooleanExpression.Operator.GREATEREQUAL, result, additive());
                    continue;
                }
                break;
            }
            return result;
        }
        private IExpression additive() // Уровень повыше. Оперции сложения
        {
            IExpression result = multiplicative();
            while (true)
            { 
                if (Match(Token.TypeToken.PLUS))
                {
                    result = new BinaryExpression('+', result, multiplicative()); 
                    continue;
                }
                if (Match(Token.TypeToken.MINUS))
                {
                    result = new BinaryExpression('-', result, multiplicative());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression multiplicative() //Уровень повыше. Операции умножения
        {
            IExpression result = unary();
            while (true)
            {
                // Если будет подряд например 2 * 3 / 7, все обработается в этом методе
                if (Match(Token.TypeToken.STAR))
                {
                    result = new BinaryExpression('*', result, unary()); // BExpr получает 2 выражения. Первое составляем сразу динамически, а второе вызываем дальше по рекурсии.
                    continue;
                }
                if (Match(Token.TypeToken.SLASH))
                {
                    result = new BinaryExpression('/', result, unary());
                    continue;
                }
                break;
            }
            return result;
        }

        private IExpression unary() //Уровень повыше. Унарные операции: -число, +число, инкремент, декремент.
        {
            if (Match(Token.TypeToken.MINUS))
            {
                return new UnaryExpression('-', primary());
            }
            if (Match(Token.TypeToken.PLUS))
            {
                return new UnaryExpression('+', primary());
            }
            return primary();
        }

        private IExpression primary() //Самый нижний уровень. Здесь будем парсить числа и строки
        {
            Token cur = GetToken(0);
            if (Match(Token.TypeToken.NUM))
            {
                return new ValueExpression(Convert.ToDouble(cur.GetText()));
            }
            if (Match(Token.TypeToken.HEX_NUM))
            {
                return new ValueExpression(Convert.ToInt32(cur.GetText(), 16));
            }
            if (Match(Token.TypeToken.WORD))
            {
                return new VariablesExpression(cur.GetText());
            }
            if (Match(Token.TypeToken.TEXT))
            {
                return new ValueExpression(cur.GetText());
            }
            if (Match(Token.TypeToken.LBRACKET))
            {
                IExpression result = expression();
                Match(Token.TypeToken.RBRACKET);
                return result;
            }

             throw new Exception("Неизвестное выражение");
        }
        private Token Consume(Token.TypeToken type) //будем передавать этому методу тип токена и будем спрашивать, равен ли текущий токен токену type. То есть проверяем, правильный токен или нет
        {
            Token cur = GetToken(0);
            if (type != cur.GetType()) //если тип, который мы передали в match, не совпадает c типом cur, возвращаем false
            {
                throw new Exception("Токен " + cur + "не является " + type);
            }
            else
            {
                position++; //если совпадает, увеличиваем позицию и возвращаем true
                return cur;
            }
        }
        private bool Match(Token.TypeToken type) //будем передавать этому методу тип токена и будем спрашивать, равен ли текущий токен токену type. То есть проверяем, правильный токен или нет
        {
            Token cur = GetToken(0);
            if (type != cur.GetType()) //если тип, который мы передали в match, не совпадает c типом cur, возвращаем false
            {
                return false;
            }
            else
            {
                position++; //если совпадает, увеличиваем позицию и возвращаем true
                return true;
            }
        }
        private Token GetToken(int currentPosition)
        {
            int pos = position + currentPosition;
            if (pos >= size)
            {
                return EOF;
            }
            return tokens[pos];
        }
    }
}
