using System;
using System.Diagnostics;

namespace LecturerTrainer.Model.AudioAnalysis
{
    /// <summary>
    /// This class is used for the Pitch Detection.
    /// The mathematic method we is using is the AutoCorrelation one.
    /// </summary>
    /// <remarks>
    /// Autor: Valentin Roux
    /// Inspired by: An autotune project leaded by Mark Health
    /// </remarks>
    public class AutoCorrelator : IPitchDetector
    {
        #region attributes

        /// <summary>
        /// Previous buffer used for the AutoCorrelation
        /// </summary>
        private float[] prevBuffer;

        /// <summary>
        /// Sampling of the signal
        /// </summary>
        private float sampleRate;

        /// <summary>
        /// Minimum Offset
        /// </summary>
        private int minOffset;

        /// <summary>
        /// Maximum Offset
        /// </summary>
        private int maxOffset;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct an AutoCorrelator object.
        /// Used to Detect the pitch in the voice.
        /// </summary>
        /// <param name="sampleRate">Sampling of the signal</param>
        public AutoCorrelator(int sampleRate)
        {
            this.sampleRate = (float)sampleRate;
            int minFreq = 90;
            int maxFreq = 255;

            this.maxOffset = sampleRate / minFreq;
            this.minOffset = sampleRate / maxFreq;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Detect the picth in the voice.
        /// </summary>
        /// <param name="buffer">Float Array containing the RAW signal.</param>
        /// <param name="frames">Number of frames contained in the buffer.</param>
        /// <returns>Returns the pitch.</returns>
        public float DetectPitch(float[] buffer, int frames)
        {
            if (prevBuffer == null)
            {
                prevBuffer = new float[frames];
            }
            float secCor = 0;
            int secLag = 0;

            float maxCorr = 0;
            int maxLag = 0;

            // starting with low frequencies, working to higher
            for (int lag = maxOffset; lag >= minOffset; lag--)
            {
                float corr = 0; // this is calculated as the sum of squares
                for (int i = 0; i < frames; i++)
                {
                    int oldIndex = i - lag;
                    float sample = ((oldIndex < 0) ? prevBuffer[frames + oldIndex] : buffer[oldIndex]);
                    corr += (sample * buffer[i]);
                }
                if (corr > maxCorr)
                {
                    maxCorr = corr;
                    maxLag = lag;
                }
                if (corr >= 0.9 * maxCorr)
                {
                    secCor = corr;
                    secLag = lag;
                }
            }
            for (int n = 0; n < frames; n++)
            {
                prevBuffer[n] = buffer[n];
            }
            float noiseThreshold = frames / 1000f;
            if (maxCorr < noiseThreshold || maxLag == 0) return 0.0f;
            return this.sampleRate / maxLag;
        }

        #endregion
    }
}
