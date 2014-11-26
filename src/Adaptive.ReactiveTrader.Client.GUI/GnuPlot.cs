﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Adaptive.ReactiveTrader.Client.Domain.Instrumentation;

namespace Adaptive.ReactiveTrader.Client
{
    public class GnuPlot : IGnuPlot
    {
        public string PathToGnuplot = @"C:\Program Files\gnuplot\bin";
        private readonly Process _extPro;
        private readonly StreamWriter _gnupStWr;
        private readonly List<StoredPlot> _plotBuffer;
        private readonly List<StoredPlot> _sPlotBuffer;
        private bool _replotWithSplot;

        public bool Hold { get; private set; }

        public GnuPlot()
        {
            if (PathToGnuplot[PathToGnuplot.Length - 1].ToString() != @"\")
                PathToGnuplot += @"\";
            _extPro = new Process();
            _extPro.StartInfo.FileName = PathToGnuplot + "gnuplot.exe";
            _extPro.StartInfo.UseShellExecute = false;
            _extPro.StartInfo.RedirectStandardInput = true;
            _extPro.Start();
            _gnupStWr = _extPro.StandardInput;
            _plotBuffer = new List<StoredPlot>();
            _sPlotBuffer = new List<StoredPlot>();
            Hold = false;
        }

        public void WriteLine(string gnuplotcommands)
        {

            _gnupStWr.WriteLine(gnuplotcommands);
            _gnupStWr.Flush();
        }

        public void Write(string gnuplotcommands)
        {
            _gnupStWr.Write(gnuplotcommands);
            _gnupStWr.Flush();
        }

        public void Set(params string[] options)
        {
            for (int i = 0; i < options.Length; i++)
                _gnupStWr.WriteLine("set " + options[i]);

        }

        public void Unset(params string[] options)
        {
            for (int i = 0; i < options.Length; i++)
                _gnupStWr.WriteLine("unset " + options[i]);
        }

        public bool SaveData(double[] Y, string filename)
        {
            StreamWriter dataStream = new StreamWriter(filename, false);
            WriteData(Y, dataStream);
            dataStream.Close();

            return true;
        }

        public bool SaveData(double[] X, double[] Y, string filename)
        {
            StreamWriter dataStream = new StreamWriter(filename, false);
            WriteData(X, Y, dataStream);
            dataStream.Close();

            return true;
        }

        public bool SaveData(double[] X, double[] Y, double[] Z, string filename)
        {
            StreamWriter dataStream = new StreamWriter(filename, false);
            WriteData(X, Y, Z, dataStream);
            dataStream.Close();

            return true;
        }

        public bool SaveData(int sizeY, double[] Z, string filename)
        {
            StreamWriter dataStream = new StreamWriter(filename, false);
            WriteData(sizeY, Z, dataStream);
            dataStream.Close();

            return true;
        }

        public bool SaveData(double[,] Z, string filename)
        {
            StreamWriter dataStream = new StreamWriter(filename, false);
            WriteData(Z, dataStream);
            dataStream.Close();

            return true;
        }

        public void Replot()
        {
            if (_replotWithSplot)
                SPlot(_sPlotBuffer);
            else
                Plot(_plotBuffer);
        }

        public void Plot(string filenameOrFunction, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(filenameOrFunction, options));
            Plot(_plotBuffer);
        }
        public void Plot(double[] y, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(y, options));
            Plot(_plotBuffer);
        }
        public void Plot(double[] x, double[] y, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(x, y, options));
            Plot(_plotBuffer);
        }

        public void Contour(string filenameOrFunction, string options = "", bool labelContours = true)
        {
            if (!Hold) _plotBuffer.Clear();
            var p = new StoredPlot(filenameOrFunction, options, PlotTypes.ContourFileOrFunction);
            p.LabelContours = labelContours;
            _plotBuffer.Add(p);
            Plot(_plotBuffer);
        }
        public void Contour(int sizeY, double[] z, string options = "", bool labelContours = true)
        {
            if (!Hold) _plotBuffer.Clear();
            var p = new StoredPlot(sizeY, z, options, PlotTypes.ContourZ);
            p.LabelContours = labelContours;
            _plotBuffer.Add(p);
            Plot(_plotBuffer);
        }
        public void Contour(double[] x, double[] y, double[] z, string options = "", bool labelContours = true)
        {
            if (!Hold) _plotBuffer.Clear();
            var p = new StoredPlot(x, y, z, options, PlotTypes.ContourXYZ);
            p.LabelContours = labelContours;
            _plotBuffer.Add(p);
            Plot(_plotBuffer);
        }
        public void Contour(double[,] zz, string options = "", bool labelContours = true)
        {
            if (!Hold) _plotBuffer.Clear();
            var p = new StoredPlot(zz, options, PlotTypes.ContourZZ);
            p.LabelContours = labelContours;
            _plotBuffer.Add(p);
            Plot(_plotBuffer);
        }

        public void HeatMap(string filenameOrFunction, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(filenameOrFunction, options, PlotTypes.ColorMapFileOrFunction));
            Plot(_plotBuffer);
        }
        public void HeatMap(int sizeY, double[] intensity, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(sizeY, intensity, options, PlotTypes.ColorMapZ));
            Plot(_plotBuffer);
        }
        public void HeatMap(double[] x, double[] y, double[] intensity, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(x, y, intensity, options, PlotTypes.ColorMapXYZ));
            Plot(_plotBuffer);
        }
        public void HeatMap(double[,] intensityGrid, string options = "")
        {
            if (!Hold) _plotBuffer.Clear();
            _plotBuffer.Add(new StoredPlot(intensityGrid, options, PlotTypes.ColorMapZZ));
            Plot(_plotBuffer);
        }

        public void SPlot(string filenameOrFunction, string options = "")
        {
            if (!Hold) _sPlotBuffer.Clear();
            _sPlotBuffer.Add(new StoredPlot(filenameOrFunction, options,PlotTypes.SplotFileOrFunction));
            SPlot(_sPlotBuffer);
        }
        public void SPlot(int sizeY, double[] z, string options = "")
        {
            if (!Hold) _sPlotBuffer.Clear();
            _sPlotBuffer.Add(new StoredPlot(sizeY, z, options));
            SPlot(_sPlotBuffer);
        }

        public void SPlot(double[] x, double[] y, double[] z, string options = "")
        {
            if (!Hold) _sPlotBuffer.Clear();
            _sPlotBuffer.Add(new StoredPlot(x, y, z, options));
            SPlot(_sPlotBuffer);
        }

        public void SPlot(double[,] zz, string options = "")
        {
            if (!Hold) _sPlotBuffer.Clear();
            _sPlotBuffer.Add(new StoredPlot(zz, options));
            SPlot(_sPlotBuffer);
        }


        public void Plot(List<StoredPlot> storedPlots)
        {
            _replotWithSplot = false;
            string plot = "plot ";
            string plotstring = "";
            string contfile;
            string defcntopts;
            removeContourLabels();
            for (int i = 0; i < storedPlots.Count; i++)
            {
                var p = storedPlots[i];
                defcntopts = (p.Options.Length > 0 && (p.Options.Contains(" w") || p.Options[0] == 'w')) ? " " : " with lines ";
                switch (p.PlotType)
                {
                    case PlotTypes.PlotFileOrFunction:
                        if (p.File != null)
                            plotstring += (plot + plotPath(p.File) + " " + p.Options);
                        else
                            plotstring += (plot + p.Function + " " + p.Options);
                        break;
                    case PlotTypes.PlotXY:
                    case PlotTypes.PlotY:
                        plotstring += (plot + @"""-"" " + p.Options);
                        break;
                    case PlotTypes.ContourFileOrFunction:
                        contfile = Path.GetTempPath() + "_cntrtempdata" + i + ".dat";
                        makeContourFile((p.File != null ? plotPath(p.File) : p.Function), contfile);
                        if (p.LabelContours) setContourLabels(contfile);
                        plotstring += (plot + plotPath(contfile) + defcntopts + p.Options);
                        break;
                    case PlotTypes.ContourXYZ:
                        contfile = Path.GetTempPath() + "_cntrtempdata" + i + ".dat";
                        makeContourFile(p.X, p.Y, p.Z, contfile);
                        if (p.LabelContours) setContourLabels(contfile);
                        plotstring += (plot + plotPath(contfile) + defcntopts + p.Options);
                        break;
                    case PlotTypes.ContourZZ:
                        contfile = Path.GetTempPath() + "_cntrtempdata" + i + ".dat";
                        makeContourFile(p.ZZ, contfile);
                        if (p.LabelContours) setContourLabels(contfile);
                        plotstring += (plot + plotPath(contfile) + defcntopts + p.Options);
                        break;
                    case PlotTypes.ContourZ:
                        contfile = Path.GetTempPath() + "_cntrtempdata" + i + ".dat";
                        makeContourFile(p.YSize, p.Z, contfile);
                        if (p.LabelContours) setContourLabels(contfile);
                        plotstring += (plot + plotPath(contfile) + defcntopts + p.Options);
                        break;


                    case PlotTypes.ColorMapFileOrFunction:
                        if (p.File != null)
                            plotstring += (plot + plotPath(p.File) + " with image " + p.Options);
                        else
                            plotstring += (plot + p.Function + " with image " + p.Options);
                        break;
                    case PlotTypes.ColorMapXYZ:
                    case PlotTypes.ColorMapZ:
                        plotstring += (plot + @"""-"" " + " with image " + p.Options);
                        break;
                    case PlotTypes.ColorMapZZ:
                        plotstring += (plot + @"""-"" " + "matrix with image " + p.Options);
                        break;
                }
                if (i == 0) plot = ", ";
            }
            _gnupStWr.WriteLine(plotstring);

            for (int i = 0; i < storedPlots.Count; i++)
            {
                var p = storedPlots[i];
                switch (p.PlotType)
                {
                    case PlotTypes.PlotXY:
                        WriteData(p.X, p.Y, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.PlotY:
                        WriteData(p.Y, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.ColorMapXYZ:
                        WriteData(p.X, p.Y, p.Z, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.ColorMapZ:
                        WriteData(p.YSize, p.Z, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.ColorMapZZ:
                        WriteData(p.ZZ, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        _gnupStWr.WriteLine("e");
                        break;
                    
                }
            }
            _gnupStWr.Flush();
        }

        public void SPlot(List<StoredPlot> storedPlots)
        {
            _replotWithSplot = true;
            var splot = "splot ";
            string plotstring = "";
            string defopts = "";
            removeContourLabels();
            for (int i = 0; i < storedPlots.Count; i++)
            {
                var p = storedPlots[i];
                defopts = (p.Options.Length > 0 && (p.Options.Contains(" w") || p.Options[0] == 'w')) ? " " : " with lines ";
                switch (p.PlotType)
                {
                    case PlotTypes.SplotFileOrFunction:
                        if (p.File != null)
                            plotstring += (splot + plotPath(p.File) + defopts + p.Options);
                        else
                            plotstring += (splot + p.Function + defopts + p.Options);
                        break;
                    case PlotTypes.SplotXYZ:
                    case PlotTypes.SplotZ:
                        plotstring += (splot + @"""-"" " + defopts + p.Options);
                        break;
                    case PlotTypes.SplotZZ:
                        plotstring += (splot + @"""-"" matrix " + defopts + p.Options);
                        break;
                }
                if (i == 0) splot = ", ";
            }
            _gnupStWr.WriteLine(plotstring);

            for (int i = 0; i < storedPlots.Count; i++)
            {
                var p = storedPlots[i];
                switch (p.PlotType)
                {
                    case PlotTypes.SplotXYZ:
                        WriteData(p.X, p.Y, p.Z, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.SplotZZ:
                        WriteData(p.ZZ, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        _gnupStWr.WriteLine("e");
                        break;
                    case PlotTypes.SplotZ:
                        WriteData(p.YSize, p.Z, _gnupStWr, false);
                        _gnupStWr.WriteLine("e");
                        break;
                }
            }
            _gnupStWr.Flush();
        }

        public void WriteData(double[] y, StreamWriter stream, bool flush = true)
        {
            for (int i = 0; i < y.Length; i++)
                stream.WriteLine(y[i].ToString());

            if (flush) stream.Flush();
        }

        public void WriteData(double[] x, double[] y, StreamWriter stream, bool flush = true)
        {
            for (int i = 0; i < y.Length; i++)
                stream.WriteLine(x[i].ToString() + " " + y[i].ToString());

            if (flush) stream.Flush();
        }

        public void WriteData(int ySize, double[] z, StreamWriter stream, bool flush = true)
        {
            for (int i = 0; i < z.Length; i++)
            {
                if (i > 0 && i % ySize == 0)
                    stream.WriteLine();
                stream.WriteLine(z[i].ToString());
            }

            if (flush) stream.Flush();
        }

        public void WriteData(double[,] zz, StreamWriter stream, bool flush = true)
        {
            int m = zz.GetLength(0);
            int n = zz.GetLength(1);
            string line;
            for (int i = 0; i < m; i++)
            {
                line = "";
                for (int j = 0; j < n; j++)
                    line += zz[i, j].ToString() + " ";
                stream.WriteLine(line.TrimEnd());
            }

            if (flush) stream.Flush();
        }

        public void WriteData(double[] x, double[] y, double[] z, StreamWriter stream, bool flush = true)
        {
            int m = Math.Min(x.Length, y.Length);
            m = Math.Min(m, z.Length);
            for (int i = 0; i < m; i++)
            {
                if (i > 0 && x[i] != x[i - 1])
                    stream.WriteLine("");
                stream.WriteLine(x[i] + " " + y[i] + " " + z[i]);
            }

            if (flush) stream.Flush();
        }

        string plotPath(string path)
        {
            return "\"" + path.Replace(@"\", @"\\") + "\"";
        }

        public void SaveSetState(string filename = null)
        {
            if (filename == null)
                filename = Path.GetTempPath() + "setstate.tmp";
            _gnupStWr.WriteLine("save set " + plotPath(filename));
            _gnupStWr.Flush();
            waitForFile(filename);
        }
        public void LoadSetState(string filename = null)
        {
            if (filename == null)
                filename = Path.GetTempPath() + "setstate.tmp";
            _gnupStWr.WriteLine("load " + plotPath(filename));
            _gnupStWr.Flush();
        }

        //these makecontourFile functions should probably be merged into one function and use a StoredPlot parameter
        void makeContourFile(string fileOrFunction, string outputFile)//if it's a file, fileOrFunction needs quotes and escaped backslashes
        {
            SaveSetState();
            Set("table " + plotPath(outputFile));
            Set("contour base");
            Unset("surface");
            _gnupStWr.WriteLine(@"splot " + fileOrFunction);
            Unset("table");
            _gnupStWr.Flush();
            LoadSetState();
            waitForFile(outputFile);
        }

        void makeContourFile(double[] x, double[] y, double[] z, string outputFile)
        {
            SaveSetState();
            Set("table " + plotPath(outputFile));
            Set("contour base");
            Unset("surface");
            _gnupStWr.WriteLine(@"splot ""-""");
            WriteData(x, y, z, _gnupStWr);
            _gnupStWr.WriteLine("e");
            Unset("table");
            _gnupStWr.Flush();
            LoadSetState();
            waitForFile(outputFile);
        }

        void makeContourFile(double[,] zz, string outputFile)
        {
            SaveSetState();
            Set("table " + plotPath(outputFile));
            Set("contour base");
            Unset("surface");
            _gnupStWr.WriteLine(@"splot ""-"" matrix");
            WriteData(zz, _gnupStWr);
            _gnupStWr.WriteLine("e");
            _gnupStWr.WriteLine("e");
            Unset("table");
            _gnupStWr.Flush();
            LoadSetState();
            waitForFile(outputFile);
        }

        void makeContourFile(int sizeY, double[] z, string outputFile)
        {
            SaveSetState();
            Set("table " + plotPath(outputFile));
            Set("contour base");
            Unset("surface");
            _gnupStWr.WriteLine(@"splot ""-""");
            WriteData(sizeY, z, _gnupStWr);
            _gnupStWr.WriteLine("e");
            Unset("table");
            _gnupStWr.Flush();
            LoadSetState();
            waitForFile(outputFile);
        }

        int contourLabelCount = 50000;
        void setContourLabels(string contourFile)
        {
            var file = new System.IO.StreamReader(contourFile);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("label:"))
                {
                    string[] c = file.ReadLine().Trim().Replace("   ", " ").Replace("  ", " ").Split(' ');
                    _gnupStWr.WriteLine("set object " + ++contourLabelCount + " rectangle center " + c[0] + "," + c[1] + " size char " + (c[2].ToString().Length + 1) + ",char 1 fs transparent solid .7 noborder fc rgb \"white\"  front");
                    _gnupStWr.WriteLine("set label " + contourLabelCount + " \"" + c[2] + "\" at " + c[0] + "," + c[1] + " front center");
                }
            }
            file.Close();
        }
        void removeContourLabels()
        {
            while (contourLabelCount > 50000)
                _gnupStWr.WriteLine("unset object " + contourLabelCount + ";unset label " + contourLabelCount--);
        }

        bool waitForFile(string filename, int timeout = 10000)
        {
            Thread.Sleep(20);
            int attempts = timeout / 100;
            System.IO.StreamReader file = null;
            while (file == null)
            {
                try { file = new System.IO.StreamReader(filename); }
                catch
                {
                    if (attempts-- > 0)
                        Thread.Sleep(100);
                    else
                        return false;
                }
            }
            file.Close();
            return true;
        }

        public void HoldOn()
        {
            Hold = true;
            _plotBuffer.Clear();
            _sPlotBuffer.Clear();
        }

        public void HoldOff()
        {
            Hold = false;
            _plotBuffer.Clear();
            _sPlotBuffer.Clear();
        }

        public void Close()
        {
            _extPro.CloseMainWindow();
        }

        public void WriteFile(string filename, string content)
        {
            File.WriteAllText(filename, content);
        }
    }
}
