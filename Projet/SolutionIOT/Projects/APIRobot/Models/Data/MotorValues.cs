

namespace APIRobot.Models.Data
{
    public class MotorValues
    {
        public long MotorL { get; set; }
        public long MotorR { get; set; }
        public long Timestamp { get; set; }

        public MotorValues()
        {
            MotorL = 0;
            MotorR = 0;
        }
    }
}
