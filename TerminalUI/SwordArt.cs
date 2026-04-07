using System.Collections.Generic;

namespace TerminalUI
{
    /// <summary>
    /// 검 레벨별 ASCII 아트 데이터.
    /// GetArt(level)로 문자열 배열을 반환한다.
    /// </summary>
    public static class SwordArt
    {
        /// <summary>레벨에 해당하는 ASCII 아트 라인 배열을 반환한다.</summary>
        public static string[] GetArt(int level)
        {
            if (level <= 0) return Wooden;
            if (level <= 3) return Iron;
            if (level <= 5) return Steel;
            if (level <= 8) return Fantasy;
            if (level <= 9) return Historic;
            if (level <= 12) return Legendary;
            if (level <= 14) return Mythic;
            if (level <= 16) return Divine;
            if (level <= 17) return Subculture;
            return Transcendent;
        }

        /// <summary>레벨에 해당하는 Spectre 마크업 색상을 반환한다.</summary>
        public static string GetColor(int level)
        {
            if (level <= 0) return "grey";
            if (level <= 3) return "silver";
            if (level <= 5) return "white";
            if (level <= 8) return "dodgerblue1";
            if (level <= 9) return "darkorange";
            if (level <= 12) return "gold1";
            if (level <= 14) return "orchid";
            if (level <= 16) return "red1";
            if (level <= 17) return "deeppink1";
            return "yellow";
        }

        private static readonly string[] Wooden = new[]
        {
            "      |      ",
            "      |      ",
            "      |      ",
            "      |      ",
            "      |      ",
            "     -+-     ",
        };

        private static readonly string[] Iron = new[]
        {
            "      /\\     ",
            "     /  \\    ",
            "     \\  /    ",
            "      ||     ",
            "      ||     ",
            "     -\"\"-    ",
        };

        private static readonly string[] Steel = new[]
        {
            "      /\\     ",
            "     /..\\    ",
            "     \\.../    ",
            "      ||     ",
            "     =''=    ",
            "             ",
        };

        private static readonly string[] Fantasy = new[]
        {
            "      /\\     ",
            "     /++\\    ",
            "     \\++/    ",
            "      |||    ",
            "     =|'|=   ",
            "             ",
        };

        private static readonly string[] Historic = new[]
        {
            "     /\\/\\    ",
            "    / <> \\   ",
            "    \\ <> /   ",
            "     \\  /    ",
            "      ||     ",
            "    ==''==   ",
        };

        private static readonly string[] Legendary = new[]
        {
            "      /#\\    ",
            "     /###\\   ",
            "     \\###/   ",
            "      |##|   ",
            "     ='  '=  ",
            "             ",
        };

        private static readonly string[] Mythic = new[]
        {
            "    * /\\ *   ",
            "     /%%\\    ",
            "     \\%%/    ",
            "      |%|    ",
            "    ='    '= ",
            "             ",
        };

        private static readonly string[] Divine = new[]
        {
            "   *  /\\  *  ",
            "    */@@\\*   ",
            "     \\@@/    ",
            "      |@|    ",
            "   ='      '=",
            "             ",
        };

        private static readonly string[] Subculture = new[]
        {
            "  * * /\\ * * ",
            "    /~~~~\\   ",
            "    \\~~~~/   ",
            "     |**|    ",
            "  ='        '=",
            "              ",
        };

        private static readonly string[] Transcendent = new[]
        {
            " * * /^^\\ * * ",
            "   /######\\   ",
            "   \\######/   ",
            "    |****|    ",
            " ='          '=",
            "               ",
        };
    }
}
