using Mono.Cecil;

namespace CecilTest
{
    public class HackyResolver : IAssemblyResolver
    {

        private readonly AssemblyDefinition assemblyDef;

        public HackyResolver(AssemblyDefinition assemblyDef)
        {
            this.assemblyDef = assemblyDef;
        }   

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return assemblyDef;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return assemblyDef;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
