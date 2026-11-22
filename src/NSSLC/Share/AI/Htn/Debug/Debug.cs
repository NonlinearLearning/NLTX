// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;
using Terraria.Ai.Htn.Conditions;

namespace Terraria.Ai.Htn.Debug
{
    public static class Debug
    {
        public static string DepthToString(int depth)
        {
            string s = "";
            for (var i = 0; i < depth; i++)
            {
                s += "\t";
            }

            s += "- ";
            return s;
        }
    }
}
