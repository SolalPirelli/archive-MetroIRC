using IrcSharp;
using IrcSharp.External;
using MetroIrc.Services;

namespace MetroIrc.Desktop.Services
{
    public sealed class WpfTcpService : ITcpService
    {
        public TcpWrapper GetWrapper( IrcNetworkInfo info )
        {
            return new SocketWrapper( info.HostName, info.PortNumber, info.UseSsl, info.AcceptInvalidCertificates );
        }

        public TcpListenerWrapper GetListenerWrapper()
        {
            return new SocketListenerWrapper();
        }
    }
}