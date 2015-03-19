using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eratter
{
    class Parser
    {
        // 正規表現定義
        private static readonly Dictionary<string, string> patternRegexes = new Dictionary<string, string>
        {
            {"DimStart", @"((?<DimStart>#dim(|s|f))\s)"},
            {"DimClassStart", @"((?<DimClassStart>#dimc)\s)"},
            {"DimSave", @"((?<DimSave>global|savedata)\s)"},
            {"DimIdentifier", @"(?<DimIdentifier>\w+)"},
            {"DimHash", @"((?<DimHash>array|hash)\s)"},
            {"DimNumber", @"(?<DimNumber>[0-9]+)"},
            {"DimComma", @"(?<DimComma>,)"},
            {"ClassStart", @"((?<ClassStart>#class)\s)"},
            {"ClassIdentifier", @"(?<ClassIdentifier>\w+)"},
            {"FunctionStart", @"((?<FunctionStart>sub)\s)"},
            {"FunctionName", @"(?<FunctionName>\w+)"},
            {"FunctionArg", @"(?<FunctionArg>\w+)"},
            {"FunctionComma", @"(?<FunctionComma>,)"},

            {"NextLine", @"(?<NextLine>\\)"},
            {"Return", @"((?<Return>return)\s)"},
            {"Sif", @"((?<Sif>sif)\s)"},
            {"IfStart", @"((?<IfStart>if)\s)"},
            {"IfEnd", @"((?<IfEnd>endif)\s)"},
            {"SelectStart", @"((?<SelectStart>selectcase)\s)"},
            {"SelectCase", @"((?<SelectCase>case)\s)"},
            {"SelectCaseNumber", @"(?<SelectCaseNumber>[0-9]+)"},
            {"SelectCaseLiteral", @"(""(?<SelectCaseLiteral>.*?)(?<!\\)"")"},
            {"SelectDefault", @"((?<SelectDefault>default)\s)"},
            {"SelectEnd", @"((?<SelectEnd>endselect)\s)"},
            {"ForStart", @"((?<ForStart>for)\s)"},
            {"ForEnd", @"((?<ForEnd>next)\s)"},
            {"WhileStart", @"((?<WhileStart>while)\s)"},
            {"WhileEnd", @"((?<WhileEnd>wend)\s)"},
            {"DoLoopStart", @"((?<DoLoopStart>do)\s)"},
            {"DoLoopEnd", @"((?<DoLoopEnd>loop)\s)"},
            {"Break", @"((?<Break>break)\s)"},
            {"Try", @"((?<Try>try)\s)"},
            {"Catch", @"((?<Catch>catch)\s)"},
            {"CatchEnd", @"((?<CatchEnd>endcatch)\s)"},

            {"Number", @"(?<Number>[0-9]+)"},
            {"Function", @"(?<Function>\w+\()"},
            {"Identifier", @"(?<Identifier>\w+)"},
            {"ExprStart", @"(?<ExprStart>\()"},
            {"ExprEnd", @"(?<ExprEnd>\()"},
            {"Comma", @"(?<Comma>,)"},
            {"IndexStart", @"(?<IndexStart>\[)"},
            {"IndexEnd", @"(?<IndexEnd>\])"},
            {"Literal", @"(""(?<Literal>.*?)(?<!\\)"")"},
            {"PreIncrement", @"(?<PreIncrement>\+\+|\-\-)"},
            {"Asign", @"(?<Asign>\<\<=|\>\>=|\^=|\|=|\&=|\%=|\/=|\*=|\-=|\+=|\=)"},
            {"BinOpe", @"(?<BinOpe>==|\!=|\<=|\>=|\<<|\>>|\|\||\&\&|\||\^|\&|\~|\>|\<|\+|\-|\*|\/|\%)"},
            {"OneOpe", @"(?<OneOpe>\+|\-|\!)"},
            {"TernaryStart", @"(?<Ternary>\?)"},
            {"TernaryDelimiter", @"(?<TernaryDelimiter>\:)"},

            {"LineComment", @"(?<LineComment>;|\/\/)"},
            {"BlockCommentStart", @"(?<BlockCommentStart>\/\*)"},
            {"BlockCommentSkip", @"(?<BlockCommentSkip>.*)"},
            {"BlockCommentEnd", @"(?<BlockCommentEnd>\*\/)"},
            {"LocalScopeStart", @"(?<LocalScopeStart>\{)"},
            {"LocalScopeEnd", @"(?<LocalScopeEnd>\})"},
        };

        // ネクストパターン定義
        private static readonly string[] FirstPattern = new[]
        {
            "DimStart", "DimClassStart", "ClassStart", "FunctionStart",
            "LineComment", "BlockCommentStart", "BlockCommentEnd",
            "Return", "Break",
            "Sif", "IfStart", "IfEnd", "SelectStart", "SelectCase", "SelectDefault", "SelectEnd",
            "ForStart", "ForEnd", "WhileStart", "WhileEnd", "DoLoopStart", "DoLoopEnd",
            "Try", "Catch", "CatchEnd",
            "LocalScopeStart", "LocalScopeEnd",
            "Function", "Identifier", "PreIncrement",
        };

        private static readonly string[] DimStartAfter = new[]
        {
            "NextLine", "DimSave", "DimIdentifier",
        };
        private static readonly string[] DimSaveAfter = new[]
        {
            "NextLine", "DimIdentifier",
        };
        private static readonly string[] DimIdentifierAfter = new[]
        {
            "NextLine", "LineComment", "DimHash", "DimNumber",
        };
        private static readonly string[] DimHashAfter = new[]
        {
            "NextLine", "DimNumber",
        };
        private static readonly string[] DimNumberAfter = new[]
        {
            "NextLine", "LineComment", "DimCommaAfter",
        };
        private static readonly string[] DimCommaAfter = DimIdentifierAfter;
        private static readonly string[] ClassStartAfter = new[]
        {
            "NextLine", "ClassIdentifier",
        };
        private static readonly string[] ClassIdentifierAfter = new[]
        {
            "NextLine", "LineComment", "ClassIdentifier",
        };
        private static readonly string[] FunctionStartAfter = new[]
        {
            "NextLine", "FunctionName",
        };
        private static readonly string[] FunctionNameAfter = new[]
        {
            "NextLine", "LineComment", "FunctionArg",
        };
        private static readonly string[] FunctionArgAfter = new[]
        {
            "NextLine", "LineComment", "FunctionComma",
        };
        private static readonly string[] FunctionCommaAfter = FunctionNameAfter;

        private static readonly string[] ReturnAfter = new[]
        {
            "NextLine", "LineComment", "Number", "Identifier", "ExprStart", "PreIncrement", "Literal", "OneOpe",
        };
        private static readonly string[] SifStartAfter = new[]
        {
            "NextLine", "Number", "Identifier", "ExprStart", "PreIncrement", "OneOpe",
        };
        private static readonly string[] IfStartAfter = SifStartAfter;
        private static readonly string[] SelectStartAfter = SifStartAfter;
        private static readonly string[] SelectCaseAfter = new[]
        {
            "NextLine", "SelectCaseNumber", "SelectCaseLiteral",
        };
        private static readonly string[] ForStartAfter = new[]
        {
            "NextLine", "Identifier",
        };
        private static readonly string[] WhileStartAfter = SifStartAfter;
        private static readonly string[] DoLoopEndAfter = SifStartAfter;
        private static readonly string[] TryAfter = new[]
        {
            "Function", "Identifier", "PreIncrement",
        };

        private static readonly string[] NumberAfter = new[]
        {
            "NextLine", "LineComment", "ExprEnd", "IndexEnd", "Comma", "BinOpe", "TernaryStart", "TernaryDelimiter", 
        };
        private static readonly string[] FunctionAfter = new[]
        {
            "NextLine", "Number", "Identifier", "ExprStart", "ExprEnd", "PreIncrement", "Literal", "OneOpe",
        };
        private static readonly string[] IdentifierAfter = new[]
        {
            "NextLine", "LineComment", "ExprEnd", "IndexStart", "IndexEnd", "Comma", "Asign", "BinOpe", "TernaryStart", "TernaryDelimiter",
        };
        private static readonly string[] ExprStartAfter = FunctionAfter;
        private static readonly string[] ExprEndAfter = NumberAfter;
        private static readonly string[] CommaAfter = FunctionAfter;
        private static readonly string[] IndexStartAfter = FunctionAfter;
        private static readonly string[] IndexEndAfter = IdentifierAfter;
        private static readonly string[] LiteralAfter = NumberAfter;
        private static readonly string[] PreIncrementAfter = new[]
        {
            "NextLine", "Number", "Identifier", "ExprStart", "TernaryDelimiter",
        };
        private static readonly string[] AsignAfter = new[]
        {
            "NextLine", "Number", "Identifier", "ExprStart", "PreIncrement", "Literal", "OneOpe", "TernaryDelimiter",
        };
        private static readonly string[] BinOpeAfter = AsignAfter;
        private static readonly string[] OneOpeAfter = PreIncrementAfter;
        private static readonly string[] TernaryStartAfter = AsignAfter;
        private static readonly string[] TernaryDelimiterAfter = AsignAfter;

        private static readonly string[] LineCommentOnly = new [] { "LineComment" };
        private static readonly string[] BlockSkip = new[] { "BlockCommentEnd", "BlockCommentSkip" };

        // パターン設定構造体
        private struct PatternSetting
        {
            public string NextPattern { get; private set; }
            public string[] PatternNames { get; private set; }
            public bool IsTerminal { get; private set; }
            public bool IsAbsoluteTerminal { get; private set; }
            public PatternSetting(string[] patternNames, bool isTerminal, bool isAbsoluteTerminal = false)
            {
                PatternNames = patternNames;
                IsTerminal = isTerminal;
                IsAbsoluteTerminal = isAbsoluteTerminal;
                NextPattern = @"";

                bool isFirst = true;
                foreach (string patternName in patternNames)
                {
                    NextPattern += (isFirst) ? @"^" : @"|";
                    NextPattern += patternRegexes[patternName];
                    isFirst = false;
                }
            }
        }

        // パターン設定配列
        private static readonly Dictionary<string, PatternSetting> Patterns = new Dictionary<string, PatternSetting>
        {
            { "First", new PatternSetting(FirstPattern, true) },

            { "DimStart", new PatternSetting(DimStartAfter, false) },
            { "DimSave", new PatternSetting(DimSaveAfter, false) },
            { "DimIdentifier", new PatternSetting(DimIdentifierAfter, true) },
            { "DimHash", new PatternSetting(DimHashAfter, false) },
            { "DimNumber", new PatternSetting(DimNumberAfter, true) },
            { "DimComma", new PatternSetting(DimCommaAfter, false) },
            { "ClassStart", new PatternSetting(ClassStartAfter, false) },
            { "ClassIdentifier", new PatternSetting(ClassIdentifierAfter, true) },
            { "FunctionStart", new PatternSetting(FunctionStartAfter, true) },
            { "FunctionName", new PatternSetting(FunctionNameAfter, true) },
            { "FunctionArg", new PatternSetting(FunctionArgAfter, true) },
            { "FunctionComma", new PatternSetting(FunctionCommaAfter, false) },

            { "NextLine", new PatternSetting(LineCommentOnly, true, true) },
            { "Return", new PatternSetting(ReturnAfter, true) },
            { "Sif", new PatternSetting(SifStartAfter, false) },
            { "IfStart", new PatternSetting(IfStartAfter, false) },
            { "IfEnd", new PatternSetting(LineCommentOnly, true, true) },
            { "SelectStart", new PatternSetting(SelectStartAfter, false) },
            { "SelectCase", new PatternSetting(SelectCaseAfter, false) },
            { "SelectCaseNumber", new PatternSetting(LineCommentOnly, true, true) },
            { "SelectCaseLiteral", new PatternSetting(LineCommentOnly, true, true) },
            { "SelectDefault", new PatternSetting(LineCommentOnly, true, true) },
            { "SelectEnd", new PatternSetting(LineCommentOnly, true, true) },
            { "ForStart", new PatternSetting(ForStartAfter, false) },
            { "ForEnd", new PatternSetting(LineCommentOnly, true, true) },
            { "WhileStart", new PatternSetting(WhileStartAfter, false) },
            { "WhileEnd", new PatternSetting(LineCommentOnly, true, true) },
            { "DoLoopStart", new PatternSetting(LineCommentOnly, true, true) },
            { "DoLoopEnd", new PatternSetting(DoLoopEndAfter, false) },
            { "Break", new PatternSetting(LineCommentOnly, true, true) },
            { "Try", new PatternSetting(TryAfter, true, true) },
            { "Catch", new PatternSetting(LineCommentOnly, true, true) },
            { "CatchEnd", new PatternSetting(LineCommentOnly, true, true) },

            { "Number", new PatternSetting(NumberAfter, true) },
            { "Function", new PatternSetting(FunctionAfter, false) },
            { "Identifier", new PatternSetting(IdentifierAfter, true) },
            { "ExprStart", new PatternSetting(ExprStartAfter, false) },
            { "ExprEnd", new PatternSetting(ExprEndAfter, true) },
            { "Comma", new PatternSetting(CommaAfter, false) },
            { "IndexStart", new PatternSetting(IndexStartAfter, false) },
            { "IndexEnd", new PatternSetting(IndexEndAfter, true) },
            { "Literal", new PatternSetting(LiteralAfter, false) },
            { "PreIncrement", new PatternSetting(PreIncrementAfter, false) },
            { "Asign", new PatternSetting(AsignAfter, false) },
            { "BinOpe", new PatternSetting(BinOpeAfter, false) },
            { "OneOpe", new PatternSetting(OneOpeAfter, false) },
            { "TernaryStart", new PatternSetting(TernaryStartAfter, true) },
            { "TernaryDelimiter", new PatternSetting(TernaryDelimiterAfter, true) },

            { "LineComment", new PatternSetting(null, true) },
            { "BlockCommentStart", new PatternSetting(BlockSkip, true) },
            { "BlockCommentSkip", new PatternSetting(BlockSkip, true) },
            { "BlockCommentEnd", new PatternSetting(LineCommentOnly, true, true) },
            
            { "NextCommentCheck", new PatternSetting(null, false)},
        };

        // トークン構造体
        private struct Token
        {
            public string Name { get; private set; }
            public string Data { get; private set; }
            public int Priority { get; private set; }

            public Token(string name, string data, int priority)
            {
                Name = name;
                Data = data;
                Priority = priority;
            }
        }

        private static PatternSetting scanSetting = Patterns["First"];
        private static List<int> priorities = new List<int>();
        private static bool isTerminal = false;
        private static bool isAbsoluteTerminal = false;
        private static List<Token> stacks = new List<Token>();
        private static List<string> brackets = new List<string>();
        private static List<string> scopes = new List<string>();
        private static List<bool> ternarys = new List<bool>();
        private static readonly int BracketAddPriority = 100;
        private static bool isContinueLine = false;
        private static bool isInBlockComment = false;

        //        private static List<Command> commands = new List<Command>();

        public static Token ScanScript(string script)
        {
            return new Token();
        }

        public static void ScanLine(string str, int defaultPriority)
        {
            string temp = (str.Trim() + "\n").Replace("\\\\", "&yen;");
            isContinueLine = false;

            while (temp != "")
            {
                int matchLength = 0;
                Match m = Regex.Match(temp, scanSetting.NextPattern);

                if (m.Success)
                {
                    string name = null;
                    string value = null;
                    foreach (string patternName in scanSetting.PatternNames)
                    {
                        if (m.Groups[patternName].Value != "")
                        {
                            name = patternName;
                            value = m.Groups[name].Value;
                            break;
                        }
                    }

                    // ラインコメント開始なら後ろは見ない
                    if (name == "LineComment")
                    {
                        temp = "";
                    }
                    else
                    {
                        int substringLength = value.Length;
                        switch (name)
                        {
                            case "NextLine":
                                isContinueLine = true;
                                break;
                            case "IfStart":
                            case "SelectStart":
                            case "ForStart":
                            case "WhileStart":
                            case "DoLoopStart":
                            case "Try":
                            case "LocalScopeStart":
                                scopes.Add(name);
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "IfEnd":
                            case "SelectEnd":
                            case "ForEnd":
                            case "DoLoopEnd":
                            case "CatchEnd":
                            case "LocalScopeEnd":
                                RemoveLastScope(name);
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "Catch":
                                RemoveLastScope(name);
                                scopes.Add(name);
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "SelectCase":
                            case "SelectDefault":
                                InvalidLastScopeAssert("SelectStart");
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "Break":
                                {
                                    bool isEnableBreak = false;
                                    foreach (string scope in scopes)
                                    {
                                        if (scope != "Try")
                                        {
                                            isEnableBreak = true;
                                            break;
                                        }
                                    }
                                    // 予期しないエラー
                                    if (!isEnableBreak)
                                        Error.Exception("todo");
                                    stacks.Add(createToken(name, value, defaultPriority));
                                }
                                break;
                            case "ExprStart":
                            case "IndexStart":
                            case "Function":
                                brackets.Add(name);
                                stacks.Add(createToken(name, value, defaultPriority + BracketPriority));
                                defaultPriority += BracketAddPriority;
                                break;
                            case "ExprEnd":
                            case "IndexEnd":
                                RemoveLastBracket(name);
                                defaultPriority -= BracketAddPriority;
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "Comma":
                                // 予期しないエラー
                                if ((stacks[0].Name != "FunctionStart") &&
                                    (stacks[0].Name != "DimStart") &&
                                    (brackets[brackets.Count - 1] != "Function"))
                                    Error.Exception("todo");
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                            case "PreIncrement":
                                stacks.Add(createToken(name, value, PreIncrementPriority + defaultPriority));
                                break;
                            case "BinOpe":
                                stacks.Add(createToken(name, value, BinOpePriorities[value] + defaultPriority));
                                break;
                            case "OneOpe":
                                if (value != "+")
                                    stacks.Add(createToken(name, value, OneOpePriorities[value] + defaultPriority));
                                break;
                            case "TernaryStart":
                                brackets.Add(name);
                                stacks.Add(createToken(name, value, defaultPriority));
                                defaultPriority += BracketAddPriority;
                                break;
                            case "TernaryDelimiter":
                                if (brackets[brackets.Count - 1] == "TernaryStart")
                                {
                                    RemoveLastBracket(name);
                                    brackets.Add(name);
                                    stacks.Add(createToken(name, value, defaultPriority));
                                }
                                else if (brackets[brackets.Count - 1] == "TernaryDelimiter")
                                {
                                    defaultPriority -= BracketAddPriority;
                                    brackets.RemoveAt(brackets.Count - 1);
                                    RemoveLastBracket(name);
                                    brackets.Add(name);
                                    stacks.Add(createToken(name, value, defaultPriority));
                                }
                                else
                                {
                                    // 予期しないエラー
                                }
                            case "BlockCommentStart":
                                isInBlockComment = true;
                                break;
                            case "BlockCommentEnd":
                                // 予期しないエラー
                                if (!isInBlockComment)
                                    Error.Exception("todo");
                                isInBlockComment = false;
                                break;
                            case "SelectCaseLiteral":
                            case "Literal":
                                substringLength += "\"\"".Length;
                            default:
                                stacks.Add(createToken(name, value, defaultPriority));
                                break;
                        }
                        // マッチした文字列の切り取り
                        temp = temp.Substring(substringLength, temp.Length - substringLength).TrimStart();
                    }
                    scanSetting = Patterns[name];
                }
                else
                {
                    // 一致しなかったらループを抜ける
                    break;
                }

                // Nextパターンがnullだったら残りはコメントなのでループを抜ける
                if (scanSetting.PatternNames == null)
                {
                    temp = "";
                    break;
                }
            }

            // 行のルールが成立していないため、解釈エラー
            if (!isContinueLine && !isInBlockComment)
            {
                // 三項演算子が終わっていない場合があるので
                for (int i = brackets.Count - 1; i >= 0; ++i)
                {
                    if (brackets[i] == "TernaryDelimiter")
                    {
                        defaultPriority -= BracketAddPriority;
                        brackets.RemoveAt(i);
                    }
                }

                // 完全終端記号で終わっていない、非終端記号で終わっている
                if (((temp != "") && (scanSetting.IsAbsoluteTerminal)) ||
                    ((temp == "") && (!scanSetting.IsTerminal)))
                    Error.Exception("todo");

                // 括弧の数があっていないため、解釈エラー
                if (brackets.Count > 0)
                    Error.Exception("todo");
            }

            if (!isContinueLine)
                scanSetting = Patterns["First"];

            return;
        }

        public static Token Parse()
        {
            return new Token();
        }

        private static void InvalidLastScopeAssert(string scope)
        {
            // 予期しないエラー
            if ((scopes.Count == 0) || (scopes[scopes.Count - 1] != scope))
                Error.Exception("todo");
        }

        private static void RemoveLastScope(string endToken)
        {
            Dictionary<string, string> endToStart = new Dictionary<string, string>
            {
                { "IfEnd",         "IfStart"         },
                { "SelectEnd",     "SelectStart"     },
                { "ForEnd",        "ForStart"        },
                { "WhileEnd",      "WhileStart"      },
                { "DoLoopEnd",     "DoLoopStart"     },
                { "Catch",         "Try"             },
                { "CatchEnd",      "Catch"           },
                { "LocalScopeEnd", "LocalScopeStart" },
            };
            InvalidLastScopeAssert(endToStart[endToken]);
            brackets.RemoveAt(scopes.Count - 1);
        }

        private static void InvalidLastBracketAssert(string[] checkBrackets)
        {
            bool isInvalid = true;
            foreach (string bracket in checkBrackets)
            {
                if (brackets[brackets.Count - 1] == bracket)
                {
                    isInvalid = false;
                    break;
                }
            }

            if (isInvalid)
                Error.Exception("todo");
        }

        private static void RemoveLastBracket(string endToken)
        {
            Dictionary<string, string[]> endToStart = new Dictionary<string, string[]>
            {
                { "ExprEnd",          new [] { "ExprStart", "FunctionStart" } },
                { "IndexEnd",         new [] { "IndexStart" } },
            };
            InvalidLastBracketAssert(endToStart[endToken]);
            brackets.RemoveAt(brackets.Count - 1);
        }

        private static Token createToken(string name, string data, int priority)
        {
            Token result = new Token(name, data, priority);

            if (!priorities.Contains(priority))
                priorities.Add(priority);

            return result;
        }
    }
}
