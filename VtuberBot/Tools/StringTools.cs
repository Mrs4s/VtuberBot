using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VtuberBot.Tools
{
    public static class StringTools
    {
        public static double ChineseRatio(this string @this)
        {
            var chineseCharsCount = @this.ToCharArray().Count(v => v >= 0x4E00 && v <= 0x9FA5);
            return chineseCharsCount * 1.0 / @this.ToCharArray().Length * 100;
        }

        public static bool IsHinaganaOrKatakana(this char @this) =>
            (@this >= 0x3040 && @this <= 0x309F) || (@this >= 0X30A0 && @this <= 0x30FF);

        public static bool IsSimplifiedChinese(this char @this)
        {
            var bytes = Encoding.GetEncoding("gb2312").GetBytes(@this.ToString());
            return bytes[0] >=0xB0 && bytes[1] <=0xF7 && bytes[1]>=0xA1 && bytes[1]<=0xFE;
        }
    }

}
