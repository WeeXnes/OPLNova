using System;

namespace OPLNova.Core;

public static class Artwork
{
    public enum Type
    {
        Front,
        Back,
        Disc
    }

    public static Type NextType(Type _type)
    {
        return (Type)(((int)_type + 1) % Enum.GetValues(typeof(Type)).Length);
    }

    public static  Type PrevType(Type _type)
    {
        return (Type)(((int)_type - 1 + Enum.GetValues(typeof(Type)).Length) % Enum.GetValues(typeof(Type)).Length);
    }
}