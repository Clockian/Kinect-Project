using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Kinect_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        KinectSensor sensor;

        /// <summary>
        /// Number of Skeletons that can be tracked with Kinect
        /// </summary>
        const int SKELETON_COUNT = 6;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the Kinect sensor.
        /// </summary>
        Skeleton[] allSkeletons = new Skeleton[SKELETON_COUNT];

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup, not robust against plug/unplug
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            //If the sensor is connected color, depth, and skeleton will be enabled
            if (sensor.Status == KinectStatus.Connected)
            {
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                sensor.SkeletonStream.Enable();

                sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(Sensor_AllFramesReady);

                sensor.Start();
            }
        }

        /// <summary>
        /// Happens when all frames (color, depth, and skeleton) are ready for use, main for Kinect
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) //Holds a single color frame, using is used so that the object is disposed of when it goes out of scope
            {
                //In the middle of shutting down, nothing to do
                if (colorFrame == null)
                {
                    return;
                }
                //Copies sensor frame to byte array for use
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);

                //Calculates sizing of image
                int stride = colorFrame.Width * 4;

                //Displays video in main window
                Vid.Source = BitmapSource.Create(
                    colorFrame.Width,
                    colorFrame.Height,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null,
                    pixels,
                    stride);
            }
            Skeleton me = null;
            getSkeleton(e, ref me);

            if (me == null)
            {
                return;
            }

            getCameraPoint(me, e);
        }

        /// <summary>
        /// Creates Skeleton of first person seen by Kinect
        /// </summary>
        /// <param name="e">event arguments</param>
        /// <param name="me">Skeleton of person referance</param>
        private void getSkeleton(AllFramesReadyEventArgs e, ref Skeleton me)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return;
                }

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //Store the first skeleton seen
                me = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
            }
        }

        /// <summary>
        /// Connects Skeleton position to Stepper simulator
        /// </summary>
        /// <param name="me">Skeleton of person</param>
        /// <param name="e">event arguments</param>
        private void getCameraPoint(Skeleton me, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || sensor == null)
                {
                    return;
                }

                //Setting up Skeleton Points to use
                DepthImagePoint headDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint rHandDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint lHandDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint spineDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint ShoulderCenterDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint ShoulderLeftDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.ShoulderLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint ShoulderRightDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint HipCenterDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint HipLeftDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint HipRightDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);


                //Lines up Skeleton points to color video
                ColorImagePoint headColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint lHandColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rHandDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rHandColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, lHandDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint spineColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, spineDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint ShoulderCenterColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, ShoulderCenterDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint ShoulderLeftColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, ShoulderLeftDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint ShoulderRightColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, ShoulderRightDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint HipCenterColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, HipCenterDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint HipLeftColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, HipLeftDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint HipRightColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, HipRightDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);

                //Move dots along with hands
                Canvas.SetLeft(BlackDot, ((rHandColorPoint.X - BlackDot.Width / 2)));
                Canvas.SetTop(BlackDot, (rHandColorPoint.Y) - BlackDot.Width / 2);

                Canvas.SetLeft(WhiteDot, ((lHandColorPoint.X - WhiteDot.Width / 2)));
                Canvas.SetTop(WhiteDot, (lHandColorPoint.Y) - WhiteDot.Width / 2);

                Canvas.SetLeft(RedDotRight, ((ShoulderRightColorPoint.X - WhiteDot.Width / 2)));
                Canvas.SetTop(RedDotRight, (ShoulderRightColorPoint.Y) - WhiteDot.Width / 2);

                Canvas.SetLeft(RedDotLeft, ((ShoulderLeftColorPoint.X - WhiteDot.Width / 2)));
                Canvas.SetTop(RedDotLeft, (ShoulderLeftColorPoint.Y) - WhiteDot.Width / 2);

                Canvas.SetLeft(RedDotHip, ((HipCenterColorPoint.X - WhiteDot.Width / 2)));
                Canvas.SetTop(RedDotHip, (HipCenterColorPoint.Y) - WhiteDot.Width / 2);

                //Both hands to right of the x of spine, in between the Y of shouldercenter and hip center
                if ((rHandColorPoint.X > ShoulderRightColorPoint.X) && (lHandColorPoint.X > ShoulderRightColorPoint.X) &&
                    (rHandColorPoint.Y < HipCenterColorPoint.Y) && (lHandColorPoint.Y < HipCenterColorPoint.Y) &&
                    (rHandColorPoint.Y > ShoulderCenterColorPoint.Y) && (lHandColorPoint.Y > ShoulderCenterColorPoint.Y))
                {
                        Console.Write("\nMove Positive Horizontal (Right)");
                        SendKeys.SendWait("{F3}");
                }

                //Both hands to left of the x of spine, in between the Y of shouldercenter and hip center
                else if ((rHandColorPoint.X < ShoulderLeftColorPoint.X) && (lHandColorPoint.X < ShoulderLeftColorPoint.X) &&
                       (rHandColorPoint.Y < HipCenterColorPoint.Y) && (lHandColorPoint.Y < HipCenterColorPoint.Y) &&
                       (rHandColorPoint.Y > ShoulderCenterColorPoint.Y) && (lHandColorPoint.Y > ShoulderCenterColorPoint.Y))
                {
                        Console.Write("\nMove Negative Horizonatal (Left)");
                        SendKeys.SendWait("{F1}");
                }

                //Both hands in between X of Shoulder left and right, Y above Shoulder center, the ints make zone bigger so it won't accidently trigger "Go-To 6"
                else if ((rHandColorPoint.Y < ShoulderCenterColorPoint.Y+20) && (lHandColorPoint.Y < ShoulderCenterColorPoint.Y+20) &&
                       (rHandColorPoint.X > ShoulderLeftColorPoint.X-30) && (lHandColorPoint.X > ShoulderLeftColorPoint.X-30) &&
                       (rHandColorPoint.X < ShoulderRightColorPoint.X+30) && (lHandColorPoint.X < ShoulderRightColorPoint.X+30))
                {
                        Console.Write("\nMove Positive Vertical (Up)");
                        SendKeys.SendWait("{F4}");
                }

                //Both hands in between X of Shoulder left and right, Y below Hip Center
                else if ((rHandColorPoint.Y > HipCenterColorPoint.Y) && (lHandColorPoint.Y > HipCenterColorPoint.Y) &&
                       (rHandColorPoint.X > ShoulderLeftColorPoint.X) && (lHandColorPoint.X > ShoulderLeftColorPoint.X) &&
                       (rHandColorPoint.X < ShoulderRightColorPoint.X) && (lHandColorPoint.X < ShoulderRightColorPoint.X))
                {
                    Console.Write("\nMove Negative Vertical (Down)");
                    SendKeys.SendWait("{F2}");
                }

                //Both hands inbetween X of Shoulder left and right, Y below shoulder center and above Hip center
                else if ((rHandColorPoint.Y < HipCenterColorPoint.Y) && (lHandColorPoint.Y < HipCenterColorPoint.Y) &&
                       (rHandColorPoint.Y > ShoulderCenterColorPoint.Y) && (lHandColorPoint.Y > ShoulderCenterColorPoint.Y) &&
                       (rHandColorPoint.X > ShoulderLeftColorPoint.X) && (lHandColorPoint.X > ShoulderLeftColorPoint.X) &&
                       (rHandColorPoint.X < ShoulderRightColorPoint.X) && (lHandColorPoint.X < ShoulderRightColorPoint.X))
                {
                    Console.Write("\nSTOP");
                    SendKeys.SendWait("{F5}");
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensor.Stop();
        }
    }
}
