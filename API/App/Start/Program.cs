using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

namespace API.App.Start
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            #region Objects
            WebApplication App;
            #endregion

            App = await Startup.InitAppAsync(args: args);

            App.Run();
        }
    }
}