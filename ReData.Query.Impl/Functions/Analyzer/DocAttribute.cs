using System.Runtime.CompilerServices;

namespace ReData.Query.Impl.Functions;

public class DocAttribute(string text) : Attribute
{
    public string Text => text;
}

public class MethodAttribute() : Attribute
{
}

    
    
