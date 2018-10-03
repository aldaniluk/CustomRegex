using CustomRegex;
using System.IO;
using System.Linq;
using SC = System.Console;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\re_and_strings.txt");
            foreach (string line in lines)
            {
                string[] reAndStrings = line.Split(' ');
                string re = reAndStrings[0];
                var bsuRegex = new BsuRegex(re);
                string[] strings = reAndStrings.Skip(1).ToArray();

                SC.Write($"{re}: ");
                foreach (string str in strings)
                {
                    bool isMatch = bsuRegex.IsMatch(str);
                    SC.Write($"{str}-{isMatch} ");
                }
                SC.WriteLine();
            }
        }
    }
}
