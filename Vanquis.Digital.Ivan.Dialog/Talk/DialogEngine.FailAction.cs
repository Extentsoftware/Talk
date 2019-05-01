using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.Model;

namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static partial class DialogEngine
    {
        /// <summary>
        /// failure
        /// </summary>
        public class FailAction : TalkAction
        {
            public string Reason { get; set; }
            public List<CollectPropertyMatch> Rejections { get; set; }
        }
    }

}
