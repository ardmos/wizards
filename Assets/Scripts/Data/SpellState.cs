public enum SpellState
{
    // 기본 상태. 쿨타임도 끝나서 언제든 스킬 시전 가능한 상태
    Ready,
    // 스킬 시전이 시작되어 캐스팅중인 상태
    Casting,
    // 스킬이 발사되어 쿨타임중인 상태
    Cooltime
}