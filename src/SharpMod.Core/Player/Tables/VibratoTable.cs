using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

partial class SharpModPlayer
{
    protected internal short[] VibratoTable = [
       0, 24, 49, 74, 97, 120, 141, 161,
        180, 197, 212, 224, 235, 244, 250, 253,
        255, 253, 250, 244, 235, 224, 212, 197,
        180, 161, 141, 120, 97, 74, 49, 24 ];
}

