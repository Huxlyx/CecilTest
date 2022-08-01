namespace BaseProject
{
    public class Class1
    {
        private readonly string someMember;
        public string MemberNotGettingReplaced { get; set; }
        public string SomeMember { get { return someMember; } }

        public Class1(string someMember)
        {
            this.someMember = someMember;
            MemberNotGettingReplaced = "lol";
        }

        public void DoSomething()
        {
            Console.WriteLine(someMember.ToLower());
        }
    }
}