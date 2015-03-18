using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eratter
{
    class Utility
    {
        private static Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

        // 文字列のShiftJISでのバイト長を取得
        public static int ShiftJISLength(string str) { return sjisEnc.GetByteCount(str); }

        // 文字列が全て半角か調べる
        public static bool IsHalf(string str) { return str.Length * 2 == sjisEnc.GetByteCount(str); }

        // \\ → コードへの変換
        public static string YenToCode(string str) { return str.Replace("\\\\", "&yen;"); }

        // コード → \への変換
        public static string YenToMark(string str) { return str.Replace("&yen;", "\\"); }
    }
}
