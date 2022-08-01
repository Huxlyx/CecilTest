using System.Reflection;

namespace BaseProject
{
    public class Class1
    {
        private readonly string someMember;
        private readonly string anotherMember;
        public string SomeMember { get { return someMember; } }
        public string AnotherMember { get { return anotherMember; } }

        public Class1(string someMember)
        {
            Console.WriteLine("Ctor of replacedMethod " + Assembly.GetAssembly(this.GetType()));
            this.someMember = someMember;
            anotherMember = someMember.ToLower() + someMember.ToUpper();
        }

        public void DoSomething()
        {
            Console.WriteLine(someMember);
            Console.WriteLine(anotherMember);
        }
    }
}