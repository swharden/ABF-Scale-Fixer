using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABF_Scale_Fixer
{
    public class AbfScaleFixer
    {
        public readonly string abfPath;
        public string abfFileName { get { return System.IO.Path.GetFileName(abfPath); } }
        public string abfID { get { return System.IO.Path.GetFileNameWithoutExtension(abfPath); } }
        public double fInstrumentScaleFactor
        {
            get
            {
                double scaleFactor = BitConverter.ToSingle(bytes, fInstrumentScaleFactorLocation);
                Debug.WriteLine($"Read scale factor: {scaleFactor}");
                scaleFactor = Math.Round(scaleFactor, 5);
                Debug.WriteLine($"Rounded to: {scaleFactor}");
                return scaleFactor;
            }
            set
            {
                Single newScaleFactor = (Single)value;
                byte[] newBytes = BitConverter.GetBytes(newScaleFactor);
                Debug.WriteLine($"Writing new scale factor: {newScaleFactor} ({newBytes.Length} bytes)");
                Array.Copy(newBytes, 0, bytes, fInstrumentScaleFactorLocation, newBytes.Length);
            }
        }

        private readonly int fInstrumentScaleFactorLocation;
        public byte[] bytes;

        public AbfScaleFixer(string abfPath)
        {
            Debug.WriteLine($"Loading: {abfPath}");
            bytes = System.IO.File.ReadAllBytes(abfPath);
            Debug.WriteLine($"Read {bytes.Length} bytes");
            AssertFormatIsABF2(bytes);
            fInstrumentScaleFactorLocation = GetByteLocationOf_fInstrumentScaleFactor(bytes);
        }

        private static void AssertFormatIsABF2(byte[] bytes)
        {
            string fileSignature = Encoding.UTF8.GetString(bytes, 0, 4);
            if (fileSignature != "ABF2")
                throw new ArgumentException("file must be an ABF2 file");
            else
                Debug.WriteLine("file is valid ABF2");
        }

        private int GetByteLocationOf_fInstrumentScaleFactor(byte[] bytes)
        {
            int bytesPerBlock = 512;

            // the block location of "ADCSection" noted by a 32-bit integer at byte 92
            int adcSectionBlock = BitConverter.ToInt32(bytes, 92);
            Debug.WriteLine($"adcSectionBlock: {adcSectionBlock}");
            int adcSectionLocation = adcSectionBlock * bytesPerBlock;
            Debug.WriteLine($"adcSectionLocation: {adcSectionLocation}");

            // fInstrumentScaleFactor is 16-bit floating point (short) 40 bytes into adcSection (assuming channel zero)
            int location = adcSectionLocation + 40;
            Debug.WriteLine($"fInstrumentScaleFactor Location: {adcSectionLocation}");
            return location;
        }

        public void Save(string filePath)
        {
            Debug.WriteLine($"Writing: {filePath}");
            System.IO.File.WriteAllBytes(filePath, bytes);
        }
    }
}
