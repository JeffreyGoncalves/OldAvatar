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

		/// <summary>
		/// AudienceMember's seat row number, 1 being the closest.
		/// </summary>
		public int rowNumber;

		/// <summary>
		/// AudienceMember's horizontal position, 0 being the center.
		/// </summary>
		public float horizontalPosition;


		/// <summary>
		/// AudienceMember's current face, corresponding to their interest level.
		/// </summary>
		public string currentFace;

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

		public OpenTK.Vector4 faceColor = new OpenTK.Vector4(120 / 255f, 120 / 255f, 0/ 255f, 1);

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

		public AudienceMember(int rowNb, float horizontalPos, float threshold1, float threshold2)
		{
			this.rowNumber = rowNb;
			this.horizontalPosition = horizontalPos;
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

		public AudienceMember(int rowNb, float horizontalPos) 
			: this(rowNb, horizontalPos, (float)new Random().NextDouble()/2.0f, (float)new Random().NextDouble()/2.0f + 0.5f) 
		{}

	
		private void updateFace()
		{ 
			if (TrackingSideTool.Get().LookRightCheckBox.IsChecked == true)
			{
				if(GlobalInterest + interest <= this.thresholds[0]*2)
				{
					currentFace = "Audience_Bore";
					//faceColor = new OpenTK.Vector4(180 / 255f, 0 / 255f, 0/ 255f, 1);
				}
				else if (GlobalInterest + interest  > this.thresholds[1]*2)
				{
					currentFace = "Audience_Interest";
					//faceColor = new OpenTK.Vector4(0 /255f, 220 /255f, 50 /255f, 1);
				}
				else
				{ 
					currentFace = "Audience_SlightBore";
					//faceColor = new OpenTK.Vector4(170 /255f, 130 /255f, 0 /255f, 1);
				}
				float totalInterest = (GlobalInterest + interest) /2; 
				faceColor = new OpenTK.Vector4((1-totalInterest)*200 /255f, totalInterest*200 /255f, totalInterest*80 /255f, 0.5f);

			}
			else
			{
				if(GlobalInterest <= this.thresholds[0])
				{
					currentFace = "Audience_Bore";
					//faceColor = new OpenTK.Vector4(180 /255f, 0 /255f, 0 /255f, 1);
				}
				else if (GlobalInterest > this.thresholds[1])
				{
					currentFace = "Audience_Interest";
					//faceColor = new OpenTK.Vector4(0 /255f, 220 /255f, 50 /255f, 1);
				}
				else
				{ 
					currentFace = "Audience_SlightBore";
					//faceColor = new OpenTK.Vector4(170 /255f, 130 /255f, 0 /255f, 1);
				}
				faceColor = new OpenTK.Vector4((1-GlobalInterest)*200 /255f, GlobalInterest*200 /255f, GlobalInterest*80 /255f, 1);
			}
		}

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
