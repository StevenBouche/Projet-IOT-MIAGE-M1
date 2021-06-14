

namespace ControlerWPF.Models
{
    public class ControllerModel
    {
        public string LeftThumbXLib;
        public string LeftThumbYLib;
        public string RightThumbXLib;
        public string RightThumbYLib;

        public double MagnitudeLeft;
        public double MagnitudeRight;

        public double LeftThumbX;
        public double LeftThumbY;
        public double RightThumbX;
        public double RightThumbY;

        public double LeftThumbXPourcent;
        public double LeftThumbYPourcent;
        public double RightThumbXPourcent;
        public double RightThumbYPourcent;

        public MotorValues MotorValues;

        public ControllerModel()
        {
            this.MotorValues = new MotorValues();
        }

    }
      


}
