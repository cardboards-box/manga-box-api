namespace MangaBox.Models;

[AttributeUsage(AttributeTargets.Class)]
public class TypeAttribute : Attribute
{
    public string Name { get; set; }

    public TypeAttribute(string name)
    {
        Name = name;
    }
}
