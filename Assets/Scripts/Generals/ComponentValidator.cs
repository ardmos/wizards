public static class ComponentValidator
{
    /// <summary>
    /// component가 비어있으면 errorMessage를 출력하고 false를 반환합니다
    /// </summary>
    /// <returns>결과 반환</returns>
    public static bool ValidateComponent<T>(T component, string errorMessage)
    {
        if (component == null)
        {
            Logger.LogError(errorMessage);
            return false;
        }
        return true;
    }
}
