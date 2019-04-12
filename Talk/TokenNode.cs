using System;
using System.Collections.Generic;

namespace Talk
{
    public class TokenNode
    {
        public TokenNode()
        {
            Children = new List<TokenNode>();
        }

        public Token Token;

        /// <summary>
        /// child tokens
        /// </summary>
        public List<TokenNode> Children;

        public void PrintTree(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)} {Token.ToString()}");

            indent += 2;

            foreach (var c in Children)
                c.PrintTree(indent);
        }

        public void Flatten(List<List<TokenNode>> master, List<TokenNode> list)
        {
            List<TokenNode> newlist = new List<TokenNode>(list)
            {
                this
            };
            foreach (var c in Children)
                c.Flatten(master, newlist);

            if (Children.Count == 0)
                master.Add(newlist);
        }

        public override string ToString()
        {
            return $"{Token}";
        }
    }

}
