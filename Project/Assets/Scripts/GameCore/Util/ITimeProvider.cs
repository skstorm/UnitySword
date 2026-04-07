using System;

namespace GameCore.Util;

/// <summary>현재 시각 제공 인터페이스. 테스트 시 고정 시각으로 대체 가능.</summary>
public interface ITimeProvider
{
    DateTime Now();
}
