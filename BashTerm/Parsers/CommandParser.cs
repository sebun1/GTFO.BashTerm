using System;
using System.Diagnostics;

namespace BashTerm.Parsers;

internal class MainParser
{
    // Main entry point of the command parser.

    public static ParseResult Parse (string input)
    {
        var parser = new Parser(input);
        
        try {
            var command = parser.ParseCommands();
            return new ParsedCommand(command);
        }

        catch (Exception e) {
            return new ParseError(e.ToString());
        }
    }
}

abstract record ParseResult;

record ParsedCommand (Command cmd) : ParseResult;

record ParseError (string cause) : ParseResult;

abstract record Command;

record Execve (string name, List<string> args) : Command;

record ListCommand (List<string> args) : Command;

record QueryCommand (string item) : Command;

record PingCommand (string item) : Command;

record Pipe (Command pre, Command post) : Command;

internal class Parser
{
    private Lexer lexer;

    public Parser (string input)
    {
        this.lexer = new Lexer(input);
    }

    public Command ParseCommands ()
    {
        var cmd = ParseOneCommand();

        if (lexer.Peek() is TokenPipe) {
            lexer.Consume();
            return new Pipe(cmd, ParseCommands());
        }
        else {
            return cmd;
        }
    }

    public Command ParseOneCommand ()
    {
        Token tok = lexer.Peek();

        if (tok is TokenWord(string word)) {
            lexer.Consume();

            return word switch {
                "list"   => new ListCommand(ParseArgs()),
                "query"  => new QueryCommand(ParseArg()),
                "ping"   => new PingCommand(ParseArg()),
                _        => throw new Exception($"unrecognized command: {word}")
            };
        }

        throw new Exception($"command must start with a word, got: {tok}");
    }

    public string ParseArg ()
    {
        Token tok = lexer.Peek();

        if (tok is TokenWord(string arg)) {
            lexer.Consume();
            return arg;
        }
        else {
            throw new Exception($"expected argument, got {tok}");
        }
    }

    public List<string> ParseArgs ()
    {
        List<string> args = new List<string>();
        while (lexer.Peek() is TokenWord(string arg)) {
            lexer.Consume();
            args.Add(arg);
        }
        return args;
    }
}