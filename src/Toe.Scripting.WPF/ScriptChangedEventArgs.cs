using System;

namespace Toe.Scripting.WPF
{
    public class ScriptChangedEventArgs : EventArgs
    {
        public ScriptChangedEventArgs(Script script)
        {
            Script = script;
        }

        public Script Script { get; }
    }
}