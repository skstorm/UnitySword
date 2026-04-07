using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Models;

public class Statistics
{
    public int HighestEnhance { get; }
    public int TotalDestroys { get; }
    public int TotalEnhances { get; }
    public int MaxConsecutiveSuccess { get; }
    public int MaxConsecutiveFail { get; }
    public int CurrentConsecutiveSuccess { get; }
    public int CurrentConsecutiveFail { get; }

    public Statistics(
        int highestEnhance = 0,
        int totalDestroys = 0,
        int totalEnhances = 0,
        int maxConsecutiveSuccess = 0,
        int maxConsecutiveFail = 0,
        int currentConsecutiveSuccess = 0,
        int currentConsecutiveFail = 0)
    {
        HighestEnhance = highestEnhance;
        TotalDestroys = totalDestroys;
        TotalEnhances = totalEnhances;
        MaxConsecutiveSuccess = maxConsecutiveSuccess;
        MaxConsecutiveFail = maxConsecutiveFail;
        CurrentConsecutiveSuccess = currentConsecutiveSuccess;
        CurrentConsecutiveFail = currentConsecutiveFail;
    }

    public Statistics With(
        int? highestEnhance = null,
        int? totalDestroys = null,
        int? totalEnhances = null,
        int? maxConsecutiveSuccess = null,
        int? maxConsecutiveFail = null,
        int? currentConsecutiveSuccess = null,
        int? currentConsecutiveFail = null)
    {
        return new Statistics(
            highestEnhance ?? HighestEnhance,
            totalDestroys ?? TotalDestroys,
            totalEnhances ?? TotalEnhances,
            maxConsecutiveSuccess ?? MaxConsecutiveSuccess,
            maxConsecutiveFail ?? MaxConsecutiveFail,
            currentConsecutiveSuccess ?? CurrentConsecutiveSuccess,
            currentConsecutiveFail ?? CurrentConsecutiveFail);
    }
}

public class AdLimits
{
    public int AdProtectionUsedToday { get; }
    public DateTime LastAdResetDate { get; }

    public AdLimits(int adProtectionUsedToday = 0, DateTime lastAdResetDate = default)
    {
        AdProtectionUsedToday = adProtectionUsedToday;
        LastAdResetDate = lastAdResetDate;
    }

    public bool CanUseAdProtection() => AdProtectionUsedToday < 2;

    public AdLimits With(
        int? adProtectionUsedToday = null,
        DateTime? lastAdResetDate = null)
    {
        return new AdLimits(
            adProtectionUsedToday ?? AdProtectionUsedToday,
            lastAdResetDate ?? LastAdResetDate);
    }
}

public class Inventory
{
    public int ProtectionAmulets { get; }
    public int BlessingScrolls { get; }
    public int GoldPouches { get; }

    public Inventory(int protectionAmulets = 0, int blessingScrolls = 0, int goldPouches = 0)
    {
        ProtectionAmulets = protectionAmulets;
        BlessingScrolls = blessingScrolls;
        GoldPouches = goldPouches;
    }

    public Inventory With(
        int? protectionAmulets = null,
        int? blessingScrolls = null,
        int? goldPouches = null)
    {
        return new Inventory(
            protectionAmulets ?? ProtectionAmulets,
            blessingScrolls ?? BlessingScrolls,
            goldPouches ?? GoldPouches);
    }
}

public class PlayerData
{
    public int Gold { get; }
    public Statistics Stats { get; }
    public int Fragments { get; }
    public Inventory Items { get; }
    public AdLimits AdLimits { get; }
    public bool IsFirstRun { get; }
    public DateTime LastSyncedAt { get; }
    public List<int> CollectedSwords { get; }
    public int MasteryExp { get; }

    public PlayerData(
        int gold = 0,
        Statistics stats = null,
        int fragments = 0,
        Inventory items = null,
        AdLimits adLimits = null,
        bool isFirstRun = true,
        DateTime lastSyncedAt = default,
        List<int> collectedSwords = null,
        int masteryExp = 0)
    {
        Gold = gold;
        Stats = stats ?? new Statistics();
        Fragments = fragments;
        Items = items ?? new Inventory();
        AdLimits = adLimits ?? new AdLimits();
        IsFirstRun = isFirstRun;
        LastSyncedAt = lastSyncedAt;
        CollectedSwords = collectedSwords ?? new List<int>();
        MasteryExp = masteryExp;
    }

    public static PlayerData CreateDefault()
    {
        return new PlayerData(
            gold: 0,
            stats: new Statistics(),
            fragments: 0,
            items: new Inventory(),
            adLimits: new AdLimits(),
            isFirstRun: true,
            lastSyncedAt: default,
            collectedSwords: new List<int>(),
            masteryExp: 0);
    }

    public PlayerData With(
        int? gold = null,
        Statistics stats = null,
        int? fragments = null,
        Inventory items = null,
        AdLimits adLimits = null,
        bool? isFirstRun = null,
        DateTime? lastSyncedAt = null,
        List<int> collectedSwords = null,
        int? masteryExp = null)
    {
        return new PlayerData(
            gold ?? Gold,
            stats ?? Stats,
            fragments ?? Fragments,
            items ?? Items,
            adLimits ?? AdLimits,
            isFirstRun ?? IsFirstRun,
            lastSyncedAt ?? LastSyncedAt,
            collectedSwords ?? CollectedSwords,
            masteryExp ?? MasteryExp);
    }
}
