namespace BaseProject
{
    public class Class2
    {

        public Class2()
        {

        }


        public void RunTest()
        {
            int someRandomInteger = 1;
            Class1 class1 = new("Running Test");
            ++someRandomInteger;
            class1.DoSomething();
            Console.WriteLine(someRandomInteger);
        }
    }
}
