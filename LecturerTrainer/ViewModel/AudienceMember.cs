using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LecturerTrainer.Model;
using LecturerTrainer.View;


namespace LecturerTrainer.ViewModel
{
	class AudienceMember
	{
		/// <summary>
		/// AudienceMember's natural thresholds for their interest levels.
		/// </summary>
		private float[] thresholds = new float[2];

        public float thresholdsLow { get { return thresholds[0]; } }
        public float thresholdsHight { get { return thresholds[1]; } }

		/// <summary>
		/// AudienceMember's seat row number, 1 being the closest.
		/// </summary>
		public int rowNumber;

		/// <summary>
		/// AudienceMember's horizontal position, 0 being the center.
		/// </summary>
		public float horizontalPosition;


		/// <summary>
		/// AudienceMember's current face when drawing the audience in 2D. Depends on their interest level.
		/// </summary>
		public string currentFace;

		/// <summary>
		/// AudienceMember's current face color when drawing the audience in 3D. Directely depends on their interest level.
		/// </summary>
		public OpenTK.Vector4 faceColor = new OpenTK.Vector4();

		/// <summary>
		/// AudienceMember's current personnal interest which will influence their interest level.
		/// </summary>
		private float interest;
		public float Interest{
			get
			{
				return interest;
			}
			set
			{
				interest = value;
				updateFace();
			}
		}

		/// <summary>
		/// Audience Member's body color when drawing the audience in 3D. Corresponds to a light blue
		/// </summary>
		public static OpenTK.Vector4 BodyColor = new OpenTK.Vector4(13 / 255f, 86 / 255f, 119 / 255f, 1);

		/// <summary>
		/// The current interest level of the whole audience, which will influence each member's interest level
		/// </summary>
		private static float globalInterest = 0.5f;
		public static float GlobalInterest{
			get
			{
				return globalInterest;
			}
			set
			{
				globalInterest = value;
				foreach (AudienceMember member in WholeAudience)
				{
					member.updateFace();
				}
			}
		}

		/// <summary>
		/// List containing all the members of the audience
		/// </summary>
		public static List<AudienceMember> WholeAudience = new List<AudienceMember>();

		/// <summary>
		/// Regular constructor
		/// </summary>
		/// <param name="rowNb"></param>
		/// <param name="horizontalPos"></param>
		/// <param name="threshold1"></param>
		/// <param name="threshold2"></param>
		/// <author> Oummar Mayaki </author>
		public AudienceMember(int rowNb, float horizontalPos, float threshold1, float threshold2)
		{
			this.rowNumber = rowNb;
			this.horizontalPosition = horizontalPos;

			// To make sure the thresholds are in order
			if (threshold1 > threshold2)
			{
				this.thresholds[0] = threshold2;
				this.thresholds[1] = threshold1;
			}
			else{
				this.thresholds[0] = threshold1;
				this.thresholds[1] = threshold2;
			}

			this.currentFace = "Audience_SlightBore";
			this.interest = 0.5f;
			WholeAudience.Add(this);
		}

		/// <summary>
		/// Constructor that creates an AudienceMember with random thresholds
		/// </summary>
		/// <param name="rowNb"></param>
		/// <param name="horizontalPos"></param>
		/// <author> Oummar Mayaki </author>
		public AudienceMember(int rowNb, float horizontalPos) 
			: this(rowNb, horizontalPos, (float)new Random().NextDouble()/2.0f, (float)new Random().NextDouble()/2.0f + 0.5f) 
		{}

		/// <summary>
		/// Updates the AudienceMember current's face(2D) or color(3D) according to their interest level.
		/// </summary>
		/// <author> Oummar Mayaki </author>
		private void updateFace()
		{ 

			// With view direction
			if (TrackingSideTool.Get().LookRightCheckBox.IsChecked == true)
			{
				// Face
				if(GlobalInterest + interest <= this.thresholds[0]*2)
					currentFace = "Audience_Bore";
				else if (GlobalInterest + interest  > this.thresholds[1]*2)
					currentFace = "Audience_Interest";
				else
					currentFace = "Audience_SlightBore";
				
				// Color
				float totalInterest = (GlobalInterest + interest) /2; 
				float red = Math.Min(2-(2*totalInterest), 1 )*150 /255f;
				float green = Math.Min(totalInterest*2, 1 ) *150 /255f;
				faceColor = new OpenTK.Vector4(red , green, 0, 1);
			}

			// Without view direction
			else
			{
				// Face
				if(GlobalInterest <= this.thresholds[0])
					currentFace = "Audience_Bore";
				else if (GlobalInterest > this.thresholds[1])
					currentFace = "Audience_Interest";
				else
					currentFace = "Audience_SlightBore";

				// Color
				float red = Math.Min(2-(2*GlobalInterest), 1 )*150 /255f;
				float green = Math.Min(GlobalInterest*2, 1 ) *150 /255f;
				faceColor = new OpenTK.Vector4(red , green, 0, 1);
			}
		}

		/// <summary>
		/// Updates each member's personal interest when view direction is activated
		/// </summary>
		/// <param name="direction"></param>
		/// <author> Oummar Mayaki </author>
		public static void updateAudienceInterest(int direction){
			foreach (AudienceMember member in WholeAudience)
			{	
				// Right
				if (direction < 0)
				{
					if (member.horizontalPosition >= 0.3f && member.Interest <= 1) member.Interest += 0.003f;
					else if (member.horizontalPosition == 0 && member.Interest >= 0) member.Interest -= 0.001f;
					else if (member.horizontalPosition < 0 && member.Interest >= 0) member.Interest -= 0.002f;
				}
				
				//Left
				else if(direction > 0)
				{
					if (member.horizontalPosition <= -0.3f && member.Interest <= 1) member.Interest += 0.003f;
					else if (member.horizontalPosition == 0 && member.Interest >= 0) member.Interest -= 0.001f;
					else if (member.horizontalPosition >= 0 && member.Interest >= 0) member.Interest -= 0.002f;
				}

				// Center
				else
				{
					if ((member.horizontalPosition >= -0.3f || member.horizontalPosition >= 0.3f) && member.Interest <= 1) member.Interest += 0.0025f;
					else if ((member.horizontalPosition < -0.60f || member.horizontalPosition > 0.60f) && member.Interest >= 0) member.Interest -= 0.0015f;
				}
			}
		}

	}

}
