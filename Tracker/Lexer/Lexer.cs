using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tracker
{
    public enum TokenType
    {
        Invalid,
        Function,
        Hook,
        Command,
        OpenDoc,
        CloseDoc,
        Summary,
        Parameter,
        Return,
        Notes,
    }

    class Lexer
    {
        private List<Token> _tokenDefinitions = new List<Token>();

        public Lexer() // TODO: allows for extension in token types / move these to a custom pre-defined token set
        {
            _tokenDefinitions.Add(new Token(TokenType.Hook, "(hook.(a|A)dd)(.+\")(.+\")")); // This this can be improved by using the below one / change it to include UniqueID
            _tokenDefinitions.Add(new Token(TokenType.Command, "(command.(a|A)?dd\\()(\")(\\w+)(\")"));
            _tokenDefinitions.Add(new Token(TokenType.OpenDoc, "(--\\[\\[)"));
            _tokenDefinitions.Add(new Token(TokenType.CloseDoc, "(\\]\\]--)"));
            _tokenDefinitions.Add(new Token(TokenType.Summary, "(@(S|s)ummary:)([\\s\\w].*)"));
            _tokenDefinitions.Add(new Token(TokenType.Parameter, "(@(P|p)aram:)([\\s\\w].*)"));
            _tokenDefinitions.Add(new Token(TokenType.Return, "(@(R|r)eturn:)([\\s\\S]).*"));
            _tokenDefinitions.Add(new Token(TokenType.Notes, "(@(N|n)otes:)([\\s\\w].*)"));
            _tokenDefinitions.Add(new Token(TokenType.Function, "(function)(\\s)+(\\w+(:|\\.)?\\w+)(\\()"));
        }

        private TokenType _lastToken;
        private LexResult _lastLexResult;
        private LexResult _activeLexResult;
        private List<LexResult> _results;
        public List<LexResult> LexFile(string[] file, string path)
        {
            _results = new List<LexResult>();
            int linenum = 0;
            foreach (string line in file)
            {
                linenum++;
                TokenResult res = new TokenResult {Matched = false};

                foreach (Token token in _tokenDefinitions)
                {
                    res = token.Match(line);

                    if (res.Matched)
                    {
                        break; // Found a matching token, we're only looking for one token per line
                    }

                }

                if (!res.Matched) { continue; } // We didnt find a matching token on that line move to the next one
                
                switch (res.Type)
                {
                    case TokenType.OpenDoc:
                        _activeLexResult = new LexResult(); // OpenDoc token creates a new version of the LexResult
                        break;
                    case TokenType.CloseDoc:
                        if (_activeLexResult != null)
                        {
                            _activeLexResult.FileInfo = new FileInfo() // TODO: Refactor FileInfo Class
                            {
                                Index = res.StartIndex.ToString(),
                                Line = linenum.ToString(),
                                Path = path,
                            };
                            _results.Add(_activeLexResult);
                            _lastLexResult = _activeLexResult;
                        }
                        _activeLexResult = null; // Clear it and wait for the next OpenDoc token to create a new instance
                        break;
                    case TokenType.Command:
                        if (_lastLexResult != null) _lastLexResult.Name = res.Groups[4].ToString();
                        _lastLexResult = null; ;
                        break;
                    case TokenType.Hook:
                        if (_lastLexResult != null)
                            _lastLexResult.Name = res.Groups[4]
                                .ToString()
                                .Substring(0, res.Groups[4].Length - 1); // TODO: Improve Regex, so we dont have to do these stupid substrings
                        _lastLexResult = null; ;
                        break;
                    case TokenType.Function:
                        if (_lastLexResult != null) _lastLexResult.Name = res.Groups[3].ToString();
                        _lastLexResult = null; ;
                        break;
                    case TokenType.Summary:
                        if (_activeLexResult != null) _activeLexResult.Summary = res.Value;
                        break;
                    case TokenType.Parameter:
                        _activeLexResult?.Parameters.Add(res.Value);
                        break;
                    case TokenType.Return:
                        _activeLexResult?.Return.Add(res.Value);
                        break;
                    case TokenType.Notes:
                        if (_activeLexResult != null) _activeLexResult.Notes = res.Value;
                        break;
                }
                _lastToken = res.Type;
            }
            return _results;
        }
    }

    class LexResult
    {
        public string Name;
        public string Summary;
        public List<string> Parameters = new List<string>();
        public List<string> Return = new List<string>();
        public string Notes;
        public FileInfo FileInfo;

    }

    public class FileInfo
    {
        public string Path;
        public string Line;
        public string Index;
    }

    class Token
    {
        private Regex _matchRegex;
        private TokenType _token;

        public Token(TokenType token, string regex)
        {
            _matchRegex = new Regex(regex);
            _token = token;
        }

        public TokenResult Match(string input)
        {
            var match = _matchRegex.Match(input);
            if (match.Success)
            {
                return new TokenResult
                {
                    Matched = true,
                    Value = match.Value,
                    Groups = match.Groups,
                    Type = _token,
                    StartIndex = match.Index,
                    Length = match.Length,
                };
            }
            return new TokenResult
            {
                Matched = false
            };
        }
    }

    class TokenResult
    {
        public bool Matched;
        public string Value;
        public GroupCollection Groups;
        public TokenType Type;
        public int StartIndex;
        public int Length;
    }
}
