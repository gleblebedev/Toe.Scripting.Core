namespace Toe.Scripting
{
    public abstract class Link
    {
        public abstract NodeAndPin From { get; }
        public abstract NodeAndPin To { get; }

        public virtual int FromNodeId => From.Node.Id;

        public virtual string FromPinId => From.Pin.Id;

        public virtual int ToNodeId => To.Node.Id;
        public virtual string ToPinId => To.Pin.Id;
    }
}