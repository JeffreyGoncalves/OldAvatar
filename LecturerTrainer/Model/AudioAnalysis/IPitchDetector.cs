using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LecturerTrainer.Model.AudioAnalysis
{
    /// <summary>
    /// Interface for differentes pitch algorithms.
    /// </summary>
    /// <remarks>
    /// Author: Valentin Roux
    /// Inspired by: An autotune project leaded by Mark Health
    /// </remarks>
    public interface IPitchDetector
    {
        /// <summary>
        /// Detect the picth in the voice.
        /// </summary>
        /// <param name="buffer">Float Array containing the RAW signal.</param>
        /// <param name="frames">Number of frames contained in the buffer.</param>
        /// <returns>Returns the pitch.</returns>
        float DetectPitch(float[] buffer, int frames);
    }
}
