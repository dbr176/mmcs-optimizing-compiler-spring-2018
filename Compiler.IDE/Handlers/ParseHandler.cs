﻿using Compiler.Parser;
using Compiler.Parser.AST;
using System;

namespace Compiler.IDE.Handlers
{
    class ParseHandler
    {
        public event EventHandler<BlockNode> ParsingCompleted;
        public event EventHandler ParsingErrored;
        public event EventHandler<SyntaxException> ParsingSyntaxErrored;
        public event EventHandler<LexException> ParsingLexErrored;

        public void Parse(string text)
        {
            try
            {
                var scanner = new Scanner();
                scanner.SetSource(text, 0);

                var parser = new Parser.Parser(scanner);

                var parseSuccess = parser.Parse();
                if (parseSuccess)
                {
                    ParsingCompleted(this, parser.root);
                }
                else
                {
                    ParsingErrored(this, null);
                }
            }
            catch (LexException ex)
            {
                ParsingLexErrored(this, ex);
            }
            catch (SyntaxException ex)
            {
                ParsingSyntaxErrored(this, ex);
            }
        }
    }
}