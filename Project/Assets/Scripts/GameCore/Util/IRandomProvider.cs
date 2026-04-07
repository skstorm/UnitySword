namespace GameCore.Util;

/// <summary>난수 제공 인터페이스. 테스트 시 결정적 값으로 대체 가능.</summary>
public interface IRandomProvider
{
    double NextDouble();
}
