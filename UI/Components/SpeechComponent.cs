using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class SpeechComponent : LogicComponent, IDeactivatableComponent
    {
        public LiveSplitState State { get; set; }
        //public MediaPlayer.IMediaPlayer Player { get; set; }
        protected SpeechSettings Settings { get; set; }

        public bool Activated { get; set; }

        public override string ComponentName
        {
            get { return "Speech"; }
        }

        public SpeechComponent(LiveSplitState state)
        {
            Settings = new SpeechSettings();
            
            State = state;
            //Player = new MediaPlayer.MediaPlayer();
            Activated = true;

            //State.OnStart += State_OnStart;
            State.OnSplit += State_OnSplit;
            //State.OnSkipSplit += State_OnSkipSplit;
            //State.OnUndoSplit += State_OnUndoSplit;
            //State.OnPause += State_OnPause;
            //State.OnResume += State_OnResume;
            //State.OnReset += State_OnReset;
        }

        void State_OnReset(object sender, TimerPhase e)
        {
            if (e != TimerPhase.Ended)
                PlaySound("Timer resetted");
        }

        void State_OnResume(object sender, EventArgs e)
        {
            PlaySound("Timer resumed");
        }

        void State_OnPause(object sender, EventArgs e)
        {
            PlaySound("Timer paused");
        }

        void State_OnUndoSplit(object sender, EventArgs e)
        {
            PlaySound("Split undone. You currently are at " + State.CurrentSplit.Name);
        }

        void State_OnSkipSplit(object sender, EventArgs e)
        {
            PlaySound("Skipped Split. You currently are at " + State.CurrentSplit.Name);
        }

        void State_OnSplit(object sender, EventArgs e)
        {
            if (State.CurrentPhase == TimerPhase.Ended)
            {
                PlaySound("GG");
            }
            else
            {
                var text = GetSoundTextForSplit();
                PlaySound(text);
            }
        }

        public String GetSoundTextForSplit()
        {
            var splitIndex = State.CurrentSplitIndex - 1;
            var timeDifference = State.Run[State.CurrentSplitIndex - 1].SplitTime[State.CurrentTimingMethod] - State.Run[State.CurrentSplitIndex - 1].Comparisons[State.CurrentComparison][State.CurrentTimingMethod];
            String text = null;
            if (timeDifference != null)
            {
                var timeDifferenceText = FormatTime(timeDifference.Value);
                var previousSegment = LiveSplitStateHelper.GetPreviousSegmentDelta(State, splitIndex, State.CurrentComparison, State.CurrentTimingMethod);
                var previousSegmentText = FormatTime(previousSegment ?? TimeSpan.Zero);

                if (timeDifference < TimeSpan.Zero)
                {
                    text = timeDifferenceText + " ahead at " + State.Run[State.CurrentSplitIndex - 1].Name + ". ";
             
                }
                else
                {
                    text = timeDifferenceText + " behind at " + State.Run[State.CurrentSplitIndex - 1].Name + ". ";
                   
                }
            }
            return text;
        }

        private String FormatTime(TimeSpan time)
        {
            var builder = new StringBuilder();

            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero - time;
            }

            var count = 0;
            var totalCount = (time.TotalHours >= 1 ? 1 : 0)
                + (time.Minutes >= 1 ? 1 : 0)
                + (time.Seconds >= 1 ? 1 : 0);

            Action insertAndMaybe = () =>
                {
                    if (count != 0 && count == totalCount - 1)
                        builder.Append("and ");
                };

            if (time.TotalHours >= 1)
            {
                builder.Append((int)time.TotalHours);
                builder.Append(" hour");
                if (time.TotalHours >= 2 || time.TotalHours < 1)
                    builder.Append("s");
                builder.Append(" ");
                count++;
            }

            insertAndMaybe();

            if (time.Minutes >= 1)
            {
                builder.Append(time.Minutes);
                builder.Append(" minute");
                if (time.Minutes != 1)
                    builder.Append("s");
                builder.Append(" ");
                count++;
            }

            insertAndMaybe();

            if (time.Seconds >= 1 || count == 0)
            {
                builder.Append(time.Seconds);
                if (count == 0)
                {
                    builder.Append(" point ");
                    builder.Append((int)((time.TotalSeconds % 1) * 10));
                }
                builder.Append(" second");
                if (time.Seconds != 1 || count == 0)
                    builder.Append("s");
                builder.Append(" ");
                count++;
            }

            return builder.ToString();
        }

        void State_OnStart(object sender, EventArgs e)
        {
            PlaySound("Timer started");
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public override System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return document.CreateElement("Settings");
            //return Settings.GetSettings(document);
        }

        public override void SetSettings(System.Xml.XmlNode settings)
        {
            //Settings.SetSettings(settings);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        public void PlaySound(String text)
        {
            if (Activated && !String.IsNullOrEmpty(text))
            {
                Task.Factory.StartNew(() =>
                {
                    var synth = new SpeechSynthesizer();
                    var voices = synth.GetInstalledVoices();
                    synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult, 0, new CultureInfo("en-US")); 
                    synth.SpeakAsync(text);
                });
            }
        }

        public override void Dispose()
        {
            State.OnStart -= State_OnStart;
            State.OnSplit -= State_OnSplit;
            State.OnSkipSplit -= State_OnSkipSplit;
            State.OnUndoSplit -= State_OnUndoSplit;
            State.OnPause -= State_OnPause;
            State.OnResume -= State_OnResume;
            State.OnReset -= State_OnReset;
            //Player.Stop();
        }

        
    }
}
