using SharpMod.Song;
using SharpMod.UniTracker;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Player;

public partial class SharpModPlayer
{

    public virtual void MP_HandleTick()
    {
        int tmpvol;
        // extern char current_file[1024];
        //int z, t, tr;
        int t;
        ActionsEnum ui_result;
        //extern int play_current;
        // extern int current_pattern;
        //extern int count_pattern, count_song;
        bool reinit_audio = false;

        PauseFlag = -128;
        if (isfirst != 0)
        {
            // don't handle the very first ticks, this allows the
            // other hardware to settle down so we don't lose any 
            // starting notes                
            isfirst--;
            return;
        }

        if (forbid)
            return;

        // don't go any further when forbid is true
        if (MP_Ready())
            return;

        if (++TickCounter >= mp_sngspd)
        {
            mp_patpos++;
            TickCounter = 0;

            if (PatternDelayCounter != 0)
            {
                SecondPatternDelayCounter = PatternDelayCounter;
                PatternDelayCounter = 0;
            }

            if (SecondPatternDelayCounter != 0)
            {
                // patterndelay active
                if ((--SecondPatternDelayCounter) != 0)
                {
                    // so turn back mp_patpos by 1
                    mp_patpos--;
                }
            }

            // Do we have to get a new patternpointer ?
            // (when mp_patpos reaches 64 or when
            // a patternbreak is active). Also check for 256 - if mod 
            // is broken it will continue forever otherwise 
            if (mp_patpos == numrow || mp_patpos > 255)
                posjmp = 3;

            if (posjmp != 0)
            {
                mp_patpos = PatternBreakPosition;
                mp_sngpos = (short)(mp_sngpos + (posjmp - 2));
                PatternBreakPosition = (short)(posjmp = 0);
                if (mp_sngpos >= CurrentUniMod.Positions.Count - 1/* .NumPos*/)
                {
                    /*				if(true) return;*/
                    if (!mp_loop)
                    {
                        OnCurrentModEnd?.Invoke();
                        return;
                    }
                    mp_sngpos = CurrentUniMod.RepPos;
                }
                if (mp_sngpos < 0)
                    mp_sngpos = (short)(CurrentUniMod.Positions.Count - 1);
            }


            if (SecondPatternDelayCounter == 0)
            {

                for (t = 0; t < CurrentUniMod.ChannelsCount; t++)
                {

                    //tr = CurrentUniMod.Patterns[(CurrentUniMod.Positions[mp_sngpos] * CurrentUniMod.NumChn) + t];
                    //Todo: Check for overflow at the end of the module

                    mp_channel = (short)t;
                    a = mp_audio[t];
                    if (CurrentUniMod.Positions[mp_sngpos] < CurrentUniMod.Patterns.Count)
                    {
                        numrow = CurrentUniMod.Patterns[CurrentUniMod.Positions[mp_sngpos]].RowsCount; //(short)(CurrentUniMod.PattRows[CurrentUniMod.Positions[mp_sngpos]]);
                        a.Row = CurrentUniMod.Patterns[CurrentUniMod.Positions[mp_sngpos]].Tracks[t].UniTrack;
                        a.RowPos = _uniTrack.UniFindRow(a.Row, mp_patpos);
                    }
                    else
                        a.Row = null;

                    //a.row = (tr<pf.numtrk) ? MikMod.MUniTrk.clMUniTrk.UniFindRow(pf.tracks[tr],mp_patpos) : ((short*)null);
                    /*if (tr < CurrentUniMod.NumTrk)
                    {
                        a.Row = CurrentUniMod.Tracks[tr];
                        a.RowPos = _uniTrack.UniFindRow(CurrentUniMod.Tracks[tr], mp_patpos);
                    }
                    else
                        a.Row = null;*/



                    PlayNote();
                }

                //run through once, repeat if paused
                do
                {
                    //don't need to eat cpu time!
                    if (PauseFlag == 127)
                        System.Threading.Thread.Sleep(1000);
                    //m_.usleep(1000);


                    //m_.UI.count_pattern++;
                    //m_.UI.count_song++;
                    //if (m_.quiet)
                    if (OnGetUIActions != null)
                        ui_result = OnGetUIActions();
                    else
                        ui_result = ActionsEnum.DEFAULT;
                    //* don't match any case */
                    //else
                    //    ui_result = m_.UI.get_ui();

                    /* volume=0 already by default if paused, so don't need to fiddle with it... */
                    switch (ui_result)
                    {
                        case ActionsEnum.UI_DELETE_MARKED:
                        /*if(!m_.cur_mod.deleted)
                        break;
                        if(!unlink(m_.cur_mod.filename))
                        m_.cur_mod.deleted=2;
                        else
                        m_.cur_mod.deleted=3;
                        m_.Display.update_file_display();
                        m_.Display.display_all(); */
                        /* FALL THROUGH */
                        case ActionsEnum.UI_NEXT_SONG:
                            //_driver.MD_PatternChange();
                            this.PlayCurrent = false;
                            break;

                        case ActionsEnum.UI_PREVIOUS_SONG:
                            //if ((m_.UI.count_song < MikMod.UI.myUI.SMALL_DELAY) && (m_.optind > 1))
                            //{
                            //    m_.optind -= 2;
                            //    this.play_current = false;
                            //}
                            //else
                            //{
                            //    mp_sngpos = 1;
                            //    MP_PrevPosition();
                            //}
                            //m_.UI.count_song = 0;
                            //_driver.MD_PatternChange();
                            break;

                        case ActionsEnum.UI_QUIT:
                            //_driver.MD_PatternChange();
                            Quit = true;
                            break;

                        case ActionsEnum.UI_JUMP_TO_NEXT_PATTERN:
                            // _driver.MD_PatternChange();
                            MP_NextPosition();
                            break;

                        case ActionsEnum.UI_JUMP_TO_PREV_PATTERN:
                            //_driver.MD_PatternChange();
                            //if (m_.UI.count_pattern < MikMod.UI.myUI.SMALL_DELAY)
                            ///* near start of pattern? */
                            //    MP_PrevPosition();
                            //else
                            //    MP_RestartPosition();
                            //m_.UI.count_pattern = 0;
                            break;

                        case ActionsEnum.UI_PAUSE:
                            PauseFlag = ~PauseFlag;
                            if (PauseFlag == 127)
                            {
                                //if (m_.md_type != 0)
                                //    _driver.MD_Mute();
                                //else
                                //_driver.MD_Exit();
                                /* temp. free the sound driver */
                                /*m_.Display.display_version();
                                m_.Display.display_pausebanner();*/
                            }
                            else
                            {
                                //if (m_.md_type != 0)
                                //{
                                //    _driver.MD_UnMute();
                                //    m_.Display.display_all();
                                //}
                                ///* need to re-init. the sound driver before leaving pause mode */
                                //else
                                //{
                                /*if (!_driver.MD_Init())
                                {                                       
                                    
                                    PauseFlag = ~PauseFlag;
                                }*/
                                /*else
                                    m_.Display.display_all();*/
                                //}
                            }
                            break;

                        case ActionsEnum.UI_SPEED_UP:
                            if ((old_bpm * (SpeedConstant + 0.05)) <= 255)
                                SpeedConstant = (float)(SpeedConstant + 0.05);
                            break;

                        case ActionsEnum.UI_SLOW_DOWN:
                            if ((old_bpm * (SpeedConstant - 0.05)) > 10)
                                SpeedConstant = (float)(SpeedConstant - 0.05);
                            break;

                        case ActionsEnum.UI_NORMAL_SPEED:
                            SpeedConstant = 1.0f;
                            break;

                        case ActionsEnum.UI_VOL_UP:
                            if (mp_volume < 250)
                                mp_volume = (short)(mp_volume + 5);
                            break;

                        case ActionsEnum.UI_VOL_DOWN:
                            if (mp_volume > 5)
                                mp_volume = (short)(mp_volume - 5);
                            break;

                        case ActionsEnum.UI_NORMAL_VOL:
                            mp_volume = 100;
                            break;

                        case ActionsEnum.UI_MARK_DELETED:
                            /*if (!m_.cur_mod.Deleted)
                                m_.cur_mod.Deleted = true;
                            else if (m_.cur_mod.Deleted == true)
                                m_.cur_mod.Deleted = false;
                            m_.Display.update_file_display();
                            m_.Display.display_all();*/
                            break;

                        case ActionsEnum.UI_SELECT_STEREO:
                            //_driver.md_mode |= DMode.DMODE_STEREO;
                            reinit_audio = true;
                            break;

                        case ActionsEnum.UI_SELECT_MONO:
                            //_driver.md_mode &= ~DMode.DMODE_STEREO;
                            reinit_audio = true;
                            break;

                        case ActionsEnum.UI_SELECT_INTERP:
                            //_driver.md_mode |= DMode.DMODE_INTERP;
                            reinit_audio = true;
                            break;

                        case ActionsEnum.UI_SELECT_NONINTERP:
                            //_driver.md_mode &= ~DMode.DMODE_INTERP;
                            reinit_audio = true;
                            break;

                        case ActionsEnum.UI_SELECT_8BIT:
                            //_driver.md_mode &= ~DMode.DMODE_16BITS;
                            reinit_audio = true;
                            break;

                        case ActionsEnum.UI_SELECT_16BIT:
                            //_driver.md_mode |= DMode.DMODE_16BITS;
                            reinit_audio = true;
                            break;

                        default:
                            break;

                    }
                    if ((old_bpm * SpeedConstant) > 255)
                        mp_bpm = 255;
                    else
                        mp_bpm = (short)rint(old_bpm * SpeedConstant);

                    if (reinit_audio)
                    {
                        reinit_audio = false;
                        /*_driver.MD_Exit();
                        _driver.MD_Init();*/
                    }
                }
                while (PauseFlag == 127);
            }

        }

        /* Update effects */
        for (t = 0; t < CurrentUniMod.ChannelsCount; t++)
        {
            mp_channel = (short)t;
            a = mp_audio[t];
            PlayEffects();
        }

        for (t = 0; t < CurrentUniMod.ChannelsCount; t++)
        {
            //INSTRUMENT *i;
            //SAMPLE *s;
            short envpan, envvol;

            a = mp_audio[t];
            //i=a.i;
            //s=a.s;

            if (a.Instrument == null || a.Sample == null)
                continue;

            if (a.Period < 40)
                a.Period = 40;
            if (a.Period > 8000)
                a.Period = 8000;

            if (a.Kick)
            {
                _mixer.VC_VoicePlay((short)t, a.Handle, a.Start, a.Sample.Length, a.Sample.LoopStart, a.Sample.LoopEnd, a.Sample.Flags);
                a.Kick = false;
                a.KeyOn = true;

                a.FadeVol = 32768;

                StartEnvelope(a.VolEnv, a.Instrument.VolFlg, a.Instrument.VolPts, a.Instrument.VolSus, a.Instrument.VolBeg, a.Instrument.VolEnd, a.Instrument.VolEnv);
                StartEnvelope(a.PanEnv, a.Instrument.PanFlg, a.Instrument.PanPts, a.Instrument.PanSus, a.Instrument.PanBeg, a.Instrument.PanEnd, a.Instrument.PanEnv);
            }

            envvol = ProcessEnvelope(a.VolEnv, (short)256, a.KeyOn);
            envpan = ProcessEnvelope(a.PanEnv, (short)128, a.KeyOn);

            tmpvol = a.FadeVol; /* max 32768 */
            tmpvol *= envvol; /* * max 256 */
            tmpvol *= a.Volume; /* * max 64 */
            tmpvol /= 16384; /* tmpvol/(256*64) => tmpvol is max 32768 */

            tmpvol *= globalvolume; /* * max 64 */
            tmpvol *= mp_volume; /* * max 100 */
            tmpvol /= 3276800; /* tmpvol/(64*100*512) => tmpvol is max 64 */

            _mixer.VC_VoiceSetVolume((short)t, (short)tmpvol);
            // _driver.MD_VoiceSetVolume(t,tmpvol&0xFF);

            if ((a.Sample.Flags & (SampleFormatFlags.SF_OWNPAN)) != 0)
            {
                _mixer.VC_VoiceSetPanning((short)t, DoPan(envpan, a.Panning));
                // _driver.MD_VoiceSetPanning(t,DoPan(envpan,a.panning) & 0xFF);
            }
            else
            {
                _mixer.VC_VoiceSetPanning((short)t, a.Panning);
                // _driver.MD_VoiceSetPanning(t,(a.panning) & 0xFF);
            }

            if ((CurrentUniMod.Flags & UniModFlags.UF_LINEAR) != 0)
                _mixer.VC_VoiceSetFrequency((short)t, GetFreq2(a.Period));
            else
                _mixer.VC_VoiceSetFrequency((short)t, (3579546 << 2) / a.Period);
            //_driver.MD_VoiceSetFrequency((short)t, (3579546 << 2) / a.Period);

            /*  if key-off, start substracting
            fadeoutspeed from fadevol: */
            if (!a.KeyOn)
            {
                if (a.FadeVol >= a.Instrument.VolFade)
                    a.FadeVol -= a.Instrument.VolFade;
                else
                    a.FadeVol = 0;
            }
        }


    }

    public virtual void MP_Init(SongModule m)
    {
        int t;

        CurrentUniMod = m;
        PatternLoopPosition = 0;
        RepeatCounter = 0;
        mp_sngpos = 0;
        mp_sngspd = m.InitialSpeed;

        TickCounter = mp_sngspd;
        PatternDelayCounter = 0;
        SecondPatternDelayCounter = 0;
        mp_bpm = m.InitialTempo;
        old_bpm = mp_bpm;
        //m_.cur_mod.Deleted = false;

        forbid = false;
        mp_patpos = 0;
        posjmp = 2; /* <- make sure the player fetches the first note */
        PatternBreakPosition = 0;

        isfirst = 2; /* delay start by 2 ticks */

        globalvolume = 64; /* reset global volume */

        /* Make sure the player doesn't start with garbage: */
        for (t = 0; t < CurrentUniMod.ChannelsCount; t++)
        {
            mp_audio[t].Kick = false;
            mp_audio[t].TmpVolume = 0;
            mp_audio[t].Retrig = 0;
            mp_audio[t].WaveControl = 0;
            mp_audio[t].Glissando = 0;
            mp_audio[t].SampleOffset = 0;
        }
    }

    public virtual bool MP_Ready()
    {
        return (mp_sngpos >= CurrentUniMod.Positions.Count);
    }

    public virtual void MP_NextPosition()
    {
        forbid = true;
        posjmp = 3;
        PatternBreakPosition = 0;
        TickCounter = mp_sngspd;
        forbid = false;
    }

    public virtual void MP_PrevPosition()
    {
        forbid = true;
        posjmp = 1;
        PatternBreakPosition = 0;
        TickCounter = mp_sngspd;
        forbid = false;
    }

    public virtual void MP_RestartPosition()
    {
        forbid = true;
        posjmp = 2;
        PatternBreakPosition = 0;
        TickCounter = mp_sngspd;
        forbid = false;
    }

    public virtual void MP_SetPosition(short pos)
    {
        /* avoid infinitely-looping mods */

        /*	if(pos>=pf.numpos) pos=pf.numpos;
        forbid=true;
        posjmp=2;
        patbrk=0;
        mp_sngpos=pos; 
        vbtick=mp_sngspd;
        forbid=false;*/
    }
}
