using System;
using System.Threading.Tasks;

namespace BinaryDataDecoders.Serial.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialRadexOne.Execute();
        }

        public static Task<string> ReadLineAsync()
        {
            return Task.FromResult(Console.ReadLine());
        }
    }
}
