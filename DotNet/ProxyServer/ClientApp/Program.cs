using System;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {     
        public static async Task<int> Main(string[] args)
        {
            string server = "127.0.0.1";

            await SslTcpClient.RunClient(server, args.Length == 0 ? 17777 : int.Parse(args[0]));

            return 0;
        }
    }
}
