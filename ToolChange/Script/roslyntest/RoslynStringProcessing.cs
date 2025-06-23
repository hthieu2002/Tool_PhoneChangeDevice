using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolChange.Script.RoslynTest
{
    public class RoslynStringProcessing
    {
        public (int, int) getParseClickCoordinates(string input)
        {
            return ParseClickCoordinates(input);
        }
        public (int, int, int, int, int) getParseSwipeCoordinates(string input)
        {
            return ParseSwipeCoordinates(input);
        }
        public (int, int, int, int ) getParseRandomClickCoordinates(string input)
        {
            return ParseRandomClickCoordinates(input);
        }
        private (int, int) ParseClickCoordinates(string input)
        {
            var regex = new Regex(@"ClickXY\((\d+)\s+(\d+)\)");
            var match = regex.Match(input);

            if (match.Success)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                return (x, y);
            }
            else
            {
                throw new FormatException("Chuỗi đầu vào không đúng định dạng.");
            }
        }
        private (int, int, int, int, int) ParseSwipeCoordinates(string input)
        {
            var regex = new Regex(@"Swipe\((\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\)");
            var match = regex.Match(input);

            if (match.Success)
            {
                int x1 = int.Parse(match.Groups[1].Value);
                int y1 = int.Parse(match.Groups[2].Value);
                int x2 = int.Parse(match.Groups[3].Value);
                int y2 = int.Parse(match.Groups[4].Value);
                int duration = int.Parse(match.Groups[5].Value);

                Console.WriteLine($"Parsed values: x1={x1}, y1={y1}, x2={x2}, y2={y2}, duration={duration}");

                return (x1, y1, x2, y2, duration);
            }
            else
            {
                throw new FormatException("Chuỗi đầu vào không đúng định dạng.");
            }
        }
        private (int , int , int , int ) ParseRandomClickCoordinates(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\d+");
            if (matches.Count < 4)
                throw new ArgumentException("Chuỗi không đủ 4 số.");

            Console.WriteLine($"{matches}");
            return (
                int.Parse(matches[0].Value),
                int.Parse(matches[1].Value),
                int.Parse(matches[2].Value),
                int.Parse(matches[3].Value)
            );
        }
    }
}
