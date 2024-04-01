namespace Aniflex.Core;

public static class StringHash
{
    public static ulong ToHash(string s)
    {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < s.Length; i++)
        {
            hashedValue += s[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}
