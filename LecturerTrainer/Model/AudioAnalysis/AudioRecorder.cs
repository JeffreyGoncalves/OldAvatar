using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.AudioAnalysis
{
    public class AudioRecorder
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveFileWriter waveWriter;
        private String filePath;
        private String fileName;

        private WaveIn waveIn;

        public AudioRecorder(String path, String name, WaveIn wi)
        {
            filePath = path;
            waveIn = wi;
            fileName = name;

            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
        }

        public void startRecording()
        {
            waveWriter = new WaveFileWriter(filePath + fileName + ".wav", waveIn.WaveFormat);
            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
        }

        public void stopRecording()
        {
            if(waveWriter != null)
                waveWriter.Close();
            waveIn.DataAvailable -= new EventHandler<WaveInEventArgs>(wi_DataAvailable);
        }

        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Write(e.Buffer, 0, e.Buffer.Length);
        }
    }
}
