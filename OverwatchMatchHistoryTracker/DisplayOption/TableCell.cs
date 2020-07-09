namespace OverwatchMatchHistoryTracker.DisplayOption
{
    public struct TableCell
    {
        public string Data { get; set; }
        public int DisplayWidth { get; }

        public TableCell(string data, int displayWidth) => (Data, DisplayWidth) = (data, displayWidth);
    }
}
