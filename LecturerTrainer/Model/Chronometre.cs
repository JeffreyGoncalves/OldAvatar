using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model
{
    class Chronometre
    {
        int seconds;
        int minutes;
        int hours;
        DateTime dateStarted;
        DateTime datePaused;
        bool paused;
        private bool isLaunched;        // True when timer launched
        public bool isStarted;

        private System.Timers.Timer timeToUpdate; 

        public System.Timers.Timer TimeToUpdate
        {
            get { return timeToUpdate; }
            set { timeToUpdate = value;}
        }
        

        public Chronometre()
        {
            seconds = 0;
            minutes = 0;
            hours = 0;
            isLaunched = false;
            paused = false;
            TimeToUpdate = new System.Timers.Timer(1000);
        }

        public void Start()
        {
            isLaunched = true;
            isStarted = true;

            if (paused == true)
            {
                TimeSpan t = DateTime.Now.Subtract(datePaused);
                dateStarted = dateStarted.Add(t);
            }
            else
                dateStarted = DateTime.Now;

            TimeToUpdate.Start();

 
        }

        public void Stop()
        {
            isLaunched = false;
            datePaused = DateTime.Now;
            paused = true;
            TimeToUpdate.Stop();
        }

        public void Reset()
        {
            isLaunched = false;
            paused = false;
            dateStarted = DateTime.Now;
            datePaused = DateTime.Now;
            isStarted = false;
        }

        public bool isChronoLaunched
        {
            get
            {
                return isLaunched;
            }
            
        }


        public override String ToString()
        {
            DateTime DateNow = DateTime.Now;
            TimeSpan Difference = DateNow - dateStarted;

            StringBuilder builder = new StringBuilder(11);
            builder.Append(Difference.Hours.ToString("d2"));
            builder.Append(":");
            builder.Append(Difference.Minutes.ToString("d2"));
            builder.Append(":");
            builder.Append(Difference.Seconds.ToString("d2"));

            return builder.ToString();
        }

        public String Now()
        {
            DateTime dateNow = DateTime.Now;
            StringBuilder builder = new StringBuilder(8);
            builder.Append(dateNow.Hour.ToString("d2"));
            builder.Append(":");
            builder.Append(dateNow.Minute.ToString("d2"));
            builder.Append(":");
            builder.Append(dateNow.Second.ToString("d2"));

            return builder.ToString();
        }

        public int NowSeconds()
        {
            return (int) (DateTime.Now.Subtract(dateStarted)).TotalSeconds; 
            //return seconds + (minutes * 60) + (hours * 60 * 60); 
        }
    }
}
