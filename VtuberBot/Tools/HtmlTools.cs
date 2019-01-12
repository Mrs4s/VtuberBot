using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace VtuberBot.Tools
{
    public static class HtmlTools
    {
        public static string GetAttributeValue(this HtmlNode node, string attName) =>
            node.Attributes.FirstOrDefault(att => att.Name == attName)?.Value;

      
    }
}
