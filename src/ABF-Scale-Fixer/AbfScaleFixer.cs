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
        private readonly int scaleFactorLocation;
        public byte[] bytes { get; private set; }
        public double ScaleFactor { get; private set; }
        public string AdcUnits { get; private set; }
        public readonly string AdcUnitsOriginal;

        const int bytesPerBlock = 512;

        public void SetScaleFactor(double scaleFactor)
        {
            byte[] newBytes = BitConverter.GetBytes((Single)scaleFactor);
            Array.Copy(newBytes, 0, bytes, scaleFactorLocation, newBytes.Length);
        }

        public AbfScaleFixer(string abfPath)
        {
            bytes = System.IO.File.ReadAllBytes(abfPath);
            string fileSignature = Encoding.UTF8.GetString(bytes, 0, 4);
            if (fileSignature != "ABF2")
                throw new ArgumentException("file must be an ABF2 file");

            // ADCSection is located at the block defined by a 32-bit integer at byte 92
            int adcSectionLocation = BitConverter.ToInt32(bytes, 92) * bytesPerBlock;
            Debug.WriteLine($"adcSectionLocation: [{adcSectionLocation}]");

            // fInstrumentScaleFactor (for Ch0) is a 16-bit floating point 40 bytes into ADCSection
            scaleFactorLocation = adcSectionLocation + 40;
            ScaleFactor = BitConverter.ToSingle(bytes, scaleFactorLocation);
            ScaleFactor = Math.Round(ScaleFactor, 5);
            Debug.WriteLine($"fInstrumentScaleFactor: [{ScaleFactor}]");

            // ADC units are in the string section, but at an index defined by lADCUnitsIndex
            // which is a byte location defined by a 32-bit integer 78 bytes into the ADCSection
            int AdcUnitsIndex = BitConverter.ToInt32(bytes, adcSectionLocation + 78);
            Debug.WriteLine($"AdcUnitsIndex: [{AdcUnitsIndex}]");

            // StringsSection is located at the block defined by a 32-bit integer at byte 220
            int stringsSectionLocation = BitConverter.ToInt32(bytes, 220) * bytesPerBlock;
            Debug.WriteLine($"stringsSectionLocation: [{stringsSectionLocation}]");

            // PLay with the bytes to create an array of indexed strings
            string rawString = Encoding.UTF8.GetString(bytes, stringsSectionLocation, 512);
            string[] usefulStrings = rawString.Split('\0').Where(x => !string.IsNullOrEmpty(x)).Skip(3).ToArray();

            // the adc units is at the index defined earlier
            AdcUnits = usefulStrings[AdcUnitsIndex];
            AdcUnitsOriginal = usefulStrings[AdcUnitsIndex];
            for (int i = 0; i <= AdcUnitsIndex; i++)
                Debug.WriteLine($"String index [{i}]: \"{usefulStrings[i]}\"");
        }

        public void Save(string filePath)
        {
            System.IO.File.WriteAllBytes(filePath, bytes);
        }
    }
}
