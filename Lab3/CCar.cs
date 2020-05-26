using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace Lab3
{
    public class CCar : CMapObject
    {
        private PointLatLng point;
        private CRoute route;
        private CHuman pass;

        GMapMarker carMarker;
        GMapControl gMap = null;

        List<PointLatLng> epoints = new List<PointLatLng>();

        // событие прибытия
        public event EventHandler Arrived;

        public CCar(string title, PointLatLng point, GMapControl map) : base(title)
        {
            this.point = point;
            pass = null;

            gMap = map;
        }

        public void setPosition(PointLatLng point)
        {
            this.point = point;
        }

        public override double getDistance(PointLatLng point)
        {
            GeoCoordinate p1 = new GeoCoordinate(point.Lat, point.Lng);
            GeoCoordinate p2 = new GeoCoordinate(this.point.Lat, this.point.Lng);

            return p1.GetDistanceTo(p2);
        }

        public override PointLatLng getFocus()
        {
            return point;
        }

        public override GMapMarker getMarker()
        {
            GMapMarker marker = new GMapMarker(point)
            {
                Shape = new Image
                {
                    Width = 40, // ширина маркера
                    Height = 40, // высота маркера
                    ToolTip = this.getTitle(), // всплывающая подсказка
                    Source = new BitmapImage(new Uri("pack://application:,,,/image/bible.png")) // картинка
                }
            };

            carMarker = marker;

            return marker;
        }

        public GMapMarker moveTo(PointLatLng dest)
        {
            // провайдер навигации
            RoutingProvider routingProvider = GMapProviders.OpenStreetMap;
            // определение маршрута
             MapRoute route = routingProvider.GetRoute(
             point, // начальная точка маршрута
             dest, // конечная точка маршрута
             false, // поиск по шоссе (false - включен)
             false, // режим пешехода (false - выключен)
             (int)15);

            // получение точек маршрута
            List<PointLatLng> routePoints = route.Points;

            this.route = new CRoute("", routePoints);

            double dist = 0;
            double minDist = 0.00001;

            List<PointLatLng> points = this.route.getPoints();
            epoints.Clear();
            

            for (int i = 0; i < points.Count-1; i++)
            {
                dist = Vector2.Distance(new Vector2((float)points[i].Lat, (float)points[i].Lng), new Vector2((float)points[i+1].Lat, (float)points[i+1].Lng));
                if (dist > minDist)
                {
                    double aPoints = dist / minDist;
                    for (int j = 0; j < aPoints; j++)
                    {
                        Vector2 t = Vector2.Lerp(new Vector2((float)points[i].Lat, (float)points[i].Lng), new Vector2((float)points[i + 1].Lat, (float)points[i + 1].Lng), (float)(j / aPoints));

                        epoints.Add(new PointLatLng(t.X, t.Y));
                    }
                }
            }


            Thread newThread = new Thread(new ThreadStart(MoveByRoute));
            newThread.Start();
           
            return this.route.getMarker();
        }

        private void MoveByRoute()
        {
            // последовательный перебор точек маршрута
            foreach (var point in epoints) //route.getPoints())
            {
                // делегат, возвращающий управление в главный поток
                Application.Current.Dispatcher.Invoke(delegate {
                    // изменение позиции маркера
                    carMarker.Position = point;
                    this.point = point;

                    if (pass != null)
                    {
                        pass.setPosition(point);
                        pass.humMarker.Position = point;
                    }
                });
                // задержка 5 мс
                Thread.Sleep(5);
            }

            // отправка события о прибытии после достижения последней точки маршрута
            if (pass == null)
            {
                Arrived?.Invoke(this, null);
            }
            else
            {
                pass = null;
            }
        }

        public void passSeated(object sender, EventArgs e)
        {
            MessageBox.Show("Такси прибыло!");

            pass = (CHuman)sender;

            Application.Current.Dispatcher.Invoke(delegate {
                gMap.Markers.Add(moveTo(pass.getDestination()));
                //moveTo(pass.getDestination());
            }
            );
        }
    }
}
