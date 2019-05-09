using Json.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SwaMe
{
    class FileMaker
    {
        int division;
        int MS2Density50;
        int MS2DensityIQR;
        int MS1Count;
        int MS2Count;
        double cycleTimes50;
        double cycleTimesIQR;
        string inputFilePath;
        double RTDuration;
        double swathSizeDifference;
        MzmlParser.Run run; 
        SwathGrouper.SwathMetrics sM;
        RTGrouper.RTMetrics rM;
        
        public void MakeMetricsPerSwathFile(SwathGrouper.SwathMetrics sM)
        {
            //tsv
            StreamWriter sm = new StreamWriter("MetricsBySwath.tsv");
            sm.Write("Filename \t swathNumber \t scansPerSwath \t AveMzRange \t TICpercentageOfSwath \t swDensityAverage \t swDensityIQR  \n");

            for (int num = 0; num < sM.maxswath; num++)
            {
                sm.Write(run.SourceFileName);
                sm.Write("\t");
                sm.Write("Swathnumber");
                sm.Write(num + 1);
                sm.Write("\t");
                sm.Write(sM.numOfSwathPerGroup.ElementAt(num));
                sm.Write("\t");
                sm.Write(sM.AveMzRange.ElementAt(num));
                sm.Write("\t");
                sm.Write(sM.TicPercentage.ElementAt(num));
                sm.Write("\t");
                sm.Write(sM.swDensity50[num]);
                sm.Write("\t");
                sm.Write(sM.swDensityIQR[num]);
                sm.Write("\n");
            }
            sm.Close();

        }
        public void MakeMetricsPerRTsegmentFile(RTGrouper.RTMetrics rM)
        {

            StreamWriter rS = new StreamWriter("RTDividedMetrics.tsv");
            rS.Write("Filename\t RTsegment \t MS2Peakwidths \t PeakSymmetry \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAverage \t DeltaTICIQR \t AveScanTime \t AveMS2Density \t AveMS1Density \t MS2TICTotal \t MS1TICTotal");

            for (int segment = 0; segment < division; segment++)
            {
                //write rS
                rS.Write("\n");
                rS.Write(run.SourceFileName);
                rS.Write("\t");
                rS.Write("RTsegment");
                rS.Write(segment);
                rS.Write(" \t ");
                rS.Write(rM.Peakwidths.ElementAt(segment).ToString());
                rS.Write(" \t ");
                rS.Write(rM.PeakSymmetry.ElementAt(segment).ToString());
                rS.Write(" \t ");
                rS.Write(rM.PeakCapacity.ElementAt(segment).ToString());
                rS.Write(" \t ");
                rS.Write(rM.PeakPrecision.ElementAt(segment).ToString());
                rS.Write("\t");
                rS.Write(rM.MS1PeakPrecision.ElementAt(segment).ToString());
                rS.Write("\t");
                rS.Write(rM.TICchange50List.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.TICchangeIQRList.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.cycleTime.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.MS2Density.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.MS1Density.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.MS1TICTotal.ElementAt(segment));
                rS.Write(" \t ");
                rS.Write(rM.MS2TICTotal.ElementAt(segment));
                rS.Write(" \t ");
            }
            rS.Close();
        }
        public void MakeUndividedMetricsFile( )
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
            um.Write(sM.maxswath);
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

        public void MakeJSON()
        {
            //Declare units:
            JsonClasses.Unit Count = new JsonClasses.Unit("UO", "UO:0000189", "count");
            JsonClasses.Unit Second = new JsonClasses.Unit("UO", "UO:0000010", "second");
            JsonClasses.Unit mZ = new JsonClasses.Unit("UO", "UO:XXXXXXX", "m/z");// I know this one doesn't exist, put it here as a placeholder until I figure out what to do with it.
            JsonClasses.Unit Hertz = new JsonClasses.Unit("UO", "UO:0000106", "Hertz");
            JsonClasses.Unit MzPercentage = new JsonClasses.Unit("UO", "UO:XXXXXXXX", "m/z percentage");//Also doesn't exist, need to re-evaluate...
            JsonClasses.Unit Intensity = new JsonClasses.Unit("UO", "UO:XXXXXXX", "Counts per second");//Also doesn't exist in the UO obo...

            //Start with the long part: adding all the metrics
            JsonClasses.QualityParameters[] qP = new JsonClasses.QualityParameters[25];
            qP[0] = new JsonClasses.QualityParameters("QC", "QC:4000053", "Quameter metric: RT-Duration", Second, RTDuration);
            qP[1] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: swathSizeDifference", mZ, swathSizeDifference);
            qP[2] = new JsonClasses.QualityParameters("QC", "QC:4000060", "Quameter metric: MS2-Count", Count, MS2Count);
            qP[3] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: NumOfSwaths", Count, sM.maxswath);
            qP[4] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: CycleTimes50", Hertz, cycleTimes50);
            qP[5] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: CycleTimesIQR", Hertz, cycleTimesIQR);
            qP[6] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: MS2Density50", Count, MS2Density50);
            qP[7] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXX", "SwaMe metric: MS2DensityIQR", Count, MS2DensityIQR);
            qP[8] = new JsonClasses.QualityParameters("QC", "QC:4000059", "Quameter metric: MS1-Count", Count, MS1Count);
            qP[9] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: scansPerSwathGroup", Count, sM.numOfSwathPerGroup);
            qP[10] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: AveMzRange", mZ, sM.AveMzRange);
            qP[11] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: TICpercentageOfSwath", MzPercentage, sM.TicPercentage);
            qP[12] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: swDensity50", Count, sM.swDensity50);
            qP[13] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: swDensityIQR", Count, sM.swDensityIQR);
            qP[14] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: Peakwidths", Second, rM.Peakwidths);
            qP[15] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: PeakCapacity", Count, rM.PeakCapacity); 
            qP[16] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: PeakSymmetry", Count, rM.PeakSymmetry);
            qP[17] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS2PeakPrecision", mZ, rM.PeakPrecision);
            qP[17] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS1PeakPrecision", mZ, rM.MS1PeakPrecision);
            qP[18] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: DeltaTICAverage", Intensity , rM.TICchange50List);
            qP[19] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: DeltaTICIQR", Intensity, rM.TICchangeIQRList);
            qP[20] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: AveScanTime", Second, rM.cycleTime);
            qP[21] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS2Density", Count, rM.MS2Density);
            qP[22] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS1Density", Count, rM.MS1Density);
            qP[23] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS2TICTotal", Count, rM.MS2TICTotal);
            qP[24] = new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS1TICTotal", Count, rM.MS1TICTotal);




            //Now for the other stuff
            JsonClasses.InputFiles iF = new JsonClasses.InputFiles(inputFilePath, run.SourceFileName);
            JsonClasses.MetaData mD = new JsonClasses.MetaData(iF);
            JsonClasses.RunQuality rQ = new JsonClasses.RunQuality(mD, qP);
            JsonClasses.NUV QC = new JsonClasses.NUV("Proteomics Standards Initiative Quality Control Ontology", "https://github.com/HUPO-PSI/qcML-development/blob/master/cv/v0_0_11/qc-cv.obo", "0.1.0");
            JsonClasses.NUV MS = new JsonClasses.NUV("Proteomics Standards Initiative Mass Spectrometry Ontology", "https://github.com/HUPO-PSI/psi-ms-CV/blob/master/psi-ms.obo", "4.1.7");
            JsonClasses.NUV OU = new JsonClasses.NUV("Unit Ontology", "https://github.com/bio-ontology-research-group/unit-ontology/blob/master/unit.obo", "09:04:2014 13:37");
            JsonClasses.CV cv = new JsonClasses.CV(QC, MS, OU);
            JsonClasses.MzQC metrics = new JsonClasses.MzQC(rQ, cv);
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

        public FileMaker(int division, string inputFilePath, MzmlParser.Run run, SwathGrouper.SwathMetrics sM, RTGrouper.RTMetrics rM,double RTDuration, double swathSizeDifference, int MS2Count, double cycleTimes50, double cycleTimesIQR, int MS2Density50, int MS2DensityIQR, int MS1Count)
        {
            this.sM = sM;
            this.division = division;
            this.inputFilePath = inputFilePath;
            this.run = run;
            this.rM = rM;
            this.RTDuration = RTDuration;
            this.swathSizeDifference = swathSizeDifference;
            this.MS2Count = MS2Count;
            this.cycleTimes50 = cycleTimes50;
            this.cycleTimesIQR = cycleTimesIQR;
            this.MS2Density50 = MS2Density50;
            this.MS2DensityIQR = MS2DensityIQR;
            this.MS1Count = MS1Count;
        }
       
    }
}
