
namespace Cell.Model.Plugin
{
    [Serializable]
    public class TodoItem : PluginModel
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsComplete { get; set; } = false;

        public DateTime DueDate { get; set; } = DateTime.Now;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public int Priority { get; set; } = 0;

    }
}
