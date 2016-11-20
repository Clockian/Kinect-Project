﻿using Microsoft.Kinect;
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

       // CameraMode _mode = CameraMode.Color; //An enum to hold color and depth

        KinectSensor sensor;
       // Skeleton[] _bodies = new Skeleton[6]; //6 is max # of skeletons allowed on camera
       // private byte[] colorPixels; //Stores color image frame
       // private DepthImagePixel[] depthPixels; //Stores depth image frame

        

        const int SKELETON_COUNT = 6;
        Skeleton[] allSkeletons = new Skeleton[SKELETON_COUNT];

        //Bitmaps
       // private WriteableBitmap colorBitmap;
      //  private WriteableBitmap depthBitmap;

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
            //If there are any kinect sensors found sensor will be set to the first one
            /*if (KinectSensor.KinectSensors.Count > 0)
            {
                sensor = KinectSensor.KinectSensors[0];
            }*/

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
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
                //sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);

                sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(Sensor_AllFramesReady);

                //For storing a frame of from color and depth stream
              //  this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                //this.depthPixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];


                //Bitmap to store pixel data, here so you only do it once at application start
               // colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
               // depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                sensor.Start();
            }
        }

        //Sets up color frame usage
        /*private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) //Holds a single color frame, using is used so that the object is disposed of when it goes out of scope
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(colorPixels);

                    //Storing data into writeablebitmap
                    colorBitmap.WritePixels(
                        new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
                        colorPixels,
                        colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }*/

        /*private void SensorDepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if(depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                }
            }
        }*/

        //Happens when all frames (color, depth, and skeleton) are ready for use
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

        private void getCameraPoint(Skeleton me, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || sensor == null)
                {
                    return;
                }
                /*
                DepthImagePoint headDepthPoint = depth.MapFromSkeletonPoint(me.Joints[JointType.Head].Position);
                DepthImagePoint rHandDepthPoint = depth.MapFromSkeletonPoint(me.Joints[JointType.HandRight].Position);
                DepthImagePoint lHandDepthPoint = depth.MapFromSkeletonPoint(me.Joints[JointType.HandLeft].Position);
                DepthImagePoint spineDepthPoint = depth.MapFromSkeletonPoint(me.Joints[JointType.Spine].Position);

                ColorImagePoint headColorPoint = depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint lHandColorPoint = depth.MapToColorImagePoint(lHandDepthPoint.X, lHandDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rHandColorPoint = depth.MapToColorImagePoint(rHandDepthPoint.X, rHandDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint spineColorPoint = depth.MapToColorImagePoint(spineDepthPoint.X, spineDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                */
                DepthImagePoint headDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint rHandDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint lHandDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint spineDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint ShoulderCenterDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                DepthImagePoint HipCenterDepthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(me.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);

                ColorImagePoint headColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, headDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint lHandColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, rHandDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rHandColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, lHandDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint spineColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, spineDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint ShoulderCenterColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, ShoulderCenterDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint HipCenterColorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, HipCenterDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);

                //  System.Console.WriteLine("Spine" + headColorPoint.X);
                //System.Console.WriteLine("RightHand" + rHandColorPoint.X);
                // System.Console.WriteLine("LeftHand" + lHandColorPoint.X);
                // System.Console.WriteLine("RH Difference" + (rHandColorPoint.X - headColorPoint.X));
                // System.Console.WriteLine("LH Difference" + (headColorPoint.X - lHandColorPoint.X));
                // System.Console.WriteLine("RH Difference" + (rHandColorPoint.Y - headColorPoint.Y));


                //make a region for follow
                int stopRegionXShort = spineColorPoint.X - 130;
                int stopRegionYShort = spineColorPoint.Y + 130;
                //make region for not follow
                int stopRegionXMax = spineColorPoint.X + 130;
                int stopRegionYMax = spineColorPoint.Y - 130;

                //if (!((rHandColorPoint.X < stopRegionXMax) && (rHandColorPoint.X > stopRegionXShort) && (rHandColorPoint.Y > stopRegionYMax)
                //   && (rHandColorPoint.Y < stopRegionYShort) && ((lHandColorPoint.X < stopRegionXMax) && (lHandColorPoint.X > stopRegionXShort)
                //  && (lHandColorPoint.Y > stopRegionYMax) && (lHandColorPoint.Y < stopRegionYShort))))
                //{
                //Move dots along with hands
                Canvas.SetLeft(BlackDot, ((rHandColorPoint.X - BlackDot.Width / 2)));
                Canvas.SetTop(BlackDot, (rHandColorPoint.Y) - BlackDot.Width / 2);

                Canvas.SetLeft(WhiteDot, ((lHandColorPoint.X - WhiteDot.Width / 2)));
                Canvas.SetTop(WhiteDot, (lHandColorPoint.Y) - WhiteDot.Width / 2);




                //both hands to right of the x of spine, in between the Y of shouldercenter and hip center
                if ((rHandColorPoint.X > spineColorPoint.X) && (lHandColorPoint.X > spineColorPoint.X) &&
                    (rHandColorPoint.Y < HipCenterColorPoint.Y) && (lHandColorPoint.Y < HipCenterColorPoint.Y) &&
                    (rHandColorPoint.Y > ShoulderCenterColorPoint.Y) && (lHandColorPoint.Y > ShoulderCenterColorPoint.Y))
                    {
                        //Console.WriteLine("\nSpeech Recognized: \t{0}\tConfidence:\t{1}");

                        Console.Write("\nMove Right");
                        SendKeys.SendWait("{F3}");

                       // Canvas.SetLeft(Fox, ((rHandColorPoint.X - Fox.Width / 2)));
                       // Canvas.SetTop(Fox, (rHandColorPoint.Y) - Fox.Width / 2);
                    }

                    //both hands to left
                    else if ((rHandColorPoint.X < headColorPoint.X) && (lHandColorPoint.X < headColorPoint.X) &&
                       (rHandColorPoint.Y > headColorPoint.Y) && (lHandColorPoint.Y > headColorPoint.Y))
                    {
                       // Canvas.SetLeft(Fox, ((lHandColorPoint.X - Fox.Width / 2)));
                        //Canvas.SetTop(Fox, (lHandColorPoint.Y) - Fox.Width / 2);
                        // Console.WriteLine("\nSpeech Recognized: \t{0}\tConfidence:\t{1}");

                        Console.Write("\nMove Left");
                        SendKeys.SendWait("{F1}");
                    }


                    else
                    {
                        //Canvas.SetLeft(Fox, (headColorPoint.X) - Fox.Width / 2);
                    }

                    if ((rHandColorPoint.Y < headColorPoint.Y) && (lHandColorPoint.Y < headColorPoint.Y))
                    {
                       // Canvas.SetLeft(Fox, (headColorPoint.X) - Fox.Width / 2);
                       // Canvas.SetTop(Fox, (headColorPoint.Y) - Fox.Width / 2);
                        //Console.WriteLine("\nSpeech Recognized: \t{0}\tConfidence:\t{1}");

                        Console.Write("\nSTOP");
                        SendKeys.SendWait("{F5}");
                    }

              //  }


                //Canvas.SetTop(Fox, (headColorPoint.Y - Fox.Height / 2) - (rHandColorPoint.X - Fox.Width / 2) / 2);
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

    //enum CameraMode
    //{
    //    Color,
     //   Depth
   // }
}