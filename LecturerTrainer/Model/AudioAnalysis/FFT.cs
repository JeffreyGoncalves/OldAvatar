using LecturerTrainer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.AudioAnalysis
{
    /// <summary>
    /// Class used to compute the Fast Fourier Transform on the input signal.
    /// </summary>
    /// <remarks>
    /// Author: Valentin Roux
    /// </remarks>
    public class FFT
    {
        #region Attributes

        /// <summary>
        /// Used to calculate the audio sample float.
        /// </summary>
        private const float Invert = 1 / (float)0x7fff;

        /// <summary>
        /// Size of the buffers for the FFT.
        /// </summary>
        private const short wBinsForFFT = 512;

        /// <summary>
        /// Real part of the FFT Calcul.
        /// </summary>
        private float[] _BinsFFTReal;

        /// <summary>
        /// Imaginary part of the FFT Calcul.
        /// </summary>
        private float[] _BinsFFTImaginary;

        /// <summary>
        /// Stock the result of the FFT.
        /// </summary>
        private float[] _BinsFFTDisplay;

        /// <summary>
        /// Stock the audio input sample for the FFT analysis.
        /// </summary>
        private float[] _AudioInputForFFT;

        /// <summary>
        /// Window function for the FFT.
        /// Here we use the Hamming window.
        /// </summary>
        private float[] _Window;


        /// <summary>
        /// Used to avoid sending too much events to the ViewModel.
        /// </summary>
        private bool sent = false;

        /// <summary>
        /// Used to count the number of frames for the sample.
        /// Once the right number reach, we can start the FFT.
        /// </summary>
        private int accumulatedSampleCountfft = 0;

        /// <summary>
        /// Stock the FFT during a short period of time.
        /// Used to detect the "Hummmmm" and the "Ehhhhh".
        /// </summary>
        private List<float[]> fftTime;

        /// <summary>
        /// Feedback to display when a bad reflex is detected.
        /// </summary>
        private static string badReflexText = "HMMMMMMMMMM not good";

        private int _hmNb;

        public int hmNb
        {
            get { return _hmNb; }
            set { _hmNb = value; }
        }

        private int _length;

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }
        
        


        public static event EventHandler<LongFeedback> reflexEvent;
        public static event EventHandler<FFTEventArgs> FFTEvent;

        #endregion

        #region Events


        /// <summary>
        /// Event raised each time a new FFT is calculated
        /// </summary>
        public class FFTEventArgs
        {
            public float[] FFTDisplay;
            public FFTEventArgs(float[] arg)
            {
                FFTDisplay = arg;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates the FFT Class
        /// </summary>
        public FFT()
        {
            _AudioInputForFFT = new float[wBinsForFFT];
            _BinsFFTReal = new float[wBinsForFFT];
            _BinsFFTImaginary = new float[wBinsForFFT];
            _BinsFFTDisplay = new float[wBinsForFFT / 2];
            _Window = new float[wBinsForFFT];
            float innerValue = (float)(2 * Math.PI / (wBinsForFFT - 1));
            for (int i = 0; i < wBinsForFFT; ++i)
            {
                _Window[i] = (float).5 * (1 - (float)Math.Cos(i * innerValue));
            }
            fftTime = new List<float[]>();
            hmNb = 0;
            Length = 10;
        }

        #endregion

        #region Methods

        public static void raiseReflexEvent(ServerFeedback feedback)
        {
            reflexEvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }

        public static void raiseReflexEvent(String feedbackMessage, bool display)
        {
            reflexEvent(null, new LongFeedback(feedbackMessage, display));
        }

        /// <summary>
        /// Compute the FFT and raise an event containing the result.
        /// </summary>
        /// <param name="audioBuffer">Byte buffer containing the raw audio</param>
        /// <param name="readCount">Number of frames contained in the buffer.</param>
        /// <returns>Returns the intensity of each frequency in a float array</returns>
        public float[] getFFT(byte[] audioBuffer, int readCount){
            const float Decay = 0.5f;
            for (int i = 0; i < readCount; i += 2)
            {
                short audioSample = BitConverter.ToInt16(audioBuffer, i);

                float audioSampleFloat = Invert * audioSample;

                try
                {
                    _AudioInputForFFT[this.accumulatedSampleCountfft++] = audioSampleFloat;
                }
                catch (Exception) { }

                if (this.accumulatedSampleCountfft < wBinsForFFT)
                {
                    continue;
                }

                //At this point we have enough samples to do our FFT
                for (int iSample = 0; iSample < wBinsForFFT; ++iSample)
                {
                    _BinsFFTReal[iSample] = _AudioInputForFFT[iSample] * _Window[iSample];
                }

                AForge.Math.Complex[] complexData = new AForge.Math.Complex[_AudioInputForFFT.Length];
                for (int j = 0; j < _BinsFFTReal.Length; ++j)
                {
                    complexData[j].Re = _BinsFFTReal[j];
                }
                for (int j = 0; j < _BinsFFTImaginary.Length; ++j)
                {
                    complexData[j].Im = _BinsFFTImaginary[j];
                }

                AForge.Math.FourierTransform.FFT(complexData, AForge.Math.FourierTransform.Direction.Forward);
                
                for (int iBin = 0; iBin < wBinsForFFT / 2; ++iBin)
                {
                    float imaginary = (float)complexData[iBin].Im;
                    float real = (float)complexData[iBin].Re;

                    float magnitude = (float)Math.Sqrt(imaginary * imaginary + real * real);

                    //This next operation will smooth out the results a little and prevent the display from jumping around wildly.
                    //You can play with this by changing the "Decay" parameter above.
                    float decayedOldValue = _BinsFFTDisplay[iBin] * Decay;
                    _BinsFFTDisplay[iBin] = Math.Max(magnitude, decayedOldValue);
                    //If no smoothing required:
                    //_BinsFFTDisplay[iBin] = magnitude;
                }
                if (FFTEvent != null && !MainWindow.main.audioProvider.replayMode)
                {
                    FFTEvent(this, new FFTEventArgs(_BinsFFTDisplay));
                }
                // We're all done with our FFT so we'll clean up and get ready for next time.
                this.accumulatedSampleCountfft = 0;
                    
                initBuff();
            }
            this.fftTime.Add((float[]) _BinsFFTDisplay.Clone());
            if (fftTime.Count > Length)
            {
                fftTime.RemoveAt(0);
            }
            return _BinsFFTDisplay;
        }

        /// <summary>
        /// Reinitialize the buffers
        /// </summary>
        private void initBuff()
        {
            for (int j = 0; j < _BinsFFTReal.Length; j++)
            {
                _BinsFFTReal[j] = 0;
            }
            for (int j = 0; j < _BinsFFTImaginary.Length; j++)
            {
                _BinsFFTImaginary[j] = 0;
            }
        }

        /// <summary>
        /// Check if someone says "Hummmmm" or "Ehhhhhhh" during the speech
        /// </summary>
        /// <returns>Boolean if yes or no there is a "Hummmmm" or a "Ehhhhhhh"</returns>
        public bool BadReflexSpeaking()
        {
            if (!MainWindow.main.audioProvider.replayMode)
            {
                bool notZero = false;
                for (int i = 0; i < fftTime.Count - 1; i++)
                {
                    notZero = false;
                    for (int j = 0; j < fftTime[i].Length; j++)
                    {
                        //Sensibility of the detection
                        if (((fftTime[i][j] >= fftTime[i + 1][j] && fftTime[i][j] * 0.5 < fftTime[i + 1][j])
                            || (fftTime[i][j] <= fftTime[i + 1][j] && fftTime[i][j] * 1.5 > fftTime[i + 1][j]))
                            //If there is no talk but just noises:
                            && fftTime[i][j] > 0.0015 && fftTime[i + 1][j] > 0.0015)
                        {
                            notZero = true;
                        }
                    }
                    if (!notZero)
                    {
                        if (sent)
                        {
                            //Not detected
                            reflexEvent(this, new LongFeedback(badReflexText, false));
                            sent = false;
                            hmNb++;
                        }
                        return false;
                    }
                }
                if (!sent)
                {
                    //Detected !
                    reflexEvent(this, new LongFeedback(badReflexText, true));
                    sent = true;
                }
                return true;
            }
            else
                return false;
        }



        #endregion
    }
}
