using IrcSharp;

namespace MetroIrc
{
    internal sealed class CommandMessage
    {
        public string Text { get; private set; }
        public IrcChannel Channel { get; private set; }
        public IrcUser User { get; private set; }
        public string TargetName { get; private set; }

        public CommandMessage( string text, IrcChannel channel, IrcUser user, string targetName )
        {
            this.Text = text;
            this.Channel = channel;
            this.User = user;
            this.TargetName = targetName;
        }
    }
}