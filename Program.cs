using System;


namespace Language
{
    class Program {
        static void Main(string[] args) {
            Parser parser = new Parser("f:\\Text.code");
            Node node = parser.parse();
            Console.WriteLine(node.toSyntaxGraph());
        }
    }
}
