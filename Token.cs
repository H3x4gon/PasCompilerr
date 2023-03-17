using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalCompilerr
{
    public class Token
    {
        public enum TypeToken
        {
            NUM,
            HEX_NUM,
            WORD,
            TEXT,

            //Ключевые слова
            WRITELN,
            WRITE,
            IF,
            THEN,
            ELSE,
            BEGIN,
            END,
            WHILE,
            FOR,
            DO,
            

            //Операторы
            PLUS,
            MINUS,
            STAR,
            SLASH,
            EQUAL, // :=
            EQUALEQUAL, // ==
            GREATER, // >
            GREATEREQUAL, // >=
            LESSER, // <
            LESSEREQUAL, // <=
            NEGATE, // not
            NEGATEEQUAL, // <>
            OROR, // or
            ANDAND, // and

            LBRACKET, // (
            RBRACKET, // )

            EOF
        }
        private TypeToken type;
        private string text;

        public Token()
        {
        
        }
        public Token(TypeToken type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public new TypeToken GetType()
        {
            return type;
        }

        public void GetType(TypeToken type)
        {
            this.type = type;
        }

        public string GetText()
        {
            return text;
        }

        public void SetText(string text)
        {
            this.text = text;
        }

        public override string ToString()
        {
            return type + " " + text;
        }
    }
}
