using LecturerTrainer.ViewModel;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.AudioAnalysis
{

    /// <summary>
    /// Class used to do do the intermediate between the raw byte signal and the Pitch detector.
    /// Also Smooth the pitch wave and compute the standard deviation of the pitch wave.
    /// </summary>
    /// <remarks>
    /// Author: Valentin Roux
    /// </remarks>
    public class Pitch
    {

        #region Attributes

        /// <summary>
        /// Format of the recording signal
        /// </summary>
        private WaveFormat recordingFormat;

        /// <summary>
        /// List containing the pitch evolution in the time.
        /// </summary>
        private List<float> pitchList;
		private static Pitch instance = null;
		public static Pitch Get(){
			return instance;
		}

        /// <summary>
        /// Used to record data
        /// </summary>
        private List<float> pitchRecorded;

        /// <summary>
        /// Boolean used to know if sending new events is necessary
        /// </summary>
        private bool sent;

        /// <summary>
        /// Used to know if the data must be recorded
        /// </summary>
        public bool isRecording;

        /// <summary>
        /// Object used to avoid writing competition on some attributes between several threads.
        /// </summary>
        public readonly object Lock = new object();

        /// <summary>
        /// Feedback to display when the speech is too boring.
        /// </summary>
        //private static string tooBoringText = "Too boring!";

        /// <summary>
        /// Feedback to display when the speech has too much variation.
        /// </summary>
        //private static string tooMuchVariationText = "Too much variation!";

        /// <summary>
        /// Feedback to display when the speech has too much variation.
        /// </summary>
        //private static string PicDetected = "A pic was detected";

        /// <summary>
        /// Feedback to display when the speech has too high frequences.
        /// </summary>
        //private static string HighSpeaking = "Too high frequences";

        /// <summary>
        /// Feedback to display when the speech has too low frequences.
        /// </summary>
        //private static string LowSpeaking = "Too low frequences";

        public static float[] wiggle = new float[300];


        /// <summary>
        /// Event raised when a speech is considered as boring
        /// </summary>
        public static event EventHandler<LongFeedback> BoringEvent;

        public static event EventHandler<InstantFeedback> PeakEvent;

        private bool canSendEvent = true;

        private int _length;

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        private double _threshold;

        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        private double _thresholdVariation;

        public double ThresholdVariation
        {
            get { return _thresholdVariation; }
            set { _thresholdVariation = value; }
        }

        private double _maxFrequency;

        public double maxFrequency
        {
            get { return _maxFrequency; }
            set { _maxFrequency = value; }
        }

        private double _minFrequency;

        public double minFrequency
        {
            get { return _minFrequency; }
            set { _minFrequency = value; }
        }

        private KinectDevice _kinect;

        public KinectDevice kinect
        {
            get { return _kinect; }
            set { _kinect = value; }
        }

        public int i;
        public int k = 0;
        public static double[] tab = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }; // table used for the peak detection
        public static double[] tabVol = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }; // table used for the peak detection
        public int state = 0; // 0 = neutral; 1 = increase; -1 = decrease
        public int advancementCounter = 0; // count since the program detect a rising
        public double beginningValue = 0; // the first value of the rising
        public int peakThreshold = 30; // Used to determine when the value is enough high or low to be a peak        
        public int volumethreshold = 9; // Used to determine when the volule is enough high to be a peak (volume between 0 and 13-14)

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a Pitch object
        /// </summary>
        public Pitch(KinectDevice kd)
        {
            this.recordingFormat = new WaveFormat(44100, 1);
            this.pitchList = new List<float>();
            this.pitchRecorded = new List<float>();
            this.sent = false;
            this.isRecording = false;
            this.kinect = kd;
            i = 0;
            AudioAnalysis.FFT.reflexEvent += switchSendEvent;
            Length = 500;
            Threshold = 20.0;
            ThresholdVariation = 50.0;
            minFrequency = 10.0;
            maxFrequency = 130.0;
			instance = this;
        }

        #endregion

        #region Methods

        public static void raiseBoringEvent(ServerFeedback feedback)
        {
            BoringEvent(null, new LongFeedback(feedback.feedbackMessage, feedback.display));
        }

        public static void raisePeakEvent(ServerFeedback feedback)
        {
            PeakEvent(null, new InstantFeedback(feedback.feedbackMessage));
        }

        private void switchSendEvent(object sender, LongFeedback e)
        {
            canSendEvent = !e.display;
        }

        /// <summary>
        /// Get the pitch evolution in the time.
        /// </summary>
        /// <returns>The list of pitch evolution</returns>
        public List<float> getPitch()
        {
            return pitchList;
        }

        /// <summary>
        /// Get the pitch recorded for the statistics
        /// </summary>
        /// <returns>The list of the pitch evolution</returns>
        public List<float> getPitchRecorded()
        {
            return pitchRecorded;
        }



        /// <summary>
        /// Cast the byte stream into a float buffer and launch the pitch detection,
        /// the smooth maker and raise events in case if the standard deviation is too high.
        /// </summary>
        /// <param name="buffer">Byte buffer corresponding to the raw signal.</param>
        /// <param name="bytesRecorded">Number of frames in the buffer.</param>
        public void pitchCompute(byte[] buffer, int bytesRecorded)
        {
            Stream stream = new MemoryStream(buffer);
            var reader = new RawSourceWaveStream(stream, recordingFormat);
            IWaveProvider stream32 = new Wave16ToFloatProvider(reader);
            PitchWaveProvider streamEffect = new PitchWaveProvider(stream32, this);
            IWaveProvider stream16 = new WaveFloatTo16Provider(streamEffect);
            var buffert = new byte[1024];
            int bytesRead;
            do
            {
                bytesRead = stream16.Read(buffert, 0, buffert.Length);
            } while (bytesRead != 0);
            reader.Close();
            stream.Close();
            this.PitchSmoothing();
            if (canSendEvent && !MainWindow.main.audioProvider.replayMode)
                lock (Lock)
                {
                    double sd1 = StdDevTShort();
                    double sd2 = StdDevLong();
                    double sd = StdDev();
                    //Console.WriteLine("SD =" + sd + " Threshiold  " + Threshold + "sd1 = " + sd1);

                    if ((sd <= Threshold && sd != 0.0) && pitchList.Last() > 0 && !this.sent)
                    {
                        BoringEvent(this, new LongFeedback("", true));
                        this.sent = true;
                    }
                    if ((sd > Threshold || sd == 0.0) && this.sent)
                    {
                        BoringEvent(this, new LongFeedback("", false));
                        this.sent = false;
                    }

                    /* There is something wrong with the way the above code. The tooBoringText displays at the beginning of a boring episode
                    and then disappears after a few seconds. Then it flashes up again at the end of a boring episode for a few seconds
                    I tried to model the code below on the emotion recognition code which also uses long feedback and responds very quickly
                    to a change in facial expression.
                    */

                    // Too much variation
                    /*        if (sd > ThresholdVariation && !this.sent && pitchList.Last() > 0)
                           {
                                BoringEvent(this, new LongFeedback(tooMuchVariationText, true));
                                this.sent = true;
                            }
                           if (sd <= ThresholdVariation || sd == 0.0 && this.sent)
                           {
                               BoringEvent(this, new LongFeedback(tooMuchVariationText, false));
                               this.sent = false;
                           }
                    */

                    i++;
                }
        }

        /// <summary>
        /// Cast the byte stream into a float buffer and launch the pitch detection,
        /// the smooth maker and raise events in case if the standard deviation is too high.
        /// </summary>
        /// <param name="buffer">Byte buffer corresponding to the raw signal.</param>
        /// <param name="bytesRecorded">Number of frames in the buffer.</param>
        public void pitchComputePeak(byte[] buffer, int bytesRecorded)
        {
            Stream stream = new MemoryStream(buffer);
            var reader = new RawSourceWaveStream(stream, recordingFormat);
            IWaveProvider stream32 = new Wave16ToFloatProvider(reader);
            PitchWaveProvider streamEffect = new PitchWaveProvider(stream32, this);
            IWaveProvider stream16 = new WaveFloatTo16Provider(streamEffect);
            var buffert = new byte[1024];
            int bytesRead;
            do
            {
                bytesRead = stream16.Read(buffert, 0, buffert.Length);
            } while (bytesRead != 0);
            reader.Close();
            stream.Close();

            // Calculate the volume of the voice
            long totalSquare = 0;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                totalSquare += sample * sample;
            }
            long meanSquare = 2 * totalSquare / buffert.Length;
            double rms = Math.Sqrt(meanSquare);
            double volume = rms / 32768.0;
            // volume is between 0.0 and 1.0

            // I used the average of 8 values to have a better result than just with the volume value
            tabVol[0] = tabVol[1];
            tabVol[1] = tabVol[2];
            tabVol[2] = tabVol[3];
            tabVol[3] = tabVol[4];
            tabVol[4] = tabVol[5];
            tabVol[5] = tabVol[6];
            tabVol[6] = tabVol[7];
            tabVol[7] = volume;
            volume = tabVol.Average();
            // To compare easily the volume value I use an integer
            volume *= 10;
            volume = Convert.ToInt32(volume);

            if (pitchList.Count > 300){
				for (i = 0; i < 300; i++) wiggle[i] = pitchList[pitchList.Count - 301 + i]; //wiggle = the 300 last elenents from pitchList
			}

            this.PitchSmoothing();
            if (canSendEvent)
                lock (Lock)
                {
                    double sd1 = StdDevTShort();
                    double sd2 = StdDevLong();
                    double sd = StdDev();



                    // each time the function is called we shift the values
                    tab[0] = tab[1];
                    tab[1] = tab[2];
                    tab[2] = tab[3];
                    tab[3] = tab[4];
                    tab[4] = tab[5];
                    tab[5] = tab[6];
                    tab[6] = tab[7];
                    tab[7] = sd2;

                    // If the rising is over, permit to don't count a peak as a rising up and down in a row
                    if (tab[0] == 0 || tab[tab.Length - 1] == 0 || (tab[0] <= beginningValue + 5 && tab[0] >= beginningValue - 5 && advancementCounter > 2))
                    {
                        state = 0;
                        beginningValue = 0;
                        advancementCounter = 0;
                    }
                    else
                    {
                        // If the curve is flat
                        if (state == 0)
                        {
                            // Calculate the wanted index
                            int indMax = localMaximum(tab);
                            int indMin = localMinimum(tab);
                            // If there is a big rising up
                            if (tab[indMax] > tab[0] + peakThreshold)
                            {
                                state = 1;
                                beginningValue = tab[0];
                                advancementCounter = 0;
                            }
                            // Or if there is a big rising down
                            else if (tab[indMin] < tab[0] - peakThreshold)
                            {
                                state = -1;
                                beginningValue = tab[0];
                                advancementCounter = 0;
                            }
                            else
                            {
                                beginningValue = 0;
                                advancementCounter = 0;
                            }
                        }
                        // If the curve is growing up or down
                        else
                        {
                            double average = 0.0;
                            // Calculate the average of the seven last value
                            for (int cpt = 1; cpt < tab.Length - 1; cpt++)
                            {
                                average += tab[cpt];
                            }
                            average /= tab.Length - 1;
                            // The curve is flat, so it reinitialize all
                            if (average > tab[0] - 3 && average < tab[0] + 3)
                            {
                                beginningValue = 0;
                                advancementCounter = 0;
                                state = 0;
                            }
                            // The counter advance
                            else
                                advancementCounter++;
                        }
                    }
                    // All the feedback files of the different tests
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\test.txt", true))
                    //{
                    //    file.WriteLine("frame n" + k + " -- state : " + state);
                    //}
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\testShort.txt", true))
                    //{
                    //    file.WriteLine(sd1);
                    //}
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\testLong.txt", true))
                    //{
                    //    file.WriteLine(sd2);
                    //}
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\testVol.txt", true))
                    //{
                    //    file.WriteLine("frame n" + k + " -- state : " + volume);
                    //}
                    // For testing the efficient of the peak detection we used the boring event to display the feedback.
                    // ASu has now changed this to use a peak event
                    if (volume >= 2) //volumethreshold)
                    {
                        if (state == 1) //&& !this.sent)
                        {
                            PeakEvent(this, new InstantFeedback("Rising Tone"));
                            //this.sent = true;
                        }
                        if (state == -1) //&& !this.sent)
                        {
                            PeakEvent(this, new InstantFeedback("Falling Tone"));
                            //this.sent = true;
                        }
                    }

                    k++;
                    i++;
                }
        }

        // Find the first maximum value
        public int localMaximum(double[] tab)
        {
            int ind = 0;
            bool go = true;
            while (ind < tab.Length - 1 && go)
            {
                if (tab[ind] <= tab[ind + 1])
                    ind++;
                else
                    go = false;
            }
            return ind;
        }

        // Find the first minimum value
        public int localMinimum(double[] tab)
        {
            int ind = 0;
            bool go = true;
            while (ind < tab.Length - 1 && go)
            {
                if (tab[ind] >= tab[ind + 1])
                    ind++;
                else
                    go = false;
            }
            return ind;
        }

        /// <summary>
        /// Smooth the pitch recorded to erase the wave discontinuities.
        /// </summary>
        private void PitchSmoothing()
        {
            int marge = 50;
            lock (Lock)
            {
                if (pitchList.Count > 3)
                {
                    for (int i = 0; i < pitchList.Count - 3; i++)
                    {
                        if (pitchList[i] == 0 || pitchList[i + 1] == 0 || pitchList[i + 2] == 0) continue;
                        //Case 1
                        if ((pitchList[i] - pitchList[i + 1] > marge)
                            && (pitchList[i] - pitchList[i + 2] > marge)
                            && (Math.Abs(pitchList[i + 1] - pitchList[i + 2]) < marge))
                        {
                            float h = pitchList[i + 1] - pitchList[i + 2];
                            pitchList[i] = pitchList[i + 1] + h / 2.0f;
                            continue;
                        }
                        //Case 2
                        if ((pitchList[i + 1] - pitchList[i] > marge)
                            && (pitchList[i + 1] - pitchList[i + 2] > marge)
                            && (Math.Abs(pitchList[i] - pitchList[i + 2]) < marge))
                        {
                            float h = pitchList[i] - pitchList[i + 2];
                            pitchList[i + 1] = pitchList[i] + h / 2.0f;
                            continue;
                        }
                        //Case 3
                        if ((pitchList[i + 2] - pitchList[i] > marge)
                            && (pitchList[i + 2] - pitchList[i + 1] > marge)
                            && (Math.Abs(pitchList[i] - pitchList[i + 1]) < marge))
                        {
                            float h = pitchList[i + 1] - pitchList[i];
                            pitchList[i + 2] = pitchList[i + 1] + h / 2.0f;
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compute the Standard Deviation of the pitch evolution on a short period of time.
        /// </summary>
        /// <returns>The Standard Deviation</returns>
        public double StdDev()
        {
            try
            {
                lock (Lock)
                {
                    // Analysis made on 300 frames
                    if (pitchList.Count < Length + 3) return 0;
                    List<float> sList = new List<float>();
                    for (int i = pitchList.Count - (Length + 3); i < pitchList.Count - 3; i++)
                    {
                        if (pitchList[i] == 0)
                        {
                            continue;
                        }
                        sList.Add(pitchList[i]);
                    }

                    if (sList.Count > 0)
                    {
                        float mean = sList.Average();

                        double sum = 0.0;
                        foreach (float f in sList)
                        {
                            double delta = f - mean;
                            sum += delta * delta;
                        }
                        double res = Math.Sqrt(sum / (sList.Count - 1));
                        return res;
                    }
                }
            }
            catch (Exception) { }
            return 0;
        }

        /// <summary>
        /// Crash test class, short looking
        /// </summary>
        /// <returns>The Standard Deviation</returns>
        public double StdDevTShort()
        {
            try
            {
                lock (Lock)
                {
                    // Analysis made on 60 frames, 2 seconds
                    int length = 60;
                    if (pitchList.Count < Length + 3) return 0;
                    List<float> sList = new List<float>();
                    for (int i = pitchList.Count - (length + 3); i < pitchList.Count - 3; i++)
                    {
                        if (pitchList[i] != 0)
                        {
                            sList.Add(pitchList[i]);
                        }
                    }

                    if (sList.Count > 0)
                    {
                        double mean = sList.Average();
                        return mean;
                    }
                }
            }
            catch (Exception) { }
            return 0;
        }

        /// <summary>
        /// Crash test class, long looking
        /// </summary>
        /// <returns>The Standard Deviation</returns>
        public double StdDevLong()
        {
            try
            {
                lock (Lock)
                {
                    // Analysis made on 300 frames, 10 seconds
                    int length = 150;
                    if (pitchList.Count < Length + 3) return 0;
                    List<float> sList = new List<float>();
                    for (int i = pitchList.Count - (length + 3); i < pitchList.Count - 3; i++)
                    {
                        if (pitchList[i] != 0)
                        {
                            sList.Add(pitchList[i]);
                        }
                    }
                    if (sList.Count > 0)
                    {
                        double mean = sList.Average();
                        return mean;
                    }
                }
            }
            catch (Exception) { }
            return 0;
        }

        public void startRecording()
        {
            this.isRecording = true;
        }

        public void stopRecording()
        {
            this.isRecording = false;
        }

        #endregion
    }
}
