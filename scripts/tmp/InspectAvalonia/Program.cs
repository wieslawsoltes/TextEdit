using System;
using System.Reflection;
using Avalonia.Input;

static void DumpMembers(Type type)
{
    Console.WriteLine($"Type: {type.FullName}");
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
    {
        if (method.IsSpecialName) continue;
        Console.WriteLine($"  {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
    }
    Console.WriteLine();
}

DumpMembers(typeof(IPointer));
