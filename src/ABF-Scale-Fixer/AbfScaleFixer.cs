using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABF_Scale_Fixer
{
    /* This class reads the entire binary content of an ABF file in memory. 
     * It then notes where the byte locations are for scale factor and ADC units.
     * Modifcations occur in memory, aand later bytes can be saved to a file.
     */
    public class AbfScaleFixer
    {
        public readonly string abfPath;
        public string abfFileName { get { return System.IO.Path.GetFileName(abfPath); } }
        public string abfID { get { return System.IO.Path.GetFileNameWithoutExtension(abfPath); } }
        private readonly int fInstrumentScaleFactorLocation;
        public byte[] bytes { get; private set; }
        public double ScaleFactor { get; private set; }

        public void SetScaleFactor(double scaleFactor)
        {
            ScaleFactor = scaleFactor;
            Single newScaleFactor = (Single)scaleFactor;
            byte[] newBytes = BitConverter.GetBytes(newScaleFactor);
            Debug.WriteLine($"Writing new scale factor: {newScaleFactor} ({newBytes.Length} bytes)");
            Array.Copy(newBytes, 0, bytes, fInstrumentScaleFactorLocation, newBytes.Length);
        }

        public AbfScaleFixer(string abfPath)
        {
            Debug.WriteLine($"Loading: {abfPath}");
            bytes = System.IO.File.ReadAllBytes(abfPath);
            Debug.WriteLine($"Read {bytes.Length} bytes");
            AssertFormatIsABF2(bytes);
            fInstrumentScaleFactorLocation = GetByteLocationOf_fInstrumentScaleFactor(bytes);
            ScaleFactor = BitConverter.ToSingle(bytes, fInstrumentScaleFactorLocation);
            ScaleFactor = Math.Round(ScaleFactor, 5);
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
