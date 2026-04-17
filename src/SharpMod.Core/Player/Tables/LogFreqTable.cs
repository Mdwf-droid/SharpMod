using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

partial class SharpModPlayer
{
    protected internal const int LOGFAC = 2 * 16;

    protected internal static readonly short[] LogFreqTable = [
        (short) (LOGFAC * 907), (short) (LOGFAC * 900), (short) (LOGFAC * 894),
        (short) (LOGFAC * 887), (short) (LOGFAC * 881), (short) (LOGFAC * 875),
        (short) (LOGFAC * 868), (short) (LOGFAC * 862), (short) (LOGFAC * 856),
        (short) (LOGFAC * 850), (short) (LOGFAC * 844), (short) (LOGFAC * 838),
        (short) (LOGFAC * 832), (short) (LOGFAC * 826), (short) (LOGFAC * 820),
        (short) (LOGFAC * 814), (short) (LOGFAC * 808), (short) (LOGFAC * 802),
        (short) (LOGFAC * 796), (short) (LOGFAC * 791), (short) (LOGFAC * 785),
        (short) (LOGFAC * 779), (short) (LOGFAC * 774), (short) (LOGFAC * 768),
        (short) (LOGFAC * 762), (short) (LOGFAC * 757), (short) (LOGFAC * 752),
        (short) (LOGFAC * 746), (short) (LOGFAC * 741), (short) (LOGFAC * 736),
        (short) (LOGFAC * 730), (short) (LOGFAC * 725), (short) (LOGFAC * 720),
        (short) (LOGFAC * 715), (short) (LOGFAC * 709), (short) (LOGFAC * 704),
        (short) (LOGFAC * 699), (short) (LOGFAC * 694), (short) (LOGFAC * 689),
        (short) (LOGFAC * 684), (short) (LOGFAC * 678), (short) (LOGFAC * 675),
        (short) (LOGFAC * 670), (short) (LOGFAC * 665), (short) (LOGFAC * 660),
        (short) (LOGFAC * 655), (short) (LOGFAC * 651), (short) (LOGFAC * 646),
        (short) (LOGFAC * 640), (short) (LOGFAC * 636), (short) (LOGFAC * 632),
        (short) (LOGFAC * 628), (short) (LOGFAC * 623), (short) (LOGFAC * 619),
        (short) (LOGFAC * 614), (short) (LOGFAC * 610), (short) (LOGFAC * 604),
        (short) (LOGFAC * 601), (short) (LOGFAC * 597), (short) (LOGFAC * 592),
        (short) (LOGFAC * 588), (short) (LOGFAC * 584), (short) (LOGFAC * 580),
        (short) (LOGFAC * 575), (short) (LOGFAC * 570), (short) (LOGFAC * 567),
        (short) (LOGFAC * 563), (short) (LOGFAC * 559), (short) (LOGFAC * 555),
        (short) (LOGFAC * 551), (short) (LOGFAC * 547), (short) (LOGFAC * 543),
        (short) (LOGFAC * 538), (short) (LOGFAC * 535), (short) (LOGFAC * 532),
        (short) (LOGFAC * 528), (short) (LOGFAC * 524), (short) (LOGFAC * 520),
        (short) (LOGFAC * 516), (short) (LOGFAC * 513), (short) (LOGFAC * 508),
        (short) (LOGFAC * 505), (short) (LOGFAC * 502), (short) (LOGFAC * 498),
        (short) (LOGFAC * 494), (short) (LOGFAC * 491), (short) (LOGFAC * 487),
        (short) (LOGFAC * 484), (short) (LOGFAC * 480), (short) (LOGFAC * 477),
        (short) (LOGFAC * 474), (short) (LOGFAC * 470), (short) (LOGFAC * 467),
        (short) (LOGFAC * 463), (short) (LOGFAC * 460), (short) (LOGFAC * 457),
        (short) (LOGFAC * 453), (short) (LOGFAC * 450), (short) (LOGFAC * 447),
        (short) (LOGFAC * 443), (short) (LOGFAC * 440), (short) (LOGFAC * 437),
        (short) (LOGFAC * 434), (short) (LOGFAC * 431)];
}

