public enum StatOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

public class StatModifier
{
    public string Id { get; }
    public StatOperation Operation { get; }
    public float Value { get; }
    public int Stack { get; private set; }

    public StatModifier(string id, StatOperation operation, float value, int initialStack = 1)
    {
        Id = id;
        Operation = operation;
        Value = value;
        Stack = initialStack;
    }

    public void AddStack(int amount = 1) => Stack += amount;
}