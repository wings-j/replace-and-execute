namespace replace_and_execute.Types
{
    /// <summary>
    /// Module
    /// </summary>
    public class Module
    {
        public String Name { get; set; } = "";
        public String Path { get; set; } = "";
        public String[] Pre { get; set; } = new String[0];
        public String[] Post { get; set; } = new String[0];

    }
}