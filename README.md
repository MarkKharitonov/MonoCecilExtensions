# MonoCecilExtensions
## GetDeclaredTypeOfThisObject
Given an instance method call instruction this extension method return the declared type of the `this` object.

For example, consider the following code:
```csharp
public interface IBase
{
    void f();
}

public interface IDerived : IBase
{
}

public class Derived : IDerived
{
    public void f(){}
}

...

void SomeFunction()
{
    IDerived obj = new Derived();
    obj.f();
}
```
Inspecting the function `SomeFunction` reveals that it depends on the method `IBase.f`, however, what does it mean in reality? `IBase` may have gazillion implementations. So the statement "`SomeFunction` depends on `IBase.f`" is not very helpful, if we want to figure out the dependencies of `SomeFunction`.

The crucial piece of dependency information here is that `IBase.f` is being called through a reference to the `Derived` object. However, I do not think it is possible in general to deduce it from the static code analysis. But, we can deduce the **declared** type of the object through which `IBase.f` is being called, i.e. the declared type of `obj` - `IDerived`.

This extension method does exactly that.

For details on how it works refer to the eponymous unit tests.

### Generics
Right now the method depends on an external interface `IGenericContext` to resolve generic types in certain situations.

For example:
```
class SomeClass<TIntf, TImpl>
    where TIntf: IBase
    where TImpl: class, TIntf, new()
{
    void SomeFunction()
    {
        TIntf obj = new TImpl();
        obj.f();
    }
}
```
It seems impossible to deduce that `TIntf` is `IDerived` when `SomeClass<IDerived, Derived>` is used in code. This is because the `MethodDefinition` object corresponding to `SomeClass<IDerived, Derived>.SomeFunction` does not capture this information (as expected, since the single definition serves many generic instances). It seems the most we can obtain by inspecting the code of `SomeFunction` is that the declared type of `obj` is `TIntf` and to resolve it further we need some external help - enter the `IGenericContext` interface.

### Credits
 - [This SO answer by Vagaus](https://stackoverflow.com/a/68474401/80002)
 - Referenced by the aforementioned SO answer - https://github.com/lytico/db4o/blob/master/db4o.net/Db4oTool/Db4oTool/Core/StackAnalyzer.cs