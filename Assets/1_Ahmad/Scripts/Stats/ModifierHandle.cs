public readonly struct ModifierHandle
{
    public readonly int Id;

    public ModifierHandle(int id) => Id = id;

    public bool IsValid => Id != 0;
}

