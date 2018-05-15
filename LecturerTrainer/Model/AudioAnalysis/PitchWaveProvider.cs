using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LecturerTrainer.Model.AudioAnalysis
{

    /// <summary>
    /// Class used to make the intermediate between the Pitch class and the AutoCorrelator class (for the pitch detection).
    /// Make some conversions to allow the pitch detection.
    /// </summary>
    /// <remarks>
    /// Author: Valentin Roux
    /// </remarks>
    public class PitchWaveProvider : IWaveProvider
    {

        #region Attributes

        /// <summary>
        /// Reference to the AutoCorrelator class for the pitch detection.
        /// </summary>
        private IPitchDetector pitchDetector;

        /// <summary>
        /// Converted stream for the pitch detection.
        /// </summary>
        private WaveBuffer waveBuffer;

        /// <summary>
        /// Original stream source.
        /// </summary>
        private IWaveProvider source;


        /// <summary>
        /// Used to prevent pitch detection errors.
        /// </summary>
        private float previousPitch;

        /// <summary>
        /// Used to detect pitch detection errors.
        /// </summary>
        private int release;
        private int maxHold = 1;

        /// <summary>
        /// Used to avoid writing concurencies between differents threads.
        /// </summary>
        private readonly object pitchLock = new object();

        /// <summary>
        /// Reference to the Pitch class
        /// (AR for audio recorder)
        /// </summary>
        Pitch AR;

        /// <summary>
        /// Get the Wave format of the strea source.
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instanciate a PitchWaveProvider
        /// </summary>
        /// <param name="source">Stream Source</param>
        /// <param name="ar">Pitch Reference</param>
        public PitchWaveProvider(IWaveProvider source, Pitch ar)
        {
            if (source.WaveFormat.SampleRate != 44100)
                throw new ArgumentException("Pitch Detection only works at 44.1kHz");
            if (source.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                throw new ArgumentException("Pitch Detection only works on IEEE floating point audio data");
            if (source.WaveFormat.Channels != 1)
                throw new ArgumentException("Pitch Detection only works on mono input sources");

            this.source = source;
            this.pitchDetector = new AutoCorrelator(source.WaveFormat.SampleRate);
            this.waveBuffer = new WaveBuffer(8192);
            this.AR = ar;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Read the buffer and launch the Pitch Detection that will be saved in the Pitch Class.
        /// </summary>
        /// <param name="buffer">Byte buffer of the raw signal</param>
        /// <param name="offset">Offset for the buffer</param>
        /// <param name="count">Lengh of the buffer</param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (waveBuffer == null || waveBuffer.MaxSize < count)
            {
                waveBuffer = new WaveBuffer(count);
            }

            int bytesRead = source.Read(waveBuffer, 0, count);

            //The last bit sometimes needs to be rounded up:
            if (bytesRead > 0) bytesRead = count;

            int frames = bytesRead / sizeof(float); 
            //Launch the pitch detection
            float pitch = pitchDetector.DetectPitch(waveBuffer.FloatBuffer, frames);

            if (pitch == 0f && release < maxHold)
            {
                pitch = previousPitch;
                release++;
            }
            else
            {
                this.previousPitch = pitch;
                release = 0;
            }

            //Save the pitch in an array in the Pitch Class
            lock (AR.Lock)
            {
                AR.getPitch().Add(pitch);
                if(AR.isRecording)
                    AR.getPitchRecorded().Add(pitch);
            }

            return bytesRead;
        }

        #endregion
    }
}
