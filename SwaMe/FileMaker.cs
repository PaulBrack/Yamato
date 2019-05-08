using Json.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SwaMe
{
    class FileMaker
    {
        
        public void MakeMetricsPerSwathFile(int numOfSwaths, MzmlParser.Run run, List<int> numOfSwathPerGroup, List<double> AveMzRange, double totalTIC, List<double> TICs, List<double> swDensity50, List<double> swDensityIQR)
        {
            //tsv
            StreamWriter sm = new StreamWriter("MetricsBySwath.tsv");
            sm.Write("Filename \t swathNumber \t scansPerSwath \t AveMzRange \t TICpercentageOfSwath \t swDensityAverage \t swDensityIQR  \n");
            for (int num = 0; num < numOfSwaths; num++)
            {
                sm.Write(run.SourceFileName);
                sm.Write("\t");
                sm.Write("Swathnumber");
                sm.Write(num + 1);
                sm.Write("\t");
                sm.Write(numOfSwathPerGroup.ElementAt(num));
                sm.Write("\t");
                sm.Write(AveMzRange.ElementAt(num));
                sm.Write("\t");
                sm.Write((TICs[num] / totalTIC) * 100);
                sm.Write("\t");
                sm.Write(swDensity50[num]);
                sm.Write("\t");
                sm.Write(swDensityIQR[num]);
                sm.Write("\n");
            }
            sm.Close();

        }
        public void MakeMetricsPerRTsegmentFile(MzmlParser.Run run, List<List<double>> Peakwidths, List<List<double>> PeakSymmetry, List<List<double>> PeakCapacity, List<List<double>> PeakPrecision, List<List<double>> MS1Peakprecision,
            List<double> cycleTime, List<double> TICchange50List, List<double> TICchangeIQRList, List<List<int>> MS1Density, List<List<int>> MS2Density, List<double> MS1TICTotal, List<double> MS2TICTotal, int division)
        {

            StreamWriter rM = new StreamWriter("RTDividedMetrics.tsv");
            rM.Write("Filename\t RTsegment \t MS2Peakwidths \t PeakSymmetry \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAverage \t DeltaTICIQR \t AveScanTime \t AveMS2Density \t AveMS1Density \t MS2TICTotal \t MS1TICTotal");

            for (int segment = 0; segment < division; segment++)
            {
                //write rM
                rM.Write("\n");
                rM.Write(run.SourceFileName);
                rM.Write("\t");
                rM.Write("RTsegment");
                rM.Write(segment);
                rM.Write(" \t ");
                rM.Write(Peakwidths.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakSymmetry.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakCapacity.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakPrecision.ElementAt(segment).Average().ToString());
                rM.Write("\t");
                rM.Write(MS1Peakprecision.ElementAt(segment).Average().ToString());
                rM.Write("\t");
                rM.Write(TICchange50List.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(TICchangeIQRList.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(cycleTime.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(MS2Density.ElementAt(segment).Average());
                rM.Write(" \t ");
                rM.Write(MS1Density.ElementAt(segment).Average());
                rM.Write(" \t ");
                rM.Write(MS1TICTotal.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(MS2TICTotal.ElementAt(segment));
                rM.Write(" \t ");
            }
            rM.Close();
        }
        public void MakeUndividedMetricsFile(MzmlParser.Run run, double RTDuration, double swathSizeDifference, int MS2Count, int NumOfSwaths, double cycleTimes50, double cycleTimesIQR, int MS2Density50, int MS2DensityIQR, int MS1Count)
        {
            StreamWriter um = new StreamWriter("undividedMetrics.tsv");
            um.Write("Filename \t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t CycleTimes50 \t CycleTimesIQR \t MS2Density50 \t MS2DensityIQR \t MS1Count");
            um.Write("\n");
            um.Write(run.SourceFileName);
            um.Write("\t");
            um.Write(RTDuration);
            um.Write("\t");
            um.Write(swathSizeDifference);
            um.Write("\t");
            um.Write(MS2Count);
            um.Write("\t");
            um.Write(NumOfSwaths);
            um.Write("\t");
            um.Write(cycleTimes50);
            um.Write("\t");
            um.Write(cycleTimesIQR);
            um.Write("\t");
            um.Write(MS2Density50);
            um.Write("\t");
            um.Write(MS2DensityIQR);
            um.Write("\t");
            um.Write(MS1Count);
            um.Close();
        }

        public void MakeJSON(string path, MzmlParser.Run run, double RTDuration, double swathSizeDifference, int MS2Count, int NumOfSwaths, double cycleTimes50, double cycleTimesIQR,int MS2Density50, int MS2DensityIQR, int MS1Count)
        {
            //Declare units:
            JSON_classes.Unit Count = new JSON_classes.Unit("UO", "UO:0000189", "count");
            JSON_classes.Unit Second = new JSON_classes.Unit("UO", "UO:0000010", "second");
            JSON_classes.Unit mZ = new JSON_classes.Unit("UO", "UO:XXXXXXX", "m/z");// I know this one doesn't exist, put it here as a placeholder until I figure out what to do with it.
            JSON_classes.Unit Hertz = new JSON_classes.Unit("UO", "UO:0000106", "Hertz");

            //Start with the long part: adding all the metrics
            JSON_classes.QualityParameters[] qP = new JSON_classes.QualityParameters[32];
            qP[0] = new JSON_classes.QualityParameters("QC", "QC:4000053", "Quameter metric: RT-Duration", Second, RTDuration);
            qP[1] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: swathSizeDifference", mZ, swathSizeDifference);
            qP[2] = new JSON_classes.QualityParameters("QC", "QC:4000060", "Quameter metric: MS2-Count", Count, MS2Count);
            qP[3] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: NumOfSwaths", Count, NumOfSwaths);
            qP[4] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: CycleTimes50", Hertz, cycleTimes50);
            qP[5] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: CycleTimesIQR", Hertz, cycleTimesIQR);
            qP[6] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: MS2Density50", Count, MS2Density50);
            qP[6] = new JSON_classes.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: MS2DensityIQR", Count, MS2DensityIQR);
            qP[7] = new JSON_classes.QualityParameters("QC", "QC:4000059", "Quameter metric: MS1-Count", Count, MS1Count);

            //Now for the other stuff
            JSON_classes.InputFiles iF = new JSON_classes.InputFiles(path, run.SourceFileName);
            JSON_classes.MetaData mD = new JSON_classes.MetaData(iF);
            JSON_classes.RunQuality rQ = new JSON_classes.RunQuality(mD, qP);
            JSON_classes.NUV QC = new JSON_classes.NUV("Proteomics Standards Initiative Quality Control Ontology", "https://github.com/HUPO-PSI/qcML-development/blob/master/cv/v0_0_11/qc-cv.obo", "0.1.0");
            JSON_classes.NUV MS = new JSON_classes.NUV("Proteomics Standards Initiative Mass Spectrometry Ontology", "https://github.com/HUPO-PSI/psi-ms-CV/blob/master/psi-ms.obo", "4.1.7");
            JSON_classes.NUV OU = new JSON_classes.NUV("Unit Ontology", "https://github.com/bio-ontology-research-group/unit-ontology/blob/master/unit.obo", "09:04:2014 13:37");
            JSON_classes.CV cv = new JSON_classes.CV(QC, MS, OU);
            JSON_classes.MzQC metrics = new JSON_classes.MzQC(rQ, cv);
            //Then print:
            string output = JsonConvert.SerializeObject(metrics);
            using (StreamWriter file = File.CreateText(@"metrics.json"))
            {
                file.Write("mzQC:");
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Serialize(file, metrics);
            }

        }


       
    }
}
