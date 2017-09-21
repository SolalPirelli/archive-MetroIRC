using IrcSharp;

namespace MetroIrc.Services
{
    public interface ITcpService
    {
        TcpWrapper GetWrapper( IrcNetworkInfo info );
        TcpListenerWrapper GetListenerWrapper();
    }
}