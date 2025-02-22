using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KekikPlayer.Core.Extensions
{
    public static class BytesExtension
    {
        public enum SizeUnits
        {
            B, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ToSize(this Int64 value, SizeUnits unit)
        {
            string ex = " KB";
            return (value / (double)Math.Pow(1024, (Int64)unit)).ToString("0.00") + ex;
        }

        // usage: string h = x.ToSize(MyExtension.SizeUnits.KB);
    }
}
