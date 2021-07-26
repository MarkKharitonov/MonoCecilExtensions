using Mono.Cecil;

namespace MonoCecilExtensions
{
    public interface IGenericContext
    {
        TypeReference Resolve(GenericParameter gp);
    }
}
