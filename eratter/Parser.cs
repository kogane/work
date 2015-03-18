using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using eratter.Tokens;

namespace eratter
{
    class Parser
    {
        // 正規表現定義
        private static readonly string DimStart          = @"((?<DimStart>#dim(|s|f))\s)";
        private static readonly string DimClassStart     = @"((?<DimClassStart>#dimc)\s)";
        private static readonly string DimGlobal         = @"((?<DimGlobal>global)\s)";
        private static readonly string DimSaveData       = @"((?<DimSaveData>savedata)\s)";
        private static readonly string DimIdentifier     = @"(?<DimIdentifier>\w+)";
        private static readonly string DimHash           = @"((?<DimHash>array|hash)\s)";
        private static readonly string DimNumber         = @"(?<DimNumber>[0-9]+)";
        private static readonly string DimComma          = @"(?<DimComma>,)";
        private static readonly string ClassStart        = @"((?<ClassStart>#class)\s)";
        private static readonly string ClassIdentifier   = @"(?<ClassIdentifier>\w+)";
        private static readonly string FunctionStart     = @"((?<FunctionStart>sub)\s)";
        private static readonly string FunctionName      = @"(?<FunctionName>\w+)";
        private static readonly string FunctionArg       = @"(?<FunctionArg>\w+)";
        private static readonly string FunctionComma     = @"(?<FunctionComma>,)";

        private static readonly string NextLine          = @"(?<NextLine>\\)";
        private static readonly string Return            = @"((?<Return>return)\s)";
        private static readonly string IfStart           = @"((?<IfStart>sif|if)\s)";
        private static readonly string IfEnd             = @"((?<IfEnd>endif)\s)";
        private static readonly string SelectStart       = @"((?<SelectStart>selectcase)\s)";
        private static readonly string SelectCase        = @"((?<SelectCase>case)\s)";
        private static readonly string SelectCaseNumber  = @"(?<SelectCaseNumber>[0-9]+)";
        private static readonly string SelectCaseLiteral = @"(""(?<SelectCaseLiteral>.*?)(?<!\\)"")";
        private static readonly string SelectDefault     = @"((?<SelectDefault>default)\s)";
        private static readonly string SelectEnd         = @"((?<SelectEnd>endselect)\s)";
        private static readonly string ForStart          = @"((?<ForStart>for)\s)";
        private static readonly string ForEnd            = @"((?<ForEnd>next)\s)";
        private static readonly string WhileStart        = @"((?<WhileStart>while)\s)";
        private static readonly string WhileEnd          = @"((?<WhileEnd>wend)\s)";
        private static readonly string DoLoopStart       = @"((?<DoLoopStart>do)\s)";
        private static readonly string DoLoopEnd         = @"((?<DoLoopEnd>loop)\s)";
        private static readonly string Break             = @"((?<Break>break)\s)";
        private static readonly string Try               = @"((?<Try>try)\s)";
        private static readonly string Catch             = @"((?<Catch>catch)\s)";
        private static readonly string CatchEnd          = @"((?<CatchEnd>endcatch)\s)";

        private static readonly string Number            = @"(?<Number>[0-9]+)";
        private static readonly string Function          = @"(?<Function>\w+\()";
        private static readonly string Identifier        = @"(?<Identifier>\w+)";
        private static readonly string ExprScopeStart    = @"(?<ExprScopeStart>\()";
        private static readonly string ExprScopeEnd      = @"(?<ExprScopeEnd>\()";
        private static readonly string Comma             = @"(?<Comma>,)";
        private static readonly string IndexStart        = @"(?<IndexStart>\[)";
        private static readonly string IndexEnd          = @"(?<IndexEnd>\])";
        private static readonly string Literal           = @"(""(?<SelectCaseLiteral>.*?)(?<!\\)"")";
        private static readonly string RegexStart        = @"(?<RegexStart>@"")";
        private static readonly string RegexEnd          = @"(?<RegexEnd>@"")";
        private static readonly string PreIncrement      = @"(?<OneOpe>\+\+|\-\-)";
        private static readonly string Asign             = @"(?<Asign>\<\<=|\>\>=|\^=|\|=|\&=|\%=|\/=|\*=|\-=|\+=|\=)";
        private static readonly string BinOpe            = @"(?<BinOpe>==|\!=|\<=|\>=|\<<|\>>|\|\||\&\&|\||\^|\&|\>|\<|\+|\-|\*|\/|\%)";
        private static readonly string OneOpe            = @"(?<OneOpe>\+|\-|\!)";

        private static readonly string LineComment       = @"(?<LineComment>;|\/\/)";
        private static readonly string BlockCommentStart = @"(?<BlockCommentStart>\/\*)";
        private static readonly string BlockCommentEnd   = @"(?<BlockCommentEnd>\*\/)";
        private static readonly string LocalScopeStart   = @"(?<LocalScopeStart>\{)";
        private static readonly string LocalScopeEnd     = @"(?<LocalScopeEnd>\})";

        private static readonly string Pre = @"^";
        private static readonly string Or = @"|";

        // ネクストパターン定義
        private static readonly string FirstPattern = Pre + DimStart + Or + DimClassStart + Or + ClassStart + Or + FunctionStart + Or +
            LineComment + Or + BlockCommentStart + Or + BlockCommentEnd + Or + Return + Or + 
            IfStart + Or + IfEnd + Or + SelectStart + Or + SelectCase + Or + SelectDefault + Or + SelectEnd + Or + 
            ForStart + Or + ForEnd + Or + WhileStart + Or + WhileEnd + Or + DoLoopStart + Or + DoLoopEnd + Or + 
            Break + Or + Try + Or + Catch + Or + CatchEnd + Or + LocalScopeStart + Or + LocalScopeEnd + Or +
            Function + Or + Identifier + Or + PreIncrement;

        private static readonly string DimStartAfter = Pre + NextLine + Or + DimGlobal + Or + DimSaveData + Or + DimIdentifier;
        private static readonly string DimGlobalAfter = Pre + NextLine + Or + DimSaveData + Or + DimIdentifier;
        private static readonly string DimSaveDataAfter = Pre + NextLine + Or + DimIdentifier;
        private static readonly string DimIdentifierAfter = Pre + NextLine + Or + LineComment + Or + DimHash + Or + DimNumber;
        private static readonly string DimHashAfter = Pre + NextLine + Or + DimNumber;
        private static readonly string DimNumberAfter = Pre + NextLine + Or + LineComment + Or + DimCommaAfter;
        private static readonly string DimCommaAfter = DimIdentifierAfter;
        private static readonly string ClassStartAfter = Pre + NextLine + Or + ClassIdentifier;
        private static readonly string ClassIdentifierAfter = Pre + NextLine + Or + LineComment + Or + ClassIdentifier;
        private static readonly string FunctionStartAfter = Pre + NextLine + Or + FunctionName;
        private static readonly string FunctionNameAfter = Pre + NextLine + Or + LineComment + Or + FunctionArg;
        private static readonly string FunctionArgAfter = Pre + NextLine + Or + LineComment + Or + FunctionComma;
        private static readonly string FunctionCommaAfter = FunctionNameAfter;

        private static readonly string ReturnAfter = Pre + NextLine + Or + LineComment + Or + Number + Or + Identifier + Or + ExprScopeStart + Or + PreIncrement + Or + Literal + Or + OneOpe;
        private static readonly string IfStartAfter = Pre + NextLine + Or + Number + Or + Identifier + Or + ExprScopeStart + Or + PreIncrement + Or + OneOpe;
        private static readonly string SelectStartAfter = IfStartAfter;
        private static readonly string SelectCaseAfter = Pre + NextLine + Or + SelectCaseNumber + Or + SelectCaseLiteral;
        private static readonly string ForStartAfter = Pre + NextLine + Or + Identifier;
        private static readonly string WhileStartAfter = IfStartAfter;
        private static readonly string DoLoopEndAfter = IfStartAfter;
        private static readonly string TryAfter = Pre + Function + Or + Identifier + Or + PreIncrement;

        private static readonly string NumberAfter = Pre + NextLine + Or + LineComment + Or + ExprScopeEnd + Or + IndexEnd + Or + Comma + Or + BinOpe;
        private static readonly string FunctionAfter = Pre + NextLine + Or + Number + Or + Identifier + Or + ExprScopeStart + Or + ExprScopeEnd + Or + PreIncrement + Or + Literal + Or + OneOpe;
        private static readonly string IdentifierAfter = Pre + NextLine + Or + LineComment + Or + ExprScopeEnd + Or + IndexStart + Or + IndexEnd + Or + Comma + Or + Asign + Or + BinOpe;
        private static readonly string ExprScopeStartAfter = FunctionAfter;
        private static readonly string ExprScopeEndAfter = NumberAfter;
        private static readonly string CommaAfter = FunctionAfter;
        private static readonly string IndexStartAfter = FunctionAfter;
        private static readonly string IndexEndAfter = IdentifierAfter;
        private static readonly string LiteralAfter = NumberAfter;
        private static readonly string PreIncrementAfter = Pre + NextLine + Or + Number + Or + Identifier + Or + ExprScopeStart;
        private static readonly string AsignAfter = Pre + NextLine + Or + Number + Or + Identifier + Or + ExprScopeStart + Or + Literal;
        private static readonly string BinOpeAfter = AsignAfter;
        private static readonly string OneOpeAfter = PreIncrementAfter;

        // パターン設定構造体
        private struct PatternSetting
        {
            public string NextPattern = null;
            public bool IsTerminal = false;
            public bool IsAbsoluteTerminal = false;
            public PatternSetting(string pattern, bool isTerminal, bool isAbsoluteTerminal = false)
            {
                NextPattern = pattern;
                IsTerminal = isTerminal;
                IsAbsoluteTerminal = isAbsoluteTerminal;
            }
        }

        // パターン設定配列
        private static readonly Dictionary<string, PatternSetting> Patterns = new Dictionary<string, PatternSetting>
        {
            { "First", new PatternSetting(FirstPattern, true) },

            { "DimStart", new PatternSetting(DimStartAfter, false) },
            { "DimGlobal", new PatternSetting(DimGlobalAfter, false) },
            { "DimSaveData", new PatternSetting(DimSaveDataAfter, false) },
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

            { "NextLine", new PatternSetting(null, true, true) },
            { "Return", new PatternSetting(ReturnAfter, true) },
            { "IfStart", new PatternSetting(IfStartAfter, false) },
            { "IfEnd", new PatternSetting(null, true, true) },
            { "SelectStart", new PatternSetting(SelectStartAfter, false) },
            { "SelectCase", new PatternSetting(SelectCaseAfter, false) },
            { "SelectCaseNumber", new PatternSetting(null, true, true) },
            { "SelectCaseLiteral", new PatternSetting(null, true, true) },
            { "SelectDefault", new PatternSetting(null, true, true) },
            { "SelectEnd", new PatternSetting(null, true, true) },
            { "ForStart", new PatternSetting(ForStartAfter, false) },
            { "ForEnd", new PatternSetting(null, true, true) },
            { "WhileStart", new PatternSetting(WhileStartAfter, false) },
            { "WhileEnd", new PatternSetting(null, true, true) },
            { "DoLoopStart", new PatternSetting(null, true, true) },
            { "DoLoopEnd", new PatternSetting(DoLoopEndAfter, false) },
            { "Break", new PatternSetting(null, true, true) },
            { "Try", new PatternSetting(TryAfter, true, true) },
            { "Catch", new PatternSetting(null, true, true) },
            { "CatchEnd", new PatternSetting(null, true, true) },

            { "Number", new PatternSetting(NumberAfter, true) },
            { "Function", new PatternSetting(FunctionAfter, false) },
            { "Identifier", new PatternSetting(IdentifierAfter, true) },
            { "ExprScopeStart", new PatternSetting(ExprScopeStartAfter, false) },
            { "ExprScopeEnd", new PatternSetting(ExprScopeEndAfter, true) },
            { "Comma", new PatternSetting(CommaAfter, false) },
            { "IndexStart", new PatternSetting(IndexStartAfter, false) },
            { "IndexEnd", new PatternSetting(IndexEndAfter, true) },
            { "Literal", new PatternSetting(LiteralAfter, false) },
            { "PreIncrement", new PatternSetting(PreIncrementAfter, false) },
            { "Asign", new PatternSetting(AsignAfter, false) },
            { "BinOpe", new PatternSetting(BinOpeAfter, false) },
            { "OneOpe", new PatternSetting(OneOpeAfter, false) },
            { "LineComment", new PatternSetting(null, true) },
            { "BlockCommentStart", new PatternSetting(null, true) },
            { "BlockCommentEnd", new PatternSetting(null, true, true) },
        };

        private static PatternSetting scanSetting = Patterns["First"];
        private static string nextPattern = FirstPattern;
        private static List<int> priorities = new List<int>();
        private static bool isTerminal = false;
        private static bool isAbsoluteTerminal = false;
        private static List<Token> stacks = new List<Token>();
//        private static List<Command> commands = new List<Command>();

        public static Token ScanScript(string script)
        {
            return null;
        }

        public static List<Token> ScanLine(string str)
        {
            List<Token> tokens = new List<Token>();
            string strCopy = (str.Trim() + "\n").Replace("\\\\", "&yen;");
            PatternSetting? nextSetting = null;
            int exprScopeNum = 0;
            int indexNum = 0;

            while (strCopy != "")
            {
                int matchLength = 0;
                Match m = Regex.Match(strCopy, scanSetting.NextPattern);

                if (m.Success)
                {

                }
                else
                {
                    // 一致しなかったらループを抜ける
                    break;
                }

                // Nextパターンがnullだったらループを抜ける
                if (!nextSetting.HasValue)
                    break;

                scanSetting = nextSetting.Value;
                strCopy = strCopy.Substring(0, matchLength).TrimStart();
            }

            // 行の体を為していないため、解釈エラー
            if (((strCopy != "") && (scanSetting.IsAbsoluteTerminal)) ||
                ((strCopy == "") && (!scanSetting.IsTerminal)))
                Error.Exception("todo");

            // 括弧の数があっていないため、解釈エラー
            if ((exprScopeNum > 0) || (indexNum > 0))
                Error.Exception("todo");

            return (tokens.Count > 0) ? tokens : null;
        }

        public static Token Parse()
        {
            return null;
        }

        private static void addPriority(int priority)
        {
            if (!priorities.Contains(priority))
                priorities.Add(priority);
        }
    }
}
