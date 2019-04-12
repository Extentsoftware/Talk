using System.Collections.Generic;
using System.Linq;

namespace Talk
{
    public static class TokenTreeExtensions
    {
        public static TokenTree PrintTree(this TokenTree tree)
        {
            tree.Head.PrintTree(0);
            return tree;
        }

        public static List<List<TokenNode>> Flatten(this TokenTree tree)
        {
            List<List<TokenNode>> master = new List<List<TokenNode>>();
            List<TokenNode> list = new List<TokenNode>();
            tree.Head.Flatten(master,list);
            return master;
        }

        static public TokenTree Build(this TokenTree tree, IEnumerable<IEntityTokeniser> tokenisers, string text, Dictionary<string, object> properties)
        {
            foreach (var tokeniser in tokenisers)
                tokeniser.BeginParse(text, properties);

            TokenNode current = new TokenNode
            {
                Token = new StartToken { Pos = 0, Length = 0 }
            };

            tree.Head = current;
            int currentOffset = 0;

            ParseText(tokenisers, tree, currentOffset, current, text, properties);

            return tree;
        }

        /// <summary>
        ///  tokenise the text into a token tree
        /// </summary>
        /// <param name="text"></param>
        /// <param name="recognisers"></param>
        /// <returns></returns>
        static private void ParseText(IEnumerable<IEntityTokeniser> tokenisers, TokenTree tree, int currentOffset, TokenNode current, string text, Dictionary<string, object> Properties)
        {
            var tokens = GetFirstTokenList(tokenisers, text.Substring(currentOffset), Properties);

            // end of parse.. add trailing text if any
            if (tokens == null || tokens.Count == 0)
            {
                var subtext = text.Substring(currentOffset).Trim();                

                if (subtext.Length > 0)
                {
                    TextToken textToken = new TextToken
                    {
                        Text = subtext,
                        Pos = currentOffset,
                        Length = subtext.Length
                    };

                    TokenNode node = new TokenNode { Token = textToken };
                    current.Children.Add(node);
                }
                return;
            }

            // calc start pos of the tokens (all will have same start pos)
            var tokenpos = currentOffset + tokens[0].Pos;


            // *** create infill *** 
            // tokens are offset from start of text so create infilling text token
            if (tokenpos > currentOffset)
            {
                var infill_txt = text.Substring(currentOffset, tokenpos - currentOffset);
                // only add infill if it contains more than just white space
                if (infill_txt.Trim().Length > 0)
                {
                    TextToken textToken = new TextToken
                    {
                        Text = infill_txt,
                        Pos = currentOffset,
                        Length = tokenpos - currentOffset - 1
                    };

                    TokenNode infill_node = new TokenNode { Token = textToken };
                    current.Children.Add(infill_node);
                    current = infill_node;
                }
            }

            // we have tokens to deal with
            // for each child, find more tokens
            foreach (var token in tokens)
            {
                var thiscurrent = current;

                // update the current offset to work from
                var nextoffset = currentOffset + (token.Pos + token.Length);

                TokenNode node = new TokenNode { Token = token };
                thiscurrent.Children.Add(node);

                ParseText(tokenisers, tree, nextoffset, node, text, Properties);
            }
        }

        /// <summary>
        /// get first matching tokens by position in the text from each tokeniser.
        /// Might get more than one token matching at the same position
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static List<Token> GetFirstTokenList(IEnumerable<IEntityTokeniser> tokenisers, string textfragment, Dictionary<string, object> properties)
        {
            List<Token> tokens = new List<Token>();

            // collect all tokens from the tokenisers
            foreach (var tokeniser in tokenisers)
            {
                var t = tokeniser.GetTokens(textfragment, properties);
                if (t != null)
                    tokens.AddRange(t);
            }

            // sort by position
            tokens = tokens.OrderBy(x => x.Pos).ToList();

            // only select tokens nearest the front of the text at the same position
            var first_group = tokens.GroupBy(x => x.Pos).OrderBy(x => x.Key).FirstOrDefault();
            if (first_group != null)
            {                
                var grp = first_group.Select(x => x).ToList();
                return grp;
            }
            return null;
        }
    }

}
