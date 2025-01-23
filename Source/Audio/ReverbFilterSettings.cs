namespace Electron2D.Audio
{
    public class ReverbFilterSettings
    {
        public float DecayTimeInSeconds;
        public float Dampening;
        public float AllPassFrequency1;
        public float AllPassResonance1;
        public float AllPassFrequency2;
        public float AllPassResonance2;
        public float AllPassFrequency3;
        public float AllPassResonance3;
        public float LowPassCutoff;
        public float LowPassResonance;
        public float HighPassCutoff;
        public float HighPassResonance;

        public readonly static ReverbFilterSettings NormalRoom = new ReverbFilterSettings(0.3f, 0.2f, 80, 0.7f, 200, 1.0f, 400, 1.2f, 7000f, 0.8f, 100f, 0.8f);
        public readonly static ReverbFilterSettings LargeRoom = new ReverbFilterSettings(0.6f, 0.2f, 100f, 0.6f, 300f, 0.8f, 800f, 1.0f, 8000f, 0.7f, 80f, 0.7f);
        public readonly static ReverbFilterSettings SmallRoom = new ReverbFilterSettings(0.15f, 0.1f, 120f, 0.5f, 250f, 0.7f, 500f, 1.0f, 6000f, 0.8f, 120f, 0.8f);
        public readonly static ReverbFilterSettings BrightStudio = new ReverbFilterSettings(0.2f, 0.15f, 150f, 0.5f, 400f, 0.9f, 1200f, 1.2f, 10000f, 0.6f, 150f, 0.9f);
        public readonly static ReverbFilterSettings Cave = new ReverbFilterSettings(0.8f, 0.15f, 80f, 0.7f, 200f, 0.9f, 600f, 1.1f, 5000f, 0.9f, 70f, 0.7f);
        public readonly static ReverbFilterSettings ConcertHall = new ReverbFilterSettings(0.8f, 0.25f, 90f, 0.6f, 350f, 0.8f, 900f, 1.0f, 7500f, 0.7f, 90f, 0.8f);

        public ReverbFilterSettings(ReverbFilterSettings settings)
        {
            DecayTimeInSeconds = settings.DecayTimeInSeconds;
            Dampening = settings.Dampening;
            AllPassFrequency1 = settings.AllPassFrequency1;
            AllPassResonance1 = settings.AllPassResonance1;
            AllPassFrequency2 = settings.AllPassFrequency2;
            AllPassResonance2 = settings.AllPassResonance2;
            AllPassFrequency3 = settings.AllPassFrequency3;
            AllPassResonance3 = settings.AllPassResonance3;
            LowPassCutoff = settings.LowPassCutoff;
            LowPassResonance = settings.LowPassResonance;
            HighPassCutoff = settings.HighPassCutoff;
            HighPassResonance = settings.HighPassResonance;
        }

        public ReverbFilterSettings(float decayTimeInSeconds, float dampening,
            float allPassFrequency1, float allPassResonance1,
            float allPassFrequency2, float allPassResonance2,
            float allPassFrequency3, float allPassResonance3,
            float lowPassCutoff, float lowPassResonance,
            float highPassCutoff, float highPassResonance)
        {
            DecayTimeInSeconds = decayTimeInSeconds;
            Dampening = dampening;
            AllPassFrequency1 = allPassFrequency1;
            AllPassResonance1 = allPassResonance1;
            AllPassFrequency2 = allPassFrequency2;
            AllPassResonance2 = allPassResonance2;
            AllPassFrequency3 = allPassFrequency3;
            AllPassResonance3 = allPassResonance3;
            LowPassCutoff = lowPassCutoff;
            LowPassResonance = lowPassResonance;
            HighPassCutoff = highPassCutoff;
            HighPassResonance = highPassResonance;
        }
    }
}
