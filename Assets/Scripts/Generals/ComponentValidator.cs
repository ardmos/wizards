public static class ComponentValidator
{
    /// <summary>
    /// component�� ��������� errorMessage�� ����ϰ� false�� ��ȯ�մϴ�
    /// </summary>
    /// <returns>��� ��ȯ</returns>
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
