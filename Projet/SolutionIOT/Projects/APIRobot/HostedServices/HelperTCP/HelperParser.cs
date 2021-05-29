using System;
using System.Linq;

namespace APIRobot.HostedServices.HelperTCP
{
    public interface IStateBuffer
    {
        public int CurrentSizeAttemping { get; set; }
        public int RestSize { get => CurrentSizeAttemping - CurrentSizeBuffer; }
        public int CurrentSizeBuffer { get; set; }
        public byte[] Buffer { get; set; }
        public byte[] Temp { get; set; }
    }

    public class HelperParser
    {

        public static byte[] ProcessBytes(ref byte[] bytes, IStateBuffer state, Action onError, Action<byte[]> onNewData)
        {

            int sizeBytes = bytes.Length;
            int cursorBytes = 0;
            int nbBytes;
            int restBytes = 0;

            if (state.RestSize == 0)
            {

                if (state.Temp != null)
                {
                    bytes = state.Temp.Concat(bytes).ToArray();
                    sizeBytes = bytes.Length;
                    state.Temp = null;
                }

                if (sizeBytes < 4)
                {
                    state.Temp = null;
                    state.Temp = new byte[sizeBytes];
                    Buffer.BlockCopy(bytes, 0, state.Temp, 0, sizeBytes);
                    return null;
                }

                state.CurrentSizeAttemping = BitConverter.ToInt32(bytes, 0);
                state.CurrentSizeBuffer = 0;
                state.Buffer = null;

                if (state.CurrentSizeAttemping <= 0 )
                {
                    onError();
                    return null;
                }

                cursorBytes = 4;
                state.Buffer = new byte[state.CurrentSizeAttemping];
            }

            int totBytes = sizeBytes - cursorBytes;

            if (totBytes <= state.RestSize) nbBytes = totBytes;
            else
            {
                nbBytes = state.RestSize;
                restBytes = totBytes - state.RestSize;
            }

            Buffer.BlockCopy(bytes, cursorBytes, state.Buffer, state.CurrentSizeBuffer, nbBytes);
            state.CurrentSizeBuffer += nbBytes;

            if (state.RestSize == 0)
                onNewData(state.Buffer);

            if (restBytes > 0)
            {
                
                byte[] rest = new byte[restBytes];
                Buffer.BlockCopy(bytes, nbBytes + cursorBytes, rest, 0, restBytes);
                bytes = null;
                return rest;
            }
            else
            {
                bytes = null;
                return null;
            }
        }
    }
}
