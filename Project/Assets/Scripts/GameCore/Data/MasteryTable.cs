namespace GameCore.Data;

public class MasteryLevel
{
    public int Level { get; }
    public int RequiredExp { get; }
    public string Description { get; }

    public MasteryLevel(int level, int requiredExp, string description)
    {
        Level = level;
        RequiredExp = requiredExp;
        Description = description;
    }
}

public class MasteryTable
{
    private static readonly MasteryLevel[] Levels =
    [
        new(1, 0, "초보 장인"),
        new(2, 50, "강화 비용 5% 할인"),
        new(3, 150, "연출 스킵 옵션"),
        new(4, 400, "강화 비용 10% 할인"),
        new(5, 800, "용광로 추가"),
        new(6, 1500, "강화 비용 15% 할인"),
        new(7, 3000, "파편 획득량 +1"),
        new(8, 5000, "강화 비용 20% 할인"),
        new(9, 8000, "전설의 공방"),
        new(10, 15000, "마스터 스미스"),
    ];

    public static int GetLevel(int exp)
    {
        int level = 1;
        for (int i = Levels.Length - 1; i >= 0; i--)
        {
            if (exp >= Levels[i].RequiredExp)
            {
                level = Levels[i].Level;
                break;
            }
        }
        return level;
    }

    public static string GetTitle(int level) => level switch
    {
        >= 10 => "마스터 스미스",
        >= 7 => "숙련 장인",
        >= 4 => "중급 장인",
        _ => "초보 장인"
    };

    public static (int current, int next) GetExpRange(int exp)
    {
        int currentThreshold = 0;
        int nextThreshold = Levels[1].RequiredExp;

        for (int i = Levels.Length - 1; i >= 0; i--)
        {
            if (exp >= Levels[i].RequiredExp)
            {
                currentThreshold = Levels[i].RequiredExp;
                nextThreshold = i + 1 < Levels.Length ? Levels[i + 1].RequiredExp : Levels[i].RequiredExp;
                break;
            }
        }

        return (currentThreshold, nextThreshold);
    }
}
