namespace CuiApp.Rendering;

public static class SwordAsciiArt
{
    public static string[] Get(int level) => level switch
    {
        0 => WoodenSword,
        <= 5 => BasicSword,
        <= 9 => MagicSword,
        <= 14 => LegendarySword,
        <= 17 => MythicSword,
        _ => CosmicSword,
    };

    // +0: 나무검 — 소박한 막대기
    private static readonly string[] WoodenSword =
    [
        @"        |        ",
        @"        |        ",
        @"        |        ",
        @"       ===       ",
        @"        |        ",
    ];

    // +1~5: 기본 검 — 깔끔한 금속 검
    private static readonly string[] BasicSword =
    [
        @"        /\        ",
        @"       /  \       ",
        @"      /    \      ",
        @"      \    /      ",
        @"       \  /       ",
        @"       |  |       ",
        @"      [====]      ",
        @"       |  |       ",
    ];

    // +6~9: 마법 검 — 룬 문양 + 빛
    private static readonly string[] MagicSword =
    [
        @"     *  /\  *     ",
        @"       /oo\       ",
        @"      / oo \      ",
        @"     /  oo  \     ",
        @"     \  oo  /     ",
        @"      \ oo /      ",
        @"    ---[==]---    ",
        @"       |oo|       ",
        @"       |  |       ",
    ];

    // +10~14: 전설 검 — 화려한 날 + 보석
    private static readonly string[] LegendarySword =
    [
        @"    \   /\   /    ",
        @"     \ /  \ /     ",
        @"      / <> \      ",
        @"     / /<>\ \     ",
        @"    / / <> \ \    ",
        @"    \ \ <> / /    ",
        @"     \ \  / /     ",
        @"   ====[**]====   ",
        @"       |<>|       ",
        @"       |  |       ",
    ];

    // +15~17: 신화 검 — 오라 + 화염
    private static readonly string[] MythicSword =
    [
        @"  ~       /\       ~  ",
        @"    *    /  \    *    ",
        @"   ~ *  / ** \  * ~  ",
        @"      */ **** \*     ",
        @"      / ****** \     ",
        @"      \ ****** /     ",
        @"    *  \ **** /  *   ",
        @"   ~  ==[ ** ]==  ~  ",
        @"       |[####]|      ",
        @"       | |  | |      ",
    ];

    // +18~20: 우주 검 — 별 + 차원의 균열
    private static readonly string[] CosmicSword =
    [
        @" .  *  .  /\  .  *  . ",
        @"   .  * _/  \_ *  .   ",
        @" *   _/  @@@@  \_   * ",
        @"   _/ @@ @@@@ @@ \_   ",
        @"  / @@@ @@@@@@ @@@ \  ",
        @"  \ @@@ @@@@@@ @@@ /  ",
        @" * \_  @@ @@@@ @@  _/ ",
        @"   . \_  @@@@  _/ .   ",
        @"  *  ==[@@##@@]==  *  ",
        @" .  *  |[####]|  *  . ",
        @"   .   | |  | |   .   ",
    ];
}
