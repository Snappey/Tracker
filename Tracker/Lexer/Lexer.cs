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
        HookCall,
        Command,
        OpenDoc,
        CloseDoc,
        Summary,
        Parameter,
        Return,
        Notes,
    }

    [Flags]
    public enum LexType
    {
        Invalid,
        Function,
        Command,
        Hook,
        HookCall,
    }

    class Lexer
    {
        private List<Token> _tokenDefinitions = new List<Token>();

        public Lexer() // TODO: allows for extension in token types / move these to a custom pre-defined token set
        {
            _tokenDefinitions.Add(new Token(TokenType.Hook, "(hook.(a|A)dd)\\(\\\"(.+)\\\",\\s\\\"(.+)\\\""));
            _tokenDefinitions.Add(new Token(TokenType.HookCall, "(hook.(Run|Call)\\(\\\")(.+)(\")"));
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
        private List<LexResult> _funcs;
        private List<LexResult> _hooks;
        private List<LexResult> _cmds;

        public FileResults LexFile(string[] file, string path)
        {
            _results = new List<LexResult>();
            _funcs = new List<LexResult>();
            _hooks = new List<LexResult>();
            _cmds = new List<LexResult>();

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
                        _activeLexResult = new LexResult(); // OpenDoc token creates a new instance of LexResult
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
                            _activeLexResult.Documented = true;
                            _results.Add(_activeLexResult);
                            _lastLexResult = _activeLexResult;
                        }
                        _activeLexResult = null; // Clear it and wait for the next OpenDoc token to create a new instance
                        break;
                    case TokenType.Command: // TODO: Cleanup below, lots of repeating
                        if (_lastLexResult != null)
                        {
                            _lastLexResult.Name = res.Groups[4].ToString();
                            _lastLexResult.Type = LexType.Command;
                        }
                        else
                        {
                            _cmds.Add(new LexResult() // TODO: concommand.Add("PrintOutside", ReturnOutside) Counts as undocumented even though the function is documented further up
                            {
                                Name = line,
                                FileInfo = new FileInfo()
                                {
                                    Index = res.StartIndex.ToString(),
                                    Line = linenum.ToString(),
                                    Path = path,
                                },
                                Documented = false,
                            });
                        }
                        _lastLexResult = null;
                        break;
                    case TokenType.HookCall:
                        if (_lastLexResult != null)
                        {
                            _lastLexResult.Name = res.Groups[3].ToString();
                            _lastLexResult.Type = LexType.HookCall;
                        }
                        else
                        {
                            _funcs.Add(new LexResult()
                            {
                                Name = line,
                                FileInfo = new FileInfo()
                                {
                                    Index = res.StartIndex.ToString(),
                                    Line = linenum.ToString(),
                                    Path = path,
                                },
                                Documented = false,
                            });
                        }
                        _lastLexResult = null;
                        break;
                    case TokenType.Hook:
                        if (_lastLexResult != null)
                        {
                            _lastLexResult.Name = res.Groups[3] + " - " + res.Groups[4];
                            _lastLexResult.Type = LexType.Hook;
                        }
                        else
                        {
                            _hooks.Add(new LexResult()
                            {
                                Name = line,
                                FileInfo = new FileInfo()
                                {
                                    Index = res.StartIndex.ToString(),
                                    Line = linenum.ToString(),
                                    Path = path,
                                },
                                Documented = false,
                            });
                        }
                        _lastLexResult = null; ;
                        break;
                    case TokenType.Function:
                        if (_lastLexResult != null) // Check if a Result is still waiting function assignment
                        {
                            _lastLexResult.Name = res.Groups[3].ToString();
                            _lastLexResult.Type = LexType.Function;
                        }
                        else
                        {
                            _funcs.Add(new LexResult()
                            {
                                Name = line,
                                FileInfo = new FileInfo()
                                {
                                    Index = res.StartIndex.ToString(),
                                    Line = linenum.ToString(),
                                    Path = path,
                                },
                                Documented = false,
                            });   
                        }
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
            return new FileResults()
            {
                DocumentedResults = _results,

                UnDocumentedFunctions =  _funcs,
                UnDocumentedHooks = _hooks,
                UnDocumentedCommands = _cmds,
            };
        }
    }

    class FileResults
    {
        public List<LexResult> DocumentedResults = new List<LexResult>();

        public List<LexResult> UnDocumentedFunctions = new List<LexResult>();
        public List<LexResult> UnDocumentedHooks = new List<LexResult>();
        public List<LexResult> UnDocumentedCommands = new List<LexResult>();
    }

    class LexResult
    {
        public string Name;
        public string Summary;
        public bool Documented;
        public List<string> Parameters = new List<string>();
        public List<string> Return = new List<string>();
        public string Notes;
        public FileInfo FileInfo;
        public LexType Type;
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
