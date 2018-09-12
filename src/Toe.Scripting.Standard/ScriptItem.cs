using System;

namespace Toe.Scripting
{
    public class ScriptItem
    {
        public const int InvalidId = Collection<ScriptItem>.InvalidId;

        private int _id = InvalidId;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != InvalidId)
                    throw new InvalidOperationException("Id is already assigned.");
                _id = value;
            }
        }
    }
}