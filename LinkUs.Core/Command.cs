namespace LinkUs.Core
{
    public class Command
    {
        public string Name { get; set; }

        public Command() { }
        public Command(string name)
        {
            Name = name;
        }
    }
}