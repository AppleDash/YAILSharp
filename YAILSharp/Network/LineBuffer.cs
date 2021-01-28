using System;
using System.Text;

namespace YAILSharp.Network
{
    public class LineBuffer
    {
        private byte[] _data = new byte[0];

        public void Append(byte[] data, int nBytes)
        {
            var newData = new byte[_data.Length + nBytes];
            
            Array.Copy(_data, newData, _data.Length);
            Array.Copy(data, 0, newData, _data.Length, nBytes);

            _data = newData;
        }

        public bool HasLine()
        {
            return Array.IndexOf(_data, (byte) '\n') != -1;
        }

        public string PopLine()
        {
            var newlineIndex = Array.IndexOf(_data, (byte) '\n');
            var lineBytes = new byte[newlineIndex];
            
            Array.Copy(_data, lineBytes, lineBytes.Length);

            var newData = new byte[_data.Length - lineBytes.Length - 1]; // extra -1 to get rid of the actual newline
            Array.Copy(_data, newlineIndex + 1, newData, 0, newData.Length);

            _data = newData;

            return Encoding.UTF8.GetString(lineBytes);
        }
    }
}