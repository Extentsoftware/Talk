using System;
using System.Collections.Generic;

namespace Vanquis.Digital.Ivan.Dialog.EntityExtractor
{
    internal class TokenNode
    {
        public TokenNode()
        {
            Children = new List<TokenNode>();
        }

        internal Token Token;

        /// <summary>
        /// child tokens
        /// </summary>
        internal List<TokenNode> Children;

        internal void PrintTree(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)} {Token.ToString()}");

            indent += 2;

            foreach (var c in Children)
                c.PrintTree(indent);
        }

        /// <summary>
        /// recursive routing that creates flattened lists of token lists from a tree structure
        /// </summary>
        /// <param name="master"></param>
        /// <param name="list"></param>
        internal void Flatten(List<List<TokenNode>> master, List<TokenNode> list)
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
