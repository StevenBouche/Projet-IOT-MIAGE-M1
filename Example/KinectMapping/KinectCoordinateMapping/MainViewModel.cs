namespace KinectCoordinateMapping
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Kinect;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    public class MainViewModel
    {
        private string _nameTypeJoint;
        public PlotModel GraphModel { get; set; }
        List<LineSeries> donnees;
        long start;

        public MainViewModel(string nameTypeJoint)
        {
            _nameTypeJoint = nameTypeJoint;
            GraphModel = new PlotModel();
            SetGraphAxesAndTitle();
            donnees = new List<LineSeries>();
            donnees.Add(new LineSeries());
            donnees.Add(new LineSeries());
            donnees.Add(new LineSeries());
            foreach (LineSeries l in donnees)
            {
                GraphModel.Series.Add(l); // Ajout de la série de points au PlotModel
            }
            this.start = DateTimeOffset.Now.ToUnixTimeMilliseconds(); 
           
        }

        private void SetGraphAxesAndTitle()
        {
            GraphModel.Title = "Coord 3D of "+ _nameTypeJoint; // Titre du graphique
            //////////Génération Axe X//////////
            LinearAxis abscisse = new LinearAxis();
            abscisse.Title = "Axe X";
            abscisse.Position = AxisPosition.Bottom;
            //////////Génération Axe Y//////////
            LinearAxis ordonnee = new LinearAxis();
            ordonnee.Title = "Axe Y";
            ordonnee.Position = AxisPosition.Left;
            //////////Ajout des axes au PlotModel//////////
            GraphModel.Axes.Add(abscisse);
            GraphModel.Axes.Add(ordonnee);
        }

        public void UpdatedData(Dictionary<string, CameraSpacePoint> squelette3D)
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long current = milliseconds - start;

            if(current > 50000)
            {
                if (squelette3D.ContainsKey(_nameTypeJoint))
                {
                    donnees[0].Points.RemoveAt(0);
                    donnees[1].Points.RemoveAt(0);
                    donnees[2].Points.RemoveAt(0);
                    donnees[0].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].X));
                    donnees[1].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].Y));
                    donnees[2].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].Z));
                }
            }
            else
            {
                if (squelette3D.ContainsKey(_nameTypeJoint))
                {
                    donnees[0].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].X));
                    donnees[1].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].Y));
                    donnees[2].Points.Add(new DataPoint(current, squelette3D[_nameTypeJoint].Z));
                }
            }
           
 
            GraphModel.InvalidatePlot(true);
        }

        private void GenerateData()
        {
            Random rndGenerator = new Random();
           
            //Génération de 50 points avec abscisse régulière et ordonnée aléatoire
            for (int abscisse = 0; abscisse < 50; abscisse++)
            {
                donnees[0].Points.Add(new DataPoint(abscisse, rndGenerator.Next(0, 100)));
            }
            for (int abscisse = 0; abscisse < 50; abscisse++)
            {
                donnees[1].Points.Add(new DataPoint(abscisse, rndGenerator.Next(0, 100)));
            }
            foreach(LineSeries l in donnees)
            {
                GraphModel.Series.Add(l); // Ajout de la série de points au PlotModel
            }
           
        }
    }
}
