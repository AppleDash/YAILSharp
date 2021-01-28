using System;

namespace YAILSharp.Event
{
    public class LineEventArgs : EventArgs
    {
        public Line Line { get; private set; }

        public LineEventArgs(Line line)
        {
            this.Line = line;
        }
    }
}