namespace ClipboardAgent
{
    public class ClipItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string CreatedAt { get; set; }

        // Texto de una sola línea para el flyout.
        public string Preview
        {
            get
            {
                var t = (Text ?? "").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();
                while (t.Contains("  ")) t = t.Replace("  ", " ");
                return t.Length > 90 ? t.Substring(0, 90) + "…" : t;
            }
        }
    }
}
