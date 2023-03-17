using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompilerr
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "";
            if (File.Exists(@"C:\Users\user\source\repos\PascalCompilerr\Program.txt"))
            {
                input = File.ReadAllText(@"C:\Users\user\source\repos\PascalCompilerr\Program.txt");
            }
            else
            {
                throw new Exception("Файл не найден");
            }
        
            Lexer lex = new Lexer(input);
            List<Token> tokens = lex.Tokenize();

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }

            IStatement program = new Parser(tokens).Parse();
            Console.WriteLine(program.ToString());
            program.execute();
            Console.ReadLine();
        }
    }
} 
